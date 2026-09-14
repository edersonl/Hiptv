using IptvStarterApp.Domain.Interfaces;
using IptvStarterApp.Domain.Playback;

namespace IptvStarterApp.Infrastructure.Playback;

public sealed class HardenedPlaybackEngine : IPlaybackEngine, IDisposable
{
    private readonly IPlaybackEngine _inner;
    private readonly PlaybackDiagnostics _diagnostics;
    private readonly SemaphoreSlim _operationLock = new(1, 1);
    private readonly int _maxRetries;
    private readonly TimeSpan _timeout;
    private readonly TimeSpan _slowBufferingThreshold;
    private readonly TimeSpan _retryBaseDelay;
    private readonly int _circuitBreakerFailureThreshold;
    private readonly TimeSpan _circuitBreakerDuration;
    private readonly CancellationTokenSource _lifetimeCancellation = new();
    private readonly object _circuitSync = new();
    private PlaybackRequest? _lastRequest;
    private string? _lastError;
    private int _eventRetryCount;
    private readonly int _maxEventRetries;
    private int _consecutiveFailures;
    private int _recoveryInProgress;
    private DateTimeOffset? _circuitOpenedAt;
    private PlaybackState _currentState = PlaybackState.Idle;
    private CancellationTokenSource? _bufferingMonitor;
    private bool _disposed;

    public HardenedPlaybackEngine(
        IPlaybackEngine inner,
        PlaybackDiagnostics? diagnostics = null,
        int maxRetries = 2,
        TimeSpan? timeout = null,
        TimeSpan? slowBufferingThreshold = null,
        TimeSpan? retryBaseDelay = null,
        int circuitBreakerFailureThreshold = 3,
        TimeSpan? circuitBreakerDuration = null)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _diagnostics = diagnostics ?? new PlaybackDiagnostics();
        _maxRetries = Math.Max(0, maxRetries);
        _timeout = timeout ?? TimeSpan.FromSeconds(15);
        _slowBufferingThreshold = slowBufferingThreshold ?? TimeSpan.FromSeconds(10);
        _retryBaseDelay = retryBaseDelay ?? TimeSpan.FromMilliseconds(250);
        _circuitBreakerFailureThreshold = Math.Max(1, circuitBreakerFailureThreshold);
        _circuitBreakerDuration = circuitBreakerDuration ?? TimeSpan.FromSeconds(30);
        Metrics = new PlaybackMetrics();
        _inner.StateChanged += OnInnerStateChanged;
        _maxEventRetries = _maxRetries;
    }

    public event EventHandler<PlaybackStateChangedEventArgs>? StateChanged;
    public PlaybackMetrics Metrics { get; }
    public TimeSpan Position => _inner.Position;
    public TimeSpan? Duration => _inner.Duration;
    public async Task PlayAsync(PlaybackRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ThrowIfDisposed();
        EnsureCircuitClosed();
        _lastRequest = request;
        _lastError = null;
        _eventRetryCount = 0;

        await _operationLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            for (var attempt = 0; ; attempt++)
            {
                Metrics.RecordAttempt();
                try
                {
                    await _inner.PlayAsync(request, cancellationToken).WaitAsync(_timeout, cancellationToken).ConfigureAwait(false);
                    return;
                }
                catch (TimeoutException)
                {
                    _lastError = "Tempo limite ao iniciar a reprodução.";
                    RegisterFailure();
                    _diagnostics.LogTimeout(_timeout);
                    if (attempt >= _maxRetries) throw;
                    await RetryAsync(attempt, _lastError, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (IsRecoverable(ex))
                {
                    _lastError = ex.Message;
                    RegisterFailure();
                    if (attempt >= _maxRetries) throw;
                    await RetryAsync(attempt, _lastError, cancellationToken).ConfigureAwait(false);
                }
            }
        }
        finally
        {
            _operationLock.Release();
        }
    }

    public Task PauseAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        return _inner.PauseAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        _lastRequest = null;
        CancelBufferingMonitor();
        return _inner.StopAsync(cancellationToken);
    }

    public PlaybackHealthReport GetHealthReport() =>
        PlaybackHealthReport.From(Metrics.Snapshot(), _lastError, _slowBufferingThreshold);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _lifetimeCancellation.Cancel();
        _inner.StateChanged -= OnInnerStateChanged;
        CancelBufferingMonitor();
        (_inner as IDisposable)?.Dispose();
        _lifetimeCancellation.Dispose();
    }

    private async Task RetryAsync(int attempt, string reason, CancellationToken cancellationToken)
    {
        Metrics.RecordRetry();
        Metrics.RecordReconnection();
        _diagnostics.LogRetry(attempt + 1, reason);
        await _inner.StopAsync(cancellationToken).ConfigureAwait(false);
        await Task.Delay(GetRetryDelay(attempt + 1), cancellationToken).ConfigureAwait(false);
    }

    private void OnInnerStateChanged(object? sender, PlaybackStateChangedEventArgs args)
    {
        _currentState = args.State;
        Metrics.RecordState(args.State);
        _diagnostics.LogState(args.State, args.Message);
        if (args.State == PlaybackState.Buffering)
        {
            StartBufferingMonitor();
        }
        else
        {
            CancelBufferingMonitor();
        }

        if (args.State == PlaybackState.Error)
        {
            _lastError = args.Message ?? "Erro de reprodução.";
            RegisterFailure();
            if (_lastRequest is not null && _eventRetryCount < _maxEventRetries && !_disposed)
            {
                _eventRetryCount++;
                QueueRecovery(_lastError, _eventRetryCount);
                return;
            }
        }
        else if (args.State == PlaybackState.Playing)
        {
            _lastError = null;
            _eventRetryCount = 0;
            ResetCircuitBreaker();
        }

        StateChanged?.Invoke(this, args);
    }

    private void StartBufferingMonitor()
    {
        CancelBufferingMonitor();
        _bufferingMonitor = CancellationTokenSource.CreateLinkedTokenSource(_lifetimeCancellation.Token);
        var token = _bufferingMonitor.Token;
        _ = MonitorBufferingAsync(token);
    }

    private async Task MonitorBufferingAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(_slowBufferingThreshold, cancellationToken).ConfigureAwait(false);
            if (!_disposed && _currentState == PlaybackState.Buffering && _lastRequest is not null)
            {
                _lastError = "O stream permaneceu em buffering além do limite.";
                if (_eventRetryCount < _maxEventRetries)
                {
                    QueueRecovery(_lastError, ++_eventRetryCount);
                }
                else
                {
                    StateChanged?.Invoke(this, new PlaybackStateChangedEventArgs(PlaybackState.Error, _lastError));
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task RecoverFromPlaybackErrorAsync(string reason, int attempt)
    {
        try
        {
            EnsureCircuitClosed();
            Metrics.RecordRetry();
            Metrics.RecordReconnection();
            _diagnostics.LogRetry(attempt, reason);
            await _inner.StopAsync(_lifetimeCancellation.Token).ConfigureAwait(false);
            await Task.Delay(GetRetryDelay(attempt), _lifetimeCancellation.Token).ConfigureAwait(false);
            if (!_disposed && _lastRequest is not null)
            {
                await _inner.PlayAsync(_lastRequest, _lifetimeCancellation.Token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (_disposed || _lifetimeCancellation.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            _lastError = exception.Message;
            RegisterFailure();
            if (!_disposed)
            {
                StateChanged?.Invoke(this, new PlaybackStateChangedEventArgs(PlaybackState.Error, _lastError));
            }
        }
        finally
        {
            Interlocked.Exchange(ref _recoveryInProgress, 0);
        }
    }

    private void QueueRecovery(string reason, int attempt)
    {
        if (Interlocked.CompareExchange(ref _recoveryInProgress, 1, 0) == 0)
        {
            _ = RecoverFromPlaybackErrorAsync(reason, attempt);
        }
    }

    private TimeSpan GetRetryDelay(int attempt)
    {
        var multiplier = Math.Pow(2, Math.Max(0, attempt - 1));
        return TimeSpan.FromMilliseconds(Math.Min(_retryBaseDelay.TotalMilliseconds * multiplier, 4000));
    }

    private void RegisterFailure()
    {
        lock (_circuitSync)
        {
            _consecutiveFailures++;
            if (_consecutiveFailures >= _circuitBreakerFailureThreshold)
            {
                _circuitOpenedAt = DateTimeOffset.UtcNow;
            }
        }
    }

    private void EnsureCircuitClosed()
    {
        lock (_circuitSync)
        {
            if (_circuitOpenedAt is not { } openedAt)
            {
                return;
            }

            if (DateTimeOffset.UtcNow - openedAt >= _circuitBreakerDuration)
            {
                _circuitOpenedAt = null;
                _consecutiveFailures = 0;
                return;
            }

            throw new InvalidOperationException("Circuit breaker aberto para o stream após falhas consecutivas.");
        }
    }

    private void ResetCircuitBreaker()
    {
        lock (_circuitSync)
        {
            _consecutiveFailures = 0;
            _circuitOpenedAt = null;
        }
    }

    private void CancelBufferingMonitor()
    {
        var monitor = Interlocked.Exchange(ref _bufferingMonitor, null);
        if (monitor is null)
        {
            return;
        }

        monitor.Cancel();
        monitor.Dispose();
    }

    private static bool IsRecoverable(Exception exception) =>
        exception is IOException or InvalidOperationException or TimeoutException;

    private void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(HardenedPlaybackEngine));
    }
}

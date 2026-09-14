# Sprint 2 QA

## Arquivos criados

- `Domain/Playback/PlaybackRequest.cs`
- `Domain/Playback/PlaybackState.cs`
- `Infrastructure/Playback/Media3PlaybackEngine.cs`
- `Infrastructure/Playback/LegacyMediaPlayerEngine.cs`
- `Infrastructure/Playback/PlaybackMetrics.cs`
- `Infrastructure/Playback/PlaybackDiagnostics.cs`
- `Infrastructure/Playback/PlaybackHealthReport.cs`
- `Infrastructure/Playback/HardenedPlaybackEngine.cs`
- `Tests/PlaybackContractTests.cs`
- `Tests/PlaybackHardeningTests.cs`
- `docs/Sprint2.md`
- `docs/QA_PLAYER_HARDENING.md`

## Arquivos modificados

- `IptvStarterApp/IptvStarterApp.csproj`
- `IptvStarterApp/UI/PlayerActivity.cs`
- `IptvStarterApp/Resources/layout/activity_player.xml`
- `Tests/IptvStarterApp.Tests.csproj`

## Riscos restantes

- O ambiente atual não possui .NET 8 SDK, workload Android ou Android SDK; não foi possível executar build/restore/teste real.
- Os bindings C# de Media3 precisam ser confirmados por compilação em ambiente Android.
- Reprodução HLS, reconexão e lifecycle ainda precisam de teste em dispositivo físico/Android TV.
- Cache persistente de segmentos não foi habilitado automaticamente; exige política de armazenamento, expiração, privacidade e dependências Media3 de cache.
- A seleção de qualidade ainda depende da lógica padrão do ExoPlayer; não há seletor de bitrate exposto na UI.

## Limitações conhecidas

- O timeout cobre a operação de início e o watchdog cobre buffering prolongado; não substitui monitoramento de rede de baixo nível.
- Métricas atuais não coletam bytes transferidos, bitrate efetivo ou dropped frames.
- O fallback legado é preservado, mas suporta menos codecs e recursos que Media3.
- Erros técnicos são registrados com `Android.Util.Log`; a UI recebe mensagem genérica.
- A Activity libera o engine em `OnPause`, `OnStop` e `OnDestroy`, e recria a infraestrutura em `OnStart`.

## Cenários verificados estaticamente

- `PlayerActivity` não referencia `Android.Media.MediaPlayer` diretamente.
- Media3 é tentado antes do engine legado.
- Falha de criação do Media3 seleciona `LegacyMediaPlayerEngine`.
- Falha síncrona de reprodução tenta fallback quando o Media3 está ativo.
- Erros assíncronos Media3 passam pelo retry do `HardenedPlaybackEngine`.
- Buffering acima do limite dispara reconexão limitada.
- Estados de reprodução são encaminhados pelo evento `StateChanged`.
- Listeners são removidos antes de descarte.
- `PlayerView` foi adicionado ao layout sem remover os controles existentes.
- `git diff --check` passou.
- Diagnósticos do editor não encontraram erros nos arquivos alterados.

## Cenários pendentes

- Build Android com .NET 8 e pacotes Media3 restaurados.
- Testes unitários executáveis.
- Stream HLS autorizado válido.
- URL inválida e manifesto HLS corrompido.
- Queda e retorno de conectividade durante reprodução.
- Buffering prolongado em rede lenta.
- Pausar, parar, concluir e retomar após background/foreground.
- Rotação, encerramento e recriação em Android TV.
- Verificação de vazamento de `Player`, listeners e `PlayerView`.
- Validação do fallback em dispositivos sem suporte ao Media3.

## Checklist de aceite

- [x] PlayerActivity sem dependência direta de `Android.Media.MediaPlayer`.
- [x] `IPlaybackEngine` usado pelos botões e comandos.
- [x] Media3 inicializado antes do legado.
- [x] Fallback legado preservado.
- [x] Eventos buffering, playing, paused, stopped, completed e error encaminhados.
- [x] Unsubscribe e descarte em `OnPause`, `OnStop` e `OnDestroy`.
- [x] Favoritos mantidos com `FavoritesService`.
- [x] `PlayerView` adicionado preservando controles atuais.
- [x] Logs técnicos sem stacktrace exibido ao usuário.
- [x] Retry e reconexão limitados.
- [x] Timeout configurável.
- [x] Watchdog de buffering lento.
- [x] Métricas e relatório de saúde.
- [ ] Compilação confirmada em ambiente Android.
- [ ] Testes unitários executados.
- [ ] Reprodução real validada.
- [ ] Cache local persistente validado e aprovado.

## Pendências para Sprint 3

A Sprint 3 não deve iniciar recursos de EPG, VOD ou séries até a compilação Android e os cenários de reprodução pendentes serem validados. Depois disso, o próximo passo é integrar o player hardening aos casos de uso de catálogo sem acoplar entidades de domínio à UI.

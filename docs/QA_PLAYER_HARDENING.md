# QA Player Hardening

## Escopo

Sprint 2.5 valida a camada de reprodução antes de catálogo, EPG, VOD e séries. A implementação adiciona retry, timeout configurável, reconexão, métricas, diagnóstico técnico e relatório de saúde sobre o engine atual.

## Componentes

- `PlaybackMetrics`: tentativas, retries, reconexões, buffering e duração.
- `PlaybackDiagnostics`: logs técnicos sem stacktrace para o usuário.
- `PlaybackHealthReport`: classifica saúde, lentidão e stream inválido.
- `HardenedPlaybackEngine`: decorator com timeout, retry e reconexão.

## Arquivos alterados/criados

- Criados `Infrastructure/Playback/PlaybackMetrics.cs`, `PlaybackDiagnostics.cs`, `PlaybackHealthReport.cs` e `HardenedPlaybackEngine.cs`.
- `PlayerActivity.cs`: usa o decorator, lifecycle e fallback já existentes.
- `Media3PlaybackEngine.cs`: eventos de estado e erro Media3.
- `LegacyMediaPlayerEngine.cs`: fallback compatível.
- `activity_player.xml`: `PlayerView` Media3.
- `docs/Sprint2.md`: critérios e limitações.

## Cenários analisados

- URL HLS `.m3u8` detectada.
- Falha síncrona de inicialização do Media3.
- Falha de rede recuperável com retries limitados.
- Timeout configurável de início.
- Stream inválido após erro Media3.
- Buffering prolongado classificado como lento.
- Parada, conclusão e descarte sem manter listeners ativos.
- Fallback para `LegacyMediaPlayerEngine` sem stacktrace na UI.

## Cenários pendentes

- Teste real em dispositivo Android 21+ com HLS autorizado.
- Reconexão real durante interrupção de rede.
- Métricas de bytes/banda por implementação Media3.
- Cache persistente de segmentos; não habilitado automaticamente para não consumir armazenamento nem persistir conteúdo sem política definida.
- Teste de rotação e background/foreground em Android TV.
- Build com .NET 8 SDK, workload Android e bindings Media3 restaurados.

## Checklist de aceite

- [x] Media3 é tentado antes do legado.
- [x] Fallback legado existe.
- [x] Retry limitado e timeout configurável.
- [x] Reconexão registra métrica técnica.
- [x] Estados buffering/playing/paused/stopped/completed/error são propagados.
- [x] Logs técnicos não exibem stacktrace ao usuário.
- [x] Relatório de saúde identifica stream lento ou inválido.
- [ ] Compilação Android executada em ambiente com SDK.
- [ ] Reprodução validada em dispositivo real.
- [ ] Cache persistente aprovado por requisito e política de conteúdo.

## Resultado

A validação funcional completa permanece pendente da compilação e dos testes em dispositivo. Nenhum recurso de catálogo deve ser adicionado antes dessas pendências serem resolvidas.

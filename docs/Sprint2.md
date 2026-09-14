# Sprint 2: Migração incremental do player

## Objetivo

Substituir gradualmente `Android.Media.MediaPlayer` por Media3/ExoPlayer, mantendo o player legado como fallback até a validação em dispositivos reais.

## Componentes

- `Domain/Playback/PlaybackRequest`: URL, título, retomada e detecção de HLS.
- `Domain/Playback/PlaybackState`: idle, buffering, playing, paused, stopped, completed e error.
- `IPlaybackEngine`: contrato único para iniciar, pausar e parar reprodução.
- `Infrastructure/Playback/Media3PlaybackEngine`: ExoPlayer com MIME HLS para `.m3u8`.
- `Resources/layout/activity_player.xml`: superfície `PlayerView` para vídeo e controles Media3.
- `Infrastructure/Playback/LegacyMediaPlayerEngine`: implementação de compatibilidade usando `Android.Media.MediaPlayer`.
- `PlayerActivity`: deve selecionar Media3 e usar o legado quando a inicialização falhar.

## Segurança

Nenhuma credencial de configuração foi copiada para os novos componentes. A sessão e qualquer autenticação futura devem permanecer fora de URLs, logs e mensagens de estado.

## QA executado

A análise estática do editor deve ser executada após a edição. A compilação e os testes dependem do .NET 8 SDK, workload Android e restauração dos pacotes Media3. O ambiente atual não possui .NET SDK instalado.

## Critérios de aceite

- HLS `.m3u8` é detectado e enviado ao ExoPlayer com MIME apropriado.
- Falha na inicialização do Media3 permite fallback para o player legado.
- Eventos de buffering, playing, paused, stopped, completed e error chegam à Activity, incluindo `PlaybackException` do Media3.
- O botão de parar libera o player e listeners.
- Rotação, background/foreground e encerramento não deixam recursos ativos.
- O fluxo atual de favoritos e abertura da Activity continua funcionando.

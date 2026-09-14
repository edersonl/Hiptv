# Sprint 1: Fundamentos e segurança

## Objetivo

Introduzir a primeira fronteira de Clean Architecture no Android/.NET sem alterar o fluxo visual existente: domínio independente de Android, contratos de aplicação, parser M3U desacoplado e testes unitários isolados.

## Entregas

- `Domain/Entities`: `MediaItem`, `Channel`, `Series` e `Episode`.
- `Domain/ValueObjects`: `MediaId`.
- `Domain/Interfaces`: `IPlaybackEngine`, `IPlaylistRepository` e `IEpgProvider`.
- `Application/UseCases/LoadPlaylistUseCase` para orquestrar carregamento.
- `Infrastructure/Playlist/M3uParser` para parsing tolerante e determinístico, retornando entidades do domínio.
- `Infrastructure/Playlist/M3uPlaylistRepository` para HTTP + parser.
- `PlaylistService` permanece como facade de compatibilidade.
- Metadados `tvg-id`, `tvg-name`, `tvg-logo`, `group-title` e atributos desconhecidos preservados.
- Linhas inválidas ignoradas, URLs limitadas a HTTP/HTTPS e URLs duplicadas deduplicadas.
- Testes unitários para playlists válidas, inválidas, URLs inválidas, duplicatas e atributos ausentes.

## Compatibilidade

As propriedades `Name`, `Url` e `Group` de `ChannelItem` permanecem disponíveis. A UI existente continua consumindo `PlaylistService.LoadAsync` e `PlaylistService.Parse`; o parser novo retorna `Domain.Channel` e fica atrás dessa facade até a migração das Activities para injeção explícita.

## Dependências

A produção não recebeu dependências novas. O projeto de testes usa xUnit e Microsoft.NET.Test.Sdk, isolados em `Tests/IptvStarterApp.Tests.csproj`.

## Segurança

Credenciais e URLs legadas em `Config/AppConfig.cs` não foram propagadas para o novo domínio. A remoção/rotação desses valores é uma ação separada, pois pode alterar comportamento público e precisa de uma fonte autorizada de configuração.

## Verificação

A máquina de desenvolvimento não possui .NET SDK instalado, apenas o runtime 6.0.36. Portanto, `dotnet build` e `dotnet test` não puderam ser executados localmente. A validação executável deve ser feita em ambiente com .NET 8 SDK e workload Android instalado.

## Próxima etapa

A Sprint 2 deve introduzir `PlaybackRequest`, `PlaybackState` e um adapter Media3/ExoPlayer com fallback do `MediaPlayer`, após instalar o SDK/workload e validar esta sprint.

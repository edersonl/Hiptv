# Sprint 3 - Fase 1: Domain Baseline

## Status

**Implementada em 8 de setembro de 2026.**

A entrega introduz o domínio unificado, contratos de repositório e testes de regras de negócio. Nenhuma UI, tela, integração EPG ou API externa foi criada. Os componentes de playback homologados na Sprint 2 permaneceram inalterados.

## Escopo entregue

### Entidades

| Entidade | Papel | Principais invariantes |
|---|---|---|
| `MediaItem` | Raiz abstrata do catálogo | ID e nome obrigatórios; tipo de mídia imutável |
| `Channel` | Conteúdo linear reproduzível | URL HTTP/HTTPS absoluta obrigatória |
| `Movie` | Conteúdo VOD reproduzível | URL HTTP/HTTPS absoluta obrigatória |
| `Series` | Aggregate root de conteúdo seriado | Temporadas pertencem à série; números não se repetem |
| `Season` | Entidade interna de `Series` | Série e ID obrigatórios; número positivo; episódios pertencem à temporada e série |
| `Episode` | Conteúdo seriado reproduzível | Série, temporada, URL e números positivos obrigatórios |
| `Playlist` | Aggregate root de fonte de catálogo | ID/nome obrigatórios; M3U remota exige HTTP/HTTPS; itens sem duplicidade |
| `Category` | Taxonomia de conteúdo | ID/nome obrigatórios; não pode ser pai de si mesma |
| `Favorite` | Referência de biblioteca do usuário | Exige instância válida de `MediaItem`, perfil e data |
| `PlaybackProgress` | Estado persistível de reprodução | Posição não negativa, não excede duração e aplica regra de conclusão |
| `ContinueWatching` | Projeção de progresso ativo | Item e progresso devem coincidir; exclui canal e conteúdo não elegível |

### Value Objects

- `MediaId`
- `PlaylistId`
- `SeasonId`
- `CategoryId`

Todos são value objects imutáveis, removem espaços nas extremidades, rejeitam valores vazios e usam igualdade estrutural por valor.

### Enums

- `MediaType`: `Channel`, `Movie`, `Series`, `Episode`.
- `ContentType`: `Live`, `VideoOnDemand`, `Serialized`.
- `PlaybackStatus`: `NotStarted`, `InProgress`, `Completed`.
- `PlaylistType`: `M3U`, `Local`.

## Agregados

### Series

`Series` é a raiz do agregado de conteúdo seriado:

```text
Series
  -> Season (uma ou mais conforme o catálogo)
       -> Episode
```

A raiz valida que cada `Season.SeriesId` corresponde ao seu `MediaId`. A temporada valida simultaneamente `Episode.SeriesId` e `Episode.SeasonId`, ordena episódios e rejeita números duplicados.

### Playlist

`Playlist` controla identidade, origem, tipo, estado de ativação e referências de mídia importadas. As referências são deduplicadas. A entidade não executa HTTP nem parsing.

### User Library

`Favorite` e `PlaybackProgress` são estados por perfil. `ContinueWatching` é uma projeção derivada e não uma segunda fonte de escrita.

## Compatibilidade

O contrato atual de `Channel` foi preservado para evitar regressão no parser e nos serviços existentes:

- `Name`
- `Source`
- `Group`
- `LogoUrl`
- `Attributes`

`MediaItem` passou a permitir itens não reproduzíveis, como `Series`, enquanto `Channel`, `Movie` e `Episode` exigem uma fonte válida.

O `IPlaylistRepository.LoadAsync(string, CancellationToken)` foi preservado. As operações futuras de catálogo de playlists foram adicionadas como capacidades padrão não suportadas, permitindo que o adaptador M3U atual continue compilando sem alteração de infraestrutura.

## Contratos criados

| Interface | Responsabilidade |
|---|---|
| `IContentRepository` | Consultar, listar e persistir itens do catálogo unificado |
| `IPlaybackProgressRepository` | Consultar/salvar progresso, histórico e remoção por perfil |
| `IContinueWatchingRepository` | Consultar a projeção de itens em andamento |
| `IFavoritesRepository` | Consultar, adicionar e remover favoritos por perfil |
| `IPlaylistRepository` | Carregar M3U atual e representar o catálogo persistido de playlists |

As interfaces são assíncronas e aceitam `CancellationToken`. Nenhuma implementação de persistência ou API externa faz parte desta fase.

## Regras implementadas

### Identidade e nomes

- IDs default, vazios ou compostos apenas por espaços são rejeitados.
- Nomes obrigatórios são validados e normalizados com `Trim`.
- Value objects com o mesmo valor normalizado são iguais.

### URLs

- `Channel`, `Movie` e `Episode` aceitam somente URLs HTTP/HTTPS absolutas.
- Playlist M3U remota aceita somente URL HTTP/HTTPS absoluta.
- Série, temporada, categoria, favorito e progresso não carregam URL de reprodução.

### Relacionamentos

- Episódio exige `SeriesId` e `SeasonId`.
- Temporada rejeita episódio de outra temporada ou série.
- Série rejeita temporada de outra série.
- Números de temporada são únicos dentro da série.
- Números de episódio são únicos dentro da temporada.
- Favorito recebe e mantém uma referência válida a `MediaItem`.

### Progresso e Continue Watching

- Progresso menor que zero é rejeitado.
- Posição maior que a duração é rejeitada.
- Conteúdo é concluído em 90% ou quando faltam no máximo 2 minutos.
- Progresso inferior a 30 segundos não entra em Continue Watching.
- Conteúdo concluído não entra em Continue Watching.
- Canal ao vivo não entra em Continue Watching.
- Item e progresso devem possuir o mesmo `MediaId`.

## Testes criados

Arquivo: `Tests/DomainBaselineTests.cs`.

| Área | Cenários |
|---|---|
| Value Objects | Igualdade normalizada e rejeição de IDs vazios |
| Entidades reproduzíveis | Nome e URL válidos; classificação de conteúdo |
| Season | Rejeição de episódio pertencente a outra temporada |
| Series | Rejeição de temporada externa; ordenação e integridade do agregado |
| Favorite | Referência obrigatória e identidade coerente |
| PlaybackProgress | Limite de 30 segundos, 90%, 2 minutos restantes e status |
| ContinueWatching | Rejeição de canal e de progresso pertencente a outro item |
| Playlist/Category | URL M3U, deduplicação e autorreferência inválida |

Resultados:

- Testes novos de domínio: **13/13 aprovados** em 106 ms.
- Suíte completa: **27/27 aprovados** em 184 ms.
- Diagnósticos de domínio e testes: **0 erros**.

## Cobertura estimada

A cobertura do domínio novo é estimada em **85% das regras e ramificações críticas**.

A estimativa considera:

- 11 entidades exercitadas direta ou indiretamente.
- 4 value objects com igualdade e validação exercitadas.
- 4 enums usados nos testes de comportamento.
- Invariantes críticas de identidade, URL, relacionamentos, favorito, progresso e Continue Watching cobertas.

Não foi gerado percentual instrumental de linhas porque o projeto não possui coletor de cobertura configurado. A próxima melhoria de QA é adicionar coleta determinística no pipeline sem contaminar o domínio com dependências de teste.

## Validação e riscos

### Aprovado

- Compilação do projeto de testes.
- 27 testes sem falha.
- Compatibilidade do parser M3U demonstrada pelos testes existentes.
- Nenhum diagnóstico estático no domínio ou nos testes.
- Nenhum arquivo de UI ou playback foi modificado nesta entrega.

### Bloqueio externo ao domínio

O build Android foi interrompido no restore por conflito de dependências já existente no projeto congelado:

```text
Xamarin.AndroidX.Lifecycle.Process 2.8.4.1
  exige Lifecycle.Runtime < 2.8.5

Xamarin.AndroidX.Media3.ExoPlayer 1.8.0
  exige Lifecycle.Runtime >= 2.9.2.1
```

A Fase 1 não altera essa configuração porque a infraestrutura homologada está protegida. O conflito deve ser tratado por change request separado, com novo build e regressão P01-P12, antes do próximo release candidate Android.

## Arquivos protegidos

Não alterados nesta fase:

- `UI/PlayerActivity.cs`
- `Infrastructure/Playback/Media3PlaybackEngine.cs`
- `Infrastructure/Playback/HardenedPlaybackEngine.cs`
- Demais implementações de infraestrutura da Sprint 2

## Critério de saída da Fase 1

| Critério | Status |
|---|---|
| Entidades solicitadas criadas | Aprovado |
| Value objects solicitados criados | Aprovado |
| Enums solicitados criados | Aprovado |
| Invariantes críticas implementadas | Aprovado |
| Cinco contratos definidos | Aprovado |
| Testes de criação, igualdade, relacionamentos, agregados e regras | Aprovado |
| Suíte completa sem regressão | Aprovado |
| Build Android | Bloqueado por conflito NuGet preexistente e fora do escopo protegido |

## Decisão

**GO para evolução interna da Sprint 3**, limitada às próximas etapas de aplicação/persistência previstas no design review.

**NO GO para release candidate Android** enquanto o conflito NuGet da infraestrutura congelada não for resolvido por change request e submetido novamente à regressão P01-P12.

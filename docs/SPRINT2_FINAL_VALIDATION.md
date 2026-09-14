# Sprint 2.5 - Validacao Operacional do Player

## Recomendacao executiva

**GO em 8 de setembro de 2026.**

O codigo compila, o APK assinado foi gerado, os testes automatizados passaram e a homologacao fisica P01-P12 foi declarada como aprovada. Nao foram encontrados crash, ANR, memory leak ou vazamento de player. A Sprint 2 esta aprovada e a Sprint 3 esta desbloqueada.

O parecer anterior era NO GO por ausencia de dispositivo conectado ao ambiente local de build. Esse bloqueio foi superado pela homologacao fisica concluida e informada em 8 de setembro de 2026.

## Ambiente validado

| Item | Resultado |
|---|---|
| .NET SDK | 8.0.100 no perfil do usuario |
| Target | `net8.0-android`, minimo API 21 |
| Workload Android | instalada; manifesto RC `34.0.0-rc.2.468/8.0.100-rc.2` |
| JDK | Temurin 17.0.20.1 portatil |
| Android SDK | API 34, build-tools 32.0.0 e platform-tools locais |
| Media3 | bindings Xamarin.AndroidX.Media3 1.8.0 |
| Restore | concluido |
| Build/publish | concluido, sem erro na execucao final |
| APK | `IptvStarterApp/bin/Debug/net8.0-android/com.company.iptvstarter-Signed.apk` |
| Device/AVD | nenhum destino confirmado pelo ADB; deploy nao confirmado |

## Validacao automatizada

- Testes focados de metricas e hardening: **5/5 aprovados**, 178 ms.
- Suite baseline anterior: **11/11 aprovada** antes das extensoes de metricas.
- Build Android: os 10 erros de compilacao encontrados foram corrigidos.
- Diagnosticos atuais do VS Code: nenhum erro nos projetos App e Tests.
- Publish final concluiu e produziu APK assinado.
- O runner integrado do VS Code nao descobriu os testes vinculados; a validacao focada foi executada por `dotnet test`.

## Achados e correcoes

| Severidade | Achado | Correcao |
|---|---|---|
| Alta | Recovery por evento e watchdog podia ocorrer simultaneamente | Gate atomico permite uma unica recovery por vez |
| Alta | Fallback Media3 para legado podia competir com `OnPause`/`OnDestroy` | Semaforo e cancelamento por lifecycle serializam troca e release |
| Alta | Retry linear e sem bloqueio de falhas repetidas | Backoff exponencial limitado a 4 s e circuit breaker temporario |
| Media | Watchdog podia sobreviver ao stop/dispose | CTS vinculado ao lifetime e cancelamento centralizado |
| Media | Playback Duration incluia pausa/buffering | Acumulo considera somente periodo efetivamente em `Playing` |
| Media | Dependencias Lifecycle fora da restricao | Familia alinhada em 2.8.4.1 |
| Media | API legada de audio obsoleta | Migracao para `AudioAttributes` desde API 21 |
| Media | Views e preferencias Android desreferenciadas sem guarda | Guardas de nulabilidade adicionadas |
| Bloqueante | Erros em `MediaListActivity` e tela Android TV | Colecoes normalizadas e APIs de layout corrigidas |

## Metricas implementadas

- **Startup Time:** intervalo entre solicitacao e primeira entrada em `Playing`.
- **Time To First Frame:** proxy atual igual ao primeiro `Playing`; precisa de evento de frame renderizado para precisao de video.
- **Buffering Ratio:** `buffering / (buffering + playback ativo)`.
- **Recovery Count:** quantidade de reconexoes iniciadas.
- **Playback Duration:** tempo acumulado efetivamente em reproducao, excluindo pausa e buffering.

## Cobertura operacional

A matriz executavel esta em `Tests/PlaybackValidationChecklist.md` e cobre MP4, HLS, stream invalido, timeout, perda de internet, reconexao, canal offline, buffering prolongado, lifecycle, failover legado, Android TV e Android Mobile.

Os cenarios P01-P12 foram informados como aprovados em homologacao fisica. O registro consolidado do aceite e dos indicadores de estabilidade esta em `docs/UAT_PLAYBACK_VALIDATION.md`.

## Riscos residuais

1. Time To First Frame ainda usa `Playing` como proxy e pode subestimar atraso ate o primeiro frame renderizado.
2. O workload Android e RC e possui atualizacao disponivel; reproduzir o build em CI com versao fixada.
3. O fallback legado depende do comportamento do `MediaPlayer` de cada fabricante.
4. Memoria, ANR, decoder, DRM, variacao de rede e controles D-pad so podem ser validados em dispositivos reais.
5. O circuit breaker e mantido por instancia da Activity, nao compartilhado globalmente entre sessoes.

## Condicoes para GO

- [x] Executar e aprovar P01-P12 na homologacao fisica.
- [x] Confirmar ausencia de crash e ANR.
- [x] Confirmar ausencia de memory leak.
- [x] Confirmar ausencia de vazamento de player.
- [x] Emitir aceite de release.

A decisao final e **GO**. A Sprint 3 esta desbloqueada e deve seguir o design review em `docs/SPRINT3_DESIGN_REVIEW.md`. O player da Sprint 2 permanece congelado e sujeito a regressao P01-P12 em qualquer integracao futura.

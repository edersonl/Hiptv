# Sprint 2.5 - Playback Validation Checklist

## Identificacao da execucao

- Data/hora:
- Responsavel:
- Commit/build:
- APK: `IptvStarterApp/bin/Debug/net8.0-android/com.company.iptvstarter-Signed.apk`
- Android TV (fabricante/modelo/API):
- Android Mobile (fabricante/modelo/API):
- Rede (Wi-Fi/Ethernet/4G/5G):
- Evidencias anexadas (logs, video, screenshots, metricas):

## Criterios gerais

- [ ] Estado visual acompanha `Buffering`, `Playing`, `Paused`, `Completed` e `Error`.
- [ ] Nao ha crash, ANR, tela preta persistente ou audio sem video.
- [ ] Ao sair da Activity, `PlayerView` perde o player e o engine e liberado.
- [ ] Nao ha callbacks, audio ou reconexao depois de `OnPause`/`OnDestroy`.
- [ ] Memoria retorna a um patamar estavel depois de 20 ciclos abrir/reproduzir/voltar.
- [ ] Logs nao apresentam excecoes nao tratadas nem retries apos dispose.
- [ ] Registrar Startup Time, Time To First Frame, Buffering Ratio, Recovery Count e Playback Duration.

## Matriz obrigatoria

| ID | Cenario | Procedimento | Resultado esperado | TV | Mobile | Evidencia |
|---|---|---|---|---|---|---|
| P01 | MP4 valido | Abrir URL HTTPS MP4 conhecida e reproduzir por 5 min | Primeiro frame, audio/video sincronizados, sem recovery indevido | [ ] | [ ] | |
| P02 | HLS `.m3u8` valido | Abrir live HLS e reproduzir por 15 min | Media3 identifica HLS, mantem playback e buffering ratio aceitavel | [ ] | [ ] | |
| P03 | Stream invalido | Informar URL malformada e URL HTTP 404 | Erro controlado, sem crash; retries limitados; mensagem ao usuario | [ ] | [ ] | |
| P04 | Timeout | Bloquear resposta do host por mais de 15 s | Timeout registrado, backoff exponencial e termino controlado | [ ] | [ ] | |
| P05 | Perda de internet | Durante playback, desligar rede por 30 s | Entra em buffering/erro sem ANR; retries nao se sobrepoem | [ ] | [ ] | |
| P06 | Reconexao | Restaurar rede antes do limite de recovery | Playback retorna ou failover legado ocorre; Recovery Count incrementa uma vez por tentativa | [ ] | [ ] | |
| P07 | Canal offline | Abrir endpoint indisponivel | Circuit breaker abre apos falhas consecutivas; sem loop infinito | [ ] | [ ] | |
| P08 | Buffering prolongado | Limitar banda ate exceder 10 s de buffering | Watchdog inicia uma unica recovery; Buffering Ratio reflete o periodo | [ ] | [ ] | |
| P09 | Lifecycle | Reproduzir e repetir Home/Retomar/Voltar 20 vezes | Sem audio orfao, listener duplicado, crash ou crescimento continuo de memoria | [ ] | [ ] | |
| P10 | Failover legado | Induzir erro recuperavel no Media3 | Uma unica troca para legado; nenhuma troca concorrente; playback ou erro final controlado | [ ] | [ ] | |
| P11 | Controle remoto TV | Navegar, iniciar, parar e voltar somente com D-pad/OK/Back | Foco visivel, comandos unicos e Activity liberada ao sair | [ ] | N/A | |
| P12 | Mobile | Rotacionar quando permitido, bloquear/desbloquear e alternar apps | Lifecycle consistente e nenhuma reproducao orfa | N/A | [ ] | |

## Limites de aceite

| Metrica | Criterio inicial | Valor observado TV | Valor observado Mobile |
|---|---:|---:|---:|
| Startup Time | <= 5 s em rede estavel | | |
| Time To First Frame | <= 5 s em rede estavel | | |
| Buffering Ratio | <= 2% em 15 min de HLS estavel | | |
| Recovery Count | 0 em rede estavel | | |
| Playback Duration | coerente com tempo efetivamente tocado | | |
| Memoria apos 20 ciclos | sem tendencia monotonicamente crescente | | |

## Coleta sugerida

```powershell
adb logcat -c
adb logcat -v threadtime Hiptv/Playback:D AndroidRuntime:E *:S
adb shell dumpsys meminfo com.company.iptvstarter
```

## Encerramento

- [ ] Todos os cenarios obrigatorios passaram nos dois perfis aplicaveis.
- [ ] Falhas possuem ticket, severidade e evidencia.
- [ ] Nenhum defeito P0/P1 esta aberto.
- [ ] QA Lead aprovou promocao para a proxima etapa.

Resultado: `PENDENTE | APROVADO | REPROVADO`

Observacoes:

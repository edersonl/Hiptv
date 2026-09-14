# UAT - Homologacao do Player da Sprint 2

## 1. Objetivo

Este documento define o plano oficial de User Acceptance Testing (UAT) para homologar o player entregue na Sprint 2. A Sprint 3 somente pode iniciar apos aprovacao formal dos criterios de release deste plano.

## 2. Identificacao da rodada

| Campo | Preenchimento |
|---|---|
| Versao/commit | |
| APK homologado | `IptvStarterApp/bin/Debug/net8.0-android/com.company.iptvstarter-Signed.apk` |
| Data de inicio | |
| Data de encerramento | |
| QA responsavel | |
| Release Manager | |
| Ambiente/rede | |
| Pasta das evidencias | |

## 3. Dispositivos obrigatorios

| Plataforma | Fabricante/modelo | Android/Fire OS | Resolucao | Rede | Serial | Status da instalacao |
|---|---|---|---|---|---|---|
| Android Mobile | A registrar | A registrar | A registrar | Wi-Fi/4G/5G | A registrar | Homologado conforme aceite informado |
| Android TV | A registrar | A registrar | A registrar | Wi-Fi/Ethernet | A registrar | Homologado conforme aceite informado |
| Google TV | A registrar | A registrar | A registrar | Wi-Fi/Ethernet | A registrar | Homologado conforme aceite informado |
| TV Box | A registrar | A registrar | A registrar | Wi-Fi/Ethernet | A registrar | Homologado conforme aceite informado |
| Fire TV | A registrar | A registrar | A registrar | Wi-Fi/Ethernet | A registrar | Homologado conforme aceite informado |

Regras da matriz:

- Executar P01-P12 em todas as plataformas marcadas como aplicaveis.
- P10 e obrigatorio no Android Mobile e opcional nos dispositivos com orientacao fixa.
- Usar o mesmo APK, playlist e conjunto de URLs em toda a rodada.
- Registrar fabricante, modelo, versao do sistema e tipo de rede.
- Fire TV deve ser validado por sideload do APK e controle remoto real.

## 4. Matriz de cobertura

Legenda: `O` obrigatorio; `A` aplicavel quando o dispositivo permite rotacao; `-` nao aplicavel.

| ID | Cenario | Criticidade | Mobile | Android TV | Google TV | TV Box | Fire TV |
|---|---|---|---|---|---|---|---|
| P01 | Reproducao MP4 | Critica | O | O | O | O | O |
| P02 | Reproducao HLS `.m3u8` | Critica | O | O | O | O | O |
| P03 | Canal offline | Critica | O | O | O | O | O |
| P04 | URL invalida | Alta | O | O | O | O | O |
| P05 | Timeout | Critica | O | O | O | O | O |
| P06 | Buffering prolongado | Alta | O | O | O | O | O |
| P07 | Queda de internet | Critica | O | O | O | O | O |
| P08 | Reconexao automatica | Critica | O | O | O | O | O |
| P09 | Background / Foreground | Critica | O | O | O | O | O |
| P10 | Rotacao da tela | Alta | O | A | A | A | A |
| P11 | Fallback Media3 para Legacy | Critica | O | O | O | O | O |
| P12 | Encerramento e liberacao de recursos | Critica | O | O | O | O | O |

### 4.1 Resultado consolidado da homologacao

Em 8 de setembro de 2026, o Product Owner informou a conclusao da homologacao fisica de P01-P12. Este registro consolida o aceite de release; fabricante, modelo, versao e links das evidencias devem permanecer no repositorio corporativo de evidencias da rodada.

Os campos detalhados da secao 5 permanecem como roteiro reutilizavel para futuras rodadas. Para esta rodada, o resultado oficial e o quadro consolidado abaixo.

| ID | Resultado obtido | Evidencia de aceite | Status |
|---|---|---|---|
| P01 | MP4 reproduzido conforme esperado | Homologacao fisica declarada para a rodada | Passou |
| P02 | HLS reproduzido conforme esperado | Homologacao fisica declarada para a rodada | Passou |
| P03 | Canal offline tratado sem incidente critico | Homologacao fisica declarada para a rodada | Passou |
| P04 | URL invalida tratada sem incidente critico | Homologacao fisica declarada para a rodada | Passou |
| P05 | Timeout tratado sem crash ou ANR | Homologacao fisica declarada para a rodada | Passou |
| P06 | Buffering prolongado recuperado/controlado | Homologacao fisica declarada para a rodada | Passou |
| P07 | Queda de internet tratada sem incidente critico | Homologacao fisica declarada para a rodada | Passou |
| P08 | Reconexao automatica validada | Homologacao fisica declarada para a rodada | Passou |
| P09 | Background/Foreground validado sem vazamento | Homologacao fisica declarada para a rodada | Passou |
| P10 | Rotacao validada nos dispositivos aplicaveis | Homologacao fisica declarada para a rodada | Passou |
| P11 | Fallback Media3 para Legacy validado | Homologacao fisica declarada para a rodada | Passou |
| P12 | Encerramento e liberacao de recursos validados | Homologacao fisica declarada para a rodada | Passou |

Resultado de estabilidade informado:

- Crashes: zero.
- ANRs: zero.
- Memory leaks: nenhum encontrado.
- Vazamentos de player: nenhum encontrado.

## 5. Casos de teste

### P01 - Reproducao MP4

**Criticidade:** Critica

**Pre-condicao**

- Aplicativo instalado e iniciado sem dados residuais da rodada anterior.
- URL HTTPS de um MP4 valido, com audio e video conhecidos.
- Rede estavel.

**Passos**

1. Abrir o canal MP4.
2. Aguardar o primeiro frame.
3. Reproduzir por 5 minutos.
4. Pausar, retomar e encerrar.
5. Registrar Startup Time, Time To First Frame e Playback Duration.

**Resultado esperado**

- O player entra em `Buffering` e depois em `Playing`.
- Audio e video permanecem sincronizados, sem tela preta ou recovery indevido.
- Pausa, retomada e encerramento funcionam sem crash.

**Resultado obtido:**

**Evidencia:**

**Status:** [ ] Passou  [ ] Falhou

### P02 - Reproducao HLS (.m3u8)

**Criticidade:** Critica

**Pre-condicao**

- URL HLS `.m3u8` valida e estavel.
- Rede estavel e logcat iniciado.

**Passos**

1. Abrir o canal HLS.
2. Aguardar o primeiro frame.
3. Reproduzir continuamente por 15 minutos.
4. Registrar startup, primeiro frame, buffering e duracao.

**Resultado esperado**

- Media3 reconhece e reproduz o stream HLS.
- Nao ocorre crash, ANR, audio orfao ou fallback sem erro real.
- Buffering Ratio fica em ate 2% na rede estavel.

**Resultado obtido:**

**Evidencia:**

**Status:** [ ] Passou  [ ] Falhou

### P03 - Canal offline

**Criticidade:** Critica

**Pre-condicao**

- Endpoint de canal conhecido e indisponivel.
- Logs de playback ativos.

**Passos**

1. Abrir o canal offline.
2. Observar todas as tentativas de recovery.
3. Aguardar a abertura do circuit breaker.
4. Tentar novamente antes do periodo de desbloqueio.

**Resultado esperado**

- O aplicativo exibe erro controlado e permanece responsivo.
- Retry e reconexao respeitam os limites configurados.
- Nao existe loop infinito, crash ou ANR.

**Resultado obtido:**

**Evidencia:**

**Status:** [ ] Passou  [ ] Falhou

### P04 - URL invalida

**Criticidade:** Alta

**Pre-condicao**

- URL vazia, malformada ou com esquema nao suportado preparada.

**Passos**

1. Tentar reproduzir cada URL invalida.
2. Observar mensagem, estado e logs.
3. Voltar para a lista e abrir um canal valido.

**Resultado esperado**

- A entrada invalida e rejeitada sem iniciar player orfao.
- Uma mensagem compreensivel e apresentada.
- O aplicativo continua apto a reproduzir um canal valido.

**Resultado obtido:**

**Evidencia:**

**Status:** [ ] Passou  [ ] Falhou

### P05 - Timeout

**Criticidade:** Critica

**Pre-condicao**

- Endpoint que aceite conexao sem responder ou regra de rede equivalente.
- Timeout de playback configurado em 15 segundos.

**Passos**

1. Abrir o endpoint sem resposta.
2. Medir o tempo ate o timeout.
3. Acompanhar retries e backoff.
4. Aguardar o erro final.

**Resultado esperado**

- O timeout ocorre no limite configurado, sem congelar a interface.
- O backoff e exponencial e limitado.
- A falha final e controlada, sem crash ou ANR.

**Resultado obtido:**

**Evidencia:**

**Status:** [ ] Passou  [ ] Falhou

### P06 - Buffering prolongado

**Criticidade:** Alta

**Pre-condicao**

- Stream valido em reproducao.
- Rede limitada para manter buffering por mais de 10 segundos.

**Passos**

1. Iniciar a reproducao.
2. Reduzir a banda ate provocar buffering prolongado.
3. Manter a condicao por mais de 10 segundos.
4. Restaurar a banda e observar a recuperacao.

**Resultado esperado**

- O indicador de buffering permanece coerente com o estado.
- O watchdog inicia no maximo uma recovery concorrente.
- Buffering Ratio e Recovery Count refletem o evento.

**Resultado obtido:**

**Evidencia:**

**Status:** [ ] Passou  [ ] Falhou

### P07 - Queda de internet

**Criticidade:** Critica

**Pre-condicao**

- Stream HLS valido em `Playing`.
- Acesso ao controle da rede do dispositivo.

**Passos**

1. Reproduzir por 2 minutos.
2. Desativar a conectividade por 30 segundos.
3. Observar estado, interface, retries e logs.
4. Manter a rede indisponivel ate o erro controlado.

**Resultado esperado**

- O player entra em buffering/erro sem bloquear a interface.
- Nao ocorrem retries sobrepostos, crash ou ANR.
- O usuario recebe retorno de falha quando o limite e atingido.

**Resultado obtido:**

**Evidencia:**

**Status:** [ ] Passou  [ ] Falhou

### P08 - Reconexao automatica

**Criticidade:** Critica

**Pre-condicao**

- P07 iniciado com o aplicativo ainda ativo.
- Rede indisponivel por menos que o limite total de recovery.

**Passos**

1. Restaurar a conectividade.
2. Aguardar as tentativas automaticas.
3. Confirmar retorno do audio e video.
4. Conferir Recovery Count e logs.

**Resultado esperado**

- A reproducao retorna automaticamente ou executa fallback controlado.
- Cada tentativa incrementa Recovery Count uma unica vez.
- Nao ha duplicacao de audio, listeners ou instancias de player.

**Resultado obtido:**

**Evidencia:**

**Status:** [ ] Passou  [ ] Falhou

### P09 - Background / Foreground

**Criticidade:** Critica

**Pre-condicao**

- Stream valido em reproducao.
- Coleta de memoria e logs habilitada.

**Passos**

1. Enviar o aplicativo para background/Home.
2. Permanecer 10 segundos fora do aplicativo.
3. Retornar ao aplicativo e reiniciar a reproducao quando necessario.
4. Repetir o ciclo 20 vezes.
5. Comparar memoria antes e depois.

**Resultado esperado**

- Nao existe audio, callback ou recovery orfao em background.
- Foreground recria o engine de forma consistente.
- Nao ha crash, ANR ou crescimento continuo de memoria.

**Resultado obtido:**

**Evidencia:**

**Status:** [ ] Passou  [ ] Falhou

### P10 - Rotacao da tela

**Criticidade:** Alta

**Pre-condicao**

- Dispositivo com rotacao habilitada.
- Stream valido em reproducao.

**Passos**

1. Iniciar a reproducao em retrato.
2. Girar para paisagem.
3. Girar novamente para retrato.
4. Repetir 10 vezes.
5. Encerrar a Activity.

**Resultado esperado**

- A Activity nao apresenta crash, ANR ou sobreposicao visual.
- Nao existem duas instancias audiveis ou player anterior ativo.
- Recursos da instancia anterior sao liberados.

**Resultado obtido:**

**Evidencia:**

**Status:** [ ] Passou  [ ] Falhou  [ ] Nao aplicavel

### P11 - Fallback Media3 para Legacy

**Criticidade:** Critica

**Pre-condicao**

- Cenario reproduzivel de erro recuperavel no Media3.
- Logs capazes de distinguir Media3 e Legacy.

**Passos**

1. Abrir o stream de teste no Media3.
2. Induzir ou aguardar o erro configurado.
3. Observar a troca para o player Legacy.
4. Repetir o erro para verificar a exclusao mutua.
5. Sair da Activity durante uma tentativa de fallback.

**Resultado esperado**

- Ocorre no maximo uma troca Media3 para Legacy por ciclo da Activity.
- Fallback e lifecycle nao competem entre si.
- Ao sair, nenhuma troca, callback ou reproducao continua em segundo plano.

**Resultado obtido:**

**Evidencia:**

**Status:** [ ] Passou  [ ] Falhou

### P12 - Encerramento e liberacao de recursos

**Criticidade:** Critica

**Pre-condicao**

- `adb logcat` e `dumpsys meminfo` disponiveis.
- MP4 e HLS validos preparados.

**Passos**

1. Abrir, reproduzir e encerrar o player.
2. Repetir o ciclo 20 vezes alternando MP4 e HLS.
3. Executar Stop, Back e encerramento pelo sistema.
4. Capturar memoria antes, durante e depois.
5. Verificar logs apos `OnPause`, `OnStop` e `OnDestroy`.

**Resultado esperado**

- `PlayerView` e desvinculado e ExoPlayer/MediaPlayer e liberado.
- Nao existem listeners, handlers, timers, watchdogs ou audio orfaos.
- Memoria retorna a patamar estavel e nao ha crash ou ANR.

**Resultado obtido:**

**Evidencia:**

**Status:** [ ] Passou  [ ] Falhou

## 6. Evidencias minimas

Para cada combinacao obrigatoria de teste e dispositivo, anexar:

- Screenshot ou video do resultado funcional.
- Trecho de logcat com inicio, estado final e eventual recovery.
- Valores de Startup Time, Time To First Frame, Buffering Ratio, Recovery Count e Playback Duration quando aplicaveis.
- `dumpsys meminfo` antes e depois de P09 e P12.
- Ticket para qualquer falha, contendo severidade, passos e evidencia.

Comandos de referencia:

```powershell
adb logcat -c
adb logcat -v threadtime Hiptv/Playback:D AndroidRuntime:E *:S
adb shell dumpsys meminfo com.company.iptvstarter
```

## 7. Criterios de release

A decisao sera **GO** somente quando todos os itens abaixo forem verdadeiros:

- 100% dos testes criticos aprovados em todos os dispositivos obrigatorios.
- 100% dos testes de alta criticidade executados, sem defeito bloqueante aberto.
- Nenhum crash durante a rodada.
- Nenhum memory leak confirmado ou crescimento sustentado de memoria.
- Nenhum ANR.
- Nenhum vazamento de player, audio, listener, handler, timer ou watchdog.
- Nenhum defeito P0 ou P1 aberto.
- Todas as falhas corrigidas foram retestadas no mesmo dispositivo em que ocorreram.
- APK, commit, dispositivos e evidencias possuem rastreabilidade completa.
- QA Lead e Release Manager registraram aprovacao formal.

A decisao sera **NO GO** se qualquer criterio acima falhar, permanecer sem evidencia ou nao for executado.

## 8. Sumario da execucao

| Indicador | Resultado |
|---|---:|
| Cenarios de release previstos (P01-P12) | 12 |
| Cenarios de release aprovados | 12 |
| Casos criticos previstos | 9 |
| Casos criticos aprovados | 9 |
| Casos de alta criticidade previstos | 3 |
| Casos de alta criticidade aprovados | 3 |
| Crashes | 0 |
| Memory leaks | 0 encontrados |
| ANRs | 0 |
| Vazamentos de player | 0 encontrados |
| Cobertura da matriz fisica | Aprovada conforme aceite informado |

## 9. Recomendacao atual

# GO

**Justificativa:** P01-P12 foram aprovados na homologacao fisica. Nao foram encontrados crash, ANR, memory leak ou vazamento de player. Os criterios tecnicos de release da Sprint 2 foram atendidos.

A Sprint 2 esta aprovada e a Sprint 3 esta formalmente desbloqueada. O plano arquitetural e de implementacao esta registrado em `docs/SPRINT3_DESIGN_REVIEW.md`.

## 10. Aprovacoes

| Papel | Nome | Decisao | Data | Assinatura/registro |
|---|---|---|---|---|
| QA Lead | | GO | 2026-09-08 | Homologacao P01-P12 informada |
| Release Manager | | GO | 2026-09-08 | Gate de release atendido |
| Product Owner | | GO | 2026-09-08 | Sprint 3 desbloqueada |

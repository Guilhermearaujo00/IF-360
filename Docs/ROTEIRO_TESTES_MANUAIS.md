# Roteiro de Testes Manuais — Reestruturação das Missões (IF 360°)

> **Contexto**: validação em **Play Mode** da reestruturação do sistema de
> missões (modelo central com `QuestManager` hospedando o `MotorRegrasMissao`).
> Os testes automatizados estão em
> `Assets/Scripts/Missoes/Tests/Editor/MotorRegrasMissaoTests.cs` (modo Edit).
> Este roteiro cobre o que só dá para validar jogando: UI, diálogos, minijogos
> e persistência.

---

## 0. Preparação

1. Abra o projeto no Unity e espere compilar **sem erros**.
2. (Opcional, recomendado) `Window > General > Test Runner > EditMode > Run All`
   — os 23 testes do motor devem passar.
3. Abra a cena `Assets/Scenes/Campus.unity` e entre em Play Mode pelo menu
   principal (`Iniciar Jogo` — garante save limpo).
4. Tenha o **Console** visível: todo evento relevante loga com o prefixo
   `[MISSION]`.

**Atalhos**: `E` interage/conversa; minijogo fios usa `←/→` + `1/2/3`;
compilação usa `1–5`; `Esc` sai do minijogo.

---

## 1. Q01 — Matrícula no Ensino Médio (Ananda → Roney → fios → Ananda)

| # | Ação | Resultado esperado |
|---|------|--------------------|
| 1.1 | Ande até a **Ananda** (Prédio do Ensino Médio) e converse (`E`) até o fim | Log `[MISSION] ... avaliado: InteractionAllowed`; missão inicia (`Missão iniciada: missao_ensino_medio`); HUD mostra "Matrícula no Ensino Médio — Etapa 1 de 4" e "0 de 4 missões concluídas" |
| 1.2 | Converse com o **Roney** (Administração) até o fim | HUD "Etapa 2 de 4"; fala da **Secretaria** aparece no diálogo (2ª fala); objetivo `Q01_EXPLICACAO_SERVIDOR` concluído |
| 1.3 | Ainda em frente ao Roney, espere o diálogo encerrar | O minijogo dos **fios** abre SOZINHO (autorização central liberada); log `Minijogo 'fios' AUTORIZADO para NPC_RONEY` |
| 1.4 | **Feche o minijogo com Esc (derrota)** | Nada avança: HUD continua "Etapa 3 de 4"; log `falhou — objetivo NÃO registrado` |
| 1.5 | Converse com Roney de novo e vença os fios | Objetivo `Q01_SERVIDOR_CONECTADO` registrado **apenas na vitória**; +100 pontos UMA vez; HUD "Etapa 4 de 4"; estado ReadyToTurnIn |
| 1.6 | Converse com Roney uma terceira vez | Diálogo legado `Dialogo_Roney_Pos` (variante); **sem** reabrir o minijogo (log: autorização negada — nada pendente) |
| 1.7 | Converse com a **Ananda** e encerre | Missão concluída; HUD "Concluída: ..." e "1 de 4 missões concluídas"; log `Missão concluída` |

**Verificação-chave do fix central (bug do Márcio)**: em 1.3 e 1.5, o objetivo
da etapa de minijogo **nunca** é registrado pelo diálogo — somente pela
vitória reportada ao `QuestManager` (pontos só com registro aceito).

---

## 2. Q02 — Biblioteca (Thiago + JP/Gleice/Rosy paralelos)

| # | Ação | Resultado esperado |
|---|------|--------------------|
| 2.1 | Converse com o **Thiago** (Biblioteca) | Missão inicia; HUD "Etapa 2 de 3"; objetivo diz **"Faltam 3 de 3: JP, Gleice e Rosy."** |
| 2.2 | Converse com **Rosy** primeiro (ordem livre) | HUD atualiza para "Faltam 2 de 3: JP e Gleice." |
| 2.3 | Converse com Rosy de novo | Diálogo `Dialogo_Rosy_Pos`; log `ObjectiveAlreadyCompleted` — nada registra |
| 2.4 | Converse com **JP** e **Gleice** em qualquer ordem | Ao terminar o terceiro, HUD "Etapa 3 de 3" (ReadyToTurnIn) |
| 2.5 | Converse com o **Thiago** | Missão concluída; "2 de 4 missões concluídas" |

---

## 3. Q03 — Núcleo (Josy → David → Márcio/compilação → Josy)

| # | Ação | Resultado esperado |
|---|------|--------------------|
| 3.1 | Converse com a **Josy** (Gabinete de Docentes) | Missão inicia |
| 3.2 | Converse com o **David** (laboratório Bridge/EAD) | Componentes recebidos; HUD etapa de compilação |
| 3.3 | Converse com o **Márcio** (Prédio do Ensino Superior) e encerre o diálogo | Minijogo de **compilação** abre autorizado; SEMPRE começa no estágio 1/3 |
| 3.4 | Deixe o tempo esgotar ou erre uma resposta | Padrão é refeito; sem punição; nada é registrado |
| 3.5 | Vença os 3 estágios | `COMPILATION` registrado na vitória; **+150 pontos uma única vez**; HUD etapa final (ReadyToTurnIn) |
| 3.6 | Tente conversar com Márcio de novo | Diálogo `Dialogo_Marcio_Pos`; **minijogo NÃO reabre** (decisão documentada: sem farm de pontos) |
| 3.7 | Converse com a **Josy** | Missão concluída; "3 de 4 missões concluídas" |

---

## 4. Q04 — Refeitório (João → Cláudio → João)

| # | Ação | Resultado esperado |
|---|------|--------------------|
| 4.1 | Converse com o **João** (Refeitório) | Missão inicia |
| 4.2 | Converse com o **Cláudio** (Posto de Vendas) | Ticket comprado; repetir a conversa não registra de novo |
| 4.3 | Converse com o **João** | Missão concluída; **"4 de 4 missões concluídas"**; HUD: "Jornada completa!" |

---

## 5. Diálogos dinâmicos (Situações A/B/C — validação central)

Executar DURANTE uma missão ativa (recomece em Q02 se necessário):

| # | Ação | Resultado esperado |
|---|------|--------------------|
| 5.1 | Com Q02 ativa, converse com a **Ananda** (já concluída) | **Situação A** (diálogo dinâmico): "você já está ocupado com outra atividade..." apontando o próximo passo da missão ativa |
| 5.2 | Com Q02 na etapa de lembretes, converse com o **David** (Q03) | **Situação A** (outra missão) — não inicia nada |
| 5.3 | Com Q02 na etapa de lembretes, converse com o **João** (iniciador Q04, Locked) | **Situação A** — orientação para terminar o que começou |
| 5.4 | Sem missão ativa (antes de iniciar qualquer missão), converse com o **Roney** | **Situação C**: "Procure primeiro Ananda (Prédio do Ensino Médio)..." |
| 5.5 | Sem missão ativa, converse com o **Thiago** (Q02 Locked) | **MissingRequirements** (dinâmico): cita a pendência com Ananda |
| 5.6 | Com Q01 na etapa do Roney, converse com a **Ananda** | **Situação B**: "Isso não é comigo agora..." + próxima ação |
| 5.7 | Converse com a **Alice** (Assistência Estudantil) a qualquer momento | Diálogo informativo normal; **nunca** loga progressão |

---

## 6. NPCs legados (fora do catálogo)

| # | Ação | Resultado esperado |
|---|------|--------------------|
| 6.1 | Se houver NPC **Secretaria** na cena, tente conversar | Conversa informativa neutra; nenhum log de progressão |
| 6.2 | Secretária pede minijogo (`chamarMinijogo=true` legado) | Log `[MISSION] Minijogo NEGADO ... NPC fora do catálogo` — **não abre** (decisão documentada) |

---

## 7. Save / Continuar / Novo jogo

| # | Ação | Resultado esperado |
|---|------|--------------------|
| 7.1 | Com Q01 em andamento, feche o Play Mode e entre de novo em Play pela cena Campus | Progresso restaurado (auto-restore do SaveManager no Start); mesma etapa de antes |
| 7.2 | No menu, use **Continuar** | Progresso restaurado; sem reset indevido |
| 7.3 | No menu, use **Iniciar Jogo** | Save apagado (inclusive via método estático — sem SaveManager na cena do menu); começa do zero |
| 7.4 | Migração: use um save da versão ANTIGA do jogo (flags + passos flat) e continue | Log `[MISSION] Migrando progresso...`; objetivos/etapas derivados; sem perda de progresso |

---

## 8. Regressões gerais

- [ ] Colecionáveis e pontos continuam funcionando.
- [ ] `E` durante minijogo (fios OU compilação) não abre diálogo/interação.
- [ ] Placas informativas continuam abrindo na caixa de diálogo.
- [ ] Nenhum `NullReferenceException` no Console durante todo o roteiro.
- [ ] Logs `[MISSION]` coerentes com as ações (nada de "objetivo registrado" fora de hora).

---

## Decisões documentadas (para referência do QA)

1. **Minijogos não reabrem** após o objetivo ser concluído (evita re-ganho de
   pontos: fios=100, compilação=150).
2. **NPCs fora do catálogo** (Diretor, Secretaria) nunca recebem autorização
   de minijogo; mantêm conclusão flat legada (`missao_diretor`).
3. O papel narrativo da **Secretaria** migrou para o **Roney** (fala adicional
   em `Dialogo_Roney_Pre`).
4. HUD conta **"X de 4"** (missões do catálogo central), não 5 — ids legados
   fora do catálogo não entram na contagem.

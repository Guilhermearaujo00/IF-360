# Relatório Final — Reestruturação do Sistema de Missões (IF 360°)

> **Honestidade sobre o ambiente de trabalho**: toda a implementação abaixo foi
> feita em ambiente CLI (edição de arquivos + verificações estruturais). Os
> testes de modo Edit foram executados FORA do Unity (Roslyn do próprio editor
> + NUnit do pacote, runner por reflexão) e **confirmados no ambiente oficial:
> 23/23 verdes no Unity Test Runner em 2026-11-11**. Compilação confirmada no
> editor (a execução dos testes exige projeto compilado). Resta apenas a
> validação manual em Play Mode (seção "Status de execução").

---

## 1. O que foi implementado

### 1.1 Lógica pura em C# (assembly `IF360.Missoes`, testável em modo Edit)

Arquivos em `Assets/Scripts/Missoes/`:

| Arquivo | Papel |
|---|---|
| `IF360.Missoes.asmdef` | Assembly isolado, sem dependência de MonoBehaviours |
| `TiposMissao.cs` | Enums (`EstadoMissao`, `TipoNPC`, `TipoInteracaoEtapa`, `ResultadoInteracao`) |
| `ModeloNPC.cs` | `PerfilNPC` (id, nome, local, tipo, missão associada, etapas/objetivos próprios) |
| `ModeloMissao.cs` | `DefinicaoMissao`/`DefinicaoEtapa`/`DefinicaoObjetivo` (etapas sequenciais, objetivos paralelos, regra de conclusão) |
| `EstadoProgresso.cs` | DTOs: save v2, save legado, `ResumoProgresso`, `ResultadoEncerramento`, `AutorizacaoMinijogo`, `FalaDinamica` |
| `CatalogoIf360.cs` | Catálogo oficial: 4 missões (Q01–Q04) e 12 NPCs classificados |
| `MotorRegrasMissao.cs` | **Autoridade central** (991 linhas): avaliação de interações, registro de objetivos, autorização/resultado de minijogos, migração e consistência |
| `DialogosContextuais.cs` | Diálogos dinâmicos (Situações A/B/C + MissingRequirements) com tokens |

### 1.2 Integração Unity (MonoBehaviours apenas chamam a lógica)

| Arquivo | Mudança |
|---|---|
| `Scripts/Sistemas/QuestManager.cs` | **Reescrito**: hospeda o motor; API central (`AvaliarInteracao`, `RegistrarEncerramentoDialogo`, `AutorizarMinijogo`, `RegistrarResultadoMinijogo`, `ObterResumoProgresso`) + API legada mantida; serialização v2 com migração automática; guarda de sessão (DDOL) |
| `Scripts/NPC/NPCBaseNovo.cs` | **Reescrito**: mantém TODOS os campos serializados legados (cena intacta); resolve perfil no catálogo pelo `nomeNPC` (ou `npcIdOverride`); avalia centralmente antes de conversar; diálogo dinâmico em runtime; encerra repassando ao central; minijogo só com autorização (expõe `AutorizacaoMinijogoAtual`) |
| `Scripts/Core/GameManager.cs` | Fachadas `RegistrarDialogoEncerrado` / `RegistrarResultadoMinijogo` com auto-save |
| `Scripts/Sistemas/SaveManager.cs` | Auto-restore no `Start` (via `RestaurarSeNecessario`); `TemSaveEstatico`/`ApagarSaveEstatico` |
| `Scripts/UI/Menus/MenuPrincipal.cs` | "Iniciar Jogo" apaga o save de verdade (estático — o menu não tem SaveManager na cena); "Continuar" verifica estaticamente |
| `Scripts/Player/PlayerInteraction.cs` | `E` bloqueado durante QUALQUER minijogo (fios ou compilação) |
| `Scripts/Minijogos/MinijogoFios.cs` | `Fechar` reporta vitória centralmente; pontos só se aceito; sem conclusão direta de etapa/missão |
| `Scripts/Minijogos/MinijogoCompilacao.cs` | Removida sincronia por passos (etapas 3–5 legadas, `emSincroniaComMissao`, `PrimeiraEtapaDoMinijogo`); sempre começa no estágio 1; reporte central idem |
| `Scripts/UI/HUD/HUDMissao.cs` | **Reescrito**: consome `ResumoProgresso` (etapa atual, "faltam X de N" dinâmico, próxima ação, "X de 4 missões concluídas"); eventos legados mantidos |
| `Scripts/Sistemas/DialogueManager.cs` | Sem mudança estrutural (já consumia `DialogoParaConversa`); doc atualizado |

### 1.3 Editor e dados (fontes de regeneração + assets)

| Arquivo | Mudança |
|---|---|
| `Assets/Editor/PosicionarNPCs.cs` | Wiring legado mantido (idempotente, NPCs da cena intactos); doc atualizado sobre autorização central |
| `Assets/Editor/CriarDialogosNPCs.cs` | Fala da Secretaria adicionada ao `Dialogo_Roney_Pre` (papel narrativo migrado para Roney) |
| `Assets/Dados/Dialogos/Dialogo_Roney_Pre.asset` | YAML estendido com a fala da Secretaria (escape Unicode no padrão do arquivo) |

### 1.4 Testes (Fase 5)

| Arquivo | Conteúdo |
|---|---|
| `Assets/Scripts/Missoes/Tests/Editor/IF360.Missoes.Tests.asmdef` | Assembly de testes (Editor only, NUnit, referência a `IF360.Missoes`) |
| `Assets/Scripts/Missoes/Tests/Editor/MotorRegrasMissaoTests.cs` | **23 testes**: cenários 1–11 (subordinado não inicia; iniciador inicia; uma missão por vez; cadeia; sequência de etapas; objetivos paralelos em qualquer ordem + "faltam X de 3" + sem duplicação; informativo; fora do catálogo; fix minijogo; só finalizador conclui; missão concluída não reabre), estados/eventos do fluxo, compatibilidade legada (passos, `missao_administracao`/`missao_assistencia`, flat), migração, consistência e round-trip de serialização |
| `Packages/manifest.json` | Adicionado `com.unity.test-framework: 1.6.0` (versão já resolvida no packages-lock) |
| `Docs/ROTEIRO_TESTES_MANUAIS.md` | Roteiro completo de Play Mode (Q01–Q04, situações A/B/C, NPCs legados, save/continuar/novo jogo, migração) |

---

## 2. Status de execução (o que rodou, o que não rodou)

### ✅ Executado neste ambiente
- Escrita/edição de **23 arquivos** listados acima.
- Verificações estruturais: balanceamento de chaves nos arquivos maiores
  (`MotorRegrasMissao` 130/130, `QuestManager` 47/47, `NPCBaseNovo` 71/71,
  testes 37/37); ausência de marcadores temporários; conferência cruzada de
  nomes de API entre chamadores (GameManager ↔ QuestManager ↔ NPCBaseNovo ↔
  minijogos ↔ HUD ↔ catálogo).
- Validação textual do YAML do asset de diálogo (padrão de escape/folding).
- **Correção do erro de compilação reportado no Unity** (CS0103 ×3 + CS0067):
  typo na declaração do evento em `NPCBaseNovo.cs` — linha 58 declarava
  `JogadorSaiuNaArea` enquanto o corpo usava `JogadorSaiuDaArea` (nome
  original). Renomeada a declaração de volta para `JogadorSaiuDaArea`.
- **Verificação estática de compilação** com o Roslyn que o próprio editor
  Unity 6000.4.0f1 instala, replicando a divisão de assemblies do Unity em
  3 passes — `IF360.Missoes` (7 fontes, C# puro), `Assembly-CSharp`
  (35 fontes, refs UnityEngine/UnityEditor + DLLs de pacotes de
  `Library/ScriptAssemblies`) e `Assembly-CSharp-Editor` (43 fontes,
  inclui `PosicionarNPCs`/`CriarDialogosNPCs`): **0 erros** nos três
  passes. Os avisos CS0649/CS0169 que aparecem são falso positivo fora do
  Unity (campos `[SerializeField]` atribuídos via Inspector).
- **Execução dos 23 testes de modo Edit (fora do Unity), 23/23 aprovados em
  2026-11-11**: o usuário executou a suíte no Unity Test Runner (21/23 —
  resultado exportado em
  `Assets/Modelos/Personagens/Materials/TestResults_20261111_005716.xml`);
  as 2 falhas eram expectativas ERRADAS nos próprios testes de "passo"
  legado (corrigidas — ver decisão nº 7 abaixo); depois disso a suíte foi
  recompilada (Roslyn do editor + `nunit.framework.dll` do pacote
  com.unity.ext.nunit, perfil unity-4.8-api) e **executada de fato** num
  runner por reflexão no CLR do PowerShell: **23 passaram, 0 falharam,
  0 erros**.
- **Confirmação no ambiente oficial (Unity, pelo usuário, 2026-11-11)**:
  **23/23 verdes** no `Window > General > Test Runner > EditMode > Run All`
  após as 2 correções de expectativa — o que também confirma a compilação
  do projeto no editor (Assembly-CSharp com o fix do `NPCBaseNovo.cs`,
  motor puro e testes).

### ⚠️ Escrito, NÃO executado
- Nada — compilação no editor e suíte EditMode foram confirmadas (acima).
  Falta apenas a validação manual em Play Mode (abaixo).

### 🖐️ Exige validação manual (Play Mode)
- Roteiro completo em `Docs/ROTEIRO_TESTES_MANUAIS.md` (inclui a verificação
  do fix do Márcio/Roney: objetivo de etapa de minijogo registrado SOMENTE
  pela vitória reportada ao centro — nunca pelo diálogo).

---

## 3. Decisões documentadas

1. **HUD conta "X de 4"** (missões do catálogo central Q01–Q04). O número 5
   citado informalmente no plano não existe no catálogo escrito — ids legados
   fora do catálogo (`missao_diretor`, `missao_assistencia`) não entram na
   contagem (mas continuam preservados no save e nas consultas legadas).
2. **Minijogos não reabrem** após o objetivo concluir (sem farm de pontos:
   fios=100, compilação=150). Derrota/Esc não registra nada.
3. **NPCs fora do catálogo** (Diretor, Secretaria) nunca recebem autorização
   de minijogo (log `[MISSION] ... NPC fora do catálogo`); mantêm conclusão
   flat legada.
4. **Compilação sempre começa no estágio 1/3** — o "passo" legado virou uma
   etapa única (`Q03_COMPILACAO`); não há mais retomada por passo.
5. **Papel narrativo da Secretaria migrado para Roney** (fala adicional em
   `Dialogo_Roney_Pre`).
6. **`ConcluirEtapa`/`DefinirPasso`/`LimparFlag`** viraram stubs com warning
   (obsoletos no modelo central) — nenhuma chamada do jogo os usa para
   progressão real.
7. **Passo legado (`ObterPassoMissao`) = ordem da etapa ATUAL do fluxo**
   (0 antes de a missão iniciar; etapa de minijogo do nucleo mapeia para 5;
   ReadyToTurnIn/Completed devolvem os limiares de "entrega": 4/4/5/2).
   Decidido a partir dos limiares `requisitoEtapaMinima` gravados na CENA
   (`Campus.unity`) — que não pode ser alterada — e do fato de que, no modelo
   central, conversas com NPC errado no meio da missão viram diálogo DINÂMICO
   (Situações A/B/C) e não consultam variantes:
   - Josy: Durante `nucleo>=2`, Pos `nucleo>=5`;
   - Thiago: Durante `biblioteca>=1`, Pos `biblioteca>=4`;
   - João: Durante `refeitorio>=1`, Pos `refeitorio>=2`;
   - Ananda/Roney: só estados completos (`-1`/missão completa).
   Os 2 testes que falharam no Unity Test Runner (21/23) codificavam a
   semântica alternativa "passo = etapas concluídas" (sem consumidor na
   cena) — um deles ainda continha uma contradição interna (esperava 2 e
   depois 5 sem transição de estado entre as asserções). Expectativas
   corrigidas para a semântica acima; a implementação NÃO mudou.

---

## 4. Como o estado flui agora (resumo)

```
E (jogador) ──► NPCBaseNovo.Interagir
                 └─► QuestManager.AvaliarInteracao(npcId)        [decisão central]
                      └─ diálogo dinâmico (Situações A/B/C) OU variante legada
DialogueManager encerra ──► NPC.NotificarDialogoEncerrado
                 └─► GameManager.RegistrarDialogoEncerrado(npcId)  [→ motor]
                      └─ inicia missão / registra objetivo de etapa de DIÁLOGO / finaliza
Minijogo (autorizado antes pelo motor) vence ──► GameManager.RegistrarResultadoMinijogo
                      └─ registra objetivo de etapa de MINIJOGO (única via) + pontos
Motor dispara eventos ──► QuestManager traduz ──► HUD / save (auto-save nas fachadas)
```

Nada de progressão acontece fora do `MotorRegrasMissao`. Uma única missão
ocupa o slot por vez; etapas avançam em ordem; objetivos paralelos registram
em qualquer ordem e 1x cada.

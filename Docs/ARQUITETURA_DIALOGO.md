# Arquitetura do Sistema de Diálogo — IFNMG 360°

> **Decisão atual (29/09/2026):** script base de NPC é o **`NPCBaseNovo`** (novo, desacoplado — não cria UI, não lê input, não controla conversa). O sistema de diálogo é **centralizado**: `PlayerInteraction` (único ouvinte de E) + `DialogueManager` (estados) + `DialogueUI` (visual, TextMeshPro). O antigo `NPCBase` auto-contido foi **removido**.

---

## 1. Diagrama de componentes

```
                    E (Input System)
                          │
                          ▼
              ┌─── PlayerInteraction (1 na cena) ───┐
              │  • único ouvinte de Player.Interact  │
              └───────┬──────────────┬───────────────┘
                      │              │ E com diálogo ativo
                      │ E sem diálogo │      │
                      ▼              ▼      ▼
            NPCBaseNovo.Interagir()   DialogueManager.Avancar()
                      │                     │
         NPCBaseNovo.DialogoSolicitado ◄────┘ (evento estático)
                      │ IniciarDialogo(npc)
                      ▼
            ┌── DialogueManager (Systems) ──┐
            │ NONE/TYPING/COMPLETE/CLOSING   │
            │ lê npc.dialogoData / fala       │
            └──────────────┬─────────────────┘
                           │ Mostrar/DefinirNome/DefinirTexto
                           ▼
                  ┌── DialogueUI (Canvas) ──┐
                  │ CaixaDialogo + 3 textos  │
                  └──────────────────────────┘
                           │
                 NPCBaseNovo.NotificarDialogoEncerrado()
                           │
                     AoEncerrarDialogo() → IniciarMinijogo()
```

Proximidade: cada `NPCBaseNovo` possui Trigger próprio (SphereCollider, raio 2.5). O trigger **apenas** marca disponibilidade (`JogadorEntrouNaArea` / `JogadorSaiuDaArea`, eventos estáticos). Não abre diálogo automaticamente — ver Fluxo abaixo.

## 2. Responsabilidades

| Componente | Arquivo | Responsabilidade | NÃO faz |
|---|---|---|---|
| NPCBaseNovo | `Scripts\NPC\NPCBaseNovo.cs` | Identidade (`nomeNPC`, `idMissao`), dados (`dialogoData` / `falaApresentacao`), proximidade (Trigger), validação de interação, evento `DialogoSolicitado`, gancho `AoEncerrarDialogo()` | Não cria UI, não lê input, não conduz a conversa |
| PlayerInteraction | `Scripts\Player\PlayerInteraction.cs` | Único ouvinte de E; mantém lista de NPCs elegíveis; escolhe o **mais próximo**; roteia E p/ o DialogueManager quando há diálogo ativo | Não tem Trigger próprio, não decide o que é pergunta/resposta |
| DialogueManager | `Scripts\Sistemas\DialogueManager.cs` | Máquina de estados da conversa; resolve falas (DialogoData → fallback `falaApresentacao`); digitação; encerramento → `NotificarDialogoEncerrado()` | Não desenha nada |
| DialogueUI | `Scripts\UI\Dialogue\DialogueUI.cs` | Camada visual: mostra/esconde a caixa, define nome, texto e indicador | Sem lógica de estados/input |
| DialogoData | `Scripts\Dados\DialogoData.cs` | ScriptableObject com `falas[]` (cada fala tem `nomeFalante` + `textoFala`) + `velDigitacao` | — |

Áreas **fora do escopo** (não alteradas): QuestManager, InventoryManager, SaveManager, AudioManager, GameManager, PlayerController, CameraController, SkateManobras, minijogos e HUD.

## 3. Estrutura de pastas (escopo do diálogo)

```
Assets\Scripts\
├── NPC\NPCBaseNovo.cs          ← script base de NPC
├── Player\PlayerInteraction.cs ← único ouvinte de E
├── Sistemas\DialogueManager.cs ← estados da conversa
├── UI\Dialogue\DialogueUI.cs   ← camada visual (TMP)
├── Dados\DialogoData.cs        ← ScriptableObject de falas (reutilizado)
├── ... demais sistemas/skate/core (não tocar)
Assets\Editor\SetupSistemaDialogo.cs ← ferramenta de setup da cena
Docs\ARQUITETURA_DIALOGO.md
```

## 4. Hierarquia da cena

```
Systems
└── DialogueManager
Canvas (Screen Space Overlay, CanvasScaler 1920×1080, Scale With Screen Size)
└── DialogoUI (DialogueUI)
    └── CaixaDialogo (Image + Outline, inativa no início)
        ├── NpcNome (TMP, topo-esquerda)
        ├── TextoFala (TMP, com quebra de linha)
        └── DicaContinuar (TMP, "Pressione [E] para continuar")
Player (tag "Player")
└── (PlayerController, CameraController ...) + PlayerInteraction
NPCs
└── NPCBaseNovo + Trigger (SphereCollider Is Trigger) (+Colisor físico)
```

Sem EventSystem: o E é lido pelo **Input System**, não por raycast de UI.

## 5. Estado (`DialogueManager`)

| Estado | Significado | E apertado |
|---|---|---|
| NONE | sem conversa | inicia com o NPC mais próximo (via `PlayerInteraction`) |
| TYPING | fala sendo digitada | completa a digitação (não fecha) |
| COMPLETE | fala inteira visível | avança para a próxima ou encerra |
| CLOSING | momento final | ignorado (evita chamada dupla) |

## 6. Fluxo completo

1. Player entra no trigger do NPC → `JogadorEntrouNaArea` → `PlayerInteraction` adiciona à lista.
2. **E** → diálogo ativo? sim → `Avancar()`; não → `NpcElegivelMaisProximo().Interagir()`.
3. `Interagir()` valida (ativo, por perto, diálogo válido, subscriber) → `DialogoSolicitado`.
4. `DialogueManager.IniciarDialogo(npc)`: se NONE e UI válida → resolve falas (`dialogoData.falas` senão `[falaApresentacao]`), `ui.Mostrar()`, `DefinirNome`, digita.
5. E em TYPING → completa; E em COMPLETE → próxima fala ou `EncerrarDialogo()`.
6. `EncerrarDialogo()`: esconde UI, zera estado, chama `npc.NotificarDialogoEncerrado()` → `AoEncerrarDialogo()` → `IniciarMinijogo()` (se `chamarMinijogo` e ainda não tentou).
7. **Sair do trigger durante a conversa**: a conversa **continua** (já aberta); o NPC sai da lista de elegíveis. Depois que E fechar o diálogo, E não faz nada até o player voltar ao trigger. O fechamento é igual ao item 6 — nunca fica caixa órfã.

## 7. Limitações conhecidas (documentadas de propósito)

- **Player com múltiplos Colliders**: `OnTriggerExit` pode falhar em cenários específicos (falso negativo de saída), mantendo o NPC "fantasma" na lista até `LimparInvalidos()` (que roda a cada E e remove inválidos). Mitigação atual: limpeza na pressão de E + `OnDisable` do NPC avisando a saída.
- **Outro diálogo em andamento**: novo `DialogoSolicitado` é **ignorado** (log de aviso) se `estado != NONE`.
- **Dois diálogos abertos/EventSystem**: não se aplica — E é centralizado em um único `PlayerInteraction`.
- `velDigitacao <= 0` exibe a fala instantânea (não trava).

## 8. Configuração da cena (Inspector)

1. Rodar **Tools > IFNMG > Configurar Sistema de Diálogo** (uma vez por cena). Ele cria/vincula `Systems\DialogueManager`, `Canvas` (Overlay 1920×1080) com `DialogoUI` → `CaixaDialogo` + TMPs, e adiciona `PlayerInteraction` ao Player (tag `Player`).
2. **Window > TextMeshPro > Import TMP Essential Resources** (recursos padrão do TMP). Se os textos aparecerem vazios, arrastar um **Font Asset** para `NpcNome`, `TextoFala` e `DicaContinuar`.
3. Nos NPCs: adicionar `NPCBaseNovo`; marcar um Collider como Trigger; preencher `nomeNPC`, e o `dialogoData` ou `falaApresentacao`.
4. No `DialogueUI`: `CaixaDialogo` / `NpcNome` / `TextoFala` / `DicaContinuar` já vêm vinculados pela ferramenta.
5. No `DialogueManager`: a referência `ui` já vem vinculada.

> **Migração da cena Campus.unity:** o NPC "Funcionário da Secretaria" usava o antigo `NPCBase` (removido). Ele ficará com um componente "Missing (Mono Script)" — **remover o componente quebrado** e adicionar `NPCBaseNovo` no objeto. O trigger (SphereCollider raio 2.5) é reaproveitado.

## 9. Checklist de testes

1. Sem Canvas: E perto do NPC não dá erro (guardas logam avisos claros).
2. Com Canvas: aproximar → indicador/nenhuma caixa até apertar **E** (não abre sozinho).
3. E → diálogo abre, digita; E durante digitação **completa** (não fecha); E depois **fecha**.
4. Dois NPCs juntos: E conversa com o **mais próximo**.
5. Sair do trigger com diálogo aberto: conversa continua e fecha normalmente.
6. Depois de fechar, E no vazio: nenhum erro ("Não há NPC próximo").
7. `velDigitacao = 0` → texto instantâneo; 2 falas → E avança e depois fecha; sem `dialogoData` → usa `falaApresentacao`.

## 10. Diagnóstico rápido

| Sintoma | Causa provável |
|---|---|
| E não faz nada (sem log) | `PlayerInteraction` ausente no Player, ou Player sem tag `Player` |
| "nenhum sistema de diálogo está inscrito" | Não há `DialogueManager` na cena (rodar a ferramenta de setup) |
| Diálogo não abre, sem erro específico | Trigger do NPC desmarcado (`Is Trigger`) → warn no Awake do NPC |
| "referência 'ui' vazia" | `DialogueManager.ui` sem atribuição (ferramenta resolve) |
| Textos vazios/vermelhos | Recursos essenciais do TMP não importados ou Font Asset não atribuído |
| Caixa não aparece na tela | Canvas em renderMode errado (usar Screen Space Overlay) |
| ⚠️ "script missing" no NPC da cena | Componente antigo `NPCBase` removido — remover o componente quebrado e adicionar `NPCBaseNovo` |
| Missão/minijogo não roda ao fechar | NPC sem `dialogoData`/`falaApresentacao`, ou `chamarMinijogo` falso |
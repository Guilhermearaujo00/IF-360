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
              │  • lista de alvos IInteragivel       │
              └───────┬──────────────┬───────────────┘
                      │              │ E com diálogo ativo
                      │ E sem diálogo │      │
                      ▼              ▼      ▼
        IInteragivel.Interagir()   DialogueManager.Avancar()
   (NPCBaseNovo / PlacaInformativa)     │
                      │
     NPCBaseNovo.DialogoSolicitado ◄────┘ (evento estático, só NPC)
   PlacaInformativa → IniciarDialogoInformacao(...)
                      │ IniciarDialogo(npc) / IniciarDialogoInformacao(nome, texto)
                      ▼
            ┌── DialogueManager (Systems) ──┐
            │ NONE/TYPING/COMPLETE/CLOSING   │
            │ lê npc.DialogoParaConversa      │
            └──────────────┬─────────────────┘
                           │ Mostrar/DefinirNome/DefinirTexto
                           ▼
                  ┌── DialogueUI (Canvas) ──┐
                  │ CaixaDialogo + 3 textos  │
                  └──────────────────────────┘
                           │
              NPCBaseNovo.NotificarDialogoEncerrado()  (só NPC)
                           │
                     AoEncerrarDialogo() → IniciarMinijogo()
```

Proximidade: cada alvo (`NPCBaseNovo` ou `PlacaInformativa`) possui Trigger próprio e **apenas** avisa o canal estático `Interacao` (`JogadorEntrouNaArea` / `JogadorSaiuDaArea`). Não abre diálogo automaticamente — ver Fluxo abaixo. O `PlayerInteraction` não lê `NPCBaseNovo` diretamente: conversa com os alvos pela interface `IInteragivel`.

## 2. Responsabilidades

| Componente | Arquivo | Responsabilidade | NÃO faz |
|---|---|---|---|
| NPCBaseNovo | `Scripts\NPC\NPCBaseNovo.cs` | Identidade (`nomeNPC`, `idMissao`), dados (`dialogoData` / `falaApresentacao`), proximidade (Trigger), validação de interação, evento `DialogoSolicitado`, gancho `AoEncerrarDialogo()`; implementa `IInteragivel` | Não cria UI, não lê input, não conduz a conversa |
| PlacaInformativa | `Scripts\Interacao\PlacaInformativa.cs` | Placa de prédio/setor sem NPC: Trigger próprio, `titulo` + `texto`; ao apertar E chama `DialogueManager.IniciarDialogoInformacao()` | Não cria UI, não lê input, não tem missão |
| IInteragivel | `Scripts\Interacao\IInteragivel.cs` | Interface de interação (`JogadorPorPerto`, `Transform`, `Interagir()`) — abastra NPCs e placas para o `PlayerInteraction` | — |
| Interacao | `Scripts\Interacao\Interacao.cs` | Canal estático de proximidade (`JogadorEntrouNaArea` / `JogadorSaiuDaArea`) com reset no Domain Reload | Não lê input, não guarda estado |
| PlayerInteraction | `Scripts\Player\PlayerInteraction.cs` | Único ouvinte de E; mantém lista de alvos `IInteragivel` elegíveis; escolhe o **mais próximo**; roteia E p/ o DialogueManager quando há diálogo ativo | Não tem Trigger próprio, não decide o que é pergunta/resposta |
| DialogueManager | `Scripts\Sistemas\DialogueManager.cs` | Máquina de estados da conversa; resolve falas (DialogoData → fallback `falaApresentacao`); digitação; encerramento → `NotificarDialogoEncerrado()` | Não desenha nada |
| DialogueUI | `Scripts\UI\Dialogue\DialogueUI.cs` | Camada visual: mostra/esconde a caixa, define nome, texto e indicador | Sem lógica de estados/input |
| DialogoData | `Scripts\Dados\DialogoData.cs` | ScriptableObject com `falas[]` (cada fala tem `nomeFalante` + `textoFala`) + `velDigitacao` | — |

Áreas **fora do escopo** (não alteradas): QuestManager, InventoryManager, SaveManager, AudioManager, GameManager, PlayerController, CameraController, SkateManobras, minijogos e HUD.

## 3. Estrutura de pastas (escopo do diálogo)

```
Assets\Scripts\
├── NPC\NPCBaseNovo.cs             ← script base de NPC
├── Interacao\IInteragivel.cs      ← interface de interação (NPC/placa)
├── Interacao\Interacao.cs         ← canal estático de proximidade
├── Interacao\PlacaInformativa.cs  ← placas de prédio/setor sem NPC
├── Player\PlayerInteraction.cs    ← único ouvinte de E
├── Sistemas\DialogueManager.cs    ← estados da conversa
├── UI\Dialogue\DialogueUI.cs      ← camada visual (TMP)
├── Dados\DialogoData.cs           ← ScriptableObject de falas (reutilizado)
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
Placas
└── PlacaInformativa + Trigger (Collider Is Trigger) + Mesh com o letreiro
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
4. `DialogueManager.IniciarDialogo(npc)`: se NONE e UI válida → resolve falas (`dialogoData.falas` senão `[falaApresentacao]`), **trava o movimento do jogador (`PlayerController.TravarControle(true)`)**, `ui.Mostrar()`, `DefinirNome`, digita.
5. E em TYPING → completa; E em COMPLETE → próxima fala ou `EncerrarDialogo()`.
6. `EncerrarDialogo()`: esconde UI, zera estado, **destrava o movimento (`TravarControle(false)`)**, chama `npc.NotificarDialogoEncerrado()` → `AoEncerrarDialogo()` → `IniciarMinijogo()` (se `chamarMinijogo` e ainda não tentou).
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
5. No `DialogueManager`: as referências `ui` e `playerController` já vêm vinculadas pela ferramenta. Se o campo `playerController` estiver vazio, o DialogueManager o procura sozinho em cena (cobre sessões sem a ferramenta).

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

## 11. Próximos passos (ideias futuras — NÃO implementadas)

> Itens acordados como evoluções futuras do sistema de diálogo. Quando forem
> feitos, marcar aqui e atualizar as seções correspondentes.

- [ ] **Escolha de resposta (estilo Undertale)**: caixa com 2+ opções; o jogador seleciona
      (teclas) antes de o NPC responder conforme a escolha.
- [ ] **Visual diferente para falas do Caio**: cor/fundo distinto quando o falante é o
      jogador, deixando claro quem está falando (além do nome no topo).
- [ ] **Retratos (portraits)**: imagem do falante na caixa, trocando por linha.
- [x] **Mais de uma conversa por NPC** (pré/pós-missão): implementado no protótipo de
      missões — ver seção 12.
- [x] **Liberar o cursor durante a conversa**: implementado — `CameraController.TravarControle`
      libera o cursor e desliga órbita/zoom enquanto há diálogo (ver seção 13).

## 12. Integração com missões (protótipo)

O NPC muda de conversa conforme o estado da missão (`QuestManager`) e pode completá-la
ao fim da conversa.

**Campos novos no `NPCBaseNovo`:**
- `dialogoDataPosMissao` — conversa exibida quando o requisito estiver cumprido.
- `missaoRequisito` — missão que precisa estar completa para a conversa do "pós".
  Vazio = usa o próprio `idMissao`.
- `completarMissaoAoTerminar` — completa `idMissao` somente na conversa de **entrega**
  (requisito cumprido). Sem requisito, completa na 1ª conversa (pré → pós direto).
- `DialogoParaConversa` — escolhe pré/pós conforme o requisito.
- O `DialogueManager` lê `npc.DialogoParaConversa` (não mais `npc.dialogoData`).

**Fluxo "corrente de missões" (ex.: Secretaria → Diretor → Secretaria):**

| Conversa | Quem | O que acontece ao fechar |
|---|---|---|
| 1ª (Secretaria) | mostra **Pré** | nada (só instrui) |
| Diretor | mostra **fala do Diretor** | `CompletarMissao("missao_diretor")` |
| 2ª (Secretaria) | mostra **Pós** | `CompletarMissao("npc_secretaria")` — missão concluída |
| 3ª (Secretaria) | mostra **Pós** | dispara `IniciarMinijogo()` (1x) |

**Montagem no Inspector:**
- `NPC_Secretaria`: `idMissao` = `npc_secretaria`, `missaoRequisito` = `missao_diretor`,
  `Dialogo Data` = `Dialogo_Secretaria_Pre`, `Dialogo Data Pós-missão` = `Dialogo_Secretaria_Pos`,
  ✓ `Completar Missão ao Terminar`, ✓ `Chamar Minijogo`.
- `NPC_Diretor`: `idMissao` = `missao_diretor`, `Dialogo Data` = `Dialogo_Diretor_Pre`,
  ✓ `Completar Missão ao Terminar`, ✗ `Chamar Minijogo`.

Assets de exemplo: `Assets\Dados\Dialogos\Dialogo_Secretaria_Pre.asset`,
`..._Pos.asset` e `Dialogo_Diretor_Pre.asset`. 2º NPC: menu `Tools/IFNMG/Criar NPC Diretor (teste de missão)`.

## 13. Placas informativas (prédios sem NPC)

Cobre os prédios que só têm placa (sem NPC). A placa **reutiliza o sistema de diálogo**:
ao apertar E, o texto aparece na mesma caixa de diálogo (o `DialogueManager` entra em
modo conversa e trava movimento/câmera).

- `PlacaInformativa` implementa `IInteragivel` e avisa proximidade pelo canal `Interacao`
  — `PlayerInteraction` continua sendo o **único** ouvinte de E (a placa não lê input).
- `DialogueManager.IniciarDialogoInformacao(titulo, texto)` mostra uma fala avulsa
  (`npcEmConversa = null`): ao encerrar, nenhum NPC é notificado.
- Montagem: adicionar `PlacaInformativa` num objeto com um **Collider Is Trigger**;
  preencher `titulo` e `texto`. O `raio` do trigger define a área de interação.

## 14. Persistência de missões e ciclo de vida

- `QuestManager` agora é `DontDestroyOnLoad` e fica no **mesmo objeto do `GameManager`**
  (que também é persistente) — o estado sobrevive à troca de cena junto da fachada.
- `SaveManager` persiste `missoesCompletas` (JSON via `QuestManager.SerializarMissoes`
  / `RestaurarMissoes`) junto de pontos e colecionáveis; `ApagarJogo` limpa tudo.
- `GameManager.CompletarMissao` faz **auto-save** (`SaveManager.SalvarJogo()`), para o
  progresso não se perder ao sair.
- Observação: ainda não há tela de menu chamando `CarregarJogo()`/`ApagarJogo()`; quando
  existir, basta chamar pelo `SaveManager`.

## 15. Travar câmera/cursor no diálogo

- `CameraController.TravarControle(bool)`: enquanto travado, desliga órbita/zoom
  (input de `Look`/`Zoom`) e libera o cursor; ao destravar, volta a travar o cursor.
- O `DialogueManager` chama `PlayerController.TravarControle` **e**
  `CameraController.TravarControle` ao abrir/encerrar (procura automaticamente se o
  campo `cameraController` estiver vazio), então movimento e câmera ficam travados juntos.

## 16. Integrações complementares

- **Áudio**: o `DialogueManager` toca `AudioManager.TocarDigitar` (ao iniciar cada fala),
  `TocarConfirmar` (a cada E válido) e `TocarFechar` (ao encerrar). `Coletavel` toca
  `TocarConfirmar` ao pegar um item. Atribua os `AudioClips` no `AudioManager`.
- **Inventário**: `Coletavel` registra o item em `InventoryManager.AdicionarItem(nomeColecionavel)`.
- **HUD de missão** (`HUDMissao`, `Assets\Scripts\UI\HUD`): mostra o contador de missões
  concluídas e o nome da última missão concluída. Reage ao evento estático
  `QuestManager.MissaoCompletada`; preencha a lista "Nomes de Missões" (id → nome amigável).
  Criar via **Tools > IFNMG > Criar HUD de Missão**.
- **Ferramenta de setup**: `Tools > IFNMG > Configurar Sistema de Diálogo` agora também
  garante `SaveManager`, `AudioManager` (+`AudioSource`) e `InventoryManager` no objeto
  `Systems` (idempotente).
- **Build Settings**: `Menu` (índice 0) e `Campus` (índice 1) adicionados ao
  `ProjectSettings/EditorBuildSettings.asset`.
- **Namespaces**: todo o código de runtime está em `namespace IF360`. O
  `PlayerInputActions` gerado (`Assets\Input`) permanece no namespace global. As ferramentas
  de Editor referenciam o runtime via `using IF360;`. Ainda **não** há `asmdef` (opcional).

> **Limitação do HUD**: mostrar o **objetivo ativo** (texto da missão corrente) exige um
> modelo de missões com etapas (`MissaoData` com `etapas[]`), ainda não implementado —
> ver discussão de opção "B" para a missão Biblioteca.

## 17. Minijogo "Ligar os 3 fios"

Vertical slice de minijogo, acionado pelo mesmo gancho de NPC que antes só logava.

- **Gancho**: `NPCBaseNovo.IniciarMinijogo()` dispara o evento estático
  `NPCBaseNovo.MinijogoSolicitado`. Um NPC com `Chamar Minijogo` ativo o aciona após a
  conversa (ex.: `NPC_Secretaria`).
- **Componente**: `Assets\Scripts\Minijogos\MinijogoFios.cs` (namespace `IF360`), em
  `Systems`. Assina o evento e abre o desafio.
- **UI**: construída em **runtime** (reaproveita o `Canvas` da cena), sem prefab nem tool
  obrigatória — 3 fios coloridos × 3 tomadas embaralhadas.
- **Controles**: `←/→` ou `A/D` escolhem o fio; `1/2/3` ligam à tomada; `Esc` cancela.
  Lê o teclado via `Keyboard.current` (não mexe no `PlayerInputActions`).
- **Regras**: acertar a cor trava a ligação; as 3 certas → vitória e `+premioPontos` via
  `GameManager.AdicionarPontos`. Enquanto aberto, trava movimento/câmera
  (`TravarControle`) e o `PlayerInteraction` ignora o `E` (`MinijogoFios.Ativo`).
- **Setup**: **Tools > IFNMG > Adicionar Minijogo 'Ligar Fios'** (ou o setup principal, que
  já o garante em `Systems`). Testar com um NPC marcado com **Chamar Minijogo**.
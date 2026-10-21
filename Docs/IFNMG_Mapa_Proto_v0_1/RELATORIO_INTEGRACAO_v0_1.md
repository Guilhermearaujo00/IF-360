# Relatório de integração — mapa protótipo v0.1

- Data: 2026-10-20
- Pacote: `IFNMG_Mapa_Proto_v0_1.zip`
- Prompt seguido: `Docs/IFNMG_Mapa_Proto_v0_1/Prompt_OpenCode_Integracao.md`
- Modo: trabalho por JSON/coordenadas/caminhos/bounds/logs. **Nenhuma imagem foi interpretada.**
- A cena principal (`Assets/Scenes/Campus.unity`) **não foi alterada** (mtime 2026-10-20 21:16, anterior à execução).

## 1. Arquivos instalados

| Arquivo | Caminho no projeto | Status |
|---|---|---|
| JSON de dados | `Assets/IFNMG360/Config/IFNMG_Mapa_Proto_v0_1.json` | copiado |
| Gerador | `Assets/Editor/IFNMGPrototypeMapGenerator.cs` | copiado |
| README | `Docs/IFNMG_Mapa_Proto_v0_1/README.md` | copiado |
| Prompt | `Docs/IFNMG_Mapa_Proto_v0_1/Prompt_OpenCode_Integracao.md` | copiado |
| Planta (conferência humana) | `Docs/IFNMG_Mapa_Proto_v0_1/IFNMG_Mapa_Proto_v0_1_planta.png` | copiado |

## 2. Compilação

- Compilado isolado com o Roslyn do Unity 6000.4.0f1: **0 erros de API/versão**.
- O JSON espacial **não foi alterado** (nenhum `pointsUvFlat`, `widthMeters`, `buildingHeightMeters` ou `enabled`).

### Correção necessária (somente código, item 1 do prompt)

O método original abria a cena do protótipo de forma **aditiva**, o que falha em `-batchmode`:

```
InvalidOperationException: Cannot create a new scene additively with an untitled scene unsaved.
  at IFNMGPrototypeMapGenerator.GenerateSeparatePrototypeScene () [IFNMGPrototypeMapGenerator.cs:120]
```

Correção mínima aplicada: em `Application.isBatchMode`, abrir/criar a cena do protótipo como **Single**
(e suprimir `EditorUtility.DisplayDialog`). No Editor interativo o comportamento original (aditivo + diálogos)
permanece. **A lógica de geração e o JSON não mudaram.**

## 3. Prefabs candidatos (item 2 do prompt)

Todos os **22** caminhos do `assetGroups` **existem** no projeto. **Nenhum fallback foi necessário.**

- pavement_candidate (5): Road Concrete Tile, Road Concrete Tile Small, Road Sidewalk, WalkPavingPlain_01, WalkCornerExternal_01 — OK
- tree (4): TreeBroadleafMature/Spreading/Young, SM_Tree_Green — OK
- bush (3): BushRoundMedium, HedgeRunLow, SM_FoliageBush_01 — OK
- flower (2): FlowerBedRound, SM_Flower_01 — OK
- grassDetail (2): GrassTuftClump, SM_Grass_01 — OK
- rock (2): HardRock_01, SoftRock_01 — OK
- mushroom (1): SM_Mushroom_01 — OK
- urban_prop (3): Bench/Lamp/StreetBin — OK (não instanciados na v0.1; falta âncora)

## 4. Geração (item 4 do prompt)

- Cena separada criada: `Assets/IFNMG360/Scenes/IFNMG_Mapa_Prototype_v0_1.unity`
- Raiz: `IFNMG_Mapa_Prototype_v0_1` (posição/rotação zero)
- Grupos: `Terrenos`, `Vias`, `Estruturas`, `Vegetacao`, `Detalhes_Naturais`
- Plano-base: `TerrenoBase_440x310m`

### Contagem do log do Console

```
zonas=38, footprints de prédios=15, árvores=58, arbustos=16,
detalhes de grama=32, rochas=6, fungos=12
```

### Contagem do arquivo de cena (YAML)

- GameObjects: **74** · MeshRenderer: **53** · MeshCollider: **37**
- Edifícios: **15** (cada um com `Walls` + `Roof_<estilo>`)
- Estilos de cobertura: laranja / cinza / azul (conforme JSON)

## 5. Avisos registrados (itens 4 e 9 do prompt)

### Zonas desativadas pela regra de correção
- `pavimento_estacionamento_leste` — desativada (sobrepõe `edificio_ead_cobertura_azul`). **Preservada.**

### Zonas de confiança baixa/baixa-média (revisar depois) — 12
`gramado_centro_oeste`, `gramado_leste_centro`, `arvores_alinhadas_centro`, `arvores_alinhadas_sudeste`,
`solo_sul_leste`, `solo_oeste_pequeno`, `edificio_noroeste_bloco_3`, `edificio_central_2`,
`edificio_pequeno_leste`, `via_sob_copas_oeste_sudoeste`.

### Bounds de edificações que se sobrepõem ou se aproximam — 5 pares
- `edificio_biblioteca_conjunto_1` / `edificio_biblioteca_conjunto_2`
- `edificio_biblioteca_conjunto_2` / `edificio_noroeste_bloco_3`
- `edificio_norte_leste_1` / `edificio_norte_leste_2`
- `edificio_central_2` / `edificio_central_leste_3`
- `edificios_sudeste_linha_1` / `edificios_sudeste_linha_2`

### Edificação cruzando pavimento — 1
- `edificio_norte_leste_2` / `pavimento_estacionamento_norte` (edifício tem prioridade visual)

> Esses avisos são esperados em blockout preliminar (footprints aproximados). Precisam de revisão humana.

## 6. Via provisória

- `via_sob_copas_oeste_sudoeste` — **mantida** (confiança `low`), com largura 4,8 m. Preservada conforme item 8.

## 7. Escala (item 7 do prompt)

- Dimensões provisórias: **440 × 310 m**, 1 unidade = 1 m, norte = +Z. **Status: PROTOTIPO_NAO_CALIBRADO.**
- A medição dos Jogos Analógicos (área 1.480,14 m², perímetro 154,80 m) **não** foi usada como calibração —
  falta o polígono numérico correspondente ao mesmo contorno.
- **Nenhuma alegação de calibração** é feita aqui.

## 8. O que NÃO foi feito

- Não declarei correção visual da composição (não há leitura de imagem neste fluxo).
- Não movi, substituí ou apaguei nada da cena principal.
- Não alterei o JSON.
- Não usei a escala antiga (0,516 m/px) nem o mapa antigo.

## 9. Próximo passo (aguarda você)

O protótipo v0.1 está gerado na cena separada. Para conferir:
- Abra `Assets/IFNMG360/Scenes/IFNMG_Mapa_Prototype_v0_1.unity` (cena isolada).
- Compare com a planta `Docs/IFNMG_Mapa_Proto_v0_1/IFNMG_Mapa_Proto_v0_1_planta.png`.

Para levar qualquer objeto à cena principal, primeiro eu gero a **lista de correções propostas** e **aguardo sua autorização** (item 9 do prompt). Isso ainda não foi feito.

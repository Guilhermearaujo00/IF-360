# Calibração de escala — mapa protótipo IFNMG 360° v0.1

- Data: 2026-10-21
- Fonte: `Assets/IFNMG360/Config/IFNMG_Mapa_Proto_v0_1.json`
- Método: **dados numéricos** (pixels, bounds da cena real, medida real de prédio). **Sem interpretação de imagem.**

## 1. Resultado

- **Escala calibrada: 0,8466 m/px** (na imagem-base 784 × 542 px)
- **Mundo: 499,6 × 364,5 m**
- **1 unidade Unity = 1 m**
- **Norte = topo do mapa = +Z** (confirmado pelo usuário)

## 2. Duas âncoras independentes (concordaram)

| Âncora | Fonte | Escala |
|---|---|---|
| **1 — Perímetro vs cena real** | Perímetro do mapa (590,1 × 430,6 px) = bounds da cena `Campus` (512,1 × 355,6 m) | **0,8468 m/px** |
| **2 — Jogos Analógicos vs bbox** | Prédio medido (39,31 × 36,53 m) = bbox 52 × 39 px da zona `edificio_noroeste_bloco_3` | **0,8463 m/px** |

- Concordância entre as âncoras: **0,06%** → validação forte e independente.
- Média adotada: **0,8466 m/px**.

## 3. De onde vem cada dado

- **Sua medição real (Jogos Analógicos):** área 1.480,14 m² · perímetro 154,80 m · frontal 39,31 m · lateral 36,53 m.
- **Bounds da cena Campus** (medidos por `InspecionarTerreno`, somente leitura): X[-256,45 … 255,60], Z[-185,62 … 169,98] → **512,1 × 355,6 m**.
- **Perímetro do mapa novo** (cantos do JSON): topo 582 px, base 598,3 px, esq. 422,1 px, dir. 439,1 px → médias **590,1 × 430,6 px**.
- **Zona do Jogos Analógicos:** confirmada pelo usuário como `edificio_noroeste_bloco_3` (noroeste, telhado laranja).

## 4. Antes x depois

| Campo | Antes (provisório) | Depois (calibrado) |
|---|---|---|
| `status` | `PROTOTIPO_NAO_CALIBRADO` | `PROTOTIPO_CALIBRADO_PRELIMINAR` |
| `worldWidthMeters` | 440,0 | **499,6** |
| `worldDepthMeters` | 310,0 | **364,5** |
| `metersPerPixel` | (ausente) | **0,8466** |

## 5. Consistência com outras referências

| Referência | Valor | Avaliação |
|---|---|---|
| Sua estimativa de planejamento | ~500 × 330 m | **muito próxima** em X; Z ~10% maior |
| Cena real (bounds medidos) | 512,1 × 355,6 m | **muito próxima** |
| E04 medido (39,3 × 36,5 m) at 0,8466 | ≈ 44 × 33 m | dentro da incerteza de tracing (±10–15 px) |
| Mapa **antigo** (0,516 m/px) → E04 = 52 m | 52 m | **descartado** (obsoleto; 25% acima da sua medida) |

## 6. Ressalvas

- A medição de contorno do mapa tem incerteza de tracing **±10–15 px** (≈ ±8–13 m). A calibração é **preliminar**.
- A cena `Campus` (origem dos 512×356 m) foi gerada do **mapa antigo** — usada aqui apenas como *referência de extensão*, não como verdade de posição.
- As âncoras concordarem não garante que cada footprint individual esteja certo; a **posição de cada zona** ainda deve ser revisada no terreno.

## 7. Mudanças aplicadas

- JSON `world` atualizado (escala calibrada + bloco `scaleCalibration`).
- Cena do protótipo **regenerada** com a nova escala: `Assets/IFNMG360/Scenes/IFNMG_Mapa_Prototype_v0_1.unity` (plano-base `TerrenoBase_500x364m`).
- **Cena principal `Campus.unity` não alterada.**

# Auditoria do terreno — cena Campus (somente leitura)

- Data: 2026-10-20 23:56
- Cena: `Assets/Scenes/Campus.unity`
- Nenhum objeto foi criado/movido/removido; a cena não foi salva.
- Escala de trabalho: 1 unidade Unity = 1 m (provisório, não confirmado).

## 1. Renderers

- Renderers totais: **1827**
- Renderers candidatos a piso (baixos/grandes, Y ≤ 0,3 m): **851**

## 2. Limites medidos (bounds)

| Conjunto | min X | max X | min Y | max Y | min Z | max Z | X×Y×Z (m) |
|---|---|---|---|---|---|---|---|
| Tudo | -256,45 | 255,60 | -1,26 | 17,89 | -185,62 | 169,98 | 512,1 × 19,1 × 355,6 |
| Piso | -256,45 | 255,60 | -1,26 | 17,89 | -185,62 | 169,98 | 512,1 × 19,1 × 355,6 |

**Dimensão do piso: 512,1 × 355,6 m** (comparar com a estimativa 500 × 330).

## 3. Extremos do piso (renderer mais a oeste/leste/norte/sul)

| Direção | X | Z | Renderer |
|---|---|---|---|
| minX (oeste) | -252,30 | 134,80 | R07_0 |
| maxX (leste) | 250,49 | -1,15 | S_Tree_C |
| minZ (sul) | 189,51 | -174,70 | E10_0_3 |
| maxZ (norte) | -89,14 | 165,67 | R06_0 |

## 4. Cantos do bounding box do piso

| Canto | X | Z | Y medido |
|---|---|---|---|
| NW (minX, maxZ) | -256,45 | 169,98 | 0,00 |
| NE (maxX, maxZ) | 255,60 | 169,98 | 0,00 |
| SW (minX, minZ) | -256,45 | -185,62 | 0,00 |
| SE (maxX, minZ) | 255,60 | -185,62 | 0,00 |

## 5. Cobertura por amostragem (grid 2 m)

- Pontos: 26623/45746 (58,2%).
- Estimativa por bounds, **não** prova de vão real.

Primeiros pontos sem piso próximo:

(-256,5, -185,6), (-256,5, -183,6), (-256,5, -181,6), (-256,5, -179,6), (-256,5, -177,6), (-256,5, -175,6), (-256,5, -173,6), (-256,5, -171,6), (-256,5, -169,6), (-256,5, -167,6), (-256,5, -165,6), (-256,5, -163,6), (-256,5, -161,6), (-256,5, -159,6), (-256,5, -157,6), (-256,5, -155,6), (-256,5, -153,6), (-256,5, -151,6), (-256,5, -149,6), (-256,5, -147,6), (-256,5, -145,6), (-256,5, -143,6), (-256,5, -141,6), (-256,5, -139,6), (-256,5, -137,6), (-256,5, -135,6), (-256,5, -133,6), (-256,5, -131,6), (-256,5, -129,6), (-256,5, -127,6), (-256,5, -125,6), (-256,5, -123,6), (-256,5, -121,6), (-256,5, -119,6), (-256,5, -117,6), (-256,5, -115,6), (-256,5, -113,6), (-256,5, -111,6), (-256,5, -109,6), (-256,5, -107,6)

## 6. Candidato a canto Noroeste (superior-esquerdo do mapa)

Convenção do mapa de referência: `u` cresce para a direita (leste), `v` cresce para baixo (sul).
Assumindo que o topo do mapa = norte = **+Z** e a esquerda = oeste = **-X** (a confirmar):

- **Canto NW provável = (minX, maxZ) = (-256,45, 169,98)**
- Canto NE provável = (255,60, 169,98)
- Canto SW provável = (-256,45, -185,62)
- Canto SE provável = (255,60, -185,62)

> **ATENÇÃO:** a correspondência norte↔+Z e oeste↔-X é uma suposição, não um dado.
> Sem um marco real confirmado (medida de um edifício ou distância entre dois marcos),
> NÃO gerar objetos. Este documento apenas registra a geometria existente.

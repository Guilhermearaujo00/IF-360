# Achados do terreno — Campus (para ancoragem do mapa de referência)

- Data: 2026-10-20
- Fonte: `Assets/Scenes/Campus.unity` (somente leitura)
- Relatórios: `AUDITORIA_TERRENO_CAMPUS.md`, `auditoria_terreno.json`, `auditoria_assets.json`
- **Nada foi criado, movido, redimensionado ou salvo na cena.**
- Escala de trabalho: **1 unidade Unity = 1 m** (provisório — não confirmado por medida real).

## 1. Dimensões do terreno existente (medido)

| Conjunto | X (m) | Z (m) | Y (m) | Dimensão XZ |
|---|---|---|---|---|
| Tudo (renderers) | -256,45 … 255,60 | -185,62 … 169,98 | -1,26 … 17,89 | **512,1 × 355,6** |
| Piso/candidatos | -256,45 … 255,60 | -185,62 … 169,98 | -1,26 … 17,89 | **512,1 × 355,6** |

- Sua estimativa de planejamento (**~500 × 330**) é **próxima**, mas o existente mede **512,1 × 355,6** —
  ~2,4% maior em X e ~7,8% maior em Z. A diferença em Z é relevante; **não redimensionar a cena**.
- Y do conjunto vai de -1,26 a 17,89 m (isso é a extensão de vegetação/prédios, não a cota do piso).
- Componentes `Terrain`: **0** (o "terreno" é composto por módulos/malhas, não por Terrain component).

> A bbox de "piso" saiu idêntica à de "tudo" porque o filtro atual (Y ≤ 0,3) não separou bem árvores/prédios.
> Os extremos de X/Z acima continuam válidos como **limites do mundo**. Ajuste do filtro fica registrado como
> melhoria, sem impacto nesta análise.

## 2. Cantos do terreno (convenção a confirmar)

Convenção do mapa de referência: `u` → direita (leste), `v` → baixo (sul).
Suposição (não confirmada): **topo do mapa = norte = +Z** e **esquerda = oeste = -X**.

| Canto | Coordenada mundo (X, Z) |
|---|---|
| **NW (superior-esquerdo) — candidato** | **(-256,45, +169,98)** |
| NE (superior-direito) | (+255,60, +169,98) |
| SW (inferior-esquerdo) | (-256,45, -185,62) |
| SE (inferior-direito) | (+255,60, -185,62) |

O JSON de zonas usa `projective_normalized_uv` com cantos do perímetro branco e a fórmula
`worldX = (u-0.5)*worldWidth; worldZ = (0.5-v)*worldDepth`. Isso **casa** com a convenção acima (u→+X, v→-Z)
**se** o topo do mapa for +Z. A rotação/origem exata ainda depende de confirmação humana (§5).

## 3. O que existe em cada canto (estruturas mais próximas)

Estruturas conhecidas (centro X,Z) ordenadas; distância ao canto correspondente:

| Canto | Estrutura mais próxima | Centro (X, Z) | Dist. ao canto (m) |
|---|---|---|---|
| NW (-256,45, 169,98) | `E15a_0_0` (23,1 × 7,6 × 18,7) | (-100,4, 129,1) | ≈ 161 |
| NE (255,60, 169,98) | `E18_0` (35,7 × 13,4 × 34,8) | (92,4, 150,2) | ≈ 164 |
| SW (-256,45, -185,62) | `E07_0_00` (32,0 × 13,4 × 32,0) | (-73,3, -69,8) | ≈ 215 |
| SE (255,60, -185,62) | `E11_0` (45,5 × 17,9 × 47,0) | (187,8, -141,4) | ≈ 82 |

- **Nenhum edifício ocupa exatamente um canto** — os cantos estão em vegetação/piso. Isso é esperado
  (o mapa antigo não tinha, e o novo JSON descreve o perímetro do terreno, não de uma construção).
- Não há objeto-raiz único que represente "o perímetro/cerca". O limite é a extensão das malhas de piso.

## 4. Marcos candidatos para medida real de escala

Edifícios grandes e isolados (bons para medir no terreno e comparar com o mapa):

| Id | Dimensão X×Y×Z (m) | Centro (X, Z) | Nota |
|---|---|---|---|
| `E11_0` | 45,5 × 17,9 × 47,0 | (187,8, -141,4) | maior volume; bom marco SE |
| `E08_0` | 60,3 × 10,0 × 51,1 | (127,5, -57,8) | segunda maior pegada |
| `E18_0` | 35,7 × 13,4 × 34,8 | (92,4, 150,2) | isolado ao norte; bom marco NE |
| `E07_0_*` | 32,0 × 13,4 × 32,0 (4 módulos) | ≈ (-55, -58) | conjunto a sudoeste |
| `E13_0_*` | 34,5 × 7,9 × 27,0 | ≈ (-142, -10) | a oeste |

> Estas medidas são dos **objetos atuais na cena** (herdados do mapa antigo, status "obsoleto"). Servem como
> *candidatos de marco*, mas **não** como verdade: o mapa antigo não é confiável (ver §6).

## 5. O que falta para ancorar e gerar (bloqueio)

1. **Confirmação do canto NW:** qual canto do terreno corresponde ao canto superior-esquerdo do mapa novo.
   Candidato medido: **(-256,45, +169,98)** — depende de a cena estar "norte-para-cima" em +Z.
2. **Medida real de 1 marco** (edifício ou distância entre dois marcos) para calcular a escala do mapa.
   Sugestão de medição: comprimento de `E08_0` (60,3 m) ou `E11_0` (45,5 m) **no terreno real**, ou a
   distância entre dois edifícios reconhecíveis.
3. **Eixo norte:** confirmar se +Z = norte (ou informar o norte real).

## 6. Ressalva sobre o mapa antigo

O `Mapa_IFNMG` na cena foi gerado do **mapa antigo** (0,516 m/px), declarado obsoleto. Portanto:
- As posições E01…E19 acima **não** devem ser usadas como referência de verdade do campus.
- Elas servem apenas como (a) inventário de escala relativa e (b) candidatos a marco físico.
- A escala nova **não** deriva de 0,516 m/px.

## 7. Conclusão

- Terreno existente **preservado** e medido: **512,1 × 355,6 m**, 0 Terrain, piso em Y≈0.
- Canto NW candidato: **(-256,45, +169,98)** — registrado, **não aplicado**.
- **Nada foi gerado/posicionado/redimensionado.** O gerador permanece bloqueado até você confirmar:
  - o canto NW, e
  - uma **medida real** de edifício/distância entre marcos.

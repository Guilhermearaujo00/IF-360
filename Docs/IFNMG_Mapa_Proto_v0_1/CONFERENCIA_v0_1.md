# Conferência do protótipo v0.1 (por dados)

- Data: 2026-10-21
- Método: leitura do JSON + da cena gerada (`IFNMG_Mapa_Prototype_v0_1.unity`) + conversão calibrada.
  **Nenhuma imagem interpretada.** Placeholders de prédio não são avaliados quanto a forma (ver relatório de integração).
- Escala usada: **0,8466 m/px → 499,6 × 364,5 m**; norte = topo (+Z).

## 1. Contagens: JSON vs cena (bateram)

| Item | JSON pede | Cena tem |
|---|---|---|
| Zonas totais | 38 | 38 |
| Footprints de prédio | 15 | 15 |
| Árvores | 58 | 58 |
| Arbustos | 16 | 16 |
| Detalhes de grama | 32 | 32 |
| Rochas | 6 | 6 |
| Fungos | 12 | 12 |
| Vias (line) | 6 | 6 |
| Superfícies (grass/soil/trees base) | 15 | 15 |

Sem divergência de contagem.

## 2. Regras de correção preservadas

- `pavimento_estacionamento_leste` (`enabled=false`): **ausente** da cena ✅
- `pavimento_estacionamento_norte`: presente ✅
- `via_sob_copas_oeste_sudoeste`: presente ✅

## 3. Conferência espacial (nome ↔ posição)

Centro de cada zona em mundo (+Z = norte). O padrão **confirma que a orientação e a escala estão coerentes** —
não há inversão de norte nem deslocamento grosseiro:

| Zona | Nome implica | Posição real | Veredito |
|---|---|---|---|
| `gramado_nordeste` | NE | X=156, Z=72 | ✅ |
| `massa_arborizada_oeste` | O | X=-197, Z=-27 | ✅ oeste |
| `massa_arborizada_sudoeste` | SO | X=-176, Z=-119 | ✅ |
| `arvores_noroeste` | NO | X=-206, Z=135 | ✅ |
| `arvores_centro_oeste` | O | X=-199, Z=44 | ✅ |
| `solo_caminho_leste` | L | X=199, Z=97 | ✅ |
| `solo_sul_leste` | SE | X=218, Z=-157 | ✅ |
| `edificio_noroeste_bloco_3` | NO | X=-114, Z=92 | ✅ |
| `edificio_sudeste_leste` | SE | X=206, Z=-122 | ✅ |
| `edificio_longo_oeste` | O | X=-188, Z=16 | ✅ |
| `edificio_ead_cobertura_azul` | L | X=122, Z=-53 | ✅ |
| `pavimento_estacionamento_norte` | N | X=10, Z=133 | ✅ |
| `gramado_sul_centro` | S | X=-27, Z=-129 | ✅ |

> Nenhuma zona caiu em quadrante **oposto** ao que o nome indica. A convenção de orientação está consistente.

## 4. Observações (não erros, apenas pontos de atenção)

- **Distribuição espacial:** a vegetação densa concentra-se no **oeste/sudoeste** (`massa_arborizada_*`,
  `arvores_*`), e os gramados no centro/sul/leste — coerente com o mapa de referência (mata densa a oeste).
- **Prédios (placeholders):** 15 volumes simplificados marcando ocupação. Forma/altura/cor não avaliadas.
  Os avisos de bounds sobrepostos entre prédios são **irrelevantes** nesta fase (serão prédios reais depois).
- **Vias:** 6 ribbons procedurais, larguras 3,8–6,0 m. Continuidade visual precisa de conferência humana.
- A amostragem de vegetação foi determinística (seed 3602026) com rejeição em prédios/vias — sem árvores
  dentro de footprints.

## 5. O que a conferência NÃO pode dizer

- Se a composição está **visualmente** correta (exige olho humano; sem leitura de imagem).
- Se cada footprint individual está no lugar certo do terreno real (incerteza de tracing ±10–15 px).
- Se as vias seguem o traçado real (traçado preliminar).

## 6. Veredito

- **Contagens, regras de correção, escala e orientação: OK.**
- Pronto para **conferência visual humana** na cena isolada contra a planta PNG.
- Ajustes de posição fina de zonas específicas devem ser feitos no JSON (`pointsUvFlat`) e regenerados.

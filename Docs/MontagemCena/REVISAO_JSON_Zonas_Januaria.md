# Revisão crítica — `IFNMG_Januaria_zonas_para_Unity.json` (v1)

> **Objetivo desta revisão:** decidir se o JSON pode ser usado para gerar geometria no campus **antes** de
> executar qualquer gerador. Regra do prompt v3: nenhuma classificação antiga deve ser reproduzida na Unity e
> uma escala incorreta não pode deslocar os assets.
>
> **Veredito: NÃO liberado para geração direta.** Pode ser usado como **fonte de zoneamento** (o que é grama,
> prédio, via, árvore, solo) e para uma **zona-piloto controlada**, mas a **transformação `u,v → mundo` continua
> indefinida** e precisa ser resolvida por 1 medida real no terreno. Detalhes abaixo.

- Data: 2026-10-20
- Arquivo revisado: `Docs/MontagemCena/IFNMG_Januaria_zonas_para_Unity.json`
- Origem: captura de tela `2026-10-20 144000.png`, 784 × 542 px (classificação **visual**, não GIS)
- Revisado por: OpenCode (leitura de dados, sem interpretar imagem)
- Comparado com: `Assets/Dados/MapaIFNMG/mapa_ifnmg.json` (mapa antigo, base **973 × 632**, `metersPerPixel 0.516`)

---

## 1. Inventário objetivo (contado do arquivo, não estimado)

| Classe | Zonas | Confiança |
|---|---|---|
| `building` | 15 | 6 medium · 4 medium-high · 3 low-medium · 2 high |
| `paved` | 7 | 6 medium · 1 medium-high |
| `grass` | 6 | 4 medium · 2 low-medium |
| `trees` | 6 | 2 high · 2 medium · 2 low-medium |
| `soil` | 3 | 1 medium · 1 low-medium · 1 low |
| **Total** | **37** | 5 high/medium-high · 19 medium · 13 abaixo de medium |

- Elementos do 2º conjunto (fora do topo esperado): 23 são `polygon`, 8 são `line` (vias/ramais), 25 têm
  `points_uv`. Todas as zonas também trazem `image_points_px`.
- Campo `recommended_generation_order`: `paved → building → grass → trees → soil`.
- `overlap_policy`: não gerar vegetação dentro de prédios/pavimento; usar buffer; permitir revisão manual.

## 2. O que o JSON declara por conta própria (e está correto declarar)

1. **Escala física NÃO deduzida** — o próprio arquivo avisa em `coordinate_system.warning` e `confidence_note`.
2. **Sistema de coordenadas = UV projetivo normalizado** (não metros). `worldX = (u-0.5)*worldWidth;
   worldZ = (0.5-v)*worldDepth`, **com `worldWidth`/`worldDepth` a definir**.
3. **Não é levantamento GIS/topográfico** e a precisão é limitada por rótulos, ícones, sombras e telhados.

## 3. Incompatibilidade com o mapa antigo (impede reaproveitar a calibração)

| Item | Mapa antigo | JSON novo | Consequência |
|---|---|---|---|
| Imagem-base | 973 × 632 px | 784 × 542 px | **diferentes** — px não são transferíveis |
| Calibração | `metersPerPixel 0.516`, `originPx (486,316)` | ausente | não dá para herdar a escala antiga |
| Geometria das vias | `R01–R08` em px do mapa antigo | 8 `line` em px/UV do mapa novo | não comparáveis |

**Conclusão:** os dados antigos (`metersPerPixel`, `originPx`, `R/T/E/V`) permanecem **descartados**, como já
registrado no Plano Espacial. O JSON novo **não os substitui em metros** — ele só traz **zonas em imagem**.

## 4. Checagem de escala/ofset/rotação (o risco citado por você)

Dois testes objetivos, ambos **não conclusivos**:

- **Aspecto do perímetro novo:** largura média 590,1 px × altura média 430,6 px → **1,371**.
- **Aspecto dos bounds da cena `Campus`:** ~512 × 356 m → **1,438**.
- Se forçássemos perímetro novo = bounds da cena, sairia **X ≈ 0,868 m/px** e **Z ≈ 0,827 m/px**
  (diferentes entre si e sem garantia de estarem certos).
- **Não há rotação/origem declarada**; o próprio arquivo diz “ajustar à cena real”.

**Portanto:** não é seguro deduzir `worldWidth/worldDepth`, origem nem rotação a partir da captura. Aplicar
qualquer valor arbitrário **deslocaria todos os assets** — exatamente o que quer evitar. Isto reprova o
requisito “escala confirmada por medida conhecida”.

## 5. Ambiguidades de classe (impedem gerar direto)

| Tema | Observação registrada no próprio JSON | Ação exigida antes de gerar |
|---|---|---|
| `grass` vs vegetação baixa | `gramado_nordeste` "confirmar se é gramado manejado ou vegetação baixa" | confirmar tratamento (grama cortada x vegetação) |
| `soil` (`solo_caminho_leste`, `solo_sul_leste`) | "pode combinar terra batida, talude e margem de via" | decidir se é solo, talude ou via |
| `trees` (`massa_arborizada_*`, `arvores_*`) | massas, não árvores individuais; posições aproximadas | gerar como **massa** com densidade, não como posições únicas |
| `paved` cantos | 8 segmentos `line` sem `width` em metros (só fração da imagem) | converter largura para metros **após** fixar escala |
| `building` low/medium | vários footprints "aproximados"/encobertos | tratar como caixa aproximada de exclusão, não footprint exato |

## 6. Requisitos para liberar a geração (checklist mínimo)

1. **Escala real (1 medida):** um par de pontos com distância conhecida no terreno (ex.: comprimento de um
   prédio, distância entre dois blocos, lado de uma quadra). Sem isso, `worldWidth/worldDepth` fica indefinido.
2. **Ancoragem:** qual objeto real da cena `Campus` corresponde a um canto do perímetro (ou ao centro), para
   definir origem e rotação. (A cena tem `Plane` 20×20 em (0,0,0) e NPCs perto da origem, mas nada mapeado ao
   perímetro branco da captura.)
3. **Norte:** se o mapa é georreferenciado, dizer qual eixo é o norte (o JSON não informa).
4. **Confirmação das 15 edificações** como “prédios” e das 8 linhas como “vias” (o usuário já confirmou
   áreas azuis = prédios e traçado vermelho = via; falta o de-para visual↔ID do JSON).
5. **Decisão de precisão:** aceitar aproximação (zonas de exclusão/inclusão) ou exigir footprint exato.

## 7. O que PODE ser feito já (sem violar o v3)

- **Zona-piloto controlada**, em 1 área **bem delimitada e de alta confiança**, gerada **fora do campus** (cena
  sandbox) ou em área neutra, apenas para: (a) medir escala real com você; (b) validar encaixe; (c) revisar
  estilo. Candidatas de maior confiança no JSON: `edificio_sudeste_leste`, `edificio_sul_oeste_cinza`,
  `edificio_ead_cobertura_azul` (building/high) e `massa_arborizada_oeste/sudoeste` (trees/high).
- **Registrar o zoneamento** como fonte oficial no Plano Espacial (substitui `pendente_de_localizacao` no
  nível de *classificação*, ainda não no nível de *coordenadas em metro*).
- Nada é aplicado na cena `Campus` até o checklist da §6 ser fechado.

## 8. Veredito formal

| Pergunta | Resposta |
|---|---|
| Pode gerar o campus inteiro agora? | **Não.** Escala/ofset/rotação indefinidos e classes ambíguas. |
| Pode reaproveitar a calibração antiga? | **Não.** Imagem-base e sistema diferentes. |
| Pode usar como zoneamento (o que é o quê)? | **Sim.** Melhor fonte espacial atual; substitui o mapa antigo como referência. |
| Pode gerar uma zona-piloto controlada? | **Sim**, com 1 área de alta confiança e depois da medida real de escala. |

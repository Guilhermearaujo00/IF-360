# Plano espacial de ambientação — IFNMG 360° (v3)

> Documento obrigatório da Fase 3 do prompt v3. Substitui o mapa antigo como referência de localização.
> Nada aqui foi obtido por interpretação visual de imagens. Toda posição citada vem da hierarquia/transforms
> da cena, de bounds medidos por script ou de descrições textuais. O que não pôde ser localizado está em
> `pendente_de_localizacao` e **não** receberá objetos.

- Data: 2026-10-20
- Cena: `Assets/Scenes/Campus.unity`
- Origem da auditoria: `Docs/MontagemCena/AUDITORIA_CENA_ASSETS.md` + `auditoria_assets.json`
- Fonte de zoneamento recebida: `Docs/MontagemCena/IFNMG_Januaria_zonas_para_Unity.json` (37 zonas,
  classificação visual) — revisada em `Docs/MontagemCena/REVISAO_JSON_Zonas_Januaria.md`.
- Status geral: **zoneamento (o quê) disponível; coordenadas em metro (onde) ainda indefinidas** →
  geração em larga escala continua bloqueada.

## 1. Convenção de coordenadas

| Item | Valor | Confiança |
|---|---|---|
| Eixo vertical | Y (Unity, Y para cima) | alta (convenção) |
| Plano horizontal | X / Z | alta (convenção) |
| Escala | 1 unidade Unity = 1 metro (**a confirmar contra marco real**) | média |
| Eixo que corresponde ao norte | **não determinado** — nenhum mapa georreferenciado foi recebido | — |
| Origem do mundo | `Plane` 20×20 m em (0,0,0); NPCs em (0,0,0) e arredores | alta |

Não foi aplicada nenhuma rotação/espelhamento de mapa. Não existe transformação `u,v→mundo` validada.

## 2. Limites do mundo (medidos)

- Renderers da cena: X [-256,45 ; 255,60], Y [-1,26 ; 17,89], Z [-185,62 ; 169,98] m.
- Dimensões: ~512 × 19 × 356 m. Quase tudo isso vem da raiz `Mapa_IFNMG` (status obsoleto).
- `Terrain` ativos: **0**. A cobertura de piso é feita por tiles modulares, não por Terrain.
- Piso detectado: 265 renderers baixos/grandes; bounding box 495,9 × 332,7 m; Y [0,00 ; 0,25].
- Cobertura por amostragem de 2 m: 14.731/41.416 pontos (35,6%) — **estimativa por bounds, não prova de vão**.

## 3. Zonas

### 3.1 Zonas do campus

A **classificação** das zonas existe agora no JSON recebido (37 zonas em `points_uv`: 15 `building`, 7 `paved`,
6 `grass`, 6 `trees`, 3 `soil`). Porém a **transformação `u,v → mundo` não está definida** (escala, origem e
rotação indefinidas) e o JSON não é GIS. Logo cada zona permanece `pendente_de_localizacao` **para efeito de
posicionamento em metros** e **não** receberá objetos até o checklist de
`REVISAO_JSON_Zonas_Januaria.md §6` ser fechado.

> Regra ativa: o zoneamento pode orientar *o que* colocar em cada área, mas **não** autoriza gerar coordenadas
> no campus enquanto a escala real não for confirmada por medida no terreno.

### 3.2 Zona de teste (sandbox)

| Campo | Valor |
|---|---|
| `id` | `sandbox_sintetico` |
| Tipo de superfície | gramado contínuo + praça de concreto + calçada (composição de teste) |
| Limites | centro (0, 1, 0), tamanho 40 × 4 × 30 m |
| Descrição | cena separada `IFNMG_AmbientacaoPreview.unity`; **não é um local do campus** |
| Elementos proibidos | nenhum (é um sandbox isolado) |
| Origem | escolha do desenvolvedor para testar encaixe/escala/composição |
| Confiança | alta (para a finalidade: teste isolado) |

> Esta zona serve **somente** para você validar materiais, escala, encaixe de módulos e estilo dos pacotes.
> Ela não autoriza copiar nada para o campus.

## 4. Objetos existentes que não podem ser cobertos/movidos

| Raiz | O que é | Observação |
|---|---|---|
| `Mapa_IFNMG` | 2.218 prefab instances geradas pelo mapa antigo | **congelada como não validada**; preservar |
| `Player` | jogador (2 renderers, 69 prefab instances) | preservar |
| `NPC_Diretor`, `NPC_Secretaria` | NPCs | preservar |
| `Main Camera`, `Canvas`, `Systems`, `GameManager`, `TelaCarregamento` | sistemas | não tocar |
| `Plane` | 20×20 m em Y=0 | superfície neutra |

## 5. Dados do mapa antigo — descartado / revalidado / mantido

| Dado antigo | Status | Motivo |
|---|---|---|
| `metersPerPixel = 0.516` | **descartado** | Válido só para a imagem 973×632 antiga; v3 proíbe reutilizar sem reconfirmar base e calibração. |
| `originPx (486,316)` | **descartado** | Depende da calibração acima. |
| `R01–R08` (vias) | **obsoleto** | Coordenadas/classificação não revalidadas. |
| `T01–T09` (terrenos) | **obsoleto** | Polígonos não revalidados. |
| `E01–E19` (estruturas) | **obsoleto** | Footprints/rotações não revalidados. |
| `V01–V10` (vegetação) | **obsoleto** | Centros/raios/quantidades não revalidados. |
| Áreas azuis como via | **corrigido pelo usuário** | São **prédios/edificações**. |
| Traçado vermelho sob árvores | **corrigido pelo usuário** | É **via**; faltam coordenadas/descrição para localizá-la. |
| Formato ID/tabela/confiança | **mantido como exemplo** | Só o formato documental; nunca os valores. |

Consequência: `Assets/Dados/MapaIFNMG/mapa_ifnmg.json` e a raiz `Mapa_IFNMG` ficam obsoletos até haver
referência atual. Não serão usados para gerar novas áreas.

## 6. O que falta para liberar a geração (pendências humanas)

Revisão detalhada em `REVISAO_JSON_Zonas_Januaria.md`. Pendências que continuam abertas:

1. **Escala real (crítica):** 1 medida conhecida no terreno (comprimento de prédio, distância entre blocos) para
   definir `worldWidth/worldDepth`. Sem isso a transformação `u,v → mundo` é indefinida e nada pode ser gerado.
2. **Ancoragem:** qual objeto real da cena `Campus` corresponde a um canto do perímetro (ou ao centro) — define
   origem e rotação do mapa.
3. **Norte:** qual eixo é o norte, se o mapa é georreferenciado (o JSON não informa).
4. **De-para visual↔ID:** confirmar as 15 `building` como prédios e as 8 `line` como vias; decidir tratamento de
   `grass` ambíguo, `soil` (solo x talude x via) e das massas `trees`.
5. **Zona inicial real:** uma única área de alta confiança para o primeiro teste de verdade (substitui o sandbox).

## 7. Enquanto isso (trabalho permitido pelo v3)

- Auditoria da cena e dos assets concluída.
- Sandbox isolado em cena separada, com seed e validação geométrica (Fase 6).
- Catálogo de prefabs com metadados reais e confiança (Fase 2).
- Nada aplicado no campus até a validação humana da prévia.

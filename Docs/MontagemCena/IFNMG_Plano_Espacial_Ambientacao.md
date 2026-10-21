# Plano espacial de ambientação — IFNMG 360° (v3)

> Documento obrigatório da Fase 3 do prompt v3. Substitui o mapa antigo como referência de localização.
> Nada aqui foi obtido por interpretação visual de imagens. Toda posição citada vem da hierarquia/transforms
> da cena, de bounds medidos por script ou de descrições textuais. O que não pôde ser localizado está em
> `pendente_de_localizacao` e **não** receberá objetos.

- Data: 2026-10-20
- Cena: `Assets/Scenes/Campus.unity`
- Origem da auditoria: `Docs/MontagemCena/AUDITORIA_CENA_ASSETS.md` + `auditoria_assets.json`
- Status geral: **sem nenhuma zona do campus validada com dados atuais** → geração em larga escala bloqueada.

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

**Nenhuma.** Não há fonte espacial atual (JSON/mapa ou descrição ancorada) que permita delimitar zonas do
campus com confiança. Todas as regiões estão `pendente_de_localizacao`.

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

1. JSON/mapa atual com coordenadas, dimensão da imagem-base e sistema de referência — **ou** descrições
   textuais ancoradas em objetos reais da cena (ex.: “gramado ao sul do prédio identificado como X”).
2. Coordenadas/descrição do traçado vermelho que é **via**.
3. Lista das áreas azuis que são **prédios** (para excluí-las de qualquer piso/vegetação).
4. Um marco real com distância conhecida, para confirmar que 1 unidade = 1 m.
5. Definição de qual eixo é o norte, se houver mapa georreferenciado.
6. Uma única zona inicial para a área de teste de verdade (a atual é sandbox sintético).

## 7. Enquanto isso (trabalho permitido pelo v3)

- Auditoria da cena e dos assets concluída.
- Sandbox isolado em cena separada, com seed e validação geométrica (Fase 6).
- Catálogo de prefabs com metadados reais e confiança (Fase 2).
- Nada aplicado no campus até a validação humana da prévia.

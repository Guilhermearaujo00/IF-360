# IFNMG 360° — seleção de assets para preencher a cena

## Base da análise

Esta seleção foi extraída de `PREFABS_TABELA.md`, que lista 2.526 prefabs com pacote, nome, caminho e dimensões de bounds em metros. A filtragem é feita pelos nomes/caminhos; não equivale a uma inspeção visual dos modelos. Confirme aparência, materiais e escala na Unity antes de aplicar globalmente.

## Prioridade: fechar a base antes de enfeitar

1. **Superfície contínua primeiro.** Use o terreno existente como base se já houver um Terrain/Mesh contínuo. Para trechos modulares de calçada, avaliar `SM_LPC_Ground_WalkPavingPlain_01` e `Road Sidewalk`. `Road Concrete Tile` e `Road Concrete Tile Small` têm 20×20 m e 10×10 m nos bounds — são candidatos para áreas pavimentadas maiores, não devem ser encaixados automaticamente como pequenas tiras de rua.
2. **Árvores de acordo com as manchas do mapa.** Use árvores maiores nas manchas arbóreas confirmadas e mantenha as clareiras de gramado livres. Não encha o meio dos gramados com árvores só para esconder vazios.
3. **Vegetação média e pequena.** Arbustos, cercas-vivas baixas, canteiros e tufos de grama fazem a transição entre árvores, bordas de prédios e terreno aberto. Distribua em grupos irregulares.
4. **Rochas e fungos são detalhes, não preenchimento geral.** Colocar rochas em bordas naturais/taludes e fungos discretamente sob a mata ou em áreas sombreadas; nunca na rua, no estacionamento ou no gramado central como padrão.
5. **Mobiliário com lógica.** Bancos, lixeiras e postes de luz só em locais de permanência/circulação, deixando portas, acessos e rotas livres.

## Assets selecionados

| Função | Pacote | Prefab | Caminho relativo em `Assets/` | Bounds (X×Y×Z) |
|---|---|---|---|---|
| pavimento_base | SimplePoly City - Low Poly Assets | `Road Concrete Tile` | `Assets/SimplePoly City - Low Poly Assets/Prefab/Roads/Road Concrete Tile.prefab` | 20×0×20 m |
| pavimento_base | SimplePoly City - Low Poly Assets | `Road Concrete Tile Small` | `Assets/SimplePoly City - Low Poly Assets/Prefab/Roads/Road Concrete Tile Small.prefab` | 10×0×10 m |
| calcadas | SimplePoly City - Low Poly Assets | `Road Sidewalk` | `Assets/SimplePoly City - Low Poly Assets/Prefab/Roads/Road Sidewalk.prefab` | 3×0.2×10 m |
| calcadas | LowPolyCityFree | `SM_LPC_Ground_WalkPavingPlain_01` | `Assets/LowPolyCityFree/Prefabs/05_Ground_Paving/SM_LPC_Ground_WalkPavingPlain_01.prefab` | 4×0.35×2 m |
| calcadas | LowPolyCityFree | `SM_LPC_Ground_WalkCornerExternal_01` | `Assets/LowPolyCityFree/Prefabs/05_Ground_Paving/SM_LPC_Ground_WalkCornerExternal_01.prefab` | 2×0.35×2 m |
| base_gramado | LowPolyCityFree | `SM_LPC_Ground_GroundGrassFlat_01` | `Assets/LowPolyCityFree/Prefabs/05_Ground_Paving/SM_LPC_Ground_GroundGrassFlat_01.prefab` | 4×0.23×4 m |
| vegetacao_urbana | LowPolyCityFree | `SM_LPC_Park_TreeBroadleafMature_01` | `Assets/LowPolyCityFree/Prefabs/08_Parks_Plants/SM_LPC_Park_TreeBroadleafMature_01.prefab` | 5×7.5×5.03 m |
| vegetacao_urbana | LowPolyCityFree | `SM_LPC_Park_TreeBroadleafSpreading_01` | `Assets/LowPolyCityFree/Prefabs/08_Parks_Plants/SM_LPC_Park_TreeBroadleafSpreading_01.prefab` | 6.09×5.7×6.09 m |
| vegetacao_urbana | LowPolyCityFree | `SM_LPC_Park_TreeBroadleafYoung_01` | `Assets/LowPolyCityFree/Prefabs/08_Parks_Plants/SM_LPC_Park_TreeBroadleafYoung_01.prefab` | 1.4×4×1.37 m |
| vegetacao_urbana | LowPolyCityFree | `SM_LPC_Park_BushRoundMedium_01` | `Assets/LowPolyCityFree/Prefabs/08_Parks_Plants/SM_LPC_Park_BushRoundMedium_01.prefab` | 1.17×0.84×1.19 m |
| vegetacao_urbana | LowPolyCityFree | `SM_LPC_Park_HedgeRunLow_01` | `Assets/LowPolyCityFree/Prefabs/08_Parks_Plants/SM_LPC_Park_HedgeRunLow_01.prefab` | 4×1×0.92 m |
| vegetacao_urbana | LowPolyCityFree | `SM_LPC_Park_FlowerBedRound_01` | `Assets/LowPolyCityFree/Prefabs/08_Parks_Plants/SM_LPC_Park_FlowerBedRound_01.prefab` | 1.95×0.45×2 m |
| vegetacao_de_detalhe | LowPolyCityFree | `SM_LPC_Park_GrassTuftClump_01` | `Assets/LowPolyCityFree/Prefabs/08_Parks_Plants/SM_LPC_Park_GrassTuftClump_01.prefab` | 0.23×0.32×0.27 m |
| vegetacao_natural_complementar | BluBlu Games — Low Poly Forest Mini Pack | `SM_Grass_01` | `Assets/BluBlu Games/Low Poly Forest Mini Pack/Prefabs/SM_Grass_01.prefab` | 0.54×0.54×0.37 m |
| vegetacao_natural_complementar | BluBlu Games — Low Poly Forest Mini Pack | `SM_FoliageBush_01` | `Assets/BluBlu Games/Low Poly Forest Mini Pack/Prefabs/SM_FoliageBush_01.prefab` | 1.82×1.41×1.56 m |
| vegetacao_natural_complementar | BluBlu Games — Low Poly Forest Mini Pack | `SM_Flower_01` | `Assets/BluBlu Games/Low Poly Forest Mini Pack/Prefabs/SM_Flower_01.prefab` | 0.29×0.4×0.29 m |
| rochas | Pandazole_Ultimate_Pack — Pandazole Nature Environment Pack | `HardRock_01` | `Assets/Pandazole_Ultimate_Pack/Pandazole Nature Environment Pack/Prefabs/HardRock_01.prefab` | 0.65×1.17×0.77 m |
| rochas | Pandazole_Ultimate_Pack — Pandazole Nature Environment Pack | `SoftRock_01` | `Assets/Pandazole_Ultimate_Pack/Pandazole Nature Environment Pack/Prefabs/SoftRock_01.prefab` | 0.84×0.39×0.87 m |
| fungos | BluBlu Games — Low Poly Forest Mini Pack | `SM_Mushroom_01` | `Assets/BluBlu Games/Low Poly Forest Mini Pack/Prefabs/SM_Mushroom_01.prefab` | 0.26×0.24×0.27 m |
| arvore_de_mata | BluBlu Games — Low Poly Forest Mini Pack | `SM_Tree_Green` | `Assets/BluBlu Games/Low Poly Forest Mini Pack/Prefabs/SM_Tree_Green.prefab` | 4.38×7.62×3.87 m |
| mobiliario_urbano | LowPolyCityFree | `SM_LPC_Street_BenchStreetSlat_01` | `Assets/LowPolyCityFree/Prefabs/06_Street_Furniture/SM_LPC_Street_BenchStreetSlat_01.prefab` | 1.8×0.9×0.53 m |
| mobiliario_urbano | LowPolyCityFree | `SM_LPC_Street_LampStreetColumn_01` | `Assets/LowPolyCityFree/Prefabs/06_Street_Furniture/SM_LPC_Street_LampStreetColumn_01.prefab` | 0.31×6×1.87 m |
| mobiliario_urbano | LowPolyCityFree | `SM_LPC_Street_StreetBinRound_01` | `Assets/LowPolyCityFree/Prefabs/06_Street_Furniture/SM_LPC_Street_StreetBinRound_01.prefab` | 0.56×1×0.56 m |

## Regras que o OpenCode deve seguir

1. Preservar todos os edifícios reais já posicionados; não substituir o campus por prédios genéricos de pacotes urbanos.
2. Não cobrir lacunas com vegetação antes de garantir uma superfície contínua de terreno/gramado ou pavimentação.
3. Classificar regiões permitidas por tipo: gramado, mata, via, passeio, footprint de prédio e área de acesso.
4. Nunca posicionar assets naturais em vias, calçadas, estacionamento, dentro de edifícios, entradas ou corredores de circulação.
5. Usar raycast para colocar a base do prefab sobre o terreno; aplicar rotação aleatória no eixo vertical e variação de escala discreta, preservando escala plausível.
6. Usar colliders/bounds e distâncias mínimas para evitar árvores e rochas sobrepostas; agrupar vegetação de maneira irregular, não em grade uniforme.
7. Distribuição sugerida: árvores só nas manchas arbóreas do mapa; arbustos/tufos nas transições; rochas em bordas naturais; fungos muito esparsos na sombra da mata; bancos/lixeiras/luminárias em áreas de circulação.
8. Gerar primeiro uma prévia editável em uma hierarquia separada, com seed fixa, contagem por classe, opção de limpar somente objetos gerados e sem apagar objetos da cena original.
9. Antes de aplicar em toda a cena, criar uma área-piloto e validar materiais, escala, normais, rotação, colliders e compatibilidade visual dos pacotes.

## Conjuntos recomendados por consistência

- **Urbano/campus:** usar `LowPolyCityFree` para gramado modular, árvores, arbustos, canteiros, bancos, postes e lixeiras; avaliar as peças de calçada de `SimplePoly City - Low Poly Assets` somente após conferir se o estilo e os materiais combinam.
- **Borda de mata:** usar uma única família de vegetação/rochas para criar grupos coerentes. `BluBlu Games — Low Poly Forest Mini Pack` possui grama, arbustos, flores, fungos, rochas e árvores no mesmo pack. Pode ser um conjunto complementar, desde que a aparência combine com a vegetação urbana selecionada.
- **Alternativa de rochas:** `Pandazole Nature Environment Pack` possui `HardRock_*`, `SoftRock_*`, `Grass_*`, `Foliage_*` e árvores sazonais. Selecionar poucos variantes da mesma família; não espalhar dezenas de variantes sem necessidade.
- Não usar assets de prédios residenciais/comerciais apenas porque a área está vazia. Os prédios devem continuar representando o campus real, não uma cidade genérica.

## Prompt pronto para o OpenCode

Analise `IFNMG_Selecao_Assets_Ambientacao.json` e `PREFABS_TABELA.md`. Localize os prefabs pelos caminhos completos fornecidos; não invente nomes nem procure substitutos fora do projeto antes de verificar esses caminhos. Crie uma ferramenta de Editor para pré-visualizar a distribuição de ambiente na cena atual: preservar prédios e objetos existentes; identificar/usar máscaras separadas de gramado, mata, pavimento, passeio, footprints de prédios e áreas de acesso; preencher primeiro superfícies contínuas; depois distribuir árvores, arbustos, capim, rochas, fungos e mobiliário conforme as regras do JSON. O gerador deve usar raycast para aderir ao terreno, colliders/bounds para impedir interseções, variação de rotação/escala discreta, seed reproduzível, hierarquia própria de objetos gerados, botão de Preview/Apply/Clear Generated e relatório das contagens. Nunca apagar ou mover objetos existentes automaticamente. Não coloque qualquer asset natural sobre vias, calçadas, estacionamento, portas ou dentro de prédios. Comece por uma área-piloto e só expanda após validação visual.
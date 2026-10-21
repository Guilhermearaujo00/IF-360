# Auditoria da cena e dos assets (prompt v3)

- Data: 2026-10-21 00:51
- Cena analisada: `Assets/IFNMG360/Scenes/IFNMG_Mapa_Prototype_v0_1.unity` (IFNMG_Mapa_Prototype_v0_1)
- Commit atual: `1968f9d5493d35520b9d42661b6e90d922ee8547`
- Backup criado: `%TEMP%/opencode/backup_20261020/Campus.unity`
- Modo: somente leitura; a cena **não** foi salva nem alterada.
- Regra v3: nenhuma imagem foi interpretada visualmente. Tudo abaixo vem de hierarquia, transforms, bounds, metadados e arquivos de texto.

## 1. Arquivos de referência

| Arquivo | Status |
|---|---|
| `Docs/PREFABS_TABELA.md` | presente |
| `Docs/MontagemCena/IFNMG_Selecao_Assets_Ambientacao.md` | presente |
| `Docs/MontagemCena/IFNMG_Selecao_Assets_Ambientacao.json` | presente |
| `Docs/MontagemCena/IFNMG_Januaria_zonas_para_Unity.json` | presente |
| `Docs/MontagemCena/IFNMG_Plano_Espacial_Ambientacao.md` | presente |
| `Docs/MontagemCena/Prompt_OpenCode_Ambientacao_IFNMG_v3.md` | **AUSENTE** |

> O mapa antigo (`mapa_ifnmg.json`, IDs E/R/T/V, 0,516 m/px) é tratado como **obsoleto**; ver seção 7.

## 2. Cenas do projeto

| Cena em Build Settings | Habilitada |
|---|---|
| `Assets/Scenes/Menu.unity` | sim |
| `Assets/Scenes/Campus.unity` | sim |

Cenas encontradas em `Assets/`: **104**.
- Assets/_Recovery/0 (1).unity
- Assets/_Recovery/0 (2).unity
- Assets/_Recovery/0 (3).unity
- Assets/_Recovery/0 (4).unity
- Assets/_Recovery/0 (5).unity
- Assets/_Recovery/0 (6).unity
- Assets/_Recovery/0 (7).unity
- Assets/_Recovery/0.unity
- Assets/3D set of stylized nature - GHIBLI style/Art/Demo/Overiew.unity
- Assets/3D_Game_Assets_Flora/SampleScene/TestScene.unity
- Assets/AGWYN's Low Poly Destructible Vegetation/Scenes/DemoScene.unity
- Assets/Anime Trees/Scenes/AllAssets.unity
- Assets/Anime Trees/Scenes/AnimeTrees.unity
- Assets/Arabic Neoclassical/Street Road/Demo_Roads.unity
- Assets/BigRookGames/Nature Pack/Nature Pack Demo HDRP.unity
- Assets/BigRookGames/Nature Pack/Nature Pack Demo URP.unity
- Assets/BigRookGames/Nature Pack/Nature Pack Demo.unity
- Assets/BluBlu Games/Low Poly Forest Mini Pack/Scenes/Assets.unity
- Assets/BluBlu Games/Low Poly Forest Mini Pack/Scenes/Demo.unity
- Assets/BOXOPHOBIC/Skybox Cubemap Extended/Demo/Demo Blend.unity
- Assets/BOXOPHOBIC/Skybox Cubemap Extended/Demo/Demo Day.unity
- Assets/BOXOPHOBIC/Skybox Cubemap Extended/Demo/Demo Night.unity
- Assets/BOXOPHOBIC/Utils/Scene/Scene Empty.unity
- Assets/Cody Dreams/Terra Canvas/Demo scene.unity
- Assets/Cozy Village & Home/Demo.unity
- Assets/CubexCube - Free City Pack I/Scenes/Free_City_Scene_I.unity
- Assets/Dzeruza/CliffStonePackL/Scenes/DemoSceneA/DemoScene_CliffStonePackL_A.unity
- Assets/Dzeruza/CliffStonePackL/Scenes/DemoSceneB/DemoScene_CliffStonePackL_B.unity
- Assets/Dzeruza/CliffStonePackM/Scenes/DemoScene_A/DemoScene_CliffStonePackM_A_HDRP.unity
- Assets/Dzeruza/CliffStonePackM/Scenes/DemoScene_B/DemoScene_CliffStonePackM_B_HDRP.unity
- Assets/Fantasy Skybox FREE/Scenes/Demo with terrain.unity
- Assets/Fantasy Skybox FREE/Scenes/Demo without terrain.unity
- Assets/Gogo Casual Pack/Gogo Casual Free Plants Pack/Demo Scene.unity
- Assets/Holotna/Mountain/Scenes/Demo.unity
- Assets/IFNMG360/Scenes/IFNMG_Mapa_Prototype_v0_1.unity
- Assets/IL3DN/Scenes/TheIllustratedNature_FreeAssets.unity
- Assets/IL3DN/Scenes/TheIllustratedNature_FreeDemo.unity
- Assets/ithappy/Cartoon_City_Free/Scenes/Built-In_Scenes/Demonstration.unity
- Assets/ithappy/Cartoon_City_Free/Scenes/Built-In_Scenes/Overview.unity
- Assets/ithappy/Cartoon_City_Free/Scenes/HDRP_Scenes/Demonstration.unity
- Assets/ithappy/Cartoon_City_Free/Scenes/HDRP_Scenes/Overview.unity
- Assets/ithappy/Cartoon_City_Free/Scenes/URP_Scenes/Demonstration.unity
- Assets/ithappy/Cartoon_City_Free/Scenes/URP_Scenes/Overview.unity
- Assets/JC_StylizedNature_Lite/Scenes/StylizedNatureLite_Demo.unity
- Assets/JC_StylizedNature_Lite/Scenes/StylizedNatureLite_Overview.unity
- Assets/Loading Games/Toon City Pack/Demo/Scenes/Demo.unity
- Assets/Lowpoly Style/Free Rocks and Plants/Lowpoly Style Demo.unity
- Assets/LowPolyCityFree/Scenes/corner.unity
- Assets/LowPolyCityFree/Scenes/culdesac.unity
- Assets/LowPolyCityFree/Scenes/green.unity
- Assets/LowPolyCityFree/Scenes/neighbourhood.unity
- Assets/lowpoly-nature/lowpoly-nature/scene/scene.unity
- Assets/LowpolyTexturesPack/Demo/Demo.unity
- Assets/ModularLowpolyStreetsFree/Scenes/All complex prefabs.unity
- Assets/ModularLowpolyStreetsFree/Scenes/All prefabs.unity
- Assets/ModularLowpolyStreetsFree/Scenes/Demo.unity
- Assets/Nature/GrassAndFlowers/GrassAndFlower.unity
- Assets/Palmov Island/Low Poly Atmospheric Locations Pack/Scenes/All models.unity
- Assets/Palmov Island/Low Poly Atmospheric Locations Pack/Scenes/All scenes.unity
- Assets/Palmov Island/Low Poly Atmospheric Locations Pack/Scenes/Animals.unity
- Assets/Palmov Island/Low Poly Atmospheric Locations Pack/Scenes/Environment.unity
- Assets/Palmov Island/Low Poly Atmospheric Locations Pack/Scenes/Houses.unity
- Assets/Palmov Island/Low Poly Atmospheric Locations Pack/Scenes/Islands.unity
- Assets/Palmov Island/Low Poly Atmospheric Locations Pack/Scenes/Lands.unity
- Assets/Palmov Island/Low Poly Atmospheric Locations Pack/Scenes/Location autumn atmosphere forest.unity
- Assets/Palmov Island/Low Poly Atmospheric Locations Pack/Scenes/Location camping forest.unity
- Assets/Palmov Island/Low Poly Atmospheric Locations Pack/Scenes/Location christmas village.unity
- Assets/Palmov Island/Low Poly Atmospheric Locations Pack/Scenes/Location desert pyramids.unity
- Assets/Palmov Island/Low Poly Atmospheric Locations Pack/Scenes/Location downtown.unity
- Assets/Palmov Island/Low Poly Atmospheric Locations Pack/Scenes/Location japanes island.unity
- Assets/Palmov Island/Low Poly Atmospheric Locations Pack/Scenes/Location soccer field.unity
- Assets/Palmov Island/Low Poly Atmospheric Locations Pack/Scenes/Location space shuttle launch.unity
- Assets/Palmov Island/Low Poly Atmospheric Locations Pack/Scenes/Location treasure Island.unity
- Assets/Palmov Island/Low Poly Atmospheric Locations Pack/Scenes/Location wild west railway station.unity
- Assets/Palmov Island/Low Poly Atmospheric Locations Pack/Scenes/Low Poly Simple Urban City 3D Asset Pack.unity
- Assets/Palmov Island/Low Poly Atmospheric Locations Pack/Scenes/Mountains.unity
- Assets/Palmov Island/Low Poly Atmospheric Locations Pack/Scenes/Plants.unity
- Assets/Palmov Island/Low Poly Atmospheric Locations Pack/Scenes/SampleScene.unity
- Assets/Palmov Island/Low Poly Atmospheric Locations Pack/Scenes/Stones.unity
- Assets/Palmov Island/Low Poly Atmospheric Locations Pack/Scenes/Trees.unity
- Assets/Palmov Island/Low Poly Atmospheric Locations Pack/Scenes/Vehicles.unity
- Assets/Palmov Island/Low Poly Atmospheric Locations Pack/Scenes/Waters.unity
- Assets/Palmov Island/Low Poly Houses Free Pack/Scenes/Low Poly City Mega Pack Free.unity
- Assets/Pandazole_Ultimate_Pack/Pandazole City Town Pack/Scenes/Demo.unity
- Assets/Pandazole_Ultimate_Pack/Pandazole Nature Environment Pack/Scenes/Demo.unity
- Assets/Park/MainScene.unity
- Assets/PolyG/Low Poly Nature Pack - Lite/Demo Scenes/Demo_01_Overview.unity
- Assets/PolyG/Low Poly Nature Pack - Lite/Demo Scenes/Demo_02.unity
- Assets/Scenes/Campus.unity
- Assets/Scenes/IFNMG_AmbientacaoPreview.unity
- Assets/Scenes/Menu.unity
- Assets/Simple city plain/scene/Demo Scene.unity
- Assets/Simple Vehicle Pack/Demo_scene.unity
- Assets/SimplePoly City - Low Poly Assets/Demo/SimplePoly City - Low Poly Assets_Demo Scene.unity
- Assets/SimpleSky/Scenes/Demo.unity
- Assets/StarsandShellsStudio/Scene/Demo.unity
- Assets/Stylized Vehicles Pack Free/Scenes/Cars.unity
- Assets/Ultrasonic/Decorative Plant Assets/Scenes/demo_scene.unity
- Assets/Vegetation_Stylized_Pack_ByLuxArtStudios/ScenesSetUps/L_AssetZoo.unity
- Assets/Vegetation_Stylized_Pack_ByLuxArtStudios/ScenesSetUps/L_Showcase.unity
- Assets/VertexRageStudio/LowPolyNatureEssentials/Scenes/FloatingIsland.unity
- Assets/VertexRageStudio/LowPolyNatureEssentials/Scenes/Overview Nice.unity
- Assets/VertexRageStudio/LowPolyNatureEssentials/Scenes/Overview.unity
- Assets/VoxelNest/cozy_Voxel_Nature_FREE/Scenes/SampleScene.unity

## 3. Eixos, escala e limites do mundo

- Convenção da Unity: **Y para cima**, X/Z no plano horizontal. O v3 não assume qual eixo é o norte; nenhuma rotação de mapa foi aplicada.
- Escala assumida: **1 unidade Unity = 1 metro** (a confirmar contra um marco real da cena).
- Limites dos renderers da cena: X [-251,48, 249,80], Y [-0,15, 8,36], Z [-182,65, 182,54] m.
- Dimensões: 501,3 × 8,5 × 365,2 m.
- Componentes `Terrain` ativos: **0**.

## 4. Hierarquia e áreas ocupadas

Objetos raiz: **1**.

| Raiz | Filhos diretos | Renderers | Colliders | PrefabInstances | Bounds X×Y×Z (m) | Y mín–máx |
|---|---:|---:|---:|---:|---|---|
| IFNMG_Mapa_Prototype_v0_1 | 5 | 196 | 124 | 0 | 501,3 × 8,5 × 365,2 | -0,15 – 8,36 |

## 5. Superfícies de piso e cobertura

Renderers candidatos a piso (baixos, grandes, próximos de Y=0): **109**.
- Área coberta pelo conjunto (bounding box, não união real): 499,6 × 364,8 m; Y [-0,15, 1,06].

| Piso (amostra) | Bounds X×Y×Z (m) | Y mín |
|---|---|---|
| TerrenoBase_500x364m | 499,6 × 0,00 × 364,5 | -0,15 |
| via_principal_norte_sul | 250,7 × 0,00 × 308,5 | 0,03 |
| gramado_nordeste | 170,7 × 0,00 × 174,2 | -0,06 |
| solo_caminho_leste | 114,2 × 0,00 × 185,9 | -0,04 |
| massa_arborizada_oeste | 105,3 × 0,00 × 197,5 | -0,04 |
| massa_arborizada_sudoeste | 163,0 × 0,00 × 120,0 | -0,04 |
| via_sul_centro | 131,9 × 0,00 × 130,3 | 0,03 |
| gramado_centro_sul | 144,3 × 0,00 × 104,3 | -0,06 |
| via_central_sudoeste | 85,7 × 0,00 × 142,3 | 0,03 |
| gramado_sudoeste_aberto | 97,2 × 0,00 × 113,4 | -0,06 |
| gramado_sul_centro | 136,6 × 0,00 × 78,5 | -0,06 |
| via_sob_copas_oeste_sudoeste | 167,4 × 0,00 × 58,3 | 0,03 |
| arvores_noroeste | 84,5 × 0,00 × 115,2 | -0,04 |
| arvores_alinhadas_sudeste | 106,8 × 0,00 × 65,8 | -0,04 |
| gramado_leste_centro | 71,6 × 0,00 × 53,7 | -0,06 |
| via_oeste_centro | 122,5 × 0,00 × 31,2 | 0,03 |
| arvores_centro_oeste | 75,0 × 0,00 × 45,3 | -0,04 |
| gramado_centro_oeste | 62,3 × 0,00 × 54,1 | -0,06 |
| arvores_alinhadas_centro | 75,2 × 0,00 × 42,6 | -0,04 |
| solo_sul_leste | 64,9 × 0,00 × 46,1 | -0,04 |

- Cobertura por amostragem de 2 m dentro da bounding box: **45627/45750** pontos (99,7%).
- Lacunas são **estimativas por bounds**, não prova de vão real (módulos podem se sobrepor).
- Primeiros pontos sem piso: (-249,8, -62,3), (-249,8, -60,3), (-249,8, -58,3), (-249,8, -56,3), (-249,8, -54,3), (-249,8, -52,3), (-249,8, -50,3), (-249,8, -48,3), (-249,8, -46,3), (-249,8, -44,3), (-249,8, -42,3), (-249,8, -40,3), ….

## 6. Catálogo de prefabs (metadados reais)

Prefabs distintos usados na cena: **0**. Candidatos da seleção: **23**. Inspecionados: **23**.

Legenda de confiança: **alta** = caminho/nome + renderer coerentes; **média** = categoria inferida apenas por nome/pacote; **baixa** = ambíguo ou sem renderer → revisão humana.

| Prefab | Pacote | Usos na cena | Renderers | Bounds X×Y×Z (m) | Meshes (amostra) | Materiais (shader; cor; smooth/metallic) | Colliders | Categoria inferida | Confiança |
|---|---|---:|---:|---|---|---|---|---|---|
| SM_Flower_01 | BluBlu Games | 0 | 2 | 0,29 × 0,40 × 0,29 | SM_Flower_05 | M_Foliage [BluBlu/Builtin_Foliage] BaseColor=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,00 metal=0,00<br>M_Foliage_LOD [BluBlu/Builtin_Foliage] BaseColor=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,00 metal=0,00 | nenhum | Vegetação | alta |
| SM_FoliageBush_01 | BluBlu Games | 0 | 1 | 1,82 × 1,41 × 1,56 | SM_FoliageBush_01 | Bush_01 1 [BluBlu/Builtin_Foliage] BaseColor=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,00 metal=0,00 | nenhum | Vegetação | alta |
| SM_Grass_01 | BluBlu Games | 0 | 1 | 0,54 × 0,54 × 0,37 | SM_Grass_03 | M_Grass_01 [BluBlu/Builtin_Foliage] BaseColor=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,00 metal=0,00 | nenhum | Vegetação | alta |
| SM_Mushroom_01 | BluBlu Games | 0 | 1 | 0,26 × 0,24 × 0,27 | SM_Mushroom_03 | M_Mushroom [BluBlu/Builtin_Foliage] BaseColor=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,00 metal=0,00 | nenhum | Vegetação | alta |
| SM_Tree_Green | BluBlu Games | 0 | 2 | 4,38 × 7,62 × 3,87 | SM_Tree_01 | Tree_Leaves_A [BluBlu/Builtin_Foliage] BaseColor=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,00 metal=0,00<br>TreeBark [BluBlu/Builtin_Foliage] BaseColor=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,00 metal=0,00<br>…(+1) | 1 (CapsuleCollider×1) | Vegetação | alta |
| SM_LPC_Ground_GroundGrassFlat_01 | LowPolyCityFree | 0 | 1 | 4,00 × 0,23 × 4,00 | SM_LPC_Ground_GroundGrassFlat_01 | M_LPC_Main [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,18 metal=0,00 tex=LPC_Palette | 1 (MeshCollider×1) | Superfície | alta |
| SM_LPC_Ground_WalkCornerExternal_01 | LowPolyCityFree | 0 | 1 | 2,00 × 0,35 × 2,00 | SM_LPC_Ground_WalkCornerExternal_01 | M_LPC_Main [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,18 metal=0,00 tex=LPC_Palette | 1 (MeshCollider×1) | Superfície | alta |
| SM_LPC_Ground_WalkPavingPlain_01 | LowPolyCityFree | 0 | 1 | 4,00 × 0,35 × 2,00 | SM_LPC_Ground_WalkPavingPlain_01 | M_LPC_Main [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,18 metal=0,00 tex=LPC_Palette | 1 (MeshCollider×1) | Superfície | alta |
| SM_LPC_Street_BenchStreetSlat_01 | LowPolyCityFree | 0 | 1 | 1,80 × 0,90 × 0,53 | SM_LPC_Street_BenchStreetSlat_01 | M_LPC_Main [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,18 metal=0,00 tex=LPC_Palette | 1 (MeshCollider×1) | Mobiliário | alta |
| SM_LPC_Street_LampStreetColumn_01 | LowPolyCityFree | 0 | 1 | 0,31 × 6,00 × 1,87 | SM_LPC_Street_LampStreetColumn_01 | M_LPC_Main [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,18 metal=0,00 tex=LPC_Palette<br>M_LPC_Emissive [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,25 metal=0,00 tex=LPC_Palette_Emissive | 1 (MeshCollider×1) | Mobiliário | alta |
| SM_LPC_Street_StreetBinRound_01 | LowPolyCityFree | 0 | 1 | 0,56 × 1,00 × 0,56 | SM_LPC_Street_StreetBinRound_01 | M_LPC_Main [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,18 metal=0,00 tex=LPC_Palette | 1 (MeshCollider×1) | Mobiliário | alta |
| SM_LPC_Park_BushRoundMedium_01 | LowPolyCityFree | 0 | 1 | 1,17 × 0,84 × 1,19 | SM_LPC_Park_BushRoundMedium_01 | M_LPC_Main [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,18 metal=0,00 tex=LPC_Palette | 1 (MeshCollider×1) | Vegetação | alta |
| SM_LPC_Park_FlowerBedRound_01 | LowPolyCityFree | 0 | 1 | 1,95 × 0,45 × 2,00 | SM_LPC_Park_FlowerBedRound_01 | M_LPC_Main [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,18 metal=0,00 tex=LPC_Palette | 1 (MeshCollider×1) | Vegetação | alta |
| SM_LPC_Park_GrassTuftClump_01 | LowPolyCityFree | 0 | 1 | 0,23 × 0,32 × 0,27 | SM_LPC_Park_GrassTuftClump_01 | M_LPC_Main [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,18 metal=0,00 tex=LPC_Palette | 1 (MeshCollider×1) | Vegetação | alta |
| SM_LPC_Park_HedgeRunLow_01 | LowPolyCityFree | 0 | 1 | 4,00 × 1,00 × 0,92 | SM_LPC_Park_HedgeRunLow_01 | M_LPC_Main [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,18 metal=0,00 tex=LPC_Palette | 1 (MeshCollider×1) | Vegetação | alta |
| SM_LPC_Park_TreeBroadleafMature_01 | LowPolyCityFree | 0 | 1 | 5,00 × 7,50 × 5,03 | SM_LPC_Park_TreeBroadleafMature_01 | M_LPC_Main [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,18 metal=0,00 tex=LPC_Palette | 1 (MeshCollider×1) | Vegetação | alta |
| SM_LPC_Park_TreeBroadleafSpreading_01 | LowPolyCityFree | 0 | 1 | 6,09 × 5,70 × 6,09 | SM_LPC_Park_TreeBroadleafSpreading_01 | M_LPC_Main [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,18 metal=0,00 tex=LPC_Palette | 1 (MeshCollider×1) | Vegetação | alta |
| SM_LPC_Park_TreeBroadleafYoung_01 | LowPolyCityFree | 0 | 1 | 1,40 × 4,00 × 1,37 | SM_LPC_Park_TreeBroadleafYoung_01 | M_LPC_Main [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,18 metal=0,00 tex=LPC_Palette | 1 (MeshCollider×1) | Vegetação | alta |
| HardRock_01 | Pandazole_Ultimate_Pack | 0 | 1 | 0,65 × 1,17 × 0,77 | HardRock_01 | PandaMat [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=PandaMat | 1 (MeshCollider×1) | Indefinido | baixa |
| SoftRock_01 | Pandazole_Ultimate_Pack | 0 | 1 | 0,84 × 0,39 × 0,87 | SoftRock_01 | PandaMat [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=PandaMat | 1 (MeshCollider×1) | Indefinido | baixa |
| Road Concrete Tile Small | SimplePoly City - Low Poly Assets | 0 | 1 | 10,00 × 0,00 × 10,00 | Road Concrete Tile Small | Road [Legacy Shaders/Diffuse] Color=RGBA(1.000, 1.000, 1.000, 1.000) tex=Road | nenhum | Superfície | alta |
| Road Concrete Tile | SimplePoly City - Low Poly Assets | 0 | 1 | 20,00 × 0,00 × 20,00 | Road Concrete Tile | Road [Legacy Shaders/Diffuse] Color=RGBA(1.000, 1.000, 1.000, 1.000) tex=Road | nenhum | Superfície | alta |
| Road Sidewalk | SimplePoly City - Low Poly Assets | 0 | 1 | 3,00 × 0,20 × 10,00 | Road Sidewalk | Road [Legacy Shaders/Diffuse] Color=RGBA(1.000, 1.000, 1.000, 1.000) tex=Road | nenhum | Superfície | alta |

> Categoria é **hipótese por nome/caminho**, não confirmação visual. Materiais mostram até 2 por prefab.

## 7. Dados do mapa antigo: descartados / revalidados

| Dado antigo | Status | Motivo |
|---|---|---|
| `metersPerPixel = 0.516` | **descartado** | Válido apenas para a imagem 973×632 antiga; v3 proíbe reutilizar sem reconfirmar base e calibração. |
| `originPx (486,316)` | **descartado** | Mesma calibração do item acima. |
| IDs `R01–R08` (vias) | **obsoleto** | Classificação e coordenadas antigas; não revalidado. |
| IDs `T01–T09` (terrenos) | **obsoleto** | Polígonos antigos; não revalidados. |
| IDs `E01–E19` (estruturas) | **obsoleto** | Footprints/rotações antigos; não revalidados. |
| IDs `V01–V10` (vegetação) | **obsoleto** | Centros/raios/n antigos; não revalidados. |
| Áreas azuis classificadas como via | **corrigido pelo usuário** | São **prédios/edificações**; classificação antiga não deve ser usada. |
| Traçado vermelho sob as árvores | **corrigido pelo usuário** | É **via**; falta o dado textual/vetorial de coordenadas para localizá-lo. |
| Formato ID/tabela/confiança | **mantido como exemplo** | Só o formato documental; nunca os valores. |

Consequência: a raiz `Mapa_IFNMG` e `Assets/Dados/MapaIFNMG/mapa_ifnmg.json` ficam **congelados como não validados** até existir referência espacial atual.

## 8. Pendências de dados (humano)

1. JSON/mapa atual com coordenadas, dimensão da imagem-base e sistema de referência (ou descrições textuais ancoradas em objetos reais).
2. Coordenadas/descrição do traçado vermelho que é via.
3. Lista das áreas azuis que são prédios (para não classificá-las como piso).
4. Confirmação da escala real (marco da cena com distância conhecida).
5. Definição de qual eixo corresponde ao norte (se houver mapa georreferenciado).
6. Zona única, localizável com dados atuais, para a área de teste.


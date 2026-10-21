# Auditoria da cena e dos assets (prompt v3)

- Data: 2026-10-20 22:58
- Cena analisada: `Assets/Scenes/Campus.unity` (Campus)
- Commit atual: `33eaab7006df7d9f921a4e006301d02737ef58cf`
- Backup criado: `%TEMP%/opencode/backup_20261020/Campus.unity`
- Modo: somente leitura; a cena **não** foi salva nem alterada.
- Regra v3: nenhuma imagem foi interpretada visualmente. Tudo abaixo vem de hierarquia, transforms, bounds, metadados e arquivos de texto.

## 1. Arquivos de referência

| Arquivo | Status |
|---|---|
| `Docs/PREFABS_TABELA.md` | presente |
| `Docs/MontagemCena/IFNMG_Selecao_Assets_Ambientacao.md` | presente |
| `Docs/MontagemCena/IFNMG_Selecao_Assets_Ambientacao.json` | presente |
| `Docs/MontagemCena/IFNMG_Januaria_zonas_para_Unity.json` | **AUSENTE** |
| `Docs/MontagemCena/IFNMG_Plano_Espacial_Ambientacao.md` | **AUSENTE** |
| `Docs/MontagemCena/Prompt_OpenCode_Ambientacao_IFNMG_v3.md` | **AUSENTE** |

> O mapa antigo (`mapa_ifnmg.json`, IDs E/R/T/V, 0,516 m/px) é tratado como **obsoleto**; ver seção 7.

## 2. Cenas do projeto

| Cena em Build Settings | Habilitada |
|---|---|
| `Assets/Scenes/Menu.unity` | sim |
| `Assets/Scenes/Campus.unity` | sim |

Cenas encontradas em `Assets/`: **102**.
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
- Limites dos renderers da cena: X [-256,45, 255,60], Y [-1,26, 17,89], Z [-185,62, 169,98] m.
- Dimensões: 512,1 × 19,1 × 355,6 m.
- Componentes `Terrain` ativos: **0**.

## 4. Hierarquia e áreas ocupadas

Objetos raiz: **10**.

| Raiz | Filhos diretos | Renderers | Colliders | PrefabInstances | Bounds X×Y×Z (m) | Y mín–máx |
|---|---:|---:|---:|---:|---|---|
| Canvas | 2 | 0 | 0 | 0 | — | — |
| GameManager | 0 | 0 | 0 | 0 | — | — |
| Main Camera | 0 | 0 | 0 | 0 | — | — |
| Mapa_IFNMG | 4 | 1822 | 784 | 2218 | 512,1 × 19,1 × 355,6 | -1,26 – 17,89 |
| NPC_Diretor | 0 | 1 | 1 | 0 | 1,0 × 2,0 × 1,0 | 0,00 – 2,00 |
| NPC_Secretaria | 0 | 1 | 2 | 0 | 1,0 × 2,0 × 1,0 | 0,00 – 2,00 |
| Plane | 0 | 1 | 1 | 0 | 20,0 × 0,0 × 20,0 | 0,00 – 0,00 |
| Player | 2 | 2 | 1 | 69 | 1,7 × 1,9 × 1,0 | 0,00 – 1,88 |
| Systems | 0 | 0 | 0 | 0 | — | — |
| TelaCarregamento | 0 | 0 | 0 | 0 | — | — |

### 4.1 `Mapa_IFNMG` — gerado a partir do mapa antigo (status: **obsoleto, não validado**)

| Grupo | Objetos | Renderers | Colliders | Extensão XZ (m) | Y mín–máx |
|---|---:|---:|---:|---|---|
| Terrenos | 110 | 109 | 1 | 476,1 × 313,8 | 0,00 – 0,05 |
| Vias | 156 | 155 | 80 | 464,5 × 332,7 | 0,05 – 0,25 |
| Estruturas | 363 | 358 | 303 | 403,6 × 353,2 | -0,22 – 17,89 |
| Vegetacao | 1601 | 1200 | 400 | 510,1 × 329,6 | -1,26 – 13,45 |

#### Estruturas geradas (ids do mapa antigo)

| Id | Bounds X×Y×Z (m) | Centro (m) |
|---|---|---|
| E01_0_0 | 26,6 × 7,9 × 26,6 | (67,3, 3,7, 67,0) |
| E01_0_1 | 26,6 × 7,9 × 26,6 | (83,1, 3,7, 51,3) |
| E01_0_2 | 26,6 × 7,9 × 26,6 | (98,8, 3,7, 35,5) |
| E01_0_0 | 26,6 × 7,9 × 26,6 | (67,1, 3,7, 83,8) |
| E01_0_1 | 26,6 × 7,9 × 26,6 | (82,9, 3,7, 99,6) |
| E01_0_2 | 26,6 × 7,9 × 26,6 | (98,7, 3,7, 115,3) |
| E01_0_0 | 26,6 × 7,9 × 26,6 | (50,5, 3,7, 66,9) |
| E01_0_1 | 26,6 × 7,9 × 26,6 | (34,8, 3,7, 51,1) |
| E01_0_2 | 26,6 × 7,9 × 26,6 | (19,0, 3,7, 35,3) |
| E01_0_0 | 26,6 × 7,9 × 26,6 | (50,3, 3,7, 83,6) |
| E01_0_1 | 26,6 × 7,9 × 26,6 | (34,6, 3,7, 99,4) |
| E01_0_2 | 26,6 × 7,9 × 26,6 | (18,8, 3,7, 115,2) |
| E01_1 | 9,9 × 0,2 × 9,9 | (18,7, 8,0, 79,0) |
| E01_2 | 9,9 × 0,2 × 9,9 | (36,9, 8,0, 97,2) |
| E01_3 | 9,9 × 0,2 × 9,9 | (55,2, 8,0, 115,5) |
| E01_4 | 9,9 × 0,2 × 9,9 | (36,9, 8,0, 60,7) |
| E01_5 | 9,9 × 0,2 × 9,9 | (55,2, 8,0, 79,0) |
| E01_6 | 9,9 × 0,2 × 9,9 | (73,4, 8,0, 97,2) |
| E02_0_0 | 26,7 × 7,6 × 25,4 | (101,6, 3,8, 29,9) |
| E02_0_1 | 26,7 × 7,6 × 25,4 | (119,8, 3,8, 14,7) |
| E03_0_0 | 20,8 × 12,0 × 19,9 | (-41,1, 6,0, 87,7) |
| E03_0_1 | 20,8 × 12,0 × 19,9 | (-27,2, 6,0, 99,4) |
| E04_0_0_0 | 30,1 × 7,9 × 27,5 | (-78,2, 3,7, 18,8) |
| E04_0_0_1 | 30,1 × 7,9 × 27,5 | (-56,9, 3,7, 33,7) |
| E04_0_1_0 | 30,1 × 7,9 × 27,5 | (-108,0, 3,7, 61,4) |
| E04_0_1_1 | 30,1 × 7,9 × 27,5 | (-86,7, 3,7, 76,3) |
| E04_0_2_0 | 27,5 × 7,9 × 30,1 | (-96,3, 3,7, 20,8) |
| E04_0_2_1 | 27,5 × 7,9 × 30,1 | (-111,3, 3,7, 42,1) |
| E04_0_3_0 | 27,5 × 7,9 × 30,1 | (-53,7, 3,7, 50,6) |
| E04_0_3_1 | 27,5 × 7,9 × 30,1 | (-68,7, 3,7, 71,9) |
| E05_0 | 26,0 × 12,0 × 23,8 | (-31,5, 6,0, 9,8) |
| E05_1 | 23,8 × 12,0 × 26,0 | (-19,6, 6,0, 18,6) |
| E06_0 | 35,5 × 7,9 × 25,4 | (18,6, 3,7, -28,1) |
| E07_0_00 | 32,0 × 13,4 × 32,0 | (-73,3, 6,7, -69,8) |
| E07_0_01 | 32,0 × 13,4 × 32,0 | (-45,2, 6,7, -73,8) |
| E07_0_10 | 32,0 × 13,4 × 32,0 | (-69,3, 6,7, -41,8) |
| E07_0_11 | 32,0 × 13,4 × 32,0 | (-41,3, 6,7, -45,8) |
| E08_0 | 60,3 × 10,0 × 51,1 | (127,5, 5,0, -57,8) |
| E09_0_0 | 26,6 × 7,9 × 21,7 | (119,5, 3,7, -127,0) |
| E09_0_1 | 26,6 × 7,9 × 21,7 | (141,5, 3,7, -134,1) |
| E09_0_2 | 26,6 × 7,9 × 21,7 | (163,4, 3,7, -141,2) |
| E09_0_3 | 26,6 × 7,9 × 21,7 | (185,3, 3,7, -148,4) |
| E09_1_0 | 26,6 × 7,9 × 21,7 | (124,7, 3,7, -111,5) |
| E09_1_1 | 26,6 × 7,9 × 21,7 | (146,6, 3,7, -118,6) |
| E09_1_2 | 26,6 × 7,9 × 21,7 | (168,5, 3,7, -125,8) |
| E09_1_3 | 26,6 × 7,9 × 21,7 | (190,4, 3,7, -132,9) |
| E10_0_0 | 27,1 × 7,9 × 21,8 | (122,4, 3,7, -152,9) |
| E10_0_1 | 27,1 × 7,9 × 21,8 | (144,8, 3,7, -160,2) |
| E10_0_2 | 27,1 × 7,9 × 21,8 | (167,1, 3,7, -167,4) |
| E10_0_3 | 27,1 × 7,9 × 21,8 | (189,5, 3,7, -174,7) |
| E11_0 | 45,5 × 17,9 × 47,0 | (187,8, 8,9, -141,4) |
| E12_0 | 27,1 × 13,4 × 19,7 | (212,1, 6,7, -79,5) |
| E13_0_0 | 34,5 × 7,9 × 27,0 | (-155,8, 3,7, -3,8) |
| E13_0_1 | 34,5 × 7,9 × 27,0 | (-127,8, 3,7, -16,9) |
| E14_0_0 | 26,7 × 7,9 × 23,3 | (-116,1, 3,7, -17,3) |
| E14_0_1 | 26,7 × 7,9 × 23,3 | (-95,9, 3,7, -26,7) |
| E15a_0_0 | 23,1 × 7,6 × 18,7 | (-100,4, 3,8, 129,1) |
| E15a_0_1 | 23,1 × 7,6 × 18,7 | (-82,1, 3,8, 137,6) |
| E15b_0_0 | 23,1 × 7,6 × 18,7 | (-79,8, 3,8, 136,8) |
| E15b_0_1 | 23,1 × 7,6 × 18,7 | (-61,5, 3,8, 145,4) |
| E16_0_0 | 16,7 × 5,8 × 14,5 | (-134,3, 2,9, 112,7) |
| E16_0_1 | 16,7 × 5,8 × 14,5 | (-121,9, 2,9, 119,9) |
| E16_0_2 | 16,7 × 5,8 × 14,5 | (-109,4, 2,9, 127,1) |
| E16_0_3 | 16,7 × 5,8 × 14,5 | (-97,0, 2,9, 134,3) |
| E16_1_0 | 16,7 × 5,8 × 14,5 | (-125,5, 2,9, 97,3) |
| E16_1_1 | 16,7 × 5,8 × 14,5 | (-113,1, 2,9, 104,4) |
| E16_1_2 | 16,7 × 5,8 × 14,5 | (-100,7, 2,9, 111,6) |
| E16_1_3 | 16,7 × 5,8 × 14,5 | (-88,2, 2,9, 118,8) |
| E16_2_0 | 16,7 × 5,8 × 14,5 | (-116,2, 2,9, 81,8) |
| E16_2_1 | 16,7 × 5,8 × 14,5 | (-103,8, 2,9, 89,0) |
| E16_2_2 | 16,7 × 5,8 × 14,5 | (-91,4, 2,9, 96,1) |
| E16_2_3 | 16,7 × 5,8 × 14,5 | (-79,0, 2,9, 103,3) |
| E16_3_0 | 16,7 × 5,8 × 14,5 | (-107,5, 2,9, 66,3) |
| E16_3_1 | 16,7 × 5,8 × 14,5 | (-95,0, 2,9, 73,5) |
| E16_3_2 | 16,7 × 5,8 × 14,5 | (-82,6, 2,9, 80,7) |
| E16_3_3 | 16,7 × 5,8 × 14,5 | (-70,2, 2,9, 87,8) |
| E17a_0 | 26,7 × 7,9 × 22,2 | (47,7, 3,7, -9,9) |
| E17b_0_0 | 17,2 × 5,8 × 12,6 | (65,1, 2,9, -42,9) |
| E17b_0_1 | 17,2 × 5,8 × 12,6 | (79,6, 2,9, -49,6) |
| E17c_0 | 15,9 × 8,3 × 13,3 | (93,4, 4,2, -30,4) |
| E17d_0 | 13,6 × 8,3 × 9,2 | (-108,9, 4,2, -47,5) |
| E17e_0 | 13,6 × 8,3 × 9,2 | (79,5, 4,2, -25,3) |
| E17f_0 | 16,4 × 9,3 × 12,4 | (-169,8, 4,7, 83,1) |
| E18_0 | 35,7 × 13,4 × 34,8 | (92,4, 6,7, 150,2) |
| E19_0 | 3,0 × 1,7 × 2,2 | (216,2, 0,9, -35,6) |

## 5. Superfícies de piso e cobertura

Renderers candidatos a piso (baixos, grandes, próximos de Y=0): **265**.
- Área coberta pelo conjunto (bounding box, não união real): 495,9 × 332,7 m; Y [0,00, 0,25].

| Piso (amostra) | Bounds X×Y×Z (m) | Y mín |
|---|---|---|
| T06_lavoura | 134,2 × 0,05 × 98,0 | 0,00 |
| T02_t15 | 20,0 × 0,00 × 20,0 | 0,01 |
| T05_t6 | 20,0 × 0,00 × 20,0 | 0,01 |
| T05_t9 | 20,0 × 0,00 × 20,0 | 0,01 |
| T02_t4 | 20,0 × 0,00 × 20,0 | 0,01 |
| T02_t17 | 20,0 × 0,00 × 20,0 | 0,01 |
| T03_t6 | 20,0 × 0,00 × 20,0 | 0,01 |
| R02_t2 | 20,0 × 0,00 × 20,0 | 0,02 |
| T02_t5 | 20,0 × 0,00 × 20,0 | 0,01 |
| T02_t14 | 20,0 × 0,00 × 20,0 | 0,01 |
| T05_t11 | 20,0 × 0,00 × 20,0 | 0,01 |
| T05_t4 | 20,0 × 0,00 × 20,0 | 0,01 |
| T09_t3 | 20,0 × 0,00 × 20,0 | 0,02 |
| T03_t2 | 20,0 × 0,00 × 20,0 | 0,01 |
| T09_t9 | 20,0 × 0,00 × 20,0 | 0,02 |
| T05_t3 | 20,0 × 0,00 × 20,0 | 0,01 |
| T05_t10 | 20,0 × 0,00 × 20,0 | 0,01 |
| R02_t3 | 20,0 × 0,00 × 20,0 | 0,02 |
| R02_t7 | 20,0 × 0,00 × 20,0 | 0,02 |
| T02_t6 | 20,0 × 0,00 × 20,0 | 0,01 |

- Cobertura por amostragem de 2 m dentro da bounding box: **14731/41416** pontos (35,6%).
- Lacunas são **estimativas por bounds**, não prova de vão real (módulos podem se sobrepor).
- Primeiros pontos sem piso: (-256,5, -162,7), (-256,5, -160,7), (-256,5, -158,7), (-256,5, -156,7), (-256,5, -154,7), (-256,5, -152,7), (-256,5, -150,7), (-256,5, -148,7), (-256,5, -146,7), (-256,5, -144,7), (-256,5, -142,7), (-256,5, -140,7), ….

## 6. Catálogo de prefabs (metadados reais)

Prefabs distintos usados na cena: **70**. Candidatos da seleção: **23**. Inspecionados: **92**.

Legenda de confiança: **alta** = caminho/nome + renderer coerentes; **média** = categoria inferida apenas por nome/pacote; **baixa** = ambíguo ou sem renderer → revisão humana.

| Prefab | Pacote | Usos na cena | Renderers | Bounds X×Y×Z (m) | Meshes (amostra) | Materiais (shader; cor; smooth/metallic) | Colliders | Categoria inferida | Confiança |
|---|---|---:|---:|---|---|---|---|---|---|
| SM_Flower_01 | BluBlu Games | 0 | 2 | 0,29 × 0,40 × 0,29 | SM_Flower_05 | M_Foliage [BluBlu/SG_Foliage]<br>M_Foliage_LOD [BluBlu/SG_Foliage_LOD] | nenhum | Vegetação | alta |
| SM_FoliageBush_01 | BluBlu Games | 0 | 1 | 1,82 × 1,41 × 1,56 | SM_FoliageBush_01 | Bush_01 1 [BluBlu/SG_Foliage] | nenhum | Vegetação | alta |
| SM_Grass_01 | BluBlu Games | 0 | 1 | 0,54 × 0,54 × 0,37 | SM_Grass_03 | M_Grass_01 [BluBlu/SG_Foliage] | nenhum | Vegetação | alta |
| SM_Mushroom_01 | BluBlu Games | 0 | 1 | 0,26 × 0,24 × 0,27 | SM_Mushroom_03 | M_Mushroom [BluBlu/SG_Lit] smooth=0,00 | nenhum | Vegetação | alta |
| SM_Tree_Green | BluBlu Games | 0 | 2 | 4,38 × 7,62 × 3,87 | SM_Tree_01 | Tree_Leaves_A [BluBlu/SG_Foliage]<br>TreeBark [BluBlu/SG_Foliage]<br>…(+1) | 1 (CapsuleCollider×1) | Vegetação | alta |
| Road_Sand | CubexCube - Free City Pack I | 107 | 1 | 13,00 × 0,00 × 13,00 | Road_Sand | Texture_1 [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,20 metal=0,20 tex=Texture_1 | nenhum | Superfície | alta |
| commercial-building-concert-hall | Loading Games | 1 | 1 | 34,82 × 17,89 × 24,92 | commercial-building-convention/concert-hall | concert-hall-windows_mat [Standard] Color=RGBA(0.906, 0.906, 0.906, 1.000) smooth=0,50 metal=0,00<br>main-color-palette_mat [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,00 metal=0,00 tex=color-palette-2 | nenhum | Edifício/Componente | alta |
| government-building-museum | Loading Games | 2 | 1 | 18,20 × 11,97 × 10,72 | government-building-museum | main-color-palette_mat [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,00 metal=0,00 tex=color-palette-2<br>museum-texture_mat [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=museum-texture<br>…(+2) | nenhum | Edifício/Componente | alta |
| government-building-school | Loading Games | 2 | 1 | 18,55 × 11,97 × 11,22 | government-building-school | school-windows_mat [Standard] Color=RGBA(0.906, 0.906, 0.906, 1.000) smooth=0,50 metal=0,00<br>main-color-palette_mat [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,00 metal=0,00 tex=color-palette-2<br>…(+1) | nenhum | Edifício/Componente | alta |
| industrial-buildings-building-C | Loading Games | 6 | 1 | 24,60 × 13,40 × 24,60 | industrial-buildings-building-C | main-color-palette_mat [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,00 metal=0,00 tex=color-palette-2<br>industrial-building-D-windows_mat [Standard] Color=RGBA(0.444, 0.444, 0.444, 1.000) smooth=0,50 metal=0,00 | nenhum | Edifício/Componente | alta |
| SM_LPC_Wall_CornerExternal_01 | LowPolyCityFree | 32 | 1 | 0,36 × 3,00 × 0,36 | SM_LPC_Wall_CornerExternal_01 | M_LPC_Main [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,18 metal=0,00 tex=LPC_Palette | 1 (MeshCollider×1) | Edifício/Componente | média |
| SM_LPC_Wall_WallHouseDoor_01 | LowPolyCityFree | 4 | 1 | 4,00 × 3,00 × 0,36 | SM_LPC_Wall_WallHouseDoor_01 | M_LPC_Main [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,18 metal=0,00 tex=LPC_Palette | 1 (MeshCollider×1) | Edifício/Componente | média |
| SM_LPC_Wall_WallHouseUpperWindow_01 | LowPolyCityFree | 34 | 1 | 4,00 × 3,00 × 0,40 | SM_LPC_Wall_WallHouseUpperWindow_01 | M_LPC_Main [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,18 metal=0,00 tex=LPC_Palette<br>M_LPC_Glass [Standard] Color=RGBA(1.000, 1.000, 1.000, 0.420) smooth=0,88 metal=0,00 tex=LPC_Palette_Glass | 1 (MeshCollider×1) | Edifício/Componente | média |
| SM_LPC_Wall_WallHouseUpperWindowWide_01 | LowPolyCityFree | 10 | 1 | 4,00 × 3,00 × 0,40 | SM_LPC_Wall_WallHouseUpperWindowWide_01 | M_LPC_Main [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,18 metal=0,00 tex=LPC_Palette<br>M_LPC_Glass [Standard] Color=RGBA(1.000, 1.000, 1.000, 0.420) smooth=0,88 metal=0,00 tex=LPC_Palette_Glass | 1 (MeshCollider×1) | Edifício/Componente | média |
| SM_LPC_Wall_WallHouseWindow_01 | LowPolyCityFree | 40 | 1 | 4,00 × 3,00 × 0,40 | SM_LPC_Wall_WallHouseWindow_01 | M_LPC_Main [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,18 metal=0,00 tex=LPC_Palette<br>M_LPC_Glass [Standard] Color=RGBA(1.000, 1.000, 1.000, 0.420) smooth=0,88 metal=0,00 tex=LPC_Palette_Glass | 1 (MeshCollider×1) | Edifício/Componente | média |
| SM_LPC_Roof_RoofRakeApex_01 | LowPolyCityFree | 8 | 1 | 4,00 × 1,20 × 0,40 | SM_LPC_Roof_RoofRakeApex_01 | M_LPC_Main [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,18 metal=0,00 tex=LPC_Palette | 1 (MeshCollider×1) | Edifício/Componente | média |
| SM_LPC_Roof_RoofRakeCourse_01 | LowPolyCityFree | 4 | 1 | 4,00 × 1,00 × 0,36 | SM_LPC_Roof_RoofRakeCourse_01 | M_LPC_Main [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,18 metal=0,00 tex=LPC_Palette | 1 (MeshCollider×1) | Edifício/Componente | média |
| SM_LPC_Roof_RoofRakeCourseVent_01 | LowPolyCityFree | 8 | 1 | 4,00 × 1,00 × 0,36 | SM_LPC_Roof_RoofRakeCourseVent_01 | M_LPC_Main [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,18 metal=0,00 tex=LPC_Palette | 1 (MeshCollider×1) | Edifício/Componente | média |
| SM_LPC_Roof_RoofRakePanel_01 | LowPolyCityFree | 4 | 1 | 2,00 × 1,14 × 0,36 | SM_LPC_Roof_RoofRakePanel_01 | M_LPC_Main [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,18 metal=0,00 tex=LPC_Palette | 1 (MeshCollider×1) | Edifício/Componente | média |
| SM_LPC_Roof_RoofRakeStart_01 | LowPolyCityFree | 16 | 1 | 2,00 × 1,14 × 0,40 | SM_LPC_Roof_RoofRakeStart_01 | M_LPC_Main [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,18 metal=0,00 tex=LPC_Palette | 1 (MeshCollider×1) | Edifício/Componente | média |
| SM_LPC_Roof_RoofRidgeStraight_01 | LowPolyCityFree | 13 | 1 | 4,00 × 0,18 × 0,36 | SM_LPC_Roof_RoofRidgeStraight_01 | M_LPC_Main [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,18 metal=0,00 tex=LPC_Palette | 1 (MeshCollider×1) | Edifício/Componente | média |
| SM_LPC_Roof_RoofSlopePanel_01 | LowPolyCityFree | 60 | 1 | 4,00 × 1,34 × 2,00 | SM_LPC_Roof_RoofSlopePanel_01 | M_LPC_Main [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,18 metal=0,00 tex=LPC_Palette | 1 (MeshCollider×1) | Edifício/Componente | média |
| SM_LPC_Open_DoorHousePanel_01 | LowPolyCityFree | 4 | 1 | 0,89 × 2,09 × 0,11 | SM_LPC_Open_DoorHousePanel_01 | M_LPC_Main [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,18 metal=0,00 tex=LPC_Palette | 1 (MeshCollider×1) | Edifício/Componente | média |
| SM_LPC_Open_WinCasementSingle_01 | LowPolyCityFree | 40 | 1 | 1,18 × 1,38 × 0,09 | SM_LPC_Open_WinCasementSingle_01 | M_LPC_Main [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,18 metal=0,00 tex=LPC_Palette<br>M_LPC_Glass [Standard] Color=RGBA(1.000, 1.000, 1.000, 0.420) smooth=0,88 metal=0,00 tex=LPC_Palette_Glass | 1 (MeshCollider×1) | Edifício/Componente | média |
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
| BLD_LPC_HouseDetached | LowPolyCityFree | 3 | 61 | 12,40 × 8,34 × 8,40 | SM_LPC_Wall_WallHouseWindow_01, SM_LPC_Open_WinCasementSingle_01, SM_LPC_Wall_WallHouseDoor_01 (+9) | M_LPC_Main [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,18 metal=0,00 tex=LPC_Palette<br>M_LPC_Glass [Standard] Color=RGBA(1.000, 1.000, 1.000, 0.420) smooth=0,88 metal=0,00 tex=LPC_Palette_Glass | 61 (MeshCollider×61) | Edifício/Componente | alta |
| BLD_LPC_HouseLarge | LowPolyCityFree | 1 | 94 | 16,40 × 9,34 × 12,40 | SM_LPC_Wall_WallHouseWindow_01, SM_LPC_Open_WinCasementSingle_01, SM_LPC_Wall_WallHouseDoor_01 (+11) | M_LPC_Main [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,18 metal=0,00 tex=LPC_Palette<br>M_LPC_Glass [Standard] Color=RGBA(1.000, 1.000, 1.000, 0.420) smooth=0,88 metal=0,00 tex=LPC_Palette_Glass | 94 (MeshCollider×94) | Edifício/Componente | alta |
| Caio_LowPoly_Corrigido | Modelos | 68 | 1 | 1,70 × 1,84 × 0,66 | geometry_0 | pbr_material [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,40 metal=0,00 tex=Caio_Textura | nenhum | Indefinido | baixa |
| Skate_V2 | Modelos | 1 | 1 | 0,22 × 0,18 × 0,80 | SkateMesh | tripo_mat_0353840c [Standard] Color=RGBA(0.800, 0.800, 0.800, 1.000) smooth=0,00 metal=0,00 tex=tripo_image_0353840c_0 | nenhum | Indefinido | baixa |
| dog house | Palmov Island | 1 | 1 | 3,04 × 1,75 × 2,21 | dog_house | mat main [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,00 metal=0,00 tex=texture_main | 1 (BoxCollider×1) | Edifício/Componente | média |
| Env_ResidentBuilding_06 | Pandazole_Ultimate_Pack | 18 | 1 | 15,94 × 5,77 × 6,52 | Env_ResidentBuilding_06 | PandaMat [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=PandaMat | 1 (MeshCollider×1) | Edifício/Componente | média |
| Env_Road_Straight_01 | Pandazole_Ultimate_Pack | 80 | 1 | 10,00 × 0,20 × 10,00 | Env_Road_Straight_01 | PandaMat [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=PandaMat | 1 (MeshCollider×1) | Superfície | alta |
| HardRock_01 | Pandazole_Ultimate_Pack | 0 | 1 | 0,65 × 1,17 × 0,77 | HardRock_01 | PandaMat [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=PandaMat | 1 (MeshCollider×1) | Indefinido | baixa |
| SoftRock_01 | Pandazole_Ultimate_Pack | 0 | 1 | 0,84 × 0,39 × 0,87 | SoftRock_01 | PandaMat [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=PandaMat | 1 (MeshCollider×1) | Indefinido | baixa |
| Building_House_04_color01 | SimplePoly City - Low Poly Assets | 38 | 1 | 24,77 × 7,87 × 15,32 | Building_House_04_color01 | Building_House_04_color01 [Legacy Shaders/Diffuse] Color=RGBA(1.000, 1.000, 1.000, 1.000) tex=Building_House_04_color01 | nenhum | Edifício/Componente | alta |
| Building_Super Market | SimplePoly City - Low Poly Assets | 6 | 1 | 23,75 × 7,58 × 13,25 | Building_Super Market | Building_Super Market [Legacy Shaders/Diffuse] Color=RGBA(1.000, 1.000, 1.000, 1.000) tex=Building_Super Market | nenhum | Edifício/Componente | alta |
| Natures_Grass Tile | SimplePoly City - Low Poly Assets | 66 | 1 | 20,00 × 0,00 × 20,00 | Natures_Grass Tile | Natures [Legacy Shaders/Diffuse] Color=RGBA(1.000, 1.000, 1.000, 1.000) tex=Natures | nenhum | Superfície | alta |
| Road Concrete Tile Small | SimplePoly City - Low Poly Assets | 0 | 1 | 10,00 × 0,00 × 10,00 | Road Concrete Tile Small | Road [Legacy Shaders/Diffuse] Color=RGBA(1.000, 1.000, 1.000, 1.000) tex=Road | nenhum | Superfície | alta |
| Road Concrete Tile | SimplePoly City - Low Poly Assets | 10 | 1 | 20,00 × 0,00 × 20,00 | Road Concrete Tile | Road [Legacy Shaders/Diffuse] Color=RGBA(1.000, 1.000, 1.000, 1.000) tex=Road | nenhum | Superfície | alta |
| Road Sidewalk | SimplePoly City - Low Poly Assets | 0 | 1 | 3,00 × 0,20 × 10,00 | Road Sidewalk | Road [Legacy Shaders/Diffuse] Color=RGBA(1.000, 1.000, 1.000, 1.000) tex=Road | nenhum | Superfície | alta |
| S_Tree_A_LOD_1 | Vegetation_Stylized_Pack_ByLuxArtStudios | 40 | 1 | 5,21 × 7,42 × 5,04 | S_Tree_A_LOD1 | MI_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=T_Trunk_D<br>MI_Leaf [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 | nenhum | Vegetação | média |
| S_Tree_A_LOD_2 | Vegetation_Stylized_Pack_ByLuxArtStudios | 40 | 1 | 5,21 × 7,42 × 5,04 | S_Tree_A_LOD2 | MI_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=T_Trunk_D<br>MI_Leaf [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 | nenhum | Vegetação | média |
| S_Tree_B_LOD_1 | Vegetation_Stylized_Pack_ByLuxArtStudios | 51 | 1 | 9,24 × 9,09 × 7,46 | S_Tree_B_LOD_1 | MI_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=T_Trunk_D<br>MI_Leaf [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 | nenhum | Vegetação | média |
| S_Tree_B_LOD_2 | Vegetation_Stylized_Pack_ByLuxArtStudios | 51 | 1 | 9,21 × 9,04 × 7,46 | S_Tree_B_LOD_2 | MI_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=T_Trunk_D<br>MI_Leaf [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 | nenhum | Vegetação | média |
| S_Tree_C_LOD_1 | Vegetation_Stylized_Pack_ByLuxArtStudios | 46 | 1 | 7,71 × 8,24 × 5,80 | S_Tree_C_LOD_1 | MI_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=T_Trunk_D<br>MI_Leaf [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 | nenhum | Vegetação | média |
| S_Tree_C_LOD_2 | Vegetation_Stylized_Pack_ByLuxArtStudios | 46 | 1 | 7,71 × 8,24 × 5,80 | S_Tree_C_LOD_2 | MI_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=T_Trunk_D<br>MI_Leaf [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 | nenhum | Vegetação | média |
| S_Tree_D_LOD_1 | Vegetation_Stylized_Pack_ByLuxArtStudios | 39 | 1 | 6,91 × 6,50 × 7,44 | S_Tree_D_1 | MI_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=T_Trunk_D<br>MI_Leaf [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 | nenhum | Vegetação | média |
| S_Tree_D_LOD_2 | Vegetation_Stylized_Pack_ByLuxArtStudios | 39 | 1 | 6,90 × 6,49 × 7,42 | S_Tree_D_2 | MI_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=T_Trunk_D<br>MI_Leaf [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 | nenhum | Vegetação | média |
| S_Tree_E_LOD_1 | Vegetation_Stylized_Pack_ByLuxArtStudios | 45 | 1 | 6,29 × 7,47 × 7,88 | S_Tree_E_LOD_1 | MI_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=T_Trunk_D<br>MI_Leaf [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 | nenhum | Vegetação | média |
| S_Tree_E_LOD_2 | Vegetation_Stylized_Pack_ByLuxArtStudios | 45 | 1 | 6,29 × 7,47 × 7,88 | S_Tree_E_LOD_2 | MI_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=T_Trunk_D<br>MI_Leaf [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 | nenhum | Vegetação | média |
| S_Tree_F_LOD_1 | Vegetation_Stylized_Pack_ByLuxArtStudios | 51 | 1 | 3,91 × 5,07 × 3,65 | S_Tree_F_LOD_1 | MI_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=T_Trunk_D<br>MI_Leaf [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 | nenhum | Vegetação | média |
| S_Tree_F_LOD_2 | Vegetation_Stylized_Pack_ByLuxArtStudios | 51 | 1 | 3,91 × 5,07 × 3,65 | S_Tree_F_LOD_2 | MI_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=T_Trunk_D<br>MI_Leaf [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 | nenhum | Vegetação | média |
| S_Tree_G_LOD_1 | Vegetation_Stylized_Pack_ByLuxArtStudios | 41 | 1 | 6,49 × 7,37 × 5,62 | S_Tree_G_LOD_1 | MI_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=T_Trunk_D<br>MI_Leaf [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 | nenhum | Vegetação | média |
| S_Tree_G_LOD_2 | Vegetation_Stylized_Pack_ByLuxArtStudios | 41 | 1 | 6,49 × 7,31 × 5,62 | S_Tree_G_LOD_2 | MI_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=T_Trunk_D<br>MI_Leaf [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 | nenhum | Vegetação | média |
| S_Tree_H_LOD_1 | Vegetation_Stylized_Pack_ByLuxArtStudios | 41 | 1 | 6,51 × 8,40 × 5,80 | S_Tree_H_LOD_1 | MI_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=T_Trunk_D<br>MI_Leaf [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 | nenhum | Vegetação | média |
| S_Tree_H_LOD_2 | Vegetation_Stylized_Pack_ByLuxArtStudios | 41 | 1 | 6,51 × 8,40 × 5,80 | S_Tree_H_LOD_2 | MI_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=T_Trunk_D<br>MI_Leaf [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 | nenhum | Vegetação | média |
| S_Tree_I_LOD_1 | Vegetation_Stylized_Pack_ByLuxArtStudios | 42 | 1 | 7,13 × 7,04 × 4,91 | S_Tree_I_LOD_1 | MI_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=T_Trunk_D<br>MI_leaf [Standard] Color=RGBA(0.800, 0.800, 0.800, 1.000) smooth=0,50 metal=0,00 | nenhum | Vegetação | média |
| S_Tree_I_LOD_2 | Vegetation_Stylized_Pack_ByLuxArtStudios | 42 | 1 | 7,13 × 7,04 × 4,91 | S_Tree_I_LOD_2 | MI_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=T_Trunk_D<br>MI_leaf [Standard] Color=RGBA(0.800, 0.800, 0.800, 1.000) smooth=0,50 metal=0,00 | nenhum | Vegetação | média |
| S_Tree_J_LOD_1 | Vegetation_Stylized_Pack_ByLuxArtStudios | 4 | 1 | 11,18 × 14,58 × 14,39 | S_Tree_J_LOD_1 | MI_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=T_Trunk_D<br>MI_Leaf [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 | nenhum | Vegetação | média |
| S_Tree_J_LOD_2 | Vegetation_Stylized_Pack_ByLuxArtStudios | 4 | 1 | 11,18 × 14,58 × 14,39 | S_Tree_J_LOD_2 | MI_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=T_Trunk_D<br>MI_Leaf [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 | nenhum | Vegetação | média |
| S_Tree_A | Vegetation_Stylized_Pack_ByLuxArtStudios | 40 | 1 | 5,21 × 7,42 × 5,04 | S_Tree_A | MI_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=T_Trunk_D<br>MI_Leaf [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 | nenhum | Vegetação | média |
| S_Tree_B | Vegetation_Stylized_Pack_ByLuxArtStudios | 51 | 1 | 9,25 × 9,10 × 7,46 | S_Tree_B | MI_Leaf [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00<br>MI_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=T_Trunk_D | nenhum | Vegetação | média |
| S_Tree_C | Vegetation_Stylized_Pack_ByLuxArtStudios | 46 | 1 | 7,74 × 8,24 × 5,80 | S_Tree_C | MI_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=T_Trunk_D<br>MI_Leaf [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 | nenhum | Vegetação | média |
| S_Tree_D | Vegetation_Stylized_Pack_ByLuxArtStudios | 39 | 1 | 7,03 × 6,53 × 7,54 | S_Tree_D | MI_Leaf [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00<br>MI_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=T_Trunk_D | nenhum | Vegetação | média |
| S_Tree_E | Vegetation_Stylized_Pack_ByLuxArtStudios | 45 | 1 | 6,29 × 7,47 × 7,88 | S_Tree_E | MI_Leaf [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00<br>MI_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=T_Trunk_D | nenhum | Vegetação | média |
| S_Tree_F | Vegetation_Stylized_Pack_ByLuxArtStudios | 51 | 1 | 3,91 × 5,07 × 3,65 | S_Tree_F | MI_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=T_Trunk_D<br>MI_Leaf [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 | nenhum | Vegetação | média |
| S_Tree_G | Vegetation_Stylized_Pack_ByLuxArtStudios | 41 | 1 | 6,49 × 7,37 × 5,62 | S_Tree_G | MI_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=T_Trunk_D<br>MI_Leaf [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 | nenhum | Vegetação | média |
| S_Tree_H | Vegetation_Stylized_Pack_ByLuxArtStudios | 41 | 1 | 6,51 × 8,40 × 5,80 | S_Tree_H | MI_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=T_Trunk_D<br>MI_Leaf [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 | nenhum | Vegetação | média |
| S_Tree_I | Vegetation_Stylized_Pack_ByLuxArtStudios | 42 | 1 | 7,13 × 7,04 × 4,91 | S_Tree_I | MI_leaf [Standard] Color=RGBA(0.800, 0.800, 0.800, 1.000) smooth=0,50 metal=0,00<br>MI_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=T_Trunk_D | nenhum | Vegetação | média |
| S_Tree_J | Vegetation_Stylized_Pack_ByLuxArtStudios | 4 | 1 | 11,20 × 14,58 × 14,40 | S_Tree_J | MI_Leaf [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00<br>MI_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,50 metal=0,00 tex=T_Trunk_D | nenhum | Vegetação | média |
| S_Tree_A | Vegetation_Stylized_Pack_ByLuxArtStudios | 40 | 3 | 5,21 × 7,42 × 5,05 | S_Tree_A, S_Tree_A_LOD1, S_Tree_A_LOD2 | M_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,00 metal=0,00 tex=T_Trunk_D<br>S_Tree_01A [Custom/S_ShaderTree] Color=RGBA(1.000, 0.602, 0.137, 1.000) tex=Leaf_2 | 1 (CapsuleCollider×1) | Vegetação | média |
| S_Tree_B | Vegetation_Stylized_Pack_ByLuxArtStudios | 51 | 3 | 9,25 × 9,10 × 7,46 | S_Tree_B, S_Tree_B_LOD_2, S_Tree_B_LOD_1 | S_Tree_02A [Custom/S_ShaderTree] Color=RGBA(0.388, 0.575, 0.117, 1.000) tex=Leaf_4<br>M_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,00 metal=0,00 tex=T_Trunk_D | 1 (CapsuleCollider×1) | Vegetação | média |
| S_Tree_C | Vegetation_Stylized_Pack_ByLuxArtStudios | 46 | 3 | 7,74 × 8,24 × 5,80 | S_Tree_C, S_Tree_C_LOD_1, S_Tree_C_LOD_2 | M_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,00 metal=0,00 tex=T_Trunk_D<br>S_Tree_03A [Custom/S_ShaderTree] Color=RGBA(1.000, 0.599, 0.860, 1.000) tex=Leaf_4 | 1 (CapsuleCollider×1) | Vegetação | média |
| S_Tree_D | Vegetation_Stylized_Pack_ByLuxArtStudios | 39 | 3 | 7,03 × 6,53 × 7,54 | S_Tree_D, S_Tree_D_1, S_Tree_D_2 | S_Tree_03A [Custom/S_ShaderTree] Color=RGBA(1.000, 0.599, 0.860, 1.000) tex=Leaf_4<br>M_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,00 metal=0,00 tex=T_Trunk_D | 1 (CapsuleCollider×1) | Vegetação | média |
| S_Tree_E | Vegetation_Stylized_Pack_ByLuxArtStudios | 45 | 3 | 6,29 × 7,47 × 7,88 | S_Tree_E, S_Tree_E_LOD_1, S_Tree_E_LOD_2 | S_Tree_04A [Custom/S_ShaderTree] Color=RGBA(0.137, 0.509, 0.296, 1.000) tex=Leaf_4<br>M_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,00 metal=0,00 tex=T_Trunk_D | 1 (CapsuleCollider×1) | Vegetação | média |
| S_Tree_F | Vegetation_Stylized_Pack_ByLuxArtStudios | 51 | 3 | 3,91 × 5,07 × 3,65 | S_Tree_F, S_Tree_F_LOD_1, S_Tree_F_LOD_2 | M_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,00 metal=0,00 tex=T_Trunk_D<br>S_Tree_06A [Custom/S_ShaderTree] Color=RGBA(1.000, 0.602, 0.137, 1.000) tex=Leaf_2 | 1 (CapsuleCollider×1) | Vegetação | média |
| S_Tree_G | Vegetation_Stylized_Pack_ByLuxArtStudios | 41 | 3 | 6,49 × 7,37 × 5,62 | S_Tree_G, S_Tree_G_LOD_1, S_Tree_G_LOD_2 | M_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,00 metal=0,00 tex=T_Trunk_D<br>S_Tree_05A [Custom/S_ShaderTree] Color=RGBA(1.000, 0.830, 0.000, 1.000) tex=Leaf_1 | 1 (CapsuleCollider×1) | Vegetação | média |
| S_Tree_H | Vegetation_Stylized_Pack_ByLuxArtStudios | 41 | 3 | 6,51 × 8,40 × 5,80 | S_Tree_H, S_Tree_H_LOD_1, S_Tree_H_LOD_2 | M_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,00 metal=0,00 tex=T_Trunk_D<br>S_Tree_04A [Custom/S_ShaderTree] Color=RGBA(0.137, 0.509, 0.296, 1.000) tex=Leaf_4 | 1 (CapsuleCollider×1) | Vegetação | média |
| S_Tree_I | Vegetation_Stylized_Pack_ByLuxArtStudios | 42 | 3 | 7,13 × 7,04 × 4,91 | S_Tree_I, S_Tree_I_LOD_1, S_Tree_I_LOD_2 | S_Tree_06A [Custom/S_ShaderTree] Color=RGBA(1.000, 0.602, 0.137, 1.000) tex=Leaf_2<br>M_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,00 metal=0,00 tex=T_Trunk_D | 1 (CapsuleCollider×1) | Vegetação | média |
| S_Tree_J | Vegetation_Stylized_Pack_ByLuxArtStudios | 4 | 3 | 11,20 × 14,58 × 14,40 | S_Tree_J, S_Tree_J_LOD_1, S_Tree_J_LOD_2 | S_Tree_04A [Custom/S_ShaderTree] Color=RGBA(0.137, 0.509, 0.296, 1.000) tex=Leaf_4<br>M_Trunk [Standard] Color=RGBA(1.000, 1.000, 1.000, 1.000) smooth=0,00 metal=0,00 tex=T_Trunk_D | 1 (CapsuleCollider×1) | Vegetação | média |

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


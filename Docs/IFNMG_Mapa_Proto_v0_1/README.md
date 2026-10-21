# IFNMG 360° — mapa protótipo v0.1

## O que é

Este pacote cria um **blockout 3D editável** para o campus na Unity, usando coordenadas numéricas do arquivo JSON. É uma primeira versão de trabalho: pode ser corrigida conforme o desenvolvimento; não é um levantamento topográfico nem uma reprodução arquitetônica final.

O gerador não interpreta imagens. Ele lê o JSON, converte coordenadas normalizadas para unidades Unity e cria geometria de terreno, áreas classificadas, vias, volumes simplificados dos prédios e uma quantidade controlada de vegetação/detalhes. As imagens são apenas para a conferência visual humana.

## Dimensões provisórias

- Perímetro representado: aproximadamente **440 × 310 m**.
- Convenção: **1 unidade Unity = 1 metro**.
- Norte proposto: **+Z**.
- Origem: centro do perímetro representado.
- Mapa completo fora do perímetro: não é modelado nesta v0.1.
- A medição dos Jogos Analógicos (área 1.480,14 m², perímetro 154,80 m) está registrada como referência futura, mas ainda não foi usada para validar a escala porque falta o polígono numérico exato desse prédio.

O perímetro de 440 × 310 m possui cerca de 1.500 m quando tratado como retângulo, coerente como sanity check com o perímetro de referência de aproximadamente 1,49 km. Isso não valida todos os detalhes nem a escala dos edifícios.

## Conteúdo

- `Assets/IFNMG360/Config/IFNMG_Mapa_Proto_v0_1.json`: dados editáveis, polígonos, linhas, zonas, escala provisória e assets candidatos.
- `Assets/Editor/IFNMGPrototypeMapGenerator.cs`: ferramenta de Editor para gerar o blockout em uma **cena separada**.
- `Preview/IFNMG_Mapa_Proto_v0_1_planta.png`: vista superior esquemática gerada dos mesmos dados do JSON; serve para o usuário conferir, não para o gerador interpretar.

## Instalação no projeto Unity

1. Faça uma cópia/commit do projeto antes de começar.
2. Copie a pasta `Assets/Editor/` para a pasta `Assets/Editor/` do projeto.
3. Copie `Assets/IFNMG360/` para `Assets/IFNMG360/` do projeto.
4. Aguarde a Unity terminar a compilação.
5. No menu superior, escolha `Tools > IFNMG 360 > 1 - Gerar mapa protótipo (cena separada)`.
6. O gerador cria ou atualiza `Assets/IFNMG360/Scenes/IFNMG_Mapa_Prototype_v0_1.unity`, mantendo a cena principal separada.
7. Abra a cena de protótipo e examine a vista superior e a perspectiva 3D.

Para remover apenas o mapa gerado da cena ativa, use `Tools > IFNMG 360 > 2 - Limpar mapa gerado da cena ativa`.

## Assets

O gerador tenta carregar os prefabs pelos caminhos exatos do catálogo. Se não encontrar um prefab, cria um fallback geométrico simples e registra um aviso no Console. Não baixa assets nem altera a aparência/material de prefabs originais. As vias e os pisos do blockout são malhas contínuas procedurais, não peças modulares, para minimizar frestas na primeira versão.

O protótipo usa um volume simplificado por footprint nas edificações. Isso serve para testar ocupação espacial; não afirma que a forma, altura ou cor de cada edifício já estejam corretas.

## Correções conhecidas/limitações

- `pavimento_estacionamento_leste` está desativado porque se sobrepunha à zona `edificio_ead_cobertura_azul`. A correção segue o aviso do usuário de que algumas regiões antes coloridas como via/pavimento são na verdade prédios.
- `via_sob_copas_oeste_sudoeste` foi adicionada com baixa confiança a partir da marcação da via sob as árvores. O traçado é aproximado e editável no JSON.
- Algumas geometrias antigas ainda podem ter interseções ou footprints aproximados. O gerador registra avisos de bounds no Console; esses avisos precisam de revisão humana.
- Mobiliário (bancos, postes, lixeiras) não é colocado automaticamente na v0.1 porque as âncoras exatas ainda não foram definidas.
- A ferramenta não examina imagens. Confira a planta e a cena na Unity; corrija os pontos numéricos no JSON e gere novamente.

## Como corrigir posições

Cada zona possui um `id`. `pointsUvFlat` guarda pares `u,v` normalizados no quadrilátero do perímetro. Ajuste os números no JSON, mantendo a quantidade de pares. A conversão usada é:

```text
X = (u - 0.5) * worldWidthMeters
Z = (0.5 - v) * worldDepthMeters
Y = 0 no terreno
```

As linhas também possuem `widthMeters`; as edificações possuem `buildingHeightMeters` e `roofStyle`. Não altere o código para mudar uma coordenada: edite o JSON e regenere a cena.

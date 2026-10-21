# Prompt OpenCode — ambientação procedural do IFNMG 360°

Você tem acesso ao projeto Unity e à tabela completa `PREFABS_TABELA.md`. Antes de editar a cena, leia `IFNMG_Selecao_Assets_Ambientacao.json` e `IFNMG_Selecao_Assets_Ambientacao.md`.

## Objetivo
Preencher áreas vazias com ambientação coerente, evitando vãos visuais, mas preservando a planta do campus e sem colocar árvores, rochas, grama ou fungos em vias, calçadas, estacionamentos, acessos ou edifícios. Não substitua as estruturas reais do campus por prédios urbanos genéricos.

## Etapa 1 — auditar a cena
- Descubra qual cena está ativa e quais terrenos, malhas, edifícios, caminhos, colliders e materiais já existem.
- Não assuma que X/Z do mapa normalizado já correspondem ao mundo Unity. Calibre a transformação mapa→mundo com as dimensões/origem/rotação corretas e exponha esses parâmetros no Editor.
- Inspecione os prefabs citados por caminho completo. Confirme se têm renderer, materiais, colliders e escala plausível. Nome sozinho não prova aparência.
- Crie uma lista de zonas permitidas e proibidas. Se não houver máscaras confiáveis de gramado/mata/pavimento/footprint, não espalhe automaticamente; crie gizmos/polígonos editáveis para confirmação.

## Etapa 2 — montar preenchimento sem vãos
1. Garantir cobertura de chão contínua no terreno, usando o Terrain existente quando possível.
2. Resolver caminhos e áreas pavimentadas como malha/superfície contínua. Avalie `Road Concrete Tile`, `Road Concrete Tile Small`, `Road Sidewalk`, `SM_LPC_Ground_WalkPavingPlain_01` e `SM_LPC_Ground_WalkCornerExternal_01`, mas não use módulos grandes como segmentos pequenos sem validar o encaixe.
3. Usar árvores nas manchas arbóreas da referência, sem invadir a via nem fechar gramados abertos.
4. Inserir arbustos, canteiros e tufos em grupos irregulares nas bordas dos gramados, junto a árvores e perto de limites naturais.
5. Usar rochas apenas em bordas naturais/taludes. Fungos são opcionais e muito esparsos, sob árvores/borda de mata — não são cobertura do solo.
6. Posicionar bancos, lixeiras e luminárias em áreas de convivência e circulação, sem bloquear rotas.

## Prefabs candidatos
Use os caminhos no JSON. Priorize `LowPolyCityFree` para árvores/arbustos/canteiros/mobiliário e avalie `SimplePoly City - Low Poly Assets` para pavimento. Para bordas de mata, o pack `BluBlu Games/Low Poly Forest Mini Pack` contém `SM_Grass_01`, `SM_FoliageBush_01`, `SM_Flower_01`, `SM_Mushroom_01`, `SM_Rock_01` e `SM_Tree_Green`; use somente se a aparência combinar. Para rochas alternativas, avalie `Pandazole Nature Environment Pack` (`HardRock_01`, `SoftRock_01`).

## Requisitos obrigatórios
- Escrever uma ferramenta no Editor, não um script de runtime que gere objetos ao entrar em Play Mode.
- Implementar `Preview`, `Apply`, `Clear Generated` e `Rebuild`, com seed reprodutível.
- Parentear os objetos gerados sob uma raiz `IFNMG_GeneratedEnvironment` e separar subgrupos `Ground`, `Roads`, `Trees`, `Undergrowth`, `Rocks`, `Mushrooms`, `StreetFurniture`.
- Usar raycast para localizar o terreno e bounds/colliders para impedir interseções com prédios, árvores existentes, objetos importantes e vias.
- Não mover nem excluir objetos existentes. `Clear Generated` deve remover só objetos criados pela ferramenta.
- Registrar prefab, posição, rotação, escala e zona em um relatório de saída.
- Começar por uma pequena área de teste. Não gerar o campus todo até eu validar a prévia.
- Se um prefab tiver escala, material, estilo ou collider inadequado, registrar isso e sugerir alternativa existente no inventário em vez de forçar o uso.

Entregue primeiro o plano de alterações e os arquivos que pretende criar. Só depois implemente a ferramenta de prévia, sem aplicar alterações destrutivas na cena.

# Instruções para o OpenCode — integrar o mapa protótipo v0.1

Leia `README.md` e o JSON `Assets/IFNMG360/Config/IFNMG_Mapa_Proto_v0_1.json` integralmente antes de executar qualquer ação.

Este pacote é um protótipo espacial deliberadamente provisório. **Não use visão de imagem**: trabalhe por JSON, coordenadas, nomes de objetos, caminhos de prefabs, bounds e relatórios do Unity Editor. A planta PNG é exclusivamente para o usuário validar visualmente.

1. Confirme que os arquivos foram copiados para os caminhos indicados e que `IFNMGPrototypeMapGenerator.cs` compila na versão Unity do projeto. Se houver erro de API/versão, corrija o código sem alterar o JSON espacial.
2. Confirme a existência dos prefabs candidatos pelos caminhos no JSON. Informe quais não existem; o gerador pode usar os fallbacks simples.
3. Não substitua nem mova objetos da cena principal. Gere o protótipo apenas na cena independente `IFNMG_Mapa_Prototype_v0_1.unity`.
4. Execute o menu de geração somente depois da compilação sem erros. Leia os logs e registre quais zonas produziram avisos, quais prefabs faltaram e quais bounds se sobrepõem.
5. Não declare que a composição está visualmente correta. Não é possível interpretar imagens neste fluxo. Entregue ao usuário a cena gerada e a planta PNG para inspeção humana.
6. Se houver discrepâncias, corrija os `pointsUvFlat`, `widthMeters`, `buildingHeightMeters` ou o estado `enabled` no JSON; mantenha os `id` para facilitar revisões.
7. A área de 1.480,14 m² e o perímetro de 154,80 m medidos no prédio Jogos Analógicos ainda não validam a escala automaticamente. Não alegue calibração sem uma geometria numérica correspondente ao mesmo contorno.
8. Preserve a correção de `pavimento_estacionamento_leste` e a via provisória `via_sob_copas_oeste_sudoeste`, salvo se o usuário der novas instruções.
9. Antes de levar objetos para a cena principal, gere uma lista das correções propostas e aguarde autorização do usuário.

Objetivo da versão: colocar em pé um blockout de todo o perímetro com terreno, vias, footprints simplificados e uma camada discreta de vegetação, priorizando continuidade e possibilidade de correção durante o desenvolvimento.

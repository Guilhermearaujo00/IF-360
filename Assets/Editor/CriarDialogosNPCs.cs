using IF360;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Cria os assets DialogoData dos 12 NPCs em Assets/Dados/Dialogos, seguindo o
/// padrão do projeto: UM ASSET POR ESTADO da conversa
/// ('Dialogo_{NPC}_Pre' / '_Durante' / '_Pos'), como os já existentes
/// Dialogo_Secretaria_Pre / Dialogo_Secretaria_Pos.
///
/// O wiring (qual NPC usa qual asset e com quais condições de VarianteDialogo)
/// é feito pelo 'Tools > IFNMG 360 > 2 - Posicionar NPCs no Campus'.
///
/// Executar via: Tools > IFNMG 360 > 1 - Criar Dialogos dos NPCs
/// </summary>
public static class CriarDialogosNPCs
{
    private const string Menu = "Tools/IFNMG 360/1 - Criar Dialogos dos NPCs";
    private const string Pasta = "Assets/Dados/Dialogos";

    private sealed class DefinicaoDialogo
    {
        public readonly string nomeAsset;
        public readonly (string falante, string texto)[] falas;

        public DefinicaoDialogo(string nomeAsset, params (string, string)[] falas)
        {
            this.nomeAsset = nomeAsset;
            this.falas = falas;
        }
    }

    // Falas vazias ('') usam o nome do NPC automaticamente; 'Caio' é o jogador
    // (mesmo padrão do Dialogo_Secretaria_Pre).
    private static readonly DefinicaoDialogo[] Definicoes =
    {
        // ---------- Q01 - Ensino Médio (Ananda) ----------
        new DefinicaoDialogo("Dialogo_Ananda_Pre",
            ("", "Olá! Seja bem-vindo ao IFNMG – Campus Januária! Eu sou Ananda e vou ajudar você a conhecer o campus."),
            ("Caio", "Oi, Ananda! Estou animado para começar."),
            ("", "Este é o prédio do Ensino Médio, onde acontecem as aulas dessa etapa de formação."),
            ("", "Antes de tudo precisamos resolver a sua matrícula — mas o sistema está sem conexão com a internet."),
            ("", "Vá até a Administração e procure o Roney. Ele vai indicar como resolver o problema no SET, dentro do Almoxarifado.")),
        new DefinicaoDialogo("Dialogo_Ananda_Pos_Matricula",
            ("", "Excelente! O servidor voltou a funcionar e sua matrícula está concluída!"),
            ("Caio", "Consegui! Foi só conectar os cabos certos."),
            ("", "Agora você já pode circular pelo campus. Que tal conhecer a Biblioteca? Fale com o Thiago para fazer seu cadastro.")),
        new DefinicaoDialogo("Dialogo_Ananda_Pos_Biblioteca",
            ("", "Ouvi dizer que você ajudou a Biblioteca com os livros atrasados. Muito obrigada!"),
            ("", "O próximo passo é conhecer os projetos extracurriculares. Procure a Josy no Gabinete de Docentes, no Núcleo.")),
        new DefinicaoDialogo("Dialogo_Ananda_Pos_Nucleo",
            ("", "Então o projeto do Núcleo foi finalizado? Parabéns!"),
            ("", "Depois de tanta jornada, uma boa refeição cai bem. Procure o João no Refeitório — mas não esqueça do ticket!")),
        new DefinicaoDialogo("Dialogo_Ananda_Pos_Refeitorio",
            ("", "Vejo que você já conhece bem o campus: matrícula, biblioteca, projetos e até o almoço!"),
            ("", "Para completar, visite a Assistência Estudantil e converse com a Alice. Ela informa sobre editais e benefícios para estudantes.")),

        // ---------- Q01 - Administração (Roney) ----------
        // Roney absorveu o papel narrativo da Secretaria legada: explica tanto a
        // Administração quanto a Secretaria (decisão da reestruturação — o NPC
        // "Secretaria" saiu do catálogo e ficou apenas como cenário/memória).
        new DefinicaoDialogo("Dialogo_Roney_Pre",
            ("", "Olá! Sou Roney. Você está na Administração, que apoia o funcionamento e a organização das atividades do campus."),
            ("", "Aqui também funciona a Secretaria, que cuida dos registros acadêmicos: matrículas, documentos e histórico dos estudantes. Qualquer papel oficial passa por ela."),
            ("", "Também temos o Almoxarifado, onde ficam materiais e equipamentos — é lá dentro que fica o SET, o setor de tecnologia."),
            ("", "Precisamos reconfigurar o servidor, que está sem conexão. Consegue me ajudar?"),
            ("Caio", "Claro! O que preciso fazer?"),
            ("", "Conecte os três cabos às entradas correspondentes do servidor. Observe as cores e verifique cada conexão antes de ligar.")),
        new DefinicaoDialogo("Dialogo_Roney_Pos",
            ("", "Perfeito! As três conexões foram feitas corretamente e o servidor voltou a responder."),
            ("", "Agora volte ao prédio do Ensino Médio e procure a Ananda para finalizar sua matrícula.")),

        // ---------- Q02 - Biblioteca (Thiago + alunos) ----------
        new DefinicaoDialogo("Dialogo_Thiago_Pre",
            ("", "Olá! Seja bem-vindo à Biblioteca do IFNMG. Aqui você encontra acervo para consulta, leitura e empréstimo."),
            ("", "Vou cadastrar você no nosso sistema... Pronto, cadastro realizado!"),
            ("", "Antes que eu esqueça: três alunos estão com livros atrasados e ainda não receberam o aviso."),
            ("", "JP está no Laboratório de Química com o livro de Química; Gleice, no de Física; e Rosy, no de Biológicas. Por favor, avise os três.")),
        new DefinicaoDialogo("Dialogo_Thiago_Durante",
            ("", "Conseguiu falar com os três alunos?"),
            ("", "Verifique seu progresso: JP (Química), Gleice (Física) e Rosy (Biológicas) estão nos laboratórios de suas áreas. Volte quando tiver avisado todos.")),
        new DefinicaoDialogo("Dialogo_Thiago_Pos",
            ("", "Excelente! Você conseguiu avisar os três alunos."),
            ("", "Obrigado por ajudar a manter nosso acervo disponível para todos. Aproveite a biblioteca sempre que precisar!")),
        new DefinicaoDialogo("Dialogo_JP_Pre",
            ("", "Oi! Está precisando de alguma coisa?"),
            ("Caio", "Vim te lembrar do livro de Química da biblioteca."),
            ("", "É verdade, eu acabei esquecendo de devolvê-lo. Obrigado pelo lembrete! Vou providenciar hoje mesmo.")),
        new DefinicaoDialogo("Dialogo_JP_Pos",
            ("", "Já fui avisado sobre o livro de Química, obrigado! Vou devolvê-lo à biblioteca.")),
        new DefinicaoDialogo("Dialogo_Gleice_Pre",
            ("", "Olá! Tudo bem?"),
            ("Caio", "Vim te lembrar do livro de Física da biblioteca."),
            ("", "Nossa, é mesmo! Acabei deixando passar. Obrigada por avisar, vou resolver isso.")),
        new DefinicaoDialogo("Dialogo_Gleice_Pos",
            ("", "Já sei do livro de Física, obrigada pelo lembrete! Vou devolvê-lo à biblioteca.")),
        new DefinicaoDialogo("Dialogo_Rosy_Pre",
            ("", "Olá! Posso ajudar?"),
            ("Caio", "Vim te lembrar do livro de Biologia da biblioteca."),
            ("", "Ah, é verdade! Eu tinha esquecido. Obrigada por avisar, vou providenciar a devolução.")),
        new DefinicaoDialogo("Dialogo_Rosy_Pos",
            ("", "Já fui avisada sobre o livro de Biologia, obrigada! Vou devolvê-lo à biblioteca.")),

        // ---------- Q03 - Núcleo (Josy, David, Márcio) ----------
        new DefinicaoDialogo("Dialogo_Josy_Pre",
            ("", "Olá! Sou Josy, seja bem-vindo ao Núcleo — aqui ficam o Gabinete de Docentes e os projetos extracurriculares."),
            ("", "Participar de projetos é uma ótima oportunidade de aprender, desenvolver ideias e trabalhar em equipe."),
            ("", "Estamos finalizando um jogo educativo, mas faltam os componentes do computador. Você pode buscá-los com o David, no laboratório Bridge, no prédio EAD?")),
        new DefinicaoDialogo("Dialogo_Josy_Durante",
            ("", "Como está o andamento do nosso projeto?"),
            ("", "Pegue os componentes com o David, no laboratório Bridge (prédio EAD), e leve-os ao Márcio, no prédio do Superior."),
            ("", "O Márcio vai realizar a compilação do jogo — são três etapas de desafio de padrões, com cronômetro. Boa sorte!")),
        new DefinicaoDialogo("Dialogo_Josy_Pos",
            ("", "Você conseguiu! Os componentes foram reunidos e o jogo foi compilado. Muito obrigada pela ajuda!"),
            ("", "É muito bom ver estudantes envolvidos com tecnologia e criatividade. Parabéns por concluir essa atividade!")),
        new DefinicaoDialogo("Dialogo_David_Pre",
            ("", "Olá! Sou David. Este é o laboratório Bridge, dentro do prédio EAD — o setor dedicado à educação a distância."),
            ("", "A Josy comentou que você viria buscar os componentes do projeto. Aqui estão eles!"),
            ("Caio", "Valeu, David! Vou levar direto ao Márcio.")),
        new DefinicaoDialogo("Dialogo_David_Pos",
            ("", "Você já está com os componentes. Agora procure o Márcio, no prédio do Superior, para a compilação do jogo.")),
        new DefinicaoDialogo("Dialogo_Marcio_Pre",
            ("", "Olá! Sou Márcio, bem-vindo ao prédio do Ensino Superior."),
            ("", "Josy me avisou que você trouxe os componentes. Agora precisamos compilar o jogo."),
            ("", "O desafio tem três etapas: observe os padrões, encontre o espaço que falta e escolha, entre as cinco alternativas, a que completa corretamente."),
            ("", "Fique atento ao cronômetro! Quando concluir as três etapas, o projeto estará compilado.")),
        new DefinicaoDialogo("Dialogo_Marcio_Pos",
            ("", "Parabéns! Você completou as três etapas e o jogo foi compilado corretamente."),
            ("", "Agora volte ao Gabinete de Docentes e converse com a Josy para apresentar o resultado.")),

        // ---------- Q04 - Refeitório (João, Cláudio) ----------
        new DefinicaoDialogo("Dialogo_Joao_Pre",
            ("", "Olá! Seja bem-vindo ao Refeitório — o espaço das refeições e dos momentos de descanso."),
            ("", "Para utilizar o serviço, você precisa adquirir um ticket antes."),
            ("", "Procure o Cláudio no Posto de Vendas, ele vai ajudar você com a compra.")),
        new DefinicaoDialogo("Dialogo_Joao_Durante",
            ("", "Já conseguiu seu ticket?"),
            ("", "Se ainda não, o Cláudio está no Posto de Vendas. Depois volte aqui com o ticket em mãos.")),
        new DefinicaoDialogo("Dialogo_Joao_Pos",
            ("", "Ótimo, seu ticket está válido! Pode seguir o procedimento de atendimento."),
            ("", "Bom apetite! Espero que aproveite o espaço.")),
        new DefinicaoDialogo("Dialogo_Claudio_Pre",
            ("", "Olá! Sou Cláudio, trabalho aqui no Posto de Vendas."),
            ("", "Você veio comprar o ticket do Refeitório? Perfeito, é aqui mesmo."),
            ("Caio", "Sim! Quero almoçar hoje."),
            ("", "Ótimo. Aquisição concluída — aqui está o seu ticket. Apresente ao João, no Refeitório!")),
        new DefinicaoDialogo("Dialogo_Claudio_Pos",
            ("", "Sua compra já foi concluída. Apresente o ticket ao João, no Refeitório!")),

        // ---------- Q05 - Assistência Estudantil (Alice) ----------
        new DefinicaoDialogo("Dialogo_Alice_Pre",
            ("", "Olá! Seja bem-vindo à Assistência Estudantil. Eu sou a Alice."),
            ("", "Aqui você encontra informações sobre editais de benefícios destinados aos estudantes."),
            ("", "Acompanhe as divulgações da instituição — cada oportunidade tem requisitos e prazos próprios.")),
        new DefinicaoDialogo("Dialogo_Alice_Pos",
            ("", "Que bom que você conheceu a Assistência Estudantil!"),
            ("", "Continue acompanhando os editais: essas oportunidades podem ser importantes na sua trajetória."))
    };

    [MenuItem(Menu)]
    public static void CriarTodos()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Criar Dialogos dos NPCs",
                "Saia do Play Mode antes de criar os assets.", "OK");
            return;
        }

        int criados = 0, existentes = 0;

        foreach (DefinicaoDialogo def in Definicoes)
        {
            string caminho = $"{Pasta}/{def.nomeAsset}.asset";

            if (AssetDatabase.LoadAssetAtPath<DialogoData>(caminho) != null)
            {
                existentes++;
                continue;
            }

            DialogoData dialogo = ScriptableObject.CreateInstance<DialogoData>();
            dialogo.falas = new DialogoData.FalaDialogo[def.falas.Length];
            for (int i = 0; i < def.falas.Length; i++)
            {
                dialogo.falas[i] = new DialogoData.FalaDialogo
                {
                    nomeFalante = def.falas[i].falante,
                    textoFala = def.falas[i].texto
                };
            }
            dialogo.velDigitacao = 0.03f;

            AssetDatabase.CreateAsset(dialogo, caminho);
            criados++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[IFNMG 360] Dialogos dos NPCs: {criados} criados, {existentes} já existentes " +
                  $"(total definido: {Definicoes.Length}) em {Pasta}.");
        Debug.Log("[IFNMG 360] Proximo passo: 'Tools > IFNMG 360 > 2 - Posicionar NPCs no Campus'.");
    }
}

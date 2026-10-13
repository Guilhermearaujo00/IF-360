namespace IF360
{
using UnityEditor;
using UnityEngine;

/// <summary>
/// Cria os assets MissaoData para as missões definidas no roteiro.
/// Executar via: Tools > IFNMG > Criar MissaoData Assets
/// </summary>
public static class CriarMissaoDataAssets
{
    private const string MENU = "Tools/IFNMG/Criar MissaoData Assets";

    [MenuItem(MENU)]
    public static void CriarTodos()
    {
        string pasta = "Assets/Resources/Missoes";
        if (!AssetDatabase.IsValidFolder(pasta))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "Missoes");
        }

        CriarMissao(pasta, "Missao_EnsinoMedio", "missao_ensino_medio", "Matrícula no Ensino Médio",
            "Ananda dá boas-vindas e explica sobre o campus. O jogador precisa se matricular mas não há internet, então deve ir à Administração falar com o técnico de T.I.",
            new[]
            {
                ("boas_vindas", "Boas-vindas e apresentação", "Ananda apresenta o campus e o prédio do Ensino Médio."),
                ("ir_administracao", "Ir à Administração", "Procurar o técnico de T.I. na Administração para resolver o problema da internet."),
                ("matricula_concluida", "Matrícula concluída", "Retornar a Ananda após resolver o servidor para finalizar a matrícula.")
            });

        CriarMissao(pasta, "Missao_Administracao", "missao_administracao", "Configurar Servidor (Administração)",
            "Roney explica sobre a Administração e Almoxarifado. O jogador deve ir ao SET no Almoxarifado e ajudar a configurar o servidor.",
            new[]
            {
                ("explicacao_roney", "Explicação da Administração", "Roney informa o que a Administração e Almoxarifado fazem."),
                ("minijogo_ligar_fios", "Mini-jogo Ligar 3 Fios", "No SET, ligar 3 fios de cores diferentes nas entradas corretas."),
                ("retornar_ananda", "Retornar a Ananda", "Voltar ao Ensino Médio e finalizar a matrícula com Ananda.")
            });

        CriarMissao(pasta, "Missao_Biblioteca", "missao_biblioteca", "Cadastro e Devolução de Livros (Biblioteca)",
            "Thiago realiza o cadastro na biblioteca e informa que 3 alunos esqueceram livros (Química, Física, Biologia) nos respectivos laboratórios.",
            new[]
            {
                ("cadastro_biblioteca", "Cadastro na Biblioteca", "Conversar com Thiago e realizar o cadastro no sistema da biblioteca."),
                ("livro_quimica", "Devolver livro de Química", "Ir ao Laboratório de Química e lembrar o aluno de devolver o livro."),
                ("livro_fisica", "Devolver livro de Física", "Ir ao Laboratório de Física e lembrar o aluno de devolver o livro."),
                ("livro_biologia", "Devolver livro de Biologia", "Ir ao Laboratório de Biológicas e lembrar o aluno de devolver o livro.")
            });

        CriarMissao(pasta, "Missao_Nucleo", "missao_nucleo", "Projeto do Núcleo (Gabinete de Docentes)",
            "Josy apresenta o Núcleo e pede ajuda para finalizar um projeto. Parte 1: ir ao Lab Bridge (David) pegar componentes e conhecer o EAD. Parte 2: ir ao Prédio do Superior (Márcio) e jogar o mini-jogo Tetris/padrão em 3 etapas.",
            new[]
            {
                ("apresentacao_nucleo", "Apresentação do Núcleo", "Josy apresenta o Núcleo e os projetos extracurriculares."),
                ("lab_bridge_componentes", "Laboratório Bridge - Componentes", "Ir ao Lab Bridge, David apresenta o EAD e entrega os componentes do computador."),
                ("tetris_etapa_1", "Mini-jogo Tetris - Etapa 1", "No Prédio do Superior, completar o padrão 1 de 5 opções antes do cronômetro acabar."),
                ("tetris_etapa_2", "Mini-jogo Tetris - Etapa 2", "Completar o segundo padrão antes do cronômetro acabar."),
                ("tetris_etapa_3", "Mini-jogo Tetris - Etapa 3", "Completar o terceiro e último padrão antes do cronômetro acabar.")
            });

        CriarMissao(pasta, "Missao_Refeitorio", "missao_refeitorio", "Recuperar Energias (Refeitório)",
            "João apresenta o refeitório e o uso de tickets. O jogador deve ir ao Posto de Vendas comprar o ticket com Cláudio.",
            new[]
            {
                ("conhecer_refeitorio", "Conhecer o Refeitório", "João explica como funciona o refeitório e a necessidade de tickets."),
                ("comprar_ticket", "Comprar Ticket", "Ir ao Posto de Vendas e comprar o ticket com Cláudio.")
            });

        CriarMissao(pasta, "Missao_Assistencia", "missao_assistencia", "Benefícios Estudantis (Assistência Estudantil)",
            "Alice apresenta a Assistência Estudantil e informa sobre editais de benefícios.",
            new[]
            {
                ("conhecer_assistencia", "Conhecer a Assistência Estudantil", "Alice apresenta o prédio e explica como funcionam os benefícios e editais.")
            });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("CriarMissaoDataAssets: 6 assets MissaoData criados/atualizados em " + pasta);
    }

    private static void CriarMissao(string pasta, string nomeAsset, string idMissao, string nome, string descricao, (string id, string nome, string descricao)[] etapas)
    {
        var missao = ScriptableObject.CreateInstance<MissaoData>();
        missao.idMissao = idMissao;
        missao.nome = nome;
        missao.descricao = descricao;
        missao.etapas = new EtapaMissao[etapas.Length];

        for (int i = 0; i < etapas.Length; i++)
        {
            missao.etapas[i] = new EtapaMissao
            {
                idEtapa = etapas[i].id,
                nome = etapas[i].nome,
                descricao = etapas[i].descricao
            };
        }

        string caminho = $"{pasta}/{nomeAsset}.asset";
        var existente = AssetDatabase.LoadAssetAtPath<MissaoData>(caminho);
        if (existente != null)
        {
            EditorUtility.CopySerialized(missao, existente);
            EditorUtility.SetDirty(existente);
        }
        else
        {
            AssetDatabase.CreateAsset(missao, caminho);
        }
    }
}
}
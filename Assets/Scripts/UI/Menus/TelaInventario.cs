namespace IF360
{
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Tela de inventário - mostra itens coletados via InventoryManager.
/// </summary>
public class TelaInventario : MenuBase
{
    public Action AoFechar;

    private List<string> cacheItens;

    protected override void Awake()
    {
        base.Awake();
        titulo = "INVENTÁRIO";
        largura = 520f;
        espacamento = 55f;
    }

    protected override void OnAbrir()
    {
        base.OnAbrir();
        AtualizarListaItens();
    }

    protected override void ConstruirConteudo()
    {
        // Item "Voltar" fixo
        AdicionarItem("Voltar", Fechar);

        // Itens dinâmicos serão adicionados em AtualizarListaItens
    }

    private void AtualizarListaItens()
    {
        if (InventoryManager.Instancia == null) return;

        // Remove itens dinâmicos anteriores (mantém apenas "Voltar")
        while (itens.Count > 1) itens.RemoveAt(1);

        var itensInventario = InventoryManager.Instancia.itens;
        if (itensInventario == null || itensInventario.Count == 0)
        {
            // Item informativo vazio
            AdicionarItem("(vazio)", null);
            ConstruirUI();
            indiceSelecionado = 0;
            AtualizarVisualSelecao();
            return;
        }

        foreach (var item in itensInventario)
        {
            string nomeExibicao = FormatarNomeItem(item);
            AdicionarItem(nomeExibicao, null);
        }

        // Reconstrui UI se já aberta
        if (ativo && painel != null)
        {
            painel = null;
            ConstruirUI();
            indiceSelecionado = 0;
            AtualizarVisualSelecao();
        }
    }

    private string FormatarNomeItem(string id)
    {
        // Mapeia IDs internos para nomes amigáveis
        return id switch
        {
            "livro_quimica" => "📚 Livro de Química",
            "livro_fisica" => "📚 Livro de Física",
            "livro_biologia" => "📚 Livro de Biologia",
            "componentes_pc" => "🔧 Componentes do PC",
            _ => id.Replace("_", " ")
        };
    }

    protected override void OnFechar()
    {
        base.OnFechar();
        AoFechar?.Invoke();
    }
}
}
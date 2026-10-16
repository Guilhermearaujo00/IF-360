namespace IF360
{
using System;
using UnityEngine;

/// <summary>
/// Tela de inventário - mostra itens coletados via InventoryManager.
/// </summary>
public class TelaInventario : MenuBase
{
    public Action AoFechar;

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

        var itensInventario = InventoryManager.Instancia.Itens;
        if (itensInventario == null || itensInventario.Count == 0)
        {
            AdicionarItem("(vazio)", null);
        }
        else
        {
            foreach (var item in itensInventario)
            {
                AdicionarItem(FormatarNomeItem(item), null);
            }
        }

        // Reconstrói a UI com a lista atualizada
        if (painel != null)
        {
            RecriarPainel();
        }
    }

    private string FormatarNomeItem(string id)
    {
        // Mapeia IDs internos para nomes amigáveis
        return id switch
        {
            "livro_quimica" => "Livro de Química",
            "livro_fisica" => "Livro de Física",
            "livro_biologia" => "Livro de Biologia",
            "componentes_pc" => "Componentes do PC",
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
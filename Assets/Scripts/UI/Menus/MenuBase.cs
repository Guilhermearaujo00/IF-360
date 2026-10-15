namespace IF360
{
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Classe base para todos os menus do jogo.
/// Constrói UI em runtime, navegação por teclado (setas/WASD + Enter/Esc).
/// </summary>
public abstract class MenuBase : MonoBehaviour
{
    protected struct ItemMenu
    {
        public string rotulo;
        public Func<string> valorAtual;
        public Action acao;
        public Action esquerda;
        public Action direita;
        public TMP_Text textoRotulo;
        public TMP_Text textoValor;
    }

    [Header("Configuração")]
    [SerializeField] protected string titulo = "MENU";
    [SerializeField] protected float largura = 480f;
    [SerializeField] protected float espacamento = 60f;
    [SerializeField] protected float inicioY = 80f;

    protected readonly List<ItemMenu> itens = new List<ItemMenu>();
    protected int indiceSelecionado;
    protected bool ativo;
    protected GameObject painel;
    protected TMP_Text textoTitulo;
    protected TMP_Text textoAviso;
    protected Canvas canvas;

    public bool Ativo => ativo;

    protected virtual void Awake()
    {
        canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError($"[{GetType().Name}] Nenhum Canvas encontrado na cena.");
        }
    }

    protected virtual void Update()
    {
        if (!ativo) return;

        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame)
        {
            MoverSelecao(-1);
        }
        else if (kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame)
        {
            MoverSelecao(1);
        }

        if (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)
        {
            ExecutarAcao();
        }

        if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame)
        {
            ExecutarEsquerda();
        }
        else if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame)
        {
            ExecutarDireita();
        }

        AtualizarValores();
    }

    protected virtual void MoverSelecao(int direcao)
    {
        if (itens.Count == 0) return;
        indiceSelecionado = (indiceSelecionado + direcao + itens.Count) % itens.Count;
        AtualizarVisualSelecao();
    }

    protected virtual void ExecutarAcao()
    {
        if (itens.Count == 0) return;
        itens[indiceSelecionado].acao?.Invoke();
    }

    protected virtual void ExecutarEsquerda()
    {
        if (itens.Count == 0) return;
        itens[indiceSelecionado].esquerda?.Invoke();
    }

    protected virtual void ExecutarDireita()
    {
        if (itens.Count == 0) return;
        itens[indiceSelecionado].direita?.Invoke();
    }

    protected virtual void AtualizarValores()
    {
        foreach (var item in itens)
        {
            if (item.textoValor != null && item.valorAtual != null)
            {
                item.textoValor.text = item.valorAtual();
            }
        }
    }

    protected virtual void AtualizarVisualSelecao()
    {
        for (int i = 0; i < itens.Count; i++)
        {
            bool selecionado = i == indiceSelecionado;
            if (itens[i].textoRotulo != null)
            {
                itens[i].textoRotulo.text = (selecionado ? "▶ " : "  ") + ObterRotuloBase(i);
                itens[i].textoRotulo.color = selecionado ? new Color(1f, 0.9f, 0.3f) : Color.white;
            }
            if (itens[i].textoValor != null)
            {
                itens[i].textoValor.color = selecionado ? new Color(1f, 0.9f, 0.3f) : new Color(0.8f, 0.8f, 0.8f);
            }
        }
    }

    protected virtual string ObterRotuloBase(int index)
    {
        return itens[index].rotulo;
    }

    protected virtual ItemMenu AdicionarItem(string rotulo, Action acao = null, Func<string> valor = null, Action esquerda = null, Action direita = null)
    {
        var item = new ItemMenu
        {
            rotulo = rotulo,
            acao = acao,
            valorAtual = valor,
            esquerda = esquerda,
            direita = direita
        };
        itens.Add(item);
        return item;
    }

    // --- Construção de UI ---

    protected virtual void ConstruirUI()
    {
        if (painel != null) return;
        if (canvas == null) return;

        painel = new GameObject("Painel_" + GetType().Name, typeof(RectTransform), typeof(Image));
        painel.transform.SetParent(canvas.transform, false);
        var rt = painel.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(largura, 600f);
        painel.GetComponent<Image>().color = new Color(0.07f, 0.07f, 0.12f, 0.97f);

        // Título
        var goTitulo = new GameObject("Titulo", typeof(RectTransform));
        goTitulo.transform.SetParent(painel.transform, false);
        var rtTitulo = goTitulo.GetComponent<RectTransform>();
        rtTitulo.anchorMin = rtTitulo.anchorMax = new Vector2(0.5f, 1f);
        rtTitulo.pivot = new Vector2(0.5f, 1f);
        rtTitulo.anchoredPosition = new Vector2(0f, -30f);
        rtTitulo.sizeDelta = new Vector2(largura - 40f, 50f);
        textoTitulo = goTitulo.AddComponent<TextMeshProUGUI>();
        textoTitulo.text = titulo;
        textoTitulo.fontSize = 36f;
        textoTitulo.alignment = TextAlignmentOptions.Center;
        textoTitulo.color = Color.white;

        // Aviso/Instrução
        var goAviso = new GameObject("Aviso", typeof(RectTransform));
        goAviso.transform.SetParent(painel.transform, false);
        var rtAviso = goAviso.GetComponent<RectTransform>();
        rtAviso.anchorMin = rtAviso.anchorMax = new Vector2(0.5f, 0f);
        rtAviso.pivot = new Vector2(0.5f, 0f);
        rtAviso.anchoredPosition = new Vector2(0f, 30f);
        rtAviso.sizeDelta = new Vector2(largura - 40f, 40f);
        textoAviso = goAviso.AddComponent<TextMeshProUGUI>();
        textoAviso.text = "↑/↓ ou W/S navega  •  Enter/Espaço seleciona  •  ←/→ ou A/D ajusta  •  Esc fecha";
        textoAviso.fontSize = 18f;
        textoAviso.alignment = TextAlignmentOptions.Center;
        textoAviso.color = new Color(0.7f, 0.7f, 0.7f);

        // Itens
        float yAtual = -inicioY;
        foreach (var item in itens)
        {
            CriarLinhaItem(item, yAtual);
            yAtual -= espacamento;
        }

        painel.SetActive(false);
    }

    private void CriarLinhaItem(ItemMenu item, float y)
    {
        var goLinha = new GameObject("Item_" + item.rotulo, typeof(RectTransform));
        goLinha.transform.SetParent(painel.transform, false);
        var rt = goLinha.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, y);
        rt.sizeDelta = new Vector2(largura - 80f, 50f);

        // Rótulo
        var goRotulo = new GameObject("Rotulo", typeof(RectTransform));
        goRotulo.transform.SetParent(goLinha.transform, false);
        var rtRotulo = goRotulo.GetComponent<RectTransform>();
        rtRotulo.anchorMin = new Vector2(0f, 0.5f);
        rtRotulo.anchorMax = new Vector2(0.5f, 0.5f);
        rtRotulo.pivot = new Vector2(0f, 0.5f);
        rtRotulo.anchoredPosition = Vector2.zero;
        rtRotulo.sizeDelta = new Vector2(largura * 0.5f - 20f, 50f);
        item.textoRotulo = goRotulo.AddComponent<TextMeshProUGUI>();
        item.textoRotulo.text = "  " + item.rotulo;
        item.textoRotulo.fontSize = 26f;
        item.textoRotulo.alignment = TextAlignmentOptions.MidlineLeft;
        item.textoRotulo.color = Color.white;

        // Valor
        if (item.valorAtual != null)
        {
            var goValor = new GameObject("Valor", typeof(RectTransform));
            goValor.transform.SetParent(goLinha.transform, false);
            var rtValor = goValor.GetComponent<RectTransform>();
            rtValor.anchorMin = new Vector2(0.5f, 0.5f);
            rtValor.anchorMax = new Vector2(1f, 0.5f);
            rtValor.pivot = new Vector2(1f, 0.5f);
            rtValor.anchoredPosition = Vector2.zero;
            rtValor.sizeDelta = new Vector2(largura * 0.5f - 20f, 50f);
            item.textoValor = goValor.AddComponent<TextMeshProUGUI>();
            item.textoValor.text = item.valorAtual?.Invoke() ?? "";
            item.textoValor.fontSize = 24f;
            item.textoValor.alignment = TextAlignmentOptions.MidlineRight;
            item.textoValor.color = new Color(0.8f, 0.8f, 0.8f);
        }
    }

    // --- API Pública ---

    public virtual void Abrir()
    {
        if (ativo) return;
        ConstruirUI();
        indiceSelecionado = 0;
        ativo = true;
        painel.SetActive(true);
        AtualizarVisualSelecao();
        AtualizarValores();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        OnAbrir();
    }

    public virtual void Fechar()
    {
        if (!ativo) return;
        ativo = false;
        if (painel != null) painel.SetActive(false);
        OnFechar();
    }

    protected virtual void OnAbrir() { }
    protected virtual void OnFechar() { }
}
}
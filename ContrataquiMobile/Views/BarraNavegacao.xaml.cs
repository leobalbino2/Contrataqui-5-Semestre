using ContrataquiMobile.Models;
using ContrataquiMobile.Services;

namespace ContrataquiMobile.Views;

public partial class BarraNavegacao : ContentView
{
    private static readonly Color CorAtiva = Color.FromArgb("#002045");
    private static readonly Color CorInativa = Color.FromArgb("#43474E");

    private readonly Dictionary<string, (Label Icone, Label Rotulo)> _abas = new();
    private string ativo = "";

    public string Ativo
    {
        get => ativo;
        set
        {
            ativo = value;
            Atualizar();
        }
    }

    public BarraNavegacao()
    {
        InitializeComponent();
        Montar();
    }

    // As abas mudam conforme o tipo de usuário logado
    private void Montar()
    {
        TipoUsuario tipo = Sessao.Atual?.Tipo ?? TipoUsuario.Cliente;
        IReadOnlyList<Aba> abas = Navegacao.AbasDe(tipo);

        for (int i = 0; i < abas.Count; i++)
        {
            Aba aba = abas[i];

            var icone = new Label
            {
                Text = aba.Icone,
                FontFamily = "MaterialIcons",
                FontSize = 24,
                HorizontalOptions = LayoutOptions.Center
            };

            var rotulo = new Label
            {
                Text = aba.Rotulo,
                FontSize = 11,
                HorizontalOptions = LayoutOptions.Center
            };

            var item = new VerticalStackLayout
            {
                Spacing = 2,
                Padding = new Thickness(0, 4),
                Children = { icone, rotulo }
            };

            var toque = new TapGestureRecognizer { CommandParameter = aba.Id };
            toque.Tapped += Aba_Tapped;
            item.GestureRecognizers.Add(toque);

            grd_abas.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            grd_abas.Add(item, i, 0);

            _abas[aba.Id] = (icone, rotulo);
        }

        Atualizar();
    }

    private void Atualizar()
    {
        foreach (var (id, (icone, rotulo)) in _abas)
            Pintar(icone, rotulo, id == ativo);
    }

    private static void Pintar(Label icone, Label rotulo, bool ligado)
    {
        Color cor = ligado ? CorAtiva : CorInativa;

        icone.TextColor = cor;
        rotulo.TextColor = cor;
        rotulo.FontAttributes = ligado ? FontAttributes.Bold : FontAttributes.None;
    }

    // Sobe na árvore de elementos até achar a página que contém a barra
    private Page? PaginaAtual()
    {
        Element? atual = this;

        while (atual is not null && atual is not Page)
            atual = atual.Parent;

        return atual as Page;
    }

    private async void Aba_Tapped(object? sender, TappedEventArgs e)
    {
        string destino = e.Parameter?.ToString() ?? "";

        Page? pagina = PaginaAtual();

        if (pagina is null)
            return;

        try
        {
            if (destino == ativo)
                return;

            Page? nova = Navegacao.CriarPagina(destino);

            if (nova is null || pagina.Window is null)
                return;

            // Troca direta da página raiz: instantâneo, sem animação
            pagina.Window.Page = new NavigationPage(nova);
        }
        catch (Exception ex)
        {
            await pagina.DisplayAlertAsync("Erro ao navegar", ex.Message, "OK");
        }
    }
}
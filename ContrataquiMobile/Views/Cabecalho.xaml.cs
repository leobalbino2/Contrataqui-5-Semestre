using ContrataquiMobile.Models;
using ContrataquiMobile.Services;
using Microsoft.Maui.Controls.Shapes;

namespace ContrataquiMobile.Views;

public partial class Cabecalho : ContentView
{
    private static readonly Color Primary = Color.FromArgb("#002045");
    private static readonly Color Secondary = Color.FromArgb("#13696A");
    private static readonly Color OnSurface = Color.FromArgb("#111C2C");
    private static readonly Color OnSurfaceVariant = Color.FromArgb("#43474E");
    private static readonly Color Divisor = Color.FromArgb("#E1E8F5");
    private static readonly Color Perigo = Color.FromArgb("#BA1A1A");

    // Última contagem conhecida de notificações não lidas (vale pro app todo)
    private static int _naoLidas;
    private static int _donoDoCache;
    private static DateTime _ultimaConsulta = DateTime.MinValue;
    private static event Action? NaoLidasAlteradas;

    private record ItemMenu(string Icone, string Rotulo, Func<Page, Task> Acao);

    private Grid? _overlay;

    public Cabecalho()
    {
        InitializeComponent();
        AtualizarAvatar();

        Loaded += Cabecalho_Loaded;
        Unloaded += Cabecalho_Unloaded;
    }

    // Chamar depois de mudar o nome do usuário (a inicial do avatar acompanha)
    public void AtualizarAvatar() => lbl_inicial.Text = Inicial();

    // A tela de notificações chama isto quando lê ou marca algo, pro ponto atualizar
    public static void DefinirNaoLidas(int total)
    {
        _donoDoCache = Sessao.Atual?.Id ?? 0;
        _naoLidas = total;
        _ultimaConsulta = DateTime.UtcNow;
        NaoLidasAlteradas?.Invoke();
    }

    private async void Cabecalho_Loaded(object? sender, EventArgs e)
    {
        NaoLidasAlteradas -= AplicarPonto;
        NaoLidasAlteradas += AplicarPonto;

        await AtualizarPontoAsync();
    }

    private void Cabecalho_Unloaded(object? sender, EventArgs e)
    {
        NaoLidasAlteradas -= AplicarPonto;
    }

    private void AplicarPonto() =>
        MainThread.BeginInvokeOnMainThread(() => pnt_notificacao.IsVisible = _naoLidas > 0);

    private async Task AtualizarPontoAsync()
    {
        if (Sessao.Atual is null) return;

        // Outro usuário entrou no mesmo aparelho: descarta a contagem anterior
        if (_donoDoCache != Sessao.Atual.Id)
        {
            _donoDoCache = Sessao.Atual.Id;
            _naoLidas = 0;
            _ultimaConsulta = DateTime.MinValue;
        }

        // Mostra na hora o último valor conhecido
        AplicarPonto();

        // Evita consultar a API a cada troca de aba
        if (DateTime.UtcNow - _ultimaConsulta < TimeSpan.FromSeconds(20)) return;

        try
        {
            List<Notificacao> lista = await ApiService.ListarNotificacoesAsync();
            DefinirNaoLidas(lista.Count(n => !n.Lida));
        }
        catch
        {
            // Sem rede ou sem login válido: mantém o último valor
        }
    }

    private static string Inicial()
    {
        string nome = Sessao.Atual?.Nome?.Trim() ?? "";
        return nome.Length > 0 ? char.ToUpper(nome[0]).ToString() : "?";
    }

    // Sobe na árvore de elementos até achar a página que contém o cabeçalho
    private Page? PaginaAtual()
    {
        Element? atual = this;

        while (atual is not null && atual is not Page)
            atual = atual.Parent;

        return atual as Page;
    }

    private async void Notificacoes_Tapped(object? sender, TappedEventArgs e)
    {
        Page? pagina = PaginaAtual();
        if (pagina is null) return;

        try
        {
            await pagina.Navigation.PushAsync(new Notificacoes());
        }
        catch (Exception ex)
        {
            await pagina.DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }

    // ---------- Itens do menu ----------
    // Perfil e Agendamentos já estão na barra de baixo (o perfil também abre tocando no cartão do topo)

    private static List<ItemMenu> ItensDoMenu() => new()
    {
        new("\uE8B8", "Configurações", p => p.Navigation.PushAsync(new Configuracoes())),
        new("\uE887", "Como funciona", p => p.Navigation.PushAsync(new ComoFunciona())),
        new("\uE88E", "Ajuda e suporte", p => EmBreveAsync(p, "Ajuda e suporte"))
    };

    private static Task IrParaAbaAsync(Page pagina, string id)
    {
        bool jaEstaAqui = (id == "perfil" && pagina is Perfil)
                       || (id == "agendamentos" && pagina is Agendamentos);

        if (!jaEstaAqui && pagina.Window is not null)
        {
            Page? nova = Navegacao.CriarPagina(id);
            if (nova is not null)
                pagina.Window.Page = new NavigationPage(nova);
        }

        return Task.CompletedTask;
    }

    private static Task EmBreveAsync(Page pagina, string titulo) =>
        pagina.DisplayAlertAsync(titulo, "Em breve.", "OK");

    private static async Task SairAsync(Page pagina)
    {
        bool confirmar = await pagina.DisplayAlertAsync(
            "Sair da conta",
            "Deseja realmente sair?",
            "Sim",
            "Não");

        if (!confirmar || pagina.Window is null) return;

        Sessao.Encerrar();
        pagina.Window.Page = new AppShell();
    }

    // ---------- Abrir e fechar o menu ----------

    private async void Perfil_Tapped(object? sender, TappedEventArgs e)
    {
        Page? pagina = PaginaAtual();
        if (pagina is null) return;

        try
        {
            if (_overlay is not null)
            {
                await FecharMenuAsync();
                return;
            }

            if (pagina is ContentPage { Content: Grid raiz })
                await AbrirMenuAsync(pagina, raiz);
            else
                await MenuSimplesAsync(pagina);
        }
        catch (Exception ex)
        {
            await pagina.DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }

    private async Task AbrirMenuAsync(Page pagina, Grid raiz)
    {
        var fundo = new BoxView { Color = Color.FromArgb("#66000000") };
        fundo.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () => await FecharMenuAsync())
        });

        var pilha = new VerticalStackLayout { Spacing = 0 };
        pilha.Children.Add(CriarTopoDoMenu(pagina));
        pilha.Children.Add(CriarDivisor());

        foreach (ItemMenu item in ItensDoMenu())
            pilha.Children.Add(CriarLinha(pagina, item, OnSurface, mostrarSeta: true));

        pilha.Children.Add(CriarDivisor());
        pilha.Children.Add(CriarLinha(pagina,
            new ItemMenu("\uE879", "Sair da conta", SairAsync),
            Perigo, mostrarSeta: false));

        var cartao = new Border
        {
            WidthRequest = 290,
            BackgroundColor = Colors.White,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            Shadow = new Shadow { Brush = Brush.Black, Opacity = 0.2f, Radius = 16, Offset = new Point(0, 6) },
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(0, 64, 16, 0),
            Content = pilha
        };

        var overlay = new Grid { Opacity = 0 };
        overlay.Children.Add(fundo);
        overlay.Children.Add(cartao);

        // Cobre a página inteira, inclusive a barra de baixo
        Grid.SetRowSpan(overlay, Math.Max(1, raiz.RowDefinitions.Count));
        Grid.SetColumnSpan(overlay, Math.Max(1, raiz.ColumnDefinitions.Count));

        _overlay = overlay;
        raiz.Children.Add(overlay);

        await overlay.FadeToAsync(1, 150);
    }

    private async Task FecharMenuAsync()
    {
        Grid? overlay = _overlay;
        if (overlay is null) return;

        _overlay = null;

        await overlay.FadeToAsync(0, 120);

        if (overlay.Parent is Grid pai)
            pai.Children.Remove(overlay);
    }

    // ---------- Peças visuais do menu ----------

    // Cartão do topo: nome, e-mail e tipo. Tocar nele abre o perfil.
    private View CriarTopoDoMenu(Page pagina)
    {
        var avatar = new Border
        {
            WidthRequest = 44,
            HeightRequest = 44,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 22 },
            BackgroundColor = Primary,
            VerticalOptions = LayoutOptions.Center,
            Content = new Label
            {
                Text = Inicial(),
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold,
                FontSize = 18,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }
        };

        var chip = new Border
        {
            BackgroundColor = Color.FromArgb("#E6F6F6"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Padding = new Thickness(8, 2),
            HorizontalOptions = LayoutOptions.Start,
            Content = new Label
            {
                Text = Sessao.EhProfissional ? "Profissional" : "Cliente",
                FontSize = 10,
                FontAttributes = FontAttributes.Bold,
                TextColor = Secondary
            }
        };

        var textos = new VerticalStackLayout
        {
            Spacing = 2,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Label
                {
                    Text = Sessao.Atual?.Nome ?? "",
                    FontSize = 15,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Primary,
                    LineBreakMode = LineBreakMode.TailTruncation
                },
                new Label
                {
                    Text = Sessao.Atual?.Email ?? "",
                    FontSize = 12,
                    TextColor = OnSurfaceVariant,
                    LineBreakMode = LineBreakMode.TailTruncation
                },
                chip
            }
        };

        var seta = new Label
        {
            Text = "\uE5CC",
            FontFamily = "MaterialIcons",
            FontSize = 22,
            TextColor = Color.FromArgb("#9AA0A8"),
            VerticalOptions = LayoutOptions.Center
        };

        var topo = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 12,
            Padding = new Thickness(16, 16, 12, 14),
            BackgroundColor = Colors.Transparent
        };
        topo.Add(avatar, 0, 0);
        topo.Add(textos, 1, 0);
        topo.Add(seta, 2, 0);

        topo.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () =>
            {
                await FecharMenuAsync();
                await IrParaAbaAsync(pagina, "perfil");
            })
        });

        return topo;
    }

    private View CriarLinha(Page pagina, ItemMenu item, Color cor, bool mostrarSeta)
    {
        var linha = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 14,
            Padding = new Thickness(16, 13),
            BackgroundColor = Colors.Transparent
        };

        linha.Add(new Label
        {
            Text = item.Icone,
            FontFamily = "MaterialIcons",
            FontSize = 22,
            TextColor = cor == Perigo ? Perigo : Secondary,
            VerticalOptions = LayoutOptions.Center
        }, 0, 0);

        linha.Add(new Label
        {
            Text = item.Rotulo,
            FontSize = 14,
            TextColor = cor,
            VerticalOptions = LayoutOptions.Center
        }, 1, 0);

        if (mostrarSeta)
        {
            linha.Add(new Label
            {
                Text = "\uE5CC",
                FontFamily = "MaterialIcons",
                FontSize = 20,
                TextColor = Color.FromArgb("#9AA0A8"),
                VerticalOptions = LayoutOptions.Center
            }, 2, 0);
        }

        linha.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () =>
            {
                await FecharMenuAsync();
                await item.Acao(pagina);
            })
        });

        return linha;
    }

    private static View CriarDivisor() => new BoxView { HeightRequest = 1, Color = Divisor };

    // Reserva para páginas cuja raiz não é um Grid: usa a caixa de opções do sistema
    private static async Task MenuSimplesAsync(Page pagina)
    {
        List<ItemMenu> itens = ItensDoMenu();

        string? escolha = await pagina.DisplayActionSheetAsync(
            "Minha conta",
            "Cancelar",
            null,
            itens.Select(i => i.Rotulo).Prepend("Meu perfil").Append("Sair da conta").ToArray());

        if (escolha == "Sair da conta")
            await SairAsync(pagina);
        else if (escolha == "Meu perfil")
            await IrParaAbaAsync(pagina, "perfil");
        else
            await (itens.FirstOrDefault(i => i.Rotulo == escolha)?.Acao(pagina) ?? Task.CompletedTask);
    }
}
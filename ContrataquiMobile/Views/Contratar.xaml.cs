using ContrataquiMobile.Models;
using ContrataquiMobile.Services;
using Microsoft.Maui.Controls.Shapes;

namespace ContrataquiMobile.Views;

public partial class Contratar : ContentPage
{
    private static readonly Color CorBorda = Color.FromArgb("#E1E8F5");
    private static readonly Color CorSelecionada = Color.FromArgb("#13696A");
    private static readonly Color CorFundoSelecionada = Color.FromArgb("#F0FDFA");

    private readonly Profissional _profissional;
    private readonly List<(Servico Servico, Border Cartao, Label Marca)> _cartoes = new();
    private Servico? _escolhido;
    private PerfilUsuario? _meuPerfil;

    public Contratar(Profissional profissional, int? servicoId = null)
    {
        InitializeComponent();
        _profissional = profissional;

        lbl_para.Text = $"Solicitação para {profissional.Nome}";

        MontarServicos(servicoId);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_meuPerfil is not null) return;

        // Se o cliente já tem endereço cadastrado, oferece usar ele
        try
        {
            _meuPerfil = await ApiService.ObterMeuPerfilAsync();

            string cadastrado = EnderecoCadastrado();

            if (cadastrado.Length > 0)
            {
                lbl_endereco_cadastro.Text = $"Usar meu endereço cadastrado: {cadastrado}";
                stk_usar_cadastro.IsVisible = true;
                stk_endereco.IsVisible = !chk_usar_cadastro.IsChecked;
            }
        }
        catch
        {
            // Sem o perfil, o cliente digita o endereço
            _meuPerfil = new PerfilUsuario();
        }
    }

    // "Rua, nº - complemento - Cidade, UF"
    private string EnderecoCadastrado()
    {
        if (_meuPerfil is null || _meuPerfil.Endereco.Length == 0) return "";
        return _meuPerfil.TemLocal ? $"{_meuPerfil.Endereco} - {_meuPerfil.Local}" : _meuPerfil.Endereco;
    }

    private void MontarServicos(int? servicoId)
    {
        foreach (Servico servico in _profissional.ListaServicos)
        {
            var marca = new Label
            {
                Text = "", // radio_button_unchecked
                FontFamily = "MaterialIcons",
                FontSize = 22,
                TextColor = CorBorda,
                VerticalOptions = LayoutOptions.Center
            };

            var texto = new Label
            {
                Text = servico.Nome,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#111C2C"),
                VerticalOptions = LayoutOptions.Center
            };

            var grade = new Grid
            {
                ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) },
                ColumnSpacing = 12
            };
            grade.Add(marca, 0, 0);
            grade.Add(texto, 1, 0);

            var cartao = new Border
            {
                BackgroundColor = Colors.White,
                Stroke = CorBorda,
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
                Padding = new Thickness(14, 12),
                Content = grade
            };

            Servico atual = servico;
            var toque = new TapGestureRecognizer();
            toque.Tapped += (_, _) => Escolher(atual);
            cartao.GestureRecognizers.Add(toque);

            stk_servicos.Add(cartao);
            _cartoes.Add((servico, cartao, marca));
        }

        // Um serviço só (ou vindo da lista filtrada): já vem marcado
        Servico? inicial =
            _profissional.ListaServicos.FirstOrDefault(s => s.Id == servicoId)
            ?? (_profissional.ListaServicos.Count == 1 ? _profissional.ListaServicos[0] : null);

        if (inicial is not null) Escolher(inicial);
    }

    private void Escolher(Servico servico)
    {
        _escolhido = servico;

        foreach (var (s, cartao, marca) in _cartoes)
        {
            bool marcado = s.Id == servico.Id;
            cartao.Stroke = marcado ? CorSelecionada : CorBorda;
            cartao.BackgroundColor = marcado ? CorFundoSelecionada : Colors.White;
            marca.Text = marcado ? "" : ""; // radio_button_checked / unchecked
            marca.TextColor = marcado ? CorSelecionada : CorBorda;
        }
    }

    private void Titulo_TextChanged(object? sender, TextChangedEventArgs e)
    {
        lbl_contador.Text = $"{e.NewTextValue?.Length ?? 0}/150";
    }

    private void UsarCadastro_CheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        // Dispara uma vez durante o InitializeComponent, antes de o campo de endereço existir
        if (stk_endereco is null) return;

        stk_endereco.IsVisible = !e.Value;
    }

    private async void Enviar_Clicked(object? sender, EventArgs e)
    {
        if (_profissional.Id == Sessao.Atual?.Id)
        {
            await DisplayAlertAsync("Atenção", "Você não pode contratar a si mesmo.", "OK");
            return;
        }

        if (_escolhido is null)
        {
            await DisplayAlertAsync("Atenção", "Escolha o serviço que você precisa.", "OK");
            return;
        }

        string titulo = (txt_titulo.Text ?? "").Trim();
        string descricao = (txt_descricao.Text ?? "").Trim();

        if (titulo.Length == 0)
        {
            await DisplayAlertAsync("Atenção", "Dê um título para a solicitação.", "OK");
            return;
        }

        if (descricao.Length < 10)
        {
            await DisplayAlertAsync("Atenção", "Descreva melhor o que você precisa (mínimo 10 caracteres).", "OK");
            return;
        }

        string endereco;

        if (stk_usar_cadastro.IsVisible && chk_usar_cadastro.IsChecked)
        {
            endereco = EnderecoCadastrado();
        }
        else
        {
            string rua = (txt_rua.Text ?? "").Trim();
            string numero = (txt_numero.Text ?? "").Trim();
            string cidade = (txt_cidade.Text ?? "").Trim();
            string uf = (txt_uf.Text ?? "").Trim().ToUpper();

            if (rua.Length == 0 || cidade.Length == 0 || uf.Length != 2)
            {
                await DisplayAlertAsync("Atenção", "Informe rua, cidade e UF do local do serviço.", "OK");
                return;
            }

            endereco = $"{rua}{(numero.Length > 0 ? ", " + numero : "")} - {cidade}/{uf}";
        }

        // O endereço segue junto da descrição (o backend não tem campo próprio)
        string descricaoFinal = $"{descricao}\n\nEndereço do serviço: {endereco}";

        try
        {
            btn_enviar.IsEnabled = false;

            await ApiService.CriarContratoAsync(_profissional.Id, _escolhido.Id, titulo, descricaoFinal);

            await DisplayAlertAsync("Solicitação enviada!",
                "O profissional foi avisado. Acompanhe a resposta em Agendamentos.", "OK");

            Navegacao.IrParaAba(this, "agendamentos");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ops!", ex.Message, "OK");
            btn_enviar.IsEnabled = true;
        }
    }

    private async void Voltar_Tapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }
}

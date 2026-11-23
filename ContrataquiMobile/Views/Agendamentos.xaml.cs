using System.Collections.ObjectModel;
using ContrataquiMobile.Models;
using ContrataquiMobile.Services;

namespace ContrataquiMobile.Views;

public partial class Agendamentos : ContentPage
{
    private static readonly Color CorTextoAtivo = Color.FromArgb("#002045");
    private static readonly Color CorTextoInativo = Color.FromArgb("#43474E");

    private List<Contrato> _todos = new();
    private bool _verHistorico;
    private bool _carregando;
    private bool _falhou;

    public ObservableCollection<Contrato> Itens { get; } = new();

    public Agendamentos()
    {
        InitializeComponent();
        BindingContext = this;
    }

    // Recarrega sempre que a tela aparece: o status muda do outro lado também
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        cab.AtualizarAvatar();
        await CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        if (_carregando) return;
        _carregando = true;
        _falhou = false;

        try
        {
            if (_todos.Count == 0)
            {
                ind_carregando.IsVisible = true;
                ind_carregando.IsRunning = true;
            }

            lbl_erro.IsVisible = false;
            _todos = await ApiService.ListarContratosAsync();
        }
        catch (Exception ex)
        {
            _falhou = true;
            lbl_erro.Text = $"{ex.Message}\nToque para tentar novamente.";
            lbl_erro.IsVisible = true;
        }
        finally
        {
            ind_carregando.IsRunning = false;
            ind_carregando.IsVisible = false;
            _carregando = false;
        }

        Filtrar();
    }

    private void Filtrar()
    {
        IEnumerable<Contrato> lista = _verHistorico
            ? _todos.Where(c => c.EstaEncerrado)
            : _todos.Where(c => c.EstaAtivo);

        // Quem presta o serviço vê primeiro o que está esperando resposta
        lista = Sessao.EhProfissional
            ? lista.OrderByDescending(c => c.Status == "pendente").ThenByDescending(c => c.Id)
            : lista.OrderByDescending(c => c.Id);

        Itens.Clear();
        foreach (Contrato contrato in lista)
            Itens.Add(contrato);

        stk_vazio.IsVisible = Itens.Count == 0 && !_falhou;

        if (_verHistorico)
        {
            lbl_vazio_titulo.Text = "Nada no histórico";
            lbl_vazio_texto.Text = "Contratos concluídos, cancelados ou recusados aparecem aqui.";
        }
        else if (Sessao.EhProfissional)
        {
            lbl_vazio_titulo.Text = "Nenhum serviço ativo";
            lbl_vazio_texto.Text = "Quando um cliente solicitar seu serviço, ele aparece aqui.";
        }
        else
        {
            lbl_vazio_titulo.Text = "Nenhum agendamento ativo";
            lbl_vazio_texto.Text = "Encontre um profissional no Explorar e solicite um serviço.";
        }
    }

    private void DefinirAba(bool historico)
    {
        _verHistorico = historico;

        brd_ativos.BackgroundColor = historico ? Colors.Transparent : Colors.White;
        brd_historico.BackgroundColor = historico ? Colors.White : Colors.Transparent;
        lbl_ativos.TextColor = historico ? CorTextoInativo : CorTextoAtivo;
        lbl_historico.TextColor = historico ? CorTextoAtivo : CorTextoInativo;

        Filtrar();
    }

    private void Ativos_Tapped(object? sender, TappedEventArgs e) => DefinirAba(false);

    private void Historico_Tapped(object? sender, TappedEventArgs e) => DefinirAba(true);

    private async void Item_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: Contrato contrato })
            await Navigation.PushAsync(new DetalheContrato(contrato.Id));
    }

    private async void Erro_Tapped(object? sender, TappedEventArgs e)
    {
        await CarregarAsync();
    }

    private async void Atualizar_Refreshing(object? sender, EventArgs e)
    {
        await CarregarAsync();
        rfv_lista.IsRefreshing = false;
    }
}

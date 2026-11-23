using System.Collections.ObjectModel;
using ContrataquiMobile.Models;
using ContrataquiMobile.Services;

namespace ContrataquiMobile.Views;

public partial class PerfilProfissional : ContentPage
{
    private const int AvaliacoesPorPagina = 5;

    private readonly int _id;
    private Profissional? _profissional;
    private List<Avaliacao> _avaliacoes = new();
    private bool _carregado;
    private bool _carregando;

    public ObservableCollection<Avaliacao> Itens { get; } = new();

    // Quem está logado é o dono deste perfil (ex.: profissional usando "Ver como o cliente vê")
    private bool EhMeuPerfil => Sessao.Atual?.Id == _id;

    public PerfilProfissional(int id)
    {
        InitializeComponent();
        _id = id;

        BindableLayout.SetItemsSource(stk_avaliacoes, Itens);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_carregado || _carregando) return;
        await CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        _carregando = true;

        try
        {
            ind_carregando.IsVisible = true;
            ind_carregando.IsRunning = true;
            lbl_erro.IsVisible = false;

            // As avaliações carregam junto, mas se falharem o perfil continua funcionando
            Task<List<Avaliacao>?> tarefaAvaliacoes = TentarListarAvaliacoesAsync();

            _profissional = await ApiService.ObterProfissionalAsync(_id);
            List<Avaliacao>? avaliacoes = await tarefaAvaliacoes;

            // Mais recentes primeiro (a API devolve sem ordem)
            _avaliacoes = avaliacoes?.OrderByDescending(a => a.Id).ToList() ?? new();
            _profissional.TotalAvaliacoes = _avaliacoes.Count;

            BindingContext = _profissional;

            Itens.Clear();
            foreach (Avaliacao avaliacao in _avaliacoes.Take(AvaliacoesPorPagina))
                Itens.Add(avaliacao);

            lbl_sem_avaliacoes.Text = avaliacoes is null
                ? "Não foi possível carregar as avaliações."
                : "Nenhuma avaliação ainda.";

            AtualizarAvaliacoes();

            scv_conteudo.IsVisible = true;

            // Só cliente contrata, e ninguém contrata a si mesmo
            stk_cta.IsVisible = !EhMeuPerfil && !Sessao.EhProfissional;

            _carregado = true;
        }
        catch (Exception ex)
        {
            lbl_erro.Text = $"{ex.Message}\nToque para tentar novamente.";
            lbl_erro.IsVisible = true;
        }
        finally
        {
            ind_carregando.IsRunning = false;
            ind_carregando.IsVisible = false;
            _carregando = false;
        }
    }

    private async Task<List<Avaliacao>?> TentarListarAvaliacoesAsync()
    {
        try
        {
            return await ApiService.ListarAvaliacoesProfissionalAsync(_id);
        }
        catch
        {
            return null;
        }
    }

    private void AtualizarAvaliacoes()
    {
        lbl_sem_avaliacoes.IsVisible = Itens.Count == 0;

        int restante = _avaliacoes.Count - Itens.Count;
        btn_ver_mais_avaliacoes.IsVisible = restante > 0;
        btn_ver_mais_avaliacoes.Text = $"Ver mais avaliações ({restante})";
    }

    private void VerMaisAvaliacoes_Clicked(object? sender, EventArgs e)
    {
        foreach (Avaliacao avaliacao in _avaliacoes.Skip(Itens.Count).Take(AvaliacoesPorPagina))
            Itens.Add(avaliacao);

        AtualizarAvaliacoes();
    }

    private async void Erro_Tapped(object? sender, TappedEventArgs e)
    {
        if (_carregando) return;
        await CarregarAsync();
    }

    // Igual ao site: tocar num serviço lista os profissionais que o oferecem
    private async void Servico_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: Servico servico })
            await Navigation.PushAsync(new ListaProfissionais(servico.CategoriaIdEfetivo, servico.Id));
    }

    private async void Contratar_Clicked(object? sender, EventArgs e)
    {
        // Proteção extra: o botão já nem aparece para quem não pode contratar
        if (EhMeuPerfil || Sessao.EhProfissional || _profissional is null) return;

        if (!_profissional.TemServicos)
        {
            await DisplayAlertAsync("Contratar", "Este profissional ainda não cadastrou serviços.", "OK");
            return;
        }

        await Navigation.PushAsync(new Contratar(_profissional));
    }

    private async void Voltar_Tapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }
}
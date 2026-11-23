using System.Collections.ObjectModel;
using ContrataquiMobile.Models;
using ContrataquiMobile.Services;

namespace ContrataquiMobile.Views;

public partial class Notificacoes : ContentPage
{
    private bool _carregando;
    private bool _falhou;

    public ObservableCollection<Notificacao> Itens { get; } = new();

    public Notificacoes()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        if (_carregando) return;

        _carregando = true;
        _falhou = false;

        try
        {
            if (Itens.Count == 0)
            {
                ind_carregando.IsVisible = true;
                ind_carregando.IsRunning = true;
            }

            List<Notificacao> lista = await ApiService.ListarNotificacoesAsync();

            Itens.Clear();
            foreach (Notificacao notificacao in lista)
                Itens.Add(notificacao);
        }
        catch (Exception ex)
        {
            _falhou = true;
            await DisplayAlertAsync("Ops!", ex.Message, "OK");
        }
        finally
        {
            ind_carregando.IsRunning = false;
            ind_carregando.IsVisible = false;
            _carregando = false;
            AtualizarEstado();
        }
    }

    private void AtualizarEstado()
    {
        int naoLidas = Itens.Count(n => !n.Lida);

        lbl_marcar_todas.IsVisible = naoLidas > 0;

        // Se deu erro de rede, não mostra "sem notificações" (não é verdade)
        stk_vazio.IsVisible = Itens.Count == 0 && !_falhou;

        // Atualiza o ponto vermelho do sino nas outras telas
        if (!_falhou)
            Cabecalho.DefinirNaoLidas(naoLidas);
    }

    // Marca como lida e abre o destino do aviso
    private async void Item_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender is not BindableObject bo || bo.BindingContext is not Notificacao notificacao)
            return;

        if (!notificacao.Lida)
        {
            // Atualiza na hora e desfaz se o servidor recusar
            notificacao.Lida = true;
            AtualizarEstado();

            try
            {
                await ApiService.MarcarNotificacaoLidaAsync(notificacao.Id);
            }
            catch (Exception ex)
            {
                notificacao.Lida = false;
                AtualizarEstado();
                await DisplayAlertAsync("Ops!", ex.Message, "OK");
                return;
            }
        }

        await AbrirDestinoAsync(notificacao);
    }

    private async Task AbrirDestinoAsync(Notificacao notificacao)
    {
        switch (notificacao.Tipo)
        {
            case "nova_mensagem":
                Navegacao.IrParaAba(this, "mensagens");
                break;

            // O aviso de avaliação leva ao próprio perfil público (as avaliações aparecem lá)
            case "avaliacao_recebida" when Sessao.Atual is not null:
                await Navigation.PushAsync(new PerfilProfissional(Sessao.Atual.Id));
                break;

            default:
                // novo_contrato, contrato_aceito, contrato_recusado, proposta_recebida, imprevisto_lancado
                if (notificacao.ReferenciaId is int contratoId)
                    await Navigation.PushAsync(new DetalheContrato(contratoId));
                break;
        }
    }

    private async void MarcarTodas_Tapped(object? sender, TappedEventArgs e)
    {
        List<Notificacao> pendentes = Itens.Where(n => !n.Lida).ToList();
        if (pendentes.Count == 0) return;

        try
        {
            await ApiService.MarcarTodasNotificacoesLidasAsync();

            foreach (Notificacao notificacao in pendentes)
                notificacao.Lida = true;

            AtualizarEstado();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ops!", ex.Message, "OK");
        }
    }

    private async void Atualizar_Refreshing(object? sender, EventArgs e)
    {
        await CarregarAsync();
        rfv_lista.IsRefreshing = false;
    }

    private async void Voltar_Tapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }
}
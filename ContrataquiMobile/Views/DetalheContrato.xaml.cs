using ContrataquiMobile.Models;
using ContrataquiMobile.Services;

namespace ContrataquiMobile.Views;

public partial class DetalheContrato : ContentPage
{
    private readonly int _id;
    private Contrato? _contrato;
    private bool _carregando;
    private bool _ocupado;

    public DetalheContrato(int id)
    {
        InitializeComponent();
        _id = id;
    }

    // Recarrega ao voltar das telas de visita, proposta, imprevisto e avaliação
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        if (_carregando) return;
        _carregando = true;

        try
        {
            if (_contrato is null)
            {
                ind_carregando.IsVisible = true;
                ind_carregando.IsRunning = true;
            }

            lbl_erro.IsVisible = false;

            Contrato contrato = await ApiService.ObterContratoAsync(_id);

            // A avaliação do cliente vem de outra rota
            if (contrato.EuSouCliente && contrato.Status == "concluido")
                contrato.MinhaAvaliacao = await TentarObterMinhaAvaliacaoAsync(contrato.Id);

            _contrato = contrato;
            BindingContext = contrato;
            scv_conteudo.IsVisible = true;

            if (contrato.MinhaAvaliacao is { } av)
            {
                lbl_estrelas_cheias.Text = av.EstrelasCheias;
                lbl_estrelas_vazias.Text = av.EstrelasVazias;
                lbl_comentario_avaliacao.Text = av.Comentario ?? "";
                lbl_comentario_avaliacao.IsVisible = av.TemComentario;
            }
        }
        catch (Exception ex)
        {
            if (_contrato is null)
            {
                lbl_erro.Text = $"{ex.Message}\nToque para tentar novamente.";
                lbl_erro.IsVisible = true;
            }
            else
            {
                await DisplayAlertAsync("Ops!", ex.Message, "OK");
            }
        }
        finally
        {
            ind_carregando.IsRunning = false;
            ind_carregando.IsVisible = false;
            _carregando = false;
        }
    }

    private static async Task<Avaliacao?> TentarObterMinhaAvaliacaoAsync(int contratoId)
    {
        try
        {
            return (await ApiService.ListarMinhasAvaliacoesAsync()).FirstOrDefault(a => a.ContratoId == contratoId);
        }
        catch
        {
            return null;
        }
    }

    // Faz a ação, mostra o erro se houver e recarrega a tela
    private async Task ExecutarAsync(Func<Task> acao, string? sucesso = null)
    {
        if (_ocupado) return;
        _ocupado = true;

        try
        {
            await acao();

            if (sucesso is not null)
                await DisplayAlertAsync("Pronto!", sucesso, "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ops!", ex.Message, "OK");
        }
        finally
        {
            _ocupado = false;
        }

        await CarregarAsync();
    }

    private async Task MudarStatusAsync(string status, string? confirmacao = null, string? sucesso = null)
    {
        if (_contrato is null) return;

        if (confirmacao is not null &&
            !await DisplayAlertAsync("Confirmar", confirmacao, "Sim", "Não"))
            return;

        await ExecutarAsync(() => ApiService.AtualizarStatusContratoAsync(_contrato.Id, status), sucesso);
    }

    // ---------- Profissional ----------

    private async void Aceitar_Clicked(object? sender, EventArgs e) =>
        await MudarStatusAsync("aceito", null, "Solicitação aceita. Agora você pode conversar com o cliente.");

    private async void Recusar_Clicked(object? sender, EventArgs e) =>
        await MudarStatusAsync("recusado", "Recusar esta solicitação?");

    private async void Iniciar_Clicked(object? sender, EventArgs e) =>
        await MudarStatusAsync("em_andamento", "Iniciar o serviço agora?");

    private async void Concluir_Clicked(object? sender, EventArgs e) =>
        await MudarStatusAsync("concluido", "Marcar o serviço como concluído?", "Serviço concluído. O cliente já pode te avaliar.");

    private async void AgendarVisita_Clicked(object? sender, EventArgs e)
    {
        if (_contrato is not null)
            await Navigation.PushAsync(new AcaoContrato(_contrato, TipoAcao.Visita));
    }

    private async void EnviarProposta_Clicked(object? sender, EventArgs e)
    {
        if (_contrato is not null)
            await Navigation.PushAsync(new AcaoContrato(_contrato, TipoAcao.Proposta));
    }

    private async void LancarImprevisto_Clicked(object? sender, EventArgs e)
    {
        if (_contrato is not null)
            await Navigation.PushAsync(new AcaoContrato(_contrato, TipoAcao.Imprevisto));
    }

    private async void VisitaRealizada_Clicked(object? sender, EventArgs e)
    {
        if (_contrato is null || sender is not BindableObject { BindingContext: ContratoVisita visita }) return;

        await ExecutarAsync(() => ApiService.MarcarVisitaRealizadaAsync(_contrato.Id, visita.Id));
    }

    // ---------- Cliente ----------

    private async void AceitarProposta_Clicked(object? sender, EventArgs e)
    {
        if (_contrato is null) return;

        await MudarStatusAsync(
            "proposta_aceita",
            $"Aceitar a proposta de {_contrato.ValorBaseTexto}?",
            "Proposta aceita. O profissional vai iniciar o serviço.");
    }

    private async void RecusarProposta_Clicked(object? sender, EventArgs e) =>
        await MudarStatusAsync("em_negociacao", "Recusar a proposta e voltar a negociar?");

    private async void AprovarImprevisto_Clicked(object? sender, EventArgs e) =>
        await ResponderImprevistoAsync(sender, true);

    private async void RecusarImprevisto_Clicked(object? sender, EventArgs e) =>
        await ResponderImprevistoAsync(sender, false);

    private async Task ResponderImprevistoAsync(object? sender, bool aceito)
    {
        if (_contrato is null || sender is not BindableObject { BindingContext: ContratoImprevisto imprevisto })
            return;

        string pergunta = aceito
            ? $"Aprovar o imprevisto de {imprevisto.ValorTexto}? O valor total será atualizado."
            : "Recusar este imprevisto?";

        if (!await DisplayAlertAsync("Confirmar", pergunta, "Sim", "Não")) return;

        await ExecutarAsync(() => ApiService.ResponderImprevistoAsync(_contrato.Id, imprevisto.Id, aceito));
    }

    private async void Avaliar_Clicked(object? sender, EventArgs e)
    {
        if (_contrato is not null)
            await Navigation.PushAsync(new AcaoContrato(_contrato, TipoAcao.Avaliacao));
    }

    // ---------- Os dois ----------

    private async void Cancelar_Clicked(object? sender, EventArgs e) =>
        await MudarStatusAsync("cancelado", "Tem certeza? O contrato será cancelado e não poderá ser retomado.");

    private async void AbrirChat_Clicked(object? sender, EventArgs e)
    {
        if (_contrato is null) return;

        await Navigation.PushAsync(new Conversa(
            _contrato.Id, _contrato.OutroNome, _contrato.OutroFoto, _contrato.Titulo));
    }

    private async void Erro_Tapped(object? sender, TappedEventArgs e)
    {
        await CarregarAsync();
    }

    private async void Voltar_Tapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }
}

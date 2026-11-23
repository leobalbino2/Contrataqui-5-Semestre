using ContrataquiMobile.Models;
using ContrataquiMobile.Services;

namespace ContrataquiMobile.Views;

public enum TipoAcao
{
    Visita,
    Proposta,
    Imprevisto,
    Avaliacao
}

// Uma tela só para as ações que pedem formulário: visita, proposta, imprevisto e avaliação
public partial class AcaoContrato : ContentPage
{
    private static readonly Color EstrelaAcesa = Color.FromArgb("#F59E0B");
    private static readonly Color EstrelaApagada = Color.FromArgb("#D1D5DB");

    private readonly Contrato _contrato;
    private readonly TipoAcao _tipo;
    private readonly Label[] _estrelas;
    private int _nota;
    private bool _ajustandoMascara;

    public AcaoContrato(Contrato contrato, TipoAcao tipo)
    {
        InitializeComponent();

        _contrato = contrato;
        _tipo = tipo;
        _estrelas = new[] { est_1, est_2, est_3, est_4, est_5 };

        switch (tipo)
        {
            case TipoAcao.Visita:
                lbl_titulo_pagina.Text = "Agendar visita";
                lbl_intro.Text = "Escolha quando você vai até o local para avaliar o serviço.";
                btn_confirmar.Text = "Agendar visita";
                stk_visita.IsVisible = true;

                dtp_data.MinimumDate = DateTime.Today;
                dtp_data.Date = DateTime.Today.AddDays(1);
                tmp_hora.Time = new TimeSpan(9, 0, 0);
                break;

            case TipoAcao.Proposta:
                lbl_titulo_pagina.Text = contrato.TemProposta ? "Nova proposta" : "Enviar proposta";
                lbl_intro.Text = "Informe o valor combinado. O cliente poderá aceitar ou pedir ajustes.";
                btn_confirmar.Text = "Enviar proposta";
                stk_proposta.IsVisible = true;

                // Renegociação: já vem com os valores da proposta anterior
                if (contrato.TemProposta)
                {
                    txt_valor_base.Text = Mascaras.Moeda(((long)Math.Round((contrato.ValorBase ?? 0) * 100)).ToString());
                    txt_teto.Text = Mascaras.Moeda(((long)Math.Round((contrato.ValorTetoImprevisto ?? 0) * 100)).ToString());
                    txt_escopo.Text = contrato.PropostaDescricao;
                }
                break;

            case TipoAcao.Imprevisto:
                lbl_titulo_pagina.Text = "Lançar imprevisto";
                lbl_intro.Text = "Registre um custo extra que surgiu durante o serviço.";
                btn_confirmar.Text = "Lançar imprevisto";
                stk_imprevisto.IsVisible = true;
                lbl_margem.Text = $"Margem disponível: {contrato.MargemRestanteTexto}";
                break;

            case TipoAcao.Avaliacao:
                lbl_titulo_pagina.Text = "Avaliar profissional";
                lbl_intro.Text = $"Como foi o serviço com {contrato.OutroNome}?";
                btn_confirmar.Text = "Enviar avaliação";
                stk_avaliacao.IsVisible = true;
                break;
        }
    }

    // Máscara de moeda: o usuário digita só os números e a vírgula aparece sozinha
    private void Moeda_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_ajustandoMascara || sender is not Entry campo) return;

        string formatado = Mascaras.Moeda(e.NewTextValue);
        if (formatado == (e.NewTextValue ?? "")) return;

        _ajustandoMascara = true;
        campo.Text = formatado;
        campo.CursorPosition = formatado.Length;
        _ajustandoMascara = false;
    }

    private void Estrela_Tapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not string texto || !int.TryParse(texto, out int nota)) return;

        _nota = nota;

        for (int i = 0; i < _estrelas.Length; i++)
            _estrelas[i].TextColor = i < nota ? EstrelaAcesa : EstrelaApagada;

        lbl_nota.Text = nota switch
        {
            1 => "Ruim",
            2 => "Regular",
            3 => "Bom",
            4 => "Muito bom",
            _ => "Excelente"
        };
    }

    private async void Confirmar_Clicked(object? sender, EventArgs e)
    {
        try
        {
            btn_confirmar.IsEnabled = false;

            string? sucesso = await ExecutarAsync();
            if (sucesso is null) return; // validação falhou, já avisou

            await DisplayAlertAsync("Pronto!", sucesso, "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ops!", ex.Message, "OK");
        }
        finally
        {
            btn_confirmar.IsEnabled = true;
        }
    }

    // Devolve a mensagem de sucesso, ou null se algum campo estiver inválido
    private async Task<string?> ExecutarAsync()
    {
        switch (_tipo)
        {
            case TipoAcao.Visita:
            {
                    DateTime quando = (dtp_data.Date ?? DateTime.Today.AddDays(1)).Date
                      + (tmp_hora.Time ?? new TimeSpan(9, 0, 0));

                    if (quando <= DateTime.Now)
                {
                    await DisplayAlertAsync("Atenção", "Escolha uma data e horário no futuro.", "OK");
                    return null;
                }

                await ApiService.AgendarVisitaAsync(_contrato.Id, quando, txt_motivo.Text);
                return "Visita agendada.";
            }

            case TipoAcao.Proposta:
            {
                decimal valor = Mascaras.MoedaParaDecimal(txt_valor_base.Text);
                decimal teto = Mascaras.MoedaParaDecimal(txt_teto.Text);

                if (valor <= 0)
                {
                    await DisplayAlertAsync("Atenção", "Informe o valor do serviço.", "OK");
                    return null;
                }

                await ApiService.EnviarPropostaAsync(_contrato.Id, valor, teto, txt_escopo.Text);
                return "Proposta enviada. O cliente foi avisado.";
            }

            case TipoAcao.Imprevisto:
            {
                string descricao = (txt_imprevisto.Text ?? "").Trim();
                decimal valor = Mascaras.MoedaParaDecimal(txt_valor_imprevisto.Text);

                if (descricao.Length < 5)
                {
                    await DisplayAlertAsync("Atenção", "Descreva o imprevisto.", "OK");
                    return null;
                }

                if (valor <= 0)
                {
                    await DisplayAlertAsync("Atenção", "Informe o valor do imprevisto.", "OK");
                    return null;
                }

                bool auto = await ApiService.LancarImprevistoAsync(_contrato.Id, descricao, valor);

                return auto
                    ? "Imprevisto lançado. Coube na margem, então já foi aprovado e entrou no valor total."
                    : "Imprevisto lançado. Passou da margem, então o cliente precisa aprovar.";
            }

            default:
            {
                if (_nota == 0)
                {
                    await DisplayAlertAsync("Atenção", "Toque nas estrelas para dar uma nota.", "OK");
                    return null;
                }

                await ApiService.CriarAvaliacaoAsync(_contrato.Id, _contrato.ProfissionalId, _nota, txt_comentario.Text);
                return "Obrigado pela avaliação!";
            }
        }
    }

    private async void Voltar_Tapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }
}

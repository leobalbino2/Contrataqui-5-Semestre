using System.Text.RegularExpressions;

namespace ContrataquiMobile.Views;

public partial class EsqueceuSenha : ContentPage
{
    private bool _metodoEmail = true;
    private bool _formatando;

    private static readonly Color Primary = Color.FromArgb("#002045");
    private static readonly Color OnSurfaceVariant = Color.FromArgb("#43474E");

    public EsqueceuSenha()
    {
        InitializeComponent();
    }

    private void TabEmail_Tapped(object? sender, TappedEventArgs e) => TrocarMetodo(true);

    private void TabTelefone_Tapped(object? sender, TappedEventArgs e) => TrocarMetodo(false);

    private void TrocarMetodo(bool email)
    {
        _metodoEmail = email;

        tab_email.BackgroundColor = email ? Colors.White : Colors.Transparent;
        tab_telefone.BackgroundColor = email ? Colors.Transparent : Colors.White;
        lbl_tab_email.TextColor = email ? Primary : OnSurfaceVariant;
        lbl_tab_telefone.TextColor = email ? OnSurfaceVariant : Primary;

        _formatando = true;
        txt_recuperacao.Text = "";
        _formatando = false;

        if (email)
        {
            lbl_campo.Text = "E-mail cadastrado";
            txt_recuperacao.Placeholder = "seu@email.com";
            txt_recuperacao.Keyboard = Keyboard.Email;
            txt_recuperacao.MaxLength = -1;
            lbl_ajuda.Text = "Enviaremos um link de acesso para a sua caixa de entrada.";
        }
        else
        {
            lbl_campo.Text = "WhatsApp ou celular com DDD";
            txt_recuperacao.Placeholder = "(11) 98765-4321";
            txt_recuperacao.Keyboard = Keyboard.Telephone;
            txt_recuperacao.MaxLength = 15;
            lbl_ajuda.Text = "Você receberá um código de 6 dígitos via WhatsApp ou SMS.";
        }
    }

    private void Recuperacao_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_metodoEmail || _formatando) return;
        _formatando = true;

        var digitos = new string((e.NewTextValue ?? "").Where(char.IsDigit).ToArray());
        if (digitos.Length > 11) digitos = digitos[..11];

        txt_recuperacao.Text = digitos.Length switch
        {
            0 => "",
            <= 2 => $"({digitos}",
            <= 7 => $"({digitos[..2]}) {digitos[2..]}",
            _ => $"({digitos[..2]}) {digitos[2..7]}-{digitos[7..]}"
        };

        _formatando = false;
    }

    private async void Enviar_Clicked(object? sender, EventArgs e)
    {
        string valor = txt_recuperacao.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(valor))
        {
            await DisplayAlertAsync("Atenção", "Preencha o campo para continuar.", "OK");
            return;
        }

        if (_metodoEmail && !Regex.IsMatch(valor, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
        {
            await DisplayAlertAsync("Atenção", "Informe um e-mail válido.", "OK");
            return;
        }

        if (!_metodoEmail && valor.Count(char.IsDigit) < 10)
        {
            await DisplayAlertAsync("Atenção", "Informe um telefone válido com DDD.", "OK");
            return;
        }

        try
        {
            btn_enviar.IsEnabled = false;
            btn_enviar.Text = "Enviando...";

            // TODO: chamar a API de recuperação de senha aqui
            await Task.Delay(800);

            await DisplayAlertAsync("Pronto!", "Instruções enviadas com sucesso.", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }
        finally
        {
            btn_enviar.IsEnabled = true;
            btn_enviar.Text = "Enviar instruções";
        }
    }

    private async void Voltar_Clicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
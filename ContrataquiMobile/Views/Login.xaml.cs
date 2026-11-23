using ContrataquiMobile.Models;
using ContrataquiMobile.Services;

namespace ContrataquiMobile.Views;

public partial class Login : ContentPage
{
    public Login()
    {
        InitializeComponent();
    }

    private async void Button_Clicked(object? sender, EventArgs e)
    {
        try
        {
            btn_entrar.IsEnabled = false;

            string email = txt_email.Text?.Trim() ?? "";
            string senha = txt_senha.Text ?? "";

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(senha))
            {
                await DisplayAlertAsync("Ops!", "Informe email e senha.", "OK");
                return;
            }

            Usuario usuario = await ApiService.LoginAsync(email, senha);

            Sessao.Iniciar(usuario);
            Navegacao.IrParaInicio(usuario);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ops!", ex.Message, "OK");
        }
        finally
        {
            btn_entrar.IsEnabled = true;
        }
    }

    private void ToggleSenha_Tapped(object? sender, TappedEventArgs e)
    {
        txt_senha.IsPassword = !txt_senha.IsPassword;
        lbl_toggle_senha.Text = txt_senha.IsPassword ? "Mostrar" : "Ocultar";
    }

    private async void EsqueceuSenha_Tapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new EsqueceuSenha());
    }

    private async void CriarConta_Tapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new Cadastro());
    }
}
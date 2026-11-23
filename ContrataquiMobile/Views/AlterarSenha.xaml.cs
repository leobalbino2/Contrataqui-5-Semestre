using ContrataquiMobile.Services;

namespace ContrataquiMobile.Views;

public partial class AlterarSenha : ContentPage
{
    public AlterarSenha()
    {
        InitializeComponent();
    }

    private void Mostrar_Tapped(object? sender, TappedEventArgs e)
    {
        bool esconder = !txt_atual.IsPassword;

        txt_atual.IsPassword = esconder;
        txt_nova.IsPassword = esconder;
        txt_confirmar.IsPassword = esconder;

        lbl_mostrar.Text = esconder ? "Mostrar senhas" : "Ocultar senhas";
    }

    private async void Alterar_Clicked(object? sender, EventArgs e)
    {
        string atual = txt_atual.Text ?? "";
        string nova = txt_nova.Text ?? "";
        string confirmar = txt_confirmar.Text ?? "";

        if (string.IsNullOrEmpty(atual) || string.IsNullOrEmpty(nova) || string.IsNullOrEmpty(confirmar))
        {
            await DisplayAlertAsync("Atenção", "Preencha todos os campos.", "OK");
            return;
        }

        if (nova.Length < 8)
        {
            await DisplayAlertAsync("Atenção", "A nova senha deve ter no mínimo 8 caracteres.", "OK");
            return;
        }

        if (nova != confirmar)
        {
            await DisplayAlertAsync("Atenção", "As senhas não coincidem.", "OK");
            return;
        }

        if (nova == atual)
        {
            await DisplayAlertAsync("Atenção", "A nova senha deve ser diferente da atual.", "OK");
            return;
        }

        try
        {
            btn_alterar.IsEnabled = false;

            await ApiService.AlterarSenhaAsync(atual, nova);

            await DisplayAlertAsync("Pronto!", "Senha alterada com sucesso.", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ops!", ex.Message, "OK");
            btn_alterar.IsEnabled = true;
        }
    }

    private async void Voltar_Tapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }
}
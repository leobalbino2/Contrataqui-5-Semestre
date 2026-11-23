using ContrataquiMobile.Models;
using ContrataquiMobile.Services;

namespace ContrataquiMobile.Views;

public partial class ExcluirConta : ContentPage
{
    public ExcluirConta()
    {
        InitializeComponent();
    }

    private async void Excluir_Clicked(object? sender, EventArgs e)
    {
        string senha = txt_senha.Text ?? "";

        if (string.IsNullOrEmpty(senha))
        {
            await DisplayAlertAsync("Atenção", "Informe sua senha para confirmar.", "OK");
            return;
        }

        bool confirmar = await DisplayAlertAsync(
            "Excluir conta",
            "Tem certeza? Esta ação não pode ser desfeita.",
            "Excluir",
            "Cancelar");

        if (!confirmar) return;

        try
        {
            btn_excluir.IsEnabled = false;

            await ApiService.ExcluirContaAsync(senha);

            Sessao.Encerrar();

            await DisplayAlertAsync("Conta excluída", "Sua conta foi excluída.", "OK");

            if (Window is not null)
                Window.Page = new AppShell();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ops!", ex.Message, "OK");
            btn_excluir.IsEnabled = true;
        }
    }

    private async void Cancelar_Clicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void Voltar_Tapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }
}
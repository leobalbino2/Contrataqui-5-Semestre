using ContrataquiMobile.Models;

namespace ContrataquiMobile.Views;

public partial class Configuracoes : ContentPage
{
    public Configuracoes()
    {
        InitializeComponent();
        lbl_versao.Text = $"Contrataqui · versão {AppInfo.Current.VersionString}";
    }

    private async void AlterarSenha_Tapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new AlterarSenha());
    }

    private async void Termos_Tapped(object? sender, TappedEventArgs e)
    {
        await DisplayAlertAsync("Termos de Uso", "Em breve.", "OK");
    }

    private async void Privacidade_Tapped(object? sender, TappedEventArgs e)
    {
        await DisplayAlertAsync("Política de Privacidade", "Em breve.", "OK");
    }

    private async void Sair_Tapped(object? sender, TappedEventArgs e)
    {
        bool confirmar = await DisplayAlertAsync(
            "Sair da conta",
            "Deseja realmente sair?",
            "Sim",
            "Não");

        if (!confirmar || Window is null) return;

        Sessao.Encerrar();
        Window.Page = new AppShell();
    }

    private async void Excluir_Tapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new ExcluirConta());
    }

    private async void Voltar_Tapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }
}
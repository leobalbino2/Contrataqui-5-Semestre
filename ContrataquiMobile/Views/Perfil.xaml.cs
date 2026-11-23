using ContrataquiMobile.Models;
using ContrataquiMobile.Services;

namespace ContrataquiMobile.Views;

public partial class Perfil : ContentPage
{
    private PerfilUsuario? _perfil;
    private bool _carregando;

    public Perfil()
    {
        InitializeComponent();
    }

    // Recarrega sempre que a tela aparece: assim as edições aparecem ao voltar
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

        try
        {
            // Só mostra o indicador na primeira vez; depois atualiza por baixo
            if (_perfil is null)
            {
                ind_carregando.IsVisible = true;
                ind_carregando.IsRunning = true;
            }

            lbl_erro.IsVisible = false;

            PerfilUsuario perfil = await ApiService.ObterMeuPerfilAsync();

            _perfil = perfil;
            BindingContext = perfil;
            scv_conteudo.IsVisible = true;

            cab.AtualizarAvatar();
        }
        catch (Exception ex)
        {
            // Se já havia dados na tela, mantém. Senão, mostra o erro.
            if (_perfil is null)
            {
                lbl_erro.Text = $"{ex.Message}\nToque para tentar novamente.";
                lbl_erro.IsVisible = true;
            }
        }
        finally
        {
            ind_carregando.IsRunning = false;
            ind_carregando.IsVisible = false;
            _carregando = false;
        }
    }

    private async void Erro_Tapped(object? sender, TappedEventArgs e)
    {
        await CarregarAsync();
    }

    private async void Editar_Clicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new EditarPerfil());
    }

    private async void VerComoCliente_Clicked(object? sender, EventArgs e)
    {
        if (_perfil is null) return;
        await Navigation.PushAsync(new PerfilProfissional(_perfil.Id));
    }
}
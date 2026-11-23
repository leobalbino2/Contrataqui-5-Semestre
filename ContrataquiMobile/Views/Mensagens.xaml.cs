using System.Collections.ObjectModel;
using ContrataquiMobile.Models;
using ContrataquiMobile.Services;

namespace ContrataquiMobile.Views;

public partial class Mensagens : ContentPage
{
    private bool _carregando;
    private bool _carregouUmaVez;

    public ObservableCollection<Chat> Itens { get; } = new();

    public Mensagens()
    {
        InitializeComponent();
        BindingContext = this;
    }

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

        bool falhou = false;

        try
        {
            if (!_carregouUmaVez)
            {
                ind_carregando.IsVisible = true;
                ind_carregando.IsRunning = true;
            }

            lbl_erro.IsVisible = false;

            List<Chat> chats = Chat.Normalizar(await ApiService.ListarChatsAsync());

            Itens.Clear();
            foreach (Chat chat in chats)
                Itens.Add(chat);

            _carregouUmaVez = true;
        }
        catch (Exception ex)
        {
            falhou = true;
            lbl_erro.Text = $"{ex.Message}\nToque para tentar novamente.";
            lbl_erro.IsVisible = true;
        }
        finally
        {
            ind_carregando.IsRunning = false;
            ind_carregando.IsVisible = false;
            _carregando = false;
        }

        stk_vazio.IsVisible = Itens.Count == 0 && !falhou;
    }

    private async void Item_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender is not BindableObject { BindingContext: Chat chat }) return;

        await Navigation.PushAsync(new Conversa(
            chat.ContratoId, chat.OutroNome, chat.OutroFoto, chat.Titulo, chat.Id));
    }

    private async void Erro_Tapped(object? sender, TappedEventArgs e)
    {
        await CarregarAsync();
    }

    private async void Atualizar_Refreshing(object? sender, EventArgs e)
    {
        await CarregarAsync();
        rfv_lista.IsRefreshing = false;
    }
}

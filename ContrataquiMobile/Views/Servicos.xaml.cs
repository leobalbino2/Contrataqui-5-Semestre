using System.Collections.ObjectModel;
using ContrataquiMobile.Models;
using ContrataquiMobile.Services;

namespace ContrataquiMobile.Views;

public partial class Servicos : ContentPage
{
    private bool _carregado;

    public Categoria Categoria { get; }

    public ObservableCollection<Servico> Itens { get; } = new();

    public Servicos(Categoria categoria)
    {
        InitializeComponent();

        Categoria = categoria;
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_carregado) return;
        await CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        try
        {
            ind_carregando.IsVisible = true;
            ind_carregando.IsRunning = true;
            lbl_vazio.IsVisible = false;

            List<Servico> lista = await ApiService.ListarServicosPorCategoriaAsync(Categoria.Id);

            Itens.Clear();
            foreach (Servico servico in lista)
                Itens.Add(servico);

            _carregado = true;
            lbl_vazio.IsVisible = Itens.Count == 0;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ops!", ex.Message, "OK");
        }
        finally
        {
            ind_carregando.IsRunning = false;
            ind_carregando.IsVisible = false;
        }
    }

    private async void BuscarProfissionais_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject bo && bo.BindingContext is Servico servico)
            await Navigation.PushAsync(new ListaProfissionais(servico.CategoriaIdEfetivo, servico.Id));
    }

    private async void Voltar_Tapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }
}
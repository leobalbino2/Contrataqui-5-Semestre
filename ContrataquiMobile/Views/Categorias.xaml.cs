using System.Collections.ObjectModel;
using System.Globalization;
using ContrataquiMobile.Models;

namespace ContrataquiMobile.Views;

public partial class Categorias : ContentPage
{
    public ObservableCollection<Categoria> Resultado { get; } = new();

    public Categorias(Categoria? selecionada = null)
    {
        InitializeComponent();
        BindingContext = this;

        // Igual ao site: abre já filtrada na categoria tocada
        if (selecionada is not null)
            txt_busca.Text = selecionada.Nome;

        Filtrar();
    }

    private void Busca_TextChanged(object? sender, TextChangedEventArgs e) => Filtrar();

    private void Filtrar()
    {
        string termo = (txt_busca.Text ?? "").Trim();
        var comparador = CultureInfo.CurrentCulture.CompareInfo;

        // Casa pelo começo de qualquer palavra do nome, sem diferenciar maiúscula nem acento
        var lista = CategoriaDados.Todas
            .Where(c => c.Nome.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Any(p => comparador.IsPrefix(p, termo, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace)))
            .ToList();

        Resultado.Clear();
        foreach (var categoria in lista)
            Resultado.Add(categoria);

        lbl_vazio.IsVisible = Resultado.Count == 0;
    }

    private async void VerServicos_Tapped(object? sender, TappedEventArgs e)
    {
        // TODO: abrir a tela de serviços da categoria
        if (sender is BindableObject bo && bo.BindingContext is Categoria categoria)
            await DisplayAlertAsync(categoria.Nome, "Lista de serviços em breve.", "OK");
    }

    private async void Voltar_Tapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }
}
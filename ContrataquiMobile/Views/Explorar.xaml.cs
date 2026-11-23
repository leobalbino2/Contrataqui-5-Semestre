using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using ContrataquiMobile.Models;
using ContrataquiMobile.Services;

namespace ContrataquiMobile.Views;

public partial class Explorar : ContentPage
{
    private const int TotalCategorias = 6;
    private const int TotalProfissionais = 5;

    private static List<Categoria>? _cacheCategorias;

    private List<Profissional> _profissionais = new();
    private List<Profissional> _filtrados = new();
    private bool _carregado;
    private bool _profissionaisProntos;

    public ObservableCollection<Categoria> Categorias { get; } = new();
    public ObservableCollection<Profissional> Profissionais { get; } = new();

    public Explorar()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_carregado) return;
        _carregado = true;

        await Task.WhenAll(CarregarCategoriasAsync(), CarregarProfissionaisAsync());
    }

    // ---------- Carregamento ----------

    private async Task CarregarCategoriasAsync()
    {
        if (Categorias.Count > 0) return;

        List<Categoria>? lista = _cacheCategorias;

        if (lista is null)
        {
            try
            {
                lista = await ApiService.ListarCategoriasAsync();
                if (lista.Count > 0)
                    _cacheCategorias = lista;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao carregar categorias: {ex.Message}");
            }
        }

        // Reserva: se a API falhar ou vier vazia, usa a lista local
        if (lista is null || lista.Count == 0)
            lista = CategoriaDados.Todas;

        foreach (Categoria categoria in lista.Take(TotalCategorias))
            Categorias.Add(categoria);
    }

    private async Task CarregarProfissionaisAsync()
    {
        try
        {
            ind_carregando.IsVisible = true;
            ind_carregando.IsRunning = true;
            lbl_vazio.IsVisible = false;

            _profissionais = await ApiService.ListarProfissionaisAsync();
            _profissionaisProntos = true;

            Filtrar();
        }
        catch (Exception ex)
        {
            lbl_vazio.Text = ex.Message;
            lbl_vazio.IsVisible = true;
        }
        finally
        {
            ind_carregando.IsRunning = false;
            ind_carregando.IsVisible = false;
        }
    }

    // ---------- Busca ----------

    private void Busca_TextChanged(object? sender, TextChangedEventArgs e) => Filtrar();

    // Botão Buscar (ou Enter): abre a lista completa já com o termo digitado
    private async void Buscar_Clicked(object? sender, EventArgs e)
    {
        txt_busca.Unfocus();
        await AbrirListaCompletaAsync();
    }

    // Todas as palavras digitadas precisam aparecer no nome, cidade, serviços ou categorias,
    // sem diferenciar maiúscula nem acento. Mostra só os 5 primeiros (os mais bem avaliados).
    private void Filtrar()
    {
        if (!_profissionaisProntos) return;

        string[] palavras = (txt_busca.Text ?? "")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var comparador = CultureInfo.CurrentCulture.CompareInfo;
        const CompareOptions opcoes = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;

        IEnumerable<Profissional> lista = _profissionais;

        if (palavras.Length > 0)
            lista = lista.Where(p => palavras.All(w => comparador.IndexOf(p.TextoBusca, w, opcoes) >= 0));

        _filtrados = lista.OrderByDescending(p => p.AvaliacaoNumero).ToList();

        Profissionais.Clear();
        foreach (Profissional profissional in _filtrados.Take(TotalProfissionais))
            Profissionais.Add(profissional);

        bool buscando = palavras.Length > 0;
        int total = _filtrados.Count;

        lbl_titulo_prof.Text = buscando ? "Resultados da busca" : "Mais bem avaliados";
        lbl_subtitulo_prof.Text = buscando
            ? (total == 1 ? "1 profissional encontrado" : $"{total} profissionais encontrados")
            : "Profissionais verificados com as melhores notas";

        lbl_vazio.Text = "Nenhum profissional encontrado.";
        lbl_vazio.IsVisible = total == 0;

        btn_ver_todos.IsVisible = total > Profissionais.Count;
        btn_ver_todos.Text = buscando
            ? $"Ver todos os {total} resultados"
            : $"Ver todos os profissionais ({total})";
    }

    // ---------- Navegação ----------

    private async Task AbrirListaCompletaAsync()
    {
        string termo = (txt_busca.Text ?? "").Trim();
        await Navigation.PushAsync(new ListaProfissionais(busca: termo));
    }

    private async void VerTodosProfissionais_Tapped(object? sender, TappedEventArgs e) =>
        await AbrirListaCompletaAsync();

    private async void VerTodosProfissionais_Clicked(object? sender, EventArgs e) =>
        await AbrirListaCompletaAsync();

    private async void VerTodas_Tapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new Categorias());
    }

    private async void Categoria_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject bo && bo.BindingContext is Categoria categoria)
            await Navigation.PushAsync(new Servicos(categoria));
    }

    private async void ComoFunciona_Tapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new ComoFunciona());
    }

    private async void VerPerfil_Clicked(object? sender, EventArgs e)
    {
        if (sender is BindableObject bo && bo.BindingContext is Profissional pro)
            await Navigation.PushAsync(new PerfilProfissional(pro.Id));
    }
}
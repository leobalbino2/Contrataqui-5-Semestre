using System.Collections.ObjectModel;
using System.Globalization;
using ContrataquiMobile.Models;
using ContrataquiMobile.Services;

namespace ContrataquiMobile.Views;

// Um filtro ativo, mostrado como etiqueta removível
public record ChipFiltro(string Chave, string Texto);

public partial class ListaProfissionais : ContentPage
{
    private const int TamanhoPagina = 10;

    private static readonly string[] Ufs =
    {
        "AC","AL","AP","AM","BA","CE","DF","ES","GO","MA","MT","MS","MG",
        "PA","PB","PR","PE","PI","RJ","RN","RS","RO","RR","SC","SP","SE","TO"
    };

    private static readonly decimal[] AvaliacoesMin = { 0m, 3m, 4m, 4.5m };
    private static readonly string[] Ordens = { "avaliacao", "nome" };

    private static readonly CultureInfo PtBr = new("pt-BR");
    private static readonly CompareInfo Comparador = PtBr.CompareInfo;
    private const CompareOptions SemAcento = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;

    private List<Profissional> _todos = new();
    private List<Profissional> _filtrados = new();
    private List<Categoria> _categorias = new();
    private List<Servico> _servicos = new();
    private List<Servico> _servicosDaCategoria = new();

    // Estado dos filtros
    private int? _categoriaId;
    private int? _servicoId;
    private string _estado = "";
    private string _cidade = "";
    private string _busca = "";
    private decimal _avaliacaoMin;
    private string _ordem = "avaliacao";

    private bool _carregado;
    private bool _carregando;
    private bool _montando;

    public ObservableCollection<Profissional> Itens { get; } = new();
    public ObservableCollection<ChipFiltro> Chips { get; } = new();

    public ListaProfissionais(int? categoriaId = null, int? servicoId = null, string? busca = null)
    {
        InitializeComponent();
        BindingContext = this;

        _categoriaId = categoriaId;
        _servicoId = servicoId;
        _busca = busca?.Trim() ?? "";

        _montando = true;
        pck_estado.ItemsSource = new[] { "Todos" }.Concat(Ufs).ToList();
        pck_avaliacao.ItemsSource = new[] { "Qualquer avaliação", "3★ ou mais", "4★ ou mais", "4,5★ ou mais" };
        pck_ordem.ItemsSource = new[] { "Melhor avaliação", "Nome (A-Z)" };
        pck_estado.SelectedIndex = 0;
        pck_avaliacao.SelectedIndex = 0;
        pck_ordem.SelectedIndex = 0;
        txt_busca.Text = _busca;
        _montando = false;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_carregado || _carregando) return;
        await CarregarAsync();
    }

    // ---------- Carregamento ----------

    private async Task CarregarAsync()
    {
        _carregando = true;

        try
        {
            ind_carregando.IsVisible = true;
            ind_carregando.IsRunning = true;
            lbl_erro.IsVisible = false;

            Task<List<Profissional>> profissionais = ApiService.ListarProfissionaisAsync();
            Task<List<Categoria>> categorias = ApiService.ListarCategoriasAsync();
            Task<List<Servico>> servicos = ApiService.ListarServicosAsync();

            _todos = await profissionais;

            try
            {
                _categorias = await categorias;
                _servicos = await servicos;
            }
            catch
            {
                // Sem categorias, os filtros de categoria e serviço ficam vazios, mas a lista funciona
            }

            MontarFiltros();

            _carregado = true;
            Aplicar();
        }
        catch (Exception ex)
        {
            lbl_erro.Text = $"{ex.Message}\nToque para tentar novamente.";
            lbl_erro.IsVisible = true;
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
        if (_carregando) return;
        await CarregarAsync();
    }

    // ---------- Filtros: montagem e sincronia com os campos ----------

    private void MontarFiltros()
    {
        // Se veio só o serviço, descobre a categoria dele
        if (_servicoId is int sid && _categoriaId is null)
            _categoriaId = _servicos.FirstOrDefault(s => s.Id == sid)?.CategoriaIdEfetivo;

        _montando = true;
        pck_categoria.ItemsSource = new[] { "Todas" }.Concat(_categorias.Select(c => c.Nome)).ToList();
        _montando = false;

        SincronizarFiltros();
    }

    // Leva o estado (_categoriaId, _estado...) para os campos da folha, sem disparar os eventos deles
    private void SincronizarFiltros()
    {
        bool estavaMontando = _montando;
        _montando = true;

        if ((txt_busca.Text ?? "").Trim() != _busca)
            txt_busca.Text = _busca;

        pck_categoria.SelectedIndex = _categoriaId is int c
            ? _categorias.FindIndex(x => x.Id == c) + 1
            : 0;

        AtualizarServicosDaCategoria();

        pck_estado.SelectedIndex = _estado.Length == 0
            ? 0
            : Math.Max(0, Array.IndexOf(Ufs, _estado) + 1);

        if ((txt_cidade.Text ?? "") != _cidade)
            txt_cidade.Text = _cidade;

        pck_avaliacao.SelectedIndex = Math.Max(0, Array.IndexOf(AvaliacoesMin, _avaliacaoMin));
        pck_ordem.SelectedIndex = Math.Max(0, Array.IndexOf(Ordens, _ordem));

        _montando = estavaMontando;
    }

    private void AtualizarServicosDaCategoria()
    {
        bool estavaMontando = _montando;
        _montando = true;

        _servicosDaCategoria = _categoriaId is int c
            ? _servicos.Where(s => s.CategoriaIdEfetivo == c).ToList()
            : new();

        pck_servico.ItemsSource = new[] { "Todos" }.Concat(_servicosDaCategoria.Select(s => s.Nome)).ToList();
        pck_servico.IsEnabled = _categoriaId is not null;
        lbl_dica_servico.IsVisible = _categoriaId is null;

        int indice = _servicoId is int sid
            ? _servicosDaCategoria.FindIndex(s => s.Id == sid) + 1
            : 0;

        if (indice == 0)
            _servicoId = null;

        pck_servico.SelectedIndex = indice;

        _montando = estavaMontando;
    }

    // ---------- Filtros: eventos dos campos ----------

    private void Busca_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_montando) return;

        _busca = (e.NewTextValue ?? "").Trim();
        Aplicar();
    }

    private void Categoria_Changed(object? sender, EventArgs e)
    {
        if (_montando) return;

        int i = pck_categoria.SelectedIndex;
        _categoriaId = i > 0 && i <= _categorias.Count ? _categorias[i - 1].Id : null;
        _servicoId = null;

        AtualizarServicosDaCategoria();
        Aplicar();
    }

    private void Servico_Changed(object? sender, EventArgs e)
    {
        if (_montando) return;

        int i = pck_servico.SelectedIndex;
        _servicoId = i > 0 && i <= _servicosDaCategoria.Count ? _servicosDaCategoria[i - 1].Id : null;

        Aplicar();
    }

    private void Estado_Changed(object? sender, EventArgs e)
    {
        if (_montando) return;

        int i = pck_estado.SelectedIndex;
        _estado = i > 0 && i <= Ufs.Length ? Ufs[i - 1] : "";

        Aplicar();
    }

    private void Cidade_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_montando) return;

        _cidade = (e.NewTextValue ?? "").Trim();
        Aplicar();
    }

    private void Avaliacao_Changed(object? sender, EventArgs e)
    {
        if (_montando) return;

        int i = pck_avaliacao.SelectedIndex;
        _avaliacaoMin = i >= 0 && i < AvaliacoesMin.Length ? AvaliacoesMin[i] : 0;

        Aplicar();
    }

    private void Ordem_Changed(object? sender, EventArgs e)
    {
        if (_montando) return;

        int i = pck_ordem.SelectedIndex;
        _ordem = i >= 0 && i < Ordens.Length ? Ordens[i] : "avaliacao";

        Aplicar();
    }

    private void Chip_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender is not BindableObject { BindingContext: ChipFiltro chip }) return;

        switch (chip.Chave)
        {
            case "categoria": _categoriaId = null; _servicoId = null; break;
            case "servico": _servicoId = null; break;
            case "estado": _estado = ""; break;
            case "cidade": _cidade = ""; break;
            case "avaliacao": _avaliacaoMin = 0; break;
        }

        SincronizarFiltros();
        Aplicar();
    }

    private void Limpar_Tapped(object? sender, TappedEventArgs e)
    {
        _categoriaId = null;
        _servicoId = null;
        _estado = "";
        _cidade = "";
        _busca = "";
        _avaliacaoMin = 0;
        _ordem = "avaliacao";

        SincronizarFiltros();
        Aplicar();
    }

    // ---------- Aplicar filtros ----------

    private void Aplicar()
    {
        if (!_carregado) return;

        string[] palavras = _busca.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        IEnumerable<Profissional> lista = _todos;

        // Todas as palavras precisam aparecer no nome, cidade, serviços ou categorias
        if (palavras.Length > 0)
            lista = lista.Where(p => palavras.All(w => Comparador.IndexOf(p.TextoBusca, w, SemAcento) >= 0));

        if (_servicoId is int servicoId)
            lista = lista.Where(p => p.ListaServicos.Any(s => s.Id == servicoId));
        else if (_categoriaId is int categoriaId)
            lista = lista.Where(p => p.ListaServicos.Any(s => s.CategoriaIdEfetivo == categoriaId));

        if (_estado.Length > 0)
            lista = lista.Where(p => string.Equals(p.Estado?.Trim(), _estado, StringComparison.OrdinalIgnoreCase));

        if (_cidade.Length > 0)
            lista = lista.Where(p => Comparador.IndexOf(p.Cidade ?? "", _cidade, SemAcento) >= 0);

        if (_avaliacaoMin > 0)
            lista = lista.Where(p => p.AvaliacaoNumero >= _avaliacaoMin);

        lista = _ordem == "nome"
            ? lista.OrderBy(p => p.Nome, StringComparer.Create(PtBr, true))
            : lista.OrderByDescending(p => p.AvaliacaoNumero);

        _filtrados = lista.ToList();

        Itens.Clear();
        foreach (Profissional profissional in _filtrados.Take(TamanhoPagina))
            Itens.Add(profissional);

        AtualizarEstado();
    }

    private void VerMais_Clicked(object? sender, EventArgs e)
    {
        foreach (Profissional profissional in _filtrados.Skip(Itens.Count).Take(TamanhoPagina))
            Itens.Add(profissional);

        AtualizarEstado();
    }

    private void AtualizarEstado()
    {
        int total = _filtrados.Count;

        lbl_contagem.Text = total == 1 ? "1 profissional encontrado" : $"{total} profissionais encontrados";
        lbl_vazio.IsVisible = total == 0;

        int restante = total - Itens.Count;
        btn_ver_mais.IsVisible = restante > 0;
        btn_ver_mais.Text = $"Ver mais ({restante})";

        // Etiquetas dos filtros ativos (só recria se mudou)
        var novos = new List<ChipFiltro>();

        if (_categoriaId is int c)
            novos.Add(new ChipFiltro("categoria", _categorias.FirstOrDefault(x => x.Id == c)?.Nome ?? "Categoria"));

        if (_servicoId is int s)
            novos.Add(new ChipFiltro("servico", _servicos.FirstOrDefault(x => x.Id == s)?.Nome ?? "Serviço"));

        if (_estado.Length > 0)
            novos.Add(new ChipFiltro("estado", _estado));

        if (_cidade.Length > 0)
            novos.Add(new ChipFiltro("cidade", _cidade));

        if (_avaliacaoMin > 0)
            novos.Add(new ChipFiltro("avaliacao", $"{_avaliacaoMin.ToString("0.#", PtBr)}★ ou mais"));

        if (!novos.SequenceEqual(Chips))
        {
            Chips.Clear();
            foreach (ChipFiltro chip in novos)
                Chips.Add(chip);
        }

        flx_chips.IsVisible = Chips.Count > 0;

        bdg_filtros.IsVisible = Chips.Count > 0;
        lbl_filtros_badge.Text = Chips.Count.ToString();

        lbl_limpar.IsVisible = Chips.Count > 0 || _busca.Length > 0;

        btn_aplicar.Text = total == 1 ? "Ver 1 resultado" : $"Ver {total} resultados";
    }

    // ---------- Folha de filtros ----------

    private async void Filtros_Tapped(object? sender, TappedEventArgs e)
    {
        txt_busca.Unfocus();

        double altura = Math.Max(320, Math.Min(620, Height * 0.85));

        bdr_sheet.HeightRequest = altura;
        bdr_sheet.TranslationY = altura;
        fundo_filtros.Opacity = 0;
        grd_filtros.IsVisible = true;

        await Task.WhenAll(
            fundo_filtros.FadeToAsync(1, 150),
            bdr_sheet.TranslateToAsync(0, 0, 200, Easing.CubicOut));
    }

    private async Task FecharFiltrosAsync()
    {
        if (!grd_filtros.IsVisible) return;

        await Task.WhenAll(
            fundo_filtros.FadeToAsync(0, 120),
            bdr_sheet.TranslateToAsync(0, bdr_sheet.Height, 160, Easing.CubicIn));

        grd_filtros.IsVisible = false;
    }

    private async void FecharFiltros_Tapped(object? sender, TappedEventArgs e) => await FecharFiltrosAsync();

    private async void Aplicar_Clicked(object? sender, EventArgs e) => await FecharFiltrosAsync();

    private void Folha_Tapped(object? sender, TappedEventArgs e)
    {
        // Só engole o toque pra não fechar a folha
    }

    // ---------- Navegação ----------

    private async void VerPerfil_Clicked(object? sender, EventArgs e) => await AbrirPerfilAsync(sender);

    private async void Card_Tapped(object? sender, TappedEventArgs e) => await AbrirPerfilAsync(sender);

    private async Task AbrirPerfilAsync(object? origem)
    {
        if (origem is BindableObject { BindingContext: Profissional profissional })
            await Navigation.PushAsync(new PerfilProfissional(profissional.Id));
    }

    private async void Voltar_Tapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }
}
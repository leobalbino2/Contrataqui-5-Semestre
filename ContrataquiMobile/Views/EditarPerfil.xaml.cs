using ContrataquiMobile.Models;
using ContrataquiMobile.Services;

namespace ContrataquiMobile.Views;

public partial class EditarPerfil : ContentPage
{
    private static readonly Color Secondary = Color.FromArgb("#13696A");
    private static readonly Color OnSurface = Color.FromArgb("#111C2C");

    private static readonly string[] Ufs =
    {
        "AC","AL","AP","AM","BA","CE","DF","ES","GO","MA","MT","MS","MG",
        "PA","PB","PR","PE","PI","RJ","RN","RS","RO","RR","SC","SP","SE","TO"
    };

    private PerfilUsuario? _perfil;
    private FileResult? _foto;

    private List<Categoria> _categorias = new();
    private List<Servico> _servicos = new();
    private HashSet<int> _selecionados = new();

    private bool _montando;
    private bool _formatando;
    private bool _carregado;
    private bool _carregando;
    private bool _salvando;

    public EditarPerfil()
    {
        InitializeComponent();
        pck_estado.ItemsSource = Ufs;
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

            PerfilUsuario perfil = await ApiService.ObterMeuPerfilAsync();

            if (perfil.EhProfissional)
            {
                _categorias = await ApiService.ListarCategoriasAsync();
                _servicos = await ApiService.ListarServicosAsync();
            }

            _perfil = perfil;
            Preencher(perfil);

            scv_conteudo.IsVisible = true;
            stk_botoes.IsVisible = true;
            _carregado = true;
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

    private void Preencher(PerfilUsuario perfil)
    {
        _montando = true;

        txt_nome.Text = perfil.Nome;
        txt_telefone.Text = Mascaras.Telefone(perfil.Telefone);
        txt_cep.Text = Mascaras.Cep(perfil.Cep);
        txt_cidade.Text = perfil.Cidade ?? "";
        txt_numero.Text = perfil.Numero ?? "";
        txt_rua.Text = perfil.Rua ?? "";
        txt_complemento.Text = perfil.Complemento ?? "";

        pck_estado.SelectedIndex = Array.IndexOf(Ufs, (perfil.Estado ?? "").Trim().ToUpperInvariant());

        if (Uri.TryCreate(perfil.ImagemPerfil, UriKind.Absolute, out Uri? uri))
        {
            img_foto.Source = ImageSource.FromUri(uri);
            MostrarFoto(true);
        }

        if (perfil.EhProfissional)
        {
            stk_sobre.IsVisible = true;
            brd_servicos.IsVisible = true;
            lbl_titulo_endereco.Text = "Área de atuação";

            txt_descricao.Text = perfil.Descricao ?? "";

            _selecionados = perfil.ListaServicos.Select(s => s.Id).ToHashSet();

            pck_categoria.ItemsSource = new[] { "Todas as categorias" }
                .Concat(_categorias.Select(c => c.Nome))
                .ToList();
            pck_categoria.SelectedIndex = 0;

            MontarListaServicos();
        }

        _montando = false;
    }

    private void MostrarFoto(bool mostrar)
    {
        img_foto.IsVisible = mostrar;
        lbl_foto_vazia.IsVisible = !mostrar;
        lbl_trocar_foto.Text = mostrar ? "Trocar foto" : "Adicionar foto";
    }

    // ---------- Foto ----------

    private async void Foto_Tapped(object? sender, TappedEventArgs e)
    {
        try
        {
            var fotos = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions { SelectionLimit = 1 });
            FileResult? foto = fotos?.FirstOrDefault();
            if (foto is null) return;

            _foto = foto;
            img_foto.Source = ImageSource.FromFile(foto.FullPath);
            MostrarFoto(true);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ops!", ex.Message, "OK");
        }
    }

    // ---------- Máscaras ----------

    private void Telefone_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_formatando) return;
        _formatando = true;

        txt_telefone.Text = Mascaras.Telefone(e.NewTextValue);

        _formatando = false;
    }

    private void Cep_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_formatando) return;
        _formatando = true;

        txt_cep.Text = Mascaras.Cep(e.NewTextValue);

        _formatando = false;
    }

    // ---------- Serviços (profissional) ----------

    private void Categoria_Changed(object? sender, EventArgs e)
    {
        if (_montando) return;
        MontarListaServicos();
    }

    // A seleção é mantida ao trocar de categoria; a lista só filtra o que aparece
    private void MontarListaServicos()
    {
        stk_servicos.Children.Clear();

        int i = pck_categoria.SelectedIndex;
        int? categoriaId = i > 0 && i <= _categorias.Count ? _categorias[i - 1].Id : null;

        IEnumerable<Servico> lista = categoriaId is int c
            ? _servicos.Where(s => s.CategoriaIdEfetivo == c)
            : _servicos;

        foreach (Servico servico in lista)
        {
            var check = new CheckBox
            {
                Color = Secondary,
                VerticalOptions = LayoutOptions.Center,
                IsChecked = _selecionados.Contains(servico.Id)
            };
            check.CheckedChanged += (s, args) => ToggleServico(servico.Id, args.Value);

            var texto = new Label
            {
                Text = servico.Nome,
                FontSize = 13,
                TextColor = OnSurface,
                VerticalOptions = LayoutOptions.Center
            };
            texto.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => check.IsChecked = !check.IsChecked)
            });

            stk_servicos.Children.Add(new HorizontalStackLayout
            {
                Spacing = 6,
                Children = { check, texto }
            });
        }

        AtualizarResumoServicos();
    }

    private void ToggleServico(int id, bool marcado)
    {
        if (marcado) _selecionados.Add(id);
        else _selecionados.Remove(id);

        AtualizarResumoServicos();
    }

    private void AtualizarResumoServicos()
    {
        int n = _selecionados.Count;

        lbl_qtd_servicos.Text = n switch
        {
            0 => "Nenhum serviço selecionado",
            1 => "1 serviço selecionado",
            _ => $"{n} serviços selecionados"
        };
    }

    // ---------- Salvar ----------

    private async void Salvar_Clicked(object? sender, EventArgs e)
    {
        if (_perfil is null || _salvando) return;

        string nome = txt_nome.Text?.Trim() ?? "";
        string telefone = txt_telefone.Text ?? "";
        string cep = txt_cep.Text ?? "";
        string cidade = txt_cidade.Text?.Trim() ?? "";
        string estado = pck_estado.SelectedIndex >= 0 ? Ufs[pck_estado.SelectedIndex] : "";

        if (string.IsNullOrWhiteSpace(nome))
        {
            await DisplayAlertAsync("Atenção", "O nome é obrigatório.", "OK");
            return;
        }

        if (telefone.Length > 0 && Mascaras.SomenteDigitos(telefone).Length < 10)
        {
            await DisplayAlertAsync("Atenção", "Informe um telefone válido com DDD.", "OK");
            return;
        }

        if (cep.Length > 0 && Mascaras.SomenteDigitos(cep).Length != 8)
        {
            await DisplayAlertAsync("Atenção", "Informe um CEP válido.", "OK");
            return;
        }

        if (_perfil.EhProfissional)
        {
            if (estado.Length == 0 || cidade.Length == 0)
            {
                await DisplayAlertAsync("Atenção", "Informe o estado e a cidade de atuação.", "OK");
                return;
            }

            if (_selecionados.Count == 0)
            {
                await DisplayAlertAsync("Atenção", "Selecione pelo menos um serviço.", "OK");
                return;
            }
        }

        try
        {
            _salvando = true;
            btn_salvar.IsEnabled = false;
            btn_cancelar.IsEnabled = false;
            btn_salvar.Text = "Salvando...";

            // Foto nova: sobe pro Cloudinary e pega a URL
            string? urlFoto = null;
            if (_foto is not null)
            {
                using Stream stream = await _foto.OpenReadAsync();
                urlFoto = await ApiService.EnviarImagemAsync(stream, _foto.ContentType ?? "image/jpeg");
            }

            var dados = new Dictionary<string, object?>
            {
                ["nome"] = nome,
                ["telefone"] = telefone,
                ["estado"] = estado,
                ["cidade"] = cidade,
                ["cep"] = cep,
                ["rua"] = txt_rua.Text?.Trim() ?? "",
                ["numero"] = txt_numero.Text?.Trim() ?? "",
                ["complemento"] = txt_complemento.Text?.Trim() ?? ""
            };

            if (_perfil.EhProfissional)
            {
                dados["descricao"] = txt_descricao.Text?.Trim() ?? "";
                dados["servico_ids"] = _selecionados.ToList();
            }

            if (urlFoto is not null)
                dados["imagem_perfil"] = urlFoto;

            await ApiService.AtualizarPerfilAsync(dados);

            // Mantém a sessão do aparelho em dia (nome no menu e no avatar)
            if (Sessao.Atual is Usuario atual)
            {
                atual.Nome = nome;
                atual.Telefone = telefone;
                Sessao.Iniciar(atual);
            }

            await DisplayAlertAsync("Pronto!", "Perfil atualizado.", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ops!", ex.Message, "OK");

            _salvando = false;
            btn_salvar.IsEnabled = true;
            btn_cancelar.IsEnabled = true;
            btn_salvar.Text = "Salvar alterações";
        }
    }

    // ---------- Navegação ----------

    private async void Voltar_Clicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void Voltar_Tapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }
}
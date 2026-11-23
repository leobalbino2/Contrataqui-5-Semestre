using System.Text.RegularExpressions;
using ContrataquiMobile.Models;
using ContrataquiMobile.Services;

namespace ContrataquiMobile.Views;

public partial class CadastroProfissional : ContentPage
{
    private const int TotalEtapas = 3;

    private int _etapa = 1;
    private bool _formatando;
    private bool _catalogoCarregado;
    private FileResult? _foto;

    private List<Categoria> _categorias = new();
    private List<Servico> _servicos = new();
    private readonly List<Servico> _servicosSelecionados = new();

    private static readonly Color Secondary = Color.FromArgb("#13696A");
    private static readonly Color OnSurface = Color.FromArgb("#111C2C");

    private static readonly string[] Etapas =
    {
        "Tipos de serviço",
        "Dados pessoais",
        "Dados profissionais"
    };

    private static readonly string[] Ufs =
    {
        "AC","AL","AP","AM","BA","CE","DF","ES","GO","MA","MT","MS","MG",
        "PA","PB","PR","PE","PI","RJ","RN","RS","RO","RR","SC","SP","SE","TO"
    };

    public CadastroProfissional()
    {
        InitializeComponent();

        pck_estado.ItemsSource = Ufs;
        pck_modalidade.ItemsSource = new[] { "Pessoa Física (CPF)", "Pessoa Jurídica (CNPJ)" };
        pck_modalidade.SelectedIndex = 0;

        AtualizarEtapa();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_catalogoCarregado) return;

        try
        {
            _categorias = await ApiService.ListarCategoriasAsync();
            _servicos = await ApiService.ListarServicosAsync();

            pck_categoria.ItemsSource = _categorias.Select(c => c.Nome).ToList();
            _catalogoCarregado = true;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ops!", ex.Message, "OK");
        }
    }

    // ---------- Navegação entre etapas ----------

    private void AtualizarEtapa()
    {
        stk_etapa1.IsVisible = _etapa == 1;
        stk_etapa2.IsVisible = _etapa == 2;
        stk_etapa3.IsVisible = _etapa == 3;

        double progresso = (double)_etapa / TotalEtapas;
        pgb_etapas.Progress = progresso;
        lbl_passo.Text = $"PASSO {_etapa} DE {TotalEtapas}";
        lbl_percentual.Text = $"{(int)Math.Round(progresso * 100)}% concluído";
        lbl_etapa_nome.Text = Etapas[_etapa - 1];

        btn_avancar.Text = _etapa == TotalEtapas ? "Cadastrar" : "Continuar";
    }

    private async void Voltar_Clicked(object? sender, EventArgs e)
    {
        if (_etapa == 1)
        {
            await Navigation.PopAsync();
            return;
        }

        _etapa--;
        AtualizarEtapa();
    }

    private async void Avancar_Clicked(object? sender, EventArgs e)
    {
        if (!await ValidarEtapaAsync()) return;

        if (_etapa < TotalEtapas)
        {
            _etapa++;
            AtualizarEtapa();
            return;
        }

        await FinalizarCadastroAsync();
    }

    private async void Login_Tapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PopToRootAsync();
    }

    // ---------- Etapa 1: serviços ----------

    private void Categoria_Changed(object? sender, EventArgs e)
    {
        if (pck_categoria.SelectedIndex < 0 || pck_categoria.SelectedIndex >= _categorias.Count) return;

        Categoria categoria = _categorias[pck_categoria.SelectedIndex];

        // Igual ao site: trocar de categoria zera a seleção
        _servicosSelecionados.Clear();
        stk_servicos.Children.Clear();

        foreach (Servico servico in _servicos.Where(s => s.CategoriaIdEfetivo == categoria.Id))
        {
            var check = new CheckBox { Color = Secondary, VerticalOptions = LayoutOptions.Center };
            check.CheckedChanged += (s, args) => ToggleServico(servico, args.Value);

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

        stk_servicos_container.IsVisible = true;
        AtualizarSelecionados();
    }

    private void ToggleServico(Servico servico, bool marcado)
    {
        if (marcado && !_servicosSelecionados.Any(s => s.Id == servico.Id))
            _servicosSelecionados.Add(servico);
        else if (!marcado)
            _servicosSelecionados.RemoveAll(s => s.Id == servico.Id);

        AtualizarSelecionados();
    }

    private void AtualizarSelecionados()
    {
        brd_selecionados.IsVisible = _servicosSelecionados.Count > 0;
        lbl_selecionados.Text = string.Join("\n", _servicosSelecionados.Select(s => $"• {s.Nome}"));
    }

    // ---------- Etapa 2: dados pessoais ----------

    private async void Foto_Tapped(object? sender, TappedEventArgs e)
    {
        try
        {
            var fotos = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions { SelectionLimit = 1 });
            FileResult? foto = fotos?.FirstOrDefault();
            if (foto is null) return;

            _foto = foto;
            img_foto.Source = ImageSource.FromFile(foto.FullPath);
            img_foto.IsVisible = true;
            lbl_foto_vazia.IsVisible = false;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ops!", ex.Message, "OK");
        }
    }

    private void ToggleSenha_Tapped(object? sender, TappedEventArgs e)
    {
        txt_senha.IsPassword = !txt_senha.IsPassword;
        lbl_toggle_senha.Text = txt_senha.IsPassword ? "Mostrar" : "Ocultar";
    }

    // ---------- Etapa 3: dados profissionais ----------

    private bool EhCnpj => pck_modalidade.SelectedIndex == 1;

    private void Modalidade_Changed(object? sender, EventArgs e)
    {
        stk_cpf.IsVisible = !EhCnpj;
        stk_cnpj.IsVisible = EhCnpj;
    }

    // ---------- Máscaras ----------

    private void Telefone_TextChanged(object? sender, TextChangedEventArgs e) =>
        Mascarar(txt_telefone, e, 11, d => d.Length switch
        {
            0 => "",
            <= 2 => $"({d}",
            <= 7 => $"({d[..2]}) {d[2..]}",
            _ => $"({d[..2]}) {d[2..7]}-{d[7..]}"
        });

    private void Cpf_TextChanged(object? sender, TextChangedEventArgs e) =>
        Mascarar(txt_cpf, e, 11, d => d.Length switch
        {
            <= 3 => d,
            <= 6 => $"{d[..3]}.{d[3..]}",
            <= 9 => $"{d[..3]}.{d[3..6]}.{d[6..]}",
            _ => $"{d[..3]}.{d[3..6]}.{d[6..9]}-{d[9..]}"
        });

    private void Cnpj_TextChanged(object? sender, TextChangedEventArgs e) =>
        Mascarar(txt_cnpj, e, 14, d => d.Length switch
        {
            <= 2 => d,
            <= 5 => $"{d[..2]}.{d[2..]}",
            <= 8 => $"{d[..2]}.{d[2..5]}.{d[5..]}",
            <= 12 => $"{d[..2]}.{d[2..5]}.{d[5..8]}/{d[8..]}",
            _ => $"{d[..2]}.{d[2..5]}.{d[5..8]}/{d[8..12]}-{d[12..]}"
        });

    private void Cep_TextChanged(object? sender, TextChangedEventArgs e) =>
        Mascarar(txt_cep, e, 8, d => d.Length <= 5 ? d : $"{d[..5]}-{d[5..]}");

    private void Mascarar(Entry campo, TextChangedEventArgs e, int maxDigitos, Func<string, string> formatar)
    {
        if (_formatando) return;
        _formatando = true;

        string digitos = SomenteDigitos(e.NewTextValue);
        if (digitos.Length > maxDigitos) digitos = digitos[..maxDigitos];

        campo.Text = formatar(digitos);
        _formatando = false;
    }

    private static string SomenteDigitos(string? texto) =>
        new((texto ?? "").Where(char.IsDigit).ToArray());

    // ---------- Validação ----------

    private async Task<bool> ValidarEtapaAsync()
    {
        switch (_etapa)
        {
            case 1:
                if (_servicosSelecionados.Count == 0)
                    return await Erro("Selecione pelo menos um serviço.");
                return true;

            case 2:
                string nome = txt_nome.Text?.Trim() ?? "";
                string email = txt_email.Text?.Trim() ?? "";
                string telefone = txt_telefone.Text ?? "";
                string senha = txt_senha.Text ?? "";

                if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(telefone) || string.IsNullOrWhiteSpace(senha))
                    return await Erro("Preencha todos os campos obrigatórios.");

                if (!Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
                    return await Erro("Informe um e-mail válido.");

                if (SomenteDigitos(telefone).Length < 10)
                    return await Erro("Informe um telefone válido com DDD.");

                if (senha.Length < 8)
                    return await Erro("A senha deve ter no mínimo 8 caracteres.");

                if (!chk_termos.IsChecked)
                    return await Erro("Aceite os Termos de Uso e a Política de Privacidade.");

                return true;

            case 3:
                if (EhCnpj)
                {
                    if (SomenteDigitos(txt_cnpj.Text).Length != 14)
                        return await Erro("Informe um CNPJ válido.");
                }
                else if (SomenteDigitos(txt_cpf.Text).Length != 11)
                {
                    return await Erro("Informe um CPF válido.");
                }

                if (pck_estado.SelectedIndex < 0 ||
                    string.IsNullOrWhiteSpace(txt_cidade.Text) ||
                    string.IsNullOrWhiteSpace(txt_rua.Text) ||
                    string.IsNullOrWhiteSpace(txt_numero.Text))
                    return await Erro("Preencha estado, cidade, rua e número.");

                if (SomenteDigitos(txt_cep.Text).Length != 8)
                    return await Erro("Informe um CEP válido.");

                return true;
        }

        return true;
    }

    private async Task<bool> Erro(string mensagem)
    {
        await DisplayAlertAsync("Atenção", mensagem, "OK");
        return false;
    }

    // ---------- Envio ----------

    private async Task FinalizarCadastroAsync()
    {
        try
        {
            btn_avancar.IsEnabled = false;
            btn_voltar.IsEnabled = false;
            btn_avancar.Text = "Cadastrando...";

            string email = txt_email.Text!.Trim();
            string senha = txt_senha.Text!;

            // 1) Foto (opcional): sobe pro Cloudinary e pega a URL
            string? urlFoto = null;
            if (_foto is not null)
            {
                using Stream stream = await _foto.OpenReadAsync();
                urlFoto = await ApiService.EnviarImagemAsync(stream, _foto.ContentType ?? "image/jpeg");
            }

            // 2) Cadastro
            var dados = new Dictionary<string, object?>
            {
                ["tipo"] = "profissional",
                ["nome"] = txt_nome.Text!.Trim(),
                ["email"] = email,
                ["senha"] = senha,
                ["telefone"] = txt_telefone.Text,
                ["estado"] = Ufs[pck_estado.SelectedIndex],
                ["cidade"] = txt_cidade.Text!.Trim(),
                ["cep"] = txt_cep.Text,
                ["rua"] = txt_rua.Text!.Trim(),
                ["numero"] = txt_numero.Text!.Trim(),
                ["complemento"] = txt_complemento.Text?.Trim(),
                ["descricao"] = txt_descricao.Text?.Trim(),
                ["imagem_perfil"] = urlFoto,
                ["servico_ids"] = _servicosSelecionados.Select(s => s.Id).ToList()
            };

            if (EhCnpj)
            {
                dados["cnpj"] = txt_cnpj.Text;
                dados["nome_fantasia"] = txt_nome_fantasia.Text?.Trim();
            }
            else
            {
                dados["cpf"] = txt_cpf.Text;
            }

            await ApiService.CadastrarAsync(dados);

            // 3) O cadastro não devolve token, então já faz o login pra entrar logado
            Usuario usuario = await ApiService.LoginAsync(email, senha);

            Sessao.Iniciar(usuario);
            Navegacao.IrParaInicio(usuario);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ops!", ex.Message, "OK");
            btn_avancar.IsEnabled = true;
            btn_voltar.IsEnabled = true;
            AtualizarEtapa();
        }
    }
}
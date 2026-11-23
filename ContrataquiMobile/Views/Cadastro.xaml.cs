using System.Text.RegularExpressions;
using ContrataquiMobile.Models;
using ContrataquiMobile.Services;

namespace ContrataquiMobile.Views;

public partial class Cadastro : ContentPage
{
    private bool _formatandoTelefone;

    public Cadastro()
    {
        InitializeComponent();
    }

    private void Telefone_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_formatandoTelefone) return;
        _formatandoTelefone = true;

        var digitos = new string((e.NewTextValue ?? "").Where(char.IsDigit).ToArray());
        if (digitos.Length > 11) digitos = digitos[..11];

        txt_telefone.Text = digitos.Length switch
        {
            0 => "",
            <= 2 => $"({digitos}",
            <= 7 => $"({digitos[..2]}) {digitos[2..]}",
            _ => $"({digitos[..2]}) {digitos[2..7]}-{digitos[7..]}"
        };

        _formatandoTelefone = false;
    }

    private void ToggleSenha_Tapped(object? sender, TappedEventArgs e)
    {
        txt_senha.IsPassword = !txt_senha.IsPassword;
        lbl_toggle_senha.Text = txt_senha.IsPassword ? "Mostrar" : "Ocultar";
    }

    private void ToggleConfirmar_Tapped(object? sender, TappedEventArgs e)
    {
        txt_confirmar_senha.IsPassword = !txt_confirmar_senha.IsPassword;
        lbl_toggle_confirmar.Text = txt_confirmar_senha.IsPassword ? "Mostrar" : "Ocultar";
    }

    private async void SouProfissional_Tapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new CadastroProfissional());
    }

    private async void Cadastrar_Clicked(object? sender, EventArgs e)
    {
        string nome = txt_nome.Text?.Trim() ?? "";
        string email = txt_email.Text?.Trim() ?? "";
        string telefone = txt_telefone.Text?.Trim() ?? "";
        string senha = txt_senha.Text ?? "";
        string confirmar = txt_confirmar_senha.Text ?? "";

        if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(telefone) || string.IsNullOrWhiteSpace(senha) ||
            string.IsNullOrWhiteSpace(confirmar))
        {
            await DisplayAlertAsync("Atenção", "Preencha todos os campos.", "OK");
            return;
        }

        if (!Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
        {
            await DisplayAlertAsync("Atenção", "Informe um e-mail válido.", "OK");
            return;
        }

        if (telefone.Count(char.IsDigit) < 10)
        {
            await DisplayAlertAsync("Atenção", "Informe um telefone válido com DDD.", "OK");
            return;
        }

        if (senha.Length < 8)
        {
            await DisplayAlertAsync("Atenção", "A senha deve ter no mínimo 8 caracteres.", "OK");
            return;
        }

        if (senha != confirmar)
        {
            await DisplayAlertAsync("Atenção", "As senhas não coincidem.", "OK");
            return;
        }

        if (!chk_termos.IsChecked)
        {
            await DisplayAlertAsync("Atenção", "Aceite os Termos de Uso e a Política de Privacidade.", "OK");
            return;
        }

        try
        {
            btn_cadastrar.IsEnabled = false;

            await ApiService.CadastrarAsync(new()
            {
                ["nome"] = nome,
                ["email"] = email,
                ["senha"] = senha,
                ["tipo"] = "cliente",
                ["telefone"] = telefone
            });

            // O cadastro não devolve token, então já faz o login pra entrar logado
            Usuario usuario = await ApiService.LoginAsync(email, senha);

            Sessao.Iniciar(usuario);
            Navegacao.IrParaInicio(usuario);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ops!", ex.Message, "OK");
            btn_cadastrar.IsEnabled = true;
        }
    }

    private async void Login_Tapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }
}
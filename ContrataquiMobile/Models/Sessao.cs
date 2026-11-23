using System.Text.Json;
using ContrataquiMobile.Services;

namespace ContrataquiMobile.Models;

public static class Sessao
{
    private const string Chave = "sessao_usuario";

    // Só o que vai pro aparelho (sem senha)
    private record SessaoSalva(int Id, string Nome, string Email, TipoUsuario Tipo);

    public static Usuario? Atual { get; private set; }

    public static bool EstaLogado => Atual is not null;
    public static bool EhProfissional => Atual?.Tipo == TipoUsuario.Profissional;
    public static bool EhCliente => Atual?.Tipo == TipoUsuario.Cliente;

    public static void Iniciar(Usuario usuario)
    {
        Atual = usuario;

        var salva = new SessaoSalva(usuario.Id, usuario.Nome ?? "", usuario.Email ?? "", usuario.Tipo);
        Preferences.Default.Set(Chave, JsonSerializer.Serialize(salva));
    }

    public static bool Restaurar()
    {
        try
        {
            string json = Preferences.Default.Get(Chave, "");
            if (string.IsNullOrEmpty(json)) return false;

            var salva = JsonSerializer.Deserialize<SessaoSalva>(json);
            if (salva is null) return false;

            Atual = new Usuario
            {
                Id = salva.Id,
                Nome = salva.Nome,
                Email = salva.Email,
                Tipo = salva.Tipo
            };
            return true;
        }
        catch
        {
            Encerrar();
            return false;
        }
    }

    public static void Encerrar()
    {
        Atual = null;
        Preferences.Default.Remove(Chave);
        ApiService.Sair();
    }
}
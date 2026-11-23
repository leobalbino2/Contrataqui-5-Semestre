using System.Text.Json.Serialization;

namespace ContrataquiMobile.Models;

// Dados do usuário logado (GET /usuarios/perfil)
public class PerfilUsuario
{
    private List<Servico>? _listaServicos;

    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Telefone { get; set; }

    // "cliente" ou "profissional"
    public string Tipo { get; set; } = "";

    [JsonPropertyName("imagem_perfil")]
    public string? ImagemPerfil { get; set; }

    public string? Estado { get; set; }
    public string? Cidade { get; set; }
    public string? Cep { get; set; }
    public string? Rua { get; set; }
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Descricao { get; set; }

    [JsonPropertyName("servicos")]
    public List<ProfissionalServico> Vinculos { get; set; } = new();

    // ---------- Só pra exibição ----------

    public bool EhProfissional => Tipo == "profissional";

    public string TipoTexto => EhProfissional ? "Profissional" : "Cliente";

    public string Iniciais =>
        string.Concat(Nome.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                          .Take(2)
                          .Select(p => char.ToUpper(p[0])));

    public bool TemFoto => !string.IsNullOrEmpty(ImagemPerfil);

    public bool TemTelefone => !string.IsNullOrWhiteSpace(Telefone);

    public string Local =>
        string.Join(", ", new[] { Cidade, Estado }.Where(t => !string.IsNullOrWhiteSpace(t)));

    public bool TemLocal => Local.Length > 0;

    // "Rua, número - complemento"
    public string Endereco
    {
        get
        {
            string linha = string.Join(", ", new[] { Rua, Numero }.Where(t => !string.IsNullOrWhiteSpace(t)));

            if (!string.IsNullOrWhiteSpace(Complemento))
                linha = linha.Length > 0 ? $"{linha} - {Complemento}" : Complemento!;

            return linha;
        }
    }

    public string CepTexto => string.IsNullOrWhiteSpace(Cep) ? "" : $"CEP {Cep}";

    public bool TemEndereco => Endereco.Length > 0 || CepTexto.Length > 0;

    public bool SemContato => !TemTelefone && !TemLocal && !TemEndereco;

    public bool TemDescricao => !string.IsNullOrWhiteSpace(Descricao);

    public List<Servico> ListaServicos =>
        _listaServicos ??= Vinculos.Select(v => v.Servico).OfType<Servico>().ToList();

    public bool TemServicos => ListaServicos.Count > 0;

    public bool MostrarSobre => EhProfissional && TemDescricao;

    public bool MostrarServicos => EhProfissional && TemServicos;
}
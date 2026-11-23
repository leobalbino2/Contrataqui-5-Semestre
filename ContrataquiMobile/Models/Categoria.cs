using System.Text.Json.Serialization;

namespace ContrataquiMobile.Models;

public class Categoria
{
    private string? icone;

    public int Id { get; set; }

    // A API chama de "titulo"; no app continua sendo Nome pra não mexer nos XAML
    [JsonPropertyName("titulo")]
    public string Nome { get; set; } = "";

    public string Slug { get; set; } = "";

    // Nome do arquivo do ícone do app, vindo do banco (coluna icone_app)
    [JsonPropertyName("icone_app")]
    public string? IconeApp { get; set; }

    // Ordem: ícone do banco, depois mapeamento por nome, depois a imagem do site
    public string? Icone
    {
        get => IconeApp ?? CategoriaDados.IconeLocal(Nome) ?? icone;
        set => icone = value;
    }

    // Foto grande do site (URL), usada no topo da tela de serviços
    [JsonIgnore]
    public string? ImagemSite => icone;

    public string Descricao { get; set; } = "";
}
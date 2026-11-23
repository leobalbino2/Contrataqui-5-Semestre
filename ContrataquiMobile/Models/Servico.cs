using System.Text.Json.Serialization;

namespace ContrataquiMobile.Models;

public class Servico
{
    public int Id { get; set; }

    // A API chama de "titulo"
    [JsonPropertyName("titulo")]
    public string Nome { get; set; } = "";

    public string Slug { get; set; } = "";

    public string? Descricao { get; set; }

    // URL da imagem do serviço
    public string? Imagem { get; set; }

    [JsonPropertyName("categoria_id")]
    public int CategoriaId { get; set; }

    public Categoria? Categoria { get; set; }

    [JsonIgnore]
    public int CategoriaIdEfetivo => Categoria?.Id ?? CategoriaId;
}
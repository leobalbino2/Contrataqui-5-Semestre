using System.Text.Json.Serialization;

namespace ContrataquiMobile.Models;

public class AvaliacaoCliente
{
    public int Id { get; set; }

    public string Nome { get; set; } = "";

    [JsonPropertyName("imagem_perfil")]
    public string? ImagemPerfil { get; set; }
}

public class AvaliacaoContrato
{
    public string? Titulo { get; set; }

    [JsonPropertyName("data_conclusao")]
    public DateTime? DataConclusao { get; set; }
}

public class Avaliacao
{
    public int Id { get; set; }

    [JsonPropertyName("contrato_id")]
    public int ContratoId { get; set; }

    public int Nota { get; set; }

    public string? Comentario { get; set; }

    public AvaliacaoCliente? Cliente { get; set; }

    public AvaliacaoContrato? Contrato { get; set; }

    // ---------- Só pra exibição ----------

    public string ClienteNome =>
        string.IsNullOrWhiteSpace(Cliente?.Nome) ? "Cliente" : Cliente!.Nome;

    public string ClienteInicial => char.ToUpper(ClienteNome[0]).ToString();

    public string? ClienteImagem => Cliente?.ImagemPerfil;

    public bool TemFoto => !string.IsNullOrEmpty(Cliente?.ImagemPerfil);

    public string EstrelasCheias => new('★', Math.Clamp(Nota, 0, 5));

    public string EstrelasVazias => new('☆', 5 - Math.Clamp(Nota, 0, 5));

    public bool TemServico => !string.IsNullOrWhiteSpace(Contrato?.Titulo);

    public string ServicoTexto => $"Serviço: {Contrato?.Titulo}";

    public bool TemComentario => !string.IsNullOrWhiteSpace(Comentario);
}
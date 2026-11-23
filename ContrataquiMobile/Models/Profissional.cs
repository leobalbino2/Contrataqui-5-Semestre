using System.Globalization;
using System.Text.Json.Serialization;

namespace ContrataquiMobile.Models;

// Vínculo profissional <-> serviço (tabela usuario_servicos)
public class ProfissionalServico
{
    public int Id { get; set; }
    public Servico? Servico { get; set; }
}

public class Profissional
{
    private static readonly CultureInfo PtBr = new("pt-BR");

    private List<Servico>? _listaServicos;
    private string? _textoBusca;

    public int Id { get; set; }

    public string Nome { get; set; } = "";

    [JsonPropertyName("nome_fantasia")]
    public string? NomeFantasia { get; set; }

    public string? Descricao { get; set; }

    [JsonPropertyName("imagem_perfil")]
    public string? ImagemPerfil { get; set; }

    public string? Cidade { get; set; }

    public string? Estado { get; set; }

    [JsonPropertyName("ano_fundacao")]
    public int? AnoFundacao { get; set; }

    // O Prisma manda Decimal como texto ("4.50"); isso aceita texto ou número
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? Avaliacao { get; set; }

    [JsonPropertyName("valor_referencia")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? ValorReferencia { get; set; }

    // Vem da API como "servicos": [{ servico: { ..., categoria } }]
    [JsonPropertyName("servicos")]
    public List<ProfissionalServico> Vinculos { get; set; } = new();

    // Quantidade de avaliações recebidas (preenchida pelo app, não vem neste JSON)
    [JsonIgnore]
    public int TotalAvaliacoes { get; set; }

    // ---------- Só pra exibição e busca ----------

    public List<Servico> ListaServicos =>
        _listaServicos ??= Vinculos.Select(v => v.Servico).OfType<Servico>().ToList();

    public bool TemServicos => ListaServicos.Count > 0;

    public int QtdServicos => ListaServicos.Count;

    public string ServicosTexto => string.Join(" · ", ListaServicos.Select(s => s.Nome));

    // Primeiro serviço + quantos mais existem (ex.: "Instalação de tomadas  +2")
    public string ServicoPrincipal
    {
        get
        {
            var lista = ListaServicos;
            if (lista.Count == 0) return "";
            return lista.Count > 1 ? $"{lista[0].Nome}  +{lista.Count - 1}" : lista[0].Nome;
        }
    }

    // Área de atuação, em frase (ex.: "Especialista em Elétrica", "Atua em Construção e Limpeza")
    public string Especialidade
    {
        get
        {
            var categorias = ListaServicos
                .Select(s => s.Categoria?.Nome)
                .OfType<string>()
                .Where(n => n.Length > 0)
                .Distinct()
                .ToList();

            return categorias.Count switch
            {
                0 => "Profissional verificado",
                1 => $"Especialista em {categorias[0]}",
                2 => $"Atua em {categorias[0]} e {categorias[1]}",
                _ => $"Atua em {categorias[0]}, {categorias[1]} e mais {categorias.Count - 2}"
            };
        }
    }

    public string Iniciais =>
        string.Concat(Nome.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                          .Take(2)
                          .Select(p => char.ToUpper(p[0])));

    public bool TemFoto => !string.IsNullOrEmpty(ImagemPerfil);

    public bool TemNota => Avaliacao is > 0;

    public decimal AvaliacaoNumero => Avaliacao ?? 0;

    public string Nota => TemNota ? Avaliacao!.Value.ToString("0.0", PtBr) : "Novo";

    public string TotalAvaliacoesTexto => TotalAvaliacoes switch
    {
        0 => "",
        1 => " (1 avaliação)",
        _ => $" ({TotalAvaliacoes} avaliações)"
    };

    public string Local =>
        string.Join(", ", new[] { Cidade, Estado }.Where(t => !string.IsNullOrWhiteSpace(t)));

    public bool TemLocal => Local.Length > 0;

    public bool TemDescricao => !string.IsNullOrWhiteSpace(Descricao);

    public string DescricaoResumo =>
        TemDescricao ? Descricao! : "Profissional qualificado e verificado.";

    public bool TemAnoFundacao => AnoFundacao is > 0;

    public string DesdeTexto => TemAnoFundacao ? $"Desde {AnoFundacao}" : "";

    public bool TemValorReferencia => ValorReferencia is > 0;

    public string ValorReferenciaTexto => ValorReferencia?.ToString("C", PtBr) ?? "";

    // Nome, cidade, serviços e categorias juntos, só pra busca
    public string TextoBusca => _textoBusca ??= string.Join(" ",
        new[] { Nome, NomeFantasia, Cidade }
            .Concat(ListaServicos.Select(s => s.Nome))
            .Concat(ListaServicos.Select(s => s.Categoria?.Nome))
            .Where(t => !string.IsNullOrWhiteSpace(t)));
}
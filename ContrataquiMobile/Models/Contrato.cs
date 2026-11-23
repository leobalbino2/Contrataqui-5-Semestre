using System.Globalization;
using System.Text.Json.Serialization;
using ContrataquiMobile.Services;

namespace ContrataquiMobile.Models;

// Formatação em português (moeda, datas locais)
public static class Formato
{
    public static readonly CultureInfo PtBr = new("pt-BR");

    public static string Moeda(decimal valor) => valor.ToString("C", PtBr);

    // A API manda datas em UTC (ISO); aqui viram horário do aparelho
    public static DateTime ParaLocal(DateTime data) =>
        data.Kind == DateTimeKind.Local
            ? data
            : DateTime.SpecifyKind(data, data.Kind == DateTimeKind.Unspecified ? DateTimeKind.Utc : data.Kind).ToLocalTime();

    public static string Data(DateTime data) => ParaLocal(data).ToString("dd/MM/yyyy", PtBr);

    public static string DataHora(DateTime data) => ParaLocal(data).ToString("dd/MM/yyyy 'às' HH:mm", PtBr);
}

public class ContratoParte
{
    public int Id { get; set; }

    public string Nome { get; set; } = "";

    [JsonPropertyName("imagem_perfil")]
    public string? ImagemPerfil { get; set; }

    public string? Telefone { get; set; }

    public string? Cidade { get; set; }

    public string? Estado { get; set; }

    [JsonIgnore]
    public string Iniciais =>
        string.Concat(Nome.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(p => char.ToUpper(p[0])));

    [JsonIgnore]
    public bool TemFoto => !string.IsNullOrEmpty(ImagemPerfil);

    [JsonIgnore]
    public string Local =>
        string.Join(", ", new[] { Cidade, Estado }.Where(t => !string.IsNullOrWhiteSpace(t)));

    [JsonIgnore]
    public bool TemTelefone => !string.IsNullOrWhiteSpace(Telefone);
}

public class ContratoVisita
{
    public int Id { get; set; }

    [JsonPropertyName("data_visita")]
    public DateTime DataVisita { get; set; }

    public string? Motivo { get; set; }

    public bool Realizada { get; set; }

    // Preenchido pelo contrato (quem vê é o profissional e a visita ainda não foi feita)
    [JsonIgnore]
    public bool PodeMarcar { get; set; }

    [JsonIgnore]
    public string DataTexto => Formato.DataHora(DataVisita);

    [JsonIgnore]
    public bool TemMotivo => !string.IsNullOrWhiteSpace(Motivo);

    [JsonIgnore]
    public string StatusTexto => Realizada ? "Realizada" : "Agendada";

    [JsonIgnore]
    public Color StatusCor => Realizada ? Color.FromArgb("#0D9488") : Color.FromArgb("#2563EB");
}

public class ContratoImprevisto
{
    public int Id { get; set; }

    public string Descricao { get; set; } = "";

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal Valor { get; set; }

    // pendente_aprovacao, aprovado, recusado
    public string Status { get; set; } = "";

    [JsonPropertyName("created_at")]
    public DateTime? CriadoEm { get; set; }

    [JsonIgnore]
    public bool PodeResponder { get; set; }

    [JsonIgnore]
    public bool Pendente => Status == "pendente_aprovacao";

    [JsonIgnore]
    public string ValorTexto => Formato.Moeda(Valor);

    [JsonIgnore]
    public string StatusTexto => Status switch
    {
        "aprovado" => "Aprovado",
        "recusado" => "Recusado",
        _ => "Aguardando aprovação"
    };

    [JsonIgnore]
    public Color StatusCor => Status switch
    {
        "aprovado" => Color.FromArgb("#0D9488"),
        "recusado" => Color.FromArgb("#EF4444"),
        _ => Color.FromArgb("#F97316")
    };
}

public class Contrato
{
    // Etapas do andamento (como no site)
    private static readonly string[] Etapas =
        { "pendente", "aceito", "em_negociacao", "proposta_enviada", "proposta_aceita", "em_andamento", "concluido" };

    public int Id { get; set; }

    public string Titulo { get; set; } = "";

    public string? Descricao { get; set; }

    public string Status { get; set; } = "pendente";

    [JsonPropertyName("cliente_id")]
    public int ClienteId { get; set; }

    [JsonPropertyName("profissional_id")]
    public int ProfissionalId { get; set; }

    [JsonPropertyName("valor_base")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? ValorBase { get; set; }

    [JsonPropertyName("valor_teto_imprevisto")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? ValorTetoImprevisto { get; set; }

    [JsonPropertyName("valor_imprevistos")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? ValorImprevistos { get; set; }

    [JsonPropertyName("valor_final")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? ValorFinal { get; set; }

    [JsonPropertyName("proposta_descricao")]
    public string? PropostaDescricao { get; set; }

    [JsonPropertyName("data_solicitacao")]
    public DateTime? DataSolicitacao { get; set; }

    [JsonPropertyName("data_inicio")]
    public DateTime? DataInicio { get; set; }

    [JsonPropertyName("data_conclusao")]
    public DateTime? DataConclusao { get; set; }

    public Servico? Servico { get; set; }

    public ContratoParte? Cliente { get; set; }

    public ContratoParte? Profissional { get; set; }

    public List<ContratoVisita> Visitas { get; set; } = new();

    public List<ContratoImprevisto> Imprevistos { get; set; } = new();

    // Avaliação que o cliente já deixou (carregada à parte, só quando concluído)
    [JsonIgnore]
    public Avaliacao? MinhaAvaliacao { get; set; }

    // ---------- Quem está olhando ----------

    [JsonIgnore]
    public bool EuSouCliente => Sessao.Atual?.Id == ClienteId;

    [JsonIgnore]
    public bool EuSouProfissional => Sessao.Atual?.Id == ProfissionalId;

    [JsonIgnore]
    public ContratoParte? Outro => EuSouCliente ? Profissional : Cliente;

    [JsonIgnore]
    public string OutroNome => Outro?.Nome is { Length: > 0 } n ? n : "Usuário";

    [JsonIgnore]
    public string OutroPapel => EuSouCliente ? "Profissional" : "Cliente";

    [JsonIgnore]
    public string LinhaOutro => $"{OutroPapel}: {OutroNome}";

    [JsonIgnore]
    public bool OutroTemFoto => Outro?.TemFoto ?? false;

    [JsonIgnore]
    public string? OutroFoto => Outro?.ImagemPerfil;

    [JsonIgnore]
    public string OutroIniciais => Outro?.Iniciais ?? "?";

    [JsonIgnore]
    public string OutroLocal => Outro?.Local ?? "";

    [JsonIgnore]
    public bool OutroTemLocal => OutroLocal.Length > 0;

    // O telefone só aparece depois que o contrato foi aceito (igual ao site)
    [JsonIgnore]
    public bool MostrarTelefone => ChatDisponivel && (Outro?.TemTelefone ?? false);

    [JsonIgnore]
    public string OutroTelefone => Outro?.Telefone ?? "";

    // ---------- Serviço ----------

    [JsonIgnore]
    public string ServicoNome => Servico?.Nome ?? "";

    [JsonIgnore]
    public bool TemServico => !string.IsNullOrEmpty(ServicoNome);

    [JsonIgnore]
    public string? ServicoImagem => Servico?.Imagem;

    [JsonIgnore]
    public bool TemImagemServico => !string.IsNullOrEmpty(Servico?.Imagem);

    [JsonIgnore]
    public string ServicoInicial => TemServico ? char.ToUpper(ServicoNome[0]).ToString() : "S";

    [JsonIgnore]
    public bool TemDescricao => !string.IsNullOrWhiteSpace(Descricao);

    // ---------- Status ----------

    [JsonIgnore]
    public string StatusTexto => Status switch
    {
        "pendente" => "Pendente",
        "aceito" => "Aceito",
        "em_negociacao" => "Em negociação",
        "proposta_enviada" => "Proposta enviada",
        "proposta_aceita" => "Proposta aceita",
        "em_andamento" => "Em andamento",
        "concluido" => "Concluído",
        "cancelado" => "Cancelado",
        "recusado" => "Recusado",
        _ => Status
    };

    [JsonIgnore]
    public Color StatusCor => Color.FromArgb(Status switch
    {
        "pendente" => "#CA8A04",
        "aceito" => "#2563EB",
        "em_negociacao" => "#9333EA",
        "proposta_enviada" => "#F97316",
        "proposta_aceita" or "em_andamento" or "concluido" => "#0D9488",
        _ => "#EF4444"
    });

    [JsonIgnore]
    public Color StatusFundo => Color.FromArgb(Status switch
    {
        "pendente" => "#FEFCE8",
        "aceito" => "#EFF6FF",
        "em_negociacao" => "#FAF5FF",
        "proposta_enviada" => "#FFF7ED",
        "proposta_aceita" or "em_andamento" or "concluido" => "#F0FDFA",
        _ => "#FEF2F2"
    });

    [JsonIgnore]
    public bool EstaEncerrado => Status is "concluido" or "cancelado" or "recusado";

    [JsonIgnore]
    public bool EstaAtivo => !EstaEncerrado;

    // O chat só existe depois que o profissional aceitou e antes de encerrar
    [JsonIgnore]
    public bool ChatDisponivel =>
        Status is "aceito" or "em_negociacao" or "proposta_enviada" or "proposta_aceita" or "em_andamento";

    // ---------- Andamento ----------

    [JsonIgnore]
    public bool MostrarProgresso => Array.IndexOf(Etapas, Status) >= 0;

    [JsonIgnore]
    public double Progresso
    {
        get
        {
            int i = Array.IndexOf(Etapas, Status);
            return i < 0 ? 0 : (i + 1) / (double)Etapas.Length;
        }
    }

    [JsonIgnore]
    public string ProgressoTexto
    {
        get
        {
            int i = Array.IndexOf(Etapas, Status);
            return i < 0 ? "" : $"Etapa {i + 1} de {Etapas.Length}";
        }
    }

    // ---------- Valores ----------

    [JsonIgnore]
    public bool TemProposta => ValorBase is > 0;

    [JsonIgnore]
    public string ValorBaseTexto => Formato.Moeda(ValorBase ?? 0);

    [JsonIgnore]
    public string TetoTexto => Formato.Moeda(ValorTetoImprevisto ?? 0);

    [JsonIgnore]
    public decimal MargemRestante =>
        Math.Max(0, (ValorTetoImprevisto ?? 0) - (ValorImprevistos ?? 0));

    [JsonIgnore]
    public string MargemRestanteTexto => Formato.Moeda(MargemRestante);

    [JsonIgnore]
    public bool TemImprevistosAprovados => (ValorImprevistos ?? 0) > 0;

    [JsonIgnore]
    public string ImprevistosTexto => Formato.Moeda(ValorImprevistos ?? 0);

    [JsonIgnore]
    public decimal ValorExibido => ValorFinal ?? ValorBase ?? 0;

    [JsonIgnore]
    public bool TemValor => ValorExibido > 0;

    [JsonIgnore]
    public string ValorTexto => Formato.Moeda(ValorExibido);

    [JsonIgnore]
    public bool TemPropostaDescricao => !string.IsNullOrWhiteSpace(PropostaDescricao);

    // O card da proposta aparece enquanto ela existe e o contrato não foi encerrado sem ela
    [JsonIgnore]
    public bool MostrarProposta => TemProposta && Status is not ("pendente" or "recusado");

    // ---------- Visitas e imprevistos ----------

    [JsonIgnore]
    public bool TemVisitas => Visitas.Count > 0;

    [JsonIgnore]
    public bool TemImprevistos => Imprevistos.Count > 0;

    [JsonIgnore]
    public List<ContratoImprevisto> ImprevistosPendentes =>
        Imprevistos.Where(i => i.Pendente).ToList();

    [JsonIgnore]
    public bool TemImprevistosPendentes => ImprevistosPendentes.Count > 0;

    // Aviso no topo da tela do cliente quando há imprevisto esperando resposta
    [JsonIgnore]
    public bool MostrarAvisoImprevisto => EuSouCliente && Status == "em_andamento" && TemImprevistosPendentes;

    [JsonIgnore]
    public string AvisoImprevistoTexto => ImprevistosPendentes.Count == 1
        ? "Há 1 imprevisto aguardando sua aprovação."
        : $"Há {ImprevistosPendentes.Count} imprevistos aguardando sua aprovação.";

    // Chamar depois de carregar: libera os botões de cada item conforme quem está vendo
    public void PrepararItens()
    {
        foreach (var v in Visitas)
            v.PodeMarcar = EuSouProfissional && !v.Realizada && Status is "aceito" or "em_negociacao" or "proposta_enviada";

        foreach (var i in Imprevistos)
            i.PodeResponder = EuSouCliente && i.Pendente && Status == "em_andamento";
    }

    // ---------- Botões por papel e status (mesmas regras do backend) ----------

    [JsonIgnore]
    public bool BtnAceitar => EuSouProfissional && Status == "pendente";

    // Visita e proposta valem em aceito e em_negociacao
    [JsonIgnore]
    public bool BtnVisitaProposta => EuSouProfissional && Status is "aceito" or "em_negociacao";

    [JsonIgnore]
    public string BtnPropostaTexto => TemProposta ? "Enviar nova proposta" : "Enviar proposta";

    [JsonIgnore]
    public bool BtnIniciar => EuSouProfissional && Status == "proposta_aceita";

    [JsonIgnore]
    public bool BtnImprevistoConcluir => EuSouProfissional && Status == "em_andamento";

    [JsonIgnore]
    public bool BtnResponderProposta => EuSouCliente && Status == "proposta_enviada";

    [JsonIgnore]
    public bool BtnCancelar =>
        EuSouCliente
            ? Status is "pendente" or "aceito" or "em_negociacao"
            : EuSouProfissional && Status is "aceito" or "em_negociacao" or "proposta_enviada" or "proposta_aceita" or "em_andamento";

    [JsonIgnore]
    public string BtnCancelarTexto => Status == "pendente" ? "Cancelar solicitação" : "Cancelar contrato";

    [JsonIgnore]
    public bool MostrarAcoes =>
        BtnAceitar || BtnVisitaProposta || BtnIniciar || BtnImprevistoConcluir || BtnResponderProposta || BtnCancelar;

    // Mensagem pra quem está esperando o outro lado
    [JsonIgnore]
    public string MensagemFinal => Status switch
    {
        "pendente" when EuSouCliente => "Aguardando o profissional aceitar sua solicitação.",
        "proposta_enviada" when EuSouProfissional => "Aguardando o cliente responder sua proposta.",
        "proposta_aceita" when EuSouCliente => "Proposta aceita. Aguardando o profissional iniciar o serviço.",
        "em_andamento" when EuSouCliente => "Serviço em andamento. Você aprova os imprevistos que passarem da margem.",
        "recusado" => "Esta solicitação foi recusada.",
        "cancelado" => "Este contrato foi cancelado.",
        "concluido" => "Serviço concluído.",
        _ => ""
    };

    [JsonIgnore]
    public bool TemMensagemFinal => MensagemFinal.Length > 0;

    // ---------- Avaliação ----------

    [JsonIgnore]
    public bool PodeAvaliar => EuSouCliente && Status == "concluido" && MinhaAvaliacao is null;

    [JsonIgnore]
    public bool MostrarMinhaAvaliacao => EuSouCliente && Status == "concluido" && MinhaAvaliacao is not null;

    // ---------- Datas ----------

    [JsonIgnore]
    public string SolicitadoTexto => DataSolicitacao is { } d ? Formato.Data(d) : "";

    [JsonIgnore]
    public bool TemDataInicio => DataInicio is not null;

    [JsonIgnore]
    public string InicioTexto => DataInicio is { } d ? Formato.Data(d) : "";

    [JsonIgnore]
    public bool TemDataConclusao => DataConclusao is not null;

    [JsonIgnore]
    public string ConclusaoTexto => DataConclusao is { } d ? Formato.Data(d) : "";
}

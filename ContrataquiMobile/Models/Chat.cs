using System.ComponentModel;
using System.Text.Json.Serialization;

namespace ContrataquiMobile.Models;

public class Mensagem : INotifyPropertyChanged
{
    private bool lida;

    public event PropertyChangedEventHandler? PropertyChanged;

    public int Id { get; set; }

    [JsonPropertyName("chat_id")]
    public int ChatId { get; set; }

    [JsonPropertyName("remetente_id")]
    public int RemetenteId { get; set; }

    // A API chama de "mensagem"
    [JsonPropertyName("mensagem")]
    public string? Texto { get; set; }

    public bool Lida
    {
        get => lida;
        set
        {
            if (lida == value) return;
            lida = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Lida)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TickTexto)));
        }
    }

    public ContratoParte? Remetente { get; set; }

    // ---------- Só pra exibição ----------

    [JsonIgnore]
    public bool Minha => Sessao.Atual?.Id == RemetenteId;

    [JsonIgnore]
    public LayoutOptions Alinhamento => Minha ? LayoutOptions.End : LayoutOptions.Start;

    [JsonIgnore]
    public Color CorBalao => Minha ? Color.FromArgb("#1A365D") : Colors.White;

    [JsonIgnore]
    public Color CorTexto => Minha ? Colors.White : Color.FromArgb("#111C2C");

    [JsonIgnore]
    public Color CorBorda => Minha ? Color.FromArgb("#1A365D") : Color.FromArgb("#E1E8F5");

    // O "visto" só aparece na última mensagem minha do grupo (a tela define)
    [JsonIgnore]
    public bool MostrarTick { get; set; }

    [JsonIgnore]
    public string TickTexto => Lida ? "Lida" : "Enviada";

    public void AtualizarTick(bool mostrar)
    {
        if (MostrarTick == mostrar) return;
        MostrarTick = mostrar;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MostrarTick)));
    }
}

public class Chat
{
    public int Id { get; set; }

    [JsonPropertyName("contrato_id")]
    public int ContratoId { get; set; }

    public Contrato? Contrato { get; set; }

    // A API manda só a última mensagem aqui
    public List<Mensagem> Mensagens { get; set; } = new();

    [JsonPropertyName("nao_lidas")]
    public int NaoLidas { get; set; }

    // ---------- Só pra exibição ----------

    [JsonIgnore]
    public ContratoParte? Outro => Contrato?.Outro;

    [JsonIgnore]
    public string OutroNome => Contrato?.OutroNome ?? "Usuário";

    [JsonIgnore]
    public string? OutroFoto => Contrato?.OutroFoto;

    [JsonIgnore]
    public bool OutroTemFoto => Contrato?.OutroTemFoto ?? false;

    [JsonIgnore]
    public string OutroIniciais => Contrato?.OutroIniciais ?? "?";

    [JsonIgnore]
    public string Titulo => Contrato?.Titulo ?? "";

    [JsonIgnore]
    public Mensagem? Ultima => Mensagens.LastOrDefault();

    [JsonIgnore]
    public int UltimoId => Ultima?.Id ?? 0;

    [JsonIgnore]
    public string UltimaTexto
    {
        get
        {
            if (Ultima is null) return "Nenhuma mensagem ainda. Toque para conversar.";
            string prefixo = Ultima.Minha ? "Você: " : "";
            return prefixo + (Ultima.Texto ?? "");
        }
    }

    [JsonIgnore]
    public bool TemNaoLidas => NaoLidas > 0;

    [JsonIgnore]
    public string NaoLidasTexto => NaoLidas > 99 ? "99+" : NaoLidas.ToString();

    // Uma linha por contrato com chat liberado; conversas com mais atividade primeiro
    public static List<Chat> Normalizar(IEnumerable<Chat> chats)
    {
        return chats
            .Where(c => c.Contrato is { ChatDisponivel: true })
            .GroupBy(c => c.ContratoId)
            .Select(g => g
                .OrderByDescending(c => c.UltimoId)
                .ThenBy(c => c.Id)
                .First())
            .OrderByDescending(c => c.UltimoId)
            .ThenByDescending(c => c.Id)
            .ToList();
    }
}

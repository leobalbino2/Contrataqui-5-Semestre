using System.ComponentModel;
using System.Text.Json.Serialization;

namespace ContrataquiMobile.Models;

public class Notificacao : INotifyPropertyChanged
{
    private bool lida;

    public event PropertyChangedEventHandler? PropertyChanged;

    public int Id { get; set; }

    public string Titulo { get; set; } = "";

    public string? Mensagem { get; set; }

    // nova_mensagem, novo_contrato, contrato_aceito, contrato_recusado,
    // avaliacao_recebida, imprevisto_lancado, proposta_recebida
    public string Tipo { get; set; } = "";

    // Id do contrato ou do chat relacionado
    [JsonPropertyName("referencia_id")]
    public int? ReferenciaId { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CriadaEm { get; set; }

    public bool Lida
    {
        get => lida;
        set
        {
            if (lida == value) return;
            lida = value;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Lida)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NaoLida)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CorFundo)));
        }
    }

    // ---------- Só pra exibição ----------

    [JsonIgnore]
    public bool NaoLida => !Lida;

    [JsonIgnore]
    public bool TemMensagem => !string.IsNullOrWhiteSpace(Mensagem);

    [JsonIgnore]
    public Color CorFundo => Lida ? Colors.White : Color.FromArgb("#EAF5F5");

    [JsonIgnore]
    public string Icone => Tipo switch
    {
        "nova_mensagem" => "\uE0B7",
        "novo_contrato" => "\uE85D",
        "contrato_aceito" => "\uE86C",
        "contrato_recusado" => "\uE5C9",
        "avaliacao_recebida" => "\uE838",
        "imprevisto_lancado" => "\uE002",
        "proposta_recebida" => "\uE873",
        _ => "\uE7F4"
    };

    [JsonIgnore]
    public Color CorIcone => Tipo switch
    {
        "contrato_recusado" => Color.FromArgb("#BA1A1A"),
        "imprevisto_lancado" => Color.FromArgb("#D4903B"),
        "avaliacao_recebida" => Color.FromArgb("#D4903B"),
        _ => Color.FromArgb("#13696A")
    };

    [JsonIgnore]
    public string Tempo
    {
        get
        {
            if (CriadaEm is null) return "";

            DateTime data = CriadaEm.Value;
            DateTime utc = data.Kind == DateTimeKind.Local
                ? data.ToUniversalTime()
                : DateTime.SpecifyKind(data, DateTimeKind.Utc);

            TimeSpan faz = DateTime.UtcNow - utc;

            if (faz.TotalMinutes < 1) return "agora";
            if (faz.TotalMinutes < 60) return $"há {(int)faz.TotalMinutes} min";
            if (faz.TotalHours < 24) return $"há {(int)faz.TotalHours} h";
            if (faz.TotalDays < 2) return "ontem";
            if (faz.TotalDays < 7) return $"há {(int)faz.TotalDays} dias";

            return utc.ToLocalTime().ToString("dd/MM/yyyy");
        }
    }
}
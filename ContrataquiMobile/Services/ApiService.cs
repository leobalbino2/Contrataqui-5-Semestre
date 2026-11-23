using System.Net.Http.Headers;
using System.Net.Http.Json;
using ContrataquiMobile.Models;

namespace ContrataquiMobile.Services;

public static class ApiService
{
    // Sem cookies: o app autentica só pelo token (Bearer)
    static readonly HttpClient http = new(new HttpClientHandler { UseCookies = false })
    {
        BaseAddress = new Uri(ApiConfig.BaseUrl + "/"),
        Timeout = TimeSpan.FromSeconds(30)
    };

    static ApiService()
    {
        AplicarToken(Token);
    }

    public static string Token
    {
        get => Preferences.Get("token", "");
        private set => Preferences.Set("token", value);
    }

    static void AplicarToken(string token)
    {
        http.DefaultRequestHeaders.Authorization =
            string.IsNullOrEmpty(token) ? null : new AuthenticationHeaderValue("Bearer", token);
    }

    public static void Sair()
    {
        Preferences.Remove("token");
        AplicarToken("");
    }

    // ---------- Helpers ----------

    static async Task<HttpResponseMessage> EnviarAsync(Func<Task<HttpResponseMessage>> envio)
    {
        try
        {
            return await envio();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new Exception("Não foi possível conectar ao servidor.");
        }
    }

    static async Task GarantirSucessoAsync(HttpResponseMessage resp, string mensagemPadrao)
    {
        if (resp.IsSuccessStatusCode) return;

        ErroResposta? erro = null;
        try { erro = await resp.Content.ReadFromJsonAsync<ErroResposta>(); }
        catch { /* corpo sem JSON */ }

        throw new Exception(erro?.Erro ?? mensagemPadrao);
    }

    // ---------- Usuários ----------

    public static async Task<Usuario> LoginAsync(string email, string senha)
    {
        var resp = await EnviarAsync(() => http.PostAsJsonAsync("usuarios/login", new { email, senha }));
        await GarantirSucessoAsync(resp, "Erro ao fazer login.");

        var dados = await resp.Content.ReadFromJsonAsync<LoginResposta>()
                    ?? throw new Exception("Resposta inválida do servidor.");

        // Admin usa só o site: barra antes de guardar o token
        if (dados.Usuario.Tipo == "admin")
            throw new Exception("Contas de administrador acessam pelo site.");

        Token = dados.Token;
        AplicarToken(dados.Token);

        return new Usuario
        {
            Id = dados.Usuario.Id,
            Nome = dados.Usuario.Nome,
            Email = dados.Usuario.Email,
            Telefone = dados.Usuario.Telefone,
            Tipo = dados.Usuario.Tipo == "profissional" ? TipoUsuario.Profissional : TipoUsuario.Cliente
        };
    }

    // Os nomes das chaves seguem o que o backend espera (nome_fantasia, servico_ids...)
    public static async Task CadastrarAsync(Dictionary<string, object?> dados)
    {
        var resp = await EnviarAsync(() => http.PostAsJsonAsync("usuarios/cadastro", dados));
        await GarantirSucessoAsync(resp, "Erro ao cadastrar.");
    }

    // ---------- Minha conta ----------

    public static async Task<PerfilUsuario> ObterMeuPerfilAsync()
    {
        var resp = await EnviarAsync(() => http.GetAsync("usuarios/perfil"));
        await GarantirSucessoAsync(resp, "Erro ao carregar o perfil.");

        return await resp.Content.ReadFromJsonAsync<PerfilUsuario>()
               ?? throw new Exception("Resposta inválida do servidor.");
    }

    public static async Task AtualizarPerfilAsync(Dictionary<string, object?> dados)
    {
        var resp = await EnviarAsync(() => http.PutAsJsonAsync("usuarios/perfil", dados));
        await GarantirSucessoAsync(resp, "Erro ao salvar o perfil.");
    }

    public static async Task AlterarSenhaAsync(string senhaAtual, string novaSenha)
    {
        var resp = await EnviarAsync(() => http.PutAsJsonAsync("usuarios/senha",
            new { senha_atual = senhaAtual, nova_senha = novaSenha }));
        await GarantirSucessoAsync(resp, "Erro ao alterar a senha.");
    }

    public static async Task ExcluirContaAsync(string senha)
    {
        var resp = await EnviarAsync(() => http.SendAsync(
            new HttpRequestMessage(HttpMethod.Delete, "usuarios/conta")
            {
                Content = JsonContent.Create(new { senha })
            }));
        await GarantirSucessoAsync(resp, "Erro ao excluir a conta.");
    }

    // ---------- Profissionais ----------

    // Já vem ordenado pela melhor avaliação (o backend faz isso)
    public static async Task<List<Profissional>> ListarProfissionaisAsync()
    {
        var resp = await EnviarAsync(() => http.GetAsync("usuarios/profissionais"));
        await GarantirSucessoAsync(resp, "Erro ao carregar profissionais.");
        return await resp.Content.ReadFromJsonAsync<List<Profissional>>() ?? new();
    }

    public static async Task<Profissional> ObterProfissionalAsync(int id)
    {
        var resp = await EnviarAsync(() => http.GetAsync($"usuarios/{id}"));
        await GarantirSucessoAsync(resp, "Profissional não encontrado.");

        return await resp.Content.ReadFromJsonAsync<Profissional>()
               ?? throw new Exception("Resposta inválida do servidor.");
    }

    // ---------- Avaliações ----------

    public static async Task<List<Avaliacao>> ListarAvaliacoesProfissionalAsync(int profissionalId)
    {
        var resp = await EnviarAsync(() => http.GetAsync($"avaliacoes/profissional/{profissionalId}"));
        await GarantirSucessoAsync(resp, "Erro ao carregar avaliações.");
        return await resp.Content.ReadFromJsonAsync<List<Avaliacao>>() ?? new();
    }

    // ---------- Catálogo ----------

    public static async Task<List<Categoria>> ListarCategoriasAsync()
    {
        var resp = await EnviarAsync(() => http.GetAsync("categorias"));
        await GarantirSucessoAsync(resp, "Erro ao carregar categorias.");
        return await resp.Content.ReadFromJsonAsync<List<Categoria>>() ?? new();
    }

    public static async Task<List<Servico>> ListarServicosAsync()
    {
        var resp = await EnviarAsync(() => http.GetAsync("servicos"));
        await GarantirSucessoAsync(resp, "Erro ao carregar serviços.");
        return await resp.Content.ReadFromJsonAsync<List<Servico>>() ?? new();
    }

    public static async Task<List<Servico>> ListarServicosPorCategoriaAsync(int categoriaId)
    {
        var todos = await ListarServicosAsync();
        return todos.Where(s => s.CategoriaIdEfetivo == categoriaId).ToList();
    }

    // ---------- Notificações ----------

    // Mais recentes primeiro (o backend já ordena)
    public static async Task<List<Notificacao>> ListarNotificacoesAsync()
    {
        var resp = await EnviarAsync(() => http.GetAsync("notificacoes"));
        await GarantirSucessoAsync(resp, "Erro ao carregar notificações.");
        return await resp.Content.ReadFromJsonAsync<List<Notificacao>>() ?? new();
    }

    public static async Task MarcarNotificacaoLidaAsync(int id)
    {
        var resp = await EnviarAsync(() =>
            http.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"notificacoes/{id}/lida")));
        await GarantirSucessoAsync(resp, "Erro ao marcar notificação.");
    }

    public static async Task MarcarTodasNotificacoesLidasAsync()
    {
        var resp = await EnviarAsync(() =>
            http.SendAsync(new HttpRequestMessage(HttpMethod.Patch, "notificacoes/todas/lidas")));
        await GarantirSucessoAsync(resp, "Erro ao marcar notificações.");
    }

    // ---------- Contratos ----------

    static Task<HttpResponseMessage> PatchJsonAsync(string url, object? corpo = null) =>
        EnviarAsync(() => http.SendAsync(new HttpRequestMessage(HttpMethod.Patch, url)
        {
            Content = JsonContent.Create(corpo ?? new { })
        }));

    // Contratos em que sou cliente ou profissional (mais recentes primeiro)
    public static async Task<List<Contrato>> ListarContratosAsync()
    {
        var resp = await EnviarAsync(() => http.GetAsync("contratos"));
        await GarantirSucessoAsync(resp, "Erro ao carregar os agendamentos.");
        var lista = await resp.Content.ReadFromJsonAsync<List<Contrato>>() ?? new();
        lista.ForEach(c => c.PrepararItens());
        return lista;
    }

    public static async Task<Contrato> ObterContratoAsync(int id)
    {
        var resp = await EnviarAsync(() => http.GetAsync($"contratos/{id}"));
        await GarantirSucessoAsync(resp, "Contrato não encontrado.");
        var contrato = await resp.Content.ReadFromJsonAsync<Contrato>()
                       ?? throw new Exception("Resposta inválida do servidor.");
        contrato.PrepararItens();
        return contrato;
    }

    public static async Task CriarContratoAsync(int profissionalId, int servicoId, string titulo, string descricao)
    {
        var resp = await EnviarAsync(() => http.PostAsJsonAsync("contratos", new
        {
            profissional_id = profissionalId,
            servico_id = servicoId,
            titulo,
            descricao
        }));
        await GarantirSucessoAsync(resp, "Erro ao enviar a solicitação.");
    }

    public static async Task AtualizarStatusContratoAsync(int id, string status)
    {
        var resp = await PatchJsonAsync($"contratos/{id}/status", new { status });
        await GarantirSucessoAsync(resp, "Erro ao atualizar o contrato.");
    }

    // A data vai em UTC (o servidor guarda assim)
    public static async Task AgendarVisitaAsync(int contratoId, DateTime dataLocal, string? motivo)
    {
        string dataUtc = dataLocal.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'");
        var resp = await EnviarAsync(() => http.PostAsJsonAsync($"contratos/{contratoId}/visitas",
            new { data_visita = dataUtc, motivo = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim() }));
        await GarantirSucessoAsync(resp, "Erro ao agendar a visita.");
    }

    public static async Task MarcarVisitaRealizadaAsync(int contratoId, int visitaId)
    {
        var resp = await PatchJsonAsync($"contratos/{contratoId}/visitas/{visitaId}/realizada");
        await GarantirSucessoAsync(resp, "Erro ao marcar a visita.");
    }

    public static async Task EnviarPropostaAsync(int contratoId, decimal valorBase, decimal teto, string? descricao)
    {
        var resp = await PatchJsonAsync($"contratos/{contratoId}/proposta", new
        {
            valor_base = valorBase,
            valor_teto_imprevisto = teto,
            proposta_descricao = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim()
        });
        await GarantirSucessoAsync(resp, "Erro ao enviar a proposta.");
    }

    // Devolve true quando o valor coube na margem e já foi aprovado sozinho
    public static async Task<bool> LancarImprevistoAsync(int contratoId, string descricao, decimal valor)
    {
        var resp = await EnviarAsync(() => http.PostAsJsonAsync($"contratos/{contratoId}/imprevistos",
            new { descricao = descricao.Trim(), valor }));
        await GarantirSucessoAsync(resp, "Erro ao lançar o imprevisto.");

        var dados = await resp.Content.ReadFromJsonAsync<ImprevistoResposta>();
        return dados?.AutoAprovado ?? false;
    }

    public static async Task ResponderImprevistoAsync(int contratoId, int imprevistoId, bool aceito)
    {
        var resp = await PatchJsonAsync($"contratos/{contratoId}/imprevistos/{imprevistoId}", new { aceito });
        await GarantirSucessoAsync(resp, "Erro ao responder o imprevisto.");
    }

    // ---------- Avaliações do cliente ----------

    public static async Task<List<Avaliacao>> ListarMinhasAvaliacoesAsync()
    {
        var resp = await EnviarAsync(() => http.GetAsync("avaliacoes/cliente"));
        await GarantirSucessoAsync(resp, "Erro ao carregar suas avaliações.");
        return await resp.Content.ReadFromJsonAsync<List<Avaliacao>>() ?? new();
    }

    public static async Task CriarAvaliacaoAsync(int contratoId, int profissionalId, int nota, string? comentario)
    {
        var resp = await EnviarAsync(() => http.PostAsJsonAsync("avaliacoes", new
        {
            contrato_id = contratoId,
            profissional_id = profissionalId,
            nota,
            comentario = string.IsNullOrWhiteSpace(comentario) ? null : comentario.Trim()
        }));
        await GarantirSucessoAsync(resp, "Erro ao enviar a avaliação.");
    }

    // ---------- Chat ----------

    public static async Task<Chat> BuscarOuCriarChatAsync(int contratoId)
    {
        var resp = await EnviarAsync(() => http.PostAsJsonAsync("chats", new { contrato_id = contratoId }));
        await GarantirSucessoAsync(resp, "Erro ao abrir a conversa.");
        return await resp.Content.ReadFromJsonAsync<Chat>()
               ?? throw new Exception("Resposta inválida do servidor.");
    }

    public static async Task<List<Chat>> ListarChatsAsync()
    {
        var resp = await EnviarAsync(() => http.GetAsync("chats"));
        await GarantirSucessoAsync(resp, "Erro ao carregar as conversas.");
        var lista = await resp.Content.ReadFromJsonAsync<List<Chat>>() ?? new();
        lista.ForEach(c => c.Contrato?.PrepararItens());
        return lista;
    }

    public static async Task<List<Mensagem>> ListarMensagensAsync(int chatId)
    {
        var resp = await EnviarAsync(() => http.GetAsync($"mensagens/{chatId}"));
        await GarantirSucessoAsync(resp, "Erro ao carregar as mensagens.");
        return await resp.Content.ReadFromJsonAsync<List<Mensagem>>() ?? new();
    }

    public static async Task EnviarMensagemAsync(int chatId, string texto)
    {
        var resp = await EnviarAsync(() => http.PostAsJsonAsync("mensagens", new { chat_id = chatId, mensagem = texto }));
        await GarantirSucessoAsync(resp, "Erro ao enviar a mensagem.");
    }

    public static async Task MarcarMensagensLidasAsync(int chatId)
    {
        var resp = await PatchJsonAsync($"mensagens/{chatId}/lidas");
        await GarantirSucessoAsync(resp, "Erro ao marcar as mensagens.");
    }

    // ---------- Upload ----------

    // O backend espera a imagem como data URI em base64 e devolve { url }
    public static async Task<string> EnviarImagemAsync(Stream stream, string contentType)
    {
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);

        // Limite do backend: 10mb de JSON (o base64 aumenta o arquivo em ~33%)
        if (ms.Length > 7 * 1024 * 1024)
            throw new Exception("A foto é muito grande. Escolha uma imagem de até 7 MB.");

        string dataUri = $"data:{contentType};base64,{Convert.ToBase64String(ms.ToArray())}";

        var resp = await EnviarAsync(() => http.PostAsJsonAsync("upload", new { imagem = dataUri }));
        await GarantirSucessoAsync(resp, "Erro ao enviar a foto.");

        var dados = await resp.Content.ReadFromJsonAsync<UploadResposta>()
                    ?? throw new Exception("Resposta inválida do servidor.");
        return dados.Url;
    }

    // ---------- Modelos de resposta ----------

    class ErroResposta { public string? Erro { get; set; } }

    class ImprevistoResposta
    {
        [System.Text.Json.Serialization.JsonPropertyName("auto_aprovado")]
        public bool AutoAprovado { get; set; }
    }

    class UploadResposta { public string Url { get; set; } = ""; }

    class LoginResposta
    {
        public string Token { get; set; } = "";
        public UsuarioResposta Usuario { get; set; } = new();
    }

    class UsuarioResposta
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public string Email { get; set; } = "";
        public string Tipo { get; set; } = "";
        public string? Telefone { get; set; }
    }
}
using System.Collections.ObjectModel;
using ContrataquiMobile.Models;
using ContrataquiMobile.Services;

namespace ContrataquiMobile.Views;

public partial class Conversa : ContentPage
{
    private static readonly TimeSpan IntervaloAtualizacao = TimeSpan.FromSeconds(3);

    private readonly int _contratoId;
    private int? _chatId;

    private IDispatcherTimer? _timer;
    private bool _atualizando;
    private bool _enviando;
    private bool _iniciou;

    public ObservableCollection<Mensagem> Itens { get; } = new();

    public Conversa(int contratoId, string nome, string? foto, string tituloContrato, int? chatId = null)
    {
        InitializeComponent();
        BindingContext = this;

        _contratoId = contratoId;
        _chatId = chatId;

        lbl_nome.Text = nome;
        lbl_contrato.Text = tituloContrato;
        lbl_inicial.Text = string.Concat(nome.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                             .Take(2)
                                             .Select(p => char.ToUpper(p[0])));

        if (!string.IsNullOrEmpty(foto))
        {
            img_foto.Source = foto;
            img_foto.IsVisible = true;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_iniciou)
        {
            _iniciou = true;
            await IniciarAsync();
        }
        else
        {
            await AtualizarAsync();
        }

        IniciarTimer();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        PararTimer();
    }

    private async Task IniciarAsync()
    {
        try
        {
            ind_carregando.IsVisible = true;
            ind_carregando.IsRunning = true;
            lbl_aviso.IsVisible = false;

            _chatId ??= (await ApiService.BuscarOuCriarChatAsync(_contratoId)).Id;

            await AtualizarAsync(primeiraVez: true);
        }
        catch (Exception ex)
        {
            _iniciou = false;
            lbl_aviso.Text = $"{ex.Message}\nToque para tentar novamente.";
            lbl_aviso.IsVisible = true;
        }
        finally
        {
            ind_carregando.IsRunning = false;
            ind_carregando.IsVisible = false;
        }
    }

    // ---------- Atualização periódica (sem sockets, só consulta a cada 3 s) ----------

    private void IniciarTimer()
    {
        if (_timer is null)
        {
            _timer = Dispatcher.CreateTimer();
            _timer.Interval = IntervaloAtualizacao;
            _timer.Tick += async (_, _) =>
            {
                // Tela já foi fechada: não deixa o relógio rodando à toa
                if (Window is null)
                {
                    PararTimer();
                    return;
                }

                await AtualizarAsync();
            };
        }

        if (!_timer.IsRunning)
            _timer.Start();
    }

    private void PararTimer()
    {
        _timer?.Stop();
    }

    private async Task AtualizarAsync(bool primeiraVez = false)
    {
        if (_atualizando || _chatId is not int chatId) return;
        _atualizando = true;

        try
        {
            List<Mensagem> recebidas = (await ApiService.ListarMensagensAsync(chatId))
                .OrderBy(m => m.Id)
                .ToList();

            bool mudou = false;
            bool chegouDoOutro = false;

            foreach (Mensagem nova in recebidas)
            {
                Mensagem? existente = Itens.FirstOrDefault(m => m.Id == nova.Id);

                if (existente is null)
                {
                    Itens.Add(nova);
                    mudou = true;
                    if (!nova.Minha && !nova.Lida) chegouDoOutro = true;
                }
                else if (existente.Lida != nova.Lida)
                {
                    existente.Lida = nova.Lida;
                }
            }

            AtualizarTick();

            if (mudou)
                RolarParaOFim();

            // Avisa o servidor que li o que chegou (some o contador da lista)
            if (primeiraVez || chegouDoOutro)
            {
                try { await ApiService.MarcarMensagensLidasAsync(chatId); }
                catch { /* não atrapalha a conversa */ }
            }
        }
        catch (Exception ex)
        {
            // Falha de rede no meio da conversa: só avisa se ainda não há nada na tela
            if (primeiraVez) throw;
            if (Itens.Count == 0)
            {
                lbl_aviso.Text = $"{ex.Message}\nToque para tentar novamente.";
                lbl_aviso.IsVisible = true;
            }
        }
        finally
        {
            _atualizando = false;
        }
    }

    // O "Enviada/Lida" aparece só na última mensagem minha
    private void AtualizarTick()
    {
        Mensagem? ultimaMinha = Itens.LastOrDefault(m => m.Minha);

        foreach (Mensagem m in Itens)
            m.AtualizarTick(ReferenceEquals(m, ultimaMinha));
    }

    // Não pode ser "esperado": no Windows o ScrollToAsync às vezes nunca termina
    // e travaria a atualização (bolinha girando e mensagens que não saem)
    private void RolarParaOFim()
    {
        Dispatcher.Dispatch(async () =>
        {
            try
            {
                await Task.Delay(80);
                _ = scv_mensagens.ScrollToAsync(0, stk_mensagens.Height + 1000, false);
            }
            catch
            {
                // rolagem é só conforto: se falhar, a conversa continua funcionando
            }
        });
    }

    // ---------- Envio ----------

    private async void Enviar_Tapped(object? sender, TappedEventArgs e) => await EnviarAsync();

    private async void Enviar_Completed(object? sender, EventArgs e) => await EnviarAsync();

    private async Task EnviarAsync()
    {
        string texto = (txt_mensagem.Text ?? "").Trim();
        if (texto.Length == 0 || _enviando || _chatId is not int chatId) return;

        _enviando = true;
        txt_mensagem.Text = "";

        try
        {
            await ApiService.EnviarMensagemAsync(chatId, texto);

            // Se uma consulta estiver em andamento, espera ela acabar pra não perder a mensagem
            for (int i = 0; i < 60 && _atualizando; i++) await Task.Delay(50);
            await AtualizarAsync();
        }
        catch (Exception ex)
        {
            txt_mensagem.Text = texto;
            await DisplayAlertAsync("Ops!", ex.Message, "OK");
        }
        finally
        {
            _enviando = false;
            txt_mensagem.Focus();
        }
    }

    private async void Aviso_Tapped(object? sender, TappedEventArgs e)
    {
        lbl_aviso.IsVisible = false;

        if (_chatId is null)
            await IniciarAsync();
        else
            await AtualizarAsync();
    }

    private async void Voltar_Tapped(object? sender, TappedEventArgs e)
    {
        PararTimer();
        await Navigation.PopAsync();
    }
}
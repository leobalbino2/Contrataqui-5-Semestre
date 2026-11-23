using ContrataquiMobile.Models;
using ContrataquiMobile.Views;

namespace ContrataquiMobile.Services;

public record Aba(string Id, string Rotulo, string Icone);

public static class Navegacao
{
    // Quando o dashboard existir: adicione a aba "inicio" no começo da lista do tipo
    // (ex.: new("inicio", "Início", "\uE871")) e o case "inicio" em CriarPagina.
    private static readonly Aba[] AbasCliente =
    {
        new("explorar", "Explorar", "\uE87A"),
        new("agendamentos", "Agendamentos", "\uE878"),
        new("mensagens", "Mensagens", "\uE0B7"),
        new("perfil", "Perfil", "\uE7FD")
    };

    private static readonly Aba[] AbasProfissional =
    {
        new("agendamentos", "Agendamentos", "\uE878"),
        new("mensagens", "Mensagens", "\uE0B7"),
        new("perfil", "Perfil", "\uE7FD")
    };

    public static IReadOnlyList<Aba> AbasDe(TipoUsuario tipo) =>
        tipo == TipoUsuario.Profissional ? AbasProfissional : AbasCliente;

    // A primeira aba de cada tipo é a página inicial
    public static string IdInicial(TipoUsuario tipo) => AbasDe(tipo)[0].Id;

    public static Page? CriarPagina(string id) => id switch
    {
        "explorar" => new Explorar(),
        "agendamentos" => new Agendamentos(),
        "mensagens" => new Mensagens(),
        "perfil" => new Perfil(),
        _ => null
    };

    // Raiz do app já na página inicial do usuário (usar quando se cria a janela)
    public static Page PaginaInicial(Usuario usuario) =>
        new NavigationPage(CriarPagina(IdInicial(usuario.Tipo))!);

    // Troca a página raiz depois de login, cadastro, etc.
    public static void IrParaInicio(Usuario usuario)
    {
        Application.Current!.Windows[0].Page = PaginaInicial(usuario);
    }

    // Vai para uma das abas a partir de qualquer tela (zera a pilha de páginas)
    public static void IrParaAba(Page origem, string id)
    {
        var pagina = CriarPagina(id);
        if (pagina is null) return;

        var janela = origem.Window ?? Application.Current!.Windows[0];
        janela.Page = new NavigationPage(pagina);
    }
}

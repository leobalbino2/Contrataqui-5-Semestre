using System.Globalization;
using System.Text;

namespace ContrataquiMobile.Models;

public static class CategoriaDados
{
    // Reserva usada quando a API não responde
    public static readonly List<Categoria> Todas = new()
    {
        new Categoria { Nome = "Elétrica", Descricao = "Instalações, reparos e manutenção elétrica." },
        new Categoria { Nome = "Hidráulica", Descricao = "Vazamentos, desentupimentos e instalações." },
        new Categoria { Nome = "Reforma", Descricao = "Reformas completas e pequenos reparos." },
        new Categoria { Nome = "Obras", Descricao = "Construção, alvenaria e acabamentos." },
        new Categoria { Nome = "Pintura", Descricao = "Pintura interna, externa e texturas." },
        new Categoria { Nome = "Limpeza", Descricao = "Limpeza residencial e pós-obra." }
    };

    // Trecho do nome da categoria -> arquivo em Resources\Images
    private static readonly (string Trecho, string Arquivo)[] Icones =
    {
        ("eletric", "categoria_eletrica.png"),
        ("hidraul", "categoria_hidraulica.png"),
        ("reform", "categoria_reforma.png"),
        ("obra", "categoria_obras.png"),
        ("constru", "categoria_obras.png"),
        ("pintur", "categoria_pintura.png"),
        ("limpe", "categoria_limpeza.png")
    };

    // Devolve o ícone do app para a categoria, ou null se não tiver um
    public static string? IconeLocal(string nome)
    {
        string normalizado = SemAcento(nome).ToLowerInvariant();

        foreach (var (trecho, arquivo) in Icones)
        {
            if (normalizado.Contains(trecho))
                return arquivo;
        }

        return null;
    }

    private static string SemAcento(string texto)
    {
        string decomposto = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (char c in decomposto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
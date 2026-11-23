namespace ContrataquiMobile.Services;

public static class Mascaras
{
    public static string SomenteDigitos(string? texto) =>
        new((texto ?? "").Where(char.IsDigit).ToArray());

    public static string Telefone(string? texto)
    {
        string d = SomenteDigitos(texto);
        if (d.Length > 11) d = d[..11];

        return d.Length switch
        {
            0 => "",
            <= 2 => $"({d}",
            <= 7 => $"({d[..2]}) {d[2..]}",
            _ => $"({d[..2]}) {d[2..7]}-{d[7..]}"
        };
    }

    public static string Cep(string? texto)
    {
        string d = SomenteDigitos(texto);
        if (d.Length > 8) d = d[..8];

        return d.Length <= 5 ? d : $"{d[..5]}-{d[5..]}";
    }

    // "170000" -> "1.700,00" (digita-se só números; os dois últimos são os centavos)
    public static string Moeda(string? texto)
    {
        string d = SomenteDigitos(texto).TrimStart('0');
        if (d.Length > 11) d = d[..11];
        if (d.Length == 0) return "";

        d = d.PadLeft(3, '0');
        string inteiro = d[..^2];
        string centavos = d[^2..];

        // Separador de milhar
        var partes = new List<string>();
        for (int i = inteiro.Length; i > 0; i -= 3)
            partes.Insert(0, inteiro[Math.Max(0, i - 3)..i]);

        return $"{string.Join(".", partes)},{centavos}";
    }

    public static decimal MoedaParaDecimal(string? texto)
    {
        string d = SomenteDigitos(texto);
        if (d.Length == 0) return 0;
        return decimal.Parse(d, System.Globalization.CultureInfo.InvariantCulture) / 100m;
    }
}

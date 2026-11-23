namespace ContrataquiMobile.Models
{
    public enum TipoUsuario
    {
        Cliente,
        Profissional
    }

    public class Usuario
    {
        string? nome, email, senha;

        public int Id { get; set; }

        public string? Nome
        {
            get => nome;
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new Exception("Faz favor, informe o nome.");

                nome = value;
            }
        }

        public string? Email
        {
            get => email;
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new Exception("Faz favor, informe o email.");

                email = value;
            }
        }

        public string? Senha
        {
            get => senha;
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new Exception("Faz favor, informe a senha.");

                senha = value;
            }
        }

        public string? Telefone { get; set; }

        public TipoUsuario Tipo { get; set; }

        // Vindos da API (lista de profissionais)
        public string? ImagemPerfil { get; set; }
        public bool TemFoto => !string.IsNullOrEmpty(ImagemPerfil);
        public string? Cidade { get; set; }
        public string? Estado { get; set; }

        // Nome, cidade, serviços e categorias juntos, só pra busca (não aparece na tela)
        public string TextoBusca { get; set; } = "";

        // Só usados quando Tipo == Profissional (cards do Explorar e perfil)
        public string Especialidade { get; set; } = "";
        public string Preco { get; set; } = "";
        public string Nota { get; set; } = "";
        public string Servicos { get; set; } = "";
        public string Disponibilidade { get; set; } = "";

        public string Iniciais =>
            string.Concat((Nome ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                      .Take(2)
                                      .Select(p => char.ToUpper(p[0])));
    }
}
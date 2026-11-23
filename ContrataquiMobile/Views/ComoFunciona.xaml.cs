using ContrataquiMobile.Models;

namespace ContrataquiMobile.Views;

public partial class ComoFunciona : ContentPage
{
    public List<Passo> PassosCliente { get; } = new()
    {
        new Passo("1", "Descreva seu serviço", "Diga o que você precisa. Quanto mais detalhes, melhores serão os orçamentos recebidos."),
        new Passo("2", "Receba orçamentos", "Profissionais qualificados enviam propostas personalizadas diretamente para você em poucos minutos."),
        new Passo("3", "Escolha o profissional", "Compare perfis, avaliações reais e portfólios para tomar a decisão mais assertiva."),
        new Passo("4", "Pague com segurança", "O pagamento é feito pela plataforma e o valor só é liberado após a conclusão do serviço.")
    };

    public List<Passo> PassosProfissional { get; } = new()
    {
        new Passo("1", "Crie seu perfil", "Cadastre suas especialidades, fotos de trabalhos realizados e documentos para verificação."),
        new Passo("2", "Receba pedidos", "Avisamos você sempre que houver um cliente precisando dos seus serviços na sua região."),
        new Passo("3", "Realize o trabalho e receba", "Execute o serviço com excelência e receba o pagamento garantido diretamente em sua conta.")
    };

    public List<Passo> Seguranca { get; } = new()
    {
        new Passo("✓", "Profissionais verificados", "Validamos identidade e antecedentes de cada profissional para a sua tranquilidade."),
        new Passo("✓", "Suporte 24/7", "Nossa equipe de atendimento está pronta para ajudar em qualquer etapa do processo."),
        new Passo("✓", "Garantia Contrataqui", "Proteção total para o seu projeto. Se algo não sair como planejado, nós resolvemos.")
    };

    public ComoFunciona()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private async void Voltar_Tapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void Encontrar_Clicked(object? sender, EventArgs e)
    {
        Application.Current!.Windows[0].Page = new NavigationPage(new Explorar());
    }
}
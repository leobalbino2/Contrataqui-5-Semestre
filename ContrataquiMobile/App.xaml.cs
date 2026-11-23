using ContrataquiMobile.Models;
using ContrataquiMobile.Services;

namespace ContrataquiMobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            EstilizarPickersNoWindows();
        }

        // No Windows o Picker vira um ComboBox com cabeçalho, borda e altura mínima próprios,
        // que não cabem dentro do Border de 48 de altura dos formulários.
        private static void EstilizarPickersNoWindows()
        {
#if WINDOWS
            // Na criação do campo
            Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("EstiloPicker",
                (handler, view) => AjustarCombo(handler.PlatformView, view.Title));

            // E de novo sempre que o Title é atualizado (o MAUI recoloca o cabeçalho aí)
            Microsoft.Maui.Handlers.PickerHandler.Mapper.ModifyMapping(nameof(Microsoft.Maui.IPicker.Title),
                (handler, view, original) =>
                {
                    original?.Invoke(handler, view);
                    AjustarCombo(handler.PlatformView, view.Title);
                });
#endif
        }

#if WINDOWS
        private static void AjustarCombo(Microsoft.UI.Xaml.Controls.ComboBox combo, string? titulo)
        {
            // O Title deixa de ser cabeçalho e vira o texto de "nada selecionado"
            combo.Header = null;
            combo.HeaderTemplate = null;
            combo.PlaceholderText = titulo ?? "";

            combo.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            combo.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
            combo.Padding = new Microsoft.UI.Xaml.Thickness(0);
            combo.Margin = new Microsoft.UI.Xaml.Thickness(0);
            combo.MinHeight = 0;
            combo.MinWidth = 0;
            combo.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch;
            combo.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;
            combo.VerticalContentAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;

            // Só liga o evento uma vez por campo
            if (combo.Tag is string and "ajustado") return;
            combo.Tag = "ajustado";

            combo.Loaded += (s, e) =>
            {
                if (s is Microsoft.UI.Xaml.Controls.ComboBox c)
                    ColapsarCabecalho(c);
            };
        }

        // Esconde o espaço que o template do ComboBox reserva para o cabeçalho
        private static void ColapsarCabecalho(Microsoft.UI.Xaml.DependencyObject pai)
        {
            int total = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(pai);

            for (int i = 0; i < total; i++)
            {
                var filho = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(pai, i);

                if (filho is Microsoft.UI.Xaml.FrameworkElement fe && fe.Name == "HeaderContentPresenter")
                {
                    fe.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                    fe.Margin = new Microsoft.UI.Xaml.Thickness(0);
                    fe.Height = 0;
                }
                else
                {
                    ColapsarCabecalho(filho);
                }
            }
        }
#endif

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Com sessão salva abre direto na página inicial do usuário, senão no Shell (Login)
            Page inicial = Sessao.Restaurar()
                ? Navegacao.PaginaInicial(Sessao.Atual!)
                : new AppShell();

            return new Window(inicial)
            {
                Width = 480,
                Height = 800
            };
        }
    }
}
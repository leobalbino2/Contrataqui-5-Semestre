using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;

namespace ContrataquiMobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialIconsOutlined-Regular.otf", "MaterialIcons");
            });

        // Remove a borda, a sombra e a linha de foco do Entry no Windows e no Android
        EntryHandler.Mapper.AppendToMapping("SemBorda", (handler, view) =>
        {
#if WINDOWS
            var tb = handler.PlatformView;
            var transparente = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
            var zero = new Microsoft.UI.Xaml.Thickness(0);

            tb.BorderThickness = zero;
            tb.Background = transparente;

            tb.Resources["TextControlBorderThemeThickness"] = zero;
            tb.Resources["TextControlBorderThemeThicknessFocused"] = zero;
            tb.Resources["TextControlBackground"] = transparente;
            tb.Resources["TextControlBackgroundPointerOver"] = transparente;
            tb.Resources["TextControlBackgroundFocused"] = transparente;
            tb.Resources["TextControlBorderBrush"] = transparente;
            tb.Resources["TextControlBorderBrushPointerOver"] = transparente;
            tb.Resources["TextControlBorderBrushFocused"] = transparente;
#elif ANDROID
            handler.PlatformView.Background = null;
#endif
        });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
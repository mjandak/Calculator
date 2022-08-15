using CalcularorInterfaces;
using CalculatorMAUI.ViewModel;
using MathExpressionParser;

namespace CalculatorMAUI
{
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
                });

            builder.Services.AddSingleton<CalculatorVM>();
            builder.Services.AddSingleton<IMathExprProvider, MathExprProvider>();
            builder.Services.AddSingleton<MainPage>();

            return builder.Build();
        }
    }
}
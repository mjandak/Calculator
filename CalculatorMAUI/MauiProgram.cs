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
                    fonts.AddFont("ErbosDraco1StOpenNbpRegular-l5wX.ttf", "ErbosDraco1StOpenNbpRegular");
                    fonts.AddFont("DigitalNormal-xO6j.otf", "DigitalNormal");
                    fonts.AddFont("CursedTimerUlil-Aznm.ttf", "CursedTimerUlil");
                    fonts.AddFont("LcdSolid-VPzB.ttf", "LcdSolid");
                    fonts.AddFont("Bitwise.ttf", "Bitwise");
                });

            builder.Services.AddSingleton<CalculatorVM>();
            builder.Services.AddSingleton<IMathExprProvider, MathExprProvider>();
            builder.Services.AddSingleton<MainPage>();

            return builder.Build();
        }
    }
}
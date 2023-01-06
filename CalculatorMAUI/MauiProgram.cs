using CalcularorInterfaces;
using CalculatorMAUI.ViewModel;
using MathExpressionProvider;

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
                    fonts.AddFont("DigitalNormal-xO6j.otf", "DigitalNormal");
                    fonts.AddFont("CursedTimerUlil-Aznm.ttf", "CursedTimerUlil");
                    fonts.AddFont("LcdSolid-VPzB.ttf", "LcdSolid");
                    fonts.AddFont("Bitwise.ttf", "Bitwise");
                    fonts.AddFont("nk57-monospace-se-rg.ttf", "nk57-monospace-se-rg");
                    fonts.AddFont("nk57-monospace-se-sb.ttf", "nk57-monospace-se-sb");
                    fonts.AddFont("Aquifer.ttf", "Aquifer");
                    fonts.AddFont("Kingthings Trypewriter 2.ttf", "KingthingsTrypewriter2");
                    fonts.AddFont("Gipsiero.otf", "Gipsiero");
                    fonts.AddFont("dontwr_r.ttf", "DontWalkRun");
                    fonts.AddFont("repet___.ttf", "RepetitionScrolling");
                    fonts.AddFont("EHSMB.ttf", "EHSMB");
                    fonts.AddFont("Receiptional Receipt.ttf", "ReceiptionalReceipt");
                    fonts.AddFont("TEXASLED.ttf", "TEXASLED");
                    fonts.AddFont("erbos_draco_1st_open_nbp.ttf", "ErbosDraco1stOpenNBP");
                    fonts.AddFont("Hacked CRT.ttf", "HackedCRT");
                });

            builder.Services.AddSingleton<CalculatorVM>();
            builder.Services.AddSingleton<IMathExprProvider, MathExprProvider>();
            builder.Services.AddSingleton<MainPage>();

            return builder.Build();
        }
    }
}
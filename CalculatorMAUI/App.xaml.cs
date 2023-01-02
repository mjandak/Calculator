using CalculatorMAUI.Resources.Themes;

namespace CalculatorMAUI
{
    public partial class App : Application
    {
        private static ResourceDictionary currentTheme;

        public static void SetTheme(string name)
        {
            var mergedDictionaries = Current.Resources.MergedDictionaries;
            if (currentTheme != null)
            {
                mergedDictionaries.Remove(currentTheme);
            }
            if (name == "normal")
            {
                currentTheme = new Dictionary1();
                mergedDictionaries.Add(currentTheme);
            }
            else if (name == "retro")
            {
                currentTheme = new Dictionary2();
                mergedDictionaries.Add(currentTheme);
            }
            else
            {
                throw new ArgumentException("Unknown theme");
            }
        }

        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}
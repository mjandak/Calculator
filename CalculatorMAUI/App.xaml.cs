using CalculatorMAUI.Resources.Themes;
using Microsoft.Maui.Controls;

namespace CalculatorMAUI
{
    public partial class App : Application
    {
        private static ResourceDictionary currentTheme;

        public static void SetTheme(Themes theme)
        {
            var mergedDictionaries = Current.Resources.MergedDictionaries;
            mergedDictionaries.Clear();

            //Microsoft.Maui.Controls.Xaml.Extensions.LoadFromXaml()

            //ResourceDictionary rd = (ResourceDictionary)XamlReader.Load(System.Xml.XmlReader.Create("skin.xaml")); 
            //mergedDictionaries.Add(rd);

            //ResourceDictionary item = new ResourceDictionary()
            //{
            //    Source = new Uri("/CalculatorMAUI; component/Styles/Colors.xaml", UriKind.RelativeOrAbsolute)
            //};
            //mergedDictionaries.Add(item);
            //mergedDictionaries.Add(new ResourceDictionary()
            //{
            //    Source = new Uri("/CalculatorMAUI; component/Styles/Styles.xaml", UriKind.RelativeOrAbsolute)
            //});

            //if (currentTheme != null)
            //{
            //    mergedDictionaries.Remove(currentTheme);
            //}
            if (theme == Themes.Simple)
            {
                //currentTheme = new Simple();
                //mergedDictionaries.Add(currentTheme);
                mergedDictionaries.Add(new Simple());
                //mergedDictionaries.Add(new ResourceDictionary()
                //{
                //    Source = new Uri("/CalculatorMAUI; component/Themes/Simple.xaml", UriKind.RelativeOrAbsolute)
                //});
            }
            else if (theme == Themes.Retro)
            {
                //currentTheme = new Retro();
                //mergedDictionaries.Add(currentTheme);
                mergedDictionaries.Add(new Retro());
                //mergedDictionaries.Add(new ResourceDictionary()
                //{
                //    Source = new Uri("/CalculatorMAUI; component/Themes/Retro.xaml", UriKind.RelativeOrAbsolute)
                //});
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
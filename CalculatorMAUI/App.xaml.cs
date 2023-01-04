﻿using CalculatorMAUI.Resources.Themes;

namespace CalculatorMAUI
{
    public partial class App : Application
    {
        private static ResourceDictionary currentTheme;

        public static void SetTheme(Themes theme)
        {
            var mergedDictionaries = Current.Resources.MergedDictionaries;
            if (currentTheme != null)
            {
                mergedDictionaries.Remove(currentTheme);
            }
            if (theme == Themes.Simple)
            {
                currentTheme = new Simple();
                mergedDictionaries.Add(currentTheme);
            }
            else if (theme == Themes.Retro)
            {
                currentTheme = new Retro();
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
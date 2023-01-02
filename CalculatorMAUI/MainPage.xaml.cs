using CalculatorMAUI.Resources.Themes;
using CalculatorMAUI.ViewModel;
using Microsoft.Maui.Handlers;

namespace CalculatorMAUI
{
    public partial class MainPage : ContentPage
    {
        private readonly CalculatorVM calculatorVM;

        public MainPage(CalculatorVM vm)
        {
            InitializeComponent();
            //Android_HideSoftKeyboard();
            calculatorVM = vm;
            BindingContext = calculatorVM;
#if ANDROID
            txtDisplay.Behaviors.Add(new HideSoftInputBehavior());
#endif
            calculatorVM.BtnPressedExecuted += CalculatorVM_BtnPressedExecuted;
            //calculatorVM.BackspaceExecuted += CalculatorVM_BackspaceExecuted;
        }

        private void CalculatorVM_BackspaceExecuted()
        {
            txtDisplay.Focus();
        }

        private void CalculatorVM_BtnPressedExecuted(string obj)
        {
            txtDisplay.Focus();
        }

        private void btnBackspace_Clicked(object sender, EventArgs e)
        {
            if (txtDisplay.CursorPosition == 0)
            {
                txtDisplay.Focus();
            }
            else
            {
                int cursorPosition = txtDisplay.CursorPosition;
                txtDisplay.Text = txtDisplay.Text.Remove(cursorPosition - 1, 1);
                txtDisplay.Focus();
                txtDisplay.CursorPosition = cursorPosition - 1;
            }
        }

        private void themeSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            ICollection<ResourceDictionary> mergedDictionaries = Application.Current.Resources.MergedDictionaries;
            if (mergedDictionaries != null)
            {
                //mergedDictionaries.Clear();
                if (e.Value)
                {
                    mergedDictionaries.Remove(mergedDictionaries.OfType<Dictionary1>().First());
                    mergedDictionaries.Add(new Dictionary2());
                }
                else
                {
                    mergedDictionaries.Remove(mergedDictionaries.OfType<Dictionary2>().First());
                    mergedDictionaries.Add(new Dictionary1());
                }
            }
        }

        private void Normal_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                App.SetTheme("normal");
            }

        }

        private void Retro_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                App.SetTheme("retro");
            }
        }
    }
}

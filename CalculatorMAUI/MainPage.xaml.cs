using CalculatorMAUI.ViewModel;

namespace CalculatorMAUI
{
    public partial class MainPage : ContentPage
    {
        private readonly CalculatorVM calculatorVM;

        public MainPage(CalculatorVM vm)
        {
            InitializeComponent();
            calculatorVM = vm;
            BindingContext = calculatorVM;
            calculatorVM.BtnPressedExecuted += CalculatorVM_BtnPressedExecuted;
            calculatorVM.BackspaceExecuted += CalculatorVM_BackspaceExecuted;
        }

        private void CalculatorVM_BackspaceExecuted()
        {
            txtDisplay.Focus();
        }

        private void CalculatorVM_BtnPressedExecuted(string obj)
        {
            txtDisplay.Focus();
        }

        private void txtDisplay_Unfocused(object sender, FocusEventArgs e)
        {
            //txtDisplay.Focus();
        }

        private void txtDisplay_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
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
            calculatorVM.BtnPressedExecuted += CalculatorVM_BtnPressedExecuted;
            calculatorVM.BackspaceExecuted += CalculatorVM_BackspaceExecuted;
        }

//        private void Android_HideSoftKeyboard()
//        {
//#if ANDROID
//            var imm = (Android.Views.InputMethods.InputMethodManager)MauiApplication.Current.GetSystemService(Android.Content.Context.InputMethodService);

//            if (imm != null)
//            {
//                //this stuff came from here:  https://www.syncfusion.com/kb/12559/how-to-hide-the-keyboard-when-scrolling-in-xamarin-forms-listview-sflistview
//                Android.App.Activity activity = Platform.CurrentActivity;
//                Android.OS.IBinder wToken = activity.CurrentFocus?.WindowToken;
//                imm.HideSoftInputFromWindow(wToken, 0);
//            }

//            var context = Android.App.Application.Context;
//            var inputMethodManager = context.GetSystemService(Android.Content.Context.InputMethodService) as Android.Views.InputMethods.InputMethodManager;
//            if (inputMethodManager != null && context is Android.App.Activity)
//            {
//                var activity = context as Android.App.Activity;
//                var token = activity.CurrentFocus?.WindowToken;
//                inputMethodManager.HideSoftInputFromWindow(token, Android.Views.InputMethods.HideSoftInputFlags.None);
//                activity.Window.DecorView.ClearFocus();
//            }

//            //EntryHandler.Mapper.AppendToMapping("HideSoftKeyboard", (entryhandler, entry) =>
//            //{
//            //});
//#endif
//        }

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
    }
}

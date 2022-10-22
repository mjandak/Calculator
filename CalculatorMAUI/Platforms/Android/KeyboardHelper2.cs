using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Views.InputMethods;

namespace CalculatorMAUI
{
    public partial class KeyboardHelper2
    {
        public partial void HideSoftInput()
        {
            //Context context = Android.App.Application.Context;
            if (Android.App.Application.Context is Activity activity &&
                activity.GetSystemService(Context.InputMethodService) is InputMethodManager inputMethodManager)
            {
                Android.OS.IBinder token = activity.CurrentFocus?.WindowToken;
                inputMethodManager.HideSoftInputFromWindow(token, HideSoftInputFlags.None);
                //activity.Window.DecorView.ClearFocus();
            }
        }
    }
}

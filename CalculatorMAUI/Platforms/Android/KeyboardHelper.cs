using Android.Content;
using Android.Runtime;
using Android.Views.InputMethods;
using CalculatorMAUI.Platforms.Android;
using Android.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application = Android.App.Application;
using System.Diagnostics.CodeAnalysis;

[assembly: Dependency(typeof(KeyboardHelper))]
namespace CalculatorMAUI.Platforms.Android
{
    //[Preserve(AllMembers = true)]
    //[DynamicDependency()]
    public class KeyboardHelper : IKeyboardHelper
    {
        static Context _context;

        public KeyboardHelper()
        {

        }

        public static void Init(Context context)
        {
            _context = context;
        }

        public void HideKeyboard()
        {
            Context context = Application.Context;
            if (context.GetSystemService(Context.InputMethodService) is InputMethodManager inputMethodManager && context is Activity)
            {
                Activity activity = context as Activity;
                global::Android.OS.IBinder token = activity.CurrentFocus?.WindowToken;
                inputMethodManager.HideSoftInputFromWindow(token, HideSoftInputFlags.None);
                //activity.Window.DecorView.ClearFocus();
            }
        }
    }
}

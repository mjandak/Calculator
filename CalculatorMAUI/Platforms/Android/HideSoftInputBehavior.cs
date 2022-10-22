using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculatorMAUI
{
    public class HideSoftInputBehavior : PlatformBehavior<Entry, EditText>
    {
        protected override void OnAttachedTo(Entry bindable, EditText platformView)
        {
            platformView.ShowSoftInputOnFocus = false;
        }
    }
}

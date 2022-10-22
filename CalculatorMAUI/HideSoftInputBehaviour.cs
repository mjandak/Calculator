using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculatorMAUI
{
    public class HideSoftInputBehaviour : Behavior<Entry>
    {
        protected override void OnAttachedTo(Entry entry)
        {
            entry.Focused += Entry_Focused;
            base.OnAttachedTo(entry);

        }

        private void Entry_Focused(object sender, FocusEventArgs e)
        {
            
            DependencyService.Get<IKeyboardHelper>().HideKeyboard();
        }
    }
}

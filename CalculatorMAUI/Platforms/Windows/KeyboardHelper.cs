using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: Dependency(typeof(CalculatorMAUI.Platforms.Windows.KeyboardHelper))]
namespace CalculatorMAUI.Platforms.Windows
{
    public class KeyboardHelper : IKeyboardHelper
    {
        public void HideKeyboard()
        {

        }
    }
}

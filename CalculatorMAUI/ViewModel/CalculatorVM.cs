using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CalcularorInterfaces;

namespace CalculatorMAUI.ViewModel
{
    public class CalculatorVM : ViewModel
    {
        private string _display = string.Empty;
        private int displayCurrPos;
        private int displaySelecteLength;
        private string errorMsg;
        private readonly IMathExprProvider mathExprProvider;

        public ICommand DisplayCmd { get; set; }
        public ICommand Insert { get; private set; }
        public ICommand Backspace { get; private set; }
        public ICommand Calculate { get; private set; }

        /// <summary>
        /// Caret position or selection start position. Zero based.
        /// </summary>
        public int DisplayCaretPos
        {
            get => displayCurrPos;
            set
            {
                displayCurrPos = value;
                Notify();
            }
        }

        public int DisplaySelectedLength
        {
            get => displaySelecteLength;
            set
            {
                displaySelecteLength = value;
                Notify();
            }
        }

        public string Display
        {
            get
            {
                return _display;
            }
            set
            {
                _display = value;
                Notify();
            }
        }

        public string ErrorMsg
        {
            get => errorMsg;
            private set
            {
                errorMsg = value;
                Notify();
            }
        }

        public event Action<string> BtnPressedExecuted;
        public event Action BackspaceExecuted;

        public CalculatorVM(IMathExprProvider mathExprProvider)
        {
            Insert = new Command<string>(ExecuteInsert);
            Backspace = new Command(ExecuteBackspace);
            Calculate = new Command(Calulate);
            this.mathExprProvider = mathExprProvider;
        }

        private void ExecuteBackspace()
        {
            if (DisplaySelectedLength > 0)
            {
                Display = Display.Remove(DisplayCaretPos, DisplaySelectedLength);
                DisplaySelectedLength = 0;
            }
            else
            {
                if (DisplayCaretPos != 0)
                {
                    Display = Display.Remove(DisplayCaretPos - 1, 1);
                    DisplayCaretPos--;
                }
            }
            BackspaceExecuted();
        }

        private void ExecuteInsert(string command)
        {
            if (DisplaySelectedLength > 0)
            {
                var newCaretPos = DisplayCaretPos + command.Length;
                Display = Display.Remove(DisplayCaretPos, DisplaySelectedLength).Insert(DisplayCaretPos, command);
                DisplayCaretPos = newCaretPos;
                DisplaySelectedLength = 0;
            }
            else
            {
                int newCaretPos = DisplayCaretPos;

                var text = command;
                newCaretPos = DisplayCaretPos + text.Length;
                Display = Display.Insert(DisplayCaretPos, text);

                DisplayCaretPos = newCaretPos;
            }

            BtnPressedExecuted(command);
        }

        private void Calulate()
        {
            ErrorMsg = string.Empty;
            try
            {
                Display = mathExprProvider.Evaluate(Display);
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message;
            }
        }
    }
}

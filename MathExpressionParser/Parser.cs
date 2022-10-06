using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathExpressionParser
{
    public class Parser<T> where T : INumericOps<T>
    {
        private static readonly Dictionary<string, int> _priorities = new()
        {
            {"+", 1},
            {"-", 1},
            {"*", 2},
            {"/", 3}, //divide first to try to avoid OverflowException
            {"^", 4},
        };
        private static readonly string[] operators = new string[] { "+", "-", "*", "/", "^", };
        private static readonly string[] functions = new string[] { "sin", "cos", "tan", "cot" };
        private static readonly Dictionary<States, int> _wrongStates = new Dictionary<States, int>()
        {
            {States.Digit, (int)(States.LeftBracket | States.Function) },
            {States.Operator, (int)(States.RightBracket | States.Operator) },
            {States.LeftBracket, 0},
            {States.RightBracket, (int)(States.Function | States.LeftBracket) },
            {States.Function, (int)(States.Digit | States.Operator | States.RightBracket) },
            {States.Start, 0 },
            {States.Whitespace, 0},
        };
        private static Stack<Symbol> _symbols;

        public static int? FindFirstLowestPriorityOp(string expr)
        {
            int lowestPriority = int.MaxValue;
            int? ret = null;
            ushort bracketsCount = 0;
            for (int i = expr.Length - 1; i > -1; i--)
            {
                if (expr[i] == ')')
                {
                    bracketsCount++;
                    continue;
                }
                else if (expr[i] == '(')
                {
                    bracketsCount--;
                    continue;
                }
                if (bracketsCount > 0) continue;
                if (_priorities.TryGetValue(expr[i].ToString(), out int priority))
                {
                    if (priority < lowestPriority)
                    {
                        lowestPriority = priority;
                        ret = i;
                    }
                }
            }
            return ret;
        }

        public static int? FindFirstLowestPriorityOp2(Symbol[] symbols)
        {
            int lowestPriority = int.MaxValue;
            int? ret = null;
            ushort bracketsCount = 0;
            for (int i = symbols.Length - 1; i > -1; i--)
            {
                if (symbols[i].Text == ")")
                {
                    bracketsCount++;
                    continue;
                }
                else if (symbols[i].Text == "(")
                {
                    bracketsCount--;
                    continue;
                }
                if (bracketsCount > 0) continue;
                if (_priorities.TryGetValue(symbols[i].Text, out int priority))
                {
                    if (priority < lowestPriority)
                    {
                        lowestPriority = priority;
                        ret = i;
                    }
                }
            }
            return ret;
        }

        public static (string fnName, string fnParam) GetFunctionParts(string expr)
        {
            var leftBracketIdx = expr.IndexOf('(');
            var fnName = expr.Substring(0, leftBracketIdx);
            var fnParam = expr.Substring(leftBracketIdx + 1, expr.Length - 2 - leftBracketIdx);
            return (fnName, fnParam);
        }

        public static void CheckValidity(string expr)
        {
            Stack<char> bracktets = new Stack<char>();
            States previousState = States.Start;
            StringBuilder currentFunc = new();

            States currentState = getState(expr[0]);
            if (currentState == States.Operator || currentState == States.RightBracket)
            {
                ThrowBadChar(0);
            }
            if (currentState == States.Function)
            {
                currentFunc.Append(expr[0]);
            }
            else if (currentState == States.LeftBracket)
            {
                bracktets.Push('(');
            }

            for (int i = 1; i < expr.Length; i++)
            {
                if (char.IsWhiteSpace(expr[i]))
                {
                    continue;
                }

                currentState = getState(expr[i]);

                if (currentState == States.RightBracket)
                {
                    if (bracktets.Peek() != '(' || bracktets.Count == 0)
                    {
                        throw new Exception($"Unexpected character at {i}.");
                    }
                    else
                    {
                        bracktets.Pop();
                    }
                }
                else if (currentState == States.LeftBracket)
                {
                    bracktets.Push('(');
                }
                else if (currentState == States.Function)
                {
                    currentFunc.Append(expr[i]);
                }

                if (currentState == States.LeftBracket && previousState == States.Function)
                {
                    var currentFuncText = currentFunc.ToString();
                    if (!functions.Contains(currentFuncText))
                    {
                        throw new Exception($"Unknown function: {currentFuncText} at {i - currentFuncText.Length}");
                    }
                    currentFunc.Clear();
                }

                if (previousState == States.Digit)
                {
                    if (currentState == States.LeftBracket || currentState == States.Function)
                    {
                        ThrowBadChar(i);
                    }
                }
                if (previousState == States.LeftBracket)
                {
                    if (currentState == States.Operator || currentState == States.RightBracket)
                    {
                        ThrowBadChar(i);
                    }
                }
                if (previousState == States.RightBracket)
                {
                    if (currentState == States.Digit || currentState == States.Function)
                    {
                        ThrowBadChar(i);
                    }
                }
                if (previousState == States.Operator)
                {
                    if (currentState == States.Operator || currentState == States.RightBracket)
                    {
                        ThrowBadChar(i);
                    }
                }
                if (previousState == States.Function)
                {
                    if (currentState == States.Digit || currentState == States.RightBracket)
                    {
                        ThrowBadChar(i);
                    }
                }

                previousState = currentState;
            }

            if (previousState == States.Function || previousState == States.LeftBracket || previousState == States.Operator)
            {
                throw new Exception("Unexpected end of expression.");
            }
            if (bracktets.Count > 0)
            {
                throw new Exception("Unexpected end of expression.");
            }
        }

        public static Node<T> ParseExpr(string expr)
        {
            return ParseExpr(CheckValidity2(expr));
        }

        private static Node<T> ParseExpr(Symbol[] symbols)
        {
            int? opIdx = FindFirstLowestPriorityOp2(symbols);
            if (opIdx == null)
            {
                //expr is a single number or expression in brackets or function
                if (symbols.First() is Number<T> number)
                {
                    //number
                    return number;
                }
                if (symbols.First().Text == "(")
                {
                    //sub-expression
                    return ParseExpr(symbols.Skip(1).Take(symbols.Length - 2).ToArray());
                }
                if (symbols.First() is Operation<T> fn)
                {
                    //function
                    fn.Left = ParseExpr(symbols.Skip(1).ToArray());
                    return fn;
                }
                return null;
            }

            Operation<T> op = new(symbols[opIdx.Value].Text);

            Symbol[] left;
            if (opIdx == 0)
            {
                T zero = default;
                zero.TryParse(")", out zero);
                op.Left = new Number<T>(zero);
            }
            else
            {
                left = symbols.Take(opIdx.Value).ToArray();
                op.Left = ParseExpr(left);
            }
            
            Symbol[] right = symbols.Skip(opIdx.Value + 1).ToArray();
            op.Right = ParseExpr(right);
            
            return op;
        }

        public static Symbol[] CheckValidity2(string expr)
        {
            _symbols = new Stack<Symbol>();
            Stack<char> bracktets = new();
            States previousState = States.Start;
            States currentState;
            StringBuilder currentFunc = new();
            StringBuilder currentNum = new();
            bool currentHasNumDot = false;

            //previousState = getState(expr[0]);
            //if (currentState == States.RightBracket)
            //{
            //    ThrowBadChar(0);
            //}
            //if (currentState == States.Function)
            //{
            //    currentFunc.Append(expr[0]);
            //}
            //else if (currentState == States.LeftBracket)
            //{
            //    bracktets.Push('(');
            //    symbols.Push(new("("));
            //}
            //previousState = currentState;

            for (int i = 0; i <= expr.Length; i++)
            {
                if (i == expr.Length)
                {
                    currentState = States.End;
                }
                else
                {
                    currentState = getState(expr[i]);
                }

                if (currentState == States.Unknown)
                {
                    ThrowBadChar(i);
                }
                if (!IsStateAllowed(previousState, currentState))
                {
                    ThrowBadChar(i);
                }
                if (currentState == States.Digit)
                {
                    //symbols.Peek().Text += expr[i];
                    currentNum.Append(expr[i]);
                    if (expr[i] == '.')
                    {
                        if (currentHasNumDot)
                        {
                            ThrowBadChar(i);
                        }
                        else { currentHasNumDot = true; }
                    }
                }
                else if (currentState == States.Function)
                {
                    //symbols.Peek().Text += expr[i];
                    currentFunc.Append(expr[i]);
                }
                if (HasSymbolSequenceEnded(previousState, currentState))
                {
                    //if (symbols.Peek() is Operation<T>)
                    //{
                    //    if (!functions.Contains(symbols.Peek().Text) && !operators.Contains(symbols.Peek().Text))
                    //    {
                    //        throw new Exception($"Unknown function at {symbols.Peek().StartIdx}");
                    //    }
                    //}

                    //create new symbol
                    if (previousState == States.Digit)
                    {
                        T number3 = default;
                        _ = number3.TryParse(currentNum.ToString(), out number3);
                        _symbols.Push(new Number<T>(number3));
                        currentNum.Clear();
                        currentHasNumDot = false;
                    }
                    else if (previousState == States.Function)
                    {
                        _symbols.Push(new Operation<T>(currentFunc.ToString()));
                        currentFunc.Clear();
                    }
                    else if (previousState == States.LeftBracket)
                    {
                        bracktets.Push('(');
                        _symbols.Push(new Symbol("("));
                    }
                    else if (previousState == States.Operator)
                    {
                        _symbols.Push(new Operation<T>(expr[i - 1]));
                        currentFunc.Clear();
                    }
                    else if (previousState == States.RightBracket)
                    {
                        if (bracktets.Count < 1)
                        {
                            ThrowBadChar(i);
                        }
                        bracktets.Pop();
                        _symbols.Push(new Symbol(")"));
                    }
                }
                //else
                //{
                //    if (currentState == States.Digit)
                //    {
                //        //symbols.Peek().Text += expr[i];
                //        currentNum.Append(expr[i]);
                //    }
                //    else if (currentState == States.Function)
                //    {
                //        //symbols.Peek().Text += expr[i];
                //        currentFunc.Append(expr[i]);
                //    }
                //}
                previousState = currentState;
            }

            if (bracktets.Count != 0)
            {
                throw new Exception("Unexpected end of expression.");
            }

            return _symbols.Reverse().ToArray();
        }

        private static States getState(char ch)
        {
            if (char.IsDigit(ch) || ch == '.')
            {
                return States.Digit;
            }
            else if (char.IsLetter(ch))
            {
                return States.Function;
            }
            else if (char.IsWhiteSpace(ch))
            {
                return States.Whitespace;
            }
            else if (ch == '(')
            {
                return States.LeftBracket;
            }
            else if (ch == ')')
            {
                return States.RightBracket;
            }
            else if (operators.Contains(ch.ToString()))
            {
                return States.Operator;
            }
            else
            {
                return States.Unknown;
            }
        }

        private static void ThrowBadChar(int i)
        {
            throw new Exception($"Unexpected character at {i}.");
        }

        private static bool IsStateAllowed(States prev, States current)
        {
            return (_wrongStates[prev] & (int)current) == 0;
        }

        private static bool HasSymbolSequenceEnded(States prev, States current)
        {
            //if (prev == States.Digit)
            //{
            //    return current == States.Operator || current == States.RightBracket || current == States.Whitespace;
            //}
            //if (prev == States.Function)
            //{
            //    return current == States.LeftBracket || current == States.Whitespace;
            //}
            if (prev == States.Start)
            {
                return false;
            }
            if (prev == States.LeftBracket && current == States.LeftBracket)
            {
                return true;
            }
            if (prev == States.RightBracket && current == States.RightBracket)
            {
                return true;
            }
            if (prev == States.Whitespace)
            {
                return false;
            }
            return prev != current;
        }

        enum States
        {
            Start = 0,
            Digit = 1,
            Operator = 2,
            LeftBracket = 4,
            RightBracket = 8,
            Function = 16,
            Unknown = 32,
            Whitespace = 64,
            End = 128
        }
    }

    [DebuggerDisplay("{Text}")]
    public class Symbol
    {
        public string Text { get; set; }

        public ushort StartIdx { get; set; }

        public Symbol(string text)
        {
            Text = text;
        }
    }

    public abstract class Node<T> : Symbol where T : INumericOps<T>
    {
        public Node(string text) : base(text)
        {

        }
        public abstract Operation<T> Parent { get; set; }
        public abstract string ToJson();
        public abstract T Evalute();
    }

    public class Operation<T> : Node<T> where T : INumericOps<T>
    {
        private Node<T> _parent;
        private Node<T> _left;
        private Node<T> _right;

        public Operation(string symbol) : base(symbol)
        {
        }

        public Operation(char symbol) : base(symbol.ToString())
        {
        }

        public Node<T> Left
        {
            get => _left;
            set
            {
                value.Parent = this;
                _left = value;
            }
        }

        public Node<T> Right
        {
            get => _right;
            set
            {
                value.Parent = this;
                _right = value;
            }
        }

        public override Operation<T> Parent { get; set; }

        public override string ToJson()
        {
            return $"{{Symbol:\"{Text}\", Left:{Left.ToJson()}, Right:{Right.ToJson()}}}";
        }

        public override T Evalute()
        {
            switch (Text)
            {
                case "+":
                    return Left.Evalute().Add(Right.Evalute());
                case "-":
                    return Left.Evalute().Subtract(Right.Evalute());
                case "*":
                    return Left.Evalute().Multiply(Right.Evalute());
                case "/":
                    return Left.Evalute().Divide(Right.Evalute());
                case "^":
                    return Left.Evalute().Power(Right.Evalute());
                case "sin":
                    return Left.Evalute().Sin();
                case "cos":
                    return Left.Evalute().Cos();
                case "tan":
                    return Left.Evalute().Tan();
                case "cot":
                    //return 1 / Math.Tan(Left.Evalute());
                    throw new NotImplementedException();
                default:
                    throw new Exception($"Unknown operation: {Text}");
            }
        }
    }

    public class Number<T> : Node<T> where T : INumericOps<T>
    {
        public T Value { get; set; }
        public override Operation<T> Parent { get; set; }

        public Number(T number) : base(number.ToString())
        {
            Value = number;
        }

        public override string ToJson()
        {
            return Value.ToString();
        }

        public override T Evalute()
        {
            return Value;
        }
    }

    /// <summary>
    /// Math operations on an instance of T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface INumericOps<T>
    {
        public T Add(T rvalue);
        public T Subtract(T rvalue);
        public T Multiply(T rvalue);
        public T Divide(T rvalue);
        public T Power(T exponent);
        public T Sin();
        public T Cos();
        public T Tan();
        bool TryParse(string expr, out T number);
    }

    public struct DecimalFractionNumeric : INumericOps<DecimalFractionNumeric>
    {
        decimal _enumerator;
        decimal _denominator;

        public DecimalFractionNumeric(decimal value)
        {
            IsFraction = false;
            _enumerator = value;
            _denominator = 1m;
        }

        public DecimalFractionNumeric(decimal enumerator, decimal denominator)
        {
            var gcd = Gcd(enumerator, denominator);
            IsFraction = true;
            _enumerator = enumerator / gcd;
            _denominator = denominator / gcd;
        }

        public decimal Value
        {
            get
            {
                return _enumerator / _denominator;
            }
        }

        public bool IsFraction { get; }

        public DecimalFractionNumeric Enumerator
        {
            get { return _enumerator; }
            private set { _enumerator = value; }
        }

        public DecimalFractionNumeric Denominator
        {
            get { return _denominator; }
            private set { _denominator = value; }
        }

        public decimal GetResult()
        {
            return _enumerator / _denominator;
        }

        public void SetFraction(DecimalFractionNumeric enumerator, DecimalFractionNumeric denominator)
        {
            Denominator = denominator;
            Enumerator = enumerator;
        }

        public DecimalFractionNumeric Add(DecimalFractionNumeric rvalue)
        {
            var g = Gcd(_denominator, rvalue._denominator);
            var e = ((rvalue._denominator / g) * _enumerator) + ((_denominator / g) * rvalue._enumerator);
            var d = (_denominator / g) * rvalue._denominator;

            return new DecimalFractionNumeric(e, d);
        }

        public DecimalFractionNumeric Divide(DecimalFractionNumeric rvalue)
        {
            var gcd1 = Gcd(_enumerator, rvalue._enumerator);
            var gcd2 = Gcd(_denominator, rvalue._enumerator);
            var e = (_enumerator / gcd1) * (rvalue._denominator / gcd2);
            var d = (_denominator / gcd2) * (rvalue._enumerator / gcd1);
            return new DecimalFractionNumeric(e, d);
        }

        public DecimalFractionNumeric Multiply(DecimalFractionNumeric rvalue)
        {
            var gcd1 = Gcd(_enumerator, rvalue._denominator);
            var gcd2 = Gcd(_denominator, rvalue._enumerator);
            var e = (_enumerator / gcd1) * (rvalue._enumerator / gcd2);
            var d = (_denominator / gcd2) * (rvalue._denominator / gcd1);
            return new DecimalFractionNumeric(e, d);
        }

        public DecimalFractionNumeric Power(DecimalFractionNumeric exponent)
        {
            return new DecimalFractionNumeric(ElementaryFunctionsDecimal.Pow(Value, exponent.Value));
        }

        public DecimalFractionNumeric Subtract(DecimalFractionNumeric rvalue)
        {
            return Add(rvalue.Negate());
        }

        public DecimalFractionNumeric Negate()
        {
            return new DecimalFractionNumeric(-1 * _enumerator, _denominator);
        }

        public bool TryParse(string expr, out DecimalFractionNumeric number)
        {
            decimal d;
            if (decimal.TryParse(expr, out d))
            {
                number = new DecimalFractionNumeric(d);
                return true;
            }
            number = new DecimalFractionNumeric(d);
            return false;
        }

        public static implicit operator DecimalFractionNumeric(decimal value)
        {
            return new DecimalFractionNumeric(value);
        }

        public static implicit operator decimal(DecimalFractionNumeric value)
        {
            return value.Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public string ToString(int decimalPlaces)
        {
            return decimal.Round(Value, decimalPlaces).ToString();
        }

        public static decimal Gcd(decimal a, decimal b)
        {
            if (b > a)
            {
                var tmp = a;
                a = b;
                b = tmp;
            }

            while (b > 0)
            {
                var r = a % b;
                a = b;
                b = r;
            }

            return a;
        }

        public static decimal Lcm(decimal a, decimal b)
        {
            return (a / Gcd(a, b)) * b;
        }

        public DecimalFractionNumeric Sin()
        {
            return new DecimalFractionNumeric(ElementaryFunctionsDecimal.Sin_taylor_reduced(Value));
        }

        public DecimalFractionNumeric Cos()
        {
            return new DecimalFractionNumeric(ElementaryFunctionsDecimal.Cos_taylor_reduced(Value));
        }

        public DecimalFractionNumeric Tan()
        {
            return new DecimalFractionNumeric(ElementaryFunctionsDecimal.Tan(Value));
        }
    }

    public struct DecimalNumneric : INumericOps<DecimalNumneric>
    {
        private decimal _value;

        public DecimalNumneric(decimal value)
        {
            _value = value;
        }

        public DecimalNumneric Add(DecimalNumneric rvalue)
        {
            return new DecimalNumneric(_value + rvalue._value);
        }

        public DecimalNumneric Divide(DecimalNumneric rvalue)
        {
            return new DecimalNumneric(_value / rvalue._value);
        }

        public DecimalNumneric Multiply(DecimalNumneric rvalue)
        {
            return new DecimalNumneric(_value * rvalue._value);
        }

        public DecimalNumneric Power(DecimalNumneric exponent)
        {
            throw new NotImplementedException();
        }

        public DecimalNumneric Subtract(DecimalNumneric rvalue)
        {
            return new DecimalNumneric(_value - rvalue._value);
        }

        public bool TryParse(string expr, out DecimalNumneric number)
        {
            if (decimal.TryParse(expr, out decimal result))
            {
                number = new DecimalNumneric(result);
                return true;
            }
            number = default;
            return false;
        }

        public static implicit operator DecimalNumneric(decimal value)
        {
            return new DecimalNumneric(value);
        }

        public static implicit operator decimal(DecimalNumneric value)
        {
            return value._value;
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public DecimalNumneric Sin()
        {
            throw new NotImplementedException();
        }

        public DecimalNumneric Cos()
        {
            throw new NotImplementedException();
        }

        public DecimalNumneric Tan()
        {
            throw new NotImplementedException();
        }
    }
}

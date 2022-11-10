using MathExpressionParser;
using MathExpressionParser.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathExpressionParser
{
    public class Parser<T> where T : INumericOps<T>, INumber, new()
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
        private static readonly string[] functions = new string[] { "sin", "cos", "tan", "cot", "log10", "ln" };
        private static readonly Dictionary<CharStates, int> _wrongStates = new()
        {
            {CharStates.Digit, (int)(CharStates.LeftBracket | CharStates.Function) },
            {CharStates.Operator, (int)(CharStates.RightBracket | CharStates.Operator) },
            {CharStates.LeftBracket, 0},
            {CharStates.RightBracket, (int)(CharStates.Function | CharStates.LeftBracket) },
            {CharStates.Function, (int)(CharStates.Digit | CharStates.Operator | CharStates.RightBracket) },
            {CharStates.Start, 0 },
            {CharStates.Whitespace, 0},
        };
        private static Stack<Token> _tokens;

        private static int? FindFirstLowestPriorityOp(Token[] symbols)
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

        public static Node<T> ParseExpr(string expr)
        {
            if (string.IsNullOrWhiteSpace(expr))
            {
                throw new ArgumentException($"'{nameof(expr)}' cannot be null or whitespace.", nameof(expr));
            }
            return ParseExpr(GetTokens(expr));
        }

        private static Node<T> ParseExpr(Token[] symbols)
        {
            int? opIdx = FindFirstLowestPriorityOp(symbols);
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

            if (symbols[opIdx.Value] is not Operation<T> op)
            {
                throw new Exception("Unexpected symbol.");
            }

            Token[] left;
            if (opIdx == 0)
            {
                op.Left = new Number<T>("0", 0);
            }
            else
            {
                left = symbols.Take(opIdx.Value).ToArray();
                op.Left = ParseExpr(left);
            }

            Token[] right = symbols.Skip(opIdx.Value + 1).ToArray();
            op.Right = ParseExpr(right);

            return op;
        }

        public static Token[] GetTokens(string expr)
        {
            _tokens = new Stack<Token>();
            Stack<char> bracktets = new();
            CharStates previousState = CharStates.Start;
            CharStates currentState;
            StringBuilder currentFunc = new();
            StringBuilder currentNum = new();
            StringBuilder currentNumOrFunc = new();
            bool currentHasNumDot = false;

            for (int i = 0; i <= expr.Length; i++)
            {
                if (i == expr.Length)
                {
                    currentState = CharStates.End;
                }
                else
                {
                    currentState = GetState(expr[i]);
                }

                if (currentState == CharStates.Unknown)
                {
                    ThrowBadChar(i);
                }
                if (!IsStateAllowed(previousState, currentState))
                {
                    ThrowBadChar(i);
                }
                if (currentState == CharStates.Digit)
                {
                    //currentNum.Append(expr[i]);
                    currentNumOrFunc.Append(expr[i]);
                    if (expr[i] == '.')
                    {
                        if (currentHasNumDot)
                        {
                            ThrowBadChar(i);
                        }
                        else { currentHasNumDot = true; }
                    }
                }
                else if (currentState == CharStates.Function)
                {
                    //currentFunc.Append(expr[i]);
                    currentNumOrFunc.Append(expr[i]);
                }
                if (TokenHasEnded(previousState, currentState))
                {
                    //create new token
                    if (previousState == CharStates.Digit)
                    {
                        if (_tokens.TryPeek(out Token n) && n is Number<T>)
                        {
                            ThrowBadChar(i - currentNum.Length);
                        }
                        _tokens.Push(new Number<T>(currentNum.ToString(), i - currentNum.Length));
                        currentNum.Clear();
                        currentHasNumDot = false;
                    }
                    else if (previousState == CharStates.Function)
                    {
                        if (!functions.Contains(currentFunc.ToString()))
                        {
                            throw new Exception($"Unknown function '{currentFunc}'");
                        }
                        _tokens.Push(new Operation<T>(currentFunc.ToString(), i - currentFunc.Length));
                        currentFunc.Clear();
                    }
                    else if (previousState == CharStates.Digit || previousState == CharStates.Function)
                    {

                    }
                    else if (previousState == CharStates.LeftBracket)
                    {
                        if (!functions.Contains(currentNumOrFunc.ToString()))
                        {
                            throw new Exception($"Unknown function '{currentFunc}'");
                        }

                        bracktets.Push('(');
                        _tokens.Push(new Token("(", i - 1));
                    }
                    else if (previousState == CharStates.Operator)
                    {
                        _tokens.Push(new Operation<T>(expr[i - 1], i - 1));
                    }
                    else if (previousState == CharStates.RightBracket)
                    {
                        if (bracktets.Count < 1)
                        {
                            ThrowBadChar(i);
                        }
                        bracktets.Pop();
                        _tokens.Push(new Token(")", i - 1));
                    }
                }
                previousState = currentState;
            }

            if (bracktets.Count != 0)
            {
                throw new Exception("Unexpected end of expression.");
            }

            return _tokens.Reverse().ToArray();
        }

        private static CharStates GetState(char ch)
        {
            if (char.IsDigit(ch) || ch == '.')
            {
                return CharStates.Digit;
            }
            else if (char.IsLetter(ch))
            {
                return CharStates.Function;
            }
            else if (char.IsWhiteSpace(ch))
            {
                return CharStates.Whitespace;
            }
            else if (ch == '(')
            {
                return CharStates.LeftBracket;
            }
            else if (ch == ')')
            {
                return CharStates.RightBracket;
            }
            else if (operators.Contains(ch.ToString()))
            {
                return CharStates.Operator;
            }
            else
            {
                return CharStates.Unknown;
            }
        }

        private static void ThrowBadChar(int i)
        {
            throw new Exception($"Unexpected character at {i}.");
        }

        private static bool IsStateAllowed(CharStates prev, CharStates current)
        {
            return (_wrongStates[prev] & (int)current) == 0;
        }

        private static bool TokenHasEnded(CharStates prev, CharStates current)
        {
            if (prev == CharStates.Start)
            {
                return false;
            }
            if (prev == CharStates.LeftBracket && current == CharStates.LeftBracket)
            {
                return true;
            }
            if (prev == CharStates.RightBracket && current == CharStates.RightBracket)
            {
                return true;
            }
            if (prev == CharStates.Whitespace)
            {
                return false;
            }
            return prev != current;
        }

        enum CharStates
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
    public class Token
    {
        public string Text { get; set; }

        public ushort StartIdx { get; set; }

        public Token(string text, ushort startIdx)
        {
            Text = text;
            StartIdx = startIdx;
        }

        public Token(string text, int startIdx)
        {
            Text = text;
            StartIdx = (ushort)startIdx;
        }
    }

    public abstract class Node<T> : Token where T : INumericOps<T>, INumber
    {
        public Node(string text, int startIdx) : base(text, startIdx)
        {

        }
        public abstract Operation<T> Parent { get; set; }
        public abstract string ToJson();
        public abstract T Evalute();
    }

    public class Operation<T> : Node<T> where T : INumericOps<T>, INumber
    {
        private Node<T> _parent;
        private Node<T> _left;
        private Node<T> _right;

        public Operation(string symbol, int startIdx) : base(symbol, startIdx)
        {
        }

        public Operation(char symbol, int startIdx) : base(symbol.ToString(), startIdx)
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
                case "log10":
                    return Left.Evalute().Log10();
                case "ln":
                    return Left.Evalute().Ln();
                default:
                    throw new Exception($"Unknown operation: {Text}");
            }
        }
    }

    public class Number<T> : Node<T> where T : INumericOps<T>, INumber, new()
    {
        public T Value { get; set; }

        public override Operation<T> Parent { get; set; }

        public Number(string number, int startIdx) : base(number, startIdx)
        {
            T n = new();
            n.SetValue(number);
            Value = n;
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

    public struct DecimalFractionNumeric : INumericOps<DecimalFractionNumeric>, INumber
    {
        decimal _enumerator;
        decimal _denominator;

        public DecimalFractionNumeric()
        {
            _enumerator = 0m;
            _denominator = 1m;
        }

        public DecimalFractionNumeric(string value) : this(decimal.Parse(value)) { }

        public DecimalFractionNumeric(decimal value)
        {
            _enumerator = value;
            _denominator = 1m;
        }

        public DecimalFractionNumeric(decimal enumerator, decimal denominator)
        {
            var gcd = Gcd(enumerator, denominator);
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

        public void SetValue(string value)
        {
            _denominator = 1m;
            _enumerator = decimal.Parse(value);
        }

        public DecimalFractionNumeric Log10()
        {
            return new DecimalFractionNumeric(ElementaryFunctionsDecimal.Log10(Value));
        }

        public DecimalFractionNumeric Ln()
        {
            return new DecimalFractionNumeric(ElementaryFunctionsDecimal.Ln(Value));
        }
    }

    public struct DecimalNumneric : INumericOps<DecimalNumneric>, INumber
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

        public void SetValue(string value)
        {
            throw new NotImplementedException();
        }

        public DecimalNumneric Log10()
        {
            throw new NotImplementedException();
        }

        public DecimalNumneric Ln()
        {
            throw new NotImplementedException();
        }
    }

    //public class DecimalFractionNumbers : INumbers<DecimalFractionNumeric>
    //{
    //    public DecimalFractionNumeric GetNumber(string number)
    //    {
    //        return new DecimalFractionNumeric(number);
    //    }
    //}
}

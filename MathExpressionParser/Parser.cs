using MathExpressionParser;
using MathExpressionParser.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
        private static Stack<Token> _tokens;

        private static int? FindFirstLowestPriorityOp(Token[] tokens)
        {
            int lowestPriority = int.MaxValue;
            int? ret = null;
            ushort bracketsCount = 0;
            for (int i = tokens.Length - 1; i > -1; i--)
            {
                if (tokens[i].Text == ")")
                {
                    bracketsCount++;
                    continue;
                }
                else if (tokens[i].Text == "(")
                {
                    bracketsCount--;
                    continue;
                }
                if (bracketsCount > 0) continue;
                if (_priorities.TryGetValue(tokens[i].Text, out int priority))
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

        private static Node<T> ParseExpr(Token[] tokens)
        {
            int? opIdx = FindFirstLowestPriorityOp(tokens);
            if (opIdx == null)
            {
                //expr is a single number or expression in brackets or function
                if (tokens.First() is Number<T> number)
                {
                    //number
                    return number;
                }
                if (tokens.First().Text == "(")
                {
                    //sub-expression
                    return ParseExpr(tokens.Skip(1).Take(tokens.Length - 2).ToArray());
                }
                if (tokens.First() is Operation<T> fn)
                {
                    //function
                    fn.Left = ParseExpr(tokens.Skip(1).ToArray());
                    return fn;
                }
                return null;
            }

            if (tokens[opIdx.Value] is not Operation<T> op)
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
                left = tokens.Take(opIdx.Value).ToArray();
                op.Left = ParseExpr(left);
            }

            Token[] right = tokens.Skip(opIdx.Value + 1).ToArray();
            op.Right = ParseExpr(right);

            return op;
        }

        public static Token[] GetTokens(string expr)
        {
            _tokens = new Stack<Token>();
            _tokens.Push(new Token(null, 0, (int)CharState.Start));
            Stack<char> bracktets = new();
            CharState previousState = CharState.Start;
            CharState currentState;
            StringBuilder currentNumOrFunc = new();
            int currentNumOrFuncStates = 0;
            ushort numberOfDots = 0;

            for (int i = 0; i <= expr.Length; i++)
            {
                if (i == expr.Length)
                {
                    currentState = CharState.End;
                }
                else
                {
                    currentState = GetState(expr[i]);
                }

                if (currentState == CharState.NotAllowed)
                {
                    ThrowBadChar(i);
                }
                if (currentState == CharState.Digit)
                {
                    currentNumOrFunc.Append(expr[i]);
                    currentNumOrFuncStates |= (int)CharState.Digit;
                }
                else if (currentState == CharState.Function)
                {
                    currentNumOrFunc.Append(expr[i]);
                    currentNumOrFuncStates |= (int)CharState.Function;
                }
                else if (currentState == CharState.Dot)
                {
                    currentNumOrFunc.Append(expr[i]);
                    currentNumOrFuncStates |= (int)CharState.Dot;
                    numberOfDots++;
                }
                if (TokenHasEnded(previousState, currentState))
                {
                    //create new token

                    Token previousToken = _tokens.Peek();

                    if (previousState == CharState.Digit || previousState == CharState.Function || previousState == CharState.Dot)
                    {
                        if (previousToken.HasState(CharState.Function) || previousToken.HasState(CharState.Digit) || previousToken.HasState(CharState.RightBracket))
                        {
                            ThrowBadChar(i - currentNumOrFunc.Length);
                        }
                        if ((currentNumOrFuncStates & (int)CharState.Digit) == 0 || numberOfDots > 1)
                        {
                            //has to be function
                            if (!functions.Contains(currentNumOrFunc.ToString()))
                            {
                                throw new Exception($"Unknown function '{currentNumOrFunc}'");
                            }
                            _tokens.Push(new Operation<T>(currentNumOrFunc.ToString(), i - currentNumOrFunc.Length, CharState.Function));
                        }
                        else
                        {
                            //can be number or function
                            _tokens.Push(new Token(currentNumOrFunc.ToString(), i - currentNumOrFunc.Length, currentNumOrFuncStates));
                        }
                        currentNumOrFunc.Clear();
                        currentNumOrFuncStates = 0;
                        numberOfDots = 0;
                    }
                    else if (previousState == CharState.LeftBracket)
                    {
                        if (previousToken.HasState(CharState.RightBracket))
                        {
                            ThrowBadChar(i);
                        }
                        if (previousToken.HasState(CharState.Digit))
                        {
                            //previousToken is considered function
                            if (!functions.Contains(previousToken.Text))
                            {
                                throw new Exception($"Unknown function '{previousToken.Text}'");
                            }
                            Operation<T> o = new(previousToken.Text, previousToken.StartIdx, CharState.Function);
                            _tokens.Pop();
                            _tokens.Push(o);
                        }

                        bracktets.Push('(');
                        _tokens.Push(new Token("(", i - 1, (int)CharState.LeftBracket));
                    }
                    else if (previousState == CharState.Operator)
                    {
                        if (previousToken.HasState(CharState.Operator))
                        {
                            ThrowBadChar(i);
                        }
                        if (previousToken.HasState(CharState.Function))
                        {
                            ThrowBadChar(i);
                        }
                        //previousToken is not function
                        if (previousToken.HasState(CharState.Digit))
                        {
                            //previousToken is number 
                            Number<T> n = new(previousToken.Text, previousToken.StartIdx);
                            _tokens.Pop();
                            _tokens.Push(n);

                        }
                        _tokens.Push(new Operation<T>(expr[i - 1], i - 1, CharState.Operator));
                    }
                    else if (previousState == CharState.RightBracket)
                    {
                        if (bracktets.Count < 1)
                        {
                            ThrowBadChar(i);
                        }
                        if (previousToken.HasState(CharState.Function))
                        {
                            ThrowBadChar(i);
                        }
                        if (previousToken.HasState(CharState.Digit))
                        {
                            //previousToken is number
                            _tokens.Pop();
                            _tokens.Push(new Number<T>(previousToken.Text, previousToken.StartIdx));
                        }
                        _tokens.Push(new Token(")", i - 1, (int)CharState.RightBracket));
                        bracktets.Pop();
                    }
                }
                previousState = currentState;
            }

            var lastToken = _tokens.Peek();

            if (bracktets.Count != 0 || lastToken.HasState(CharState.Operator) || lastToken.HasState(CharState.Function))
            {
                throw new Exception("Unexpected end of expression.");
            }

            if (lastToken.HasState(CharState.Digit))
            {
                _tokens.Pop();
                _tokens.Push(new Number<T>(lastToken.Text, lastToken.StartIdx));
            }

            return _tokens.Reverse().Skip(1).ToArray();
        }

        private static CharState GetState(char ch)
        {
            if (char.IsDigit(ch))
            {
                return CharState.Digit;
            }
            else if (ch == '.')
            {
                return CharState.Dot;
            }
            else if (char.IsLetter(ch))
            {
                return CharState.Function;
            }
            else if (char.IsWhiteSpace(ch))
            {
                return CharState.Whitespace;
            }
            else if (ch == '(')
            {
                return CharState.LeftBracket;
            }
            else if (ch == ')')
            {
                return CharState.RightBracket;
            }
            else if (operators.Contains(ch.ToString()))
            {
                return CharState.Operator;
            }
            else
            {
                return CharState.NotAllowed;
            }
        }

        private static void ThrowBadChar(int i)
        {
            throw new Exception($"Unexpected character at {i}.");
        }

        private static bool TokenHasEnded(CharState prev, CharState current)
        {
            if (prev == CharState.Start)
            {
                return false;
            }
            if (prev == CharState.Whitespace)
            {
                return false;
            }
            if (prev == CharState.LeftBracket && current == CharState.LeftBracket)
            {
                return true;
            }
            if (prev == CharState.RightBracket && current == CharState.RightBracket)
            {
                return true;
            }
            if (prev == CharState.Digit && (current == CharState.Function || current == CharState.Dot))
            {
                return false;
            }
            if (prev == CharState.Function && (current == CharState.Digit || current == CharState.Dot))
            {
                return false;
            }
            if (prev == CharState.Dot && (current == CharState.Function || current == CharState.Digit))
            {
                return false;
            }
            return prev != current;
        }
    }

    public enum CharState
    {
        Start = 0,
        Digit = 1,
        Operator = 2,
        LeftBracket = 4,
        RightBracket = 8,
        Function = 16,
        NotAllowed = 32,
        Whitespace = 64,
        End = 128,
        Dot = 256
    }

    [DebuggerDisplay("{Text}")]
    public class Token
    {
        public string Text { get; set; }

        public ushort StartIdx { get; set; }

        public int CharStates { get; set; }

        public Token(string text, ushort startIdx)
        {
            Text = text;
            StartIdx = startIdx;
        }

        public Token(string text, int startIdx, int charStates)
        {
            Text = text;
            StartIdx = (ushort)startIdx;
            CharStates = charStates;
        }

        public bool HasState(CharState state)
        {
            return (CharStates & (int)state) == (int)state;
        }
    }

    public abstract class Node<T> : Token where T : INumericOps<T>, INumber
    {
        public Node(string text, int startIdx, CharState charState) : base(text, (ushort)startIdx, (int)charState) { }
        public abstract Operation<T> Parent { get; set; }
        public abstract string ToJson();
        public abstract T Evalute();
    }

    public class Operation<T> : Node<T> where T : INumericOps<T>, INumber
    {
        private Node<T> _parent;
        private Node<T> _left;
        private Node<T> _right;

        public Operation(string symbol, int startIdx, CharState charState) : base(symbol, startIdx, charState)
        {
        }

        public Operation(char symbol, int startIdx, CharState charState) : base(symbol.ToString(), startIdx, charState)
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

        public Number(string number, int startIdx) : base(number, startIdx, CharState.Digit)
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
            gcd = gcd > 1 ? gcd : 1;
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
            g = g > 1 ? g : 1;
            var e = ((rvalue._denominator / g) * _enumerator) + ((_denominator / g) * rvalue._enumerator);
            var d = (_denominator / g) * rvalue._denominator;

            return new DecimalFractionNumeric(e, d);
        }

        public DecimalFractionNumeric Divide(DecimalFractionNumeric rvalue)
        {
            var gcd1 = Gcd(_enumerator, rvalue._enumerator);
            gcd1 = gcd1 > 1 ? gcd1 : 1;
            var gcd2 = Gcd(_denominator, rvalue._enumerator);
            gcd2 = gcd2 > 1 ? gcd2 : 1;
            var e = (_enumerator / gcd1) * (rvalue._denominator / gcd2);
            var d = (_denominator / gcd2) * (rvalue._enumerator / gcd1);
            return new DecimalFractionNumeric(e, d);
        }

        public DecimalFractionNumeric Multiply(DecimalFractionNumeric rvalue)
        {
            var gcd1 = Gcd(_enumerator, rvalue._denominator);
            gcd1 = gcd1 > 1 ? gcd1 : 1;
            var gcd2 = Gcd(_denominator, rvalue._enumerator);
            gcd2 = gcd2 > 1 ? gcd2 : 1;
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
            return Value.ToString(CultureInfo.InvariantCulture);
        }

        public string ToString(int decimalPlaces)
        {
            return decimal.Round(Value, decimalPlaces).ToString(CultureInfo.InvariantCulture);
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
            if (decimal.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal enumerator))
            {
                _enumerator = enumerator;
            }
            else
            {
                throw new ArgumentException($"Couldn't turn value '{value}' into decimal number.", nameof(value));
            }
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
            return _value.ToString(CultureInfo.InvariantCulture);
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

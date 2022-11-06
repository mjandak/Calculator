using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathExpressionParser.Interfaces
{
    /// <summary>
    /// Math operations on an instance of T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface INumericOps<T> where T : INumber
    {
        public T Add(T rvalue);
        public T Subtract(T rvalue);
        public T Multiply(T rvalue);
        public T Divide(T rvalue);
        public T Power(T exponent);
        public T Sin();
        public T Cos();
        public T Tan();
    }

    public interface INumber
    {
        public void SetValue(string value);
    }

    //public interface INumbers<T>
    //{
    //    public T GetNumber(string number);
    //}
}

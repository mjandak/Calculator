using CalcularorInterfaces;
using MathExpressionParser;

namespace MathExpressionProvider
{
    public class MathExprProvider : IMathExprProvider
    {
        public string Evaluate(string expression)
        {
            Node<DecimalFractionNumeric> tree = Parser<DecimalFractionNumeric>.ParseExpr(expression);
            return tree.Evalute().ToString(20);
        }
    }
}

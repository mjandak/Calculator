using CalcularorInterfaces;
using MathExpressionParser_v2;

namespace MathExpressionParser
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

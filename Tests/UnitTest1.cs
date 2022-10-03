using MathExpressionParser;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void CheckValidity()
        {
            var r = Parser<DecimalFractionNumeric>.CheckValidity2("1 + 2/ 3-123 + sin(123)/cos(123 *   123 ) ");
            r = Parser<DecimalFractionNumeric>.CheckValidity2("729^(1/2)");
            r = Parser<DecimalFractionNumeric>.CheckValidity2("-2+9-(-8) ");
            r = Parser<DecimalFractionNumeric>.CheckValidity2("(-2) +9- (-8) ");
            r = Parser<DecimalFractionNumeric>.CheckValidity2("8244.0735992+2.36.25");
            //Assert.Pass();
        }

        [Test]
        public void ParserTest()
        {
            Symbol[] r = Parser<DecimalFractionNumeric>.CheckValidity2("1 + 2");
            Node<DecimalFractionNumeric> r2 = Parser<DecimalFractionNumeric>.ParseExpr(r);
            DecimalFractionNumeric r3 = r2.Evalute();

            r = Parser<DecimalFractionNumeric>.CheckValidity2(" 1 +2  ");
            r2 = Parser<DecimalFractionNumeric>.ParseExpr(r);
            r3 = r2.Evalute();

            r = Parser<DecimalFractionNumeric>.CheckValidity2("-1+2");
            r2 = Parser<DecimalFractionNumeric>.ParseExpr(r);
            r3 = r2.Evalute();

            r = Parser<DecimalFractionNumeric>.CheckValidity2("-1 + 2");
            r2 = Parser<DecimalFractionNumeric>.ParseExpr(r);
            r3 = r2.Evalute();

            r = Parser<DecimalFractionNumeric>.CheckValidity2("  -3+ 2 ");
            r2 = Parser<DecimalFractionNumeric>.ParseExpr(r);
            r3 = r2.Evalute();

            r = Parser<DecimalFractionNumeric>.CheckValidity2("1+(2/3)");
            r2 = Parser<DecimalFractionNumeric>.ParseExpr(r);
            r3 = r2.Evalute();

            r = Parser<DecimalFractionNumeric>.CheckValidity2("1+(2/3)-((4*5)-(6^7))");
            r2 = Parser<DecimalFractionNumeric>.ParseExpr(r);
            r3 = r2.Evalute();

            r = Parser<DecimalFractionNumeric>.CheckValidity2(" 1+ (2  /3)- ((4 *5)-(6 ^7   )) ");
            r2 = Parser<DecimalFractionNumeric>.ParseExpr(r);
            r3 = r2.Evalute();

            r = Parser<DecimalFractionNumeric>.CheckValidity2("sin(1)");
            r2 = Parser<DecimalFractionNumeric>.ParseExpr(r);
            r3 = r2.Evalute();

            r = Parser<DecimalFractionNumeric>.CheckValidity2("sin(1) + cos(2)");
            r2 = Parser<DecimalFractionNumeric>.ParseExpr(r);
            r3 = r2.Evalute();

            r = Parser<DecimalFractionNumeric>.CheckValidity2("sin(-1+2) + cos(2-(3 *4) ) ");
            r2 = Parser<DecimalFractionNumeric>.ParseExpr(r);
            r3 = r2.Evalute();

            //1.1214279939932775905606310107673884381681241714752190055239491504...
            r = Parser<DecimalFractionNumeric>.CheckValidity2("sin(-1+2) + cos(2-(sin(2^(-1/3))) ) ");
            r2 = Parser<DecimalFractionNumeric>.ParseExpr(r);
            r3 = r2.Evalute();

            //Assert.Pass();
        }
    }
}
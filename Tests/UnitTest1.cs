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
        public void Test1()
        {
            var r = Parser<DecimalFractionNumeric>.CheckValidity2("1 + 2/ 3-123 + sin(123)/cos(123 *   123 ) ");
            r = Parser<DecimalFractionNumeric>.CheckValidity2("729^(1/2)");
            r = Parser<DecimalFractionNumeric>.CheckValidity2("-2+9-(-8) ");
            r = Parser<DecimalFractionNumeric>.CheckValidity2("(-2) +9- (-8) ");
            r = Parser<DecimalFractionNumeric>.CheckValidity2("8244.0735992+2.36.25");
            //Assert.Pass();
        }
    }
}
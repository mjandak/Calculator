using MathExpressionParser;
using NUnit.Framework;

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
            var r = Parser<DecimalFractionNumeric>.CheckValidity("1 + 2/ 3-123 + sin(123)/cos(123 *   123 ) ");
            r = Parser<DecimalFractionNumeric>.CheckValidity("729^(1/2)");
            r = Parser<DecimalFractionNumeric>.CheckValidity("-2+9-(-8) ");
            r = Parser<DecimalFractionNumeric>.CheckValidity("(-2) +9- (-8) ");
            r = Parser<DecimalFractionNumeric>.CheckValidity("234.56*23.+123");
            r = Parser<DecimalFractionNumeric>.CheckValidity("234.56*.23+123");

            r = Parser<DecimalFractionNumeric>.CheckValidity("234.56*23 5+123");
            Assert.Throws<Exception>(() => Parser<DecimalFractionNumeric>.CheckValidity("8244.0735992+2.36.25")); //unexpected at 17
            Assert.Throws<Exception>(() => Parser<DecimalFractionNumeric>.CheckValidity(" 7+ (5 "));
            Assert.Throws<Exception>(() => Parser<DecimalFractionNumeric>.CheckValidity(" 7  + ((5* 3  )"));
            Assert.Throws<Exception>(() => Parser<DecimalFractionNumeric>.CheckValidity("234.56*23 5+123"));
            //Assert.Pass();
        }

        [Test]
        public void ParserTest()
        {
            Node<DecimalFractionNumeric> r2 = Parser<DecimalFractionNumeric>.ParseExpr("1 + 2");
            DecimalFractionNumeric r3 = r2.Evalute();

            //r = Parser<DecimalFractionNumeric>.CheckValidity2(" 1 +2  ");
            r2 = Parser<DecimalFractionNumeric>.ParseExpr(" 1 +2  ");
            r3 = r2.Evalute();

            //r = Parser<DecimalFractionNumeric>.CheckValidity2("-1+2");
            r2 = Parser<DecimalFractionNumeric>.ParseExpr("-1+2");
            r3 = r2.Evalute();

            //r = Parser<DecimalFractionNumeric>.CheckValidity2("-1 + 2");
            r2 = Parser<DecimalFractionNumeric>.ParseExpr("-1 + 2");
            r3 = r2.Evalute();

            //r = Parser<DecimalFractionNumeric>.CheckValidity2("  -3+ 2 ");
            r2 = Parser<DecimalFractionNumeric>.ParseExpr("  -3+ 2 ");
            r3 = r2.Evalute();

            //r = Parser<DecimalFractionNumeric>.CheckValidity2("1+(2/3)");
            r2 = Parser<DecimalFractionNumeric>.ParseExpr("1+(2/3)");
            r3 = r2.Evalute();

            //r = Parser<DecimalFractionNumeric>.CheckValidity2("1+(2/3)-((4*5)-(6^7))");
            r2 = Parser<DecimalFractionNumeric>.ParseExpr("1+(2/3)-((4*5)-(6^7))");
            r3 = r2.Evalute();

            //r = Parser<DecimalFractionNumeric>.CheckValidity2(" 1+ (2  /3)- ((4 *5)-(6 ^7   )) ");
            r2 = Parser<DecimalFractionNumeric>.ParseExpr(" 1+ (2  /3)- ((4 *5)-(6 ^7   )) ");
            r3 = r2.Evalute();

            //r = Parser<DecimalFractionNumeric>.CheckValidity2("sin(1)");
            r2 = Parser<DecimalFractionNumeric>.ParseExpr("sin(1)");
            r3 = r2.Evalute();

            //r = Parser<DecimalFractionNumeric>.CheckValidity2("sin(1) + cos(2)");
            r2 = Parser<DecimalFractionNumeric>.ParseExpr("sin(1) + cos(2)");
            r3 = r2.Evalute();

            //r = Parser<DecimalFractionNumeric>.CheckValidity2("sin(-1+2) + cos(2-(3 *4) ) ");
            r2 = Parser<DecimalFractionNumeric>.ParseExpr("sin(-1+2) + cos(2-(3 *4) ) ");
            r3 = r2.Evalute();

            //1.1214279939932775905606310107673884381681241714752190055239491504...
            //r = Parser<DecimalFractionNumeric>.CheckValidity2("sin(-1+2) + cos(2-(sin(2^(-1/3))) ) ");
            r2 = Parser<DecimalFractionNumeric>.ParseExpr("sin(-1+2) + cos(2-(sin(2^(-1/3))) ) ");
            r3 = r2.Evalute();

            //Assert.Pass();
        }

        [Test]
        public void DecimalFractionNumericTest()
        {
            Node<DecimalFractionNumeric> r;

            r = Parser<DecimalFractionNumeric>.ParseExpr("26409387504754779197847983445/79228162514264337593543950335*79228162514264337593543950335");
            DecimalFractionNumeric d = r.Evalute();
            Assert.That<decimal>(d, Is.EqualTo(26409387504754779197847983445m));

            r = Parser<DecimalFractionNumeric>.ParseExpr("79228162514264337593543950335*26409387504754779197847983445/79228162514264337593543950335");
            d = r.Evalute();
            Assert.That<decimal>(d, Is.EqualTo(26409387504754779197847983445m));

            r = Parser<DecimalFractionNumeric>.ParseExpr("2/3*79228162514264337593543950335");
            d = r.Evalute();
            Assert.That<decimal>(d, Is.EqualTo(52818775009509558395695966890m));

            r = Parser<DecimalFractionNumeric>.ParseExpr("2/3*999999999999999");
            d = r.Evalute();
            Assert.That<decimal>(d, Is.EqualTo(666666666666666m));

            r = Parser<DecimalFractionNumeric>.ParseExpr("111*22+33");
            Assert.That<decimal>(r.Evalute(), Is.EqualTo(2475m));

            r = Parser<DecimalFractionNumeric>.ParseExpr("111*22*33.7");
            Assert.That<decimal>(r.Evalute(), Is.EqualTo(82295.4m));

            r = Parser<DecimalFractionNumeric>.ParseExpr("3567735.101+0.01");
            Assert.That<decimal>(r.Evalute(), Is.EqualTo(3567735.111m));

            r = Parser<DecimalFractionNumeric>.ParseExpr("1/3*3");
            Assert.That<decimal>(r.Evalute(), Is.EqualTo(1m));

            r = Parser<DecimalFractionNumeric>.ParseExpr("(1/3)*3");
            Assert.That<decimal>(r.Evalute(), Is.EqualTo(1m));

            r = Parser<DecimalFractionNumeric>.ParseExpr("3*1/3");
            Assert.That<decimal>(r.Evalute(), Is.EqualTo(1m));

            r = Parser<DecimalFractionNumeric>.ParseExpr("3*(1/3)");
            Assert.That<decimal>(r.Evalute(), Is.EqualTo(1m));

            r = Parser<DecimalFractionNumeric>.ParseExpr("1/3+1/3+1/3");
            Assert.That<decimal>(r.Evalute(), Is.EqualTo(1m));

            r = Parser<DecimalFractionNumeric>.ParseExpr("26409387504754779197847983445/79228162514264337593543950335+26409387504754779197847983445/79228162514264337593543950335+26409387504754779197847983445/79228162514264337593543950335");
            Assert.That<decimal>(r.Evalute(), Is.EqualTo(1m));

            r = Parser<DecimalFractionNumeric>.ParseExpr("41.041041041/123.123123123*369.369369369/123.123123123");
            Assert.That<decimal>(r.Evalute(), Is.EqualTo(1m));

            r = Parser<DecimalFractionNumeric>.ParseExpr("41.041041041/123.123123123+41.041041041/123.123123123+41.041041041/123.123123123");
            Assert.That<decimal>(r.Evalute(), Is.EqualTo(1m));

            r = Parser<DecimalFractionNumeric>.ParseExpr("111*22*33.7-444*5");
            Assert.That<decimal>(r.Evalute(), Is.EqualTo(80075.4m));

            r = Parser<DecimalFractionNumeric>.ParseExpr("(111*22*33.7-444*5)");
            Assert.That<decimal>(r.Evalute(), Is.EqualTo(80075.4m));

            r = Parser<DecimalFractionNumeric>.ParseExpr("((111*22*33.7-444*5))");
            Assert.That<decimal>(r.Evalute(), Is.EqualTo(80075.4m));

            r = Parser<DecimalFractionNumeric>.ParseExpr("111*22*(33.7-444)*5");
            Assert.That<decimal>(r.Evalute(), Is.EqualTo(-5009763m));

            r = Parser<DecimalFractionNumeric>.ParseExpr("4/3*0.25");
            Assert.That<decimal>(r.Evalute(), Is.EqualTo(1m / 3m));

            r = Parser<DecimalFractionNumeric>.ParseExpr("2*(8/6*0.25+(4*10.2880380473065/123.456456567678)+123.456456567678/370.369369703034)");
            Assert.That<decimal>(r.Evalute(), Is.EqualTo(2m));

            r = Parser<DecimalFractionNumeric>.ParseExpr("123 +(89.3*  9.1)* 4.1^5");
            Assert.That<decimal>(r.Evalute(), Is.EqualTo(941605.2461863m));

            r = Parser<DecimalFractionNumeric>.ParseExpr("234.56*23.+123");
            Assert.That<decimal>(r.Evalute(), Is.EqualTo(5517.88m));

            r = Parser<DecimalFractionNumeric>.ParseExpr("234.56*.23+123");
            Assert.That<decimal>(r.Evalute(), Is.EqualTo(176.9488m));
        }
    }
}
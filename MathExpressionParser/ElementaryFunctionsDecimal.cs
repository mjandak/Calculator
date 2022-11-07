using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathExpressionParser
{
    public class ElementaryFunctionsDecimal
    {
        public const decimal E = 2.7182818284590452353602874713526624977572470936999595749669676277m;

        public const decimal LN_10 = 2.3025850929940456840179914546843642076011014886287729760333279009m;
        public const decimal LN_4 = 1.3862943611198906188344642429163531361510002687205105082413600189m;
        public const decimal LN_2 = 0.6931471805599453094172321214581765680755001343602552541206800094m;
        public const decimal LN_SQRT_10 = 1.1512925464970228420089957273421821038005507443143864880166639504m;

        public const decimal SQRT_10 = 3.1622776601683793319988935444327185337195551393252168268575048527m;

        public const decimal PI_DOUBLE = 6.2831853071795864769252867665590057683943387987502116419498891846m;
        public const decimal PI_3HALF = 4.7123889803846898576939650749192543262957540990626587314624168884m;
        public const decimal PI = 3.1415926535897932384626433832795028841971693993751058209749445923m;
        public const decimal PI_3QUARTER = 2.3561944901923449288469825374596271631478770495313293657312084442m;
        public const decimal PI_HALF = 1.5707963267948966192313216916397514420985846996875529104874722961m;
        public const decimal PI_QUARTER = 0.7853981633974483096156608458198757210492923498437764552437361480m;

        public static decimal Ln_reduced(decimal x)
        {
            //ln(a*2^n) = ln(a) + n*ln(2)
            int n = 0;
            while (x > 4 / 3)
            {
                x /= 4;
                n += 2;
            }

            while (x <= 4 / 3)
            {
                x *= 2;
                n -= 1;
            }

            x /= 2;
            n += 1;

            return Ln(x) + n * 0.69314718055994530941723212145818m;
        }

        public static decimal Ln(decimal x)
        {
            if (x <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            //ln(x) = 2*(((x-1)/(x+1)) + (1/3)*((x-1)/(x+1))^3 + (1/5)*((x-1)/(x+1))^5 + ...)

            const decimal numOfTerms = 40;
            const decimal loopMax = ((numOfTerms - 1) * 2) + 3;

            decimal expDiff = ((x - 1) / (x + 1)) * ((x - 1) / (x + 1));
            decimal expCached = (x - 1) / (x + 1); //first term
            decimal result = expCached;

            for (decimal i = 3; i < loopMax; i += 2)
            {
                expCached *= expDiff;
                result += 1m / i * expCached;
            }

            return 2m * result;
        }

        public static decimal Log10(decimal x)
        {
            //log(x) = ln(x) * log(e)
            return Ln_reduced(x) * 0.4342944819032518276511289189166050822943970058036665661144537831m;
        }

        public static decimal exp_taylor(decimal x)
        {
            //Adapted from https://www.pseudorandom.com/implementing-exp

            /*
             * Evaluates f(x) = e^x for any x in the interval [-709, 709].
             * If x < -709 or x > 709, raises an assertion error. Implemented
             * using the truncated Taylor series of e^x with ceil(|x| * e) * 12
             * terms. Achieves at least 14 and at most 16 digits of precision
             * over the entire interval.
             * Performance - There are exactly 36 * ceil(|x| * e) + 5
             * operations; 69,413 in the worst case (x = 709 or -709):
             * - (12 * ceil(|x| * e)) + 2 multiplications
             * - (12 * ceil(|x| * e)) + 1 divisions
             * - (12 * ceil(|x| * e)) additions
             * - 1 rounding
             * - 1 absolute value
             * Accuracy - Over a sample of 10,000 linearly spaced points in
             * [-709, 709] we have the following error statistics:
             * - Max relative error = 8.39803e-15
             * - Min relative error = 0.0
             * - Avg relative error = 0.0
             * - Med relative error = 1.90746e-15
             * - Var relative error = 0.0
             * - 0.88 percent of the values have less than 15 digits of precision
             * Args:
             *      - x: (double float) power of e to evaluate
             * Returns:
             *      - (double float) approximation of e^x in double precision
             */

            // Check that x is a valid input.
            //assert(-709 <= x && x <= 709);

            // When x = 0 we already know e^x = 1.
            if (x == 0) return 1;

            // Normalize x to a non-negative value to take advantage of
            // reciprocal symmetry. But keep track of the original sign
            // in case we need to return the reciprocal of e^x later.
            decimal x0 = Math.Abs(x);

            // First term of Taylor expansion of e^x at a = 0 is 1.
            // tn is the variable we we will return for e^x, and its
            // value at any time is the sum of all currently evaluated
            // Taylor terms thus far.
            decimal tn = 1;

            // Chose a truncation point for the Taylor series using the
            // heuristic bound 12 * ceil(|x| e), then work down from there
            // using Horner's method.
            int n = (int)decimal.Ceiling(x0 * E) * 24; //* 12;
            for (int i = n; i > 0; i--)
            {
                tn = tn * (x0 / i) + 1.0m;
            }

            // If the original input x is less than 0, we want the reciprocal of the e^x we calculated.
            if (x < 0) tn = 1 / tn;
            return tn;
        }

        public static decimal exp_taylor_v2(decimal x)
        {
            int sign = Math.Sign(x);

            decimal sum = 1;
            decimal product = 1;
            int i = 1;
            x = sign * x; //make x positive
            while (true)
            {
                decimal f = x / i; //divide first to avoid OverflowException too early
                product *= f;
                if (product == 0m) break;
                sum += product;
                i++;
            }

            //e^(-x) = 1/(e^x)
            if (sign < 0)
            {
                return 1 / sum;
            }

            return sum;
        }

        public static decimal Pow(decimal a, decimal b)
        {
            if (b == decimal.Zero) return 1;
            if (b == decimal.One) return a;

            //make exponent positive
            if (b < 0)
            {
                //a^(-b) = (1/a)^b
                a = 1 / a;
                b = -b;
            }

            decimal q = decimal.Truncate(b);
            decimal r = b - q;
            decimal a_q = Pow(a, (uint)q);
            if (r == 0)
            {
                return a_q;
            }
            else
            {
                decimal a_r = exp_taylor(r * Ln_reduced(a));
                return a_q * a_r;
            }
        }

        public static decimal Pow(decimal a, uint b)
        {
            if (a == 0) return 0;
            if (a == 1) return 1;
            if (b == 0) return 1;
            if (b == 1) return a;

            decimal result = a;
            int exp = 1;

            while (true)
            {
                exp *= 2;
                if (exp > b) break;
                result *= result;
            }

            return result * Pow(a, (uint)(b - exp / 2));
        }

        public static decimal PowOf2(ushort k)
        {
            if (k > 95) throw new ArgumentOutOfRangeException();

            int i1 = 0;
            int i2 = 0;
            int i3 = 0;
            if (k < 32)
            {
                i1 = 1;
                i1 <<= k;
            }
            else if (k < 64)
            {
                i2 = 1;
                i2 <<= k - 32;
            }
            else
            {
                i3 = 1;
                i3 <<= k - 64;
            }
            return new decimal(i1, i2, i3, false, 0);
        }

        public static decimal Cos_taylor(decimal x)
        {
            decimal prev_term = 1; //first term
            int sign = -1; //sign of second term
            decimal sum = 1; //first term
            int number = 2;

            while (true)
            {
                decimal current_term = prev_term * (x / (number - 1)) * (x / number);
                if (current_term == 0) break;
                sum = sum + sign * current_term;
                prev_term = current_term;
                sign = -1 * sign;
                number += 2;
            }

            return sum;
        }

        public static decimal Cos_taylor_reduced(decimal x)
        {
            if (x < 0)
            {
                return Cos_taylor_reduced(-x);
            }

            decimal x1 = x % PI_DOUBLE;

            if (x1 > PI)
            {
                return -Cos_taylor(x1 - PI);
            }

            return Cos_taylor(x1);
        }

        public static decimal Sin_taylor(decimal x)
        {
            if (x == 0) return 0;
            if (x < 0) return -Sin_taylor(-x);
            decimal sum = x;
            int sign = -1;
            decimal cached = x;
            ushort i = 1;
            while (true)
            {
                cached = cached * (x / (i + 1)) * (x / (i + 2));
                if (cached == 0)
                {
                    return sum;
                }
                sum += sign * cached;
                sign = -sign;
                i += 2;
            }
        }

        public static decimal Sin_taylor_reduced(decimal x)
        {
            if (x < 0)
                return -Sin_taylor_reduced(-x);
            if (x == 0)
                return 0;
            var r = x % PI_DOUBLE; //0 <= r < PI_DOUBLE
            decimal x1;
            if (r > PI)
            {
                x1 = r - PI_DOUBLE;
                decimal q = Math.Ceiling(x / PI_DOUBLE);
                Debug.Assert(q * PI_DOUBLE + x1 == x);
            }
            else if (r < PI)
            {
                x1 = r;
            }
            else
            {
                return 0;
                //throw new Exception();
            }

            var x1abs = Math.Abs(x1);
            var x1sign = Math.Sign(x1);

            if (x1abs > PI_QUARTER && x1abs < PI_3QUARTER)
            {
                return x1sign * Cos_taylor(x1abs - PI_HALF); //sin(x) = cos(x-pi/2)
            }
            else if (x1abs > PI_3QUARTER && x1abs < PI)
            {
                return x1sign * -Sin_taylor(x1abs - PI);
            }
            return x1sign * Sin_taylor(x1abs);
        }

        public static decimal Tan(decimal x)
        {
            if (x < 0)
            {
                return -Tan(-x);
            }
            //decimal q = x / PI;
            decimal r = x % PI;

            if (r == 0)
            {
                return 0;
            }

            if (r > PI_HALF)
            {
                decimal s = Sin_taylor(r - PI);
                decimal c = Cos_taylor(r - PI);
                return s / c;
            }
            else if (r < PI_HALF)
            {
                return Sin_taylor(r) / Cos_taylor(r);
            }
            else
            {
                Debug.Assert(r == PI_HALF);
                throw new InvalidOperationException("tanget not defined");
            }
        }
    }
}

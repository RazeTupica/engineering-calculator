using System;

namespace EngineeringCalculator
{
    public static class CalculatorEngine
    {
        public const double PI = Math.PI;
        public const double E = Math.E;

        // Основные операции
        public static double Add(double a, double b) => a + b;
        public static double Subtract(double a, double b) => a - b;
        public static double Multiply(double a, double b) => a * b;
        public static double Divide(double a, double b)
        {
            if (Math.Abs(b) < 1e-15)
                throw new DivideByZeroException("Деление на ноль невозможно.");
            return a / b;
        }

        // Инженерные функции
        public static double Power(double x, double y) => Math.Pow(x, y);
        public static double Square(double x) => x * x;
        public static double Sqrt(double x)
        {
            if (x < 0)
                throw new ArgumentException("Корень из отрицательного числа невозможен.");
            return Math.Sqrt(x);
        }
        public static double Reciprocal(double x)
        {
            if (Math.Abs(x) < 1e-15)
                throw new DivideByZeroException("Обратное число к нулю невозможно.");
            return 1 / x;
        }
        public static double Modulus(double x) => Math.Abs(x);
        public static double Percentage(double value, double percent) => (value * percent) / 100;

        // Тригонометрия (работает с градусами/радианами)
        private static double ToRadians(double degrees) => degrees * PI / 180;
        public static double Sin(double x, bool inDegrees = true)
            => Math.Sin(inDegrees ? ToRadians(x) : x);
        public static double Cos(double x, bool inDegrees = true)
            => Math.Cos(inDegrees ? ToRadians(x) : x);
        public static double Tan(double x, bool inDegrees = true)
        {
            double rad = inDegrees ? ToRadians(x) : x;
            if (Math.Abs(Math.Cos(rad)) < 1e-15)
                throw new ArgumentException("Тангенс не определен для данного угла.");
            return Math.Tan(rad);
        }

        // Обратные тригонометрические функции
        public static double Asin(double x, bool inDegrees = true)
        {
            if (x < -1 || x > 1)
                throw new ArgumentException("Арксинус определен для значений от -1 до 1.");
            double result = Math.Asin(x);
            return inDegrees ? result * 180 / PI : result;
        }

        public static double Acos(double x, bool inDegrees = true)
        {
            if (x < -1 || x > 1)
                throw new ArgumentException("Арккосинус определен для значений от -1 до 1.");
            double result = Math.Acos(x);
            return inDegrees ? result * 180 / PI : result;
        }

        // Логарифмы
        public static double Ln(double x)
        {
            if (x <= 0)
                throw new ArgumentException("Логарифм определен только для положительных чисел.");
            return Math.Log(x);
        }

        public static double Log10(double x)
        {
            if (x <= 0)
                throw new ArgumentException("Логарифм определен только для положительных чисел.");
            return Math.Log10(x);
        }

        // Экспоненциальные функции
        public static double Exp(double x) => Math.Exp(x);
        public static double Power10(double x) => Math.Pow(10, x);

        // Факториал
        public static double Factorial(double x)
        {
            if (x < 0 || x != Math.Floor(x))
                throw new ArgumentException("Факториал определен только для целых неотрицательных чисел.");

            if (x == 0 || x == 1) return 1;

            double result = 1;
            for (int i = 2; i <= (int)x; i++)
                result *= i;
            return result;
        }
        // Котангенс
        public static double Cot(double x, bool inDegrees = true)
        {
            double rad = inDegrees ? ToRadians(x) : x;
            double tan = Math.Tan(rad);
            if (Math.Abs(tan) < 1e-15)
                throw new ArgumentException("Котангенс не определен для данного угла.");
            return 1 / tan;
        }

        // Арктангенс
        public static double Atan(double x, bool inDegrees = true)
        {
            double result = Math.Atan(x);
            return inDegrees ? result * 180 / PI : result;
        }

        // Арккотангенс
        public static double Acot(double x, bool inDegrees = true)
        {
            double result = Math.Atan(1 / x);
            return inDegrees ? result * 180 / PI : result;
        }

        // Гиперболические функции
        public static double Sinh(double x) => Math.Sinh(x);
        public static double Cosh(double x) => Math.Cosh(x);
        public static double Tanh(double x) => Math.Tanh(x);

        // Секанс и косеканс (если нужны)
        public static double Sec(double x, bool inDegrees = true)
        {
            double rad = inDegrees ? ToRadians(x) : x;
            double cos = Math.Cos(rad);
            if (Math.Abs(cos) < 1e-15)
                throw new ArgumentException("Секанс не определен для данного угла.");
            return 1 / cos;
        }

        public static double Csc(double x, bool inDegrees = true)
        {
            double rad = inDegrees ? ToRadians(x) : x;
            double sin = Math.Sin(rad);
            if (Math.Abs(sin) < 1e-15)
                throw new ArgumentException("Косеканс не определен для данного угла.");
            return 1 / sin;
        }
    }
}
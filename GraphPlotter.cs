using System;
using System.Globalization;

namespace EngineeringCalculator
{
    public static class GraphPlotter
    {
        public static (double[] xValues, double[] yValues) Plot(
            string function,
            double xMin,
            double xMax,
            int points = 200)
        {
            if (xMin >= xMax)
                throw new ArgumentException("Минимальное значение X должно быть меньше максимального.");

            var xValues = new double[points];
            var yValues = new double[points];
            var step = (xMax - xMin) / (points - 1);
            var parser = new ExpressionParser();

            for (int i = 0; i < points; i++)
            {
                double x = xMin + i * step;
                xValues[i] = x;

                try
                {
                    // Заменяем x в выражении на значение
                    string expression = function.Replace("x", x.ToString(CultureInfo.InvariantCulture))
                                                .Replace("X", x.ToString(CultureInfo.InvariantCulture))
                                                .Replace(",", ".");
                    yValues[i] = parser.Evaluate(expression);
                }
                catch
                {
                    yValues[i] = double.NaN;
                }
            }

            return (xValues, yValues);
        }
    }
}
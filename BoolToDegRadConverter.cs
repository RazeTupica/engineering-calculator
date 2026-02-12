using System;
using System.Globalization;
using System.Windows.Data;

namespace EngineeringCalculator
{
    public class BoolToDegRadConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isDegrees)
                return isDegrees ? "Градусы" : "Радианы";
            return "Градусы";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string mode)
                return mode == "Градусы";
            return true;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FikusIn.UIConverters
{
    public class IsGreaterThanConverter : IValueConverter
    {
        public static readonly IValueConverter Instance = new IsGreaterThanConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool) && targetType != typeof(object))
            {
                throw new ArgumentException("Target must be a boolean");
            }

            if ((value == null) || (parameter == null))
            {
                return false;
            }

            double convertedValue;
            if (!double.TryParse(value.ToString(), out convertedValue))
            {
                throw new InvalidOperationException("The Value can not be converted to a Double");
            }

            double convertedParameter;
            if (!double.TryParse(parameter.ToString(), out convertedParameter))
            {
                throw new InvalidOperationException("The Parameter can not be converted to a Double");
            }

            return convertedValue > convertedParameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

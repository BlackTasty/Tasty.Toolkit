using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Tasty.MaterialDesign.FilePicker.Converter
{
    class StringLimitConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int maxLength = System.Convert.ToInt32(parameter);

            if (value is string text)
            {
                if (text.Length > maxLength)
                {
                    return text.Substring(0, maxLength - 3) + "...";
                }
                else
                {
                    return text;
                }
            }
            else
                return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}

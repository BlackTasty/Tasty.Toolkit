using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace Tasty.MaterialDesign.FilePicker.Converter
{
    class IntToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum)
            {
                var underlyingType = Enum.GetUnderlyingType(value.GetType());
                value = System.Convert.ChangeType(value, underlyingType);
            }

            string parameterParsed = parameter?.ToString() ?? "";
            if (!string.IsNullOrEmpty(parameterParsed))
            {
                string[] parameterValues = parameterParsed.Split(';');
                for (int i = 0; i < parameterValues.Length; i++)
                {
                    if (int.Parse(value?.ToString() ?? "-1") == int.Parse(parameterValues[i]))
                        return Visibility.Visible;
                }
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}

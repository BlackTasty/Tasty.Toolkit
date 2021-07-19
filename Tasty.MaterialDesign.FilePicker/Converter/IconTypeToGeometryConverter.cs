using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using Tasty.MaterialDesign.FilePicker.Core;

namespace Tasty.MaterialDesign.FilePicker.Converter
{
    public class IconTypeToGeometryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IconType iconType)
                return Geometry.Parse(Util.GetIconSvg(iconType));
            else
                return Geometry.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}

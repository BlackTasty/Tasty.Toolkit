using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Tasty.MaterialDesign.FilePicker.Core
{
    static class Util
    {
        static ResourceDictionary loadedIcons = new ResourceDictionary()
        {
            Source = new Uri("pack://application:,,,/Tasty.MaterialDesign.FilePicker;component/Resources/Icons.xaml",
                        UriKind.Absolute)
        };

        internal static BitmapImage LoadImage(string path)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                BitmapImage bmp = new BitmapImage();

                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = fs;
                    bmp.EndInit();
                }

                return bmp;
            }
            else return null;
        }

        internal static string NormalizePath(string path)
        {
            return Path.GetFullPath(new Uri(path).LocalPath)
                       .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                       .ToUpperInvariant();
        }

        internal static string GetFilePickerCachePath()
        {
            return Path.Combine(Path.GetTempPath(), AppDomain.CurrentDomain.FriendlyName, "explorer.cache");
        }

        internal static string FindLocalisedString(string targetName)
        {
            try
            {
                return (string)Application.Current.FindResource(targetName);
            }
            catch
            {
                return "NO STRING FOUND!";
            }
        }


        public static string GetIconSvg(IconType iconType)
        {
            return loadedIcons[iconType.ToString()].ToString();
        }
    }
}

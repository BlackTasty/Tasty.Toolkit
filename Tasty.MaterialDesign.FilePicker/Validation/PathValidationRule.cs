using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Tasty.MaterialDesign.FilePicker.Core;

namespace Tasty.MaterialDesign.FilePicker.Validation
{
    class PathValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Runtime)
            {
                if (value is IFilePickerEntry entry)
                {
                    return Directory.Exists(entry.Path) ?
                        ValidationResult.ValidResult :
                        new ValidationResult(false, Util.FindLocalisedString("validationError_file"));
                }
                else
                {
                    return Directory.Exists((value ?? "").ToString()) ?
                                            ValidationResult.ValidResult :
                                            new ValidationResult(false, Util.FindLocalisedString("validationError_directory"));
                }
            }
            else return ValidationResult.ValidResult;
        }
    }
}

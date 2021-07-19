using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.MaterialDesign.FilePicker.Core
{
    class NaturalFilePickerEntryComparer : IComparer<IFilePickerEntry>
    {
        public int Compare(IFilePickerEntry a, IFilePickerEntry b)
        {
            return SafeNativeMethods.StrCmpLogicalW(a.Name, b.Name);
        }
    }
}

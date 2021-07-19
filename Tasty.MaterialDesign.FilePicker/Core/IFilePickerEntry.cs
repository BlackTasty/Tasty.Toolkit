using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.MaterialDesign.FilePicker.Core
{
    public interface IFilePickerEntry
    {
        string Name { get; }

        string Path { get; }

        IconType Icon { get; }

        bool IsFile { get; }

        bool IsLocked { get; }

        bool IsImage { get; }
    }
}

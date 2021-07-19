using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Tasty.MaterialDesign.FilePicker.Core
{
    public class FilePickerClosedEventArgs : EventArgs
    {
        private string filePath;
        private string fileName;
        private MessageBoxResult dialogResult;

        public string FilePath => filePath;

        public string FileName => fileName;

        public MessageBoxResult DialogResult => dialogResult;

        public FilePickerClosedEventArgs(IFilePickerEntry filePickerEntry)
        {
            filePath = filePickerEntry?.Path;
            fileName = filePickerEntry?.Name;
            dialogResult = filePickerEntry != null ? MessageBoxResult.OK : MessageBoxResult.Cancel;
        }

        public FilePickerClosedEventArgs() : this(null) { }
    }
}

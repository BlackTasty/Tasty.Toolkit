using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Tasty.MaterialDesign.FilePicker.Core
{
    public class FileEntry : IFilePickerEntry
    {
        private FileInfo fi;

        public string Path
        {
            get => fi.FullName;
        }

        public string Name
        {
            get => fi.Name;
        }

        public bool IsFile => true;

        public bool IsLocked => false;

        public string Extension
        {
            get => fi.Extension;
        }

        public IconType Icon
        {
            get; private set;
        }

        public ImageSource Preview
        {
            get; private set;
        }

        public bool IsImage
        {
            get; private set;
        }

        public FileEntry(FileInfo fi)
        {
            this.fi = fi;

            switch (fi.Extension.ToLower())
            {
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".gif":
                case ".bmp":
                    Icon = IconType.FileImage;
                    Preview = Util.LoadImage(fi.FullName);
                    IsImage = true;
                    break;
                case ".wav":
                case ".ogg":
                case ".mp3":
                    Icon = IconType.FileMusic;
                    break;
                case ".doc":
                case ".dot":
                case ".wbk":
                case "docx":
                case ".docm":
                case ".docx":
                case ".dotm":
                case ".docb":
                    Icon = IconType.FileWord;
                    break;
                case ".xls":
                case ".xlt":
                case ".xlm":
                case ".xlsx":
                case ".xlsm":
                case ".xltx":
                case ".xltm":
                case ".xlsb":
                case ".xla":
                case ".xlam":
                case ".xll":
                case ".xtw":
                    Icon = IconType.FileExcel;
                    break;
                case ".pdf":
                    Icon = IconType.FilePdf;
                    break;
                case ".ppt":
                case ".pot":
                case ".pps":
                case ".pptx":
                case ".pptm":
                case ".potx":
                case ".potm":
                case ".ppam":
                case ".ppsx":
                case ".ppsm":
                case ".sldx":
                case ".sldm":
                    Icon = IconType.FilePowerpoint;
                    break;
                case ".mp4":
                case ".avi":
                case ".flv":
                    Icon = IconType.FileVideo;
                    break;
                case ".txt":
                    Icon = IconType.FileDocument;
                    break;
                case ".zip":
                case ".rar":
                case ".tar":
                    Icon = IconType.ZipBox;
                    break;
                case "":
                case null:
                    Icon = IconType.FileQuestion;
                    break;
                default:
                    Icon = IconType.File;
                    break;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

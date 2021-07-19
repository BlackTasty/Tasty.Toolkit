using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.MaterialDesign.FilePicker.Core
{
    public class UnloadedFolderEntry
    {
        public string Path
        {
            get => DirectoryInfo.FullName;
        }

        public string Name
        {
            get => DirectoryInfo.Name;
        }

        public bool IsLocked { get; private set; }

        public DirectoryInfo DirectoryInfo { get; private set; }

        public List<string> UnloadedSubDirectories { get; private set; }
            = new List<string>();

        public UnloadedFolderEntry(DirectoryInfo di, bool isLocked)
        {
            DirectoryInfo = di;
            IsLocked = isLocked;

            if (!isLocked)
            {
                foreach (DirectoryInfo subDi in di.EnumerateDirectories())
                {
                    try
                    {
                        subDi.EnumerateDirectories();
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    UnloadedSubDirectories.Add(subDi.Name);
                }
            }
            else
            {
                UnloadedSubDirectories.Add("Admin rights required");
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.MaterialDesign.FilePicker.Core
{
    class DefaultDestinationSelectedEventArgs : EventArgs
    {
        private FolderEntry selectedFolder;

        public FolderEntry SelectedFolder => selectedFolder;

        public DefaultDestinationSelectedEventArgs(FolderEntry selectedFolder)
        {
            this.selectedFolder = selectedFolder;
        }
    }
}

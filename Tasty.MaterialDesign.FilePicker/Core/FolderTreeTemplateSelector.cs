using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace Tasty.MaterialDesign.FilePicker.Core
{
    class FolderTreeTemplateSelector : DataTemplateSelector
    {
        public DataTemplate UnloadedDirectoryTemplate { get; set; }

        public DataTemplate LoadedDirectoryTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is FolderEntry entry)
            {
                if (entry.FoldersLoaded)
                {
                    return LoadedDirectoryTemplate;
                }
                else
                {
                    return UnloadedDirectoryTemplate;
                }
            }
            else
            {
                return UnloadedDirectoryTemplate;
            }
        }
    }
}

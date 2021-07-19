using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace Tasty.MaterialDesign.FilePicker.Core
{
    class FolderContentTemplateSelector : DataTemplateSelector
    {
        DataTemplate FolderDataTemplate { get; set; }

        DataTemplate FileDataTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is FileEntry)
            {
                return FileDataTemplate;
            }
            else if (item is FolderEntry)
            {
                return FolderDataTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}

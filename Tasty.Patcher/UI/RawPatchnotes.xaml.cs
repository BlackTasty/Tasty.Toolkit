using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tasty.Patcher.Core;

namespace Tasty.Patcher.UI
{
    /// <summary>
    /// Interaction logic for RawPatchnotes.xaml
    /// </summary>
    public partial class RawPatchnotes : UserControl
    {
        private static RawPatchnotes instance;

        public static RawPatchnotes Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new RawPatchnotes();
                }

                return instance;
            }
        }

        public RawPatchnotes()
        {
            InitializeComponent();
        }
    }
}

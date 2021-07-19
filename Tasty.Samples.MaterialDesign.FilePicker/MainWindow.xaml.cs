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
using Tasty.MaterialDesign.FilePicker.Core;

namespace Tasty.Samples.MaterialDesign.FilePicker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void FilePicker_DialogClosed(object sender, FilePickerClosedEventArgs e)
        {
            if (e.DialogResult == MessageBoxResult.OK)
            {
                MessageBox.Show(
                    string.Format("In-window file picker result was \"OK\"!" +
                        "\nSelected file name: {0}" +
                        "\nSelected file path: {1}", e.FileName, e.FilePath)
                    , "OK");
            }
            else
            {
                MessageBox.Show("In-window file picker result was \"Abort\"!", "Abort");
            }
        }
    }
}

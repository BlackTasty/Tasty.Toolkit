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
using Tasty.Samples.ViewModel;

namespace Tasty.Samples.Controls
{
    /// <summary>
    /// Interaktionslogik für ViewModelExample.xaml
    /// </summary>
    public partial class ViewModelExample : DockPanel
    {
        public ViewModelExample()
        {
            InitializeComponent();
        }

        private void ResetObservers_Click(object sender, RoutedEventArgs e)
        {
            // Get our viewmodel from the example control
            ObserverExampleViewModel vm = example_observers.DataContext as ObserverExampleViewModel;

            // Here we simply call "ResetObservers()" to apply all current values to the original value
            vm.ObserverManager.ResetObservers();
        }
    }
}

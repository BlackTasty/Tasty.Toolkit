using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel.Commands;

namespace Tasty.Samples.ViewModel.Commands
{
    class OpenUrlCommand : CommandBase
    {
        public override bool CanExecute(object parameter)
        {
            // Here we combine checking the parameter type and parsing the parameter to a string
            if (parameter is string url)
            {
                // If we received a string we'll check if the string isn't empty, null or just exists out of white spaces
                return !string.IsNullOrWhiteSpace(url);
            }

            // If we didn't receive a string return false, disabling the control
            return false;
        }

        public override void Execute(object parameter)
        {
            // Checking and parsing parameter to string
            if (parameter is string url)
            {
                // With Process.Start(...) we can not only start new programs, but also open an URL in the standard webbrowser
                Process.Start(url);
            }
        }
    }
}

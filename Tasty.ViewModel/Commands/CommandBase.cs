using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Tasty.ViewModel.Commands
{
    /// <summary>
    /// Base class for command implementation.
    /// <para></para>
    /// Override "CanExecute(parameter)" to implement data validation or "Execute(parameter)" to implement functionality on execution
    /// </summary>
    public class CommandBase : ICommand
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="parameter"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public virtual bool CanExecute(object parameter)
        {
            return true;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="parameter"><inheritdoc/></param>
        public virtual void Execute(object parameter)
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.ViewModel.Observer
{
    /// <summary>
    /// Defines a new <see cref="EventArgs"/> which contains data regarding an <see cref="Observer{T}"/> change
    /// </summary>
    public class ChangeObservedEventArgs : EventArgs
    {
        private IObserver observer;
        private bool unsavedChanges;
        private dynamic newValue;

        /// <summary>
        /// Returns true if the original value of this <see cref="Observer"/> differs from the new value
        /// </summary>
        public bool UnsavedChanges => unsavedChanges;

        /// <summary>
        /// Returns the new value as a dynamic object
        /// </summary>
        public dynamic NewValue => newValue;

        /// <summary>
        /// Returns the <see cref="Observer{T}"/> which fired this event
        /// </summary>
        public IObserver Observer => observer;

        /// <summary>
        /// Create a new <see cref="ChangeObservedEventArgs"/>.
        /// </summary>
        /// <param name="unsavedChanges">Are there any unsaved changes</param>
        /// <param name="newValue">The new value for the observed object</param>
        /// <param name="observer">The observer which fired this event</param>
        public ChangeObservedEventArgs(bool unsavedChanges, dynamic newValue, IObserver observer)
        {
            this.unsavedChanges = unsavedChanges;
            this.newValue = newValue;
            this.observer = observer;
        }
    }
}

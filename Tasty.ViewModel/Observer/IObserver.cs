using System;

namespace Tasty.ViewModel.Observer
{
    /// <summary>
    /// Interface for observing properties.
    /// </summary>
    public interface IObserver
    {
        event EventHandler<ChangeObservedEventArgs> ChangeObserved;

        /// <summary>
        /// Return the original value as a dynamic object
        /// </summary>
        /// <returns></returns>
        dynamic GetOriginalValue();

        /// <summary>
        /// Returns true if original value and new value differ
        /// </summary>
        bool UnsavedChanges { get; }

        /// <summary>
        /// The display name of the observed property
        /// </summary>
        string PropertyName { get; }

        /// <summary>
        /// Resets this observer and applies the new value as the original value
        /// </summary>
        void Reset();
    }
}

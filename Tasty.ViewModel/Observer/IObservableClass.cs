using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.ViewModel.Observer
{
    /// <summary>
    /// Interface for classes which need observable properties.
    /// </summary>
    public interface IObservableClass
    {
        /// <summary>
        /// Holds all observers for this object
        /// </summary>
        ObserverManager ObserverManager { get; }

        /// <summary>
        /// A unique <see cref="System.Guid"/> represented as a string
        /// </summary>
        string Guid { get; }

        /// <summary>
        /// Returns if this object has any unsaved changes
        /// </summary>
        bool UnsavedChanges { get; }

        /// <summary>
        /// Create a copy of this object
        /// </summary>
        /// <returns>A copy of this <see cref="IObservableClass"/></returns>
        IObservableClass Copy();
    }
}

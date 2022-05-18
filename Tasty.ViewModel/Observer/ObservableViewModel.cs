using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.ViewModel.Observer
{
    /// <summary>
    /// Offers both <see cref="ViewModelBase"/> capabilities and an <see cref="Observer.ObserverManager"/>.
    /// </summary>
    public class ObservableViewModel : ViewModelBase, IObservableClass
    {
        private ObserverManager observerManager = new ObserverManager();
        private string guid;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ObserverManager ObserverManager => observerManager;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string Guid => guid;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool UnsavedChanges => observerManager.UnsavedChanges;

        /// <summary>
        /// Initialize a new <see cref="ObservableViewModel"/> with an <see cref="Observer.ObserverManager"/>.
        /// </summary>
        public ObservableViewModel() : this(System.Guid.NewGuid().ToString())
        {
        }

        private ObservableViewModel(string guid)
        {
            this.guid = guid;
            ObserverManager.ChangeObserved += ObserverManager_ChangeObserved;
        }

        private void ObserverManager_ChangeObserved(object sender, ChangeObservedEventArgs e)
        {
            InvokePropertyChanged("UnsavedChanges");
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public virtual IObservableClass Copy()
        {
            return new ObservableViewModel(guid);
        }
    }
}

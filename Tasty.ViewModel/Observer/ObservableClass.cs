namespace Tasty.ViewModel.Observer
{
    /// <summary>
    /// Default implementation for <see cref="IObservableClass"/>.
    /// </summary>
    public class ObservableClass : IObservableClass
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
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public virtual IObservableClass Copy()
        {
            return new ObservableClass(guid);
        }

        /// <summary>
        /// Initialize a new <see cref="ObservableClass"/> with a new guid.
        /// </summary>
        public ObservableClass() : this(System.Guid.NewGuid().ToString())
        {
        }

        /// <summary>
        /// Initialize a new <see cref="ObservableClass"/> with an existing guid.
        /// </summary>
        public ObservableClass(string guid)
        {
            this.guid = guid;
        }
    }
}

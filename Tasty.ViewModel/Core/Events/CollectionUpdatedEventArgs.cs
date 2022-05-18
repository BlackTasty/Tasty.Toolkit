using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel.Core.Enums;

namespace Tasty.ViewModel.Core.Events
{
    /// <summary>
    /// A class used for firing events when a collection changes
    /// </summary>
    /// <typeparam name="T">Object type of a collection</typeparam>
    public class CollectionUpdatedEventArgs<T> : EventArgs
    {
        private VeryObservableCollection<T> changedCollection;
        private CollectionChangeType changeType;
        private T content;

        /// <summary>
        /// The collection which has changed
        /// </summary>
        public VeryObservableCollection<T> ChangedCollection => changedCollection;

        /// <summary>
        /// The type of changed made to the collection
        /// </summary>
        public CollectionChangeType ChangeType => changeType;

        /// <summary>
        /// The added or changed content of a collection. Is null when change type is <see cref="CollectionChangeType.Removed"/>
        /// </summary>
        public T Content => content;

        /// <summary>
        /// Create a new <see cref="CollectionUpdatedEventArgs{T}"/> object
        /// </summary>
        /// <param name="changedCollection">The collection which has changed</param>
        /// <param name="content">The added or changed content</param>
        /// <param name="changeType">The type of change to the collection</param>
        public CollectionUpdatedEventArgs(VeryObservableCollection<T> changedCollection, T content, CollectionChangeType changeType)
        {
            this.changedCollection = changedCollection;
            this.changeType = changeType;
            this.content = content;
        }
    }
}

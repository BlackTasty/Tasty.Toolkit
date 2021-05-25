using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Tasty.ViewModel.Communication;
using Tasty.ViewModel.Observer;

namespace Tasty.ViewModel
{
    /// <summary>
    /// This class is an extension to the <see cref="ObservableCollection{T}"/>. 
    /// (this removes the need to call the "OnNotifyPropertyChanged" event every time you add, edit or remove an entry.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    public class VeryObservableCollection<T> : ObservableCollection<T>, INotifyPropertyChanged, IVeryObservableCollection
    {
        public event EventHandler<EventArgs> ObserveChanges;

        protected bool autoSort;
        protected List<string> triggerAlso = new List<string>();
        protected bool observeChanges = true; //If this flag is set to false the collection won't fire CollectionChanged events
        protected Enum message;
        protected ObserverManager observerManager;

        private List<string> itemChecksums = new List<string>();
        private string checksum;
        private bool concatChecksums;

        new event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The property name of the collection. Change notifications for data binding are sent with this name!
        /// </summary>
        public string CollectionName { get; }

        /// <summary>
        /// The checksum for this collection
        /// </summary>
        public virtual string Checksum
        {
            get => checksum;
        }

        /// <summary>
        /// Returns the amount of items in this collection
        /// </summary>
        public virtual new int Count
        {
            get => base.Count;
        }

        /// <summary>
        /// Returns the current <see cref="Observer.ObserverManager"/> of this collection (if set in constructor)
        /// </summary>
        public virtual ObserverManager ObserverManager
        {
            get => observerManager;
        }

        /// <summary>
        /// Returns false if <see cref="Observer.ObserverManager"/> is set! Returns true if content of collection has changed, otherwise false
        /// </summary>
        public virtual bool UnsavedChanged
        {
            get
            {
                return observerManager?.UnsavedChanges ?? false;
            }
        }

        /*/// <summary>
        /// Returns false if <see cref="Observer.ObserverManager"/> is set! Returns true if content of collection has changed, otherwise false
        /// </summary>
        public virtual bool AnyUnsavedChanges
        {
            get
            {
                if (observerManager == null)
                {
                    return false;
                }

                foreach (var item in this)
                {
                    if (item is IObservableClass data)
                    {
                        return data.UnsavedChanges;
                    }
                }
                return false;
            }
        }*/

        /// <summary>
        /// Initializes the collection with the specified name.
        /// </summary>
        /// <param name="collectionName">The name of the collection (must match the property name!)</param>
        /// <param name="generateObservers">Default is false. When set to true, changes to this collection are monitored</param>
        /// <param name="message">Default is null. If set to an enum, viewmodels which subscribed to this message will receive change notifications</param>
        public VeryObservableCollection(string collectionName,
            bool generateObservers = false, Enum message = null)
        {
            if (typeof(T).GetProperties().FirstOrDefault(x => x.PropertyType == typeof(ObserverManager)) != null)
            {
                concatChecksums = true;
            }

            CollectionName = collectionName;
            CollectionChanged += Collection_CollectionChanged;
            this.message = message;
            if (generateObservers)
            {
                observerManager = new ObserverManager();
            }

            RefreshChecksum();
        }

        /// <summary>
        /// Tell this <see cref="VeryObservableCollection{T}"/> to trigger the PropertyChanged event on another property.
        /// </summary>
        /// <param name="propertyName">The name of the property (the property which is exposed to XAML)</param>
        public void TriggerAlso(string propertyName)
        {
            if (!triggerAlso.Contains(propertyName))
            {
                triggerAlso.Add(propertyName);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        public void RegisterParent(ObserverManager parent)
        {
            observerManager = parent;
            //changeManager?.ObserveProperty(this, CollectionName);
            RefreshChecksum();
        }

        public void UnregisterParent(ObserverManager parent)
        {

        }

        /// <summary>
        /// Adds multiple objects to the end of the <see cref="VeryObservableCollection{T}"/>.
        /// </summary>
        /// <param name="items">The objects to be added to the end of the <see cref="Collection{T}"/>.</param>
        public virtual void AddRange(IEnumerable<T> items)
        {
            observeChanges = false;
            foreach (T item in items)
            {
                Add(item);
            }

            observeChanges = true;
            //changeManager?.ObserveProperty(this, CollectionName);
            RefreshChecksum();
        }

        /// <summary>
        /// Adds multiple objects to the end of the <see cref="VeryObservableCollection{T}"/>.
        /// </summary>
        /// <param name="items">The objects to be added to the end of the <see cref="VeryObservableCollection{T}"/>.</param>
        public virtual void AddRange(List<T> items)
        {
            if (items == null)
            {
                return;
            }
            observeChanges = false;
            foreach (T item in items)
            {
                Add(item);
            }

            observeChanges = true;
            //changeManager?.ObserveProperty(this, CollectionName);
            RefreshChecksum();
        }

        /// <summary>
        /// Add a new object to the <see cref="VeryObservableCollection{T}"/>.
        /// </summary>
        /// <param name="item">THe object to be added.</param>
        public virtual new void Add(T item)
        {
            if (GetChildFromItem(item) is ObserverManager child && child != null)
            {
                itemChecksums.Add(child.Guid);
                observerManager?.RegisterChild(child);
            }
            else
            {
                itemChecksums.Add(item.ToString() + Count);
            }

            base.Add(item);
            if (autoSort)
            {
                List<T> lookupList = Items.OrderBy(x => x.ToString(), StringComparer.CurrentCultureIgnoreCase)
                    .ToList();
                foreach (T obj in lookupList)
                {
                    if (obj.Equals(item))
                    {
                        Remove(item);
                        Insert(lookupList.IndexOf(obj), obj);
                    }
                }
            }

            if (observeChanges)
            {
                //changeManager?.ObserveProperty(this, CollectionName);
                RefreshChecksum();
            }
        }

        /// <summary>
        /// Clears the current <see cref="VeryObservableCollection{T}"/>.
        /// </summary>
        public new void Clear()
        {
            itemChecksums.Clear();
            foreach (T item in this)
            {
                if (observerManager != null && GetChildFromItem(item) is ObserverManager child && child != null)
                {
                    observerManager.UnregisterChild(child);
                }
            }

            try
            {
                base.Clear();
            }
            catch
            {
                observeChanges = false;
                for (int i = 0; i < Count; i++)
                {
                    RemoveAt(0);
                }
                observeChanges = true;
            }
            //changeManager?.ObserveProperty(this, CollectionName);
            RefreshChecksum();
        }

        /// <summary>
        /// Clears the <see cref="VeryObservableCollection{T}"/> and adds the given object.
        /// </summary>
        /// <param name="value">The object to add</param>
        public void Set(T value)
        {
            Clear();
            Add(value);
        }

        /// <summary>
        /// Clears the <see cref="VeryObservableCollection{T}"/> and adds the given list objects.
        /// </summary>
        /// <param name="value">The objects to add</param>
        public void Set(IEnumerable<T> values)
        {
            Clear();
            AddRange(values);
        }

        /// <summary>
        /// Removes the target object from the <see cref="VeryObservableCollection{T}"/>.
        /// </summary>
        /// <param name="item">The object to remove</param>
        public new void Remove(T item)
        {
            if (GetChildFromItem(item) is ObserverManager child && child != null)
            {
                itemChecksums.Remove(child.Guid);
                observerManager?.UnregisterChild(child);
            }
            else
            {
                itemChecksums.Remove(item.ToString() + (Count - 1));
            }
            base.Remove(item);

            if (observeChanges)
            {
                //changeManager?.ObserveProperty(this, CollectionName);
                RefreshChecksum();
            }
        }

        /// <summary>
        /// Removes an object from the <see cref="VeryObservableCollection{T}"/> at the specified index.
        /// </summary>
        /// <param name="index">The index of the object to remove</param>
        public new void RemoveAt(int index)
        {
            if (index < 0 || index >= Count)
            {
                return;
            }

            T item = this[index];
            Remove(item);
        }

        /// <summary>
        /// Removes all items starting at the given index
        /// </summary>
        /// <param name="startIndex">Defines at which index the collection should remove all items</param>
        public void RemoveRange(int startIndex)
        {
            RemoveRange(startIndex, Count);
        }

        /// <summary>
        /// Removes all items in a range starting at the given index
        /// </summary>
        /// <param name="startIndex">Defines at which index the collection should remove all items</param>
        /// <param name="range">How many items shall be removed beginning from the start index</param>
        public void RemoveRange(int startIndex, int range)
        {
            observeChanges = false;
            for (int i = startIndex; i < range; i++)
            {
                RemoveAt(startIndex);
            }
            observeChanges = true;

            //changeManager?.ObserveProperty(this, CollectionName);
            RefreshChecksum();
        }

        /// <summary>
        /// Forces this <see cref="VeryObservableCollection{T}"/> to fire OnPropertyChanged events and lets the <see cref="Observer.ObserverManager"/> observe changes.
        /// </summary>
        public void Refresh()
        {
            if (message != null)
            {
                Mediator.Instance.NotifyColleagues(message, this);
            }
            OnObserveChanges(EventArgs.Empty);

            OnPropertyChanged(this, new PropertyChangedEventArgs(CollectionName));
            if (triggerAlso.Count > 0)
            {
                foreach (string trigger in triggerAlso)
                {
                    OnPropertyChanged(this, new PropertyChangedEventArgs(trigger));
                }
            }

            if (!concatChecksums)
            {
                itemChecksums.Clear();
                for (int i = 0; i < Count; i++)
                {
                    itemChecksums.Add(this[i].ToString() + i);
                }
            }

            //changeManager?.ObserveProperty(this, CollectionName);
            RefreshChecksum();
        }

        /// <summary>
        /// Creates a copy of this <see cref="VeryObservableCollection{T}"/>.
        /// </summary>
        /// <returns>A copy of this collection. If an <see cref="Observer.ObserverManager"/> is currently present, a new one will be generated.</returns>
        public IVeryObservableCollection Copy()
        {
            VeryObservableCollection<T> copy = new VeryObservableCollection<T>(CollectionName, observerManager != null, message);
            copy.AddRange(this.ToList());

            return copy;
        }

        /*public void Sort(SortFactory sortFactory)
        {
            observeChanges = false;
            Clear();
            AddRange(sortFactory.Execute());

            observeChanges = true;
            changeManager?.ObserveProperty(this, CollectionName);
        }*/

        protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);

            RefreshChecksum();
        }

        protected virtual void OnObserveChanges(EventArgs e)
        {
            ObserveChanges?.Invoke(this, e);
        }

        /// <summary>
        /// Searches an object of <see cref="T"/> for a property with class <see cref="Observer.ObserverManager"/> and returns the latter.
        /// </summary>
        /// <param name="item">The object to search</param>
        /// <returns>Returns the first occurrence of a property with class <see cref="Observer.ObserverManager"/> and returns it.
        /// If nothing is found, null is returned.</returns>
        private ObserverManager GetChildFromItem(T item)
        {
            var changeManagerProperty = typeof(T).GetProperties().FirstOrDefault(x => x.PropertyType == typeof(ObserverManager));
            if (changeManagerProperty != null)
            {
                return changeManagerProperty.GetValue(item) as ObserverManager;
            }

            return null;
        }

        private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (observeChanges)
            {
                Refresh();
            }
        }

        internal void RefreshChecksum(bool resetObserver = false)
        {
            /*string newChecksum = Hasher.HashPassword(string.Concat(itemChecksums),
                string.Format("{0}-{1}", CollectionName, Count));*/
            string newChecksum;
            if (concatChecksums)
            {
                newChecksum = GetChecksumForString(string.Concat(itemChecksums));
            }
            else
            {
                newChecksum = "";
                foreach (string itemChecksum in itemChecksums)
                {
                    newChecksum += itemChecksum;
                }
            }

            observerManager?.ObserveProperty(newChecksum, "Checksum");
            checksum = newChecksum;

            if (resetObserver)
            {
                observerManager.ResetObservers();
            }
        }

        private string GetChecksumForString(string target)
        {
            string hash;
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                hash = BitConverter.ToString(
                  md5.ComputeHash(Encoding.UTF8.GetBytes(target))
                ).Replace("-", String.Empty);
            }

            return hash;
        }
    }
}
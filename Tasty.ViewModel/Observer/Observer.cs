﻿using System;
using System.Collections;

namespace Tasty.ViewModel.Observer
{
    /// <summary>
    /// Define a new <see cref="Observer{T}"/> for a property.
    /// </summary>
    /// <typeparam name="T">The type of the observed property</typeparam>
    class Observer<T> : IObserver
    {
        /// <summary>
        /// Gets fired whenever the current value changes
        /// </summary>
        public event EventHandler<ChangeObservedEventArgs> ChangeObserved;

        private string propertyName;

        private T originalValue;
        private T currentValue;

        /// <summary>
        /// Return the original value
        /// </summary>
        public T OriginalValue => originalValue;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public dynamic GetOriginalValue()
        {
            return originalValue;
        }

        /// <summary>
        /// The current value of the observed property
        /// </summary>
        public T CurrentValue
        {
            get => currentValue;
            set
            {
                currentValue = value;
                OnChangeObserved(new ChangeObservedEventArgs(UnsavedChanges, currentValue, this));
            }
        }

        /// <summary>
        /// Returns true if originalValue doesn't match currentValue
        /// </summary>
        public bool UnsavedChanges
        {
            get
            {
                if (originalValue == null && currentValue == null)
                {
                    return false;
                }

                if (currentValue is IVeryObservableCollection currentCollection)
                {
                    if (originalValue is IVeryObservableCollection originalCollection)
                    {
                        return originalCollection.Count != currentCollection.Count && currentCollection.UnsavedChanged;
                    }

                    return true;
                }
                else if (currentValue is IObservableClass currentBaseData)
                {
                    if (originalValue is IObservableClass originalBaseData)
                    {
                        return currentBaseData.ObserverManager.Compare(originalBaseData.ObserverManager);
                    }
                    return true;
                }
                else if (currentValue is IList currentList)
                {
                    if (originalValue != null && originalValue is IList originalList)
                    {
                        //TODO implement list comparison
                        return false;
                    }

                    return true;
                }
                else
                {
                    if (originalValue == null && currentValue != null)
                    {
                        return true;
                    }
                    return !originalValue.Equals(currentValue);
                }
            }
        }

        /// <summary>
        /// The name of the observed property
        /// </summary>
        public string PropertyName => propertyName;

        /// <summary>
        /// Initialize a new Observer to watch changes
        /// </summary>
        /// <param name="propertyName">The property name to observe</param>
        /// <param name="currentValue">The current value</param>
        public Observer(string propertyName, T currentValue)
        {
            this.propertyName = propertyName;
            this.currentValue = currentValue;
            Reset();
        }

        /// <summary>
        /// Set currentValue as originalValue
        /// </summary>
        public void Reset()
        {
            if (currentValue is IVeryObservableCollection currentCollection)
            {
                originalValue = (T)currentCollection.Copy();
            }
            else if (currentValue is IObservableClass currentBaseData)
            {
                originalValue = (T)currentBaseData.Copy();
            }
            else
            {
                originalValue = currentValue;
            }
        }

        protected virtual void OnChangeObserved(ChangeObservedEventArgs e)
        {
            ChangeObserved?.Invoke(this, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            return "{ Type: " + originalValue.GetType().ToString() + " }";
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Tasty.ViewModel.Observer
{
    /// <summary>
    /// Defines a new <see cref="ObserverManager"/>, which holds multiple <see cref="Observer{T}"/> objects. 
    /// Implements <see cref="ViewModelBase"/> and fires PropertyChanged events for the "UnsavedChanges" property
    /// <para></para>
    /// This class manages registration of Observers, firing ChangeObserved events and more.
    /// </summary>
    public class ObserverManager : ViewModelBase
    {
        /// <summary>
        /// Fires whenever an <see cref="Observer{T}"/> changes
        /// </summary>
        public event EventHandler<ChangeObservedEventArgs> ChangeObserved;

        private string guid;
        private List<IObserver> changeObservers = new List<IObserver>();
        private ObserverManager parent;
        private List<ObserverManager> children = new List<ObserverManager>();

        /// <summary>
        /// Returns true if any observer has unsaved changes
        /// </summary>
        public bool UnsavedChanges => changeObservers.Any(x => x.UnsavedChanges) || children.Any(x => x.UnsavedChanges);

        /// <summary>
        /// Returns all hooked observers for this <see cref="ObserverManager"/>
        /// </summary>
        public List<IObserver> ChangeObservers => changeObservers;

        /// <summary>
        /// Overrides the internal Guid used in detection of <see cref="VeryObservableCollection{T}"/> changes.
        /// </summary>
        public string GuidOverride { get; set; }

        internal string Guid => GuidOverride != null ? GuidOverride : guid;

        /// <summary>
        /// If set to false, all functionality is disabled. 
        /// <para></para>
        /// This includes the method <see cref="ObserveProperty{T}(T, string)"/>, which disables hooking up new <see cref="Observer{T}"/>.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Initializes a new <see cref="ObserverManager"/>.
        /// </summary>
        public ObserverManager()
        {
            guid = System.Guid.NewGuid().ToString();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        ~ObserverManager()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            // Unhook all event handlers for observers and child ObserverManager classes
            foreach (var observer in changeObservers)
            {
                observer.ChangeObserved -= Observer_ChangeObserved;
            }

            if (children?.Count > 0)
            {
                foreach (var child in children.ToList())
                {
                    UnregisterChild(child);
                }
            }
        }

        /// <summary>
        /// Register a child <see cref="ObserverManager"/>. "ChangeObserved" events fired by children will bubble up and get re-fired by this manager.
        /// <para></para>
        /// <see cref="IsEnabled"/> is set to false, the child is already added or is null this will have no effect!
        /// </summary>
        /// <param name="child">The child to add and observe</param>
        public void RegisterChild(ObserverManager child)
        {
            if (child == null || !IsEnabled)
            {
                return;
            }

            if (!children.Any(x => x.guid == child.guid))
            {
                children.Add(child);
                child.ChangeObserved += Observer_ChangeObserved;
                child.RegisterParent(this);
            }
        }

        /// <summary>
        /// Unregister a child <see cref="ObserverManager"/> if registered.
        /// <para></para>
        /// If the child is null or isn't registered this will have no effect!
        /// </summary>
        /// <param name="child">The child to remove</param>
        public void UnregisterChild(ObserverManager child)
        {
            if (child == null)
            {
                return;
            }

            int index = children.FindIndex(x => x.guid == child.guid);

            if (index > -1)
            {
                child.UnregisterParent();
                child.ChangeObserved -= Observer_ChangeObserved;
                children.RemoveAt(index);
            }
        }

        /// <summary>
        /// Used both to register new observers and update existing ones. Observes changes on the specified property.
        /// <para></para>
        /// If <see cref="IsEnabled"/> is set to false, this will have no effect!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The current value of the property</param>
        /// <param name="propertyName">Can be left empty when called from inside the target property. The display name of the property to watch.</param>
        public void ObserveProperty<T>(T value, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null || !IsEnabled)
            {
                return;
            }

            if (changeObservers.FirstOrDefault(x => x.PropertyName == propertyName) is Observer<T> observer)
            {
                observer.CurrentValue = value;
            }
            else
            {
                observer = new Observer<T>(propertyName, value);
                observer.ChangeObserved += Observer_ChangeObserved;
                changeObservers.Add(observer);
            }
        }

        /// <summary>
        /// Returns a registered <see cref="Observer{T}"/> with the specified property name
        /// </summary>
        /// <param name="name">The display name of the observed property</param>
        /// <returns></returns>
        public IObserver GetObserverByName(string name)
        {
            return ChangeObservers.FirstOrDefault(x => x.PropertyName == name);
        }

        /// <summary>
        /// Returns the original value of an <see cref="Observer{T}"/> with the specified property name
        /// </summary>
        /// <param name="name">The display name of the observed property</param>
        /// <param name="defaultValue">The default value should no observer exist yet for the property</param>
        /// <returns></returns>
        public dynamic GetOriginalValueByName(string name, dynamic defaultValue)
        {
            return GetObserverByName(name)?.GetOriginalValue() ?? defaultValue;
        }

        /// <summary>
        /// Resets all observers, which sets the "UnsavedChanges" flag to false. You would usually call this method after saving a file for example.
        /// </summary>
        public void ResetObservers()
        {
            foreach (var observer in changeObservers)
            {
                observer.Reset();
                OnChangeObserved(new ChangeObservedEventArgs(UnsavedChanges, observer.GetOriginalValue(), observer));
            }

            foreach (var child in children)
            {
                child.ResetObservers();
            }

            /*InvokePropertyChanged("UnsavedChanges");
            if (changeObservers.FirstOrDefault(x => x.PropertyName == "Name") is ChangeObserver<string> nameObserver)
            {
                OnChangeObserved(new ChangeObservedEventArgs(UnsavedChanges, nameObserver.CurrentValue, nameObserver));
            }*/
        }

        /// <summary>
        /// Compares two <see cref="ObserverManager"/> and returns if they match.
        /// </summary>
        /// <param name="changeManager">The manager to compare this to</param>
        /// <returns>Returns true if both <see cref="ObserverManager"/> match, otherwise false</returns>
        public bool Compare(ObserverManager changeManager)
        {
            foreach (var observer in ChangeObservers)
            {
                var targetObserver = changeManager.ChangeObservers.FirstOrDefault(x => x.PropertyName == observer.PropertyName);
                if (targetObserver == null ||
                    observer.GetOriginalValue() == null && targetObserver.GetOriginalValue() == null ||
                    (observer.GetOriginalValue() == null && targetObserver.GetOriginalValue() != null) ||
                    (observer.GetOriginalValue() != null && targetObserver.GetOriginalValue() == null) ||
                    !observer.GetOriginalValue().Equals(targetObserver.GetOriginalValue()))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (changeObservers == null)
            {
                return "Not initialized!";
            }

            string observerString = "";
            foreach (var observer in changeObservers)
            {
                if (observerString != "")
                {
                    observerString += ", " + observer.ToString();
                }
                else
                {
                    observerString = observer.ToString();
                }
            }
            return "{" + string.Format(" Observer count: {0}; Observers: {1} ", changeObservers.Count, observerString) + "}";
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected virtual void OnChangeObserved(ChangeObservedEventArgs e)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            ChangeObserved?.Invoke(this, e);
        }

        private void Observer_ChangeObserved(object sender, ChangeObservedEventArgs e)
        {
            OnChangeObserved(e);
            InvokePropertyChanged("UnsavedChanges");
        }

        private void RegisterParent(ObserverManager parent)
        {
            this.parent = parent;
        }

        private void UnregisterParent()
        {
            parent = null;
        }
    }
}
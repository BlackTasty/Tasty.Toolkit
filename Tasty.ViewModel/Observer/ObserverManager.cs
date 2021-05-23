using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.ViewModel.Observer
{
    public class ObserverManager : ViewModelBase
    {
        public event EventHandler<ChangeObservedEventArgs> ChangeObserved;

        private string guid;
        private List<IObserver> changeObservers = new List<IObserver>();
        private ObserverManager parent;
        private List<ObserverManager> children = new List<ObserverManager>();

        /// <summary>
        /// Returns true if any observer has unsaved changes
        /// </summary>
        public bool UnsavedChanges => changeObservers.Any(x => x.UnsavedChanges) || children.Any(x => x.UnsavedChanges);

        public List<IObserver> ChangeObservers => changeObservers;

        public string Guid => RootGuid != null ? RootGuid : guid;

        //public List<ObserverManager> Parents => parents;

        public ObserverManager()
        {
            guid = System.Guid.NewGuid().ToString();
        }

        public string RootGuid { get; set; }

        ~ObserverManager()
        {
            foreach (var observer in changeObservers)
            {
                observer.ChangeObserved -= Observer_ChangeObserved;
            }
        }

        public void RegisterParent(ObserverManager parent)
        {
            this.parent = parent;
            /*if (!this.parent.Any((object x) => x.guid == parent.guid))
            {
                parent.ChangeObserved += Child_ChangeObserved;
                this.parent.Add(parent);
            }*/
        }

        public void UnregisterParent()
        {
            parent = null;
            /*int index = this.parent.FindIndex((object x) => x.guid == parent.guid);

            if (index > -1)
            {
                parent.ChangeObserved -= Child_ChangeObserved;
                this.parent.RemoveAt(index);
            }*/
        }

        public void RegisterChild(ObserverManager child)
        {
            if (child == null)
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
        /// Observes changes on the specified property. Used both to register new observers and update existing ones
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The current value of the property</param>
        /// <param name="propertyName">The property to watch</param>
        public void ObserveProperty<T>(T value, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
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

        public IObserver GetObserverByName(string name)
        {
            return ChangeObservers.FirstOrDefault(x => x.PropertyName == name);
        }

        /// <summary>
        /// Set currentValue as originalValue on every observer
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

        private void Observer_ChangeObserved(object sender, ChangeObservedEventArgs e)
        {
            OnChangeObserved(e);
            InvokePropertyChanged("UnsavedChanges");
        }

        protected virtual void OnChangeObserved(ChangeObservedEventArgs e)
        {
            ChangeObserved?.Invoke(this, e);
        }

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
    }
}
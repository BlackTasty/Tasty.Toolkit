using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.ViewModel.Observer
{
    public class ObservableClass : IObservableClass
    {
        protected ObserverManager observerManager = new ObserverManager();
        protected string guid;

        public ObserverManager ObserverManager => observerManager;

        public string Guid => guid;

        public bool UnsavedChanges => observerManager.UnsavedChanges;

        public IObservableClass Copy()
        {
            return new ObservableClass(guid);
        }

        public ObservableClass()
        {
            guid = System.Guid.NewGuid().ToString();
        }

        public ObservableClass(string guid)
        {
            this.guid = guid;
        }
    }
}

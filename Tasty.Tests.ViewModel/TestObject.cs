using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel;
using Tasty.ViewModel.Observer;

namespace Tasty.Tests.ViewModel
{
    class TestObject : ObservableClass
    {
        private string name;
        private VeryObservableCollection<TestObject> nestedObjects = new VeryObservableCollection<TestObject>("NestedObjects", true);

        public string Name
        {
            get => name;
            set
            {
                observerManager.ObserveProperty(value);
                name = value;
            }
        }

        public VeryObservableCollection<TestObject> NestedObjects
        {
            get => nestedObjects;
            set
            {
                observerManager.ObserveProperty(value);
                nestedObjects = value;
            }
        }

        public TestObject(string name, List<TestObject> nestedObjects)
        {
            ObserverManager.RegisterChild(NestedObjects.ObserverManager);
            Name = name;
            NestedObjects.AddRange(nestedObjects);
        }

        ~TestObject()
        {
            ObserverManager.UnregisterChild(NestedObjects.ObserverManager);
        }
    }
}

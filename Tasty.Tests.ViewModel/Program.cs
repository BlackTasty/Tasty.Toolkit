using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.Tests.Base;
using Tasty.ViewModel;

namespace Tasty.Tests.ViewModel
{
    class Program
    {
        private static VeryObservableCollection<string> simpleCollection = new VeryObservableCollection<string>("SimpleCollection", true);
        private static VeryObservableCollection<TestObject> complexCollection = new VeryObservableCollection<TestObject>("ComplexCollection", true);

        static void Main(string[] args)
        {
            System.Console.Clear();
            TestRunner.RunTest(Test_CheckObserverBindings_Simple, "Checking observer bindings (simple collection)");
            TestRunner.RunTest(Test_AddItem_Simple, "Checking observer functionality (simple collection)");
            TestRunner.RunTest(Test_CheckObserverBindings_Complex, "Checking observer bindings (complex collection)");
            TestRunner.RunTest(Test_AddItem_Complex, "Checking observer functionality (complex collection)");

            //Console.Write("Correct data returned:\t\t", Test_Select());
            if (TestRunner.FailedTests == 0)
            {
                Base.Console.WriteLine_Status(string.Format("Successfully ran {0}/{0} tests!", TestRunner.TestCount), Status.Success);
            }
            else if (TestRunner.FailedTests < TestRunner.TestCount)
            {
                Base.Console.WriteLine_Status(string.Format("Ran {0}/{1} tests, but some failed!", TestRunner.TestCount - TestRunner.FailedTests, TestRunner.TestCount), Status.Warning);
            }
            else
            {
                Base.Console.WriteLine_Status("All tests failed!", Status.Fail);
            }
            System.Console.WriteLine("Press any key to exit.");
            System.Console.ReadLine();
        }

        static bool Test_CheckObserverBindings_Simple()
        {
            bool observerManagerExists = simpleCollection.ObserverManager != null;
            Base.Console.WriteLine_Status("Observer manager set", observerManagerExists);

            if (!observerManagerExists)
            {
                System.Console.WriteLine("Test aborted!");
                return false;
            }

            bool observersSet = simpleCollection.ObserverManager.ChangeObservers.Count == 1 && simpleCollection.ObserverManager.ChangeObservers[0].PropertyName == "Checksum";
            Base.Console.WriteLine_Status("Observer set", observersSet);

            if (!observersSet)
            {
                System.Console.WriteLine("Test aborted!");
                return false;
            }

            return true;
        }

        static bool Test_CheckObserverBindings_Complex()
        {
            bool observerManagerExists = complexCollection.ObserverManager != null;
            Base.Console.WriteLine_Status("Observer manager set", observerManagerExists);

            if (!observerManagerExists)
            {
                System.Console.WriteLine("Test aborted!");
                return false;
            }

            bool observersSet = complexCollection.ObserverManager.ChangeObservers.Count == 1 && complexCollection.ObserverManager.ChangeObservers[0].PropertyName == "Checksum";
            Base.Console.WriteLine_Status("Observers set", observersSet);

            if (!observersSet)
            {
                System.Console.WriteLine("Test aborted!");
                return false;
            }

            return true;
        }

        static bool Test_AddItem_Simple()
        {
            simpleCollection.Add("Foobar");

            bool itemAdded = simpleCollection.Count == 1 && simpleCollection[0] == "Foobar";
            Base.Console.WriteLine_Status("Item added", itemAdded);

            if (!itemAdded)
            {
                System.Console.WriteLine("Test aborted!");
                return false;
            }

            bool unsavedFlagSet = simpleCollection.UnsavedChanged == true;
            Base.Console.WriteLine_Status("Unsaved flag set", unsavedFlagSet);

            if (!unsavedFlagSet)
            {
                System.Console.WriteLine("Test aborted!");
                return false;
            }

            simpleCollection.ObserverManager.ResetObservers();

            unsavedFlagSet = simpleCollection.UnsavedChanged == false;
            Base.Console.WriteLine_Status("Unsaved flag reset success", unsavedFlagSet);

            if (!unsavedFlagSet)
            {
                System.Console.WriteLine("Test aborted!");
                return false;
            }

            return true;
        }

        static bool Test_AddItem_Complex()
        {
            complexCollection.Add(new TestObject("Foobar", new List<TestObject>()));

            bool itemAdded = complexCollection.Count == 1 && complexCollection[0].Name == "Foobar";
            Base.Console.WriteLine_Status("Item added", itemAdded);

            if (!itemAdded)
            {
                System.Console.WriteLine("Test aborted!");
                return false;
            }

            bool unsavedFlagSet = complexCollection.UnsavedChanged == true;
            Base.Console.WriteLine_Status("Unsaved flag set after add item", unsavedFlagSet);

            if (!unsavedFlagSet)
            {
                System.Console.WriteLine("Test aborted!");
                return false;
            }

            complexCollection.ObserverManager.ResetObservers();
            unsavedFlagSet = complexCollection.UnsavedChanged == false;
            Base.Console.WriteLine_Status("Unsaved flag reset success", unsavedFlagSet);

            if (!unsavedFlagSet)
            {
                System.Console.WriteLine("Test aborted!");
                return false;
            }


            complexCollection[0].NestedObjects.Add(new TestObject("Barfoo", new List<TestObject>()));

            bool nestedItemAdded = complexCollection[0].NestedObjects.Count == 1 && complexCollection[0].NestedObjects[0].Name == "Barfoo";
            Base.Console.WriteLine_Status("Nested item added", nestedItemAdded);

            if (!nestedItemAdded)
            {
                System.Console.WriteLine("Test aborted!");
                return false;
            }

            unsavedFlagSet = complexCollection.UnsavedChanged == true;
            Base.Console.WriteLine_Status("Unsaved flag set after add nested item", unsavedFlagSet);

            if (!unsavedFlagSet)
            {
                System.Console.WriteLine("Test aborted!");
                return false;
            }

            complexCollection.ObserverManager.ResetObservers();
            unsavedFlagSet = complexCollection.UnsavedChanged == false;
            Base.Console.WriteLine_Status("Unsaved flag reset success", unsavedFlagSet);

            if (!unsavedFlagSet)
            {
                System.Console.WriteLine("Test aborted!");
                return false;
            }

            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.ViewModel.Observer
{
    public interface IObservableClass
    {
        ObserverManager ObserverManager { get; }

        string Guid { get; }

        bool UnsavedChanges { get; }

        IObservableClass Copy();
    }
}

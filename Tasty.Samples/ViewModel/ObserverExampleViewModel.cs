using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel;
using Tasty.ViewModel.Observer;

namespace Tasty.Samples.ViewModel
{
    class ObserverExampleViewModel : ObservableViewModel
    {
        private string mMediatorReceiverText;

        public string MediatorReceiverText
        {
            get => mMediatorReceiverText;
            set
            {
                ObserverManager.ObserveProperty(value);
                mMediatorReceiverText = value;
                InvokePropertyChanged();
            }
        }

        public ObserverExampleViewModel()
        {
            // Here we initially set MediatorReceiverText so our ObserverManager hooks up an Observer for this property
            MediatorReceiverText = "Change me!";
        }
    }
}

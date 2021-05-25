using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel;
using Tasty.ViewModel.Communication;

namespace Tasty.Samples.ViewModel
{
    class MediatorExampleViewModel : ViewModelBase
    {
        private string mMediatorReceiverText;

        public string MediatorReceiverText
        {
            get => mMediatorReceiverText;
            set
            {
                mMediatorReceiverText = value;
                InvokePropertyChanged();
            }
        }

        public MediatorExampleViewModel()
        {
            // Register mediator receiver. Set enum property to the same as the sender!
            Mediator.Instance.Register(o =>
            {
                if (o is string value)
                {
                    MediatorReceiverText = value;
                }
            }, MediatorExampleEnum.ExampleChanged);
        }
    }
}

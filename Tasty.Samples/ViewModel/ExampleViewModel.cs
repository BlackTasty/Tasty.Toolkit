using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.Samples.ViewModel.Commands;
using Tasty.ViewModel;
using Tasty.ViewModel.Communication;

namespace Tasty.Samples.ViewModel
{
    class ExampleViewModel : ViewModelBase
    {
        private string mUrl;
        private string mMediatorSenderText;
        // This is where we initialize our command. You can also move the initialization into the constructor
        private OpenUrlCommand mOpenUrlCommand = new OpenUrlCommand();
        private string mInput;

        public string Url
        {
            get => mUrl;
            set
            {
                mUrl = value;
                InvokePropertyChanged();
            }
        }

        public string Input
        {
            get => mInput;
            set
            {
                mInput = value;
                InvokePropertyChanged();
            }
        }

        public string MediatorSenderText
        {
            get => mMediatorSenderText;
            set
            {
                mMediatorSenderText = value;
                InvokePropertyChanged();

                // Notifies every class that registered for changes on "MediatorExampleEnum.ExampleChanged"
                Mediator.Instance.NotifyColleagues(MediatorExampleEnum.ExampleChanged, value);
            }
        }

        // This is the property we will bind the control command to
        // In this case we just need a getter as we don't change the variable
        public OpenUrlCommand OpenUrlCommand => mOpenUrlCommand;
    }
}

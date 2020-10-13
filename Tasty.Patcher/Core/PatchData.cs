using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.Patcher.ViewModel;

namespace Tasty.Patcher.Core
{
    class PatchData : ViewModelBase
    {
        private string mContent;
        private PatchType mType;

        public string Content
        {
            get => mContent;
            set
            {
                mContent = value;
                InvokePropertyChanged();
            }
        }

        public PatchType Type
        {
            get => mType;
            set
            {
                mType = value;
                InvokePropertyChanged();
            }
        }

        public string TypeToString()
        {
            switch (mType)
            {
                case PatchType.Added:
                    return "Added";
                case PatchType.Changed:
                    return "Changed";
                case PatchType.Disabled:
                    return "Disabled";
                case PatchType.Fixed:
                    return "Fixed";
                case PatchType.ReEnabled:
                    return "Re-Enabled";
                case PatchType.Removed:
                    return "Removed";
                default:
                    return null;
            }
        }

        public string GetColorForType()
        {
            switch (mType)
            {
                case PatchType.Added:
                    return "#008000";
                case PatchType.Changed:
                    return "#00aaff";
                case PatchType.Disabled:
                    return "#A4A4A4";
                case PatchType.Fixed:
                    return "#FF8000";
                case PatchType.ReEnabled:
                    return "#008000";
                case PatchType.Removed:
                    return "#FF0000";
                default:
                    return null;
            }
        }
    }
}

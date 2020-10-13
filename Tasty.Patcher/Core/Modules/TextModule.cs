using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.Patcher.Core.Modules
{
    class TextModule : IModule
    {
        public bool IsEnabled { get; set; }

        public TextModule(bool isEnabled)
        {
            IsEnabled = isEnabled;
        }

        public string FormatData(string title, string content, IEnumerable<PatchData> changes)
        {
            StringBuilder stringBuilder = new StringBuilder(DateTime.Now.ToString("dd.MM.yyyy"));

            foreach (PatchData data in changes)
            {
                stringBuilder.AppendFormat("\n{0}: {1}", data.TypeToString(), data.Content);
            }

            return stringBuilder.ToString();
        }
    }
}

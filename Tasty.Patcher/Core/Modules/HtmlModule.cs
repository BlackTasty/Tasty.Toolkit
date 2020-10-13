using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.Patcher.Core.Modules
{
    class HtmlModule : IModule
    {
        public bool IsEnabled { get; set; }

        public HtmlModule(bool isEnabled)
        {
            IsEnabled = isEnabled;
        }

        public string FormatData(string title, string content, IEnumerable<PatchData> changes)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("[size=150]{0}[/size]\n", title);

            if (!string.IsNullOrWhiteSpace(content))
            {
                stringBuilder.AppendLine(content)
                    .AppendLine();
            }

            stringBuilder.AppendLine("[list]");
            foreach (PatchData data in changes)
            {
                stringBuilder.AppendFormat("[*] [color={0}]{1}[/color]: {2}\n", data.GetColorForType(), data.TypeToString(), data.Content);
            }
            stringBuilder.Append("[/list]");

            return stringBuilder.ToString();
        }
    }
}

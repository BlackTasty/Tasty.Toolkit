using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.Patcher.Core.Modules
{
    class DiscordModule : IModule
    {
        public bool IsEnabled { get; set; }

        public DiscordModule(bool isEnabled)
        {
            IsEnabled = isEnabled;
        }

        public string FormatData(string title, string content, IEnumerable<PatchData> changes)
        {
            StringBuilder stringBuilder = new StringBuilder(title + "\n");
            if (!string.IsNullOrWhiteSpace(content))
            {
                stringBuilder.AppendLine(content)
                    .AppendLine();
            }

            foreach (PatchData data in changes)
            {
                stringBuilder.AppendFormat("**{0}**: {1}\n", data.TypeToString(), data.Content);
            }

            return stringBuilder.ToString();
        }
    }
}

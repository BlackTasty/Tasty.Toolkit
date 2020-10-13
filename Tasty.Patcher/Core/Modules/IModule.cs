using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.Patcher.Core.Modules
{
    /// <summary>
    /// Interface for modules like HTML module, Discord module, etc...
    /// This allows to create patchnotes for different platforms
    /// </summary>
    interface IModule
    {
        bool IsEnabled { get; set; }

        string FormatData(string title, string content, IEnumerable<PatchData> changes);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.Logging
{
    public interface IConsole
    {
        bool VerboseLogging();

        void WriteString(string str, LogType type);
    }
}

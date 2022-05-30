using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Exceptions
{
    class ColumnParseException : Exception
    {
        public ColumnParseException(string typeName) : base(string.Format("SQLiteManager cannot access Enum {0}, make sure to set its modifier to public.", typeName))
        {

        }
    }
}

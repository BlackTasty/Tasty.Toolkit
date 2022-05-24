using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Exceptions
{
    class AutoTableException : Exception
    {
        public AutoTableException(string message) : 
            base(message)
        {

        }
    }
}

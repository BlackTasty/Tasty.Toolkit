using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Exceptions
{
    class MissingAccessException : Exception
    {
        public MissingAccessException(string msg) : 
            base(msg)
        {

        }
    }
}

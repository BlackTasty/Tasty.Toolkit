using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Exceptions
{
    class MissingWriteAccessException : Exception
    {
        public MissingWriteAccessException(string propertyName) : 
            base("Cannot write to property \"" + propertyName + "\" because no set method is set! Check out the documentation for more information: [LINK]")
        {

        }
    }
}

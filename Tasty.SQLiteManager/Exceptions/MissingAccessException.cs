using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Exceptions
{
    class MissingAccessException : Exception
    {
        public MissingAccessException(PropertyInfo propertyInfo) : 
            base(string.Format("Cannot write to property \"{0}\" because no set method is set! Check out the documentation for more information: [LINK]", propertyInfo.Name))
        {

        }

        public MissingAccessException(Type classType) :
            base(string.Format("Cannot access class \"{0}\", set access modifier to public.", classType.Name))
        {

        }
    }
}

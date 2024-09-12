using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Exceptions
{
    class MissingPrimaryKeyException : Exception
    {
        internal MissingPrimaryKeyException(Type type) : base(string.Format("There is no property with the [SqlitePrimaryKey] attribute for \"{0}\" defined!\n" +
            "Either add the attribute to a property or derive from DatabaseEntry<T> class.", type.Name))
        {

        }
    }
}

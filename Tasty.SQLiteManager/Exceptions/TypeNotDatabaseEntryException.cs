using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Exceptions
{
    internal class TypeNotDatabaseEntryException : Exception
    {
        public TypeNotDatabaseEntryException(Type type) : base(string.Format("The type \"{0}\" does not inherit from DatabaseEntry, or implements the IDatabaseEntry interface!", type.Name))
        { }
    }
}

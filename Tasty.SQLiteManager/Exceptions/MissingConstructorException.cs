using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Exceptions
{
    internal class MissingConstructorException : Exception
    {
        public MissingConstructorException(Type type) : base(string.Format("The type \"{0}\" requires a constructor similar to the following:\n\n" +
                    "public {0}(TableDefinition<{0}> table) : base(table) {{ }}\n", type.Name))
        { }
    }
}

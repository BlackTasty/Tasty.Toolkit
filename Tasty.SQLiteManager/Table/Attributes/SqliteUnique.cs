using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Table.Attributes
{
    /// <summary>
    /// Attribute for defining UNIQUE columns.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SqliteUnique : Attribute
    {
        /// <summary>
        /// Defines a property as a UNIQUE column.
        /// </summary>
        public SqliteUnique()
        {
        }
    }
}

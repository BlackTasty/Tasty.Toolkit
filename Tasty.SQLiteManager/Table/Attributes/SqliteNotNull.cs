using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Table.Attributes
{
    /// <summary>
    /// Attribute for setting NOT NULL on columns.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SqliteNotNull : Attribute
    {
        /// <summary>
        /// Sets the NOT NULL flag for this property.
        /// </summary>
        public SqliteNotNull() { }
    }
}

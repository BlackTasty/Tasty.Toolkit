using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Table.Attributes
{
    /// <summary>
    /// Attribute for excluding property from table.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SqliteIgnore : Attribute
    {
        /// <summary>
        /// Tell SQLiteManager to exclude this property from the table.
        /// </summary>
        public SqliteIgnore() { }
    }
}

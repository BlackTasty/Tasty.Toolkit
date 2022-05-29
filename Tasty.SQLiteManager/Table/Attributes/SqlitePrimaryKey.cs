using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Table.Attributes
{
    /// <summary>
    /// Attribute for defining PRIMARY KEY columns.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SqlitePrimaryKey : Attribute
    {
        /// <summary>
        /// Defines a property as a PRIMARY KEY column.
        /// </summary>
        public SqlitePrimaryKey() { }
    }
}

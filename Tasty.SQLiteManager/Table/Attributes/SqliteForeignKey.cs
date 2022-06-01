using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.SQLiteManager.Table.ForeignKey;

namespace Tasty.SQLiteManager.Table.Attributes
{
    /// <summary>
    /// Attribute for defining foreign key properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SqliteForeignKey : Attribute
    {
        private readonly ForeignKeyData data;

        public ForeignKeyData Data => data;

        /// <summary>
        /// Defines a property as having foreign keys.
        /// </summary>
        /// <param name="childTableName">The name for the relationship table.</param>
        public SqliteForeignKey(string childTableName)
        {
            data = new ForeignKeyData(childTableName);
        }

        /// <summary>
        /// Defines a property as having foreign keys.
        /// </summary>
        /// <param name="isOneToOne">The name for the relationship table.</param>
        public SqliteForeignKey()
        {
            data = new ForeignKeyData(true);
        }
    }
}

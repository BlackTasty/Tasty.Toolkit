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
        public SqliteForeignKey(string childTableName) : this(childTableName, false)
        {
        }

        /// <summary>
        /// Defines a property as having foreign keys.
        /// </summary>
        /// <param name="childTableName">The name for the relationship table.</param>
        /// <param name="isManyToMany">If set to true, this relation is treated as a many-to-many (n-n) relation.</param>
        public SqliteForeignKey(string childTableName, bool isManyToMany)
        {
            data = new ForeignKeyData(childTableName, isManyToMany);
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

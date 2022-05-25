using System;
using System.Collections.Generic;
using Tasty.SQLiteManager.Table.Column;

namespace Tasty.SQLiteManager.Table
{
    /// <summary>
    /// Define a FOREIGN KEY for a <see cref="TableDefinition{T}"/>
    /// </summary>
    [Obsolete("This type of foreign key declaration is deprecated! See the documentation for more details: [LINK]")]
    public class ForeignKeyDefinition : DefinitionBase
    {
        private ITable foreignTable;
        private IColumn foreignKey;
        private IColumn targetKey;

        /// <summary>
        /// Define a new foreign key.
        /// </summary>
        /// <param name="name">The name of the foreign key.</param>
        /// <param name="foreignTable">The target table where the foreign key is located</param>
        /// <param name="targetKey">The <see cref="ColumnDefinition{T}"/> of the foreign key</param>
        /// <param name="foreignKey">The <see cref="ColumnDefinition{T}"/> of the local key</param>
        [Obsolete("This type of foreign key declaration is deprecated! See the documentation for more details: [LINK]")]
        public ForeignKeyDefinition(string name, ITable foreignTable, IColumn targetKey, IColumn foreignKey)
        {
            this.foreignKey = foreignKey;
            this.targetKey = targetKey;
            this.foreignTable = foreignTable;
            this.name = name;
        }

        /// <summary>
        /// Returns the SQL query for a FOREIGN KEY definition
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string foreignKeyName = (foreignKey as DefinitionBase).Name;
            string targetKeyName = (targetKey as DefinitionBase).Name;

            if (!foreignTable.ColumnExists(foreignKey))
            {
                throw new KeyNotFoundException("The target column with the name \"" + foreignKeyName +
                    "\" doesn't exist in the foreign table \"" + foreignTable.Name + "\"");
            }

            return string.Format("FOREIGN KEY(\"{0}\") REFERENCES \"{1}\"(\"{2}\") {3}",
                targetKeyName, foreignTable.Name, foreignKeyName, ParseConstraintType());
        }

        private string ParseConstraintType()
        {
            //TODO: Implement multiple constraints
            return "ON DELETE SET NULL";
        }
    }
}

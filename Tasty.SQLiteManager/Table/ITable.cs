using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.SQLiteManager.Table.Attributes;
using Tasty.SQLiteManager.Table.Column;
using Tasty.SQLiteManager.Table.Conditions;
using Tasty.SQLiteManager.Table.ForeignKey;

namespace Tasty.SQLiteManager.Table
{
    /// <summary>
    /// Interface for tables
    /// </summary>
    public interface ITable : ITableBase
    {
        /// <summary>
        /// Only used by the library; contains foreign key data for database initalization
        /// </summary>
        List<ForeignKeyData> ForeignKeyData { get; }

        /// <summary>
        /// Returns all child tables for this table, which are auto-generated when adding the <see cref="SqliteForeignKey"/> attribute to a list property
        /// </summary>
        List<ChildTableDefinition> ChildTables { get; }

        /// <summary>
        /// Returns the type of <see cref="T"/> for comparisons
        /// </summary>
        Type TableType { get; }

        /// <summary>
        /// A list of foreign keys defined for this table.
        /// </summary>
        [Obsolete("This method of retrieving foreign keys is deprecated and will be removed soon! Check out the documentation for more information: [LINK]")]
        List<ForeignKeyDefinition> ForeignKeys { get; }

        /// <summary>
        /// Returns if the specified column exists in this table.
        /// </summary>
        /// <param name="target">The column to search for</param>
        /// <returns>Returns true if the column exists</returns>
        bool ColumnExists(IColumn target);

        /// <summary>
        /// Returns if a column with the specified exists in this table.
        /// </summary>
        /// <param name="colName">The column name to search for</param>
        /// <returns>Returns true if the column exists</returns>
        bool ColumnExists(string colName);

        /// <summary>
        /// Returns the primary key column for this table.
        /// </summary>
        /// <returns>Returns the column which has the "PRIMARY KEY" flag set</returns>
        IColumn GetPrimaryKeyColumn();
    }
}

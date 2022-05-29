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
    }
}

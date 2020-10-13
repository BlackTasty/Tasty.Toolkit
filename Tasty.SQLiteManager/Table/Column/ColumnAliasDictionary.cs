using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Table.Column
{
    /// <summary>
    /// A list of <see cref="ColumnAlias"/> for a specific <see cref="ColumnDefinition{T}"/>
    /// </summary>
    public class ColumnAliasDictionary
    {
        private List<ColumnAlias> columnAliases;

        /// <summary>
        /// Check if the column has aliases
        /// </summary>
        public bool HasAliases => columnAliases?.Count > 0;

        /// <summary>
        /// Define a new set of aliases for a column.
        /// </summary>
        /// <param name="columnAliases">A list of column aliases</param>
        public ColumnAliasDictionary(List<ColumnAlias> columnAliases)
        {
            this.columnAliases = columnAliases;
        }

        /// <summary>
        /// Returns an empty <see cref="ColumnAliasDictionary"/>.
        /// </summary>
        public static ColumnAliasDictionary Default => new ColumnAliasDictionary(null);

        /// <summary>
        /// Return the column name, taking column aliases into account.
        /// </summary>
        /// <param name="name">The name of the column.</param>
        /// <returns></returns>
        public string FindByName(string name)
        {
            if (HasAliases)
            {
                ColumnAlias alias = columnAliases.FirstOrDefault(x => x.Name == name);
                return alias != null ? alias.AliasName : name;
            }
            else
            {
                return name;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Table.Column
{
    /// <summary>
    /// Used to define an alias for a column.
    /// </summary>
    public class ColumnAlias
    {
        private string name;
        private string aliasName;
        private int columnIndex;
        private int aliasColumnIndex;

        /// <summary>
        /// The current name of the column
        /// </summary>
        public string Name => name;

        /// <summary>
        /// The current index of the column
        /// </summary>
        public int ColumnIndex => columnIndex;

        /// <summary>
        /// The alias name for the column
        /// </summary>
        public string AliasName => !string.IsNullOrWhiteSpace(aliasName) ? aliasName : name;

        /// <summary>
        /// The alias index for the column
        /// </summary>
        public int AliasColumnIndex => aliasColumnIndex > -1 ? aliasColumnIndex : columnIndex;

        /// <summary>
        /// Define an alias for columns. Primarily used if a <see cref="ColumnDefinition{T}"/> has changed it's name or position in a <see cref="TableDefinition"/>.
        /// </summary>
        /// <param name="name">The new name for the column</param>
        /// <param name="columnIndex">The new index for the column</param>
        /// <param name="aliasName">Old name of the column</param>
        /// <param name="aliasColumnIndex">Old index of the column</param>
        public ColumnAlias(string name, int columnIndex, string aliasName, int aliasColumnIndex)
        {
            this.name = name;
            this.columnIndex = columnIndex;
            this.aliasName = aliasName;
            this.aliasColumnIndex = aliasColumnIndex;
        }
    }
}

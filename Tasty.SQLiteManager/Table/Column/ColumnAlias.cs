using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Table.Column
{
    public class ColumnAlias
    {
        private string name;
        private string aliasName;
        private int columnIndex;
        private int aliasColumnIndex;

        public string Name => name;

        public int ColumnIndex => columnIndex;

        public string AliasName => !string.IsNullOrWhiteSpace(aliasName) ? aliasName : name;

        public int AliasColumnIndex => aliasColumnIndex > -1 ? aliasColumnIndex : columnIndex;

        public ColumnAlias(string name, int columnIndex, string aliasName, int aliasColumnIndex)
        {
            this.name = name;
            this.columnIndex = columnIndex;
            this.aliasName = aliasName;
            this.aliasColumnIndex = aliasColumnIndex;
        }

        public ColumnAlias(string name, int columnIndex) : this(name, columnIndex, "", -1)
        {
        }
    }
}

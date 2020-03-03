using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Table.Column
{
    public class ColumnAliasDictionary
    {
        private List<ColumnAlias> columnAliases;

        public bool HasAliases => columnAliases?.Count > 0;

        public ColumnAliasDictionary(List<ColumnAlias> columnAliases)
        {
            this.columnAliases = columnAliases;
        }

        public static ColumnAliasDictionary Default => new ColumnAliasDictionary(null);

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

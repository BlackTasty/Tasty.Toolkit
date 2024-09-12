using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.SQLiteManager;
using Tasty.SQLiteManager.Table;
using Tasty.SQLiteManager.Table.Attributes;

namespace Tasty.Tests.SQLiteManager.Test
{
    [SqliteUseDatabase(Program.FOOBAR_IDENT)]
    [SqliteTable]
    public class DemoExcludedTable : DatabaseEntry<DemoExcludedTable>
    {
        public string Name { get; set; }

        public DemoExcludedTable(TableDefinition<DemoExcludedTable> table) : base(table) { }

        public DemoExcludedTable() : this(Database.GetTable<DemoExcludedTable>())
        {
        }
    }
}

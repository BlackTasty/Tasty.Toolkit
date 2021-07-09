using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.SQLiteManager.Attributes;
using Tasty.SQLiteManager.Table.Column;

namespace Tasty.Tests.SQLiteManager.TestTables
{
    [TableName("foobar")]
    class FoobarTable
    {
        [Column(ColumnMode.PRIMARY_KEY)]
        public int ID { get; set; }


        public string Name { get; set; }

        /*
        new TableDefinition("foobar", new List<IColumn>()
            {
                new ColumnDefinition<int>("ID", ColumnMode.PRIMARY_KEY),
                new ColumnDefinition<string>("name", ColumnMode.NOT_NULL),
                new ColumnDefinition<string>("email")
            })*/
    }
}

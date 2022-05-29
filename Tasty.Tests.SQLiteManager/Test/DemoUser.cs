using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.SQLiteManager.Table;
using Tasty.SQLiteManager.Table.Attributes;

namespace Tasty.Tests.SQLiteManager.Test
{
    [SqliteTable]
    class DemoUser : DatabaseEntry<DemoUser>
    {
        private string guid = "Foobar";

        [SqliteUnique]
        public string Guid { get => guid; private set => guid = value; }

        [SqLiteDefaultValue("Foobar")]
        public string Name { get; set; }

        public int Age { get; set; }

        [SqliteNotNull]
        public string Password { get; set; }

        [SqliteForeignKey("mapping_user_posts")]
        public List<DemoPost> Posts { get; set; } = new List<DemoPost>();

        public DemoUser(TableDefinition<DemoUser> table): base(table)
        {

        }
    }
}

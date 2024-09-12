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
    [SqliteTable]
    public class DemoUser : DatabaseEntry<DemoUser>
    {
        private string guid;

        [SqliteUnique]
        public string Guid { get => guid; private set => guid = value; }

        [SqLiteDefaultValue("Foobar")]
        public string Name { get; set; }

        public int Age { get; set; }

        [SqliteNotNull]
        public string Password { get; set; }

        [SqliteForeignKey("mapping_user_posts")]
        public List<DemoPost> Posts { get; set; } = new List<DemoPost>();

        [SqliteForeignKey("mapping_users_friends", true)]
        public List<DemoUser> Friends { get; set; } = new List<DemoUser>();

        [SqliteForeignKey()]
        public DemoUserSettings UserSettings { get; set; }

        [SqliteForeignKey("mapping_users_communities", true)]
        public List<DemoCommunity> Communities { get; set; } = new List<DemoCommunity>();

        public DemoUser(TableDefinition<DemoUser> table): base(table)
        {

        }

        public DemoUser(): this(Database.GetTable<DemoUser>())
        {
            guid = System.Guid.NewGuid().ToString();
        }
    }
}

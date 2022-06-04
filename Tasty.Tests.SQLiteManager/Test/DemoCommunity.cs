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
    public class DemoCommunity : DatabaseEntry<DemoCommunity>
    {
        public string Name { get; set; }

        [SqliteForeignKey("mapping_users_communities", true)]
        public List<DemoUser> Users { get; set; }

        public DemoCommunity(TableDefinition<DemoCommunity> table) : base(table)
        {

        }

        public DemoCommunity(): this(Database.Instance.GetTable<DemoCommunity>())
        {

        }
    }
}

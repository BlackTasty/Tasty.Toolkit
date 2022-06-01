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
    public class DemoPost : DatabaseEntry<DemoPost>
    {
        public string Title { get; set; }

        public DateTime CreateDate { get; set; }

        [SqliteForeignKey("mapping_user_posts")]
        public DemoUser Author { get; set; }

        public DemoPost(TableDefinition<DemoPost> table) : base(table)
        {

        }
    }
}

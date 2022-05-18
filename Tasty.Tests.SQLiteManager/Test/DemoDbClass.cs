using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.SQLiteManager.Table.Attributes;

namespace Tasty.Tests.SQLiteManager.Test
{
    class DemoDbClass
    {
        private string guid = "Foobar";

        [SqliteUnique]
        public string Guid => guid;

        [SqlitePrimaryKey]
        public int Id { get; set; }

        [SqLiteDefaultValue("Foobar")]
        public string Name { get; set; }

        [SqliteIgnore]
        public string IgnoredClass { get; set; }

        public int Integer { get; set; }

        public double Double { get; set; }

        public float Float{ get; set; }

        public long Long{ get; set; }

        public DateTime DateTime { get; set; }

        [SqliteNotNull]
        public bool Boolean { get; set; }
    }
}

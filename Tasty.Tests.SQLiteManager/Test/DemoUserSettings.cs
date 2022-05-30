using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.SQLiteManager.Table.Attributes;
using Tasty.SQLiteManager.Table;
using Tasty.Tests.SQLiteManager.Test.Enum;

namespace Tasty.Tests.SQLiteManager.Test
{
    [SqliteTable]
    class DemoUserSettings : DatabaseEntry<DemoUserSettings>
    {
        [SqLiteDefaultValue(true)]
        public bool DarkMode { get; set; }

        public DemoLanguageType Language { get; set; }

        public DemoUserSettings(TableDefinition<DemoUserSettings> table) : base(table)
        {

        }
    }
}

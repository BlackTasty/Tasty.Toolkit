using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.SQLiteManager.Table.ForeignKey;

namespace Tasty.SQLiteManager.Table.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SqliteForeignKey : Attribute
    {
        private ForeignKeyData data;

        public ForeignKeyData Data => data;

        public SqliteForeignKey(string childTableName)
        {
            data = new ForeignKeyData(childTableName);
        }
    }
}

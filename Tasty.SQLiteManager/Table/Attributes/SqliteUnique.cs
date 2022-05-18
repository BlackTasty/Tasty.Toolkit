using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Table.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SqliteUnique : Attribute
    {
        public SqliteUnique()
        {
        }
    }
}

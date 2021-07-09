using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.SQLiteManager.Table;

namespace Tasty.SQLiteManager.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableCaching : Attribute
    {
        public CacheMethod CacheMethod { get; set; }
    }
}

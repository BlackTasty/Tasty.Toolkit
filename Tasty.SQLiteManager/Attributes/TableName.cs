using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableName : Attribute
    {
        public string Name { get; set; }

        public TableName(string name)
        {
            Name = name;
        }
    }
}

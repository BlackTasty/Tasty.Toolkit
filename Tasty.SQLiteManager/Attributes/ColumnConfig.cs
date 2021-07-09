using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.SQLiteManager.Table.Column;

namespace Tasty.SQLiteManager.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnConfig : Attribute
    {
        public ColumnMode ColumnMode { get; set; } = ColumnMode.DEFAULT;
    }
}

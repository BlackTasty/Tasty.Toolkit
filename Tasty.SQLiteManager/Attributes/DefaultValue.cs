using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DefaultValue : Attribute
    {
        public dynamic Value { get; set; }

        public DefaultValue(dynamic value)
        {
            Value = value;
        }
    }
}

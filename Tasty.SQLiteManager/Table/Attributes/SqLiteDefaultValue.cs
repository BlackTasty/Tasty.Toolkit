using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Table.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SqLiteDefaultValue : Attribute
    {
        private readonly object defaultValue;

        public object DefaultValue => defaultValue;

        public SqLiteDefaultValue(object defaultValue)
        {
            this.defaultValue = defaultValue;
        }
    }
}

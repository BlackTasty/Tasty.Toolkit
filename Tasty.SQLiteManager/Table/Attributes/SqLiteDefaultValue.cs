using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Table.Attributes
{
    /// <summary>
    /// Attribute for setting a DEFAULT VALUE on a column.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SqLiteDefaultValue : Attribute
    {
        private readonly object defaultValue;

        public object DefaultValue => defaultValue;

        /// <summary>
        /// Define a default value for this property.
        /// </summary>
        /// <param name="defaultValue">The default value for this property.</param>
        public SqLiteDefaultValue(object defaultValue)
        {
            this.defaultValue = defaultValue;
        }
    }
}

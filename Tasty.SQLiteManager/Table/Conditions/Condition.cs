using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.SQLiteManager.Table.Column;

namespace Tasty.SQLiteManager.Table.Conditions
{
    public class Condition
    {
        private KeyValuePair<IColumn, dynamic> left;
        private KeyValuePair<IColumn, dynamic> right;
        ConditionType conditionType;

        public Condition(IColumn columnLeft, dynamic valueLeft, IColumn columnRight, dynamic valueRight, ConditionType conditionType)
        {
            this.left = new KeyValuePair<IColumn, dynamic>(columnLeft, valueLeft);
            this.right = new KeyValuePair<IColumn, dynamic>(columnRight, valueRight);
            this.conditionType = conditionType;
        }

        public Condition(IColumn column, dynamic value)
        {
            this.left = new KeyValuePair<IColumn, dynamic>(column, value);
            this.right = default;
            this.conditionType = ConditionType.NONE;
        }

        public override string ToString()
        {
            switch (conditionType)
            {
                case ConditionType.AND:
                    return string.Format("{0} = {1} AND {2} = {3}", left.Key.Name, left.Key.ParseColumnValue(left.Value),
                        right.Key.Name, right.Key.ParseColumnValue(right.Value));
                case ConditionType.OR:
                    return string.Format("{0} = {1} OR {2} = {3}", left.Key.Name, left.Key.ParseColumnValue(left.Value),
                        right.Key.Name, right.Key.ParseColumnValue(right.Value));
                default:
                    return string.Format("{0} = {1}", left.Key.Name, left.Key.ParseColumnValue(left.Value));
            }
        }
    }
}

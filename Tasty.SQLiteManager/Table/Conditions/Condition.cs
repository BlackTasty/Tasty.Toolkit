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

        public Condition(KeyValuePair<IColumn, dynamic> left, KeyValuePair<IColumn, dynamic> right, ConditionType conditionType)
        {
            this.left = left;
            this.right = right;
            this.conditionType = conditionType;
        }

        public Condition(KeyValuePair<IColumn, dynamic> left) : this(left, default(KeyValuePair<IColumn, dynamic>), ConditionType.NONE)
        {
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

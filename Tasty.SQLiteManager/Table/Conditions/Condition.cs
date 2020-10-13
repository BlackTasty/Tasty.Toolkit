using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.SQLiteManager.Table.Column;

namespace Tasty.SQLiteManager.Table.Conditions
{
    /// <summary>
    /// Define a condition for SQL queries
    /// </summary>
    public class Condition
    {
        private KeyValuePair<IColumn, dynamic> left;
        private KeyValuePair<IColumn, dynamic> right;
        ConditionType conditionType;

        /// <summary>
        /// Define a multi-condition for SQL queries
        /// </summary>
        /// <param name="columnLeft">The first column to compare</param>
        /// <param name="valueLeft">The first column value to compare</param>
        /// <param name="columnRight">The second column to compare</param>
        /// <param name="valueRight">The second column value to compare</param>
        /// <param name="conditionType">The type of condition between (<see cref="ConditionType.AND"/>, <see cref="ConditionType.OR"/>)</param>
        public Condition(IColumn columnLeft, dynamic valueLeft, IColumn columnRight, dynamic valueRight, ConditionType conditionType)
        {
            this.left = new KeyValuePair<IColumn, dynamic>(columnLeft, valueLeft);
            this.right = new KeyValuePair<IColumn, dynamic>(columnRight, valueRight);
            this.conditionType = conditionType;
        }

        /// <summary>
        /// Define a condition for SQL queries
        /// </summary>
        /// <param name="column">The column to compare</param>
        /// <param name="value">The column value to compare</param>
        public Condition(IColumn column, dynamic value)
        {
            this.left = new KeyValuePair<IColumn, dynamic>(column, value);
            this.right = default;
            this.conditionType = ConditionType.NONE;
        }

        /// <summary>
        /// Parses the condition to SQL-friendly text
        /// </summary>
        /// <returns>Returns the condition as SQL query</returns>
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

using System.Collections.Generic;
using System.Linq;

namespace Tasty.SQLiteManager.Table.Conditions
{
    class ConditionSet : List<Condition>
    {
        public ConditionSet()
        {

        }

        public ConditionSet(List<Condition> conditions)
        {
            AddRange(conditions);
            for (int i = 1; i < Count - 1; i++)
            {
            }
        }

        public ConditionSet(params Condition[] conditions) : this(conditions.ToList())
        {

        }
    }
}

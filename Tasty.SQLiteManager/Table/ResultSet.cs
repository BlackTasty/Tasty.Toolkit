using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Table
{
    public class ResultSet : List<RowData>
    {
        public bool IsEmpty { get => Count == 0; }

        public ResultSet()
        {

        }

        public ResultSet(List<RowData> data)
        {
            AddRange(data);
        }

        public bool ColumnExists(string columnName)
        {
            return IsEmpty ? false : this[0].Columns?.ContainsKey(columnName) ?? false;
        }
    }
}

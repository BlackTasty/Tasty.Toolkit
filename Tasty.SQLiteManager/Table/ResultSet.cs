using System.Collections.Generic;

namespace Tasty.SQLiteManager.Table
{
    /// <summary>
    /// Contains results from SELECT statements
    /// </summary>
    public class ResultSet : List<RowData>
    {
        /// <summary>
        /// Returns if this <see cref="ResultSet"/> contains any results
        /// </summary>
        public bool IsEmpty { get => Count == 0; }

        /// <summary>
        /// 
        /// </summary>
        public ResultSet()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public ResultSet(List<RowData> data)
        {
            AddRange(data);
        }

        /// <summary>
        /// Check if the specified column exists
        /// </summary>
        /// <param name="columnName">The name of the column</param>
        /// <returns></returns>
        public bool ColumnExists(string columnName)
        {
            return IsEmpty ? false : this[0].Columns?.ContainsKey(columnName) ?? false;
        }
    }
}

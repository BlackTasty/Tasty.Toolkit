using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Table
{
    public interface IResultSet
    {
        /// <summary>
        /// Returns if this <see cref="IResultSet"/> contains any results
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Check if the specified column exists
        /// </summary>
        /// <param name="columnName">The name of the column</param>
        /// <returns></returns>
        bool ColumnExists(string columnName);
    }
}

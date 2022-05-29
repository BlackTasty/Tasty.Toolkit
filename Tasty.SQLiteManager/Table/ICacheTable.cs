using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.SQLiteManager.Table.Column;

namespace Tasty.SQLiteManager.Table
{
    /// <summary>
    /// Interface for <see cref="CacheTableDefinition{T}"/> objects.
    /// </summary>
    public interface ICacheTable : ITable
    {
        /// <summary>
        /// The target <see cref="ColumnDefinition{T}"/> to define expiring values when <see cref="CacheMethod.DELETE_EXPIRED"/> is set.
        /// </summary>
        IColumn ExpireDateColumn { get; set; }

        /// <summary>
        /// Based on the <see cref="CacheMethod"/> this function will either delete all entries (<see cref="CacheMethod.DELETE_ON_LOAD"/>) 
        /// or only delete entries which expiration date has expired (<see cref="CacheMethod.DELETE_EXPIRED"/>)
        /// </summary>
        /// <returns>
        /// Returns a sql query string (index 0) and a log message for a successful execution (index 1)
        /// </returns>
        string[] ClearCache();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Table.Column
{
    /// <summary>
    /// Interface for table columns
    /// </summary>
    public interface IColumn
    {
        /// <summary>
        /// The name of the column
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Returns the default value of the column
        /// </summary>
        dynamic DefaultValue { get; }

        /// <summary>
        /// Returns the column type for this column.
        /// </summary>
        ColumnType ColumnType { get; }

        /// <summary>
        /// Returns if this column allows null values (<see cref="ColumnMode.NOT_NULL"/>)
        /// </summary>
        bool NotNull { get; }

        /// <summary>
        /// Returns if this column is a primary key (<see cref="ColumnMode.PRIMARY_KEY"/>)
        /// </summary>
        bool PrimaryKey { get; }

        /// <summary>
        /// Returns if this column has unique values (<see cref="ColumnMode.UNIQUE"/>)
        /// </summary>
        bool Unique { get; }

        /// <summary>
        /// Returns the string formatter for this column. This is primarily used to create SQL queries.
        /// </summary>
        string StringFormatter { get; }

        /// <summary>
        /// Formats a value into a SQL-friendly value
        /// </summary>
        /// <param name="value">The value to format</param>
        /// <returns></returns>
        string ParseColumnValue(dynamic value);
    }
}

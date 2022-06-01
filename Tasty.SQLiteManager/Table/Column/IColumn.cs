using System;
using System.Reflection;

namespace Tasty.SQLiteManager.Table.Column
{
    /// <summary>
    /// Interface for <see cref="ColumnDefinition{T}"/> objects
    /// </summary>
    public interface IColumn
    {
        /// <summary>
        /// Returns the data type of this column.
        /// </summary>
        Type DataType { get; }

        /// <summary>
        /// Returns the real type of this column in case DataType is <see cref="Nullable"/>, or null.
        /// </summary>
        Type UnderlyingType { get; }

        /// <summary>
        /// Property data, used to match database rows to their class property equivalent.
        /// </summary>
        PropertyInfo PropertyInfo { get; }

        /// <summary>
        /// Returns the name of the table this column belongs to.
        /// </summary>
        string ParentTableName { get; }

        /// <summary>
        /// The name of the column.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Returns the default value of the column.
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
        /// Returns if this column is a foreign key.
        /// </summary>
        bool IsForeignKey { get; }

        /// <summary>
        /// Formats a value into a SQL-friendly value
        /// </summary>
        /// <param name="value">The value to format</param>
        /// <returns></returns>
        string ParseColumnValue(dynamic value);
    }
}

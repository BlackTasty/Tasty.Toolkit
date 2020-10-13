namespace Tasty.SQLiteManager.Table.Column
{
    /// <summary>
    /// Used to control the mode of columns
    /// </summary>
    public enum ColumnMode
    {
        /// <summary>
        /// No rules are applied
        /// </summary>
        DEFAULT,
        /// <summary>
        /// Only on columns with type <see cref="int"/>! Define this column as primary key, auto-increments with each insert into table
        /// </summary>
        PRIMARY_KEY,
        /// <summary>
        /// null not allowed as value
        /// </summary>
        NOT_NULL,
        /// <summary>
        /// Value can only appear once in table
        /// </summary>
        UNIQUE
    }
}

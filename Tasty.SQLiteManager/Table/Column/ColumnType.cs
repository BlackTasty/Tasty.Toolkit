namespace Tasty.SQLiteManager.Table.Column
{
    /// <summary>
    /// The value type of a column
    /// </summary>
    public enum ColumnType
    {
        /// <summary>
        /// The value is a signed integer, stored in 1, 2, 3, 4, 6, or 8 bytes depending on the magnitude of the value.
        /// </summary>
        INTEGER,
        /// <summary>
        /// The value is a text string, stored using the database encoding(UTF-8, UTF-16BE or UTF-16LE)
        /// </summary>
        TEXT,
        /// <summary>
        /// = REAL (The value is a floating point value, stored as an 8-byte IEEE floating point number.)
        /// </summary>
        FLOAT,
        /// <summary>
        /// Same as INTEGER, but it only accepts 0 or 1
        /// </summary>
        BOOLEAN,
        /// <summary>
        /// = BLOB (The value is a blob of data, stored exactly as it was input.)
        /// </summary>
        OBJECT,
        /// <summary>
        /// Tells the API that it should expect a custom column type which gets parsed to a string
        /// </summary>
        SPECIAL
    }
}

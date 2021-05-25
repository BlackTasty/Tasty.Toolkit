namespace Tasty.SQLiteManager.Table
{
    enum CacheMethod
    {
        /// <summary>
        /// Re-creates the entire table on initialization. Useful for temporary tables
        /// </summary>
        DELETE_ON_LOAD,
        /// <summary>
        /// Deletes expired entries in a table. The target column can be set via <see cref="CacheTableDefinition.ExpireDateColumn"/>
        /// </summary>
        DELETE_EXPIRED
    }
}

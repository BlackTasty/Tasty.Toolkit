using System;
using System.Collections.Generic;
using System.Globalization;
using Tasty.SQLiteManager.Table.Column;

namespace Tasty.SQLiteManager.Table
{
    /// <summary>
    /// Cache tables are a special definition of tables. 
    /// Use this for tables which save temporary data like expiration tokens and call "ClearCache()" whenever you want to clean this table.
    /// Note: ClearCache() is always called on startup of the database!
    /// </summary>
    class CacheTableDefinition<T> : TableDefinition<T>, ICacheTable
    {
        private readonly CacheMethod cacheMethod;
        private IColumn expireDateColumn;

        /// <summary>
        /// The target <see cref="ColumnDefinition{T}"/> to define expiring values when <see cref="CacheMethod.DELETE_EXPIRED"/> is set.
        /// </summary>
        public IColumn ExpireDateColumn { get => expireDateColumn; set => expireDateColumn = value; }

        /// <summary>
        /// Define a new temporary table. Default cache method is <see cref="CacheMethod.DELETE_ON_LOAD"/>
        /// </summary>
        /// <param name="name">The name of the temporary table</param>
        public CacheTableDefinition(string name) : base(name)
        {
            cacheMethod = CacheMethod.DELETE_ON_LOAD;
        }

        /// <summary>
        /// Define a new temporary table.
        /// </summary>
        /// <param name="name">The name of the temporary table</param>
        /// <param name="cacheMethod">Configure how the temporary table should behave.</param>
        public CacheTableDefinition(string name, CacheMethod cacheMethod) : base(name)
        {
            this.cacheMethod = cacheMethod;
        }

        [Obsolete("This constructor is deprecated and will be removed soon. Check out the documentation for setting up tables for more details.", false)]
        /// <summary>
        /// Define a new temporary table with the specified columns. Default cache method is <see cref="CacheMethod.DELETE_ON_LOAD"/>
        /// </summary>
        /// <param name="name">The name of the temporary table</param>
        /// <param name="columns">A list of <see cref="ColumnDefinition{T}"/> objects for this table.</param>
        public CacheTableDefinition(string name, List<IColumn> columns) : base(name, columns)
        {
            cacheMethod = CacheMethod.DELETE_ON_LOAD;
        }

        /// <summary>
        /// Define a new temporary table with the specified columns.
        /// </summary>
        /// <param name="name">The name of the temporary table</param>
        /// <param name="columns">A list of <see cref="ColumnDefinition{T}"/> objects for this table.</param>
        /// <param name="cacheMethod">Configure how the temporary table should behave.</param>
        [Obsolete("This constructor is deprecated and will be removed soon. Check out the documentation for setting up tables for more details: [LINK]", false)]
        public CacheTableDefinition(string name, List<IColumn> columns, CacheMethod cacheMethod) : base(name, columns)
        {
            this.cacheMethod = cacheMethod;
        }

        /// <summary>
        /// Define a new temporary table with the specified columns. Default cache method is <see cref="CacheMethod.DELETE_ON_LOAD"/>
        /// </summary>
        /// <param name="name">The name of the temporary table</param>
        /// <param name="foreignKeys">A list of foreign key definitions.</param>
        /// <param name="columns">A list of <see cref="ColumnDefinition{T}"/> objects for this table.</param>
        [Obsolete("This constructor is deprecated and will be removed soon. Check out the documentation for setting up tables for more details: [LINK]", false)]
        public CacheTableDefinition(string name, List<IColumn> columns, List<ForeignKeyDefinition> foreignKeys) : base(name, columns, foreignKeys)
        {
            cacheMethod = CacheMethod.DELETE_ON_LOAD;
        }

        /// <summary>
        /// Define a new temporary table with the specified columns.
        /// </summary>
        /// <param name="name">The name of the temporary table</param>
        /// <param name="columns">A list of <see cref="ColumnDefinition{T}"/> objects for this table.</param>
        /// <param name="foreignKeys">A list of foreign key definitions.</param>
        /// <param name="cacheMethod">Configure how the temporary table should behave.</param>
        [Obsolete("This constructor is deprecated and will be removed soon. Check out the documentation for setting up tables for more details: [LINK]", false)]
        public CacheTableDefinition(string name, List<IColumn> columns, List<ForeignKeyDefinition> foreignKeys, CacheMethod cacheMethod) :
            base(name, columns, foreignKeys)
        {
            this.cacheMethod = cacheMethod;
        }

        /// <summary>
        /// Based on the <see cref="CacheMethod"/> this function will either delete all entries (<see cref="CacheMethod.DELETE_ON_LOAD"/>) 
        /// or only delete entries which expiration date has expired (<see cref="CacheMethod.DELETE_EXPIRED"/>)
        /// </summary>
        /// <returns>
        /// Returns a sql query string (index 0) and a log message for a successful execution (index 1)
        /// </returns>
        public string[] ClearCache()
        {
            switch (cacheMethod)
            {
                case CacheMethod.DELETE_EXPIRED:
                    if (ColumnExists(expireDateColumn))
                    {
                        ResultSet result = Select(new List<IColumn>() { this["ID"], expireDateColumn });

                        string dateTimeFormat = expireDateColumn.StringFormatter;
                        string sql = "BEGIN TRANSACTION;\n";
                        int removedEntries = 0;

                        foreach (var row in result)
                        {
                            DateTime expireDate = DateTime.ParseExact(row.Columns[ExpireDateColumn.Name], dateTimeFormat, CultureInfo.InvariantCulture);
                            if (DateTime.Now > expireDate)
                            {
                                sql += string.Format("DELETE FROM {0} WHERE ID = {1};\n", Name, this["ID"].ParseColumnValue(row.Columns["ID"]));
                                removedEntries++;
                            }
                        }
                        sql += "COMMIT;";
                        return new string[] { sql, string.Format("Removed {0} expired entries in table \"{1}\"!", removedEntries, Name) };
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Format("The table \"{0}\" does not contain a column with the name \"{1}\"!",
                            Name, expireDateColumn.Name));
                    }
                case CacheMethod.DELETE_ON_LOAD:
                    return new string[] { "DELETE FROM '" + Name + "';", string.Format("Emptied cache table \"{0}\"!", Name) };
            }

            throw new NotImplementedException(string.Format("The method \"{0}\" is not implemented yet!", cacheMethod.ToString()));
        }
    }
}

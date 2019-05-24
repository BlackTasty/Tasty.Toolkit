using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.SQLiteManager.Table.Column;

namespace Tasty.SQLiteManager.Table
{
    /// <summary>
    /// Cache tables are a special definition of tables. Use this for tables which save temporary data like expiration tokens and call "ClearCache()" whenever you want to cleanup this table.
    /// PS: ClearCache() is always called on startup of the database!
    /// </summary>
    class CacheTableDescriptor : TableDescriptor
    {
        private CacheMethod cacheMethod;
        private IColumn expireDateColumn;
        
        public IColumn ExpireDateColumn { get => expireDateColumn; set => expireDateColumn = value; }

        public CacheTableDescriptor(string name) : base(name) { }

        public CacheTableDescriptor(string name, CacheMethod cacheMethod) : base(name)
        {
            this.cacheMethod = cacheMethod;
        }

        public CacheTableDescriptor(string name, List<IColumn> columns) : base(name, columns) { }

        public CacheTableDescriptor(string name, List<IColumn> columns, CacheMethod cacheMethod) : base(name, columns)
        {
            this.cacheMethod = cacheMethod;
        }

        public CacheTableDescriptor(string name, List<IColumn> columns, List<ForeignKeyDescriptor> foreignKeys) : base(name, columns, foreignKeys) { }

        public CacheTableDescriptor(string name, List<IColumn> columns, List<ForeignKeyDescriptor> foreignKeys, CacheMethod cacheMethod) : base(name, columns, foreignKeys)
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

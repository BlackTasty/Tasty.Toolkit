using Tasty.Logging;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using Tasty.SQLiteManager.Table;
using Tasty.SQLiteManager.Table.Column;
using System.Collections;
using System.Text;

namespace Tasty.SQLiteManager
{
    /// <summary>
    /// The heart of the SQLiteManager. Manages the database
    /// </summary>
    public class Database : IList<TableDefinition>
    {
        /// <summary>
        /// </summary>
        protected List<TableDefinition> tables = new List<TableDefinition>();

        #region IList implementation
        /// <summary>
        /// Gets the number of tables in this database.
        /// </summary>
        public int Count => tables.Count;

        /// <inheritdoc/>
        public bool IsReadOnly => true;

        /// <summary>
        /// Gets or sets the table at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the table to get or set.</param>
        /// <returns></returns>
        public TableDefinition this[int index]
        {
            get => tables[index];
            set => tables[index] = value;
        }

        /// <summary>
        /// Gets or sets the table with the specified table name.
        /// </summary>
        /// <param name="tableName">The table with the specified name.</param>
        /// <returns></returns>
        public TableDefinition this[string tableName]
        {
            get => tables.Find(x => x.Name == tableName);
            set => tables[tables.IndexOf(tables.Find(x => x.Name == tableName))] = value;
        }

        /// <summary>
        /// Adds a table
        /// </summary>
        /// <param name="item">The table to add</param>
        public void Add(TableDefinition item)
        {
            tables.Add(item);
        }

        /// <summary>
        /// Clears all tables
        /// </summary>
        public void Clear()
        {
            tables.Clear();
        }

        /// <summary>
        /// Returns if the specified table exists 
        /// </summary>
        /// <param name="item">The table definition to search</param>
        /// <returns></returns>
        public bool Contains(TableDefinition item)
        {
            return tables.Contains(item);
        }

        /// <summary>
        /// Copies all <see cref="TableDefinition"/> objects into a new array.
        /// </summary>
        /// <param name="array">Copy of all table definitions</param>
        /// <param name="arrayIndex">Starting index from where the copying should begin</param>
        public void CopyTo(TableDefinition[] array, int arrayIndex)
        {
            tables.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes a table.
        /// </summary>
        /// <param name="item">The <see cref="TableDefinition"/> to remove</param>
        /// <returns></returns>
        public bool Remove(TableDefinition item)
        {
            return tables.Remove(item);
        }

        /// <inheritdoc/>
        public IEnumerator<TableDefinition> GetEnumerator()
        {
            return tables.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return tables.GetEnumerator();
        }

        /// <summary>
        /// Returns the index of the specified table.
        /// </summary>
        /// <param name="item">The <see cref="TableDefinition"/> of which you want the index</param>
        /// <returns>The index of the specified table</returns>
        public int IndexOf(TableDefinition item)
        {
            return tables.IndexOf(item);
        }

        /// <summary>
        /// Inserts a new table
        /// </summary>
        /// <param name="index">The index where you want to insert the table</param>
        /// <param name="item">The <see cref="TableDefinition"/> to insert</param>
        public void Insert(int index, TableDefinition item)
        {
            tables.Insert(index, item);
        }

        /// <summary>
        /// Remove a table at the specified index.
        /// </summary>
        /// <param name="index">The index of the table you want to remove</param>
        public void RemoveAt(int index)
        {
            tables.RemoveAt(index);
        }
        #endregion

        private static Database instance;
        private static int loops;
        private Logger logger;
        
        private string dbPath;
        private string connString;
        
        /// <summary>
        /// Allows access to the <see cref="Database"/> instance if initialized, else an exception is thrown.
        /// </summary>
        public static Database Instance
        {
            get
            {
                if (instance == null)
                {
                    throw new DatabaseNotInitializedException();
                }

                if (loops > 5)
                {
                    throw new StackOverflowException("Do not execute code on the database while it is being initialized!");
                }

                loops++;
                loops = 0;
                return instance;
            }
        }

        /// <summary>
        /// Returns if the database file exists
        /// </summary>
        public bool FileExists { get => File.Exists(dbPath); }

        /// <summary>
        /// Initializes the database.
        /// </summary>
        /// <param name="dbPath">The path to your SQLite database</param>
        /// <param name="tables">A list of <see cref="TableDefinition"/> which represent the database structure</param>
        /// <param name="logger">(optional) A custom <see cref="Logger"/> to redirect output to another file</param>
        /// <param name="forceInitialize">(optional) Forces the re-initialization of the <see cref="Database"/> singleton object</param>
        public static void Initialize(string dbPath, List<TableDefinition> tables, Logger logger = null, bool forceInitialize = false)
        {
            if (instance == null || forceInitialize)
            {
                if (tables == null)
                {
                    tables = new List<TableDefinition>();
                }
                instance = new Database(dbPath, tables, logger);
                instance.ClearCacheTables();
            }
        }

        private Database(string dbPath, List<TableDefinition> tables, Logger logger = null)
        {
            this.logger = logger != null ? logger : Logger.Instance; 
            this.dbPath = dbPath;
            connString = string.Format("Data Source={0};Version=3;", dbPath);

            this.tables = tables;

            CheckDatabase();
        }

        /// <summary>
        /// Converts all tables, columns and rows to a SQL query string. Useful for exporting the database as a text
        /// </summary>
        /// <returns></returns>
        public string ExportToSQL()
        {
            string tableSql = "";
            string dataSql = "";
            //TODO: Iterate through all tables, get all rows of each table and "convert" those entries into INSERT statements
            //this.logger.WriteLog("Starting database export...", Color.Magenta);
            foreach (TableDefinition table in this)
            {
                #region Create DROP TABLE
                tableSql += string.Format("-- Recreate table '{0}'\n" +
                    "DROP TABLE IF EXISTS \"{0}\";\n{1}\n\n", table.Name, table.ToString());
                #endregion

                #region Iterate through all rows and create INSERT
                ResultSet result = table.Select();
                if (result.Count > 0)
                {
                    #region Create first part of INSERT
                    string columnSql = "";
                    foreach (IColumn column in table)
                    {
                        if (string.IsNullOrEmpty(columnSql))
                        {
                            columnSql += column.Name;
                        }
                        else
                        {
                            columnSql += ", " + column.Name;
                        }
                    }

                    dataSql += string.Format("-- Fill table '{0}' with data\n" +
                        "INSERT INTO \"{0}\" ({1}) VALUES", table.Name, columnSql);
                    #endregion

                    #region Iterate every row, read column data and add to INSERT
                    foreach (RowData row in result)
                    {
                        string rowSql = "";
                        foreach (IColumn resultColumn in table)
                        {
                            if (row.Columns.TryGetValue(resultColumn.Name, out dynamic value))
                            {
                                if (string.IsNullOrEmpty(rowSql))
                                {
                                    rowSql += resultColumn.ParseColumnValue(value);
                                }
                                else
                                {
                                    rowSql += ", " + resultColumn.ParseColumnValue(value);
                                }
                            }
                        }
                        dataSql += string.Format("\n\t({0})", rowSql);
                    }
                    dataSql += ";\n";
                    #endregion
                }
                #endregion
            }

            return tableSql + "\n" + dataSql;
        }

        /// <summary>
        /// Imports a database from an sql file
        /// </summary>
        /// <param name="path">The path to the sql file</param>
        /// <returns>Returns a error message if something didn't work</returns>
        internal string ImportFromSQL(string path)
        {
            //TODO: Import sql file into database
            string fileContent = File.ReadAllText(path);
            return null;
        }

        #region  Database
        private void CreateDatabase()
        {
            if (FileExists)
                File.Delete(dbPath);
            
            SQLiteConnection.CreateFile(dbPath);

            #region Initializing database
            foreach (TableDefinition table in this)
            {
                ExecuteSQL(table.ToString());
            }
            #endregion

            this.logger.WriteLog("Database created!");
        }

        private void CheckTables()
        {
            this.logger.WriteLog("Scanning database for changes...");
            foreach (TableDefinition table in this)
            {
                string tableExistsSql = "SELECT name FROM sqlite_master WHERE type='table' AND name='" + table.Name + "';";
                bool tableExists = SelectData(tableExistsSql, table).Count > 0;
                if (tableExists)
                {
                    #region Check all columns of the current table for changes
                    List<string> columns = GetTableColumns(table.Name);
                    foreach (IColumn column in table)
                    {
                        if (!columns.Contains(column.Name))
                        {
                            if (!column.Unique)
                            {
                                this.logger.WriteLog("Missing column in table \"{0}\" detected! (Column: {1}; SQL: {2})",
                                    LogType.WARNING, table.Name, column.Name, column.ToString());

                                ExecuteSQL(string.Format("ALTER TABLE {0} ADD COLUMN {1}",
                                    table.Name, column.ToString()));
                                this.logger.WriteLog("Missing column added!");
                            }
                            else
                            {
                                this.logger.WriteLog("Unable to add a unique column to \"{0}\" with ALTER TABLE! (Column: {1}; SQL: {2})",
                                    LogType.ERROR, table.Name, column.Name, column.ToString());
                            }
                        }
                    }

                    List<string> removableColumns = new List<string>();
                    foreach (string column in columns)
                    {
                        if (!table.Any(x => x.Name == column))
                        {
                            removableColumns.Add(column);
                        }
                    }

                    if (removableColumns.Count > 0)
                    {
                        this.logger.WriteLog("Leftover columns in table \"{0}\" detected! (Columns: {1})",
                            LogType.WARNING, table.Name, string.Join(", ", removableColumns.ToArray()));

                        bool columnsRemoved = false;
                        Console.WriteLine("Would you like to remove these columns? (y/N)");
                        if (Console.ReadKey().Key == ConsoleKey.Y)
                        {
                            Console.WriteLine("\n\t\tWARNING: Any data stored in these columns will be lost forever!\n");
                            Console.WriteLine("Would you like to proceed? (y/N)");
                            if (Console.ReadKey().Key == ConsoleKey.Y)
                            {
                                Console.WriteLine();
                                #region Copy data
                                this.logger.WriteLog("(1/3) Copying data...");

                                #region Build SELECT
                                string selector = null;
                                foreach (IColumn newColumn in table)
                                {
                                    if (selector == null)
                                    {
                                        selector = newColumn.Name;
                                    }
                                    else
                                    {
                                        selector += ", " + newColumn.Name;
                                    }
                                }
                                #endregion

                                var result = SelectData("SELECT " + selector + " FROM " + table.Name, table);
                                Dictionary<IColumn, dynamic>[] dataBackup = new Dictionary<IColumn, dynamic>[result.Count];
                                for (int i = 0; i < result.Count; i++)
                                {
                                    Console.CursorLeft = 0;
                                    Console.Write("                                                                     ");
                                    Console.CursorLeft = 0;
                                    Console.Write("Process: {0}/{1}", i + 1, result.Count);
                                    RowData row = result[i];
                                    Dictionary<IColumn, dynamic> dataCopy = new Dictionary<IColumn, dynamic>();
                                    foreach (KeyValuePair<string, dynamic> data in row.Columns)
                                    {
                                        dataCopy.Add(table[data.Key], data.Value);
                                    }
                                    dataBackup[i] = dataCopy;
                                }
                                #endregion

                                #region Rebuild original table
                                Console.WriteLine();
                                this.logger.WriteLog("(2/3) Rebuilding table...");
                                ExecuteSQL("DROP TABLE " + table.Name + ";");
                                ExecuteSQL(table.ToString());
                                #endregion

                                #region Restoring data
                                this.logger.WriteLog("(3/3) Restoring data...");
                                ExecuteSQL(table.GenerateBulkInsert(dataBackup));
                                #endregion

                                columnsRemoved = true;
                            }
                        }

                        if (columnsRemoved)
                        {
                            this.logger.WriteLog("Columns removed!");
                        }
                        else
                        {
                            this.logger.WriteLog("Columns kept!");
                        }
                    }
                    #endregion
                }
                else
                {
                    #region Create the table if it doesn't exist
                    ExecuteSQL(table.ToString());
                    #endregion
                }
            }
        }

        /// <summary>
        /// Iterate through all cache tables and clear them.
        /// </summary>
        public void ClearCacheTables()
        {
            foreach (TableDefinition table in this)
            {
                if (table is CacheTableDefinition cacheTable)
                {
                    string[] data = cacheTable.ClearCache();
                    ExecuteSQL(data[0]);
                    this.logger.WriteLog(data[1]);
                }
            }
        }

        private void RemoveColumns(List<string> columns)
        {

        }

        /// <summary>
        /// Returns a list of all column names of the specified table
        /// </summary>
        /// <param name="tableName">The name of the table</param>
        /// <returns></returns>
        public List<string> GetTableColumns(string tableName)
        {
            List<string> columnNames = new List<string>();
            using (SQLiteConnection con = new SQLiteConnection(connString))
            {
                con.Open();
                using (SQLiteCommand cmd = new SQLiteCommand("PRAGMA table_info(" + tableName + ");", con))
                {
                    cmd.ExecuteNonQuery();

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var test = reader.GetFieldType(1);
                            columnNames.Add(reader.GetString(1));
                        }
                    }
                }
            }

            return columnNames;
        }

        /// <summary>
        /// Checks if the database exists and if the structure has changed.
        /// </summary>
        public void CheckDatabase()
        {
            if (!FileExists)
            {
                CreateDatabase();
            }
            else
            {
                CheckTables();
            }
            this.logger.WriteLog("Database is working and ready!");
        }

        /// <summary>
        /// Re-creates the database.
        /// </summary>
        public void ResetDatabase()
        {
            File.Delete(dbPath);
            CreateDatabase();
        }

        /// <summary>
        /// Execute a custom SQL query against the database.
        /// </summary>
        /// <param name="sql">The SQL query to execute</param>
        /// <returns></returns>
        public bool ExecuteSQL(string sql)
        {
            try
            {
                using (SQLiteConnection con = new SQLiteConnection(connString))
                {
                    con.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(sql, con))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                this.logger.WriteLog("Executed sql query against database!", LogType.DEBUG);
                this.logger.WriteLog("Query: {0}", sql.Replace("\n", ""), LogType.DEBUG);
                return true;
            }
            catch (Exception ex)
            {
                this.logger.WriteLog("Can't execute database command \"" + sql + "\"!", LogType.ERROR, ex);
                return false;
            }
        }

        /// <summary>
        /// Execute a custom SQL query against a tablee.
        /// </summary>
        /// <param name="table">The target table</param>
        /// <param name="data">The data to insert into the table</param>
        /// <param name="method">(optional) Either "insert" or "update".</param>
        /// <returns></returns>
        public bool ExecuteSQL(TableDefinition table, Dictionary<IColumn, dynamic> data, string method = "insert")
        {
            string sql = "";

            switch (method)
            {
                case "insert":
                    sql = "INSERT INTO {0} ({1}) VALUES ({2})";
                    break;
            }

            string columns = null;
            string values = null;
            foreach (KeyValuePair<IColumn, dynamic> column in data)
            {
                if (columns == null)
                {
                    columns = column.Key.Name;
                    values = "@" + column.Key.Name;
                }
                else
                {
                    columns += ", " + column.Key.Name;
                    values += ", @" + column.Key.Name;
                }
            }

            sql = string.Format(sql, table.Name, columns, values);

            try
            {
                using (SQLiteConnection con = new SQLiteConnection(connString))
                {
                    con.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(sql, con))
                    {
                        foreach (KeyValuePair<IColumn, dynamic> column in data)
                        {
                            cmd.Parameters.AddWithValue("@" + column.Key.Name, column.Value);
                        }

                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.WriteLog("Can't execute database command \"" + sql + "\"!", ex);
                return false;
            }
        }

        /// <summary>
        /// Execute a SQL query which expects results.
        /// </summary>
        /// <param name="sql">The SQL query to execute</param>
        /// <param name="table">The table on which the query shall be executed</param>
        /// <returns></returns>
        public ResultSet SelectData(string sql, TableDefinition table)
        {
            string colName = "";
            try
            {
                using (SQLiteConnection con = new SQLiteConnection(connString))
                {
                    con.Open();

                    using (SQLiteCommand cmd = new SQLiteCommand(sql, con))
                    {
                        cmd.ExecuteNonQuery();

                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            List<RowData> rows = new List<RowData>();
                            while (reader.Read())
                            {
                                Dictionary<string, dynamic> columns = new Dictionary<string, dynamic>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    colName = reader.GetName(i);

                                    if (!reader.IsDBNull(i))
                                    {
                                        string value = reader[colName].ToString();
                                        switch (table.ColumnDefinitions.Find(x => x.Name == colName)?.ColumnType)
                                        {
                                            case ColumnType.BOOLEAN:
                                                if (int.TryParse(value, out int result))
                                                {
                                                    columns.Add(colName, result == 1);
                                                }
                                                break;
                                            case ColumnType.FLOAT:
                                                if (float.TryParse(value, out float bigNumber))
                                                {
                                                    columns.Add(colName, bigNumber);
                                                }
                                                break;
                                            case ColumnType.INTEGER:
                                                if (int.TryParse(value, out int number))
                                                {
                                                    columns.Add(colName, number);
                                                }
                                                break;
                                            case ColumnType.TEXT:
                                                columns.Add(colName, value.Replace("''", "\""));
                                                break;
                                            default:
                                                columns.Add(colName, reader.GetValue(i));
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        var test = table[colName].DefaultValue;
                                        columns.Add(colName, table[colName].DefaultValue);
                                    }
                                }
                                rows.Add(new RowData(columns));
                            }
                            return new ResultSet(rows);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.WriteLog("Unknown error while reading column {0}! (Select)", LogType.ERROR, ex, colName);
                return null;
            }

        }

        private string ParseString(string str)
        {
            return str != null ? str.Replace("'", "''") : "";
        }
        #endregion
    }
}
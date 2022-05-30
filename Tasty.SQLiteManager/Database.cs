using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Tasty.Logging;
using Tasty.SQLiteManager.Exceptions;
using Tasty.SQLiteManager.Table;
using Tasty.SQLiteManager.Table.Attributes;
using Tasty.SQLiteManager.Table.Column;
using Tasty.SQLiteManager.Table.ForeignKey;

namespace Tasty.SQLiteManager
{
    /// <summary>
    /// The heart of the SQLiteManager. Manages the database
    /// </summary>
    public class Database : IList<ITable>
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        private readonly List<ITable> tables = new List<ITable>();

        private readonly List<ChildTableDefinition> childTables = new List<ChildTableDefinition>();

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
        /// <returns>Returns a <see cref="TableDefinition{T}"/> or null.</returns>
        public ITable this[int index]
        {
            get => tables[index];
            set => tables[index] = value;
        }

        /// <summary>
        /// Gets or sets the table with the specified table name.
        /// </summary>
        /// <param name="tableName">The table with the specified name.</param>
        /// <returns>Returns a <see cref="TableDefinition{T}"/> or null.</returns>
        public ITable this[string tableName]
        {
            get => tables.Find(x => x.Name == tableName);
            set => tables[tables.IndexOf(tables.Find(x => x.Name == tableName))] = value;
        }

        /// <summary>
        /// Gets the table with the specified table type
        /// </summary>
        /// <param name="type">The type of the table</param>
        /// <returns>Returns a <see cref="TableDefinition{T}"/> or null.</returns>
        public ITable this[Type type]
        {
            get => tables.Find(x => x.TableType == type);
        }

        /// <summary>
        /// Adds a table
        /// </summary>
        /// <param name="item">The table to add</param>
        public void Add(ITable item)
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
        public bool Contains(ITable item)
        {
            return tables.Contains(item);
        }

        /// <summary>
        /// Copies all <see cref="ITable"/> objects into a new array.
        /// </summary>
        /// <param name="array">Copy of all table definitions</param>
        /// <param name="arrayIndex">Starting index from where the copying should begin</param>
        public void CopyTo(ITable[] array, int arrayIndex)
        {
            tables.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes a table.
        /// </summary>
        /// <param name="item">The <see cref="ITable"/> to remove</param>
        /// <returns></returns>
        public bool Remove(ITable item)
        {
            return tables.Remove(item);
        }

        /// <inheritdoc/>
        public IEnumerator<ITable> GetEnumerator()
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
        /// <param name="item">The <see cref="ITable"/> of which you want the index</param>
        /// <returns>The index of the specified table</returns>
        public int IndexOf(ITable item)
        {
            return tables.IndexOf(item);
        }

        /// <summary>
        /// Inserts a new table
        /// </summary>
        /// <param name="index">The index where you want to insert the table</param>
        /// <param name="item">The <see cref="ITable"/> to insert</param>
        public void Insert(int index, ITable item)
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
        private readonly Logger logger;

        private readonly string dbPath;
        private readonly string connString;

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

        public List<ChildTableDefinition> ChildTables => childTables;

        /// <summary>
        /// Initializes the database.
        /// </summary>
        /// <param name="dbPath">The path to your SQLite database</param>
        /// <param name="tables">A list of <see cref="ITable"/> which represent the database structure</param>
        /// <param name="logger">(optional) A custom <see cref="Logger"/> to redirect output to another file</param>
        /// <param name="forceInitialize">(optional) Forces the re-initialization of the <see cref="Database"/> singleton object</param>
        public static void Initialize(string dbPath, List<ITable> tables, Logger logger = null, bool forceInitialize = false)
        {
            if (instance == null || forceInitialize)
            {
                if (tables == null)
                {
                    tables = new List<ITable>();
                }
                instance = new Database(dbPath, tables, logger);
                instance.ClearCacheTables();
            }
        }

        /// <summary>
        /// Initializes the database, and automatically create tables from classes.
        /// <para></para>
        /// Classes need the [<see cref="SqliteTable"/>] attribute in order to be detected.
        /// </summary>
        /// <param name="dbPath">The path to your SQLite database</param>
        /// <param name="logger">(optional) A custom <see cref="Logger"/> to redirect output to another file</param>
        /// <param name="forceInitialize">(optional) Forces the re-initialization of the <see cref="Database"/> singleton object</param>
        public static void Initialize(string dbPath, Logger logger = null, bool forceInitialize = false)
        {
            if (instance == null || forceInitialize)
            {
                instance = new Database(dbPath, logger);
                instance.ClearCacheTables();
            }
        }

        private Database(string dbPath, List<ITable> tables, Logger logger = null)
        {
            this.logger = logger;
            this.dbPath = dbPath;
            connString = string.Format("Data Source={0};Version=3;", dbPath);

            // Check foreign key properties and create mapping tables
            #region Create mapping tables if needed
            List<ChildTableData> childTables = new List<ChildTableData>();
            foreach (ITable table in tables)
            {
                if (table.ForeignKeyData.Count == 0)
                {
                    continue;
                }
                IColumn primaryKey = table.GetPrimaryKeyColumn();
                if (primaryKey == null)
                {
                    throw new AutoTableException(string.Format("Cannot create child tables for table {0} when no primary key is set!", table.Name));
                }

                ForeignKeyData rootKeyData = new ForeignKeyData(primaryKey.Name,
                    string.Format("{0}_{1}", Util.GetSingular(table.Name).ToUpper(), primaryKey.Name), primaryKey.PropertyInfo.PropertyType,
                    table.Name);

                foreach (ForeignKeyData foreignKeyData in table.ForeignKeyData)
                {
                    if (!foreignKeyData.IsList)
                    {
                        continue;
                    }
                    ChildTableData existing = childTables.FirstOrDefault(x => x.TableName == foreignKeyData.ChildTableName);

                    if (existing != null)
                    {
                        if (!existing.ForeignKeyData.Contains(foreignKeyData))
                        {
                            existing.ForeignKeyData.Add(foreignKeyData);
                        }

                        if (!existing.ForeignKeyData.Contains(rootKeyData))
                        {
                            existing.ForeignKeyData.Add(rootKeyData);
                        }
                    }
                    else
                    {
                        existing = new ChildTableData(foreignKeyData, rootKeyData);
                        childTables.Add(existing);
                    }
                }
            }

            this.tables = tables;

            foreach (ITable table in tables)
            {
                table.SetOneToOneRelationData(tables);
            }

            foreach (ChildTableData childTableData in childTables)
            {
                ChildTableDefinition childTable = new ChildTableDefinition(childTableData);
                this.childTables.Add(childTable);
                foreach (ForeignKeyData foreignKeyData in childTableData.ForeignKeyData)
                {
                    this.FirstOrDefault(x => x.Name == foreignKeyData.ParentTableName)?.ChildTables.Add(childTable);
                }
            }
            #endregion

            CheckDatabase();
        }

        private Database(string dbPath, Logger logger = null) : this(dbPath, GetTablesFromAssemblies(), logger)
        {

        }

        /// <summary>
        /// Returns the <see cref="TableDefinition{T}"/> for the given type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public TableDefinition<T> GetTable<T>()
        {
            return (TableDefinition<T>)this[typeof(T)];
        }

        public ChildTableDefinition GetChildTable(string tableName)
        {
            return childTables.FirstOrDefault(x => x.Name == tableName);
        }

        /// <summary>
        /// Converts all tables, columns and rows to a SQL query string. Useful for exporting the database as a text
        /// </summary>
        /// <returns></returns>
        public string ExportToSQL(bool includeData)
        {
            StringBuilder tableSql = new StringBuilder();
            StringBuilder dataSql = new StringBuilder();
            //TODO: Iterate through all tables, get all rows of each table and "convert" those entries into INSERT statements
            //this.logger?.WriteLog("Starting database export...", Color.Magenta);
            ExportTablesToSql(tables.OrderBy(x => x.ForeignKeyData.Count), "table", tableSql, dataSql, includeData);
            ExportTablesToSql(childTables, "child table", tableSql, dataSql, includeData);

            StringBuilder sql = new StringBuilder();
            sql.Append(tableSql.ToString());
            if (dataSql.Length > 0)
            {
                sql.Append(dataSql.ToString());
            }

            return sql.ToString();
        }

        private void ExportTablesToSql(IEnumerable<ITableBase> tables, string tableType,
            StringBuilder tableSql, StringBuilder dataSql, bool includeData)
        {
            foreach (ITableBase table in tables)
            {
                string sql = table.ToString();
                /*if (table is IChildTable childTable)
                {
                    sql = childTable.GetQueryData(false);
                    foreignKeySql.Append(string.Format(""));
                }
                else
                {
                    sql = table.ToString();
                }*/


                #region Create DROP TABLE + CREATE TABLE
                tableSql.Append(string.Format("-- Recreate {0} '{1}'\n" +
                    "DROP TABLE IF EXISTS \"{1}\";\n{2};\n\n", tableType, table.Name, sql));
                #endregion

                if (includeData)
                {
                    #region Iterate through all rows and create INSERT
                    ResultSet result = table.Select();
                    if (result.Count > 0)
                    {
                        #region Create first part of INSERT
                        StringBuilder columnSql = new StringBuilder();
                        foreach (IColumn column in table)
                        {
                            if (columnSql.Length == 0)
                            {
                                columnSql.Append(column.Name);
                            }
                            else
                            {
                                columnSql.Append(", " + column.Name);
                            }
                        }

                        dataSql.Append(string.Format("-- Fill child table '{0}' with data\n" +
                            "INSERT INTO \"{0}\" ({1}) VALUES", table.Name, columnSql));
                        #endregion

                        #region Iterate every row, read column data and add to INSERT
                        foreach (RowData row in result)
                        {
                            StringBuilder rowSql = new StringBuilder();
                            foreach (IColumn resultColumn in table)
                            {
                                if (row.Columns.TryGetValue(resultColumn.Name, out dynamic value))
                                {
                                    if (rowSql.Length == 0)
                                    {
                                        rowSql.Append(resultColumn.ParseColumnValue(value));
                                    }
                                    else
                                    {
                                        rowSql.Append(", " + resultColumn.ParseColumnValue(value));
                                    }
                                }
                            }
                            dataSql.Append(string.Format("\n\t({0})", rowSql));
                        }
                        dataSql.Append(";\n");
                        #endregion
                    }
                    #endregion
                }
            }
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
            ExecuteSQL(fileContent);
            return null;
        }

        #region Database
        private void CreateDatabase()
        {
            if (FileExists)
                File.Delete(dbPath);

            SQLiteConnection.CreateFile(dbPath);

            #region Initializing database
            foreach (ITable table in this)
            {
                ExecuteSQL(table.ToString());
            }

            foreach (ITableBase childTable in childTables)
            {
                ExecuteSQL(childTable.ToString());
            }
            #endregion

            this.logger?.WriteLog("Database created!");
        }

        private void CheckTables()
        {
            this.logger?.WriteLog("Scanning database for changes...");

            CheckTables(tables.OfType<ITableBase>());
            CheckTables(childTables, "child table");
        }

        private void CheckTables(IEnumerable<ITableBase> tables, string tableType = "table")
        {
            foreach (ITableBase table in tables)
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
                                this.logger?.WriteLog("Missing column in {0} \"{1}\" detected! (Column: {2}; SQL: {3})",
                                    LogType.WARNING, tableType, table.Name, column.Name, column.ToString());

                                ExecuteSQL(string.Format("ALTER TABLE {0} ADD COLUMN {1}",
                                    table.Name, column.ToString()));
                                this.logger?.WriteLog("Missing column added!");
                            }
                            else
                            {
                                this.logger?.WriteLog("Unable to add a unique column to {0} \"{1}\" with ALTER TABLE! (Column: {2}; SQL: {3})",
                                    LogType.ERROR, tableType, table.Name, column.Name, column.ToString());
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
                        this.logger?.WriteLog("Leftover columns in {0} \"{1}\" detected! (Columns: {2})",
                            LogType.WARNING, tableType, table.Name, string.Join(", ", removableColumns.ToArray()));

                        bool columnsRemoved = false;
                        bool consoleWindowAttached = GetConsoleWindow() != IntPtr.Zero;
                        ConsoleKey pressedKey = ConsoleKey.Y;

                        if (consoleWindowAttached)
                        {
                            Console.WriteLine("Would you like to remove these columns? (y/N)");
                            pressedKey = Console.ReadKey().Key;
                        }

                        if (pressedKey == ConsoleKey.Y)
                        {
                            if (consoleWindowAttached)
                            {
                                Console.WriteLine("\n\t\tWARNING: Any data stored in these columns will be lost forever!\n");
                                Console.WriteLine("Would you like to proceed? (y/N)");
                                pressedKey = Console.ReadKey().Key;
                            }
                            if (pressedKey == ConsoleKey.Y)
                            {
                                Console.WriteLine();
                                RebuildTable(table, tableType, consoleWindowAttached);

                                columnsRemoved = true;
                            }
                        }

                        if (columnsRemoved)
                        {
                            this.logger?.WriteLog("Columns removed!");
                        }
                        else
                        {
                            this.logger?.WriteLog("Columns kept!");
                        }
                    }
                    #endregion

                    #region Check if foreign keys have changed
                    string tableCreateStatementSql = "SELECT sql FROM sqlite_master WHERE name='" + table.Name + "';";

                    RowData tableData = SelectData(tableCreateStatementSql, table).FirstOrDefault();
                    if (tableData != null)
                    {
                        string dbSql = tableData[0];
                        if (table.ToString().Replace("IF NOT EXISTS ", "") != dbSql)
                        {
                            RebuildTable(table, tableType, false);
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
            foreach (ITable table in this)
            {
                if (table is ICacheTable cacheTable)
                {
                    string[] data = cacheTable.ClearCache();
                    ExecuteSQL(data[0]);
                    this.logger?.WriteLog(data[1]);
                }
            }
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
            this.logger?.WriteLog("Database is working and ready!");
        }

        /// <summary>
        /// Re-create all tables.
        /// </summary>
        public void DropDatabase()
        {
            try
            {
                string sql = ExportToSQL(false);
                ExecuteSQL(sql);
            }
            catch (Exception ex)
            {
                this.logger?.WriteLog("Couldn't drop database, re-creating file...", LogType.WARNING, ex);
                ResetDatabase();
            }
        }

        /// <summary>
        /// Re-creates the database file.
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

                this.logger?.WriteLog("Executed sql query against database!", LogType.DEBUG);
                this.logger?.WriteLog("Query: {0}", sql.Replace("\n", ""), LogType.DEBUG);
                return true;
            }
            catch (Exception ex)
            {
                string[] messageLines = ex.Message.Replace("\r\n", "\n").Split('\n');
                
                switch (messageLines[0])
                {
                    case "constraint failed":
                        string rowName = messageLines[1].Replace("UNIQUE constraint failed: ", "");
                        this.logger?.WriteLog("Trying to insert new entry with existing " + rowName + 
                            "!\r\nSQLite error: " + ex.Message, LogType.ERROR, ex);
                        break;
                    default:
                        this.logger?.WriteLog("Can't execute database command \"" + sql + "\"!\r\nSQLite error: " + ex.Message, LogType.ERROR, ex);
                        break;
                }
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
        public bool ExecuteSQL(ITable table, Dictionary<IColumn, dynamic> data, string method = "insert")
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
                this.logger?.WriteLog("Can't execute database command \"" + sql + "\"!", ex);
                return false;
            }
        }

        /// <summary>
        /// Execute a SQL query which expects results.
        /// </summary>
        /// <param name="sql">The SQL query to execute</param>
        /// <param name="table">The table on which the query shall be executed</param>
        /// <returns></returns>
        public ResultSet SelectData(string sql, ITableBase table)
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
                this.logger?.WriteLog("Unknown error while reading column {0}! (Select)", LogType.ERROR, ex, colName);
                return null;
            }

        }

        private string ParseString(string str)
        {
            return str != null ? str.Replace("'", "''") : "";
        }

        private void RebuildTable(ITableBase table, string tableType, bool consoleWindowAttached)
        {
            #region Copy data
            this.logger?.WriteLog("(1/3) Copying data...");

            var result = SelectData("SELECT * FROM " + table.Name, table);
            Dictionary<IColumn, dynamic>[] dataBackup = new Dictionary<IColumn, dynamic>[result.Count];
            for (int i = 0; i < result.Count; i++)
            {
                if (consoleWindowAttached)
                {
                    Console.CursorLeft = 0;
                    Console.Write("                                                                     ");
                    Console.CursorLeft = 0;
                    Console.Write("Process: {0}/{1}", i + 1, result.Count);
                }
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
            this.logger?.WriteLog("(2/3) Rebuilding {0}...", tableType);
            ExecuteSQL("DROP TABLE " + table.Name + ";");
            ExecuteSQL(table.ToString());
            #endregion

            #region Restoring data
            this.logger?.WriteLog("(3/3) Restoring data...");
            ExecuteSQL(table.GenerateBulkInsert(dataBackup));
            #endregion
        }

        private static List<ITable> GetTablesFromAssemblies()
        {
            List<ITable> tables = new List<ITable>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type tableType in assembly.GetTypes().Where(x => Attribute.IsDefined(x, typeof(SqliteTable))))
                {
                    SqliteTable sqliteTableAttribute = (SqliteTable)tableType.GetCustomAttribute(typeof(SqliteTable));

                    string tableName = sqliteTableAttribute.AutoName ?
                        Util.GetTableName(tableType.Name) : sqliteTableAttribute.TableName;

                    // Create generic ITable object from tableType property
                    Type tableDefinitionType = Util.MakeGenericTableDefinition(tableType);
                    ConstructorInfo ctor = tableDefinitionType.GetConstructors()
                        .FirstOrDefault(x => Attribute.IsDefined(x, typeof(SqliteConstructor)));
                    if (ctor != null)
                    {
                        tables.Add((ITable)ctor.Invoke(new object[] { tableName }));
                    }
                    else
                    {
                        throw new AutoTableException(string.Format("Error creating tables from class {0}!\nMissing required attributes.", tableName));
                    }
                }
            }

            return tables;
        }
        #endregion
    }
}
﻿using System;
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
        private static extern IntPtr GetConsoleWindow();

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

        private readonly string ident;

        private byte[] passwordHash;

        internal string Ident => ident;

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

        internal List<ITable> Tables => tables;

        /// <summary>
        /// Initializes the default database, which can be called via "<c><see cref="Database"/>.Instance</c>".
        /// </summary>
        /// <param name="dbPath">The path to your SQLite database</param>
        /// <param name="tables">A list of <see cref="ITable"/> which represent the database structure</param>
        /// <param name="ident">(optional) A unique identifier for this database instance</param>
        /// <param name="password">(optional) Sets a password for this database</param>
        /// <param name="logger">(optional) A custom <see cref="Logger"/> to redirect output to another file</param>
        /// <param name="forceInitialize">(optional) Forces the re-initialization of the <see cref="Database"/> singleton object</param>
        public static void Initialize(string dbPath, List<ITable> tables, string password = null, Logger logger = null, bool forceInitialize = false)
        {
            if (instance == null || forceInitialize)
            {
                if (tables == null)
                {
                    tables = new List<ITable>();
                }

                instance = InstanceManager.AddInstance(new Database(dbPath, tables, null, password, logger));
            }
        }

        /// <summary>
        /// Initializes the default database, and automatically create tables from classes. The created database instance can be called via "<c><see cref="Database"/>.Instance</c>".
        /// <para>
        /// Classes need the [<see cref="SqliteTable"/>] attribute in order to be detected.
        /// </para>
        /// Also when handling multiple databases with this library, make sure to assign your tables to the correct database with the [<see cref="SqliteUseDatabase"/>(<c>DB_PATH</c>)] attribute!
        /// </summary>
        /// <param name="dbPath">The path to your SQLite database</param>
        /// <param name="ident">(optional) A unique identifier for this database instance</param>
        /// <param name="password">(optional) Sets a password for this database. NULL = no password</param>
        /// <param name="logger">(optional) A custom <see cref="Logger"/> to redirect output to another file</param>
        /// <param name="forceInitialize">(optional) Forces the re-initialization of the <see cref="Database"/> singleton object</param>
        public static void Initialize(string dbPath, string password = null, Logger logger = null, bool forceInitialize = false)
        {
            if (instance == null || forceInitialize)
            {
                instance = InstanceManager.AddInstance(new Database(dbPath, null, password, logger));
            }
        }

        /// <summary>
        /// Create a new instance of a database with an identifier and initialize it. This instance needs to be handled by the user, thus returning a <see cref="Database"/> object.
        /// </summary>
        /// <param name="dbPath">The path to your SQLite database</param>
        /// <param name="ident">A unique identifier for this database instance. This field can neither be NULL nor an empty string!</param>
        /// <param name="password">(optional) Sets a password for this database. NULL = no password</param>
        /// <param name="logger">(optional) A custom <see cref="Logger"/> to redirect output to another file</param>
        /// <returns>Returns a <see cref="Database"/> instance separate from the "<c><see cref="Database"/>.Instance</c>" instance.</returns>
        public static Database CreateInstance(string dbPath, string ident, string password = null, Logger logger = null)
        {
            if (InstanceManager.GetInstance(ident, true) is Database existingInstance)
            {
                return existingInstance;
            }

            return InstanceManager.AddInstance(new Database(dbPath, ident, password, logger));
        }

        private Database(string dbPath, List<ITable> tables, string ident, string password = null, Logger logger = null)
        {
            if (!string.IsNullOrEmpty(password))
            {
                passwordHash = password.ComputeSHA256Hash();
            }

            this.ident = ident;
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
                        ForeignKeyData realRootData = null;
                        bool isSameTableRelation = false;
                        if (foreignKeyData.IsManyToMany)
                        {
                            Type tableType = table.TableType;
                            foreach (PropertyInfo propertyInfo in tableType.GetProperties().Where(x => Attribute.IsDefined(x, typeof(SqliteForeignKey))))
                            {
                                SqliteForeignKey foreignKeyAttribute = propertyInfo.GetCustomAttribute<SqliteForeignKey>();
                                if (foreignKeyAttribute.Data.ChildTableName == foreignKeyData.ChildTableName)
                                {
                                    realRootData = new ForeignKeyData(foreignKeyData.ChildTableName, true);
                                    isSameTableRelation = rootKeyData.ForeignKeyName == foreignKeyData.ForeignKeyName;
                                    string targetKey = isSameTableRelation ? string.Format("{0}_{1}", Util.GetColumnName(Util.GetSingular(propertyInfo.Name)).ToUpper(), rootKeyData.ParentKeyName) :
                                        string.Format("{0}_{1}", Util.GetSingular(table.Name).ToUpper(), primaryKey.Name);

                                    realRootData.SetManyToManyData(rootKeyData.ParentKeyName, propertyInfo, targetKey);

                                    if (rootKeyData.ForeignKeyName == foreignKeyData.ForeignKeyName)
                                    {
                                        foreignKeyData.ManyToManyTargetKeyName = targetKey;
                                    }
                                    else
                                    {
                                        realRootData.ChildTableName = foreignKeyData.ChildTableName;
                                        realRootData.ParentTableName = rootKeyData.ParentTableName;
                                    }
                                    break;
                                }
                            }
                        }
                        else
                        {
                            realRootData = rootKeyData;
                        }

                        if (realRootData == null)
                        {
                            throw new UnknownSqliteManagerException("An error occurred while preparing relationship table data.");
                        }

                        if (foreignKeyData.ForeignKeyName == realRootData.ForeignKeyName)
                        {
                            realRootData.ForeignKeyName += "_1";
                            foreignKeyData.ForeignKeyName += "_2";
                        }
                        existing = new ChildTableData(foreignKeyData, realRootData, isSameTableRelation);
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
                    ITable target = this.FirstOrDefault(x => x.Name == foreignKeyData.ParentTableName);
                    if (target != null)
                    {
                        if (foreignKeyData.IsManyToMany)
                        {
                            Type tableType = target.TableType;
                            foreach (PropertyInfo propertyInfo in tableType.GetProperties().Where(x => Attribute.IsDefined(x, typeof(SqliteForeignKey))))
                            {
                                SqliteForeignKey foreignKeyAttribute = propertyInfo.GetCustomAttribute<SqliteForeignKey>();
                                if (foreignKeyAttribute.Data.ChildTableName == foreignKeyData.ChildTableName)
                                {
                                    ForeignKeyData childForeignKeyData = new ForeignKeyData(foreignKeyData.ChildTableName, true);
                                    string primaryKeyName = target.GetPrimaryKeyColumn().Name;
                                    childForeignKeyData.SetManyToManyData(primaryKeyName, propertyInfo,
                                        string.Format("{0}_{1}", Util.GetColumnName(Util.GetSingular(propertyInfo.Name)).ToUpper(), primaryKeyName));
                                    target.ForeignKeyData.Add(childForeignKeyData);
                                    break;
                                }
                            }
                        }
                        target.ChildTables.Add(childTable);
                    }

                }
            }
            #endregion

            CheckDatabase();
        }

        private Database(string dbPath, string ident, string password = null, Logger logger = null) : this(dbPath, GetTablesFromAssemblies(ident), ident, password, logger)
        {

        }


        /// <summary>
        /// Returns the <see cref="TableDefinition{T}"/> for the given type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static TableDefinition<T> GetTable<T>()
        {
            return (TableDefinition<T>)InstanceManager.GetTableFromType(typeof(T));
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
                    ResultSet result = table.Select(false);
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
                                        rowSql.Append(resultColumn.ParseToDatabaseValue(value));
                                    }
                                    else
                                    {
                                        rowSql.Append(", " + resultColumn.ParseToDatabaseValue(value));
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
        /// Add, change or remove the password for this database
        /// </summary>
        /// <param name="currentPassword">The currently set password</param>
        /// <param name="newPassword">The new password</param>
        public void ChangePassword(string currentPassword, string newPassword)
        {
            if (passwordHash == null && string.IsNullOrEmpty(newPassword) ||
                passwordHash != null && passwordHash.CompareSHA256Hashes(newPassword))
            {
                return;
            }

            if (passwordHash != null && !passwordHash.CompareSHA256Hashes(currentPassword))
            {
                throw new DatabaseAccessException("The provided current password does not match the password set for the database!");
            }

            SQLiteConnection con = new SQLiteConnection(connString);

            if (passwordHash != null)
            {
                con.SetPassword(passwordHash);
            }

            using (con)
            {
                if (!string.IsNullOrEmpty(newPassword))
                {
                    byte[] newPasswordHash = newPassword.ComputeSHA256Hash();
                    con.ChangePassword(newPasswordHash);
                    passwordHash = newPasswordHash;
                }
                else
                {
                    con.ChangePassword(string.Empty);
                    passwordHash = null;
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

            using (SQLiteConnection con = GetConnection())
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
                using (SQLiteConnection con = GetConnection())
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
                using (SQLiteConnection con = GetConnection())
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
                using (SQLiteConnection con = GetConnection())
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
                                        IColumn column = table.ColumnDefinitions.Find(x => x.Name == colName);
                                        if (column == null)
                                        {
                                            columns.Add(colName, reader.GetValue(i));
                                        }
                                        else if (string.IsNullOrEmpty(column.StringFormatter))
                                        {
                                            switch (column?.ColumnType)
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
                                                    column.ParseToDatabaseValue(value.Replace("''", "\""));
                                                    columns.Add(colName, value.Replace("''", "\""));
                                                    break;
                                                default:
                                                    columns.Add(colName, reader.GetValue(i));
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            columns.Add(colName, column.ParseFromDatabaseValue(value));
                                        }
                                    }
                                    else if (table.ColumnExists(colName))
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

        private SQLiteConnection GetConnection()
        {
            SQLiteConnection con = new SQLiteConnection(connString);

            if (passwordHash != null)
            {
                con.SetPassword(passwordHash);
            }

            return con;
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
                    IColumn targetColumn = table[data.Key];

                    if (targetColumn != null)
                    {
                        dataCopy.Add(targetColumn, data.Value);
                    }
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

        private static List<ITable> GetTablesFromAssemblies(string ident)
        {
            List<ITable> tables = new List<ITable>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type tableType in assembly.GetTypes().Where(x => Attribute.IsDefined(x, typeof(SqliteTable))))
                {
                    if (Attribute.IsDefined(tableType, typeof(SqliteUseDatabase)))
                    {
                        SqliteUseDatabase sqliteUseDatabaseAttribute = (SqliteUseDatabase)tableType.GetCustomAttribute(typeof(SqliteUseDatabase));

                        if (sqliteUseDatabaseAttribute.Ident != ident)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(ident))
                        {
                            continue;
                        }
                    }

                    SqliteTable sqliteTableAttribute = (SqliteTable)tableType.GetCustomAttribute(typeof(SqliteTable));

                    string tableName = sqliteTableAttribute.AutoName ?
                        Util.GetTableName(tableType.Name) : sqliteTableAttribute.TableName;

                    // Create generic ITable object from tableType property
                    Type tableDefinitionType = Util.MakeGenericTableDefinition(tableType);
                    ConstructorInfo ctor = tableDefinitionType.GetConstructors()
                        .FirstOrDefault(x => Attribute.IsDefined(x, typeof(SqliteConstructor)));
                    if (ctor != null)
                    {
                        ITable table = (ITable)ctor.Invoke(new object[] { tableName });
                        table._DatabaseIdent = ident;
                        tables.Add(table);
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
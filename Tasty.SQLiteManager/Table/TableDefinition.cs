using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Tasty.SQLiteManager.Table.Attributes;
using Tasty.SQLiteManager.Table.Column;
using Tasty.SQLiteManager.Table.Conditions;

namespace Tasty.SQLiteManager.Table
{
    /// <summary>
    /// Used to define a table with columns for the database
    /// </summary>
    public class TableDefinition<T> : DefinitionBase, ITable
    {
        private Type tableType;

        /// <summary>
        /// </summary>
        protected List<IColumn> columns = new List<IColumn>();
        /// <summary>
        /// </summary>
        protected List<ForeignKeyDefinition> foreignKeys = new List<ForeignKeyDefinition>();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public List<IColumn> ColumnDefinitions => columns;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public List<ForeignKeyDefinition> ForeignKeys => foreignKeys;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Type TableType => tableType;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="columnName"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public IColumn this[string columnName]
        {
            get => columns.FirstOrDefault(x => x.Name == columnName);
        }

        /// <summary>
        /// Define a new table with the specified columns.
        /// </summary>
        /// <param name="name">The name of the table</param>
        public TableDefinition(string name) : this(name, GetColumnsFromClass(typeof(T)))
        {
            tableType = typeof(T);
        }

        /// <summary>
        /// Define a new table with the specified columns.
        /// </summary>
        /// <param name="name">The name of the table</param>
        /// <param name="columns">A list of <see cref="ColumnDefinition{T}"/> objects for this table.</param>
        public TableDefinition(string name, List<IColumn> columns)
        {
            this.columns = columns;
            base.name = name;
        }

        /// <summary>
        /// Define a new table with the specified columns.
        /// </summary>
        /// <param name="name">The name of the table</param>
        /// <param name="columns">A list of <see cref="ColumnDefinition{T}"/> objects for this table.</param>
        /// <param name="foreignKeys">A list of foreign key definitions.</param>
        public TableDefinition(string name, List<IColumn> columns, List<ForeignKeyDefinition> foreignKeys) : this(name, columns)
        {
            this.foreignKeys = foreignKeys;
        }

        #region IList implementation
        /// <summary>
        /// Returns the amount of columns in this table.
        /// </summary>
        public int Count => columns.Count;

        /// <inheritdoc/>
        public bool IsReadOnly => true;

        /// <summary>
        /// Search for a column by index
        /// </summary>
        /// <param name="index">The index of the column</param>
        /// <returns></returns>
        public IColumn this[int index] { get => columns[index]; set => columns[index] = value; }

        /// <summary>
        /// Add a column to the table
        /// </summary>
        /// <param name="item"></param>
        public void Add(IColumn item)
        {
            columns.Add(item);
        }

        /// <summary>
        /// Clear all columns in this table
        /// </summary>
        public void Clear()
        {
            columns.Clear();
        }

        /// <summary>
        /// Check if a column exists in this table
        /// </summary>
        /// <param name="item">The <see cref="ColumnDefinition{T}"/> to search</param>
        /// <returns></returns>
        public bool Contains(IColumn item)
        {
            return columns.Contains(item);
        }

        /// <summary>
        /// Copies all <see cref="ColumnDefinition{T}"/> objects into a new array.
        /// </summary>
        /// <param name="array">Copy of all column definitions</param>
        /// <param name="arrayIndex">Starting index from where the copying should begin</param>
        public void CopyTo(IColumn[] array, int arrayIndex)
        {
            columns.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes a column from this table.
        /// </summary>
        /// <param name="item">The <see cref="ColumnDefinition{T}"/> to remove</param>
        /// <returns></returns>
        public bool Remove(IColumn item)
        {
            return columns.Remove(item);
        }

        /// <inheritdoc/>
        public IEnumerator<IColumn> GetEnumerator()
        {
            return columns.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return columns.GetEnumerator();
        }

        /// <summary>
        /// Returns the index of the specified column.
        /// </summary>
        /// <param name="item">The <see cref="ColumnDefinition{T}"/> of which you want the index</param>
        /// <returns>The index of the specified column</returns>
        public int IndexOf(IColumn item)
        {
            return columns.IndexOf(item);
        }

        /// <summary>
        /// Inserts a new column into the table
        /// </summary>
        /// <param name="index">The index where you want to insert the column</param>
        /// <param name="item">The <see cref="ColumnDefinition{T}"/> to insert</param>
        public void Insert(int index, IColumn item)
        {
            columns.Insert(index, item);
        }

        /// <summary>
        /// Remove a column at the specified index.
        /// </summary>
        /// <param name="index">The index of the column you want to remove</param>
        public void RemoveAt(int index)
        {
            columns.RemoveAt(index);
        }
        #endregion

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="target"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public bool ColumnExists(IColumn target)
        {
            return columns.Exists(x => x.Equals(target));
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="colName"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public bool ColumnExists(string colName)
        {
            return columns.Exists(x => x.Name.Equals(colName));
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public bool TableExists()
        {
            return Query("SELECT name FROM sqlite_master WHERE type='table' AND name='" + Name + "';", true).Count > 0;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="command"><inheritdoc/></param>
        /// <param name="awaitData"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public ResultSet Query(string command, bool awaitData)
        {
            if (awaitData)
            {
                return Database.Instance.SelectData(command, this);
            }
            else
            {
                Database.Instance.ExecuteSQL(string.Format(command, this.Name));
                return null;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="conditions"><inheritdoc/></param>
        public ResultSet Select(params Condition[] conditions)
        {
            string conditionParameter = ParseConditions(conditions);
            string sql;

            if (!string.IsNullOrWhiteSpace(conditionParameter))
            {
                sql = string.Format("SELECT * FROM {0} WHERE {1};", name, conditionParameter);
            }
            else
            {
                sql = string.Format("SELECT * FROM {0};", name);
            }
            return Database.Instance.SelectData(sql, this);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="columns"><inheritdoc/></param>
        /// <param name="conditions"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public ResultSet Select(List<IColumn> columns, params Condition[] conditions)
        {
            return Select(columns, false, conditions);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="columns"><inheritdoc/></param>
        /// <param name="excludeColumns"><inheritdoc/></param>
        /// <param name="conditions"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public ResultSet Select(List<IColumn> columns, bool excludeColumns, params Condition[] conditions)
        {
            string conditionParameter = ParseConditions(conditions);
            string selectParameter = "";
            string sql;

            if (!excludeColumns)
            {
                foreach (IColumn column in columns)
                {
                    if (string.IsNullOrEmpty(selectParameter))
                    {
                        selectParameter = column.Name;
                    }
                    else
                    {
                        selectParameter += ", " + column.Name;
                    }
                }
            }
            else
            {
                foreach (IColumn column in this)
                {
                    if (!columns.Contains(column))
                    {
                        if (string.IsNullOrEmpty(selectParameter))
                        {
                            selectParameter = column.Name;
                        }
                        else
                        {
                            selectParameter += ", " + column.Name;
                        }
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(selectParameter))
            {
                return Select(conditions);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(conditionParameter))
                {
                    sql = string.Format("SELECT {0} FROM {1} WHERE {2};", selectParameter, name, conditionParameter);
                }
                else
                {
                    sql = string.Format("SELECT {0} FROM {1};", selectParameter, name);
                }
                return Database.Instance.SelectData(sql, this);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="data"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        /// <exception cref="MissingRequiredColumnsException"><inheritdoc/></exception>
        public bool Insert(Dictionary<IColumn, dynamic> data)
        {
            return Database.Instance.ExecuteSQL(GenerateInsertSQL(data, false));
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="data"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        /// <exception cref="MissingRequiredColumnsException"><inheritdoc/></exception>
        public bool Replace(Dictionary<IColumn, dynamic> data)
        {
            return Database.Instance.ExecuteSQL(GenerateInsertSQL(data, true));
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="data"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        /// <exception cref="MissingRequiredColumnsException"><inheritdoc/></exception>
        public int Insert_GetIndex(Dictionary<IColumn, dynamic> data)
        {
            if (Database.Instance.ExecuteSQL(GenerateInsertSQL(data, false)))
            {
                string query = "SELECT ID FROM '" + Name + "' WHERE ";
                string conditions = "";
                foreach (var whereData in data)
                {
                    if (!string.IsNullOrEmpty(conditions))
                    {
                        conditions += string.Format(" AND {0} == {1}", whereData.Key.Name, whereData.Key.ParseColumnValue(whereData.Value));
                    }
                    else
                    {
                        conditions = string.Format("{0} == {1}", whereData.Key.Name, whereData.Key.ParseColumnValue(whereData.Value));
                    }
                }

                var result = Database.Instance.SelectData(query + conditions, this);
                if (result.ColumnExists("ID"))
                {
                    return result[0].Columns["ID"];
                }
                else
                {
                    return -2;
                }
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="data"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public string GenerateBulkInsert(Dictionary<IColumn, dynamic>[] data)
        {
            string sql = "BEGIN TRANSACTION;\n";
            foreach (Dictionary<IColumn, dynamic> row in data)
            {
                sql += GenerateInsertSQL(row, false) + "\n";
            }
            sql += "COMMIT;";

            return sql;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="data"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public string GenerateBulkUpdate(Dictionary<IColumn, dynamic>[] data)
        {
            string sql = "BEGIN TRANSACTION;\n";
            foreach (Dictionary<IColumn, dynamic> row in data)
            {
                sql += GenerateInsertSQL(row, true) + "\n";
            }
            sql += "COMMIT;";

            return sql;
        }

        private string GenerateInsertSQL(Dictionary<IColumn, dynamic> data, bool isReplace, params Condition[] conditions)
        {
            List<IColumn> requiredColumns = columns.FindAll(x => !x.PrimaryKey && x.NotNull);
            for (int i = 0; i < requiredColumns.Count; i++)
            {
                bool columnIncluded = data.ContainsKey(requiredColumns[i]);
                if (columnIncluded)
                {
                    requiredColumns.RemoveAt(i);
                    i--;
                }
            }
            if (requiredColumns.Count > 0)
            {
                throw new MissingRequiredColumnsException("You need to provide a value for all columns which have the \"NOT NULL\" modifier!", requiredColumns);
            }

            string rows = "";
            string values = "";
            string combinedRowAndValue = "";
            foreach (KeyValuePair<IColumn, dynamic> entry in data)
            {
                if (!isReplace)
                {
                    if (string.IsNullOrEmpty(rows))
                    {
                        rows = entry.Key.Name;

                        if (entry.Value != null)
                        {
                            values = entry.Key.ParseColumnValue(entry.Value);
                        }
                        else
                        {
                            values = "NULL";
                        }
                    }
                    else
                    {
                        rows += ", " + entry.Key.Name;
                        if (entry.Value != null)
                        {
                            values += ", " + entry.Key.ParseColumnValue(entry.Value);
                        }
                        else
                        {
                            values += ", NULL";
                        }
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(combinedRowAndValue))
                    {
                        combinedRowAndValue = string.Format("{0} = {1}", entry.Key.Name, entry.Value != null ? entry.Key.ParseColumnValue(entry.Value) : "NULL");
                    }
                    else
                    {
                        combinedRowAndValue += string.Format(", {0} = {1}", entry.Key.Name, entry.Value != null ? entry.Key.ParseColumnValue(entry.Value) : "NULL");
                    }
                }
            }

            // Add missing columns with their default values
            foreach (var column in columns)
            {
                if (!column.PrimaryKey && !data.ContainsKey(column) && column.DefaultValue != null)
                {
                    if (!isReplace)
                    {
                        if (string.IsNullOrEmpty(rows))
                        {
                            rows = column.Name;
                            values = column.ParseColumnValue(column.DefaultValue);
                        }
                        else
                        {
                            rows += ", " + column.Name;
                            values += ", " + column.ParseColumnValue(column.DefaultValue);
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(combinedRowAndValue))
                        {
                            combinedRowAndValue = string.Format("{0} = {1}", column.Name, column.ParseColumnValue(column.DefaultValue));
                        }
                        else
                        {
                            combinedRowAndValue = string.Format(", {0} = {1}", column.Name, column.ParseColumnValue(column.DefaultValue));
                        }
                    }
                }
            }

            string conditionParameter = ParseConditions(conditions);

            if (!isReplace)
            {
                return string.IsNullOrWhiteSpace(conditionParameter) ? string.Format("INSERT INTO {0} ({1}) VALUES ({2});", name, rows, values) :
                    string.Format("INSERT INTO {0} ({1}) VALUES ({2}) WHERE {3};", name, rows, values, conditionParameter);
            }
            else
            {
                return string.IsNullOrWhiteSpace(conditionParameter) ? string.Format("UPDATE {0} SET {1};", name, combinedRowAndValue) :
                    string.Format("UPDATE {0} SET {1} WHERE {2};", name, combinedRowAndValue, conditionParameter);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="data"><inheritdoc/></param>
        /// <param name="conditions"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public bool Update(Dictionary<IColumn, dynamic> data, params Condition[] conditions)
        {
            string setParameter = "";
            string conditionParameter = ParseConditions(conditions);
            foreach (KeyValuePair<IColumn, dynamic> entry in data)
            {
                if (string.IsNullOrWhiteSpace(setParameter))
                {
                    setParameter = string.Format("{0} = {1}", entry.Key.Name, entry.Key.ParseColumnValue(entry.Value));
                }
                else
                {
                    setParameter += string.Format(", {0} = {1}", entry.Key.Name, entry.Key.ParseColumnValue(entry.Value));
                }
            }

            string sql;
            if (!string.IsNullOrWhiteSpace(conditionParameter))
            {
                sql = string.Format("UPDATE {0} SET {1} WHERE {2};", name, setParameter, conditionParameter);
            }
            else
            {
                sql = string.Format("UPDATE {0} SET {1};", name, setParameter);
            }

            return Database.Instance.ExecuteSQL(sql);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public bool Delete()
        {
            return Delete(null);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="conditions"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public bool Delete(params Condition[] conditions)
        {
            string conditionParameter = conditions != null ? ParseConditions(conditions) : "";
            string sql;
            if (!string.IsNullOrWhiteSpace(conditionParameter))
            {
                sql = string.Format("DELETE FROM {0} WHERE {1};", name, conditionParameter);
            }
            else
            {
                sql = string.Format("DELETE FROM {0};", name);
            }

            return Database.Instance.ExecuteSQL(sql);
        }

        /// <summary>
        /// Returns the CREATE statement for this table.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("CREATE TABLE IF NOT EXISTS \"{0}\" ({1});", name, ParseColumns());
        }

        private string ParseConditions(params Condition[] conditions)
        {
            string conditionParameter = "";
            foreach (Condition condition in conditions)
            {
                if (string.IsNullOrWhiteSpace(conditionParameter))
                {
                    conditionParameter = condition.ToString();
                }
                else
                {
                    conditionParameter += ", " + condition.ToString();
                }
            }

            return conditionParameter;
        }

        private string ParseColumns()
        {
            string sql_inner = null;
            foreach (IColumn column in columns)
            {
                if (sql_inner == null)
                {
                    sql_inner = "\n\t" + column.ToString();
                }
                else
                {
                    sql_inner += ",\n\t" + column.ToString();
                }
            }

            foreach (ForeignKeyDefinition foreignKey in foreignKeys)
            {
                sql_inner += ",\n\t" + foreignKey.ToString();
            }

            return sql_inner;
        }

        private static List<IColumn> GetColumnsFromClass(Type target)
        {
            List<IColumn> columns = new List<IColumn>();

            foreach (PropertyInfo property in target.GetProperties())
            {
                if (!Attribute.IsDefined(property, typeof(SqliteIgnore)))
                {
                    ColumnMode columnMode;

                    if (Attribute.IsDefined(property, typeof(SqlitePrimaryKey)))
                    {
                        columnMode = ColumnMode.PRIMARY_KEY;
                    }
                    else if (Attribute.IsDefined(property, typeof(SqliteNotNull)))
                    {
                        columnMode = ColumnMode.NOT_NULL;
                    }
                    else if (Attribute.IsDefined(property, typeof(SqliteUnique)))
                    {
                        columnMode = ColumnMode.UNIQUE;
                    }
                    else
                    {
                        columnMode = ColumnMode.DEFAULT;
                    }

                    dynamic defaultValue;
                    if (Attribute.IsDefined(property, typeof(SqLiteDefaultValue)))
                    {
                        SqLiteDefaultValue defaultValueAttribute = (SqLiteDefaultValue)Attribute.GetCustomAttribute(property, typeof(SqLiteDefaultValue));
                        defaultValue = defaultValueAttribute.DefaultValue;
                    }
                    else
                    {
                        defaultValue = null;
                    }

                    string columnName = columnMode != ColumnMode.PRIMARY_KEY ? GetColumnName(property.Name) : property.Name.ToUpper();
                    switch (property.PropertyType.Name)
                    {
                        case "String":
                            if (defaultValue != null)
                            {
                                columns.Add(new ColumnDefinition<string>(columnName, columnMode, defaultValue));
                            }
                            else
                            {
                                columns.Add(new ColumnDefinition<string>(columnName, columnMode));
                            }
                            break;
                        case "Int32":
                            if (defaultValue != null)
                            {
                                columns.Add(new ColumnDefinition<int>(columnName, columnMode, defaultValue));
                            }
                            else
                            {
                                columns.Add(new ColumnDefinition<int>(columnName, columnMode));
                            }
                            break;
                        case "Double":
                            if (defaultValue != null)
                            {
                                columns.Add(new ColumnDefinition<double>(columnName, columnMode, defaultValue));
                            }
                            else
                            {
                                columns.Add(new ColumnDefinition<double>(columnName, columnMode));
                            }
                            break;
                        case "Single":
                            if (defaultValue != null)
                            {
                                columns.Add(new ColumnDefinition<float>(columnName, columnMode, defaultValue));
                            }
                            else
                            {
                                columns.Add(new ColumnDefinition<float>(columnName, columnMode));
                            }
                            break;
                        case "Int64":
                            if (defaultValue != null)
                            {
                                columns.Add(new ColumnDefinition<long>(columnName, columnMode, defaultValue));
                            }
                            else
                            {
                                columns.Add(new ColumnDefinition<long>(columnName, columnMode));
                            }
                            break;
                        case "DateTime":
                            if (defaultValue != null)
                            {
                                columns.Add(new ColumnDefinition<DateTime>(columnName, columnMode, defaultValue));
                            }
                            else
                            {
                                columns.Add(new ColumnDefinition<DateTime>(columnName, columnMode));
                            }
                            break;
                        default:
                            if (defaultValue != null)
                            {
                                columns.Add(new ColumnDefinition<object>(columnName, columnMode, defaultValue));
                            }
                            else
                            {
                                columns.Add(new ColumnDefinition<object>(columnName, columnMode));
                            }
                            break;
                    }
                }
            }

            return columns;
        }

        private static string GetColumnName(string propertyName)
        {
            StringBuilder sb = new StringBuilder(char.ToLower(propertyName[0]).ToString());

            for (int i = 1; i < propertyName.Length; i++)
            {
                if (char.IsUpper(propertyName[i]))
                {
                    sb.Append("_" + char.ToLower(propertyName[i]));
                }
                else
                {
                    sb.Append(propertyName[i]);
                }
            }

            return sb.ToString();
        }
    }
}

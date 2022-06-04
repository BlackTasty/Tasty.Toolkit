using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.SQLiteManager.Exceptions;
using Tasty.SQLiteManager.Table.Column;
using Tasty.SQLiteManager.Table.Conditions;

namespace Tasty.SQLiteManager.Table
{
    /// <summary>
    /// Base class for table definitions
    /// </summary>
    public class TableBaseDefinition : DefinitionBase, ITableBase
    {
        /// <summary>
        /// </summary>
        protected List<IColumn> columns = new List<IColumn>();

        /// <inheritdoc/>
        public List<IColumn> ColumnDefinitions => columns;

        /// <inheritdoc/>
        public IColumn this[string columnName] => columns.FirstOrDefault(x => x.Name == columnName);

        public TableBaseDefinition(string name)
        {
            base.name = name;
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

        /// <inheritdoc/>
        public bool TableExists()
        {
            return Query("SELECT name FROM sqlite_master WHERE type='table' AND name='" + Name + "';", true).Count > 0;
        }

        /// <inheritdoc/>
        public bool ColumnExists(IColumn target)
        {
            return columns.Exists(x => x.Equals(target));
        }

        /// <inheritdoc/>
        public bool ColumnExists(string colName)
        {
            return columns.Exists(x => x.Name.Equals(colName));
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public ResultSet Select(bool isOr = true, params Condition[] conditions)
        {
            string conditionParameter = ParseConditions(isOr, conditions);
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

        /// <inheritdoc/>
        public ResultSet Select(bool isOr, IEnumerable<Condition> conditions)
        {
            return Select(isOr, conditions.ToArray());
        }

        /// <inheritdoc/>
        public ResultSet Select(bool isOr, List<IColumn> columns, params Condition[] conditions)
        {
            return Select(isOr, columns, false, conditions);
        }

        /// <inheritdoc/>
        public ResultSet Select(bool isOr, List<IColumn> columns, bool excludeColumns, params Condition[] conditions)
        {
            string conditionParameter = ParseConditions(isOr, conditions);
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
                return Select(isOr, conditions);
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
            IColumn primaryKey = data.FirstOrDefault(x => x.Key.PrimaryKey).Key;

            if (Database.Instance.ExecuteSQL(GenerateInsertSQL(data, false)))
            {
                string query = "SELECT ID FROM '" + Name + "' WHERE ";
                string conditions = "";
                foreach (var whereData in data)
                {
                    if (!string.IsNullOrEmpty(conditions))
                    {
                        conditions += string.Format(" AND {0} == {1}", whereData.Key.Name, whereData.Key.ParseToDatabaseValue(whereData.Value));
                    }
                    else
                    {
                        conditions = string.Format("{0} == {1}", whereData.Key.Name, whereData.Key.ParseToDatabaseValue(whereData.Value));
                    }
                }

                var result = Database.Instance.SelectData(query + conditions, this);
                if (primaryKey != null)
                {
                    if (result.ColumnExists(primaryKey.Name))
                    {
                        return result[0].Columns[primaryKey.Name];
                    }
                    else
                    {
                        return -2;
                    }
                }
                else
                {
                    return -3;
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
        public bool BulkInsert(Dictionary<IColumn, dynamic>[] data)
        {
#pragma warning disable CS0618 // Typ oder Element ist veraltet
            return Database.Instance.ExecuteSQL(GenerateBulkInsert(data));
#pragma warning restore CS0618 // Typ oder Element ist veraltet
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="data"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        [Obsolete("This method is replaced with BulkInsert(data), use this method instead. This method will be removed soon!")]
        // TODO: Set access modifier to internal
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
        public bool BulkUpdate(Dictionary<IColumn, dynamic>[] data)
        {
#pragma warning disable CS0618 // Typ oder Element ist veraltet
            return Database.Instance.ExecuteSQL(GenerateBulkUpdate(data));
#pragma warning restore CS0618 // Typ oder Element ist veraltet
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="data"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        [Obsolete("This method is replaced with BulkInsert(data), use this method instead. This method will be removed soon!")]
        // TODO: Set access modifier to internal
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

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="data"><inheritdoc/></param>
        /// <param name="conditions"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public bool Update(Dictionary<IColumn, dynamic> data, params Condition[] conditions)
        {
            string setParameter = "";
            string conditionParameter = ParseConditions(true, conditions);
            foreach (KeyValuePair<IColumn, dynamic> entry in data)
            {
                if (entry.Key.PrimaryKey)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(setParameter))
                {
                    setParameter = string.Format("{0} = {1}", entry.Key.Name, entry.Key.ParseToDatabaseValue(entry.Value));
                }
                else
                {
                    setParameter += string.Format(", {0} = {1}", entry.Key.Name, entry.Key.ParseToDatabaseValue(entry.Value));
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
            string conditionParameter = conditions != null ? ParseConditions(true, conditions) : "";
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
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public int GetNextId()
        {
            IColumn idColumn = this.FirstOrDefault(x => x.PrimaryKey);

            if (idColumn != null)
            {
                ResultSet result = Select(true);
                IEnumerable<int> ids = result.Select(x => x.GetColumn<int>(idColumn.Name)).OrderByDescending(x => x);
                int lastId = ids.FirstOrDefault();

                return lastId + 1;
            }
            else
            {
                return -3;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public IColumn GetPrimaryKeyColumn()
        {
            return this.FirstOrDefault(x => x.PrimaryKey);
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
                            values = entry.Key.ParseToDatabaseValue(entry.Value);
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
                            values += ", " + entry.Key.ParseToDatabaseValue(entry.Value);
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
                        combinedRowAndValue = string.Format("{0} = {1}", entry.Key.Name, entry.Value != null ? entry.Key.ParseToDatabaseValue(entry.Value) : "NULL");
                    }
                    else
                    {
                        combinedRowAndValue += string.Format(", {0} = {1}", entry.Key.Name, entry.Value != null ? entry.Key.ParseToDatabaseValue(entry.Value) : "NULL");
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
                            values = column.ParseToDatabaseValue(column.DefaultValue);
                        }
                        else
                        {
                            rows += ", " + column.Name;
                            values += ", " + column.ParseToDatabaseValue(column.DefaultValue);
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(combinedRowAndValue))
                        {
                            combinedRowAndValue = string.Format("{0} = {1}", column.Name, column.ParseToDatabaseValue(column.DefaultValue));
                        }
                        else
                        {
                            combinedRowAndValue = string.Format(", {0} = {1}", column.Name, column.ParseToDatabaseValue(column.DefaultValue));
                        }
                    }
                }
            }

            string conditionParameter = ParseConditions(true, conditions);

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

        private string ParseConditions(bool isOr, params Condition[] conditions)
        {
            string conditionParameter = "";
            foreach (Condition condition in conditions)
            {
                if (condition.UseNewSystem)
                {
                    condition.ProvideData(this);
                }

                if (string.IsNullOrWhiteSpace(conditionParameter))
                {
                    conditionParameter = condition.ToString();
                }
                else
                {
                    conditionParameter += (isOr ? " OR " : " AND ") + condition.ToString();
                }
            }

            return conditionParameter;
        }
    }
}

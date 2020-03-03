using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.SQLiteManager.Table.Column;
using Tasty.SQLiteManager.Table.Conditions;

namespace Tasty.SQLiteManager.Table
{
    public class TableDescriptor : DescriptorBase, IList<IColumn>
    {
        protected List<IColumn> columns = new List<IColumn>();
        protected List<ForeignKeyDescriptor> foreignKeys = new List<ForeignKeyDescriptor>();

        #region IList implementation
        public int Count => columns.Count;

        public bool IsReadOnly => true;

        public IColumn this[int index] { get => columns[index]; set => columns[index] = value; }

        public IColumn this[string columnName]
        {
            get => columns.Find(x => x.Name == columnName);
            set => columns[columns.IndexOf(columns.Find(x => x.Name == columnName))] = value;
        }

        public void Add(IColumn item)
        {
            columns.Add(item);
        }

        public void Clear()
        {
            columns.Clear();
        }

        public bool Contains(IColumn item)
        {
            return columns.Contains(item);
        }

        public void CopyTo(IColumn[] array, int arrayIndex)
        {
            columns.CopyTo(array, arrayIndex);
        }

        public bool Remove(IColumn item)
        {
            return columns.Remove(item);
        }

        public IEnumerator<IColumn> GetEnumerator()
        {
            return columns.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return columns.GetEnumerator();
        }

        public int IndexOf(IColumn item)
        {
            return columns.IndexOf(item);
        }

        public void Insert(int index, IColumn item)
        {
            columns.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            columns.RemoveAt(index);
        }
        #endregion

        public List<IColumn> ColumnDescriptors { get => columns; }

        public List<ForeignKeyDescriptor> ForeignKeys { get => foreignKeys; }

        public TableDescriptor(string name)
        {
            base.name = name;
        }

        public TableDescriptor(string name, List<IColumn> columns) : this(name)
        {
            this.columns = columns;
        }

        public TableDescriptor(string name, List<IColumn> columns, List<ForeignKeyDescriptor> foreignKeys) : this(name, columns)
        {
            this.foreignKeys = foreignKeys;
        }

        public bool ColumnExists(IColumn target)
        {
            return columns.Exists(x => x.Equals(target));
        }

        public bool ColumnExists(string colName)
        {
            return columns.Exists(x => x.Name.Equals(colName));
        }

        public bool TableExists()
        {
            return Query("SELECT name FROM sqlite_master WHERE type='table' AND name='" + Name + "';", true).Count > 0;
        }

        /// <summary>
        /// Run a custom query on this table. Note that there have to be string formatter present in the command
        /// </summary>
        /// <param name="command">The command to execute. {0} = table name</param>
        /// <param name="awaitData">If true the query will execute your command as a SELECT query, else you can create any custom query</param>
        /// <returns>Returns a <see cref="ResultSet"/> if awaitData is true, otherwise null</returns>
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
        /// Returns the result of a SELECT query for this table
        /// </summary>
        /// <param name="conditions">The WHERE statement of the query. Leave empty to return all data</param>
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
        /// Returns the result of a SELECT query for this table
        /// </summary>
        /// <param name="columns">The columns to return</param>
        /// <param name="conditions">The WHERE statement of the query</param>
        /// <returns></returns>
        public ResultSet Select(List<IColumn> columns, params Condition[] conditions)
        {
            return Select(columns, false, conditions);
        }

        /// <summary>
        /// Returns the result of a SELECT query for this table
        /// </summary>
        /// <param name="columns">The columns to return. Can be toggled to act as a blacklist with the second parameter</param>
        /// <param name="excludeColumns">If true the columns parameter acts as a blacklist. The result will contain all columns except the blacklisted ones</param>
        /// <param name="conditions">The WHERE statement of the query</param>
        /// <returns></returns>
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
        /// Inserts a new dataset into the table
        /// </summary>
        /// <param name="data">The data to insert into the table</param>
        /// <returns>Returns false if the query failed, otherwise true</returns>
        /// <exception cref="MissingRequiredColumnsException"></exception>
        public bool Insert(Dictionary<IColumn, dynamic> data)
        {
            return Database.Instance.ExecuteSQL(Generate_Insert_SQL(data, false));
        }

        /// <summary>
        /// Inserts a new dataset into the table or replaces an existing entry
        /// </summary>
        /// <param name="data">The data to insert/replace into the table</param>
        /// <returns>Returns false if the query failed, otherwise true</returns>
        /// <exception cref="MissingRequiredColumnsException"></exception>
        public bool Replace(Dictionary<IColumn, dynamic> data)
        {
            return Database.Instance.ExecuteSQL(Generate_Insert_SQL(data, true));
        }

        /// <summary>
        /// Inserts a new dataset into the table and returns the ID of the new dataset
        /// </summary>
        /// <param name="data">The data to insert into the table</param>
        /// <returns>Returns -1 if the query failed and -2 if there are no results. Otherwise the ID of the inserted data is returned</returns>
        /// <exception cref="MissingRequiredColumnsException"></exception>
        public int Insert_GetIndex(Dictionary<IColumn, dynamic> data)
        {
            if (Database.Instance.ExecuteSQL(Generate_Insert_SQL(data, false)))
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

        public string GenerateBulkInsert(Dictionary<IColumn, dynamic>[] data)
        {
            string sql = "BEGIN TRANSACTION;\n";
            foreach (Dictionary<IColumn, dynamic> row in data)
            {
                sql += Generate_Insert_SQL(row, false) + "\n";
            }
            sql += "COMMIT;";

            return sql;
        }

        public string GenerateBulkUpdate(Dictionary<IColumn, dynamic>[] data)
        {
            string sql = "BEGIN TRANSACTION;\n";
            foreach (Dictionary<IColumn, dynamic> row in data)
            {
                sql += Generate_Insert_SQL(row, true) + "\n";
            }
            sql += "COMMIT;";

            return sql;
        }

        private string Generate_Insert_SQL(Dictionary<IColumn, dynamic> data, bool isReplace, params Condition[] conditions)
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
                        combinedRowAndValue = string.Format("{0} = {1}", entry.Key.Name, entry.Value != null ?  entry.Key.ParseColumnValue(entry.Value) : "NULL");
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
        /// Update a dataset inside this table
        /// </summary>
        /// <param name="data">The columns which shall be updated</param>
        /// <param name="conditions">The WHERE statement of the query</param>
        /// <returns>Returns false if the query failed, otherwise true</returns>
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
        /// Remove all entries in this table
        /// </summary>
        /// <returns>Returns false if the query failed, otherwise true</returns>
        public bool Delete()
        {
            return Delete(null);
        }

        /// <summary>
        /// Remove a dataset from this table
        /// </summary>
        /// <param name="conditions">The WHERE statement of the query</param>
        /// <returns>Returns false if the query failed, otherwise true</returns>
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

            foreach (ForeignKeyDescriptor foreignKey in foreignKeys)
            {
                sql_inner += ",\n\t" + foreignKey.ToString();
            }

            return sql_inner;
        }
    }
}

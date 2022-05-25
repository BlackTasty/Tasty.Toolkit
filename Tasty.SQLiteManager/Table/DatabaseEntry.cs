using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tasty.SQLiteManager.Exceptions;
using Tasty.SQLiteManager.Table.Attributes;
using Tasty.SQLiteManager.Table.Column;
using Tasty.SQLiteManager.Table.Conditions;

namespace Tasty.SQLiteManager.Table
{
    public class DatabaseEntry<T>
    {
        protected TableDefinition<T> table;

        private bool useDB;
        private bool fromDatabase;
        private int id;

        [SqliteIgnore]
        public bool FromDatabase => fromDatabase;

        [SqlitePrimaryKey]
        public int ID
        {
            get => id;
            private set => id = value;
        }

        public DatabaseEntry(TableDefinition<T> table)
        {
            this.table = table;
            useDB = table != null;
        }

        public DatabaseEntry(TableDefinition<T> table, RowData data) : this(table)
        {
            fromDatabase = true;
            id = data["ID"];

            foreach (var row in data.Columns)
            {
                IColumn column = table[row.Key];
                if (column.Name == "ID")
                {
                    continue;
                }

                PropertyInfo propertyInfo = typeof(T).GetProperty(column.PropertyInfo.Name);
                if (!propertyInfo.CanWrite)
                {
                    throw new MissingWriteAccessException(propertyInfo.Name);
                }
                propertyInfo.SetValue(this, Convert.ChangeType(row.Value, propertyInfo.PropertyType), null);
            }
        }

        public static RowData LoadFromDatabase(TableDefinition<T> table, Dictionary<string, dynamic> conditionParams)
        {
            List<Condition> conditions = new List<Condition>();
            foreach (var conditionParam in conditionParams)
            {
                conditions.Add(new Condition(table[conditionParam.Key], conditionParam.Value));
            }

            ResultSet result = table.Select(conditions.ToArray());

            return result.FirstOrDefault();
        }

        public static ResultSet LoadAllFromDatabase(TableDefinition<T> table)
        {
            return table.Select();
        }

        protected Dictionary<IColumn, dynamic> GetRowsForBulkInsertUpdate()
        {
            return new Dictionary<IColumn, dynamic>()
            {
                { table["ID"], id }
            };
        }

        public virtual void SaveToDatabase()
        {
            if (!useDB)
            {
                return;
            }

            if (FromDatabase)
            {
                IColumn primaryKeyColumn = table.FirstOrDefault(x => x.PrimaryKey);
                table.Update(GetRowData(), new Condition(primaryKeyColumn, id));
            }
            else
            {
                id = table.GetNextId();
                if (id >= 1)
                {
                    if (!table.Insert(GetRowData()))
                    {
                        Console.WriteLine("Error inserting database entry!");
                    }
                }
                fromDatabase = true;
            }
        }

        protected ResultSet Select(params Condition[] conditions)
        {
            return useDB ? table.Select(conditions) : new ResultSet();
        }

        protected ResultSet Select(List<IColumn> columns, params Condition[] conditions)
        {
            return useDB ? table.Select(columns, conditions) : new ResultSet();
        }

        protected ResultSet Select(List<IColumn> columns, bool excludeColumns, params Condition[] conditions)
        {
            return useDB ? table.Select(columns, excludeColumns, conditions) : new ResultSet();
        }

        public int GetNextAvailableId()
        {
            var result = table.Select();
            if (!result.IsEmpty)
            {
                return result.Last().Columns["ID"] + 1;
            }
            else
            {
                return 0;
            }
        }

        private Dictionary<IColumn, dynamic> GetRowData()
        {
            Dictionary<IColumn, dynamic> data = new Dictionary<IColumn, dynamic>();
            Type t = typeof(T);

            foreach (PropertyInfo property in t.GetProperties())
            {
                bool hasForeignKeys = Attribute.IsDefined(property, typeof(SqliteForeignKey));
                bool isList = property.PropertyType.IsGenericType;
                if (isList && !hasForeignKeys) // If property is a list and doesn't have SqliteForeignKey attribute, skip
                {
                    continue;
                }

                if (!Attribute.IsDefined(property, typeof(SqliteIgnore)))
                {
                    if (!hasForeignKeys)
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

                        string columnName = columnMode != ColumnMode.PRIMARY_KEY ? Util.GetColumnName(property.Name) : property.Name.ToUpper();
                        data.Add(table[columnName], property.GetValue(this));
                    }
                    else
                    {
                        SqliteForeignKey sqliteForeignKeyAttribute = (SqliteForeignKey)property.GetCustomAttribute(typeof(SqliteForeignKey));
                        List<Condition> conditions = new List<Condition>();
                    }
                }
            }

            return data;
        }
    }
}

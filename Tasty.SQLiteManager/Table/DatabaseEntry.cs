using System;
using System.Collections;
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
    public class DatabaseEntry<T> : IDatabaseEntry
    {
        protected TableDefinition<T> table;

        private bool useDB;
        private bool fromDatabase;
        private int id;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        [SqliteIgnore]
        public bool FromDatabase => fromDatabase;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        [SqliteIgnore]
        public ITable Table => table;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        [SqlitePrimaryKey]
        public int ID
        {
            get => id;
        }

        public DatabaseEntry(TableDefinition<T> table)
        {
            this.table = table;
            useDB = table != null;
        }

        [SqliteConstructor]
        public DatabaseEntry(TableDefinition<T> table, RowData data) : this(table)
        {
            fromDatabase = true;
            id = data["ID"];

            SetRowData(data);
        }

        public static T LoadFromDatabase(TableDefinition<T> table, params Condition[] conditions)
        {
            ResultSet result = table.Select(conditions);
            if (result.IsEmpty)
            {
                return default(T);
            }

            return ConstructGeneric(table, result.FirstOrDefault());
        }

        public static List<T> LoadAllFromDatabase(TableDefinition<T> table)
        {
            ResultSet result = table.Select();
            List<T> entries = new List<T>();
            foreach (RowData data in result)
            {
                entries.Add(ConstructGeneric(table, data));
            }

            return entries;
        }

        [SqliteDataSetter]
        internal void SetRowData(RowData data)
        {
            Type dataType = typeof(T);

            // Set data from DB
            foreach (var row in data.Columns)
            {
                IColumn column = table[row.Key];
                if (column.Name == "ID")
                {
                    id = row.Value;
                    fromDatabase = true;
                    continue;
                }

                PropertyInfo propertyInfo = dataType.GetProperty(column.PropertyInfo.Name);
                if (!propertyInfo.CanWrite)
                {
                    throw new MissingWriteAccessException(propertyInfo.Name);
                }


                object value = Convert.ChangeType(row.Value, propertyInfo.PropertyType);
                propertyInfo.SetValue(this, value, null);
            }

            // Populate lists with SqliteForeignKey attribute
            var foreignProperties = dataType.GetProperties().Where(x => Attribute.IsDefined(x, typeof(SqliteForeignKey)));
            foreach (PropertyInfo foreignProperty in foreignProperties)
            {
                if (foreignProperty.PropertyType.IsGenericType)
                {
                    SqliteForeignKey foreignKeyAttribute = foreignProperty.GetCustomAttribute<SqliteForeignKey>();
                    ChildTableDefinition childTable = Database.Instance.GetChildTable(foreignKeyAttribute.Data.ChildTableName);
                    IColumn foreignKey = childTable.GetChildColumnByParentTable(table.Name);
                    ResultSet mappingResults = childTable.Select(new Condition(foreignKey, id)); // Get mapping data for list type

                    if (!mappingResults.IsEmpty)
                    {
                        Type childType = foreignProperty.PropertyType.GenericTypeArguments.FirstOrDefault();
                        Type databaseEntryType = typeof(DatabaseEntry<>).MakeGenericType(new Type[] { childType });
                        Type targetTableType = Util.MakeGenericTableDefinition(childType);


                        ConstructorInfo ctor = childType.GetConstructor(new[] { targetTableType });
                        if (ctor != null)
                        {
                            ITable table = Database.Instance[childType]; // Get table for list type

                            var obj = ctor.Invoke(new object[] { table });
                            MethodInfo setRowData = databaseEntryType.GetMethod("ConstructGeneric", BindingFlags.Static | BindingFlags.NonPublic);
                            if (setRowData != null)
                            {
                                string targetKeyName = this.table.ForeignKeyData.FirstOrDefault(x => x.ChildTableName == childTable.Name)?.ForeignKeyName;

                                if (targetKeyName != null)
                                {
                                    List<Condition> conditions = new List<Condition>();
                                    foreach (RowData rowData in mappingResults)
                                    {
                                        conditions.Add(new Condition(table.GetPrimaryKeyColumn(), rowData[targetKeyName]));
                                    }

                                    ResultSet dataResult = table.Select(conditions);

                                    Type genericListType = typeof(List<>);
                                    Type listType = genericListType.MakeGenericType(childType);
                                    var listInstance = (IList)Activator.CreateInstance(listType);

                                    foreach (RowData rowData in dataResult)
                                    {
                                        listInstance.Add(setRowData.Invoke(obj, new object[] { table, rowData }));
                                    }

                                    foreignProperty.SetValue(this, listInstance);
                                }
                            }
                        }
                    }

                }
            }
        }

        private static T ConstructGeneric(TableDefinition<T> table, RowData data)
        {
            Type rowType = typeof(T);

            Type tableDefinitionType = Util.MakeGenericTableDefinition(rowType);
            ConstructorInfo ctor = rowType.GetConstructor(new[] { tableDefinitionType });
            if (ctor != null)
            {
                T obj = (T)ctor.Invoke(new object[] { table });
                MethodInfo setRowData = rowType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                    .FirstOrDefault(x => Attribute.IsDefined(x, typeof(SqliteDataSetter)) && !((SqliteDataSetter)x.GetCustomAttribute<SqliteDataSetter>()).SetChildData);
                if (setRowData != null)
                {
                    setRowData.Invoke(obj, new object[] { data });
                }


                return obj;
            }
            else
            {
                return default(T);
            }
        }

        internal IColumn GetColumnByPropertyName(string propertyName)
        {
            return table[propertyName];
        }

        private static Type GetDatabaseEntryType(Type rowType)
        {
            Type currentType = rowType;
            while (currentType != typeof(DatabaseEntry<T>))
            {
                if (currentType.BaseType == null)
                {
                    return null;
                }
                currentType = currentType.BaseType;
            }

            return currentType;
        }

        protected Dictionary<IColumn, dynamic> GetRowsForBulkInsertUpdate()
        {
            return new Dictionary<IColumn, dynamic>()
            {
                { table["ID"], id }
            };
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public virtual int SaveToDatabase()
        {
            if (!useDB)
            {
                return -10;
            }

            if (FromDatabase)
            {
                IColumn primaryKeyColumn = table.FirstOrDefault(x => x.PrimaryKey);
                if (!table.Update(GetRowData(out bool childSuccess), new Condition(primaryKeyColumn, id)))
                {
                    Console.WriteLine("Error updating database entry!");
                    return -1;
                }
                else if (!childSuccess)
                {
                    Console.WriteLine("Error updating child connections!");
                    return -2;
                }
            }
            else
            {
                id = table.GetNextId();
                if (id >= 1)
                {
                    if (!table.Insert(GetRowData(out bool childSuccess)))
                    {
                        Console.WriteLine("Error inserting database entry!");
                        return -1;
                    }
                    else if (!childSuccess)
                    {
                        Console.WriteLine("Error updating child connections!");
                        return -2;
                    }
                }
                fromDatabase = true;
            }
            return 0;
        }

        private void UpdateChildTables()
        {

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

        private Dictionary<IColumn, dynamic> GetRowData(out bool childSuccess)
        {
            Dictionary<IColumn, dynamic> data = new Dictionary<IColumn, dynamic>();
            Type t = typeof(T);
            childSuccess = true;

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
                        IList childData = property.GetValue(this) as IList;

                        if (childData.Count > 0)
                        {
                            SqliteForeignKey sqliteForeignKeyAttribute = (SqliteForeignKey)property.GetCustomAttribute(typeof(SqliteForeignKey));

                            ChildTableDefinition childTable = Database.Instance.GetChildTable(sqliteForeignKeyAttribute.Data.ChildTableName);
                            IColumn foreignKey = childTable.GetChildColumnByParentTable(table.Name);
                            IColumn childForeignKey = null;

                            Dictionary<IColumn, dynamic>[] queryData = new Dictionary<IColumn, dynamic>[childData.Count];
                            for (int i = 0; i < childData.Count; i++)
                            {
                                var childEntry = (childData[i] as IDatabaseEntry);
                                if (childForeignKey == null)
                                {
                                    childForeignKey = childTable.GetChildColumnByParentTable(childEntry.Table.Name);
                                }
                                childEntry.SaveToDatabase();

                                queryData[i] = new Dictionary<IColumn, dynamic>()
                                {
                                    { foreignKey, ID }, // Put ID of this element into foreign key for that field
                                    { childForeignKey, childEntry.ID } // Put ID of the child element into foreign key for that field
                                };

                            }

                            string sql = childTable.GenerateBulkInsert(queryData);
                            childSuccess = Database.Instance.ExecuteSQL(sql);
                        }
                    }
                }
            }

            return data;
        }
    }
}

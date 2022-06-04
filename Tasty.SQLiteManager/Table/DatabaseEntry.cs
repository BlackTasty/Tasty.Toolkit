using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tasty.SQLiteManager.Exceptions;
using Tasty.SQLiteManager.Table.Attributes;
using Tasty.SQLiteManager.Table.Column;
using Tasty.SQLiteManager.Table.Conditions;
using Tasty.SQLiteManager.Table.ForeignKey;

namespace Tasty.SQLiteManager.Table
{
    /// <summary>
    /// Define basic properties for database functionality
    /// </summary>
    /// <typeparam name="T">The type of database object.</typeparam>
    public class DatabaseEntry<T> : IDatabaseEntry
    {
        protected TableDefinition<T> table;

        private readonly bool useDB;
        private bool fromDatabase;
        private int id;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        [SqliteIgnore]
        public bool FromDatabase
        {
            get => fromDatabase;
            protected set => fromDatabase = value;
        }

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
            protected set => id = value;
        }

        /// <summary>
        /// Initializes a new <see cref="DatabaseEntry{T}"/>.
        /// </summary>
        /// <param name="table">The table containing data for this <see cref="DatabaseEntry{T}"/>.</param>
        public DatabaseEntry(TableDefinition<T> table)
        {
            if (!GetType().Attributes.HasFlag(TypeAttributes.Public))
            {
                throw new MissingAccessException("Cannot access to class \"" + GetType().Name + "\", set access modifier to public.");
            }

            this.table = table;
            useDB = table != null;
            SetDefaultValues();
        }

        /// <summary>
        /// Initializes a <see cref="DatabaseEntry{T}"/> and populates it with the given <see cref="RowData"/>.
        /// </summary>
        /// <param name="table">The table containing data for this <see cref="DatabaseEntry{T}"/>.</param>
        /// <param name="data">The <see cref="RowData"/> for this <see cref="DatabaseEntry{T}"/>.</param>
        [SqliteConstructor]
        public DatabaseEntry(TableDefinition<T> table, RowData data) : this(table)
        {
            fromDatabase = true;
            id = data["ID"];

            SetRowData(data, true);
        }

        /// <summary>
        /// Load a single <see cref="DatabaseEntry{T}"/> from the database.
        /// </summary>
        /// <param name="conditions">Optional: Conditions to filter results.</param>
        /// <returns>If no conditions are passed, the last entry in the table is returned. Else the first occurrence meeting the conditions is returned.</returns>
        public static T LoadFromDatabase(params Condition[] conditions)
        {
            var table = GetTableDefinitionForType<T>();
            foreach (Condition condition in conditions)
            {
                if (condition.UseNewSystem)
                {
                    condition.ProvideData(table);
                }
            }

            ResultSet result = table.Select(true, conditions);
            if (result.IsEmpty)
            {
                return default;
            }

            return ConstructGeneric(table, conditions.Length > 0 ? result.FirstOrDefault() : result.LastOrDefault(), true);
        }

        /// <summary>
        /// Load multiple <see cref="DatabaseEntry{T}"/> from the database.
        /// </summary>
        /// <param name="conditions">Optional: Conditions to filter results.</param>
        /// <returns>If no conditions are passed, returns all entries in the table. Else all occurrences meeting the conditions are returned.</returns>
        public static List<T> LoadAllFromDatabase(params Condition[] conditions)
        {
            var table = GetTableDefinitionForType<T>();

            ResultSet result = table.Select(true, conditions);

            List<T> entries = new List<T>();
            foreach (RowData data in result)
            {
                entries.Add(ConstructGeneric(table, data, true));
            }

            return entries;
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
                if (!table.Update(GetRowData(false, out bool childSuccess), new Condition(primaryKeyColumn, id)))
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
                    if (!table.Insert(GetRowData(true, out bool childSuccess)))
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

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void DeleteFromDatabase()
        {
            table.Delete(new Condition("ID", ID));
            fromDatabase = false;
            id = 0;
        }

        protected Dictionary<IColumn, dynamic> GetRowsForBulkInsertUpdate()
        {
            return new Dictionary<IColumn, dynamic>()
            {
                { table["ID"], id }
            };
        }

        protected ResultSet Select(bool isOr, params Condition[] conditions)
        {
            return useDB ? table.Select(isOr, conditions) : new ResultSet();
        }

        protected ResultSet Select(bool isOr, List<IColumn> columns, params Condition[] conditions)
        {
            return useDB ? table.Select(isOr, columns, conditions) : new ResultSet();
        }

        protected ResultSet Select(bool isOr, List<IColumn> columns, bool excludeColumns, params Condition[] conditions)
        {
            return useDB ? table.Select(isOr, columns, excludeColumns, conditions) : new ResultSet();
        }

        internal IColumn GetColumnByPropertyName(string propertyName)
        {
            return table[propertyName];
        }

        [SqliteDataSetter]
        internal void SetRowData(RowData data, bool loadChildren)
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
                    throw new MissingAccessException("Cannot write to property \"" + propertyInfo.Name + "\" because no set method is set! Check out the documentation for more information: [LINK]");
                }

                if (!column.IsForeignKey)
                {
                    if (row.Value != null)
                    {
                        dynamic parsedValue;
                        Type columnType = column.UnderlyingType != null ? column.UnderlyingType : column.DataType;

                        if (columnType == typeof(DateTime) || columnType == typeof(TimeSpan))
                        {
                            parsedValue = row.Value;
                        }
                        else
                        {
                            parsedValue = ParseValue(column, row.Value);
                        }

                        propertyInfo.SetValue(this, Convert.ChangeType(!columnType.IsEnum ? parsedValue : Enum.ToObject(columnType, row.Value),
                            propertyInfo.PropertyType));
                    }
                    else
                    {
                        propertyInfo.SetValue(this, null);
                    }
                }
                else
                {
                    Type foreignType = column.PropertyInfo.PropertyType;
                    Type foreignEntryType = typeof(DatabaseEntry<>).MakeGenericType(new Type[] { foreignType });
                    Type foreignTableType = Util.MakeGenericTableDefinition(foreignType);
                    ITable foreignTable = Database.Instance[foreignType];
                    IColumn foreignKeyColumn = foreignTable.GetPrimaryKeyColumn();

                    ResultSet result = foreignTable.Select(true, new Condition(foreignKeyColumn, row.Value));
                    if (!result.IsEmpty)
                    {
                        ConstructorInfo ctor = foreignType.GetConstructor(new[] { foreignTableType });
                        if (ctor != null)
                        {
                            var obj = ctor.Invoke(new object[] { foreignTable });
                            MethodInfo setRowData = foreignEntryType.GetMethod("ConstructGeneric", BindingFlags.Static | BindingFlags.NonPublic);
                            if (setRowData != null)
                            {
                                IDatabaseEntry entry = (IDatabaseEntry)setRowData.Invoke(obj, new object[] { foreignTable, result.FirstOrDefault(), false });
                                propertyInfo.SetValue(this, Convert.ChangeType(entry, foreignType));
                            }
                        }
                    }
                }
            }

            if (!loadChildren) // Skip loading of child data for this entry
            {
                return;
            }

            // Populate lists (for parents) and objects (for children) with SqliteForeignKey attribute
            foreach (PropertyInfo foreignProperty in dataType
                .GetProperties().Where(x => Attribute.IsDefined(x, typeof(SqliteForeignKey))))
            {
                SqliteForeignKey foreignKeyAttribute = foreignProperty.GetCustomAttribute<SqliteForeignKey>();
                
                if (!foreignKeyAttribute.Data.IsOneToOne)
                {
                    #region One-to-many relation
                    ChildTableDefinition childTable = Database.Instance.GetChildTable(foreignKeyAttribute.Data.ChildTableName);

                    if (foreignProperty.PropertyType.IsGenericType) // Is list, load data for children from database
                    {
                        #region Load generic data into list of property
                        IColumn foreignKey = childTable.GetChildColumnByParentTable(table.Name);
                        ResultSet mappingResults = childTable.Select(true, new Condition(foreignKey, id)); // Get mapping data for list type

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
                                    ForeignKeyData foreignKeyData = this.table.ForeignKeyData.FirstOrDefault(x => x.ChildTableName == childTable.Name);
                                    if (foreignKeyData != null)
                                    {
                                        string targetKeyName = foreignKeyData.ManyToManyTargetKeyName == null ? foreignKeyData.ForeignKeyName : foreignKeyData.ManyToManyTargetKeyName;

                                        if (targetKeyName != null)
                                        {
                                            List<Condition> conditions = new List<Condition>();
                                            foreach (RowData rowData in mappingResults)
                                            {
                                                Condition condition = new Condition(table.GetPrimaryKeyColumn(), rowData[targetKeyName]);
                                                if (!conditions.Any(x => x.ToString().Equals(condition.ToString())))
                                                {
                                                    conditions.Add(condition);
                                                }
                                            }

                                            ResultSet dataResult = table.Select(true, conditions);
                                            if (dataResult.IsEmpty)
                                            {
                                                return;
                                            }

                                            Type genericListType = typeof(List<>);
                                            Type listType = genericListType.MakeGenericType(childType);
                                            var listInstance = (IList)Activator.CreateInstance(listType);

                                            foreach (RowData rowData in dataResult)
                                            {
                                                listInstance.Add(setRowData.Invoke(obj, new object[] { table, rowData, !foreignKeyAttribute.Data.IsManyToMany ? loadChildren : false }));
                                            }

                                            foreignProperty.SetValue(this, listInstance);
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                    else // Is foreign object, load data
                    {
                        #region Load generic data into property
                        IColumn foreignKey = childTable.GetChildColumnByParentTable(table.Name);
                        ResultSet mappingResults = childTable.Select(true, new Condition(foreignKey, id)); // Get mapping data for list type

                        if (!mappingResults.IsEmpty)
                        {
                            Type childType = foreignProperty.PropertyType;
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
                                        ResultSet dataResult = table.Select(true, new Condition(table.GetPrimaryKeyColumn(),
                                            mappingResults.FirstOrDefault()[targetKeyName]));
                                        if (dataResult.IsEmpty)
                                        {
                                            return;
                                        }

                                        IDatabaseEntry entry = (IDatabaseEntry)setRowData.Invoke(obj, new object[] { table, dataResult.FirstOrDefault(), false });
                                        foreignProperty.SetValue(this, Convert.ChangeType(obj, childType));
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                    #endregion
                }
            }
        }

        private dynamic ParseValue(IColumn column, dynamic value)
        {
            if (value == null)
            {
                return default;
            }

            if (column.UnderlyingType != null)
            {
                return Expression.Constant(value, column.DataType);
            }
            else
            {
                return value;
            }
        }

        private void SetDefaultValues()
        {
            Type t = typeof(T);

            foreach (PropertyInfo property in t.GetProperties().Where(x => Attribute.IsDefined(x, typeof(SqLiteDefaultValue))))
            {
                var defaultValue = GetDefaultValue(property);
                property.SetValue(this, Convert.ChangeType(defaultValue, property.PropertyType));
            }
        }

        private Dictionary<IColumn, dynamic> GetRowData(bool isInsert, out bool childSuccess)
        {
            Dictionary<IColumn, dynamic> data = new Dictionary<IColumn, dynamic>();
            Type t = typeof(T);
            childSuccess = true;

            foreach (PropertyInfo propertyInfo in t.GetProperties())
            {
                if (Attribute.IsDefined(propertyInfo, typeof(SqliteIgnore)))
                {
                    continue;
                }

                bool hasForeignKeys = Attribute.IsDefined(propertyInfo, typeof(SqliteForeignKey));
                Type underlyingType = Nullable.GetUnderlyingType(propertyInfo.PropertyType);
                bool isList = propertyInfo.PropertyType.IsGenericType && underlyingType == null;
                if (isList && !hasForeignKeys) // If property is a list and doesn't have SqliteForeignKey attribute, skip
                {
                    continue;
                }

                if (!hasForeignKeys)
                {
                    ColumnMode columnMode;

                    if (Attribute.IsDefined(propertyInfo, typeof(SqlitePrimaryKey)))
                    {
                        columnMode = ColumnMode.PRIMARY_KEY;
                    }
                    else if (Attribute.IsDefined(propertyInfo, typeof(SqliteNotNull)))
                    {
                        columnMode = ColumnMode.NOT_NULL;
                    }
                    else if (Attribute.IsDefined(propertyInfo, typeof(SqliteUnique)))
                    {
                        columnMode = ColumnMode.UNIQUE;
                    }
                    else
                    {
                        columnMode = ColumnMode.DEFAULT;
                    }

                    string columnName = columnMode != ColumnMode.PRIMARY_KEY ? Util.GetColumnName(propertyInfo.Name) : propertyInfo.Name.ToUpper();
                    data.Add(table[columnName], propertyInfo.GetValue(this));
                }
                else
                {
                    SqliteForeignKey foreignKeyAttribute = propertyInfo.GetCustomAttribute<SqliteForeignKey>();
                    if (propertyInfo.PropertyType.IsGenericType) // Property is list, which makes this entry the parent
                    {
                        #region Save list of generic data into database
                        IList childData = propertyInfo.GetValue(this) as IList;

                        if (childData?.Count > 0)
                        {
                            SqliteForeignKey sqliteForeignKeyAttribute = (SqliteForeignKey)propertyInfo.GetCustomAttribute(typeof(SqliteForeignKey));

                            ChildTableDefinition childTable = Database.Instance.GetChildTable(sqliteForeignKeyAttribute.Data.ChildTableName);
                            IColumn foreignKey = childTable.GetChildColumnByParentTable(table.Name);
                            IColumn childForeignKey = null;

                            Dictionary<IColumn, dynamic>[] queryData = new Dictionary<IColumn, dynamic>[childData.Count];
                            for (int i = 0; i < childData.Count; i++)
                            {
                                var childEntry = (childData[i] as IDatabaseEntry);
                                if (childForeignKey == null)
                                {
                                    if (!sqliteForeignKeyAttribute.Data.IsManyToMany)
                                    {
                                        childForeignKey = childTable.GetChildColumnByParentTable(childEntry.Table.Name);
                                    }
                                    else
                                    {
                                        ITable foreignParentTable = Database.Instance[propertyInfo.DeclaringType];
                                        childForeignKey = childTable[string.Format("{0}_{1}", 
                                            Util.GetColumnName(Util.GetSingular(propertyInfo.Name)).ToUpper(), foreignParentTable.GetPrimaryKeyColumn().Name)];
                                    }
                                }
                                childEntry.SaveToDatabase();

                                queryData[i] = new Dictionary<IColumn, dynamic>()
                                    {
                                        { foreignKey, ID }, // Put ID of this element into foreign key for that field
                                        { childForeignKey, childEntry.ID } // Put ID of the child element into foreign key for that field
                                    };
                            }

#pragma warning disable CS0618 // Typ oder Element ist veraltet
                            string sql = childTable.GenerateBulkInsert(queryData);
#pragma warning restore CS0618 // Typ oder Element ist veraltet
                            childSuccess = Database.Instance.ExecuteSQL(sql);
                        }
                        #endregion
                    }
                    else if (foreignKeyAttribute.Data.IsOneToOne)
                    {
                        var value = propertyInfo.GetValue(this);
                        #region Save foreign data into database and save id into foreign column
                        if (value is IDatabaseEntry foreignEntry)
                        {
                            foreignEntry.SaveToDatabase();

                            string columnName = Util.GetColumnName(propertyInfo.Name).ToUpper() + "_ID";
                            data.Add(table[columnName], foreignEntry);
                        }
                        #endregion
                    }
                }
            }

            return data;
        }

        private dynamic GetDefaultValue(PropertyInfo propertyInfo)
        {
            dynamic value = propertyInfo.GetValue(this);
            if (Attribute.IsDefined(propertyInfo, typeof(SqLiteDefaultValue))) // Checks if the property has the SqliteDefaultValue attribute
            {
                SqLiteDefaultValue defaultValueAttribute = propertyInfo.GetCustomAttribute<SqLiteDefaultValue>();
                Type valueType = propertyInfo.PropertyType;
                dynamic defaultValueForType = valueType.IsPrimitive ? Activator.CreateInstance(valueType) : null; // Generates a new instance of the value type

                if (value == defaultValueForType) // Checks if currently set value is default value for type
                {
                    // Returns the default value set with the SqliteDefaultValue attribute
                    return Convert.ChangeType(defaultValueAttribute.DefaultValue, valueType);
                }
            }

            // Returns the currently set value
            return value;
        }

        private static TableDefinition<T> GetTableDefinitionForType<T>()
        {
            return (TableDefinition<T>)Database.Instance[typeof(T)];
        }

        private static T ConstructGeneric(TableDefinition<T> table, RowData data, bool loadChildren)
        {
            Type rowType = typeof(T);

            Type tableDefinitionType = Util.MakeGenericTableDefinition(rowType);
            ConstructorInfo ctor = rowType.GetConstructor(new[] { tableDefinitionType });
            if (ctor != null)
            {
                T obj = (T)ctor.Invoke(new object[] { table });
                MethodInfo setRowData = rowType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                    .FirstOrDefault(x => Attribute.IsDefined(x, typeof(SqliteDataSetter)) && !(x.GetCustomAttribute<SqliteDataSetter>()).SetChildData);
                if (setRowData != null)
                {
                    setRowData.Invoke(obj, new object[] { data, loadChildren });
                }


                return obj;
            }
            else
            {
                return default;
            }
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
    }
}

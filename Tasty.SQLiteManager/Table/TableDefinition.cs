using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Tasty.SQLiteManager.Exceptions;
using Tasty.SQLiteManager.Table.Attributes;
using Tasty.SQLiteManager.Table.Column;
using Tasty.SQLiteManager.Table.Conditions;
using Tasty.SQLiteManager.Table.ForeignKey;

namespace Tasty.SQLiteManager.Table
{
    /// <summary>
    /// Used to define a table with columns for the database
    /// </summary>
    public class TableDefinition<T> : TableBaseDefinition, ITable
    {
        private readonly Type tableType;
        private readonly List<ForeignKeyData> foreignKeyData = new List<ForeignKeyData>();
        private List<ChildTableDefinition> childTables = new List<ChildTableDefinition>();

        /// <summary>
        /// </summary>
        [Obsolete]
        protected List<ForeignKeyDefinition> foreignKeys = new List<ForeignKeyDefinition>();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public List<ForeignKeyData> ForeignKeyData => foreignKeyData;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool HasOneToOneRelations => foreignKeyData.Any(x => x.IsOneToOne);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public List<ChildTableDefinition> ChildTables
        {
            get => childTables;
            internal set => childTables = value;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        [Obsolete("This method of retrieving foreign keys is deprecated and will be removed soon! Check out the documentation for more information: [LINK]")]
        public List<ForeignKeyDefinition> ForeignKeys => foreignKeys;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Type TableType => tableType;

        /// <summary>
        /// Define a new table with the specified columns.
        /// </summary>
        /// <param name="name">The name of the table</param>
        [SqliteConstructor]
        public TableDefinition(string name) : base(name)
        {
            columns = GetColumnsFromClass(typeof(T), name);

            tableType = typeof(T);
            foreignKeyData = GetForeignKeysFromClass(typeof(T));
        }

        /// <summary>
        /// Define a new table with the specified columns.
        /// </summary>
        /// <param name="name">The name of the table</param>
        /// <param name="columns">A list of <see cref="ColumnDefinition{T}"/> objects for this table.</param>
        [Obsolete("This constructor is deprecated and will be removed soon. Check out the documentation for setting up tables for more details: [LINK]", false)]
        public TableDefinition(string name, List<IColumn> columns) : base(name)
        {
            this.columns = columns;
        }

        /// <summary>
        /// Define a new table with the specified columns.
        /// </summary>
        /// <param name="name">The name of the table</param>
        /// <param name="columns">A list of <see cref="ColumnDefinition{T}"/> objects for this table.</param>
        /// <param name="foreignKeys">A list of foreign key definitions.</param>
        [Obsolete("This constructor is deprecated and will be removed soon. Check out the documentation for setting up tables for more details: [LINK]", false)]
        public TableDefinition(string name, List<IColumn> columns, List<ForeignKeyDefinition> foreignKeys) : this(name, columns)
        {
            this.foreignKeys = foreignKeys;
        }

        /// <summary>
        /// Define a new table with the specified columns.
        /// </summary>
        /// <param name="name">The name of the table</param>
        /// <param name="foreignKeys">A list of foreign key definitions.</param>
        [Obsolete("This constructor is deprecated and will be removed soon. Check out the documentation for setting up tables for more details: [LINK]", false)]
        public TableDefinition(string name, List<ForeignKeyDefinition> foreignKeys) : this(name)
        {
            this.foreignKeys = foreignKeys;
        }

        public void SetOneToOneRelationData(IEnumerable<ITable> tables)
        {
        }

        /// <summary>
        /// Returns the CREATE statement for this table.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("CREATE TABLE IF NOT EXISTS \"{0}\" ({1});", name, GetQueryData());
        }

        private string GetQueryData()
        {
            StringBuilder queryBuilder = new StringBuilder();
            foreach (IColumn column in columns)
            {
                if (queryBuilder.Length > 0)
                {
                    queryBuilder.Append(",\n\t" + column.ToString());
                }
                else
                {
                    queryBuilder.Append("\n\t" + column.ToString());
                }
            }

            foreach (ForeignKeyData foreignKey in foreignKeyData.Where(x => x.IsOneToOne))
            {
                string foreignKeySql = foreignKey.ToString();
                if (foreignKeySql != null)
                {
                    if (queryBuilder.Length > 0)
                    {
                        queryBuilder.Append(",\n\t" + foreignKeySql);
                    }
                    else
                    {
                        queryBuilder.Append("\n\t" + foreignKeySql);
                    }
                }
            }

            return queryBuilder.ToString();
        }

        private static List<IColumn> GetColumnsFromClass(Type target, string tableName)
        {
            List<IColumn> columns = new List<IColumn>();

            foreach (PropertyInfo property in target.GetProperties().OrderByDescending(x => Attribute.IsDefined(x, typeof(SqlitePrimaryKey))))
            {
                bool isOneToManyRelation = false;
                bool isOneToOneRelation = false;
                if (Attribute.IsDefined(property, typeof(SqliteForeignKey)))
                {
                    SqliteForeignKey foreignKeyAttribute = property.GetCustomAttribute<SqliteForeignKey>();
                    isOneToOneRelation = foreignKeyAttribute.Data.IsOneToOne;
                    isOneToManyRelation = !isOneToOneRelation;
                }

                if (!Attribute.IsDefined(property, typeof(SqliteIgnore)) && !isOneToManyRelation)
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

                    string columnName;
                    Type columnType;
                    if (isOneToOneRelation)
                    {
                        Type entryType = property.PropertyType.GetInterface("IDatabaseEntry");
                        if (entryType != null)
                        {
                            columnType = entryType.GetProperty("ID").PropertyType;
                            columnName = Util.GetColumnName(property.Name).ToUpper() + "_ID";
                        }
                        else // Property type is not a database entry, throw exception
                        {
                            throw new ForeignKeyException(tableName, property.Name);
                        }
                    }
                    else
                    {
                        columnType = property.PropertyType;
                        columnName = columnMode != ColumnMode.PRIMARY_KEY ? Util.GetColumnName(property.Name) : property.Name.ToUpper();
                    }

                    ConstructorInfo ctor;
                    object[] args;
                    Type columnDefinitionType = Util.MakeGenericColumnDefinition(columnType);

                    if (defaultValue == null)
                    {
                        ctor = MakeColumnConstructor(columnDefinitionType, new Type[] { typeof(string), typeof(string), typeof(ColumnMode), typeof(PropertyInfo), typeof(bool) });
                        args = new object[] { columnName, tableName, columnMode, property, isOneToOneRelation };
                    }
                    else
                    {
                        ctor = MakeColumnConstructor(columnDefinitionType, new Type[] { typeof(string), typeof(string), typeof(ColumnMode), columnType, typeof(PropertyInfo), typeof(bool) });
                        args = new object[] { columnName, tableName, columnMode, defaultValue, property, isOneToOneRelation };
                    }

                    columns.Add((IColumn)ctor.Invoke(args));
                }
            }

            return columns;
        }

        private static ConstructorInfo MakeColumnConstructor(Type columnDefinitionType, Type[] paramTypes)
        {
            return columnDefinitionType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, paramTypes, null);
        }

        private List<ForeignKeyData> GetForeignKeysFromClass(Type target)
        {
            List<ForeignKeyData> foreignKeyData = new List<ForeignKeyData>();

            PropertyInfo[] propertyInfos = target.GetProperties();
            PropertyInfo primaryKeyProperty = propertyInfos
                .FirstOrDefault(x => Attribute.IsDefined(x, typeof(SqlitePrimaryKey)));

            if (primaryKeyProperty == null)
            {
                return new List<ForeignKeyData>();
            }

            foreach (PropertyInfo property in propertyInfos)
            {
                if (Attribute.IsDefined(property, typeof(SqliteForeignKey)))
                {
                    SqliteForeignKey foreignKeyAttribute = (SqliteForeignKey)Attribute.GetCustomAttribute(property, typeof(SqliteForeignKey));
                    foreignKeyAttribute.Data.SetOneToManyData(primaryKeyProperty.Name.ToUpper(), property);
                    foreignKeyData.Add(foreignKeyAttribute.Data);
                }
            }

            return foreignKeyData;
        }
    }
}

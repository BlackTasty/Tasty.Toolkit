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

        /// <summary>
        /// Returns the CREATE statement for this table.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("CREATE TABLE IF NOT EXISTS \"{0}\" ({1});", name, ParseColumns());
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

            return sql_inner;
        }

        private static List<IColumn> GetColumnsFromClass(Type target, string tableName)
        {
            List<IColumn> columns = new List<IColumn>();

            foreach (PropertyInfo property in target.GetProperties().OrderByDescending(x => Attribute.IsDefined(x, typeof(SqlitePrimaryKey))))
            {
                if (!Attribute.IsDefined(property, typeof(SqliteIgnore)) && !Attribute.IsDefined(property, typeof(SqliteForeignKey)))
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

                    string columnName = columnMode != ColumnMode.PRIMARY_KEY ? Util.GetColumnName(property.Name) : property.Name.ToUpper();
                    switch (property.PropertyType.Name)
                    {
                        case "String":
                            if (defaultValue != null)
                            {
                                columns.Add(new ColumnDefinition<string>(columnName, tableName, columnMode, defaultValue, property));
                            }
                            else
                            {
                                columns.Add(new ColumnDefinition<string>(columnName, tableName, columnMode, property));
                            }
                            break;
                        case "Int32":
                            if (defaultValue != null)
                            {
                                columns.Add(new ColumnDefinition<int>(columnName, tableName, columnMode, defaultValue, property));
                            }
                            else
                            {
                                columns.Add(new ColumnDefinition<int>(columnName, tableName, columnMode, property));
                            }
                            break;
                        case "Double":
                            if (defaultValue != null)
                            {
                                columns.Add(new ColumnDefinition<double>(columnName, tableName, columnMode, defaultValue, property));
                            }
                            else
                            {
                                columns.Add(new ColumnDefinition<double>(columnName, tableName, columnMode, property));
                            }
                            break;
                        case "Single":
                            if (defaultValue != null)
                            {
                                columns.Add(new ColumnDefinition<float>(columnName, tableName, columnMode, defaultValue, property));
                            }
                            else
                            {
                                columns.Add(new ColumnDefinition<float>(columnName, tableName, columnMode, property));
                            }
                            break;
                        case "Int64":
                            if (defaultValue != null)
                            {
                                columns.Add(new ColumnDefinition<long>(columnName, tableName, columnMode, defaultValue, property));
                            }
                            else
                            {
                                columns.Add(new ColumnDefinition<long>(columnName, tableName, columnMode, property));
                            }
                            break;
                        case "DateTime":
                            if (defaultValue != null)
                            {
                                columns.Add(new ColumnDefinition<DateTime>(columnName, tableName, columnMode, defaultValue, property));
                            }
                            else
                            {
                                columns.Add(new ColumnDefinition<DateTime>(columnName, tableName, columnMode, property));
                            }
                            break;
                        default:
                            if (defaultValue != null)
                            {
                                columns.Add(new ColumnDefinition<object>(columnName, tableName, columnMode, defaultValue, property));
                            }
                            else
                            {
                                columns.Add(new ColumnDefinition<object>(columnName, tableName, columnMode, property));
                            }
                            break;
                    }
                }
            }

            return columns;
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
                    foreignKeyAttribute.Data.SetParentData(primaryKeyProperty.Name.ToUpper(), property);
                    foreignKeyData.Add(foreignKeyAttribute.Data);
                }
            }

            return foreignKeyData;
        }
    }
}

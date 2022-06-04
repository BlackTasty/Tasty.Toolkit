using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Globalization;
using System.Reflection;
using Tasty.SQLiteManager.Exceptions;
using Tasty.SQLiteManager.Table.Attributes;

namespace Tasty.SQLiteManager.Table.Column
{
    /// <summary>
    /// Used to define columns in a <see cref="TableDefinition{T}"/>
    /// </summary>
    /// <typeparam name="T">Column type</typeparam>
    public class ColumnDefinition<T> : DefinitionBase, IColumn
    {
        private readonly Type dataType;
        private readonly Type underlyingType;
        private readonly PropertyInfo propertyInfo;
        private string parentTableName;
        private bool isForeignKey;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Type DataType => dataType;

        public Type UnderlyingType => underlyingType;

        /// <inheritdoc/>
        public PropertyInfo PropertyInfo => propertyInfo;

        /// <inheritdoc/>
        public string ParentTableName
        {
            get => parentTableName;
            internal set => parentTableName = value;
        }

        /// <inheritdoc/>
        public bool IsForeignKey => isForeignKey;

        #region Property definitions
        private readonly T defaultValue = default;
        private readonly ColumnType columnType;
        private bool notNull = false;
        private bool primaryKey = false;
        private bool autoIncrement = false;
        private bool unique = false;

        private readonly string stringFormatter = null; //Just in case a special type like DateTime is used as column type
        private readonly string trueColumnType = null; //Just in case a special type like DateTime is used as column type. Only use this field for column types which are not defined by SQLite!

        /// <inheritdoc/>
        public dynamic DefaultValue => defaultValue;

        /// <inheritdoc/>
        public ColumnType ColumnType => columnType;

        /// <inheritdoc/>
        public bool NotNull => notNull;

        /// <inheritdoc/>
        public bool PrimaryKey => primaryKey;

        /// <summary>
        /// Returns if this column should auto-increment with each insert
        /// </summary>
        public bool AutoIncrement => autoIncrement;

        /// <inheritdoc/>
        public bool Unique => unique;

        /// <inheritdoc/>
        public string StringFormatter => stringFormatter;
        #endregion

        /// <summary>
        /// Define a new column with the specified name
        /// </summary>
        /// <param name="name">The name of the column</param>
        [SqliteConstructor]
        public ColumnDefinition(string name, string parentTableName, bool isForeignKey)
        {
            this.isForeignKey = isForeignKey;
            this.parentTableName = parentTableName;
            dataType = typeof(T);
            underlyingType = Nullable.GetUnderlyingType(dataType);

            if (IsOfType(dataType, underlyingType, typeof(int)))
            {
                columnType = ColumnType.INTEGER;
            }
            else if (IsOfType(dataType, underlyingType, typeof(float)) ||
                        IsOfType(dataType, underlyingType, typeof(double)))
            {
                columnType = ColumnType.FLOAT;
            }
            else if (IsOfType(dataType, underlyingType, typeof(string)))
            {
                columnType = ColumnType.TEXT;
            }
            else if (IsOfType(dataType, underlyingType, typeof(DateTime)))
            {
                columnType = ColumnType.TEXT;
                stringFormatter = "dd-MM-yyyy HH:mm:ss";
                trueColumnType = "datetime";
            }
            else if (IsOfType(dataType, underlyingType, typeof(TimeSpan)))
            {
                columnType = ColumnType.TEXT;
                stringFormatter = "hh\\:mm\\:ss";
                trueColumnType = "timespan";
            }
            else if (IsOfType(dataType, underlyingType, typeof(ulong)))
            {
                columnType = ColumnType.TEXT;
                trueColumnType = "ulong";
            }
            else if (IsOfType(dataType, underlyingType, typeof(bool)))
            {
                columnType = ColumnType.BOOLEAN;
            }
            else if (IsTypeEnum(dataType, underlyingType))
            {
                columnType = ColumnType.INTEGER;
                trueColumnType = "enum";
            }
            else
            {
                columnType = ColumnType.OBJECT;
            }

            base.name = name;
        }

        /// <summary>
        /// Define a new column with the specified name
        /// </summary>
        /// <param name="name">The name of the column</param>
        /// <param name="columnMode">Set the column to be one of the following:
        /// - NOT_NULL: null not allowed as value
        /// - UNIQUE: Value can only appear once in table
        /// - PRIMARY_KEY: Only on columns with type <see cref="int"/>! Define this column as primary key, auto-increments with each insert into table</param>
        public ColumnDefinition(string name, string parentTableName, ColumnMode columnMode, bool isForeignKey) :
            this(name, parentTableName, isForeignKey)
        {
            SetColumnMode(columnMode);
        }

        /// <summary>
        /// Define a new column with the specified name
        /// </summary>
        /// <param name="name">The name of the column</param>
        /// <param name="defaultValue">Define a default value for this column</param>
        public ColumnDefinition(string name, string parentTableName, T defaultValue, bool isForeignKey) :
            this(name, parentTableName, isForeignKey)
        {
            this.defaultValue = defaultValue;
        }

        /// <summary>
        /// Define a new column with the specified name
        /// </summary>
        /// <param name="name">The name of the column</param>
        /// <param name="columnMode">Set the column to be one of the following:
        /// - NOT_NULL: null not allowed as value
        /// - UNIQUE: Value can only appear once in table
        /// - PRIMARY_KEY: Only on columns with type <see cref="int"/>! Define this column as primary key, auto-increments with each insert into table</param>
        /// <param name="defaultValue">Define a default value for this column</param>
        public ColumnDefinition(string name, string parentTableName, ColumnMode columnMode, T defaultValue, bool isForeignKey) :
            this(name, parentTableName, defaultValue, isForeignKey)
        {
            SetColumnMode(columnMode);
        }

        internal ColumnDefinition(string name, string parentTableName, ColumnMode columnMode, T defaultValue, PropertyInfo propertyInfo, bool isForeignKey) :
            this(name, parentTableName, columnMode, defaultValue, isForeignKey)
        {
            this.propertyInfo = propertyInfo;
        }

        internal ColumnDefinition(string name, string parentTableName, ColumnMode columnMode, PropertyInfo propertyInfo, bool isForeignKey) :
            this(name, parentTableName, columnMode, isForeignKey)
        {
            this.propertyInfo = propertyInfo;
        }

        /// <inheritdoc/>
        public string ParseToDatabaseValue(dynamic value)
        {
            if (value == null)
            {
                return "NULL";
            }

            if (propertyInfo != null && isForeignKey) // Column is foreign key
            {
                SqliteForeignKey foreignKeyAttribute = propertyInfo.GetCustomAttribute<SqliteForeignKey>();
                if (foreignKeyAttribute.Data.IsOneToOne && value is IDatabaseEntry foreignEntry) // Foreign column key has one-to-one relation
                {
                    return foreignEntry.ID.ToString();
                }
            }

            if (columnType == ColumnType.TEXT)
            {
                if (stringFormatter == null)
                {
                    return "'" + ((string)value).Replace("\"", "''").Replace("'", "''") + "'";
                    //return "\"data://" + Base64Encoding.EncodeBase64(Encoding.UTF8, value) + "\"";
                }
                else
                {
                    return "\"" + ParseSpecialColumnValue(value) + "\"";
                }
            }
            else if (columnType == ColumnType.BOOLEAN)
            {
                return value ? "1" : "0";
            }
            else if (columnType == ColumnType.FLOAT)
            {
                return value.ToString().Replace(',', '.');
            }
            else if (!string.IsNullOrWhiteSpace(trueColumnType))
            {
                try
                {
                    return ParseSpecialColumnValue(value);
                }
                catch (RuntimeBinderException)
                {
                    throw new ColumnParseException(typeof(T).Name);
                }
            }
            else
            {
                return value.ToString();
            }
        }

        public dynamic ParseFromDatabaseValue(dynamic value)
        {
            switch (trueColumnType)
            {
                case "datetime":
                    return DateTime.TryParseExact(value, stringFormatter,
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime) ?
                        dateTime : default;
                case "timespan":
                    return TimeSpan.TryParseExact(value, stringFormatter,
                        CultureInfo.InvariantCulture, out TimeSpan timeSpan) ?
                        timeSpan : default;
                case "ulong":
                    return ulong.TryParse(value, out ulong result) ? result : default;
                case "enum":
                    return (T)value;
                default:
                    return value;
            }
        }

        private string ParseSpecialColumnValue(dynamic value)
        {
            switch (trueColumnType)
            {
                case "datetime":
                    if (value is DateTime date)
                    {
                        return date.ToString(stringFormatter);
                    }
                    return default(DateTime).ToString(stringFormatter);
                case "timespan":
                    if (value is TimeSpan time)
                    {
                        return time.ToString();
                    }
                    return default(TimeSpan).ToString();
                case "ulong":
                    return value.ToString();
                case "enum":
                    return ((int)value).ToString();
                default:
                    return value;
            }
        }

        /// <summary>
        /// NOT IMPLEMENTED!
        /// </summary>
        /// <returns></returns>
        public string GetInsertQuery()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// NOT IMPLEMENTED!
        /// </summary>
        /// <returns></returns>
        public string GetUpdateQuery()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// NOT IMPLEMENTED!
        /// </summary>
        /// <returns></returns>
        public string GetDeleteQuery()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the CREATE statement for this column.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("\"{0}\" {1} {2}", name, ParseColumnType(), ParseOptions());
        }

        private void SetColumnMode(ColumnMode columnMode)
        {
            switch (columnMode)
            {
                case ColumnMode.NOT_NULL:
                    notNull = true;
                    break;
                case ColumnMode.PRIMARY_KEY:
                    primaryKey = true;
                    notNull = true;
                    autoIncrement = true;
                    break;
                case ColumnMode.UNIQUE:
                    unique = true;
                    break;
            }
        }

        private string ParseColumnType()
        {
            switch (columnType)
            {
                case ColumnType.BOOLEAN:
                case ColumnType.INTEGER:
                    return "INTEGER";
                case ColumnType.FLOAT:
                    return "REAL";
                case ColumnType.TEXT:
                    return "TEXT";
                default:
                    return "BLOB";
            }
        }

        private string ParseOptions()
        {
            string options = "";

            if (primaryKey)
            {
                options += "NOT NULL PRIMARY KEY AUTOINCREMENT";
            }

            if (unique)
            {
                options += options == "" ? "UNIQUE" : " UNIQUE";
            }

            return options;
        }

        private bool IsOfType(Type columnType, Type underlyingType, Type target)
        {
            return underlyingType != null ? underlyingType == target : columnType == target;
        }

        private bool IsTypeEnum(Type columnType, Type underlyingType)
        {
            return underlyingType != null ? underlyingType.IsEnum : columnType.IsEnum;
        }
    }
}

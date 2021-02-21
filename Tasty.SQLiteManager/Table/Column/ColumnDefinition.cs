using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Table.Column
{
    /// <summary>
    /// Used to define columns in a <see cref="TableDefinition"/>
    /// </summary>
    /// <typeparam name="T">Column type</typeparam>
    public class ColumnDefinition<T> : DefinitionBase, IColumn
    {
        #region Property definitions
        private T defaultValue = default(T);
        private ColumnType columnType;
        private bool notNull = false;
        private bool primaryKey = false;
        private bool autoIncrement = false;
        private bool unique = false;

        private string stringFormatter = null; //Just in case a special type like DateTime is used as column type
        private string trueColumnType = null; //Just in case a special type like DateTime is used as column type. Only use this field for column types which are not defined by SQLite!

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
        public ColumnDefinition(string name)
        {
            Type inputType = typeof(T);
            
            if (inputType == typeof(int))
            {
                columnType = ColumnType.INTEGER;
            }
            else if (inputType == typeof(float) || inputType == typeof(double))
            {
                columnType = ColumnType.FLOAT;
            }
            else if (inputType == typeof(string))
            {
                columnType = ColumnType.TEXT;
            }
            else if (inputType == typeof(DateTime))
            {
                columnType = ColumnType.TEXT;
                stringFormatter = "dd-MM-yyyy HH:mm:ss";
                trueColumnType = "datetime";
            }
            else if (inputType == typeof(ulong))
            {
                columnType = ColumnType.TEXT;
                trueColumnType = "ulong";
            }
            else if (inputType == typeof(bool))
            {
                columnType = ColumnType.BOOLEAN;
            }
            else
            {
                columnType = ColumnType.OBJECT;
            }

            base.name = name;
            //this.columnType = columnType;
        }

        /// <summary>
        /// Define a new column with the specified name
        /// </summary>
        /// <param name="name">The name of the column</param>
        /// <param name="columnMode">Set the column to be one of the following:
        /// - NOT_NULL: null not allowed as value
        /// - UNIQUE: Value can only appear once in table
        /// - PRIMARY_KEY: Only on columns with type <see cref="int"/>! Define this column as primary key, auto-increments with each insert into table</param>
        public ColumnDefinition(string name, ColumnMode columnMode) : 
            this(name)
        {
            SetColumnMode(columnMode);
        }

        /// <summary>
        /// Define a new column with the specified name
        /// </summary>
        /// <param name="name">The name of the column</param>
        /// <param name="defaultValue">Define a default value for this column</param>
        public ColumnDefinition(string name, T defaultValue) : 
            this(name)
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
        public ColumnDefinition(string name, ColumnMode columnMode, T defaultValue) : 
            this(name, defaultValue)
        {
            SetColumnMode(columnMode);
        }

        /// <inheritdoc/>
        public string ParseColumnValue(dynamic value)
        {
            if (value == null)
            {
                return "NULL";
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
                return ParseSpecialColumnValue(value);
            }
            else
            {
                return value.ToString();
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
                case "ulong":
                    return default(ulong).ToString();
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
    }
}

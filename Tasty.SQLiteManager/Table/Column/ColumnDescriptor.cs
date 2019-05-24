using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Table.Column
{
    public class ColumnDescriptor<T> : DescriptorBase, IColumn
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

        public dynamic DefaultValue => defaultValue;

        public ColumnType ColumnType => columnType;

        public bool NotNull => notNull;

        public bool PrimaryKey => primaryKey;

        public bool AutoIncrement => autoIncrement;

        public bool Unique => unique;

        public string StringFormatter => stringFormatter;
        #endregion

        public ColumnDescriptor(string name)
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

        public ColumnDescriptor(string name, ColumnMode columnMode) : 
            this(name)
        {
            SetColumnMode(columnMode);
        }

        public ColumnDescriptor(string name, T defaultValue) : 
            this(name)
        {
            this.defaultValue = defaultValue;
        }

        public ColumnDescriptor(string name, ColumnMode columnMode, T defaultValue) : 
            this(name, defaultValue)
        {
            SetColumnMode(columnMode);
        }

        public string ParseColumnValue(dynamic value)
        {
            if (columnType == ColumnType.TEXT)
            {
                if (stringFormatter == null)
                {
                    return "\"" + value + "\"";
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
                default:
                    return value;
            }
        }

        public string GetInsertQuery()
        {
            throw new NotImplementedException();
        }

        public string GetUpdateQuery()
        {
            throw new NotImplementedException();
        }

        public string GetDeleteQuery()
        {
            throw new NotImplementedException();
        }

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

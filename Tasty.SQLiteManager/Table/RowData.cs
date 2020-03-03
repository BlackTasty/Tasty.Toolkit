using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Table
{
    public class RowData
    {
        Dictionary<string, dynamic> columns = new Dictionary<string, dynamic>();

        /// <summary>
        /// The data for each column. Key is the column name, value is the value (duh)
        /// </summary>
        public Dictionary<string, dynamic> Columns
        {
            get => columns;
            set => columns = value;
        }

        public RowData(params KeyValuePair<string, dynamic>[] columns)
        {
            foreach (KeyValuePair<string, dynamic> column in columns)
            {
                this.columns.Add(column.Key, column.Value);
            }
        }

        public RowData(Dictionary<string, dynamic> columns)
        {
            this.columns = columns;
        }

        public bool ColumnExists(string columnName)
        {
            return Columns.ContainsKey(columnName);
        }

        public dynamic GetColumn(string columnName, char columnType = 'a')
        {
            if (ColumnExists(columnName))
            {
                dynamic value = Columns[columnName];

                if (value == null && columnType != 'a')
                {
                    return GetDefaultValue(columnType);
                }
                else
                {
                    return Columns[columnName];
                }
            }
            else
            {
                return GetDefaultValue(columnType);
            }
        }

        private dynamic GetDefaultValue(char columnType)
        {
            switch (columnType)
            {
                case 'i':
                    return default(int);
                case 'b':
                    return default(bool);
                case 'd':
                    return default(double);
                case 't':
                    return DateTime.Now;
                default:
                    return null;
            }
        }
    }
}

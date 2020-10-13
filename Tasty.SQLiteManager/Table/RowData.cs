using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Table
{
    /// <summary>
    /// Contains a single row of a <see cref="ResultSet"/>
    /// </summary>
    public class RowData
    {
        Dictionary<string, dynamic> columns = new Dictionary<string, dynamic>();

        /// <summary>
        /// The data for each column. Key is the column name, value is the value of the column
        /// </summary>
        public Dictionary<string, dynamic> Columns
        {
            get => columns;
            set => columns = value;
        }

        /// <summary>
        /// Returns the value of the column with the specified name.
        /// </summary>
        /// <param name="columnName">The name of the column</param>
        /// <returns></returns>
        public dynamic this[string columnName]
        {
            get => columns.FirstOrDefault(x => x.Key == columnName).Value;
        }

        /// <summary>
        /// Returns the value of the column at the specified index.
        /// </summary>
        /// <param name="index">The index of the column</param>
        /// <returns></returns>
        public dynamic this[int index]
        {
            get => columns.ElementAt(index).Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="columns"></param>
        public RowData(params KeyValuePair<string, dynamic>[] columns)
        {
            foreach (KeyValuePair<string, dynamic> column in columns)
            {
                this.columns.Add(column.Key, column.Value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="columns"></param>
        public RowData(Dictionary<string, dynamic> columns)
        {
            this.columns = columns;
        }

        /// <summary>
        /// Check if the specified column exists
        /// </summary>
        /// <param name="columnName">The name of the column</param>
        /// <returns></returns>
        public bool ColumnExists(string columnName)
        {
            return Columns.ContainsKey(columnName);
        }

        /// <summary>
        /// Returns the value for the specified column
        /// </summary>
        /// <param name="columnName">The name of the column</param>
        /// <param name="columnType">The type of column. Available values:
        /// 'a': Treat the value as a <see cref="string"/>
        /// 'i': Treat the value as an <see cref="int"/>
        /// 'd': Treat the value as a <see cref="double"/>
        /// 'b': Treat the value as a <see cref="bool"/>
        /// 't': Treat the value as a <see cref="DateTime"/> object</param>
        /// <returns></returns>
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

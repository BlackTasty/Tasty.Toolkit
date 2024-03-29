﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Table.Attributes
{
    /// <summary>
    /// Attribute for defining a class as a table.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SqliteTable : Attribute
    {
        private string tableName;

        public string TableName
        {
            get => tableName;
            internal set
            {
                tableName = value;
            }
        }

        public bool AutoName => tableName == null;

        /// <summary>
        /// Tell the sqlite manager library to create a table with this class, and generate the table name from class name
        /// </summary>
        public SqliteTable()
        {

        }

        /// <summary>
        /// Tell the SQLiteManager library to create a table with this class, and set custom table name
        /// </summary>
        /// <param name="tableName">The name for the table</param>
        public SqliteTable(string tableName)
        {
            this.tableName = tableName;
        }
    }
}

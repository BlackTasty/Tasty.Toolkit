﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Table.ForeignKey
{
    internal class ChildTableData
    {
        private readonly string tableName;
        private readonly List<ForeignKeyData> foreignKeys;

        public string TableName => tableName;

        public List<ForeignKeyData> ForeignKeys => foreignKeys;

        public ChildTableData(ForeignKeyData remoteKeyData, ForeignKeyData rootKeyData)
        {
            tableName = remoteKeyData.ChildTableName;
            foreignKeys = new List<ForeignKeyData>() {
                remoteKeyData,
                rootKeyData
            };
        }
    }
}
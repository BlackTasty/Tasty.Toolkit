using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Table.ForeignKey
{
    internal class ChildTableData
    {
        private readonly string tableName;
        private readonly List<ForeignKeyData> foreignKeyData;

        internal string TableName => tableName;

        internal List<ForeignKeyData> ForeignKeyData => foreignKeyData;

        internal bool IsManyToMany => foreignKeyData.FirstOrDefault()?.IsManyToMany ?? false;

        internal ChildTableData(ForeignKeyData remoteKeyData, ForeignKeyData rootKeyData)
        {
            tableName = remoteKeyData.ChildTableName;
            foreignKeyData = new List<ForeignKeyData>() {
                remoteKeyData,
                rootKeyData
            };
        }
    }
}

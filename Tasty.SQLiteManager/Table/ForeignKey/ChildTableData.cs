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
        private readonly bool isSameTableRelation;

        internal string TableName => tableName;

        internal List<ForeignKeyData> ForeignKeyData => foreignKeyData;

        internal bool IsManyToMany => foreignKeyData.FirstOrDefault()?.IsManyToMany ?? false;

        internal bool IsSameTableRelation => isSameTableRelation;

        internal ChildTableData(ForeignKeyData remoteKeyData, ForeignKeyData rootKeyData, bool isSameTableRelation)
        {
            tableName = remoteKeyData.ChildTableName;
            foreignKeyData = new List<ForeignKeyData>() {
                remoteKeyData,
                rootKeyData
            };
            this.isSameTableRelation = isSameTableRelation;
        }
    }
}

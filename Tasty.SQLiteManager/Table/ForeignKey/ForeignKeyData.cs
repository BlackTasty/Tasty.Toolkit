using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tasty.SQLiteManager.Table.Attributes;

namespace Tasty.SQLiteManager.Table.ForeignKey
{
    public class ForeignKeyData
    {
        private string parentTableName;

        private string childTableName;
        private string foreignKeyName;
        private string parentKeyName;
        private Type keyType;

        internal string ChildTableName => childTableName;

        internal string ForeignKeyName => foreignKeyName;

        internal string ParentKeyName => parentKeyName;

        internal Type KeyType => keyType;

        internal ForeignKeyData(string childTableName)
        {
            this.childTableName = childTableName;
        }

        internal ForeignKeyData(string parentKeyName, string foreignKeyName, Type keyType, string parentTableName)
        {
            this.parentKeyName = parentKeyName;
            this.foreignKeyName = foreignKeyName;
            this.keyType = keyType;
            this.parentTableName = parentTableName;
        }

        internal void SetParentData(string parentKeyName, PropertyInfo propertyInfo)
        {
            this.parentKeyName = parentKeyName;
            Type realType = GetRealType(propertyInfo.PropertyType);
            keyType = GetPrimaryKeyTypeFromType(realType);
            foreignKeyName = string.Format("{0}_{1}", realType.Name.ToUpper(), parentKeyName);

            SqliteTable sqliteTableAttribute = (SqliteTable)realType.GetCustomAttribute(typeof(SqliteTable));
            this.parentTableName = sqliteTableAttribute.AutoName ? Util.GetTableName(realType.Name) : sqliteTableAttribute.TableName;
        }

        private Type GetRealType(Type rootType)
        {
            return rootType.IsGenericType ? rootType.GenericTypeArguments.FirstOrDefault() : rootType;
        }

        private Type GetPrimaryKeyTypeFromType(Type type)
        {
            PropertyInfo primaryKeyProperty = type.GetProperties().FirstOrDefault(x => Attribute.IsDefined(x, typeof(SqlitePrimaryKey)));

            return primaryKeyProperty.PropertyType;
        }

        public override string ToString()
        {
            return string.Format("FOREIGN KEY (\"{0}\") REFERENCES \"{1}\"(\"{2}\") ON DELETE SET NULL", foreignKeyName, parentTableName, parentKeyName);
        }
    }
}

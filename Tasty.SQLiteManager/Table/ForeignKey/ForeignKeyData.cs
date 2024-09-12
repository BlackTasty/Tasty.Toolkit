using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tasty.SQLiteManager.Exceptions;
using Tasty.SQLiteManager.Table.Attributes;

namespace Tasty.SQLiteManager.Table.ForeignKey
{
    /// <summary>
    /// For internal use. Holds mapping data for relationship tables.
    /// </summary>
    public class ForeignKeyData
    {
        private string parentTableName;
        private string childTableName;
        private string foreignKeyName;
        private string parentKeyName;
        private Type keyType;

        private bool isList;
        private bool isOneToOne;
        private bool isManyToMany;
        private string manyToManyTargetKeyName;

        internal string ParentTableName
        {
            get => parentTableName;
            set => parentTableName = value;
        }

        internal string ChildTableName
        {
            get => childTableName;
            set => childTableName = value;
        }

        internal string ForeignKeyName
        {
            get => foreignKeyName;
            set => foreignKeyName = value;
        }

        internal string ParentKeyName => parentKeyName;

        internal Type KeyType => keyType;

        internal bool IsList => isList;

        internal bool IsOneToOne => isOneToOne;

        internal bool IsManyToMany => isManyToMany;

        internal string ManyToManyTargetKeyName
        {
            get => manyToManyTargetKeyName;
            set => manyToManyTargetKeyName = value;
        }

        internal ForeignKeyData(bool isOneToOne)
        {
            this.isOneToOne = isOneToOne;
        }

        internal ForeignKeyData(string childTableName, bool isManyToMany)
        {
            this.childTableName = childTableName;
            this.isManyToMany = isManyToMany;
        }

        internal ForeignKeyData(string parentKeyName, string foreignKeyName, Type keyType, string parentTableName)
        {
            SetOneToOneData(parentKeyName, foreignKeyName, keyType, parentTableName);
        }

        internal void SetOneToOneData(string parentKeyName, string foreignKeyName, Type keyType, string parentTableName)
        {
            this.parentKeyName = parentKeyName;
            this.foreignKeyName = foreignKeyName;
            this.keyType = keyType;
            this.parentTableName = parentTableName;
        }

        internal void SetOneToManyData(string parentKeyName, PropertyInfo propertyInfo)
        {
            isList = propertyInfo.PropertyType.IsGenericType;

            this.parentKeyName = parentKeyName;
            Type realType = GetRealType(propertyInfo.PropertyType);
            keyType = GetPrimaryKeyTypeFromType(realType);
            foreignKeyName = string.Format("{0}_{1}", Util.GetColumnName(realType.Name).ToUpper(), parentKeyName);

            SqliteTable sqliteTableAttribute = (SqliteTable)realType.GetCustomAttribute(typeof(SqliteTable), true);
            this.parentTableName = sqliteTableAttribute.AutoName ? Util.GetTableName(realType.Name) : sqliteTableAttribute.TableName;
        }

        internal void SetManyToManyData(string parentKeyName, PropertyInfo propertyInfo, string foreignKeyName)
        {
            SetOneToManyData(parentKeyName, propertyInfo);
            this.foreignKeyName = foreignKeyName;
        }

        private Type GetRealType(Type rootType)
        {
            return rootType.IsGenericType ? rootType.GenericTypeArguments.FirstOrDefault() : rootType;
        }

        private Type GetPrimaryKeyTypeFromType(Type type)
        {
            PropertyInfo primaryKeyProperty = GetPrimaryKeyProperty(type);

            if (primaryKeyProperty == null && type.GetInterface("IDatabaseEntry") is Type iDBEntry)
            {
                primaryKeyProperty = GetPrimaryKeyProperty(iDBEntry);
            }

            if (primaryKeyProperty == null)
            {
                throw new MissingPrimaryKeyException(type);
            }
            return primaryKeyProperty.PropertyType;
        }

        private PropertyInfo GetPrimaryKeyProperty(Type type)
        {
            return type.GetProperties().FirstOrDefault(x => Attribute.IsDefined(x, typeof(SqlitePrimaryKey)));
        }

        public override string ToString()
        {
            return isList ? string.Format("FOREIGN KEY (\"{0}\") REFERENCES \"{1}\"(\"{2}\") ON DELETE SET NULL", foreignKeyName, parentTableName, parentKeyName) 
                        : null;
        }
    }
}

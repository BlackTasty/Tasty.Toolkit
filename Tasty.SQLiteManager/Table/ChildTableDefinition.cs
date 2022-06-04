using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tasty.SQLiteManager.Table.Attributes;
using Tasty.SQLiteManager.Table.Column;
using Tasty.SQLiteManager.Table.ForeignKey;

namespace Tasty.SQLiteManager.Table
{
    /// <summary>
    /// Defines a child table, containing foreign keys and an ID column. Auto-generated when using the <see cref="SqliteForeignKey"/> attribute on list properties.
    /// </summary>
    public class ChildTableDefinition : TableBaseDefinition
    {
        private readonly List<ForeignKeyData> foreignKeyData;
        private readonly bool isSameTableRelation;

        public bool IsSameTableRelation => isSameTableRelation;

        internal ChildTableDefinition(ChildTableData tableData) : base(tableData.TableName)
        {
            isSameTableRelation = tableData.IsSameTableRelation;

            #region Sanity check on foreign key data
            List<ForeignKeyData> sanitized = new List<ForeignKeyData>();
            foreach(ForeignKeyData foreignKeyData in tableData.ForeignKeyData)
            {
                if (!sanitized.Any(x => x.ToString().Equals(foreignKeyData.ToString())))
                {
                    sanitized.Add(foreignKeyData);
                }
            }
            #endregion
            foreignKeyData = sanitized;

            Add(new ColumnDefinition<int>("ID", name, ColumnMode.PRIMARY_KEY, false));

            #region Generate tables for each foreign key
            foreach (ForeignKeyData foreignKeyData in foreignKeyData)
            {
                Type columnDefinitionType = Util.MakeGenericColumnDefinition(foreignKeyData.KeyType);

                ConstructorInfo ctor = columnDefinitionType.GetConstructors()
                    .FirstOrDefault(x => Attribute.IsDefined(x, typeof(SqliteConstructor)));

                if (ctor != null)
                {
                    IColumn column = (IColumn)ctor.Invoke(new object[] { foreignKeyData.ForeignKeyName, tableData.TableName, true });
                    if (!this.Any(x => x.Name == column.Name))
                    {
                        Add(column);
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// Returns the linked column for the parent table.
        /// </summary>
        /// <param name="parentTableName">The name of the parent table whose linked column you want to return.</param>
        /// <returns>The child <see cref="ColumnDefinition{T}"/> object or null for the parent table.</returns>
        public IColumn GetChildColumnByParentTable(string parentTableName)
        {
            ForeignKeyData target = foreignKeyData.FirstOrDefault(x => x.ParentTableName == parentTableName);
            return this.FirstOrDefault(x => x.Name == target.ForeignKeyName);
        }

        public override string ToString()
        {
            return string.Format("CREATE TABLE IF NOT EXISTS \"{0}\" ({1});", name, GetQueryData());
        }

        private string GetQueryData()
        {
            string sql_inner = null;
            foreach (IColumn column in columns)
            {
                if (sql_inner == null)
                {
                    sql_inner = "\n\t" + column.ToString();
                }
                else
                {
                    sql_inner += ",\n\t" + column.ToString();
                }
            }

            foreach (ForeignKeyData foreignKey in foreignKeyData)
            {
                string foreignKeySql = foreignKey.ToString();
                if (foreignKeySql != null)
                {
                    sql_inner += ",\n\t" + foreignKeySql;
                }
            }

            return sql_inner;
        }
    }
}

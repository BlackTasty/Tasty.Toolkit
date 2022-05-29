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
        private readonly List<ForeignKeyData> foreignKeys;

        internal ChildTableDefinition(ChildTableData tableData) : base(tableData.TableName)
        {
            foreignKeys = tableData.ForeignKeys;

            Add(new ColumnDefinition<int>("ID", name, ColumnMode.PRIMARY_KEY));

            foreach (ForeignKeyData foreignKeyData in tableData.ForeignKeys)
            {
                Type columnDefinitionType = Util.MakeGenericColumnDefinition(foreignKeyData.KeyType);

                ConstructorInfo ctor = columnDefinitionType.GetConstructors()
                    .Where(x => Attribute.IsDefined(x, typeof(SqliteConstructor))).FirstOrDefault();

                if (ctor != null)
                {
                    Add((IColumn)ctor.Invoke(new object[] { foreignKeyData.ForeignKeyName, tableData.TableName }));
                }
            }
        }

        public IColumn GetChildColumnByParentTable(string parentTableName)
        {
            ForeignKeyData target = foreignKeys.FirstOrDefault(x => x.ParentTableName == parentTableName);
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

            foreach (ForeignKeyData foreignKey in foreignKeys)
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

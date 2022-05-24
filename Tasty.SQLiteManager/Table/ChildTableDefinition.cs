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
    class ChildTableDefinition : TableBaseDefinition
    {
        private List<ForeignKeyData> foreignKeys;

        public ChildTableDefinition(ChildTableData tableData) : base(tableData.TableName)
        {
            foreignKeys = tableData.ForeignKeys;

            Add(new ColumnDefinition<int>("ID", ColumnMode.PRIMARY_KEY));

            foreach (ForeignKeyData foreignKeyData in tableData.ForeignKeys)
            {
                Type columnDefinitionType = typeof(ColumnDefinition<>)
                    .MakeGenericType(new Type[] { foreignKeyData.KeyType });

                ConstructorInfo ctor = columnDefinitionType.GetConstructors()
                    .Where(x => Attribute.IsDefined(x, typeof(SqliteConstructor))).FirstOrDefault();

                if (ctor != null)
                {
                    Add((IColumn)ctor.Invoke(new object[] { foreignKeyData.ForeignKeyName }));
                }

                //IColumn foreignColumn = 
            }
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
                sql_inner += ",\n\t" + foreignKey.ToString();
            }

            return sql_inner;
        }
    }
}

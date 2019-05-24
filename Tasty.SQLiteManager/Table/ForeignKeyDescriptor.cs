using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.SQLiteManager.Table.Column;

namespace Tasty.SQLiteManager.Table
{
    public class ForeignKeyDescriptor : DescriptorBase
    {
        private TableDescriptor targetTable;
        private IColumn targetKey;
        private IColumn foreignKey;

        public ForeignKeyDescriptor(string name, TableDescriptor targetTable, IColumn foreignKey, IColumn targetKey)
        {
            this.targetKey = targetKey;
            this.foreignKey = foreignKey;
            this.targetTable = targetTable;
            this.name = name;
        }

        public override string ToString()
        {
            string targetKeyName = (targetKey as DescriptorBase).Name;
            string foreignKeyName = (foreignKey as DescriptorBase).Name;

            if (!targetTable.ColumnExists(targetKey))
            {
                throw new KeyNotFoundException("The target column with the name \"" + targetKeyName +
                    "\" doesn't exist in the table \"" + targetTable.Name + "\"");
            }
            
            return string.Format("FOREIGN KEY(\"{0}\") REFERENCES \"{1}\"(\"{2}\") {3}",
                foreignKeyName, targetTable.Name, targetKeyName, ParseConstraintType());
        }

        private string ParseConstraintType()
        {
            //TODO: Implement multiple constraints
            return "ON DELETE SET NULL";
        }
    }
}

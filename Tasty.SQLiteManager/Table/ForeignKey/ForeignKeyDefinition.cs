using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.SQLiteManager.Table.Column;

namespace Tasty.SQLiteManager.Table.ForeignKey
{
    public class ForeignKeyDefinition
    {
        private ITable parentTable;
        private IColumn parentColumn;
        private IColumn foreignColumn;
    }
}

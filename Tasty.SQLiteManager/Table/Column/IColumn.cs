using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Table.Column
{
    public interface IColumn
    {
        string Name { get; }

        dynamic DefaultValue { get; }

        ColumnType ColumnType { get; }

        bool NotNull { get; }

        bool PrimaryKey { get; }

        bool Unique { get; }

        string StringFormatter { get; }

        string ParseColumnValue(dynamic value);
    }
}

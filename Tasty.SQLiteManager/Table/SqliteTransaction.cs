using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.SQLiteManager.Table.Column;

namespace Tasty.SQLiteManager.Table
{
    /// <summary>
    /// Used for creating transactions to the database (insert/update/delete)
    /// </summary>
    public class SqliteTransaction
    {
        private Dictionary<IColumn, dynamic> data;

    }
}

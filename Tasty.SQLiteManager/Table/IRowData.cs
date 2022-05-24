using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Table
{
    public interface IRowData
    {
        Dictionary<string, dynamic> Columns { get; set; }


    }
}

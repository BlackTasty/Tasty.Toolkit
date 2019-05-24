using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Table
{
    class TableData
    {
        private List<RowData> data = new List<RowData>();

        public List<RowData> Data
        {
            get => data;
            set => data = value;
        }

        public TableData()
        {

        }

        public TableData(List<RowData> data)
        {
            this.data = data;
        }
    }
}

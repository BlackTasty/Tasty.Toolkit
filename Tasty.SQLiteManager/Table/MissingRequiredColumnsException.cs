using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.SQLiteManager.Table.Column;

namespace Tasty.SQLiteManager.Table
{
    class MissingRequiredColumnsException : Exception
    {
        private List<IColumn> missingColumns = new List<IColumn>();

        public List<IColumn> MissingColumns { get => missingColumns; }

        public MissingRequiredColumnsException() : base()
        {
        }

        public MissingRequiredColumnsException(string message) : base(message)
        {
            
        }

        public MissingRequiredColumnsException(string message, List<IColumn> missingColumns) : this(message)
        {
            this.missingColumns = missingColumns;
        }

        public MissingRequiredColumnsException(string message, Exception inner) : base (message, inner)
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Exceptions
{
    class ForeignKeyException : Exception
    {
        public ForeignKeyException(string tableName, string propertyName) : 
            base(string.Format("Table \"{0}\" > column \"{1}\" is defined as a one-to-one foreign key, but there is no database entry object to bind to!", tableName, propertyName))
        {

        }
    }
}

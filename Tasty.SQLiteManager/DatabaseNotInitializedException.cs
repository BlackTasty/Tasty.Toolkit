using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager
{
    class DatabaseNotInitializedException : Exception
    {
        public DatabaseNotInitializedException() : base("You have to initialize the database first before using it!")
        {

        }
    }
}

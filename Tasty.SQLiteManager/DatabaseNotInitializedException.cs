using System;

namespace Tasty.SQLiteManager
{
    class DatabaseNotInitializedException : Exception
    {
        public DatabaseNotInitializedException() : base("You have to initialize the database first before using it!")
        {

        }
    }
}

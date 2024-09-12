using System;
using System.Collections.Generic;
using System.Linq;
using Tasty.SQLiteManager.Table;

namespace Tasty.SQLiteManager
{
    internal class Instance
    {
        private readonly Database database;
        private readonly string ident;
        private readonly List<Type> assignedDatabaseEntryTypes = new List<Type>();

        internal Database Database => database;

        internal string Ident => ident;

        internal Instance(Database database)
        {
            this.database = database;
            ident = database.Ident;

            foreach (ITable table in database.Tables)
            {
                assignedDatabaseEntryTypes.Add(table.TableType);
            }
        }

        internal bool IsTypeLinkedToInstance(Type type)
        {
            return assignedDatabaseEntryTypes.Any(x => x.Equals(type));
        }
    }
}

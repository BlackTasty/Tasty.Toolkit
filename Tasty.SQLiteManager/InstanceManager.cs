using System;
using System.Collections.Generic;
using System.Linq;
using Tasty.SQLiteManager.Exceptions;
using Tasty.SQLiteManager.Table;

namespace Tasty.SQLiteManager
{
    internal static class InstanceManager
    {
        private static readonly List<Instance> instances = new List<Instance>();

        internal static Database GetInstance(string ident)
        {
            return GetInstance(ident, false);
        }

        internal static Database GetInstance(string ident, bool allowNull)
        {
            Instance instance = !allowNull ? instances.First(x => x.Ident == ident) :
                instances.FirstOrDefault(x => x.Ident == ident);

            return instance?.Database;
        }

        internal static Database AddInstance(Database database)
        {
            database.ClearCacheTables();
            instances.Add(new Instance(database));

            return database;
        }

        internal static Database GetInstanceContainingType(Type type)
        {
            Database instance = instances.FirstOrDefault(x => x.IsTypeLinkedToInstance(type))?.Database;

            if (instance == null)
            {
                throw new TypeNotDatabaseEntryException(type);
            }

            return instance;
        }

        internal static ITable GetTableFromType(Type type)
        {
            return GetInstanceContainingType(type)[type];
        }
    }
}

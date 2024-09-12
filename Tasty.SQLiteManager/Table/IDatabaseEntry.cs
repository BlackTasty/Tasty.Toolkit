using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.SQLiteManager.Table.Attributes;

namespace Tasty.SQLiteManager.Table
{
    /// <summary>
    /// Interface for <see cref="DatabaseEntry{T}"/> objects.
    /// </summary>
    public interface IDatabaseEntry
    {
        /// <summary>
        /// Returns true if this <see cref="DatabaseEntry{T}"/> is loaded from the database.
        /// </summary>
        [SqliteIgnore]
        bool FromDatabase { get; }

        /// <summary>
        /// Returns the <see cref="TableDefinition{T}"/> for this <see cref="DatabaseEntry{T}"/>.
        /// </summary>
        [SqliteIgnore]
        ITable Table { get; }

        /// <summary>
        /// Returns the ID for this <see cref="DatabaseEntry{T}"/>.
        /// </summary>
        [SqlitePrimaryKey]
        int ID { get; }

        /// <summary>
        /// Saves this <see cref="DatabaseEntry{T}"/> to the database.
        /// </summary>
        /// <returns>Return codes:
        /// <para></para>
        /// 0 = Success<para></para>
        /// -1 = Error executing SQL query<para></para>
        /// -10 = Database is not used</returns>
        int SaveToDatabase(bool isRoot);

        /// <summary>
        /// Deletes this <see cref="DatabaseEntry{T}"/> from the database.
        /// </summary>
        void DeleteFromDatabase();
    }
}

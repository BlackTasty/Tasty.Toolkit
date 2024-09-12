using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Table.Attributes
{
    /// <summary>
    /// Attribute for asssigning tables to a specific <see cref="Database"/> instance, when handling multiple databases with this library.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SqliteUseDatabase : Attribute
    {
        private readonly string ident;

        internal string Ident => ident;

        /// <summary>
        /// Tells the SQLiteManager library which <see cref="Database"/> instance contains this table.
        /// </summary>
        /// <param name="ident">The <c>ident</c> value that was also provided in the <c><see cref="Database"/>.Initialize()</c> function</param>
        public SqliteUseDatabase(string ident)
        {
            this.ident = ident;
        }


        /// <summary>
        /// Tells the SQLiteManager library that this table is part of the default <see cref="Database"/> instance.
        /// </summary>
        public SqliteUseDatabase() { }
    }
}

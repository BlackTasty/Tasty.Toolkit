using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.SQLiteManager.Table.Column;
using Tasty.SQLiteManager.Table.Conditions;

namespace Tasty.SQLiteManager.Table
{
    public interface ITableBase : IList<IColumn>
    {
        /// <summary>
        /// Name of this definition
        /// </summary>
        string Name { get; }

        /// <summary>
        /// A list of all columns in this table
        /// </summary>
        List<IColumn> ColumnDefinitions { get; }

        /// <summary>
        /// Search for a column by name
        /// </summary>
        /// <param name="columnName">The name of the column</param>
        /// <returns></returns>
        IColumn this[string columnName] { get; }

        /// <summary>
        /// Returns if this table exists in the database.
        /// </summary>
        /// <returns>Returns true if the table has been found</returns>
        bool TableExists();

        /// <summary>
        /// Returns if the specified column exists in this table.
        /// </summary>
        /// <param name="target">The column to search for</param>
        /// <returns>Returns true if the column exists</returns>
        bool ColumnExists(IColumn target);

        /// <summary>
        /// Returns if a column with the specified exists in this table.
        /// </summary>
        /// <param name="colName">The column name to search for</param>
        /// <returns>Returns true if the column exists</returns>
        bool ColumnExists(string colName);

        /// <summary>
        /// Run a custom query on this table. Note that there have to be string formatter present in the command
        /// </summary>
        /// <param name="command">The command to execute. {0} = table name</param>
        /// <param name="awaitData">If true the query will execute your command as a SELECT query, else you can create any custom query</param>
        /// <returns>Returns a <see cref="ResultSet"/> if awaitData is true, otherwise null</returns>
        ResultSet Query(string command, bool awaitData);

        /// <summary>
        /// Returns the result of a SELECT query for this table
        /// </summary>
        /// <param name="conditions">The WHERE statement of the query. Leave empty to return all data</param>
        ResultSet Select(params Condition[] conditions);

        /// <summary>
        /// Returns the result of a SELECT query for this table
        /// </summary>
        /// <param name="conditions">The WHERE statement of the query. Leave empty to return all data</param>
        ResultSet Select(IEnumerable<Condition> conditions);

        /// <summary>
        /// Returns the result of a SELECT query for this table
        /// </summary>
        /// <param name="columns">The columns to return</param>
        /// <param name="conditions">The WHERE statement of the query</param>
        /// <returns></returns>
        ResultSet Select(List<IColumn> columns, params Condition[] conditions);

        /// <summary>
        /// Returns the result of a SELECT query for this table
        /// </summary>
        /// <param name="columns">The columns to return. Can be toggled to act as a blacklist with the second parameter</param>
        /// <param name="excludeColumns">If true the columns parameter acts as a blacklist. The result will contain all columns except the blacklisted ones</param>
        /// <param name="conditions">The WHERE statement of the query</param>
        /// <returns></returns>
        ResultSet Select(List<IColumn> columns, bool excludeColumns, params Condition[] conditions);

        /// <summary>
        /// Inserts a new dataset into the table
        /// </summary>
        /// <param name="data">The data to insert into the table</param>
        /// <returns>Returns false if the query failed, otherwise true</returns>
        /// <exception cref="MissingRequiredColumnsException"></exception>
        bool Insert(Dictionary<IColumn, dynamic> data);

        /// <summary>
        /// Inserts a new dataset into the table or replaces an existing entry
        /// </summary>
        /// <param name="data">The data to insert/replace into the table</param>
        /// <returns>Returns false if the query failed, otherwise true</returns>
        /// <exception cref="MissingRequiredColumnsException"></exception>
        bool Replace(Dictionary<IColumn, dynamic> data);

        /// <summary>
        /// Inserts a new dataset into the table and returns the ID of the new dataset
        /// </summary>
        /// <param name="data">The data to insert into the table</param>
        /// <returns>Return values:
        /// <para/>
        /// -1 if the query failed
        /// <para/>
        /// -2 if there are no results
        /// <para/>
        /// -3 if the table does not have an ID column
        /// <para/>
        /// Otherwise the ID of the inserted data is returned
        /// </returns>
        /// <exception cref="MissingRequiredColumnsException"></exception>
        int Insert_GetIndex(Dictionary<IColumn, dynamic> data);

        /// <summary>
        /// Generates a SQL query for inserting multiple data into the table.
        /// </summary>
        /// <param name="data">A <see cref="Dictionary{TKey, TValue}"/> of <see cref="ColumnDefinition{T}"/> objects and values. These get transformed into a SQL query</param>
        /// <returns>Returns the formatted INSERT statement</returns>
        string GenerateBulkInsert(Dictionary<IColumn, dynamic>[] data);

        /// <summary>
        /// Generates a SQL query for updating multiple data into the table.
        /// </summary>
        /// <param name="data">A <see cref="Dictionary{TKey, TValue}"/> of <see cref="ColumnDefinition{T}"/> objects and values. These get transformed into a SQL query</param>
        /// <returns>Returns the formatted UPDATE statement</returns>
        string GenerateBulkUpdate(Dictionary<IColumn, dynamic>[] data);

        /// <summary>
        /// Insert multiple data into the table.
        /// </summary>
        /// <param name="data">A <see cref="Dictionary{TKey, TValue}"/> of <see cref="ColumnDefinition{T}"/> objects and values. These get transformed into a SQL query</param>
        /// <returns>Returns true on success.</returns>
        bool BulkInsert(Dictionary<IColumn, dynamic>[] data);

        /// <summary>
        /// Update multiple data into the table.
        /// </summary>
        /// <param name="data">A <see cref="Dictionary{TKey, TValue}"/> of <see cref="ColumnDefinition{T}"/> objects and values. These get transformed into a SQL query</param>
        /// <returns>Returns true on success.</returns>
        bool BulkUpdate(Dictionary<IColumn, dynamic>[] data);

        /// <summary>
        /// Update a dataset inside this table
        /// </summary>
        /// <param name="data">The columns which shall be updated</param>
        /// <param name="conditions">The WHERE statement of the query</param>
        /// <returns>Returns false if the query failed, otherwise true</returns>
        bool Update(Dictionary<IColumn, dynamic> data, params Condition[] conditions);

        /// <summary>
        /// Remove all entries in this table
        /// </summary>
        /// <returns>Returns false if the query failed, otherwise true</returns>
        bool Delete();

        /// <summary>
        /// Remove a dataset from this table
        /// </summary>
        /// <param name="conditions">The WHERE statement of the query</param>
        /// <returns>Returns false if the query failed, otherwise true</returns>
        bool Delete(params Condition[] conditions);

        /// <summary>
        /// Returns the next available ID, or -3 if no ID column (primary key) exists
        /// </summary>
        /// <returns>Next available ID or -3</returns>
        int GetNextId();
    }
}

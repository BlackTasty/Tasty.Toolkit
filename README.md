# Tasty.Toolkit

## Tasty.Logging
Contains classes for logging to a file. 

Hooking up a control to the "IConsole" interface allows to catch the logged strings and output them for example into a textbox.

## Tasty.Logging.Colorful
*Not implemented yet!*

Extension for Tasty.Logging which allows the Windows cmd to display text in every possible color. (Uses Colorful.Console)

## Tasty.Logging.Sentry
Extension for Tasty.Logging. Can be used with [Sentry.io](https://sentry.io/welcome/) to send error reports to developer. 

## Tasty.SQLiteManager
Custom SQLite API which handles communication between database and application. Contains functions to allow ALTER TABLE as SQLite doesn't support it.

Current features available:
- CREATE/ALTER/DROP TABLE: SQLiteManager does this automatically for every table
- Import/Export SQL: via their respective methods in Database class
- INSERT/UPDATE/DELETE: via their respective methods in TableDescriptor class
- SELECT: via Select() method in TableDescriptor, returns a ResultSet object. Can also accept a Condition object to filter results

### How to set up your database
The best way to initialize you database is by setting up a separate static class containing your database structure.

In this example we'll call our class "DatabaseStructure"
```csharp
class DatabaseStructure
{
  public readonly TableDescriptor users;
  // We need a list of all tables to initialize the database later on 
  public readonly List<TableDescriptor> tables;

  public DatabaseStructure() 
  {
    List<IColumn> columns_users = new List<IColumn>()
    {
      // The object type inside <> tells the table which type to use
      // First variable is the column name, the second variable table can be used to set column modes
      new ColumnsDescriptor<int>("ID", ColumnMode.PRIMARY_KEY),
      new ColumnsDescriptor<string>("name", ColumnMode.NOT_NULL),
      new ColumnsDescriptor<string>("information"),
      new ColumnsDescriptor<string>("email")
    };
    
    // First variable sets the table name, second variable is a list of columns
    users = new TableDescriptor("users", columns_users);
    
    // Add all your tables to the list
    tables = new List<TableDescriptor>()
    {
      users
    };
  }
}
```

Now we just need to initialize the database:
```csharp
DatabaseStructure structure = new DatabaseStructure();

Database.Initialize("users.db", structure.tables);
```

This will create all defined tables and update itself depending if you add or remove columns or tables.

### Creating foreign keys
If you need to create foreign keys (in case that you need to map two tables together for example) use the following syntax:
```csharp
// Initialize your tables
List<IColumn> columns_users = new List<IColumn>() { ... };
TableDescriptor users = new TableDescriptor( ... );
List<IColumn> columns_roles = new List<IColumn>() { ... };
TableDescriptor roles = new TableDescriptor( ... );

List<IColumn> columns_mapping_users_roles = new List<IColumn>()
{
  // mapping column to users id table
  new ColumnDescriptor<int>("U_ID", ColumnMode.NOT_NULL),
  // mapping column to roles id table
  new ColumnDescriptor<string>("R_ID", ColumnMode.NOT_NULL)
};
            
List<ForeignKeyDescriptor> foreignKeys_users_roles = new List<ForeignKeyDescriptor>()
{
  // Define foreign key. Variables in order:
  // 1: Foreign key name
  // 2: Target table
  // 3: Target foreign key column in our mapping table
  // 4: Target key in our target table
  new ForeignKeyDescriptor("fk_U_ID", users, columns_mapping_users_roles[0], columns_users[0]),
  new ForeignKeyDescriptor("fk_R_ID", roles, columns_mapping_users_roles[1], columns_roles[0])
};

TableDescriptor mapping_users_roles = new TableDescriptor("mapping_users_roles", columns_mapping_artists_songs, foreignKeys_artists_songs);
```

### Adding conditions to SELECT statements

We're using our DatabaseStructure class from before in this example:
```csharp
ResultSet result = structure.songs.Select(new Condition(new KeyValuePair<IColumn, dynamic>(structure.users["name"], "John Doe")));
```

### Limiting the columns returned by SELECT

By adding a list of columns you can select which columns shall be returned in your result set
```csharp
List<IColumn> columns = new List<IColumn>()
{
  structure.songs["email"],
  structure.songs["information"]
}

ResultSet result = structure.songs.Select(columns, new Condition(new KeyValuePair<IColumn, dynamic>(structure.users["name"], "John Doe")));
```

Or you can tell the database manager to **exclude** the tables you provide:
```csharp
List<IColumn> columns = new List<IColumn>()
{
  structure.songs["id"],
  structure.songs["name"]
}

ResultSet result = structure.songs.Select(columns, true, new Condition(new KeyValuePair<IColumn, dynamic>(structure.users["name"], "John Doe")));
```


## Tasty.Tests.SQLiteManager
Contains tests for SQLiteManager

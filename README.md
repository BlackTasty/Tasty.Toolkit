# Tasty.Toolkit

## Tasty.Logging
Contains classes for logging to a file. 

Hooking up a control with the "IConsole" interface allows to catch the logged strings and output them for example into a textbox..

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

## Tasty.Tests.SQLiteManager
Contains tests for SQLiteManager

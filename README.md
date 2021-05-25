# Tasty.Toolkit

This project contains libraries for different purposes. Below are short descriptions to each library. (Check out the Wiki for more information to each library)

## Tasty.Logging
NuGet: `Install-Package TastyApps.Core.Logging -Version 1.0.2`

Contains classes for logging to a file. 
Hooking up a control to the "IConsole" interface allows to catch the logged strings and output them for example into a textbox.

See the [Wiki](https://github.com/BlackTasty/Tasty.Toolkit/wiki/Tasty.Logging) for more details.

## Tasty.Logging.Colorful
*Not implemented yet!*

Extension for Tasty.Logging which allows the Windows cmd to display text in every possible color. (Uses Colorful.Console)

## Tasty.Logging.Sentry (deprecated)
Extension for Tasty.Logging. Can be used with [Sentry.io](https://sentry.io/welcome/) to send error reports to developer. 

## Tasty.SQLiteManager
Custom SQLite API which handles communication between database and application. Contains functions to allow ALTER TABLE as SQLite doesn't support it.

Current features:
- [**CREATE/ALTER/DROP TABLE**](): SQLiteManager does this automatically for every table
- [**Import/Export SQL**](): via their respective methods in Database class
- [**INSERT/UPDATE/DELETE**](): via their respective methods in TableDescriptor class
- [**SELECT**](): via Select() method in TableDescriptor, returns a ResultSet object. Can also accept a Condition object to filter results

## Tasty.ViewModel
Provides classes for WPF data binding (MVVM). Additionally you can observe objects and collections of type **VeryObservableCollection** to detect unsaved changes. (For example when providing a form to edit data)

Current features:
- [**ViewModelBase**](): A base class for ViewModels.
- [**VeryObservableCollection**](): Extension for ObservableCollection<T> objects with optional Observer support
- [**VeryObservableStackCollection**](): Extension of VeryObservableCollection<T>. Additional field "limit" allows limiting of items.
- [**Observer**]() & [**ObserverManager**](): Classes for context-based change detection. Works on primitive types, custom classes and VeryObservableCollection<T>.
  

## Tasty.ViewModel.JsonNet
Overrides some of the classes from Tasty.ViewModel to add the **JSONIgnore** flag to properties. Requires Newtonsoft.Json dependency!

### Unit tests

## Tasty.Tests.Base
Contains simple base classes and methods for testing

## Tasty.Tests.SQLiteManager
Contains tests for Tasty.SQLiteManager

## Tasty.Tests.ViewModel
Contains tests for Tasty.ViewModel

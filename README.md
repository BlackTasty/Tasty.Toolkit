# Tasty.Toolkit

This project contains libraries for different purposes. Below are short descriptions to each library. (Check out the Wiki for more information to each library)

## Tasty.Logging
#### NuGet: `Install-Package TastyApps.Core.Logging -Version 1.0.4`
#### GitHub packages: `dotnet add PROJECT package TastyApps.Core.Logging --version 1.0.4`

Contains classes for logging to a file. 
Hooking up a control to the "IConsole" interface allows to catch the logged strings and output them for example into a textbox.

### See the [Wiki](https://github.com/BlackTasty/Tasty.Toolkit/wiki/Tasty.Logging) for more details.

## Tasty.SQLiteManager
#### NuGet: `Install-Package TastyApps.Core.SQLiteManager -Version 1.0.3`
#### GitHub packages: `dotnet add PROJECT package TastyApps.Core.SQLiteManager --version 1.0.3`

Custom SQLite API which handles communication between database and application. Contains functions to allow ALTER TABLE as SQLite doesn't support it.

Current features:
- [**CREATE/ALTER/DROP TABLE**](): SQLiteManager does this automatically for every table
- [**Import/Export SQL**](): via their respective methods in Database class
- [**INSERT/UPDATE/DELETE**](): via their respective methods in TableDescriptor class
- [**SELECT**](): via Select() method in TableDescriptor, returns a ResultSet object. Can also accept a Condition object to filter results

### See the [Wiki](https://github.com/BlackTasty/Tasty.Toolkit/wiki/Tasty.SQLiteManager) for more details.

## Tasty.ViewModel
#### NuGet: `Install-Package TastyApps.Core.ViewModel -Version 1.0.5.1`
#### GitHub packages: `dotnet add PROJECT package TastyApps.Core.ViewModel --version 1.0.5.1`

Provides classes for WPF data binding (MVVM). Additionally you can observe objects and collections of type **VeryObservableCollection** to detect unsaved changes. (For example when providing a form to edit data)

Current features:
- [**ViewModelBase**](): A base class for ViewModels.
- [**VeryObservableCollection**](): Extension for ObservableCollection<T> objects with optional Observer support
- [**VeryObservableStackCollection**](): Extension of VeryObservableCollection<T>. Additional field "limit" allows limiting of items.
- [**Observer**]() & [**ObserverManager**](): Classes for context-based change detection. Works on primitive types, custom classes and VeryObservableCollection<T>.

### See the [Wiki](https://github.com/BlackTasty/Tasty.Toolkit/wiki/Tasty.ViewModel) for more details.

## Tasty.ViewModel.JsonNet
#### NuGet: `Install-Package TastyApps.Core.ViewModel.JsonNet -Version 1.0.4`
#### GitHub packages: `dotnet add PROJECT package TastyApps.Core.ViewModel.JsonNet --version 1.0.4`
  
Overrides some of the classes from Tasty.ViewModel to add the **JSONIgnore** flag to properties. Requires Newtonsoft.Json dependency!

### See the [Wiki](https://github.com/BlackTasty/Tasty.Toolkit/wiki/ViewModel---Collections-&-Newtonsoft.Json) for more details.
  
## Tasty.Samples
Contains documented examples for the libraries above.
  
***

## Unit tests

### Tasty.Tests.Base
Contains simple base classes and methods for testing

### Tasty.Tests.SQLiteManager
Contains tests for Tasty.SQLiteManager

### Tasty.Tests.ViewModel
Contains tests for Tasty.ViewModel
  
## Other

### Tasty.Logging.Colorful
*Not implemented yet!*

Extension for Tasty.Logging which allows the Windows cmd to display text in every possible color. (Uses Colorful.Console)

### Tasty.Logging.Sentry (deprecated)
Extension for Tasty.Logging. Can be used with [Sentry.io](https://sentry.io/welcome/) to send error reports to developer. 

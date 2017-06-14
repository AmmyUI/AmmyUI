## [1.2.72] - 14.06.2017

* support for local types written in C# 6

## [1.2.71] - 04.05.2017

* support variable/parameter as resource key:
  Background: resource $backgroundKey

* improved performance by removing unnecessary compilations

* fixed Static binding source bug i.e.
  bind "Something" from Type.StaticProperty

## [1.2.53] - 01.05.2017

* support variable or parameter as node Key:
  Grid Key=$var {
    ...
  }

* x:Arguments support:
  MyControl {
    arguments [
      MyArgument {}
      MyArgument {}
    ]
  }

* Support `const` members in property assignment:
  TextBlock {
    Text: Strings.HelloString // HelloString is const
  }
  
* small bugfixes

## [1.2.41] - 14.04.2017

### Fixed
* adb.exe location for Google Android Emulator

### Added
* "Xamarin Forms" setting tab

## [1.2.38] - 06.04.2017

### Added
* Visual Studio Error List support

### Fixed
* resolving in ancestor type binding:  `bind ... from $ancestor<Type>`
* error not disappearing bug
* error indicator problem

## [1.2.35] - 04.04.2017

### Improved
* Intellisense support 
  - Ordering (Properties -> Nodes -> Other)
  - Item is always selected if it starts with current string
  - Better Levenstein distance usage

### Fixed
* Alias Namespace resolving bug
* Array Namespace resolving bug
* EOF syntax issue

## [1.2.31] - 27.03.2017

### Added
* Use reference values as mixin/alias argument:
  #Setter("Background", Red)

## [1.2.30] - 19.03.2017

### Fixed
* NO_AMMY_UPDATE bug
* Any control defined in ammy didn't work upon second instantiation

## [1.2.29] - 16.03.2017

### Added
* UWP support (Make sure to update Visual Studio extension!)

## [1.2.22] - 13.03.2017

### NB! This update requires Visual Studio extension update to work

### Added
* IDE: Color value adornments
* IDE: Closing bracket adornments
* IDE: Outlining (Collapsible regions)
* IDE: Preferences window
* IDE: Brace on same/next line option
* IDE: Auto document formatting (Preferences window)
* Optional parenthesis for aliases when omitting arguments

### Fixed
* Local property hides type (issue #48)

## [1.2.20] - 06.03.2017

### Fixed
* Added missing System.Collections.Immutable dependency

## [1.2.18] - 03.03.2017

### Added
* XAML to Ammy project converter
* AST caching for better performance
* Do not reload component if it has just been registered

### Fixed
* Preserve attribute/node ordering
* ResourceDictionary generated file rewrite bug
* large string performance issues
* property resolution for cases like: `Property: ExternalType.Property`
* ambiguity between C# parsed types and Ammy parsed types
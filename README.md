# Ammy - UI Language for XAML platforms

Ammy is a modern UI language that either replaces or compliments XAML in your projects. 

[Project site](http://www.ammyui.com)

[Documentation](http://www.ammyui.com/documentation/)

[Gitter chat](https://gitter.im/AmmyUI/Ammy)

### Main features of Ammy 

* Very simple JSON-like syntax
* Mixins and aliases to fight copy-pasting and to keep code [DRY](https://en.wikipedia.org/wiki/Don%27t_repeat_yourself)
* Inline Binding Converters to avoid implementing `IValueConverter`s for every simple task
* Runtime Update that actually works (VS2017 has very limited update functionality at the moment)

### How Ammy code looks

```
Window "MyApp.MainWindow" {
  Title: "My first Application"

  Grid {
    TextBlock { "Hello, World!" }
  }
}
```


Note that you don't need to import any namespaces manually. In case you need to import external namespace, there is C# a `using` keyword available.


### Slightly more complicated example

```
// Define alias for form field (label + textbox)
// Alias takes parameters and returns an element
alias FormField (labelText, binding)
{
  StackPanel {
    Orientation: Horizontal
    
    TextBlock { Text: $labelText }
    TextBlock {
      Text: $binding
    }
  }
}

Window "MyApp.MainWindow" {
  Title: "My First App"
  
  StackPanel { 
    @FormField ("First name", bind FirstName)
    @FormField ("Last name", bind LastName)
    
    TextBlock {
      Text: bind 
            convert (MyViewModel vm) => "Hello, " + vm.FirstName + " " + vm.LastName
      
      // Empty binding path binds whole DataContext
      // `convert` defines binding converter inline
    }
  }
}
```

## Building and debugging

* Install Visual Studio 2017 if you don't have one
* Install [Nemerle](http://nemerle.org) (Click "Download Now" button)
* Clone repository
* Open `Ammy.sln` solution in VS2017 and build it
* Set `Ammy.VisualStudio` project as StartUp Project and start Debugging session
* Open `Ammy.Tests.sln` solution 
* `Ammy.Test.Workbench` project is for debugging
* `Ammy.Test.Wpf` contains permanent tests that should all compile

## Project structure

### Syntax and AST

Ammy uses [Nitra](https://github.com/rsdn/nitra) for parsing and typing. First, file is parsed with syntax defined in Syntax.nitra. Resulting `ParseTree` is then mapped to AST (Mapping.nitra, MappingExpr.nitra, MappingFunctions.nitra). 

Semantic analysis is a process where types loaded from Backend are binded to AST. This process defined inside Ast and AstBase projects in `.nitra` files.

* Ammy.Backend (Loads referenced assemblies and creates Nitra symbols)
* Ammy.AstBase (Common AST types)
* Ammy.Ast (More AST types)
* Ammy.Syntax (Syntax and Mapping to AST)

### Sidekick 

Sidekick library has two primary functions. 1) `ExpressionConverter` used for inline binding converters 2) Runtime update logic

* Ammy.Sidekick.XamarinForms
* Ammy.Sidekick.Uwp
* Ammy.Sidekick.Common

### Compilation and Tasks

Build assembly is a glue between IDE/MSBuild and Ammy language. 

* Ammy.Build
* Ammy.BamlCompilerWPF

### IDE

* Ammy.VisualStudio
* Ammy.VisualStudio.Service
* Ammy.VisualStudio.ItemTemplates

Ammy.VisualStudio only contains service providers. These providers use RuntimeLoader to load Ammy.VisualStudio.Service assembly and load actual services. Ammy.VisualStudio.Service contains all the logic for highlighting, intelli-sense, regions, adornments, classisifiers and other stuff. 







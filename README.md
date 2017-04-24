## Ammy - UI Language for XAML platforms

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
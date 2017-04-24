## Nodes

Nodes are defined like this

    Grid {
    }

They can have children

    StackPanel {
        TextBlock { "Hello, World!" }
    }

Or they can be empty

    Grid {}

Node can have a name (which is compiled as x:Name)

    Grid "MainGrid" {}

And a key (compiled as x:Key"

    Grid Key="GridKey" {}

Root node requires a name

    Window "MyApp.MainWindow" {
    }

Name is a fully qualified name of a class generated for this control.

Previous sample compiles to something like this

    <Window x:Class="MyApp.MainWindow" 
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    </Window>

## <a id="properties"></a>Properties

Properties can be defined on a same line

    Width: 100, Height: 50, HorizontalAlignment: Right

Or separate lines

    Width: 100,
    Height: 100 // <-- Note that comma is optional
    HorizontalAlignment: Right

## Property values

Any value that is correct in XAML is also correct with Ammy

    Width: "100" 
    Width: "{Binding Width}"
    Width: "{StaticResource WidthValue}"

While using string values makes it easier to convert from XAML code, it doesn't provide you with intellisense or special syntax coloring. It also requires more characters than Ammy's builtin syntax.

Same properties could be rewritten as

    Width: 100
    Width: bind Width
    Width: resource WidthValue

#### <a id="strings"></a>Strings

String values use double quotes

    TextBlock { Text: "Hello, World!" }

String value can be used as a content for **Node**

    TextBlock { "Hello, World!" }

And here is the XAML that is generated

    <TextBlock>Hello, World!</TextBlock>

#### <a id="integers"></a>Integers

    Width: 10
    Width: 10.5
    Width: "10"
    Width: "10.5"

### <a id="boolean"></a>Boolean, x:Null

    IsItemsHost: true
    IsItemsHost: false
    Text: null

#### <a id="enum"></a>Enum values

    HorizontalAlignment: Center
    HorizontalAlignment: "Center"

#### <a id="xtype"></a>x:Type

Referencing a type is a simple

    TargetType: TextBlock

This will generate your usual XAML

    TargetType="{x:Type TextBlock}"

Note that you can still use XAML markup extensions with Ammy

    TargetType: {x:Type TextBlock}

#### <a id="eventhandlers"></a>Event handlers

    MouseDown: OnMouseDown
    MouseDown: "OnMouseDown"

#### <a id="nodevalues"></a>Node values

You can assign **Node** as a value

    ItemTemplate: DataTemplate {
        ...
    }

This would be similar to this XAML

    <Node.ItemTemplate>
        <DataTemplate></DataTemplate>
    </Node.ItemTemplate>

#### <a id="arrayvalues"></a>Array values

Arrays use square brackets as in JSON and lots of other languages

    RowDefinitions: [
        RowDefinition { Height: 30 }
        RowDefinition { }
        RowDefinition { Height: "*" }
    ]

#### <a id="parameters"></a>Parameters

You can reference a variable

    FontSize: $normalFontSize

 or mixin parameter

    mixin MyTextBlock (text) {
        TextBlock {
            Text: $text
        }
    }

#### <a id="resources"></a>Resources

To reference a static resources use `resource` keyword

    FontFamily: resource AppFontFamily

`dyn` suffix makes it a `{DymamicResource ...}`

    Background: resource dyn ButtonBackground

#### <a id="binding"></a>Binding

By default binding has DataContext as a source, so you can just write

    Text: bind Text

You can specify binding source with `from` keyword, like this:
    
    Text: bind Text from $viewmodel
    
There are many sources available:

| Ammy | Xaml |
| ---- | ---- |
| `$viewmodel` | default behaviour |
| `$this`      | `RelativeSource={RelativeSource Self}` |
| `$template`  | `RelativeSource={RelativeSource TemplatedParent}` |
| `$ancestor<TextBlock>(3)` where ancestor level `(3)` is optional | `RelativeSource={RelativeSource FindAncestor, AncestorType={x:Type TextBlock}, AncestorLevel=3}`
| `$previous` | `RelativeSource={RelativeSource PreviousData}` |
| `"myTextBlock"` | `Elementid=myTextBlock` |
| `SomeType.StaticProperty` | `Source={x:Static ns:SomeType.StaticProperty}` |
| `$resource SomeResource` | `Source={StaticResource SomeResource}` |

If you need to manually specify binding properties use `set` keyword

    Text: bind Text 
          set [
              BindingGroupName: "BindingGroup1",
              BindsDirectlyToSource: true,
              ConverterCulture: "en-us",
              ConverterParameter: 5,
              FallbackValue: 50,
              IsAsync: false,
              Mode: OneWay,
              NotifyOnSourceUpdated: true,
              NotifyOnTargetUpdated: false,
              NotifyOnValidationError: true,
              StringFormat: "",
              TargetNullValue: 0,
              UpdateSourceExceptionFilter: TestViewModel.ExceptionFilter,
              UpdateSourceTrigger: PropertyChanged,
              ValidatesOnDataErrors: true, 
              ValidatesOnExceptions: false,
              XPath: ""
         ]

Othwerwise there is still good old XAML markup extension

    Text: "{Binding Text}"

## <a id="variables"></a>Variables

Ammy allows you to define Variables

    $fontColor = "#363636"

and use them

    TextBlock {
        Foreground: $fontColor
    }

Variables can be defined globally

    // MyControl1.ammy
    $normalFontSize = "14"

    UserControl "MyControl1" {
    }

    // MyControl2.ammy
    UserControl "MyControl2" {
        TextBlock { FontSize: $normalFontSize }
    }

Or locally

    // MyControl1.ammy
    UserControl "MyControl1" {
        $normalFontSize = "14"
    }

    // MyControl2.ammy
    UserControl "MyControl2" {
        TextBlock { FontSize: $normalFontSize } // Error!
    }

Variable only allows string as a value

    $myColor = Red // Compilation error!
    $myColor = "Red" // OK!

## Mixins

You can think of mixins as functions that take zero or more arguments and return either properties or a node.

In fact, there are two types of mixins:

**Content mixin**

    mixin Centered() : TextBlock {
        HorizontalAlignment: Center
        VerticalAlignment: Center
    }

Which requires target type to be specified. `TextBlock` in this case.

And **type mixin**

    mixin HorizontalStackPanel() {
        StackPanel {
            Orientation: Horizontal
        }
    }

Using **content mixin** is simple

    TextBlock {
        @Centered()
    }

Using **Type mixin** is also simple, but slightly different

    Grid {
        @HorizontalStackPanel() {
            // You can add nodes and properties here
        }
    }

Note, that **content mixins** can also return nodes, like so

    mixin MyMixin() : StackPanel {
        Width: 100
        Height: 100

        TextBlock { "First element" }
    }
    ...
    StackPanel {
        @MyMixin()
        TextBlock { "Second element" }
    }

Mixins can define parameters 

    mixin Setter(property, value) {
        Setter {
            Property: $property
            Value: $value
        }
    }

Parameters can have default values

    mixin Cell (column = 0, row = 0, columnSpan = 1, rowSpan = 1) : FrameworkElement {
        Grid.Row: $row
        Grid.Column: $column
        Grid.RowSpan: $rowSpan
        Grid.ColumnSpan: $columnSpan
    }

You can use named parameter syntax when invoking a mixin

    Button {
        @Cell(row: 3)
    }

This would generate

    <Button Grid.Row="3" Grid.Column="0" Grid.RowSpan="1" Grid.ColumnSpan="1" />

You can also use `none` value if you want specific property assignment to be removed 

    Button {
        @Cell(1, none, none, none)
    }

This will result in

    <Button Grid.Column="1" />



`none` can also act as a default value, so `Cell` should be rewritten as

    mixin Cell (columnn = none, row = none, columnSpan = none, rowSpan = none) : FrameworkElement {
        Grid.Row: $row
        Grid.Column: $column
        Grid.RowSpan: $rowSpan
        Grid.ColumnSpan: $columnSpan
    }

This way, calling

    Button {
        @Cell(1)
    }

Would also only generate `Grid.Column` assignment
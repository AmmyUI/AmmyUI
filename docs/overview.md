# About Ammy

Ammy is a UI language that compiles to XAML. Any XAML construct can be directly translated to Ammy syntax, but not vise versa, since Ammy has unique features specific to it. The goal is to create a concise, readable syntax and get rid of unnecessary clutter.

## Basics

    Window "MainWindow" {
        Width: 200
        Height: 100
        TextBlock { 
            Text: "Hello, World!"
        }
    }

As you can see, Ammy syntax resembles JSON, but takes shortcuts to make it more human readable. 

One important distinction is the fact that comma is optional if you define properties on different lines.

    Width: 100 // <-- No comma
    Height: 100, // <-- Comma 
    Background: Red 

If your properties are defined on the same line though, comma is mandatory.

    Width: 100, Height: 200, Background: Red // Error will be given, if comma is omitted

Other distinction is support for C-style comments. Both `//` and `/* */` work as intended.

## Mixins

Ammy allows you to create reusable components called mixins. Once defined they can be used anyplace in the same scope.

    // Definition
    mixin RedTextBlock (text) {
        TextBlock {
            Background: Red
            Text: $text
        }
    }

    // Usage
    Grid {
        @RedTextBlock ("Hello, World!")
    }

In this case we have defined a mixin that inserts a TextBlock with red background. Mixins can define zero or more parameters that can be used as attribute values.

More on mixins here: Syntax -> Mixins

## Realtime update

If you are using Visual Studio as an IDE for Ammy development you can update UI on live application. 

1. Start debugging
2. Change something in `ammy` file 
3. Hit `Control + S`
4. UI is updated without application restart
5. Viewmodel state and everything else stays the same

So, for example, you have an application that displays a twitter feed. You have defined a Tweet control as following:

    UserControl "Tweet" {
        TextBlock { Text: bind tweet_text }
    }

You run this application and see that avatar is missing. So naturally, you 
decide to fix it:

    UserControl "Tweet" {
        Grid {
            Image { Source: bind tweet_avatar }
            TextBlock { 
                Text: bind tweet_text, 
                Margin: "40, 0, 0, 0"
            }
        }
    }

Now you just save this file and watch as all loaded tweets are updated to a new look.

There are some restrictions though. For example you can't add new `ammy` file and expect it be used without restarting your application. You also can't change root node type. But using `UserControl` as a root object basically negates this problem.

## IDE support

We have intellisense, realtime error checking, syntax coloring and a lot more!

## Try it!

Write `install-package Ammy` in your Visual Studio package manager and hit `Enter`.

After that install Visual Studio extension called `Ammy` from extension gallery. Without this extension you won't have syntax coloring and realtime update support. Extension also adds new item templates to `Add -> New Item...` dialog.

Now go to documentation and discover more great features!
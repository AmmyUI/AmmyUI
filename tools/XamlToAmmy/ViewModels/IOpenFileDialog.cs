using System;

namespace XamlToAmmy.ViewModels
{
    internal interface IOpenFileDialog
    {
        IObservable<string> BrowseFile();
    }
}
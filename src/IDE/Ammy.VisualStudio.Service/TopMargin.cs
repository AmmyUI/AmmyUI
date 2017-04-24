using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.Text.Editor;

namespace Ammy.VisualStudio.Service
{
    public class TopMargin : Grid, IWpfTextViewMargin
    {
        // ReSharper disable once UnusedParameter.Local
        public TopMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer, IServiceProvider serviceProvider)
        {
        }

        public void Dispose()
        {
        }

        public ITextViewMargin GetTextViewMargin(string marginName)
        {
            return marginName == "AmmyTopMargin" ? this : null;
        }

        public double MarginSize => ActualHeight;
        public bool Enabled => true;
        public FrameworkElement VisualElement => this;
    }
}
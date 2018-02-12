using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ammy.VisualStudio.Service.Settings;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.Win32;

namespace Ammy.VisualStudio.Service.Views
{
    /// <summary>
    /// Interaction logic for Preferences.xaml
    /// </summary>
    public partial class Preferences : DialogWindow
    {
        public Preferences(Action documentFormatter)
        {
            InitializeComponent();

            if (AmmySettings.OpeningBraceOnSameLine)
                BraceOnSameLine.IsChecked = true;
            else
                BraceOnNewLine.IsChecked = true;

            ShowEndTagAdornments.IsChecked = AmmySettings.ShowEndTagAdornments;
            SuppressAdbWarning.IsChecked = AmmySettings.SuppressAdbWarning;
            TransformOnSave.IsChecked = AmmySettings.TransformOnSave;
            AdbPath.Text = AmmySettings.AdbPath;

            FormatDocument.Click += (sender, args) => {
                AmmySettings.OpeningBraceOnSameLine = BraceOnSameLine.IsChecked ?? false;
                AmmySettings.ShowEndTagAdornments = ShowEndTagAdornments.IsChecked ?? false;
                documentFormatter();
                Close();
            };

            Cancel.Click += (sender, args) => {
                Close();
            };

            Save.Click += (sender, args) => {
                AmmySettings.OpeningBraceOnSameLine = BraceOnSameLine.IsChecked ?? false;
                AmmySettings.ShowEndTagAdornments = ShowEndTagAdornments.IsChecked ?? false;
                AmmySettings.TransformOnSave = TransformOnSave.IsChecked ?? false;
                AmmySettings.SuppressAdbWarning = SuppressAdbWarning.IsChecked ?? false;
                AmmySettings.AdbPath = AdbPath.Text;

                Close();
            };
        }

        private void BrowseButtonClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog() {
                Filter = "adb|adb.exe"
            };

            if (dialog.ShowDialog() == true)
                AdbPath.Text = dialog.FileName;
        }
    }
}

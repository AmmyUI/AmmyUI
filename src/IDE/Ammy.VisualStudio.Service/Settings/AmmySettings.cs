using System;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;

namespace Ammy.VisualStudio.Service.Settings
{
    public class AmmySettings
    {
        public static event EventHandler<EventArgs> SettingsChanged;

        static readonly SettingsManager SettingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);

        private const string CollectionName = "AmmySettings";
        private static bool _openingBraceOnSameLine;
        private static bool _showEndTagAdornments;
        private static bool _suppressAdbWarning;
        private static string _adbPath;

        static AmmySettings()
        {
            LoadSettings();
        }

        public static bool OpeningBraceOnSameLine
        {
            get { return _openingBraceOnSameLine; }
            set {
                _openingBraceOnSameLine = value;
                SaveBooleanSetting(nameof(OpeningBraceOnSameLine), value);
            }
        }

        public static bool ShowEndTagAdornments
        {
            get { return _showEndTagAdornments; }
            set {
                _showEndTagAdornments = value;
                SaveBooleanSetting(nameof(ShowEndTagAdornments), value);
            }
        }

        public static bool SuppressAdbWarning
        {
            get { return _suppressAdbWarning; }
            set {
                _suppressAdbWarning = value;
                SaveBooleanSetting(nameof(SuppressAdbWarning), value);
            }
        }

        public static string AdbPath
        {
            get { return _adbPath; }
            set {
                _adbPath = value;
                SaveStringSetting(nameof(AdbPath), value);
            }
        }

        public static bool TransformOnSave { get; internal set; }

        private static void LoadSettings()
        {
            var store = SettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            store.CreateCollection(CollectionName);

            OpeningBraceOnSameLine = GetBooleanProperty(store, nameof(OpeningBraceOnSameLine), true);
            ShowEndTagAdornments = GetBooleanProperty(store, nameof(ShowEndTagAdornments), true);
            SuppressAdbWarning = GetBooleanProperty(store, nameof(SuppressAdbWarning), false);
            AdbPath = GetStringProperty(store, nameof(AdbPath), string.Empty);
        }

        private static void SaveBooleanSetting(string propertyName, bool value)
        {
            GetStore().SetBoolean(CollectionName, propertyName, value);
            InvokeSettingsChanged();
        }

        private static void SaveStringSetting(string propertyName, string value)
        {
            GetStore().SetString(CollectionName, propertyName, value);
            InvokeSettingsChanged();
        }

        private static WritableSettingsStore GetStore()
        {
            return SettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
        }

        private static void InvokeSettingsChanged()
        {
            var settingsChanged = SettingsChanged;
            settingsChanged?.Invoke(null, new EventArgs());
        }

        private static bool GetBooleanProperty(WritableSettingsStore store, string propertyName, bool defaultValue)
        {
            try {
                if (!store.PropertyExists(CollectionName, propertyName))
                    store.SetBoolean(CollectionName, propertyName, defaultValue);

                return store.GetBoolean(CollectionName, propertyName);
            } catch {
                store.SetBoolean(CollectionName, propertyName, defaultValue);
                return defaultValue;
            }
        }

        private static string GetStringProperty(WritableSettingsStore store, string propertyName, string defaultValue)
        {
            try { 
                if (!store.PropertyExists(CollectionName, propertyName))
                    store.SetString(CollectionName, propertyName, defaultValue);

                return store.GetString(CollectionName, propertyName);
            } catch {
                store.SetString(CollectionName, propertyName, defaultValue);
                return defaultValue;
            }
        }
    }
}
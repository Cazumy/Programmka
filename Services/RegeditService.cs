using Microsoft.Win32;
using System;
using System.Windows;

namespace Programmka.Services
{
    public static class RegeditService
    {
        /// <summary>
        /// RegistryHive.CurrentUser or RegistryHive.LocalMachine
        /// </summary>
        public static bool ContainsReg(RegistryHive registryHive, string subkey, string key)
        {
            var baseKey = registryHive switch
            {
                RegistryHive.CurrentUser => Registry.CurrentUser,
                RegistryHive.LocalMachine => Registry.LocalMachine,
                _ => null
            };

            using var hkey = baseKey?.OpenSubKey(subkey);
            return hkey?.GetValue(key) != null;
        }

        /// <summary>
        /// RegistryHive.CurrentUser or RegistryHive.LocalMachine
        /// </summary>
        ///
        public static bool ContainsRegValue<T>(RegistryHive hive, string subkey, string key, T? input = default)
        {
            RegistryKey? baseKey = hive switch
            {
                RegistryHive.CurrentUser => Registry.CurrentUser,
                RegistryHive.LocalMachine => Registry.LocalMachine,
                _ => null
            };

            using var target = baseKey?.OpenSubKey(subkey);
            var value = target?.GetValue(key);

            return value is T t && t.Equals(input);
        }
        public static T? GetRegValue<T>(RegistryHive hive, string subkey, string key, T? fallback = default)
        {
            RegistryKey? baseKey = hive switch
            {
                RegistryHive.CurrentUser => Registry.CurrentUser,
                RegistryHive.LocalMachine => Registry.LocalMachine,
                _ => null
            };

            if (baseKey == null) return fallback;

            using var target = baseKey.OpenSubKey(subkey);
            if (target == null) return fallback;

            try
            {
                var value = target.GetValue(key);
                if (value is T tValue)
                {
                    return tValue;
                }

                if (value != null && !typeof(T).IsAssignableFrom(value.GetType()))
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
            }
            catch{}

            return fallback;
        }

        public static void CreateReg<T>(RegistryHive registryHive, string subkey, string key, string dir = "", T? value = default)
        {
            RegistryKey? baseKey = registryHive switch
            {
                RegistryHive.CurrentUser => Registry.CurrentUser,
                RegistryHive.LocalMachine => Registry.LocalMachine,
                _ => null
            };
            if (baseKey == null) return;

            using var target = baseKey.CreateSubKey(subkey);
            if (target == null) return;

            var registryKey = string.IsNullOrEmpty(dir) ? target : target.CreateSubKey(dir, true);

            if (value == null)
                return;

            if (value is int intValue)
            {
                registryKey.SetValue(key, intValue, RegistryValueKind.DWord);
            }
            else if (value is string strValue)
            {
                registryKey.SetValue(key, strValue, RegistryValueKind.String);
            }
            ///
            else
            {
                throw new System.Exception();
            }
        }

        public static void CreateRegDir(string subkey)
        {
            Registry.LocalMachine.CreateSubKey(subkey);
        }

        public static void DeleteReg(RegistryHive registryHive, string subkey, string key)
        {
            try
            {
                var baseKey = registryHive == RegistryHive.CurrentUser ? Registry.CurrentUser : Registry.LocalMachine;
                using var hKey = baseKey.OpenSubKey(subkey, true);
                hKey?.DeleteValue(key);
            }
            catch { MessageBox.Show("Something went wrong..."); }
        }
        public static void DeleteRegDir(string subkey, string dir)
        {
            try
            {
                using var key = Registry.LocalMachine.CreateSubKey(subkey, true);
                key.DeleteSubKeyTree(dir, false);
            }
            catch { MessageBox.Show("Something went wrong..."); }
        }

        public static void SetWallpaperCompressionQuality(int quality) // ллееньньььпщпьупыкшщзт
        {
            const string subKey = @"Control Panel\Desktop";

            using var key = Registry.CurrentUser.OpenSubKey(subKey, writable: true);
            if (key != null)
            {
                key.SetValue("JPEGImportQuality", quality, RegistryValueKind.DWord);
            }
            else
            {
                using var newKey = Registry.CurrentUser.CreateSubKey(subKey);
                newKey.SetValue("JPEGImportQuality", quality, RegistryValueKind.DWord);
            }
        }
    }
}

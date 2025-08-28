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
        public static bool ContainsReg(RegistryHive registryHive, string key, string name)
        {
            var baseKey = registryHive switch
            {
                RegistryHive.CurrentUser => Registry.CurrentUser,
                RegistryHive.LocalMachine => Registry.LocalMachine,
                _ => null
            };

            using var hkey = baseKey?.OpenSubKey(key);
            return hkey?.GetValue(name) != null;
        }

        /// <summary>
        /// RegistryHive.CurrentUser or RegistryHive.LocalMachine
        /// </summary>
        ///
        public static bool ContainsRegValue<T>(RegistryHive registryHive, string key, string name, T? inputValue = default)
        {
            RegistryKey? baseKey = registryHive switch
            {
                RegistryHive.CurrentUser => Registry.CurrentUser,
                RegistryHive.LocalMachine => Registry.LocalMachine,
                _ => null
            };

            using var target = baseKey?.OpenSubKey(key);
            var value = target?.GetValue(name);

            return value is T t && t.Equals(inputValue);
        }
        public static T? GetRegValue<T>(RegistryHive registryHive, string key, string name, T? fallback = default)
        {
            RegistryKey? baseKey = registryHive switch
            {
                RegistryHive.CurrentUser => Registry.CurrentUser,
                RegistryHive.LocalMachine => Registry.LocalMachine,
                _ => null
            };

            if (baseKey == null) return fallback;

            using var target = baseKey.OpenSubKey(key);
            if (target == null) return fallback;

            try
            {
                var value = target.GetValue(name);
                if (value is T tValue)
                {
                    return tValue;
                }

                if (value != null && !typeof(T).IsAssignableFrom(value.GetType()))
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
            }
            catch (Exception e) { MessageBox.Show(e.Message); }

            return fallback;
        }

        public static void CreateReg<T>(RegistryHive registryHive, string key, string name, string subkey = "", T? value = default)
        {
            RegistryKey? baseKey = registryHive switch
            {
                RegistryHive.CurrentUser => Registry.CurrentUser,
                RegistryHive.LocalMachine => Registry.LocalMachine,
                _ => null
            };
            if (baseKey == null) return;

            using var target = baseKey.CreateSubKey(key);
            if (target == null) return;

            var registryKey = string.IsNullOrEmpty(subkey) ? target : target.CreateSubKey(subkey, true);

            if (value == null)
                return;

            if (value is int intValue)
            {
                registryKey.SetValue(name, intValue, RegistryValueKind.DWord);
            }
            else if (value is string strValue)
            {
                registryKey.SetValue(name, strValue, RegistryValueKind.String);
            }
            ///
            else
            {
                throw new System.Exception();
            }
        }

        public static void CreateSubkey(string subkey)
        {
            Registry.LocalMachine.CreateSubKey(subkey);
        }

        public static void DeleteReg(RegistryHive registryHive, string key, string name)
        {
            try
            {
                var baseKey = registryHive == RegistryHive.CurrentUser ? Registry.CurrentUser : Registry.LocalMachine;
                using var hKey = baseKey.OpenSubKey(key, true);
                hKey?.DeleteValue(name);
            }
            catch { MessageBox.Show("Something went wrong..."); }
        }
        public static void DeleteSubkey(string key, string subkey)
        {
            try
            {
                using var fullKey = Registry.LocalMachine.CreateSubKey(key, true);
                fullKey.DeleteSubKeyTree(subkey, false);
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

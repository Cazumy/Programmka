using Microsoft.Win32;
using System;
using System.IO;

namespace Programmka.Services
{
    public static class MethodsService
    {
        #region base
        public static void SetExeNotifications(bool value)
        {
            const string subkey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System";
            int value1, value2, value3;
            if (value)
            {
                value1 = value2 = value3 = 0;
            }
            else
            {
                value1 = 2;
                value2 = value3 = 1;
            }

            RegeditService.CreateReg(RegistryHive.LocalMachine, subkey, "ConsentPromptBehaviorAdmin", "", value1);
            RegeditService.CreateReg(RegistryHive.LocalMachine, subkey, "EnableLUA", "", value2);
            RegeditService.CreateReg(RegistryHive.LocalMachine, subkey, "PromptOnSecureDesktop", "", value3);
        }
        public static bool CheckExeNotifications()
        {
            const string subkey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System";
            return
                RegeditService.ContainsRegValue<int>(RegistryHive.LocalMachine, subkey, "ConsentPromptBehaviorAdmin") &&
                RegeditService.ContainsRegValue<int>(RegistryHive.LocalMachine, subkey, "EnableLUA") &&
                RegeditService.ContainsRegValue<int>(RegistryHive.LocalMachine, subkey, "PromptOnSecureDesktop");
        }

        public static void SetHibernation(bool value)
        {
            const string subkey = @"SYSTEM\CurrentControlSet\Control\Power";
            const string key = "HibernateEnabled";
            int iValue = value ? 0 : 1;
            RegeditService.CreateReg(RegistryHive.LocalMachine, subkey, key, "", iValue);
        }
        public static bool CheckHibernation()
        {
            const string subkey = @"SYSTEM\CurrentControlSet\Control\Power";
            const string key = "HibernateEnabled";
            return RegeditService.ContainsRegValue<int>(RegistryHive.LocalMachine, subkey, key);
        }
        public static void SetMouseAcceleration(bool value)
        {
            const string subkey = @"Control Panel\Mouse";
            const string key1 = "MouseSpeed", key2 = "MouseThreshold1", key3 = "MouseThreshold2";
            string value1, value2, value3;
            if (value)
            {
                value1 = value2 = value3 = "0";
            }
            else
            {
                value1 = "1"; value2 = "6"; value3 = "10";
            }
            RegeditService.CreateReg(RegistryHive.CurrentUser, subkey, key1, "", value1);
            RegeditService.CreateReg(RegistryHive.CurrentUser, subkey, key2, "", value2);
            RegeditService.CreateReg(RegistryHive.CurrentUser, subkey, key3, "", value3);
        }
        public static bool CheckMouseAcceleration()
        {
            const string subkey = @"Control Panel\Mouse";
            const string key1 = "MouseSpeed", key2 = "MouseThreshold1", key3 = "MouseThreshold2";
            return RegeditService.ContainsRegValue(RegistryHive.CurrentUser, subkey, key1, "0") &&
                RegeditService.ContainsRegValue(RegistryHive.CurrentUser, subkey, key2, "0") &&
                RegeditService.ContainsRegValue(RegistryHive.CurrentUser, subkey, key3, "0");
        }
        public static void SetKeySticking(bool value)
        {
            const string subkey = @"Control Panel\Accessibility\StickyKeys";
            const string key = "Flags";
            string keyValue = value ? "506" : "511";
            RegeditService.CreateReg(RegistryHive.CurrentUser, subkey, key, "", keyValue);
        }
        public static bool CheckKeySticking()
        {
            const string subkey = @"Control Panel\Accessibility\StickyKeys";
            const string key = "Flags";
            return RegeditService.ContainsRegValue(RegistryHive.CurrentUser, subkey, key, "506");
        }
        #endregion
        #region explorer
        public static void SetDiskDuplicate(bool value)
        {
            const string subkey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\DelegateFolders";
            const string dir = "{F5FB2C77-0E2F-4A16-A381-3E560C68BC83}";
            if (value)
            {
                RegeditService.DeleteRegDir(subkey, dir);
            }
            else
            {
                const string key = "Removable Drives";
                RegeditService.CreateReg(RegistryHive.LocalMachine, subkey, key, dir, "");
            }
        }
        public static bool CheckDuplicate()
        {
            const string subkey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\DelegateFolders\{F5FB2C77-0E2F-4A16-A381-3E560C68BC83}";
            return Registry.LocalMachine.OpenSubKey(subkey) == null;
        }
        public static void SetQuickAccess(bool value)
        {
            const string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer";
            const string key = "HubMode";
            if (value)
            {
                RegeditService.CreateReg(RegistryHive.LocalMachine, subKey, key, "", "1");
            }
            else
            {
                RegeditService.DeleteReg(RegistryHive.LocalMachine, subKey, key);
            }
        }
        public static bool CheckQuickAccess()
        {
            const string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer", key = "HubMode";
            return RegeditService.ContainsReg(RegistryHive.CurrentUser, subKey, key);
        }
        public static void SetObjects3D(bool value)
        {
            if (value)
            {
                const string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\"; const string dir = "{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}";
                RegeditService.DeleteRegDir(subKey, dir);
            }
            else
            {
                const string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}";
                RegeditService.CreateRegDir(subKey);
            }
        }
        public static bool Check3DObjects()
        {
            const string subkey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}";
            return Registry.LocalMachine.OpenSubKey(subkey) == null;
        }
        public static async void SetNetworkIcon(bool value)
        {
            if (value)
            {
                var commands = new[]
                {
                    "reg add \"HKCU\\Software\\Classes\\CLSID\\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\\ShellFolder\" /v \"Attributes\" /t REG_DWORD /d \"0xb0940064\" /f >nul 2>&1",
                    "reg add \"HKCU\\Software\\Classes\\CLSID\\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\" /v \"System.IsPinnedtoNameSpaceTree\" /t REG_DWORD /d \"0\" /f >nul 2>&1",
                    "reg add \"HKCU\\Software\\Classes\\WOW6432Node\\CLSID\\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\\ShellFolder\" /v \"Attributes\" /t REG_DWORD /d \"0xb0940064\" /f >nul",
                    "reg add \"HKCU\\Software\\Classes\\WOW6432Node\\CLSID\\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\" /v \"System.IsPinnedtoNameSpaceTree\" /t REG_DWORD /d \"0\" /f >nul 2>&1",
                    "TI.exe cmd.exe /c reg add \"HKLM\\SOFTWARE\\Classes\\CLSID\\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\\ShellFolder\" /v \"Attributes\" /t REG_DWORD /d \"0xb0040064\" /f >nul 2>&1",
                    "TI.exe cmd.exe /c reg delete \"HKLM\\SOFTWARE\\Classes\\CLSID\\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\" /v \"System.IsPinnedtoNameSpaceTree\" /f >nul 2>&1",
                    "TI.exe cmd.exe /c reg add \"HKLM\\SOFTWARE\\Classes\\WOW6432Node\\CLSID\\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\\ShellFolder\" /v \"Attributes\" /t REG_DWORD /d \"0xb0040064\" /f >nul 2>&1",
                    "TI.exe cmd.exe /c reg delete \"HKLM\\SOFTWARE\\Classes\\WOW6432Node\\CLSID\\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\" /v \"System.IsPinnedtoNameSpaceTree\" /f >nul 2>&1",
                };
                await WinCmdService.RunInCMD(commands);
            }
            else
            {
                var commands = new[]
                {
                    "reg add \"HKCU\\Software\\Classes\\CLSID\\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\\ShellFolder\" /v \"Attributes\" /t REG_DWORD /d \"0xb0040064\" /f >nul 2>&1",
                    "reg delete \"HKCU\\Software\\Classes\\CLSID\\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\" /v \"System.IsPinnedtoNameSpaceTree\" /f >nul 2>&1",
                    "reg add \"HKCU\\Software\\Classes\\WOW6432Node\\CLSID\\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\\ShellFolder\" /v \"Attributes\" /t REG_DWORD /d \"0xb0040064\" /f >nul",
                    "reg delete \"HKCU\\Software\\Classes\\WOW6432Node\\CLSID\\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\" /v \"System.IsPinnedtoNameSpaceTree\" /f >nul 2>&1",
                    "TI.exe cmd.exe /c reg add \"HKLM\\SOFTWARE\\Classes\\CLSID\\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\\ShellFolder\" /v \"Attributes\" /t REG_DWORD /d \"0xb0940064\" /f >nul 2>&1",
                    "TI.exe cmd.exe /c reg delete \"HKLM\\SOFTWARE\\Classes\\CLSID\\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\" /v \"System.IsPinnedtoNameSpaceTree\" /f >nul 2>&1",
                    "TI.exe cmd.exe /c reg add \"HKLM\\SOFTWARE\\Classes\\WOW6432Node\\CLSID\\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\\ShellFolder\" /v \"Attributes\" /t REG_DWORD /d \"0xb0940064\" /f >nul 2>&1",
                    "TI.exe cmd.exe /c reg delete \"HKLM\\SOFTWARE\\Classes\\WOW6432Node\\CLSID\\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\" /v \"System.IsPinnedtoNameSpaceTree\" /f >nul 2>&1",
                };
                await WinCmdService.RunInCMD(commands);
            }
        }
        public static bool CheckNetworkIcon()
        {
            bool attributes = RegeditService.ContainsRegValue(RegistryHive.CurrentUser, @"SOFTWARE\Classes\CLSID\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\ShellFolder", "Attributes", 0xb0940064U);
            const string subkey = @"SOFTWARE\Classes\CLSID\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}"; const string key = "System.IsPinnedtoNameSpaceTree";
            return !(!attributes && !RegeditService.ContainsRegValue<int>(RegistryHive.CurrentUser, subkey, key));
        }
        public static void SetFileExtensions(bool value)
        {
            const string subkey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
            const string key = "HideFileExt";
            if (value)
            {
                RegeditService.CreateReg(RegistryHive.CurrentUser, subkey, key, "", "");
            }
            else
            {
                const int iValue = 1;
                RegeditService.CreateReg(RegistryHive.CurrentUser, subkey, key, "", iValue);
            }
        }
        public static bool CheckFileExtensions()
        {
            const string subkey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
            const string key = "HideFileExt";
            return RegeditService.ContainsRegValue<int>(RegistryHive.CurrentUser, subkey, key);
        }
        #endregion
        #region desktop
        public static void SetLabelArrows(bool value)
        {
            if (value)
            {
                const string subkey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons";
                const string key = "29";
                using var target = Registry.LocalMachine.CreateSubKey(subkey);
                target.SetValue(key, "", RegistryValueKind.String);
            }
            else
            {
                const string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\"; const string dir = "Shell Icons";
                RegeditService.DeleteRegDir(subKey, dir);
            }
        }
        public static bool CheckLabels()
        {
            const string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons", key = "29";
            return RegeditService.ContainsReg(RegistryHive.LocalMachine, subKey, key);
        }

        private const string wallpaperFolder = "ProgrammkaWallpapersTemp";
        public static (string, string) LoadWallpaperImage()
        {
            const string registryKey = @"Control Panel\Desktop";
            const string registryValue = "WallPaper";

            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(registryKey);
            string? wallpaper = null;
            if (key != null)
            {
                wallpaper = key.GetValue(registryValue)?.ToString();
            }

            bool wallpaperExists = !string.IsNullOrEmpty(wallpaper) && File.Exists(wallpaper);
            if (!wallpaperExists)
            {
                string appFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), wallpaperFolder);
                if (!Directory.Exists(appFolder)) Directory.CreateDirectory(appFolder);

                wallpaper = Path.Combine(appFolder, "default_background.jpg");

                if (!File.Exists(wallpaper))
                {
                    using Stream? resourceStream = System.Windows.Application.GetResourceStream(new Uri("/Resources/Images/background.jpg", UriKind.Relative))?.Stream;
                    using var fileStream = new FileStream(wallpaper, FileMode.Create, FileAccess.Write);
                    resourceStream?.CopyTo(fileStream);
                }
            }

            string appCacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), wallpaperFolder);
            if (!Directory.Exists(appCacheFolder)) Directory.CreateDirectory(appCacheFolder);

            string compressedWallpaper = Path.Combine(appCacheFolder, "compressed_wallpaper.jpg");

            using var image = SixLabors.ImageSharp.Image.Load(wallpaper!);
            var encoder = new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder { Quality = 35 };
            using var outputStream = new FileStream(compressedWallpaper, FileMode.Create);
            image.Save(outputStream, encoder);

            return (wallpaper!, compressedWallpaper);
        }
        public static void DeleteWallpaperTemp()
        {
            if (Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), wallpaperFolder)))
            {
                try
                {
                    Directory.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), wallpaperFolder), true);
                }
                catch (Exception) { }
            }
        }
        public static void SetWallpaperCompression(bool value)
        {
            int keyValue = value ? 100 : 80;
            RegeditService.SetWallpaperCompressionQuality(keyValue);
        }
        public static bool CheckWallpaperCompression()
        {
            const string subKey = @"Control Panel\Desktop"; const string key = "JPEGImportQuality"; const int value = 100;
            return RegeditService.ContainsRegValue(RegistryHive.CurrentUser, subKey, key, value);
        }
        public static void SetHighlightColor(string rgbValue)
        {
            const string subkey = @"Control Panel\Colors";
            RegeditService.CreateReg(RegistryHive.CurrentUser, subkey, "Hilight", "", rgbValue);
        }
        public static string GetHighlightColor()
        {
            const string subkey = @"Control Panel\Colors";
            return RegeditService.GetRegValue<string>(RegistryHive.CurrentUser, subkey, "Hilight")!;
        }
        #endregion
    }
}

#pragma warning disable CS8618

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Programmka.Middleware;
using Programmka.Models;
using Programmka.Services;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using static Programmka.Services.MethodsService;

namespace Programmka.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool loadingStatus;
        public MainViewModel()
        {
            CommandMiddleware.SetStatus = status => TabItemDescription = status;
            SetWallpaperImage();
            SetBrush();
            UpdateTempSizeText();
        }
        #region tweaks
        #region base
        public ToggleAction ExeNotifications { get; } = new(
            onStatus: "Выкл.",
            offStatus: "Вкл.",
            initial: CheckExeNotifications(),
            callback: SetExeNotifications);
        public ToggleAction Hibernation { get; } = new(
            onStatus: "Выкл.",
            offStatus: "Вкл.",
            initial: CheckHibernation(),
            callback: SetHibernation);
        public ToggleAction MouseAcceleration { get; } = new(
            onStatus: "Выкл.",
            offStatus: "Вкл.",
            initial: CheckMouseAcceleration(),
            callback: SetMouseAcceleration);
        public ToggleAction KeySticking { get; } = new(
            onStatus: "Выкл.",
            offStatus: "Вкл.",
            initial: CheckKeySticking(),
            callback: SetKeySticking);
        #endregion
        #region explorer
        [ObservableProperty]
        private bool explorerExtensionsEnabled;
        [RelayCommand]
        private static void ReloadExplorer()
        {
            foreach (var process in Process.GetProcessesByName("explorer"))
            {
                process.Kill();
            }
            Process.Start("explorer.exe");
        }
        public ToggleAction DiskDuplicate { get; } = new(
            onStatus: "Выкл.",
            offStatus: "Вкл.",
            initial: CheckDuplicate(),
            callback: SetDiskDuplicate);
        public ToggleAction QuickAccess { get; } = new(
            onStatus: "Выкл.",
            offStatus: "Вкл.",
            initial: CheckQuickAccess(),
            callback: SetQuickAccess);
        public ToggleAction Objects3D { get; } = new(
            onStatus: "Выкл.",
            offStatus: "Вкл.",
            initial: Check3DObjects(),
            callback: SetObjects3D);
        public ToggleAction NetworkIcon { get; } = new(
            onStatus: "Выкл.",
            offStatus: "Вкл.",
            initial: CheckNetworkIcon(),
            callback: SetNetworkIcon);
        public ToggleAction FileExtensions { get; } = new(
            onStatus: "Вкл.",
            offStatus: "Выкл.",
            initial: CheckFileExtensions(),
            callback: SetFileExtensions);
        #endregion
        #region desktop
        public ToggleAction LabelArrows { get; } = new(
            onStatus: "Выкл.",
            offStatus: "Вкл.",
            initial: CheckLabels(),
            callback: SetLabelArrows);
        public ToggleAction WallpaperCompression { get; } = new(
            onStatus: "Выкл.",
            offStatus: "Вкл.",
            initial: CheckWallpaperCompression(),
            callback: SetWallpaperCompression);

        [ObservableProperty]
        private string wallpaperImageSource;
        [ObservableProperty]
        private string compressedWallpaperImageSource;
        private void SetWallpaperImage()
        {
            var paths = LoadWallpaperImage();
            WallpaperImageSource = paths.Item1;
            CompressedWallpaperImageSource = paths.Item2;
        }
        [ObservableProperty]
        private SolidColorBrush selectedBrush;

        partial void OnSelectedBrushChanged(SolidColorBrush value)
        {
            OnPropertyChanged(nameof(BorderBrush));
            OnPropertyChanged(nameof(BackgroundBrush));
        }
        public Brush? BorderBrush => SelectedBrush is SolidColorBrush solid
            ? new SolidColorBrush(solid.Color) : Brushes.Transparent;

        public Brush? BackgroundBrush => SelectedBrush is SolidColorBrush solid
                ? new SolidColorBrush(Color.FromArgb(0x30, solid.Color.R, solid.Color.G, solid.Color.B)) : Brushes.Transparent;
        private void SetBrush()
        {
            var colorString = GetHighlightColor().Split(' ');
            if (colorString.Length != 3 ||
                !byte.TryParse(colorString[0], out byte r) || !byte.TryParse(colorString[1], out byte g) || !byte.TryParse(colorString[2], out byte b))
            {
                SelectedBrush = Brushes.Transparent;
                return;
            }
            SelectedBrush = new SolidColorBrush(Color.FromRgb(r, g, b));
        }
        [RelayCommand]
        private static void ChangeHighlightColor(SolidColorBrush brush) => CommandMiddleware.Run(() =>
        {
            if (brush == null) return;
            var color = brush.Color;
            var rgbValue = $"{color.R} {color.G} {color.B}";
            SetHighlightColor(rgbValue);
        });
        #endregion
        #region activations
        [RelayCommand]
        private static void ActivateWindows()
        {
            var cmdProcessInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c powershell.exe -Command \"irm https://massgrave.dev/get | iex\"",
                Verb = "runas",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            Process.Start(cmdProcessInfo);
        }
        [RelayCommand]
        private void ActivateWinRar()
        {
            LoadingStatus = true;
            var dialog = MessageBox.Show("Вы меняли директорию по умолчанию для установки WINRAR?", "Ответьте на вопрос", MessageBoxButton.YesNoCancel);
            string? selectedFolderPath;
            if (dialog == MessageBoxResult.Cancel) { LoadingStatus = false; return; }
            else if (dialog == MessageBoxResult.No)
            {
                selectedFolderPath = @"C:\Program Files\WinRAR";
            }
            else
            {
                var folderDialog = new Microsoft.Win32.OpenFolderDialog { Title = "Выберите папку, в которой установлен WinRar" };
                folderDialog.ShowDialog();
                selectedFolderPath = folderDialog.FolderName;
            }
            if (string.IsNullOrEmpty(selectedFolderPath)) { LoadingStatus = false; return; }
            const string key = @"RAR registration data
PROMSTROI GROUP
15 PC usage license
UID = 42079a849eb3990521f3
641221225021f37c3fecc934136f31d889c3ca46ffcfd8441d3d58
9157709ba0f6ded3a528605030bb9d68eae7df5fedcd1c12e96626
705f33dd41af323a0652075c3cb429f7fc3974f55d1b60e9293e82
ed467e6e4f126e19cccccf98c3b9f98c4660341d700d11a5c1aa52
be9caf70ca9cee8199c54758f64acc9c27d3968d5e69ecb901b91d
538d079f9f1fd1a81d656627d962bf547c38ebbda774df21605c33
eccb9c18530ee0d147058f8b282a9ccfc31322fafcbb4251940582";
            File.WriteAllText(selectedFolderPath + @"\rarreg.key.txt", key);
            LoadingStatus = false;
        }
        [RelayCommand]
        private async Task BecameAdminWin10()
        {
            await WinCmdService.RunInCMD($"net user {System.Environment.UserName} /active:yes");
            TabItemDescription = "Пользователь стал администратором";
        }
        #endregion
        #region fixes
        public static ICommand FixHardDisksCommand => CommandMiddleware.Run(async () =>
        {
            const string command = "reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\storahci\\Parameters\\Device\" /f /v TreatAsInternalPort /t REG_MULTI_SZ /d \"0\\0 1\\0 2\\0 3\\0 4\\0 5\\0 6\\0 7\\0 8\\0 9\\0 10\\0 11\\0 12\\0 13\\0 14\\0 15\\0 16";
            await WinCmdService.RunInCMD(command);
        });
        public static ICommand ReturnLabelArrowsCommand => CommandMiddleware.Run(() =>
        {
            const string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\"; const string dir = "Shell Icons";
            RegeditService.DeleteRegDir(subKey, dir);
        });
        [RelayCommand]
        private static async Task RepairSystem()
        {
            const string commandCmd = "sfc /scannow";
            const string commandPowerShell = "dism /Online /Cleanup-Image /RestoreHealth";
            await WinCmdService.RunInCMD(commandCmd, true);
            WinCmdService.RunInPowerShell(commandPowerShell);
        }
        #endregion
        #region downloading
        [RelayCommand]
        private async Task DownloadOffice()
        {
            LoadingStatus = true;
            const string fileName = "Configuration.xml";
            System.Text.StringBuilder fileContent = new("""
<Configuration ID="aa6c9195-b180-4d82-b808-48f4d1886c73">
  <Add OfficeClientEdition="64" Channel="PerpetualVL2024">
    <Product ID="ProPlus2024Volume" PIDKEY="XJ2XN-FW8RK-P4HMP-DKDBV-GCVGB">
      <Language ID="ru-ru" />
""" + "\n");
            var selectionWindow = new Views.OfficeSelectionWindow();
            bool? result = selectionWindow.ShowDialog();
            if (!selectionWindow.IsConfirmed)
            {
                TabItemDescription = "Установка офиса отменена";
                LoadingStatus = false;
                return;
            }
            if (!selectionWindow.officeSelections[0]) { fileContent.Append("""      <ExcludeApp ID="Access" />""" + "\n"); }
            if (!selectionWindow.officeSelections[1]) { fileContent.Append("""      <ExcludeApp ID="OneDrive" />""" + "\n"); }
            if (!selectionWindow.officeSelections[2]) { fileContent.Append("""      <ExcludeApp ID="Outlook" />""" + "\n"); }
            if (!selectionWindow.officeSelections[3]) { fileContent.Append("""      <ExcludeApp ID="Publisher" />""" + "\n"); }
            if (!selectionWindow.officeSelections[4]) { fileContent.Append("""      <ExcludeApp ID="Excel" />""" + "\n"); }
            if (!selectionWindow.officeSelections[5]) { fileContent.Append("""      <ExcludeApp ID="Lync" />""" + "\n"); }
            if (!selectionWindow.officeSelections[6]) { fileContent.Append("""      <ExcludeApp ID="OneNote" />""" + "\n"); }
            if (!selectionWindow.officeSelections[7]) { fileContent.Append("""      <ExcludeApp ID="PowerPoint" />""" + "\n"); }
            if (!selectionWindow.officeSelections[8]) { fileContent.Append("""      <ExcludeApp ID="Word" />""" + "\n"); }
            fileContent.Append("""
    </Product>
  </Add>
  <Property Name="SharedComputerLicensing" Value="0" />
  <Property Name="FORCEAPPSHUTDOWN" Value="FALSE" />
  <Property Name="DeviceBasedLicensing" Value="0" />
  <Property Name="SCLCacheOverride" Value="0" />
  <Property Name="AUTOACTIVATE" Value="1" />
  <Updates Enabled="TRUE" />
  <RemoveMSI />
</Configuration>
""");
            File.WriteAllText(@"C:\Program Files\Microsoft Office\" + fileName, fileContent.ToString());

            const string urlOffice = "https://officecdn.microsoft.com/pr/wsus/setup.exe";
            using var httpClient = new System.Net.Http.HttpClient();
            var response = await httpClient.GetAsync(urlOffice, System.Net.Http.HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            await using (var contentStream = await response.Content.ReadAsStreamAsync())
            await using (var fileStream = new FileStream(@"C:\Program Files\Microsoft Office\setup.exe", FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
            {
                await contentStream.CopyToAsync(fileStream);
            }

            const string reg = """reg add "HKCU\Software\Microsoft\Office\16.0\Common\ExperimentConfigs\Ecs" /v "CountryCode" /t REG_SZ /d "std::wstring|US" /f""";
            await WinCmdService.RunInCMD(reg);
            const string command = """cd /d "C:\Program Files\Microsoft Office" && setup.exe /configure Configuration.xml""";
            LoadingStatus = false;
            await WinCmdService.RunInCMD(command);
        }
        #endregion
        #region cleanup
        [ObservableProperty]
        private string? tempSizeText;
        [RelayCommand]
        private void UpdateTempSizeText() => TempSizeText = TempCleanService.NormalizeByteSize(TempCleanService.GetFullTempSize());
        [RelayCommand]
        private static void CheckWinSxS() => WinCmdService.RunInPowerShell("Dism.exe /Online /Cleanup-Image /AnalyzeComponentStore", "-NoExit");
        [RelayCommand]
        private static void CleanupWinSxS()
        {
            WinCmdService.RunInPowerShell("Dism.exe /Online /Cleanup-Image /StartComponentCleanup");
            WinCmdService.RunInPowerShell("Dism.exe /Online /Cleanup-Image /StartComponentCleanup /ResetBase");
        }
        [RelayCommand]
        private async Task CleanupTemp()
        {
            LoadingStatus = true;
            long prevSize = TempCleanService.GetFullTempSize();
            await TempCleanService.CleanAllTemp();
            var byteDiff = prevSize - TempCleanService.GetFullTempSize();
            TabItemDescription = $"Успешно очищено: {TempCleanService.NormalizeByteSize(byteDiff)}";
            UpdateTempSizeText();
            LoadingStatus = false;
        }
        #endregion
        #endregion
        #region decor
        [ObservableProperty]
        private string? tabItemDescription;

        [RelayCommand]
        private void TabMouseEnter(object sender)
        {
            if (sender is System.Windows.Controls.TabItem tabItem)
            {
                TabItemDescription = tabItem.Name switch
                {
                    "BaseTweaksItem" => "Базовые твики",
                    "FileExplorerItem" => "Кастомизация проводника",
                    "DesktopItem" => "Кастомизация рабочего стола",
                    "ActivationItem" => "Активация",
                    "FixesItem" => "Исправление багов системы",
                    "Downloads" => "Загрузка приложений и файлов",
                    "Cleanup" => "Очистка системы",
                    _ => string.Empty,
                };
            }
        }
        [RelayCommand]
        private void TabMouseLeave() => TabItemDescription = string.Empty;
        #endregion
    }
}
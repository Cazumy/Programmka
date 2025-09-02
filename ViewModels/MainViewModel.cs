#pragma warning disable IDE0079 // Удалить ненужное подавление
#pragma warning disable CS8618
#pragma warning disable CA1416 // Проверка совместимости платформы

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Programmka.Middleware;
using Programmka.Models;
using Programmka.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using static Programmka.Services.MethodsService;

namespace Programmka.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        public static MainViewModel Instance { get; private set; }
        public MainViewModel()
        {
            Instance = this;
            CommandMiddleware.SetStatus = status => TabItemDescription = status;
            SetWallpaperImage();
            SetHighlightBrush();
            DeleteTextInfoFix();

            WallpaperCompression = new( // init here cause of conflict beetween static and this
                onStatus: "Выкл.",
                offStatus: "Вкл.",
                initial: CheckWallpaperCompression(),
                callback: value =>
                {
                    SetWallpaperCompression(value); // static method
                    this.UpdateWallpaperImage(); // instance method
                });
            UpdateWallpaperImage();

            TabItemDescription = String.Empty;
        }
        [RelayCommand] private async Task MainWindowLoaded()
        {
            UpdateCleanupPageInfo();

            AppUpdaterService.UpdateInfo? update = await AppUpdaterService.CheckForUpdateAsync(); // update check
            UpdateAvailable = update != null;
        }
        [RelayCommand] private static void MainWindowClosing()          // deleting all (as planned) temp files on exit
        {
            if (Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), wallpaperFolder)))
            {
                try
                {
                    Directory.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), wallpaperFolder), true);
                }
                catch (Exception e) { Debug.WriteLine(e); }
            }
        }
        [ObservableProperty] private bool updateAvailable;
        [RelayCommand] private static async Task UpdateApp()
        {
            if (Instance.UpdateAvailable)
            {
                Instance.LoadingStatus = true;
                Instance.TabItemDescription = "Применение обновления...";
                var mainWindow = Application.Current.MainWindow;
                if (mainWindow.Content is Panel panel)
                {
                    foreach (UIElement child in panel.Children)
                    {
                        if (child is Border border && border.Name == "CloseButton")
                        {
                            border.IsEnabled = true;
                            border.Opacity = 1.0;
                        }
                        else
                        {
                            child.IsEnabled = false;
                            child.Opacity = 0.5;
                        }
                    }
                }

#pragma warning disable CS8604 // Возможно, аргумент-ссылка, допускающий значение NULL.
                await AppUpdaterService.ApplyUpdateAsync(await AppUpdaterService.CheckForUpdateAsync()); // 100% not null here
#pragma warning restore CS8604 // Возможно, аргумент-ссылка, допускающий значение NULL.
            }
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
        public ToggleAction StartupDelay { get; } = new(
            onStatus: "Вкл.",
            offStatus: "Выкл.",
            initial: CheckStartupDelay(),
            callback: SetStartupDelay);
        #endregion
        #region explorer
        [RelayCommand] private static void ReloadExplorer()
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

        [ObservableProperty] private ImageSource wallpaperImageSource;
        private ImageSource normalImageSource;
        private ImageSource compressedImageSource;
        private const string wallpaperFolder = "ProgrammkaWallpapersTemp";
        public ToggleAction WallpaperCompression { get; }
        private void SetWallpaperImage()
        {
            var path = LoadWallpaperImage(wallpaperFolder);
            normalImageSource = ImagesService.LoadImage(path.Item1); //pack
            compressedImageSource = ImagesService.LoadImage(path.Item2); //file
            UpdateWallpaperImage();
        }
        private void UpdateWallpaperImage()
        {
            if (WallpaperCompression == null) { return; }
            WallpaperImageSource = WallpaperCompression.IsChecked ? normalImageSource : compressedImageSource;
        }

        [ObservableProperty] private SolidColorBrush selectedBrush;
        partial void OnSelectedBrushChanged(SolidColorBrush value)
        {
            OnPropertyChanged(nameof(BorderBrush));
            OnPropertyChanged(nameof(BackgroundBrush));
        }
        public Brush? BorderBrush => SelectedBrush is SolidColorBrush solid
            ? new SolidColorBrush(solid.Color) : Brushes.Transparent;
        public Brush? BackgroundBrush => SelectedBrush is SolidColorBrush solid
                ? new SolidColorBrush(Color.FromArgb(0x30, solid.Color.R, solid.Color.G, solid.Color.B)) : Brushes.Transparent;
        private void SetHighlightBrush()
        {
            var colorString = GetHighlightColor().Split(' ');
            if (colorString.Length != 3 || !byte.TryParse(colorString[0], out byte r) || !byte.TryParse(colorString[1], out byte g) || !byte.TryParse(colorString[2], out byte b))
            {
                SelectedBrush = Brushes.Transparent;
                return;
            }
            SelectedBrush = new SolidColorBrush(Color.FromRgb(r, g, b));
        }
        [RelayCommand] private static void ChangeHighlightColor(SolidColorBrush brush) => CommandMiddleware.Run(() =>
        {
            if (brush == null) return;
            var color = brush.Color;
            var rgbValue = $"{color.R} {color.G} {color.B}";
            SetHighlightColor(rgbValue);
        }).Execute(null);
        #endregion
        #region activations
        [RelayCommand]
        private static async Task ActivateWindows()
        {
            Instance.LoadingStatus = true;
            await WinCmdService.RunInPowerShell("irm https://get.activated.win | iex", showWindow: true); 132132
            Instance.LoadingStatus = false;
        }

        [RelayCommand] private static void ActivateWinRar()
        {
            Instance.LoadingStatus = true;
            var dialog = HandyControl.Controls.MessageBox.Show("Вы меняли директорию по умолчанию для установки WINRAR?", "Ответьте на вопрос", MessageBoxButton.YesNoCancel);
            string? selectedFolderPath;
            if (dialog == MessageBoxResult.Cancel) { Instance.LoadingStatus = false; Instance.TabItemDescription = "Процесс активации прерван"; return; }
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
            if (string.IsNullOrEmpty(selectedFolderPath)) { Instance.LoadingStatus = false; Instance.TabItemDescription = "Процесс активации прерван"; return; }
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
            Instance.TabItemDescription = "WinRar успешно активан";
            Instance.LoadingStatus = false;
        }
        [RelayCommand] private async Task BecameAdminWin10()
        {
            await WinCmdService.RunInCMD($"net user {Environment.UserName} /active:yes");
            TabItemDescription = "Пользователь стал администратором";
        }
        #endregion
        #region fixes
        [ObservableProperty] private string textInfoFix;

        [ObservableProperty] private BitmapImage currentInfoImage;
        [RelayCommand] public void DeleteTextInfoFix()
        {
            TextInfoFix = "Описание проблемы.";
            CurrentInfoImage = ImagesService.LoadImage("pack://application:,,,/Programmka;component/Resources/Images/InfoExamples/info-image-example.png");
        }
        ////
        [RelayCommand] public void ShowInfoFixHardDisks()
        {
            TextInfoFix = "Внутренние диски SATA (жесткие диски и твердотельные накопители) могут отображаться как съемные носители.\n" +
                "Перед фиксом прежде всего проверьте наличие доступных обновлений BIOS.\n" +
                "При исправлении все диски, подключенные через SATA порт будут отображаться как внутренние диски.";
            CurrentInfoImage = ImagesService.LoadImage("pack://application:,,,/Programmka;component/Resources/Images/InfoExamples/disk-duplication.png");
        }
        [RelayCommand] public static void FixHardDisks() => CommandMiddleware.Run(async () =>
        {
            const string command = "reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\storahci\\Parameters\\Device\" /f /v TreatAsInternalPort /t REG_MULTI_SZ /d \"0\\0 1\\0 2\\0 3\\0 4\\0 5\\0 6\\0 7\\0 8\\0 9\\0 10\\0 11\\0 12\\0 13\\0 14\\0 15\\0 16";
            await WinCmdService.RunInCMD(command);
            Instance.TabItemDescription = "Исправление применено, перезагрузите ПК";
        }).Execute(null);
        ////
        [RelayCommand] private void ShowInfoFixArrowLabels()
        {
            TextInfoFix = "Наиболее вероятная причина - это твик с отключением стрелок на ярлыках, фикс возвращает иконки папок и ярлыков, но также возвращает и стрелки.\n" +
                "Если фикс не помог, то пройдитесь по базе: перезагрузка ПК, проверка системных файлов и т.д.";
            CurrentInfoImage = ImagesService.LoadImage("pack://application:,,,/Programmka;component/Resources/Images/InfoExamples/arrow-labels-tweak-bug.png");
        }
        [RelayCommand] public static void FixArrowLabels() => CommandMiddleware.Run(() =>
        {
            const string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\"; const string dir = "Shell Icons";
            RegeditService.DeleteSubkey(subKey, dir);
            Instance.TabItemDescription = "Исправление применено, перезагрузите ПК";
        }).Execute(null);
        ////
        [RelayCommand] private void ShowInfoRestoreSysFiles()
        {
            TextInfoFix = "Произойдёт проверка и автоматическое восстановление повреждённых системных файлов Windows, используя их кэшированные копии и файлы из Центра обновления.";
            CurrentInfoImage = ImagesService.LoadImage("pack://application:,,,/Programmka;component/Resources/Images/InfoExamples/restore-sys-files.png");
        }
        [RelayCommand] private async Task RestoreSysFiles()
        {
            LoadingStatus = true;
            const string commandCmd = "sfc /scannow";
            const string commandPowerShell = "dism /Online /Cleanup-Image /RestoreHealth";
            await WinCmdService.RunInCMD(commandCmd, true, false);
            await WinCmdService.RunInPowerShell(commandPowerShell, showWindow: true);
            LoadingStatus = false;
        }
        ////
        [RelayCommand] private void ShowInfoAppVerifier()
        {
            TextInfoFix = "Устранение проблем с компонентами Windows, вызванных неправильными путями к .dll-файлам, которые загромождают диск C:\\. Исправление автоматически обновит соответствующие записи реестра, чтобы они указывали на правильные расположения файлов, и перемещает файлы в соответствующий каталог C:\\Windows\\SysWOW64.";
            CurrentInfoImage = ImagesService.LoadImage("pack://application:,,,/Programmka;component/Resources/Images/InfoExamples/app-verifier.png");
        }
        [RelayCommand] private static async Task ReplaceAppVerifierDll()
        {
            const string bat = """
@echo off
setlocal

REM Define constants
set "COMP=HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Components"
set "OUT_DIR=C:\Windows\SysWOW64"

REM Start processing
goto :Process

:Fix
REM Arguments: %1 - Registry key suffix, %2 - Old file path, %3 - New file path
set "KEY=%COMP%\%~1"
set "OLD_FILE=%~2"
set "NEW_FILE=%~3"

REM Search for the registry value matching the old file path
for /f "tokens=1,2,*" %%A in ('reg query "%KEY%" 2^>nul') do (
    if "%%B"=="REG_SZ" if /i "%%C"=="%OLD_FILE%" (
        set "VAL=%%A"
        goto :Found
    )
)

REM If not found, log and check the file existence
echo Value %OLD_FILE% not found in %KEY%
goto :CheckFile

:Found
REM Update the registry value to the new file path
reg add "%KEY%" /v "%VAL%" /t REG_SZ /d "%NEW_FILE%" /f >nul && (
    echo Registry component %KEY%\%VAL% fixed
) || (
    echo Failed to update registry for %KEY%\%VAL%
)

:CheckFile
REM Check if the old file exists and move it to the new location
if exist "%OLD_FILE%" (
    move /Y "%OLD_FILE%" "%NEW_FILE%" >nul && (
        echo %OLD_FILE% successfully moved to %NEW_FILE%
    ) || (
        echo Failed to move %OLD_FILE%
    )
) else (
    echo %OLD_FILE% does not exist, nothing to move
)
goto :EOF

:Process
REM Fix registry and move files
call :Fix "0AF818DE4685190F5347FAF54BD80C82" "C:\appverifUI.dll" "%OUT_DIR%\appverifUI.dll"
call :Fix "E0C37F311948CF28AE087B694A681271" "C:\vfcompat.dll" "%OUT_DIR%\vfcompat.dll"

REM End script
endlocal
""";
            await WinCmdService.RunBat(bat, false);
            Instance.TabItemDescription = "Исправление применено";
        }
        #endregion
        #region downloading
        [RelayCommand]
        private async Task DownloadOffice()
        {
            Instance.LoadingStatus = true;
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
        private string cleanupInfoText;
        public string CleanupInfoText
        {
            get => cleanupInfoText;
            set
            {
                string? cleanValue = value?.Trim();
                if (cleanValue == null || cleanValue == cleanupInfoText) return;
                if (cleanValue.Contains('%'))
                {
                    var m = Regex.Match(cleanValue, @"([-+]?\d+(?:\.\d+)?)\s*%");
                    cleanupInfoText = $"Процедура завершена на {m.Groups[1].Value} %";
                }
                else if (cleanValue.Contains("Рекомендуется очистка хранилища компонентов"))
                {
                    cleanupInfoText += $"\n{cleanValue}";
                }
                OnPropertyChanged();
            }
        }
        public double UsedSpaceCDrive { get; set; }
        [ObservableProperty] private string? tempSizeText;
        [RelayCommand] private void UpdateCleanupPageInfo()
        {
            TempSizeText = TempCleanService.NormalizeByteSize(TempCleanService.GetFullTempSize());
            GetDiskCInfo();
        }
        [RelayCommand]
        private async Task CheckWinSxS()
        {
            LoadingStatus = true;
            CleanupInfoText = "Запуск...";
            await WinCmdService.RunInPowerShell("Dism.exe /Online /Cleanup-Image /AnalyzeComponentStore", onLineReceived: line => CleanupInfoText = line);
            LoadingStatus = false;
        }
        [RelayCommand] private async Task CleanupWinSxS()
        {
            LoadingStatus = true;
            CleanupInfoText = String.Empty;
            await WinCmdService.RunInPowerShell("Dism.exe /Online /Cleanup-Image /StartComponentCleanup", onLineReceived: line => CleanupInfoText = line);
            CleanupInfoText = String.Empty;
            await WinCmdService.RunInPowerShell("Dism.exe /Online /Cleanup-Image /StartComponentCleanup /ResetBase", onLineReceived: line => CleanupInfoText = line);
            UpdateCleanupPageInfo();
            LoadingStatus = false;
        }
        [RelayCommand] private async Task CleanupTemp()
        {
            LoadingStatus = true;
            long prevSize = TempCleanService.GetFullTempSize();
            await TempCleanService.CleanAllTemp();
            var byteDiff = prevSize - TempCleanService.GetFullTempSize();
            CleanupInfoText = $"Успешно очищено: {TempCleanService.NormalizeByteSize(byteDiff)}";
            UpdateCleanupPageInfo();
            LoadingStatus = false;
        }
        private void GetDiskCInfo()
        {
            try
            {
                var drive = new DriveInfo(Path.GetPathRoot(Environment.SystemDirectory)!);
                if (drive.IsReady)
                {
                    UsedSpaceCDrive = (1 - (drive.AvailableFreeSpace / (double)drive.TotalSize)) * 100;
                    OnPropertyChanged(nameof(UsedSpaceCDrive));
                }
            }
            catch (Exception e) { Debug.WriteLine(e.Message); }
        }
        #endregion
        #endregion
        #region decor
        [ObservableProperty] private string? tabItemDescription;
        private bool loadingStatus; // decor for lasting operations
        private static int loadingCounter;
        public bool LoadingStatus
        {
            get => loadingStatus;
            set
            {
                if (value)
                {
                    loadingCounter++;
                    SetProperty(ref loadingStatus, true);
                }
                else
                {
                    if (loadingCounter > 0) loadingCounter--;
                    bool newValue = loadingCounter > 0;
                    SetProperty(ref loadingStatus, newValue);
                }
            }
        }

        [RelayCommand] private void TabMouseEnter(object sender)
        {
            if (sender is TabItem tabItem)
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
        [RelayCommand] private void TabMouseLeave() => TabItemDescription = string.Empty;
        #endregion
    }
}
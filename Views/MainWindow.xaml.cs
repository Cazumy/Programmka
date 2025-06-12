using MahApps.Metro.Controls;
using Programmka.Services;
using Programmka.ViewModels;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shell;

namespace Programmka.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
        Loaded += MainWindow_Loaded;

        var chrome = new WindowChrome
        {
            CaptionHeight = 0,
            CornerRadius = new CornerRadius(15),
            GlassFrameThickness = new Thickness(0),
            ResizeBorderThickness = new Thickness(6),
            UseAeroCaptionButtons = false
        };

        WindowChrome.SetWindowChrome(this, chrome);
    }
    private void MainWindow_Loaded(object sender, RoutedEventArgs e) => Closing += MainWindow_Closing;
    private void DragWindow(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ChangedButton == System.Windows.Input.MouseButton.Left && e.OriginalSource is DependencyObject element && !element.IsDescendantOf(ColorPicker))
        {
            this.DragMove();
        }
    }
    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e) => MethodsService.DeleteWallpaperTemp();
    private void CloseWindow(object sender, System.Windows.Input.MouseButtonEventArgs e) => this.Close();
}
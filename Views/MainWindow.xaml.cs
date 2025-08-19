using MahApps.Metro.Controls;
using Programmka.Services;
using Programmka.ViewModels;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Shell;

namespace Programmka.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();

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
    private void DragWindow(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ChangedButton == System.Windows.Input.MouseButton.Left && e.OriginalSource is DependencyObject element && !element.IsDescendantOf(ColorPicker))
        {
            this.DragMove();
        }
    }

    private void CloseWindow(object sender, System.Windows.Input.MouseButtonEventArgs e) => this.Close();
}
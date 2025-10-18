using System.Windows;

namespace Programmka.Views
{
    /// <summary>
    /// Логика взаимодействия для OfficeSelectionWindow.xaml
    /// </summary>
    public partial class OfficeSelectionWindow : Window
    {
        public OfficeSelectionWindow()
        {
            InitializeComponent();
        }
        public bool[] officeSelections = new bool[9];
        public bool IsConfirmed { get; private set; } = false;
        private void CloseClick(object sender, RoutedEventArgs e) => this.Close();
        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void UpdateSelection(int index, object? IsEnabled)
        {
            officeSelections[index] = (bool)IsEnabled!;
        }
        private void AccessSelection(object sender, RoutedEventArgs e)
        {
            UpdateSelection(0, AccessToggle.IsChecked);
        }
        private void OneDriveDesktopSelection(object sender, RoutedEventArgs e)
        {
            UpdateSelection(1, OneDriveDesktopToggle.IsChecked);
        }
        private void OutlookSelection(object sender, RoutedEventArgs e)
        {
            UpdateSelection(2, OutlookToggle.IsChecked);
        }
        private void PublisherSelection(object sender, RoutedEventArgs e)
        {
            UpdateSelection(3, PublisherToggle.IsChecked);
        }
        private void ExcelSelection(object sender, RoutedEventArgs e)
        {
            UpdateSelection(4, ExcelToggle.IsChecked);
        }
        private void SkypeSelection(object sender, RoutedEventArgs e)
        {
            UpdateSelection(5, SkypeToggle.IsChecked);
        }
        private void OneNoteSelection(object sender, RoutedEventArgs e)
        {
            UpdateSelection(6, OneNoteToggle.IsChecked);
        }
        private void PowerPointSelection(object sender, RoutedEventArgs e)
        {
            UpdateSelection(7, PowerPointToggle.IsChecked);
        }
        private void WordSelection(object sender, RoutedEventArgs e)
        {
            UpdateSelection(8, WordToggle.IsChecked);
        }

        private void Confirm(object sender, RoutedEventArgs e)
        {
            IsConfirmed = true;
            this.Close();
        }
    }
}

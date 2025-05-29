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
        private void CloseClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        private void UpdateSelection(int index, bool IsEnabled)
        {
            officeSelections[index] = IsEnabled;
        }
        private void AccessSelection(object sender, RoutedEventArgs e)
        {
            UpdateSelection(0, AccessToggle.IsEnabled);
        }
        private void OneDriveDesktopSelection(object sender, RoutedEventArgs e)
        {
            UpdateSelection(1, OneDriveDesktopToggle.IsEnabled);
        }
        private void OutlookSelection(object sender, RoutedEventArgs e)
        {
            UpdateSelection(2, OutlookToggle.IsEnabled);
        }
        private void PublisherSelection(object sender, RoutedEventArgs e)
        {
            UpdateSelection(3, PublisherToggle.IsEnabled);
        }
        private void ExcelSelection(object sender, RoutedEventArgs e)
        {
            UpdateSelection(4, ExcelToggle.IsEnabled);
        }
        private void SkypeSelection(object sender, RoutedEventArgs e)
        {
            UpdateSelection(5, SkypeToggle.IsEnabled);
        }
        private void OneNoteSelection(object sender, RoutedEventArgs e)
        {
            UpdateSelection(6, OneNoteToggle.IsEnabled);
        }
        private void PowerPointSelection(object sender, RoutedEventArgs e)
        {
            UpdateSelection(7, PowerPointToggle.IsEnabled);
        }
        private void WordSelection(object sender, RoutedEventArgs e)
        {
            UpdateSelection(8, WordToggle.IsEnabled);
        }

        private void Confirm(object sender, RoutedEventArgs e)
        {
            IsConfirmed = true;
            this.Close();
        }
    }
}

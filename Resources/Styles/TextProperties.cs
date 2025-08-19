using System.Windows;

namespace Programmka.Resources.Styles
{
    public class TextProperties : DependencyObject
    {
        public static readonly DependencyProperty StatusTextProperty =
            DependencyProperty.RegisterAttached(
                "StatusText",
                typeof(string),
                typeof(TextProperties),
                new PropertyMetadata(string.Empty)
            );

        public static string GetStatusText(DependencyObject obj) =>
            (string)obj.GetValue(StatusTextProperty);

        public static void SetStatusText(DependencyObject obj, string value) =>
            obj.SetValue(StatusTextProperty, value);
    }
}
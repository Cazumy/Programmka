using System.Windows;
using System.Windows.Input;

namespace Programmka.Resources.Styles
{
    public static class MouseEnterCommandBehavior
    {
        public static readonly DependencyProperty EnterCommandProperty =
        DependencyProperty.RegisterAttached(
            "EnterCommand",
            typeof(ICommand),
            typeof(MouseEnterCommandBehavior),
            new PropertyMetadata(null, OnEnterCommandChanged));

        public static void SetEnterCommand(UIElement element, ICommand value) =>
            element.SetValue(EnterCommandProperty, value);
        public static ICommand GetEnterCommand(UIElement element) =>
            (ICommand)element.GetValue(EnterCommandProperty);

        private static void OnEnterCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
                element.MouseEnter += (s, args) => GetEnterCommand(element)?.Execute(null);
        }
        public static readonly DependencyProperty LeaveCommandProperty =
            DependencyProperty.RegisterAttached(
                "LeaveCommand",
                typeof(ICommand),
                typeof(MouseEnterCommandBehavior),
                new PropertyMetadata(null, OnLeaveCommandChanged));

        public static void SetLeaveCommand(UIElement element, ICommand value) =>
            element.SetValue(LeaveCommandProperty, value);
        public static ICommand GetLeaveCommand(UIElement element) =>
            (ICommand)element.GetValue(LeaveCommandProperty);

        private static void OnLeaveCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
                element.MouseLeave += (s, args) => GetLeaveCommand(element)?.Execute(null);
        }
    }
}
#pragma warning disable CS0067
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Programmka.Middleware
{
    public sealed class CommandMiddleware : ICommand
    {
        public static Action<string>? SetStatus;

        private readonly Func<object?, Task> _execute;
        private readonly Predicate<object?>? _canExecute;

        public static event Action<string>? OnBeforeExecute;
        public static event Action<string>? OnAfterExecute;
        public static event Action<string, Exception>? OnError;
        public event EventHandler? CanExecuteChanged;

        private CommandMiddleware(Func<object?, Task> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public static ICommand Run(Func<Task> action, Func<bool>? canExecute = null, [CallerMemberName] string caller = "")
            => new CommandMiddleware(
                _ => Middleware(action, caller),
                canExecute is null ? null : _ => canExecute());
        public static ICommand Run(Action action, Func<bool>? canExecute = null, [CallerMemberName] string caller = "")
            => new CommandMiddleware(
                _ => Middleware(() => { action(); return Task.CompletedTask; }, caller),
                canExecute is null ? null : _ => canExecute());
        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
        public async void Execute(object? parameter) => await _execute(parameter);
        private static async Task Middleware(Func<Task> action, string caller)
        {
            try
            {
                OnBeforeExecute?.Invoke(caller);
                await action();
                SetStatus?.Invoke("Успешно");
                OnAfterExecute?.Invoke(caller);
            }
            catch (Exception ex)
            {
                SetStatus?.Invoke("Ошибка");
                OnError?.Invoke(caller, ex);
            }
        }
    }
}

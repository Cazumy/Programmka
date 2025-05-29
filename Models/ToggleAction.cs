using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Programmka.Middleware;

namespace Programmka.Models
{
    public partial class ToggleAction : ObservableObject
    {
        public string OnStatus { get; }
        public string OffStatus { get; }
        private readonly Action<bool> _callback;

        public ToggleAction(string onStatus, string offStatus, bool initial, Action<bool> callback)
        {
            OnStatus = onStatus;
            OffStatus = offStatus;
            IsChecked = initial;
            _callback = callback;
            StatusText = initial ? OnStatus : OffStatus;
        }

        [ObservableProperty]
        private bool isChecked;

        [ObservableProperty]
        private string statusText;

        partial void OnIsCheckedChanged(bool value)
        {
            try
            {
                _callback?.Invoke(value);
                StatusText = value ? OnStatus : OffStatus;
                CommandMiddleware.SetStatus?.Invoke("Успешно");
            }
            catch
            {
                CommandMiddleware.SetStatus?.Invoke("Ошибка");
            }
        }
    }

}

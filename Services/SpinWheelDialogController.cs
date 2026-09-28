using System;
using System.Windows;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace WheelSpinner.Services
{
     
    public class SpinWheelDialogController
    {
        private readonly IPlayniteAPI _api;
        private readonly ILogger _logger;
        private readonly SaveStateRepository _saveStates;
        private bool _isSpinWindowOpen;

        public SpinWheelDialogController(IPlayniteAPI api, ILogger logger, SaveStateRepository saveStates)
        {
            _api = api;
            _logger = logger;
            _saveStates = saveStates;
        }

        public void ShowSpinWindow()
        {
            _isSpinWindowOpen = true;

            var window = _api.Dialogs.CreateWindow(new WindowCreationOptions { ShowMinimizeButton = false });
            var content = new SpinWheelWindow(_api, _logger, _saveStates.Load());

            window.Title = "Spin Wheel";
            window.ResizeMode = ResizeMode.CanResize;
            window.SizeToContent = SizeToContent.Manual;
            window.MinHeight = 500;
            window.MinWidth = 700;
            window.Content = content;
            window.Owner = _api.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            window.Closed += OnSpinWindowClosed;
            content.SpinCompleted += game =>
                window.Dispatcher.InvokeAsync(() => ShowWinnerWindow(game, window));

            window.ShowDialog();
        }

        private void OnSpinWindowClosed(object sender, EventArgs e)
        {
            if (sender is Window window && window.Content is SpinWheelWindow content)
            {
                _saveStates.Save(content.GetSaveState());
            }

            _isSpinWindowOpen = false;
        }

        private void ShowWinnerWindow(Game game, Window spinWindow)
        {
            if (!_isSpinWindowOpen)
                return;

            spinWindow.IsEnabled = false;

            var winnerWindow = _api.Dialogs.CreateWindow(new WindowCreationOptions
            {
                ShowMaximizeButton = false,
                ShowMinimizeButton = false
            });
            var content = new WinnerWindow(_api, game, _logger);

            winnerWindow.Title = "Spin Wheel Selected Game: " + game.Name;
            winnerWindow.Content = content;
            winnerWindow.ResizeMode = ResizeMode.NoResize;
            winnerWindow.Width = 600;
            winnerWindow.Height = 800;
            winnerWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            winnerWindow.Owner = _api.Dialogs.GetCurrentAppWindow();

            winnerWindow.Closed += (s, e) => spinWindow.IsEnabled = true;
            content.CloseAll += _ =>
            {
                winnerWindow.Close();
                spinWindow.Close();
            };
            content.ExcludeGame += excluded => (spinWindow.Content as SpinWheelWindow)?.ExcludeGame(excluded.Id);

            winnerWindow.ShowDialog();
        }
    }
}

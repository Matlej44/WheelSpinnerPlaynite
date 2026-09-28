using System;
using System.Windows;
using System.Windows.Controls;
using Playnite.SDK;
using Playnite.SDK.Models;
using WheelSpinner.Services;

namespace WheelSpinner
{
    public partial class WinnerWindow : UserControl
    {
        private readonly IPlayniteAPI _api;
        private readonly Game _winner;

        public event Action<Window> CloseAll;
        public event Action<Game> ExcludeGame;

        public WinnerWindow(IPlayniteAPI api, Game winner, ILogger logger)
        {
            InitializeComponent();

            _api = api;
            _winner = winner;

            GameName.Content = winner.Name;
            CoverArt.Source = new CoverImageLoader(api, logger).LoadOrFallback(winner);
        }

        private void ShowGameAndClose(object sender, RoutedEventArgs e)
        {
            _api.MainView.SelectGame(_winner.Id);
            CloseAll?.Invoke(Window.GetWindow(this));
        }

        private void PlayGameButton(object sender, RoutedEventArgs e)
        {
            CloseAll?.Invoke(Window.GetWindow(this));
            _api.StartGame(_winner.Id);
        }

        private void BackToWheelButton(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }

        private void ExcludeGameButton(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
            ExcludeGame?.Invoke(_winner);
        }
    }
}

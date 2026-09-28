using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using WheelSpinner.Models;

namespace WheelSpinner
{
    public class WheelSpinner : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private WheelSpinnerSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("75428eb9-dec4-4aa7-9f1e-62e0a2dcd044");
        private IPlayniteAPI Api { get; set; }
        

        public WheelSpinner(IPlayniteAPI api) : base(api)
        {
            settings = new WheelSpinnerSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };
            this.Api = api;
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            yield return new MainMenuItem
            {
                Description = "Spin the Wheel",
                Action = (gargs) => CreateWindow()
            };
        }

        private bool IsWindowOpen;
        private void CreateWindow()
        {
            IsWindowOpen = true;
            var window = Api.Dialogs.CreateWindow(new WindowCreationOptions
            {
                ShowMinimizeButton = false
            });
            var saveState = new SaveState();
            try
            {
                saveState = LoadPluginSettings<SaveState>();
            }
            catch (Exception exception)
            {
                logger.Error(exception, "Failed to load settings.");
            }
            window.ResizeMode = ResizeMode.CanResize;
            window.SizeToContent = SizeToContent.Manual;
            window.MinHeight = 500;
            window.MinWidth = 700;
            window.Title = "Spin Wheel";
            var windowContent = new SpinWheelWindow(api: Api,logger, saveState);
            window.Content = windowContent;
            
            window.Owner = Api.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Closed += OnWindowClosed;
            windowContent.SpinCompleted += (game) =>
            {
                window.Dispatcher.InvokeAsync(()=>OnSpinCompleted(game, window));
            };
            
            
            window.ShowDialog();
        }

        private void OnSpinCompleted(Game game, Window window)
        {
            if (!IsWindowOpen)
                return;
            window.IsEnabled = false;
            var winnerWindow = Api.Dialogs.CreateWindow(new WindowCreationOptions
            {
                ShowMaximizeButton = false,
                ShowMinimizeButton = false
            });
            winnerWindow.Title = "Spin Wheel Selected Game: " + game.Name;
            winnerWindow.Closed += (sender, args) =>
            {
                window.IsEnabled = true;
            };
            var winnerContent = new WinnerWindow(api: Api, winner: game, logger: logger);
            
           winnerContent.CloseAll += (winner) =>
           {
               winnerWindow.Close();
               window.Close();
           };

           winnerContent.ExcludeGame += game1 =>
           {
               var windowContent = window.Content as SpinWheelWindow;
               windowContent.ExcludeGame(game1.Id);
           };
            
            winnerWindow.Content = winnerContent;
            
            winnerWindow.ResizeMode = ResizeMode.NoResize;
            winnerWindow.Height = 800;
            winnerWindow.Width = 600;
            winnerWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            winnerWindow.Owner = Api.Dialogs.GetCurrentAppWindow();

            winnerWindow.ShowDialog();
            
            //Api.Dialogs.ShowMessage($"You won {game.Name}!", "Congratulations!");

        }
        public void OnWindowClosed(object sender, EventArgs e)
        {
            
            try
            {
                if (sender is Window window && window.Content is SpinWheelWindow windowContent)
                {
                    var save = windowContent.GetSaveState();
                    if (save != null)
                    {
                        SavePluginSettings(save);
                    }
                }
            }
            catch (Exception exception)
            {
                logger.Error(exception, "Failed to save settings on window close.");
            }
            IsWindowOpen = false;
        }
        
        
        public override void OnGameInstalled(OnGameInstalledEventArgs args)
        {
            // Add code to be executed when game is finished installing.
        }

        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            // Add code to be executed when game is started running.
        }

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            // Add code to be executed when game is preparing to be started.
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            // Add code to be executed when game is preparing to be started.
        }

        public override void OnGameUninstalled(OnGameUninstalledEventArgs args)
        {
            // Add code to be executed when game is uninstalled.
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            // Add code to be executed when Playnite is initialized.
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
            // Add code to be executed when Playnite is shutting down.
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            // Add code to be executed when library is updated.
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new WheelSpinnerSettingsView();
        }
    }
}
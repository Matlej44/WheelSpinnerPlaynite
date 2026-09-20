using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        private void CreateWindow()
        {
            var window = Api.Dialogs.CreateWindow(new WindowCreationOptions
            {
                ShowMinimizeButton = false
            });
            var saveState = LoadPluginSettings<SaveState>();
            window.ResizeMode = ResizeMode.CanResize;
            window.SizeToContent = SizeToContent.Width;
            window.MinHeight = 500;
            window.MinWidth = 700;

            var windowContent = new SpinWheelWindow(api: Api, saveState);
            window.Content = windowContent;
            
            window.Owner = Api.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Closed += OnWindowClosed;
            
            
            window.ShowDialog();
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
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using WheelSpinner.Services;

namespace WheelSpinner
{
    public class WheelSpinner : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private readonly WheelSpinnerSettingsViewModel settings;
        private readonly SpinWheelDialogController dialogs;

        public override Guid Id { get; } = Guid.Parse("75428eb9-dec4-4aa7-9f1e-62e0a2dcd044");

        public WheelSpinner(IPlayniteAPI api) : base(api)
        {
            settings = new WheelSpinnerSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };
            dialogs = new SpinWheelDialogController(api, logger, new SaveStateRepository(this, logger));
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            yield return new MainMenuItem
            {
                Description = "Spin the Wheel",
                Action = _ => dialogs.ShowSpinWindow()
            };
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

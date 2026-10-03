using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WheelSpinner.Models;

namespace WheelSpinner
{
    public class WheelSpinnerSettings : ObservableObject
    {
        private double volume = 0.8;
        private string speed = "Normal";

        public double Volume { get => volume; set => SetValue(ref volume, value); }
        public string Speed { get => speed; set => SetValue(ref speed, value); }
        
        [DontSerialize]
        public List<SpeedModel> ItemList { get; set; } = new List<SpeedModel>
        {
            new SpeedModel("Very Slow", 0.5, 0.8),
            new SpeedModel("Slow", 0.75, 0.9),
            new SpeedModel("Normal", 1, 1),
            new SpeedModel("Fast", 1.5, 1.1),
            new SpeedModel("Very Fast", 2, 1.2),
        };
        [DontSerialize]
        public SpeedModel ActiveItem => ItemList.FirstOrDefault(x => x.Name == Speed) ?? ItemList[2];
    }

    public class WheelSpinnerSettingsViewModel : ObservableObject, ISettings
    {
        private readonly WheelSpinner plugin;
        private WheelSpinnerSettings editingClone { get; set; }

        private WheelSpinnerSettings settings;
        public WheelSpinnerSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public WheelSpinnerSettingsViewModel(WheelSpinner plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<WheelSpinnerSettings>();

            // LoadPluginSettings returns null if no saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new WheelSpinnerSettings();
            }
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            editingClone = Serialization.GetClone(Settings);
        }

        public void CancelEdit()
        {
            // Code executed when user decides to cancel any changes made since BeginEdit was called.
            // This method should revert any changes made to Option1 and Option2.
            Settings = editingClone;
        }

        public void EndEdit()
        {
            // Code executed when user decides to confirm changes made since BeginEdit was called.
            // This method should save settings made to Option1 and Option2.
            plugin.SavePluginSettings(Settings);
        }

        public bool VerifySettings(out List<string> errors)
        {
            // Code execute when user decides to confirm changes made since BeginEdit was called.
            // Executed before EndEdit is called and EndEdit is not called if false is returned.
            // List of errors is presented to user if verification fails.
            errors = new List<string>();
            return true;
        }
    }
}
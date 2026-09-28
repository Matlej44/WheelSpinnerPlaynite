using System;
using Playnite.SDK;
using Playnite.SDK.Plugins;
using WheelSpinner.Models;

namespace WheelSpinner.Services
{
     
    public class SaveStateRepository
    {
        private readonly Plugin _plugin;
        private readonly ILogger _logger;

        public SaveStateRepository(Plugin plugin, ILogger logger)
        {
            _plugin = plugin;
            _logger = logger;
        }

        public SaveState Load()
        {
            try
            {
                return _plugin.LoadPluginSettings<SaveState>();
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Failed to load settings.");
                return new SaveState();
            }
        }

        public void Save(SaveState state)
        {
            if (state == null)
                return;

            try
            {
                _plugin.SavePluginSettings(state);
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Failed to save settings on window close.");
            }
        }
    }
}

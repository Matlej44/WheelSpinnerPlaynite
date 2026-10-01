using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Playnite.SDK;
using Playnite.SDK.Plugins;

namespace WheelSpinner
{
    public partial class WheelSpinnerSettingsView : UserControl
    {
        public WheelSpinnerSettingsView()
        {
            InitializeComponent();
            
        }

        private void ResetToDefault(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            {
                var settingsProperty = DataContext.GetType().GetProperty("Settings");
        
                if (settingsProperty != null)
                {
                    var settingsValue = settingsProperty.GetValue(DataContext, null);
                    
                    if (settingsValue is WheelSpinnerSettings settings)
                    {
                        settings.Volume = 0.8;
                        return; 
                    }
                }
            }
            
            API.Instance.Dialogs.ShowErrorMessage("Nie udało się odnaleźć kontekstu ustawień.");
        }
    }
}
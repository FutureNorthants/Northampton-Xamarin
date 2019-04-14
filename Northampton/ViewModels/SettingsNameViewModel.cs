using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace Northampton
{
    class SettingsNameViewModel : INotifyPropertyChanged
    {
        String settingsName = "";

        public event PropertyChangedEventHandler PropertyChanged;

        public SettingsNameViewModel()
        {
            if (Application.Current.Properties.ContainsKey("SettingsName"))
            {
                settingsName = Application.Current.Properties["SettingsName"] as String;
            }

        }

        public String SettingsName
        {
            set
            {
                if (settingsName != value)
                {
                    settingsName = value;
                    Application.Current.Properties["SettingsName"] = settingsName;
                    Application.Current.SavePropertiesAsync();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SettingsName"));
                }
            }
            get
            {
                return settingsName;
            }
        }
    }
}
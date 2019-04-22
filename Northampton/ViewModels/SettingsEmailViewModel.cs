using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace Northampton
{
    class SettingsEmailViewModel : INotifyPropertyChanged
    {
        String settingsEmail = "";

        public event PropertyChangedEventHandler PropertyChanged;

        public SettingsEmailViewModel()
        {
            if (Application.Current.Properties.ContainsKey("SettingsEmail"))
            {
                settingsEmail = Application.Current.Properties["SettingsEmail"] as String;
            }

        }

        public String SettingsEmail
        {
            set
            {
                if (settingsEmail != value)
                {
                    settingsEmail = value;
                    Application.Current.Properties["SettingsEmail"] = settingsEmail;
                    Application.Current.SavePropertiesAsync();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SettingsEmail"));
                }
            }
            get
            {
                return settingsEmail;
            }
        }
    }
}
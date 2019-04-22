using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace Northampton
{
    public class SettingsPhoneNumberViewModel : INotifyPropertyChanged
    {
        String settingsPhoneNumber = "";

        public event PropertyChangedEventHandler PropertyChanged;

        public SettingsPhoneNumberViewModel()
        {
            if (Application.Current.Properties.ContainsKey("SettingsPhoneNumber"))
            {
                settingsPhoneNumber = Application.Current.Properties["SettingsPhoneNumber"] as String;
            }

        }

        public String SettingsPhoneNumber
        {
            set
            {
                if (settingsPhoneNumber != value)
                {
                    settingsPhoneNumber = value;
                    Application.Current.Properties["SettingsPhoneNumber"] = settingsPhoneNumber;
                    Application.Current.SavePropertiesAsync();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SettingsPhoneNumber"));
                }
            }
            get
            {
                return settingsPhoneNumber;
            }
        }
    }
}
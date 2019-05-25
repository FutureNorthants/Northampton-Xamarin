using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace Northampton
{
    class SettingsPostcodeViewModel : INotifyPropertyChanged
    {
        String settingsPostcode = "";

        public event PropertyChangedEventHandler PropertyChanged;

        public SettingsPostcodeViewModel()
        {
            if (Application.Current.Properties.ContainsKey("SettingsPostcode"))
            {
                settingsPostcode = Application.Current.Properties["SettingsPostcode"] as String;
                Application.Current.Properties["CollectionFinderPostcode"] = settingsPostcode;
                Application.Current.SavePropertiesAsync();
            }

        }

        public String SettingsPostcode
        {
            set
            {
                if (settingsPostcode != value)
                {
                    settingsPostcode = value;
                    Application.Current.Properties["SettingsPostcode"] = settingsPostcode;
                    Application.Current.SavePropertiesAsync();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SettingsPostcode"));
                }
            }
            get
            {
                return settingsPostcode;
            }
        }
    }
}
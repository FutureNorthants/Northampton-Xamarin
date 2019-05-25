using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace Northampton
{
    class CollectionFinderPostcodeViewModel : INotifyPropertyChanged
    {
        String collectionFinderPostcode = "";

        public event PropertyChangedEventHandler PropertyChanged;

        public CollectionFinderPostcodeViewModel()
        {
            if (Application.Current.Properties.ContainsKey("SettingsPostcode"))
            {
                collectionFinderPostcode = Application.Current.Properties["SettingsPostcode"] as String;
                Application.Current.Properties["CollectionFinderPostcode"] = collectionFinderPostcode;
                Application.Current.SavePropertiesAsync();
            }

        }

        public String CollectionFinderPostcode
        {
            set
            {
                if (collectionFinderPostcode != value)
                {
                    collectionFinderPostcode = value;
                    Application.Current.Properties["CollectionFinderPostcode"] = collectionFinderPostcode;
                    Application.Current.SavePropertiesAsync();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CollectionFinderPostcode"));
                }
            }
            get
            {
                return collectionFinderPostcode;
            }
        }
    }
}
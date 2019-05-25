using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Northampton
{
    public partial class SettingsPostcodePage : ContentPage
    {
        public SettingsPostcodePage()
        {
            InitializeComponent();
        }

        void Entry_Completed(object sender, EventArgs e)
        {
            String postCode = ((Entry)sender).Text;
            if (!Validators.IsValidPostcode(postCode))
            {
                DisplayAlert("Invalid Postcode", "Please enter a valid UK postcode.", "OK");
            }
            else
            if (!Validators.IsValidAreaPostcode(postCode))
            {
                DisplayAlert("Postcode not in area", "Please enter a valid area postcode.", "OK");
            }
            else
            {
                String settingsPostcode = Application.Current.Properties["SettingsPostcode"] as String;
                if (settingsPostcode.Length == 6)
                {
                    settingsPostcode = settingsPostcode.Substring(0, 3) + " " + settingsPostcode.Substring(3, 3);
                }
                Application.Current.Properties["SettingsPostcode"] = settingsPostcode;
                Application.Current.SavePropertiesAsync();
                Navigation.PopAsync();
            }
        }
    }
}

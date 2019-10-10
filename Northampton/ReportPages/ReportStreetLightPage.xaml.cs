using System;
using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace Northampton
{
    public partial class ReportStreetLightPage : ContentPage
    {
        int streetLight = 0;

        public ReportStreetLightPage()
        {
            InitializeComponent();
        }

        async void EntryCompleted(object sender, EventArgs e)
        {
            if (Int32.TryParse(((Entry)sender).Text, out streetLight))
            {
                if (Regex.IsMatch(streetLight.ToString(), @"^[0-9]{6}$"))
                {
                    Application.Current.Properties["StreetLightID"] = streetLight.ToString();
                    await Application.Current.SavePropertiesAsync();
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Missing Information", "Please enter the six digit Street Light ID found on the street light", "OK");
                }
            }
            else
            {
                await DisplayAlert("Missing Information", "Please enter the six digit Street Light ID found on the street light", "OK");
            }
        }
    }
}

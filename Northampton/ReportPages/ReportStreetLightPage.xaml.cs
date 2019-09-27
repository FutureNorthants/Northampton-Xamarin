using System;

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
                Application.Current.Properties["StreetLightID"] = streetLight.ToString();
                await Application.Current.SavePropertiesAsync();
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Missing Information", "Please enter the Street Light ID found on the street light", "OK");
            }
        }
    }
}

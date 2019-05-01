using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Northampton
{
    public partial class ReportStreetNamePage : ContentPage
    {
        public ReportStreetNamePage()
        {
            InitializeComponent();
        }

       async void EntryCompleted(object sender, EventArgs e)
        {
            String streetName = ((Entry)sender).Text;
            if(streetName.Equals(""))
            {
                await DisplayAlert("Missing Information", "Please enter a street name", "OK");
            }
            else
            {
                Application.Current.Properties["ProblemStreetSearch"] = streetName;
                await Application.Current.SavePropertiesAsync();
                await Navigation.PushAsync(new WebServiceHandlerPage("ReportMenuByStreet",null));
            }
 

        }
    }
}

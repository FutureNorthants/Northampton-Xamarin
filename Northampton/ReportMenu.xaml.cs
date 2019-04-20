using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Northampton
{
    public partial class ReportMenu : ContentPage
    {
        Label monkeyNameLabel= new Label();
        public ReportMenu()
        {
            InitializeComponent();
        }

        async void AtThisLocationButtonClicked(object sender, EventArgs args)
        {
            Application.Current.Properties["WebServiceHandlerPageTitle"] = "Report a problem";
            Application.Current.Properties["WebServiceHandlerPageDescription"] = "Please wait whilst we find nearby streets...";
            await Application.Current.SavePropertiesAsync();
            await Navigation.PushAsync(new WebServiceHandlerPage("ReportMenu"));
        }

    }
}

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
            await Navigation.PushAsync(new ReportDetailsPage());
        }

    }
}

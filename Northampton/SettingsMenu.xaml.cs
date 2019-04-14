using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Northampton
{
    public partial class SettingsMenu : ContentPage
    {
        public SettingsMenu()
        {
            InitializeComponent();
        }

        async void NameButtonClicked(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new SettingsNamePage());
        }

        async void OnButtonClicked(object sender, EventArgs args)
        {
            await label.RelRotateTo(360, 1000);
        }
    }
}

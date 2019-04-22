using System;
using Xamarin.Forms;

namespace Northampton
{
    public partial class SettingsMenu : ContentPage
    {
        public SettingsMenu()
        {
            InitializeComponent();
        }

        async void OnNameButtonClicked(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new SettingsNamePage(true));
        }

        async void OnPostcodeButtonClicked(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new SettingsPostcodePage());
        }

        async void OnEmailButtonClicked(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new SettingsEmailPage(true));
        }

        async void OnPhoneNumberButtonClicked(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new SettingsPhoneNumberPage(true));
        }
    }
}

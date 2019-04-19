using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Northampton
{
    public partial class ReportDetailsPage : ContentPage
    {
        public ReportDetailsPage()
        {
            InitializeComponent();
            //monkeyNameLabel.SetBinding(Label.TextProperty, new Binding("SelectedItem", source: picker));
        }

        void OnPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            //var picker = (Picker)sender;
            int selectedIndex = picker.SelectedIndex;

            if (selectedIndex != -1)
            {
                //monkeyNameLabel.Text = (string)picker.ItemsSource[selectedIndex];
            }
        }

        async void SubmitButtonClicked(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new SettingsNamePage());
        }

    }
}

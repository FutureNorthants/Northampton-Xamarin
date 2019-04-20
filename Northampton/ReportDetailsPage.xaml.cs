using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;

namespace Northampton
{
    public partial class ReportDetailsPage : ContentPage
    {
        public ReportDetailsPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        void OnPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            //var picker = (Picker)sender;
            int selectedIndex = typePicker.SelectedIndex;

            if (selectedIndex != -1)
            {
                //monkeyNameLabel.Text = (string)picker.ItemsSource[selectedIndex];
            }
        }

        async void SubmitButtonClicked(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new SettingsNamePage());
        }

        public IList<String> streets
        {
            //get
            //{
            //    IList<String> tempStreets = new List<string>();
            //    tempStreets.Add("D");
            //    tempStreets.Add("E");
            //    tempStreets.Add("F");
            //    return tempStreets;
            //}
            get
            {
                String streetsJson = null;
                if (Application.Current.Properties.ContainsKey("WebServiceHandlerPageDescription"))
                {
                    streetsJson = Application.Current.Properties["JsonStreets"] as String;
                }
                IList<String> tempStreets = new List<string>();
                tempStreets.Add("D");
                tempStreets.Add("E");
                tempStreets.Add("F");
                return tempStreets;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            get
            {
                String streetsJson = null;
                if (Application.Current.Properties.ContainsKey("WebServiceHandlerPageDescription"))
                {
                    streetsJson = Application.Current.Properties["JsonStreets"] as String;
                }
                IList<String> tempStreets = new List<string>();
                JObject streetsJSONobject = JObject.Parse(streetsJson);
                JArray resultsArray = (JArray)streetsJSONobject["results"];
                for(int currentResult = 0; currentResult < resultsArray.Count; currentResult++)
                {
                    tempStreets.Add(resultsArray[currentResult][1].ToString());
                }
                return tempStreets;
            }
        }
    }

    public class jsonResults
    {
        public IList<string> Parameters { get; set; }
        public string DateSubmitted { get; set; }
        public IList<Streets> Results { get; set; }
    }

    public class Parameters
    {
        public string Latitude { get; set; }
        public string Longtitude { get; set; }
    }

    public class Streets
    {
        public string USRN { get; set; }
        public string StreetName { get; set; }
    }
}

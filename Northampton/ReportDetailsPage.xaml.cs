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
        private int typePickerIndex = -1;
        IList<Street> storedStreets = new List<Street>();

        public ReportDetailsPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        public int TypePickerIndex
        {
            get
            {
                return typePickerIndex;
            }
            set
            {
                typePickerIndex=value;
            }
        }

        async void SubmitButtonClicked(object sender, EventArgs args)
        {
            await DisplayAlert("Test", "OK", "OK");
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
                    storedStreets.Add(new Street(resultsArray[currentResult][0].ToString(), resultsArray[currentResult][1].ToString()));
                }
                return tempStreets;
            }
        }
    }

    public class Street
    {
        public string USRN { get; set; }
        public string StreetName { get; set; }
        public Street(String usrn,String streetName)
        {
            USRN = usrn;
            StreetName = streetName;
        }

    }
}

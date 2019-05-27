using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace Northampton
{
    public partial class CollectionFinderPropertyPage : ContentPage
    {
        private int propertyPickerIndex = -1;

        public CollectionFinderPropertyPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        public int PropertyPickerIndex
        {
            get
            {
                return propertyPickerIndex;
            }
            set
            {
                propertyPickerIndex = value;
            }
        }

        public IList<String> Properties
        {
            get
            {
                String propertiesJson = null;
                if (Application.Current.Properties.ContainsKey("JsonProperties"))
                {
                    propertiesJson = Application.Current.Properties["JsonProperties"] as String;
                }
                IList<String> tempProperties = new List<string>();
                JObject streetsJSONobject = JObject.Parse(propertiesJson);
                JArray resultsArray = (JArray)streetsJSONobject["properties"];
                for (int currentResult = 0; currentResult < resultsArray.Count; currentResult++)
                {
                    tempProperties.Add(resultsArray[currentResult]["property"].ToString());
                    //if (Application.Current.Properties["ProblemLat"].ToString().Equals(""))
                    //{
                    //    storedStreets.Add(new Street(resultsArray[currentResult][0].ToString(),
                    //                                 resultsArray[currentResult][1].ToString(),
                    //                                 resultsArray[currentResult][2].ToString(),
                    //                                 resultsArray[currentResult][3].ToString()
                    //                                 ));
                    //}
                    //else
                    //{
                    //    storedStreets.Add(new Street(resultsArray[currentResult][0].ToString(),
                    //                                 resultsArray[currentResult][1].ToString(),
                    //                                 "",
                    //                                 ""
                    //                                 ));
                    //}

                }
                if (resultsArray.Count == 1)
                {
                    propertyPickerIndex = 1;
                }
                return tempProperties;
            }
        }
    }
}

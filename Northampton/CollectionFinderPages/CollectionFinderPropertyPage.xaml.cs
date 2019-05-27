using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace Northampton
{
    public partial class CollectionFinderPropertyPage : ContentPage
    {
        private int propertyPickerIndex = -1;
        IList<Property> storedProperties = new List<Property>();

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
                Navigation.PushAsync(new CollectionFinderResultPage(storedProperties[propertyPickerIndex].CollectionDay,storedProperties[propertyPickerIndex].CollectionType));
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
                    storedProperties.Add(new Property(resultsArray[currentResult]["day"].ToString(),
                                                   resultsArray[currentResult]["type"].ToString()
                                                 ));

                }
                if (resultsArray.Count == 1)
                {
                    propertyPickerIndex = 1;
                }
                return tempProperties;
            }
        }
    }

    public class Property
    {
        public String CollectionDay { get; set; }
        public String CollectionType { get; set; }
        public Property(String collectionDay, String collectionType)
        {
            CollectionDay = collectionDay;
            CollectionType = collectionType;
        }
    }
}

using System;
using System.Collections.Generic;
using Microsoft.AppCenter.Analytics;
using Xamarin.Forms;

namespace Northampton
{
    public partial class CollectionFinderResultPage : ContentPage
    {
        String collectionDay = "";
        String collectionType = "";

        public CollectionFinderResultPage(String postCode, String collectionDay, String collectionType)
        {
            InitializeComponent();
            this.collectionDay = collectionDay;
            Boolean collectionFound = true;
            switch (collectionType)
            {
                case "black":
                    this.collectionType = "Black Wheelie Bin";
                    break;
                case "brown":
                    this.collectionType = "Brown Wheelie Bin";
                    break;
                case "bags":
                    this.collectionType = "Green Bags";
                    break;
                default:
                    collectionFound = false;
                    Analytics.TrackEvent("CollectionFinder - Unexpected CollectionType", new Dictionary<string, string>
                    {
                        { "Postcode", postCode },
                        { "CollectionDay", collectionDay },
                        { "CollectionType", collectionType },
                    });
                    DisplayAlert("Error", "Sorry, there has been an enexpected response. This has been automatically reported to our Digital Service, please try again later.", "OK");
                    break;
            }
            if (collectionFound)
            {
                Analytics.TrackEvent("CollectionFinder - Completed", new Dictionary<string, string>
                    {
                        { "Postcode", postCode }
                    });
            }
            BindingContext = this;
        }

        public string CollectionDay
        {
            get
            {
                return collectionDay;
            }
        }

        public string CollectionType
        {
            get
            {
                return collectionType;
            }
        }

        void CheckAnotherCollectionButtonClicked(object sender, EventArgs args)
        {
            if (Navigation.NavigationStack.Count > 0)
            {
                Navigation.PopToRootAsync();
            }
        }
    }
}

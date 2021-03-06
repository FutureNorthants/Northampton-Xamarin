﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Xamarin.Forms;

namespace Northampton
{
    public partial class CollectionFinderPostcodePage : ContentPage
    {
        public CollectionFinderPostcodePage()
        {
            InitializeComponent();
        }

        void Entry_Completed(object sender, EventArgs e)
        {
            String postCode = ((Entry)sender).Text;
            ProcessPostcode(postCode);
        }

        private void GetCollectionInfoButtonClicked(object sender, EventArgs e)
        {
            Task.Delay(500);
            try
            {
                ProcessPostcode(Application.Current.Properties["CollectionFinderPostcode"] as String);
            }
            catch(Exception error)
            {
                Crashes.TrackError(error, new Dictionary<string, string> { });
                DisplayAlert("Memory Error", "Sorry, there has been a system error. This has been reported to our Digital Service, please try again later.", "OK");
            }
            
        }

        private void ProcessPostcode(String postCode)
        {
            Analytics.TrackEvent("CollectionFinder - Started", new Dictionary<string, string>
            {
                { "Postcode", postCode },
            });
            if (!Validators.IsValidPostcode(postCode))
            {
                Analytics.TrackEvent("CollectionFinder - Invalid Postcode", new Dictionary<string, string>
                {
                    { "Postcode", postCode },
                });
                DisplayAlert("Invalid Postcode", "Please enter a valid UK postcode.", "OK");
            }
            else
               if (!Validators.IsValidAreaPostcode(postCode))
            {
                Analytics.TrackEvent("CollectionFinder - Out of Area Postcode", new Dictionary<string, string>
                {
                    { "Postcode", postCode },
                });
                DisplayAlert("Postcode not in area", "Please enter a valid area postcode.", "OK");
            }
            else
            {
                if (postCode.Length == 6)
                {
                    String tempPostcode = postCode;
                    postCode = postCode.Substring(0, 3) + " " + postCode.Substring(3, 3);
                    Analytics.TrackEvent("CollectionFinder - Trimmed Postcode", new Dictionary<string, string>
                    {
                        { "Before Postcode", tempPostcode },
                        { "After Postcode", postCode },
                    });
                }
                Application.Current.Properties["WebServiceHandlerPageTitle"] = "Find your collection day";
                Application.Current.Properties["WebServiceHandlerPageDescription"] = "Please wait whilst we find your collection details";
                Application.Current.SavePropertiesAsync();
                Navigation.PushAsync(new WebServiceHandlerPage("GetCollectionDetails"));
            }
        }
    }
}

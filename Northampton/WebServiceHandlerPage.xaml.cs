﻿using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Plugin.Media.Abstractions;
using Xamarin.Essentials;
using Xamarin.Forms;
using System.Text.Encodings.Web;
using System.Collections.Generic;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System.Net.Http.Headers;

namespace Northampton
{
    public partial class WebServiceHandlerPage : ContentPage
    {

        private String pageTitle = "";
        private String pageDescription = "";

        public WebServiceHandlerPage(String callingPage)
        {
            InitializeComponent();
            if (Application.Current.Properties.ContainsKey("WebServiceHandlerPageTitle"))
            {
                pageTitle = Application.Current.Properties["WebServiceHandlerPageTitle"] as String;
            }
            if (Application.Current.Properties.ContainsKey("WebServiceHandlerPageDescription"))
            {
                pageDescription = Application.Current.Properties["WebServiceHandlerPageDescription"] as String;
            }
            if (callingPage.Equals("SendProblemToCRM"))
            {
                pageDescription = "Please wait whilst we submit your problem";
            }
            BindingContext = this;

            switch (callingPage)
            {
                case "ReportMenuByLocation":
                    GetLocationByGPS();
                    break;
                case "ReportMenuByStreet":
                    if (Application.Current.Properties.ContainsKey("ProblemStreetSearch"))
                    {
                        GetLocationByStreet(Application.Current.Properties["ProblemStreetSearch"] as String);
                    }
                    break;
                case "SendProblemToCRM":
                    SendProblemToCRM();
                    break;
                case "GetCollectionDetails":
                    GetCollectionDetails(Application.Current.Properties["CollectionFinderPostcode"] as String);
                    break;
                default:
                    Analytics.TrackEvent("WebServiceHandler - Unexpected CallingPage", new Dictionary<string, string>
                    {
                        { "CallingPage", callingPage }
                    });
                    break;
            }
        }

        public string PageTitle
        {
            get
            {
                return pageTitle;
            }
        }

        public string PageDescription
        {
            get
            {
                return pageDescription;
            }
        }

        async void GetLocationByGPS()
        {
            Analytics.TrackEvent("ReportIt - GetLocationByGPS Attempted");
            Xamarin.Essentials.Location currentLocation = null;
            Boolean noStreetsFound = false;
            try
            {
                CancellationTokenSource source = new CancellationTokenSource();
                CancellationToken cancelToken = source.Token;
                var locationRequest = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(20));
                currentLocation = await Geolocation.GetLocationAsync(locationRequest, cancelToken);

                if (currentLocation == null)
                {
                    Analytics.TrackEvent("ReportIt - Location Not Found", new Dictionary<string, string>{});
                }
                else
                {
                    Application.Current.Properties["ProblemLat"] = currentLocation.Latitude.ToString();
                    Application.Current.Properties["ProblemLng"] = currentLocation.Longitude.ToString();
                    await Application.Current.SavePropertiesAsync();
                }
                NetworkAccess connectivity = Connectivity.NetworkAccess;

                if (connectivity == NetworkAccess.Internet)
                {
                    WebRequest streetRequest = WebRequest.Create(string.Format(@"https://api.northampton.digital/vcc/getstreetbylatlng?lat={0}&lng={1}", currentLocation.Latitude, currentLocation.Longitude));
                    streetRequest.ContentType = "application/json";
                    streetRequest.Method = "GET";

                    using (HttpWebResponse response = streetRequest.GetResponse() as HttpWebResponse)
                    {
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            Analytics.TrackEvent("ReportIt - Server Error from GetStreetByLatLng", new Dictionary<string, string>
                            {
                               { "Latitude", currentLocation.Latitude.ToString() },
                               { "Longitude", currentLocation.Longitude.ToString() },
                               { "StatusCode", response.StatusCode.ToString() },
                            });
                            await Task.Delay(5000);
                            if (Navigation.NavigationStack.Count > 1)
                            {
                                Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
                            }
                            await DisplayAlert("Error", "Sorry, there has been a system error (" + response.StatusCode + "). This has been reported to our Digital Service, please try again later.", "OK");
                            await Navigation.PopAsync();
                        }

                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            var content = reader.ReadToEnd();
                            if (string.IsNullOrWhiteSpace(content))
                            {
                                Analytics.TrackEvent("ReportIt - Server Response Empty from GetStreetByLatLng", new Dictionary<string, string>
                                {
                                    { "Latitude", currentLocation.Latitude.ToString() },
                                    { "Longitude", currentLocation.Longitude.ToString() },
                                });
                                await DisplayAlert("Error", "Sorry, there has been a system issue. This has been reported to our Digital Service, please try again later.", "OK");
                                await Navigation.PopAsync();
                            }
                            else
                            {
                                Application.Current.Properties["JsonStreets"] = content;
                                await Application.Current.SavePropertiesAsync();
                                JObject streetsJSONobject = JObject.Parse(content);
                                JArray resultsArray = (JArray)streetsJSONobject["results"];
                                if (resultsArray.Count == 0)
                                {
                                    noStreetsFound = true;
                                }
                            }
                        }
                    }
                    if (noStreetsFound)
                    {
                        Analytics.TrackEvent("ReportIt - No Streets Found By GPS", new Dictionary<string, string>
                        {
                            { "Latitude", currentLocation.Latitude.ToString() },
                            { "Longitude", currentLocation.Longitude.ToString() },
                        });
                        await DisplayAlert("Missing Information", "No streets found at this location, please try again", "OK");
                        await Navigation.PopAsync();
                    }
                    else
                    {
                        Analytics.TrackEvent("ReportIt - GetLocationByGPS Successful");
                        await Navigation.PushAsync(new ReportDetailsPage(true));
                        if (Navigation.NavigationStack.Count > 1)
                        {
                            Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
                        }
                    }
                }
                else
                {
                    Analytics.TrackEvent("No Internet", new Dictionary<string, string>
                    {
                        { "Function", "ReportIt" },
                        { "Method", "GetLocationByGPS" },
                        { "Latitude", currentLocation.Latitude.ToString() },
                        { "Longitude", currentLocation.Longitude.ToString() },

                    });
                    await DisplayAlert("No Connectivity", "Your device does not currently have an internet connection, please try again later.", "OK");
                    await Navigation.PopAsync();
                }
            }
            catch (Exception error)
            {
                Crashes.TrackError(error, new Dictionary<string, string>{});
                Application.Current.Properties["ProblemLat"] = "";
                Application.Current.Properties["ProblemLng"] = "";
                Application.Current.Properties["UsedLatLng"] = "false";
                Application.Current.Properties["WebServiceHandlerPageTitle"] = "Report a problem";
                Application.Current.Properties["WebServiceHandlerPageDescription"] = "Please wait whilst we find that street";
                await Application.Current.SavePropertiesAsync();
                await Navigation.PushAsync(new ReportStreetNamePage());
            }
        }

        async void GetLocationByStreet(String streetName)
        {
            Analytics.TrackEvent("ReportIt - GetLocationByStreet Attempted");
            await Task.Delay(1000);
            NetworkAccess connectivity = Connectivity.NetworkAccess;

            if (connectivity == NetworkAccess.Internet)
            {
                Boolean noStreetsFound = false;
                WebRequest streetRequest = WebRequest.Create(string.Format(@"https://api.northampton.digital/vcc/getstreetbyname?StreetName={0}", streetName));
                streetRequest.ContentType = "application/json";
                streetRequest.Method = "GET";

                try
                {
                    using (HttpWebResponse response = streetRequest.GetResponse() as HttpWebResponse)
                    {
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            Analytics.TrackEvent("ReportIt - Server Error from GetStreetByStreetName", new Dictionary<string, string>
                            {
                               { "StreetName", streetName },
                               { "StatusCode", response.StatusCode.ToString() },
                            });
                            await Task.Delay(5000);
                            if (Navigation.NavigationStack.Count > 1)
                            {
                                Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
                            }
                            await DisplayAlert("Error", "Sorry, there has been a system error (" + response.StatusCode + "). This has been reported to our Digital Service, please try again later.", "OK");
                            await Navigation.PopAsync();
                        }
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            var content = reader.ReadToEnd();
                            if (string.IsNullOrWhiteSpace(content))
                            {
                                Analytics.TrackEvent("ReportIt - Server Response Empty from GetStreetByStreetName", new Dictionary<string, string>
                                {
                                    { "StreetName", streetName },
                                });
                                await DisplayAlert("Error", "Sorry, there has been a system issue. This has been reported to our Digital Service, please try again later.", "OK");
                                await Navigation.PopAsync();
                            }
                            else
                            {
                                Application.Current.Properties["JsonStreets"] = content;
                                await Application.Current.SavePropertiesAsync();
                                JObject streetsJSONobject = JObject.Parse(content);
                                JArray resultsArray = (JArray)streetsJSONobject["results"];
                                if (resultsArray.Count == 0)
                                {
                                    noStreetsFound = true;
                                }
                            }
                        }
                    }
                }
                catch (Exception error)
                {
                    Crashes.TrackError(error, new Dictionary<string, string>
                        {
                            { "StreetName", streetName },
                        });
                    await DisplayAlert("Error", "Sorry, there has been a system crash. This has been reported to our Digital Service, please try again later.", "OK");
                    await Navigation.PopAsync();
                }
                if (noStreetsFound)
                {
                    Analytics.TrackEvent("ReportIt - No Streets Found With Name", new Dictionary<string, string>
                        {
                            { "StreetName", streetName },
                        });
                    await DisplayAlert("Missing Information", "No streets found with the name '" + streetName + "', please try again", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    Analytics.TrackEvent("ReportIt - GetLocationByStreet Successful");
                    await Navigation.PushAsync(new ReportDetailsPage(false));
                    if (Navigation.NavigationStack.Count > 1)
                    {
                        Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
                    }
                }
            }
            else
            {
                Analytics.TrackEvent("No Internet", new Dictionary<string, string>
                    {
                        { "Function", "ReportIt" },
                        { "Method", "GetLocationByStreet" },
                        { "StreetName", streetName },
                    });
                await Task.Delay(5000);
                if (Navigation.NavigationStack.Count > 1)
                {
                    Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
                }
                await DisplayAlert("No Connectivity", "Your device does not currently have an internet connection, please try again later.", "OK");
                await Navigation.PopAsync();
            }
        }

        async void SendProblemToCRM()
        {
            Analytics.TrackEvent("ReportIt - SendProblemToCRM Attempted");
            String problemType = "";
            String problemLat = "";
            String problemLng = "";
            String problemEmail = "";
            String problemText = "";
            String streetLightID = "";

            if (Application.Current.Properties.ContainsKey("ProblemType"))
            {
                problemType = Application.Current.Properties["ProblemType"] as String;
            }
            if (Application.Current.Properties.ContainsKey("ProblemLat"))
            {
                problemLat = Application.Current.Properties["ProblemLat"] as String;
            }
            if (Application.Current.Properties.ContainsKey("ProblemLng"))
            {
                problemLng = Application.Current.Properties["ProblemLng"] as String;
            }
            if (Application.Current.Properties.ContainsKey("StreetLightID"))
            {
                streetLightID = Application.Current.Properties["StreetLightID"] as String;
            }
            if (Application.Current.Properties.ContainsKey("ProblemUpdates"))
            {
                String problemUpdates = Application.Current.Properties["ProblemUpdates"] as String;
                switch (problemUpdates)
                {
                    case "email":
                        if (Application.Current.Properties.ContainsKey("SettingsEmail"))
                        {
                            problemEmail = Application.Current.Properties["SettingsEmail"] as String;
                        }
                        break;
                    case "text":
                        if (Application.Current.Properties.ContainsKey("SettingsPhoneNumber"))
                        {
                            problemText = Application.Current.Properties["SettingsPhoneNumber"] as String;
                        }
                        break;
                    case "none":                
                        break;
                    default:
                        Analytics.TrackEvent("ReportIt - Unexpected ProblemUpdates", new Dictionary<string, string>
                            {
                                { "ProblemUpdates", problemUpdates }
                            });
                        break;
                }
            }

            NetworkAccess connectivity = Connectivity.NetworkAccess;

            if (connectivity == NetworkAccess.Internet)
            {
                HttpClient client = new HttpClient();

                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("dataSource", "xamarin");
                client.DefaultRequestHeaders.Add("DeviceID", DeviceInfo.Platform.ToString());
                client.DefaultRequestHeaders.Add("ProblemNumber", problemType);
                client.DefaultRequestHeaders.Add("ProblemLatitude", problemLat);
                client.DefaultRequestHeaders.Add("ProblemLongitude", problemLng);
                client.DefaultRequestHeaders.Add("ProblemDescription", JavaScriptEncoder.Default.Encode(Application.Current.Properties["ProblemDescription"] as String));
                client.DefaultRequestHeaders.Add("ProblemLocation", Application.Current.Properties["ProblemLocation"] as String);
                client.DefaultRequestHeaders.Add("ProblemStreet", Application.Current.Properties["ProblemUSRN"] as String);
                client.DefaultRequestHeaders.Add("ProblemEmail", problemEmail);
                client.DefaultRequestHeaders.Add("ProblemPhone", problemText);
                client.DefaultRequestHeaders.Add("ProblemName", Application.Current.Properties["SettingsName"] as String);
                client.DefaultRequestHeaders.Add("ProblemUsedGPS", Application.Current.Properties["UsedLatLng"] as String);
                client.DefaultRequestHeaders.Add("postref", streetLightID);
                HttpContent content = null;
                if (Application.Current.Properties["ProblemUsedImage"].ToString().Equals("true"))
                {
                    client.DefaultRequestHeaders.Add("includesImage", "true");
                    MediaFile imageData = Application.Current.Properties["ProblemImage"] as MediaFile;
                    content = new StreamContent(imageData.GetStream());
                    content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                }
                else
                {
                    client.DefaultRequestHeaders.Add("includesImage", "false");
                }

                client.BaseAddress = new Uri("https://api.northampton.digital/vcc/mycouncil");

                try
                {
                    HttpResponseMessage response = await client.PostAsync("", content);
                    String jsonResult = await response.Content.ReadAsStringAsync();
                    if (jsonResult.Contains("HTTP Status "))
                    {
                        int errorIndex = jsonResult.IndexOf("HTTP Status ", StringComparison.Ordinal);
                        Analytics.TrackEvent("ReportIt - Unable to Submit", new Dictionary<string, string>
                            {
                                { "HTTP Status Code", jsonResult.Substring(errorIndex + 12, 3) },
                                { "DeviceID", DeviceInfo.Platform.ToString() },
                                { "ProblemNumber", problemType },
                                { "ProblemLatitude", problemLat },
                                { "ProblemLongitude", problemLng },
                                { "ProblemDescription", JavaScriptEncoder.Default.Encode(Application.Current.Properties["ProblemDescription"] as String) },
                                { "ProblemLocation", Application.Current.Properties["ProblemLocation"] as String },
                                { "ProblemStreet", Application.Current.Properties["ProblemUSRN"] as String },
                                { "ProblemEmail", problemEmail },
                                { "ProblemPhone", problemText },
                                { "ProblemName", Application.Current.Properties["SettingsName"] as String },
                                { "ProblemUsedGPS", Application.Current.Properties["UsedLatLng"] as String },
                                { "ProblemUsedImage", Application.Current.Properties["ProblemUsedImage"] as String },
                                { "StreetLightID", streetLightID },
                            });
                        await DisplayAlert("Error", "Sorry, there has been an unexpected response (" + jsonResult.Substring(errorIndex + 12, 3) + "). This has been reported to our Digital Service, please try again later.", "OK");
                        await Navigation.PopAsync();
                    }
                    else
                    {
                        JObject crmJSONobject = JObject.Parse(jsonResult);
                        try
                        {
                            if (((string)crmJSONobject.SelectToken("result")).Equals("success"))
                            {
                                Analytics.TrackEvent("ReportIt - SendProblemToCRM Successful");
                                await Navigation.PushAsync(new ReportResultPage((string)crmJSONobject.SelectToken("callNumber"), (string)crmJSONobject.SelectToken("slaDate")));
                                if (Navigation.NavigationStack.Count > 1)
                                {
                                    Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
                                }
                            }
                        }
                        catch (Exception error)
                        {
                            Crashes.TrackError(error, new Dictionary<string, string>
                            {
                                { "Issue", "ReportIt - Unsuccessful Submit"},
                                { "Message", ((string)crmJSONobject.SelectToken("message"))},
                                { "DeviceID", DeviceInfo.Platform.ToString()},
                                { "ProblemNumber", problemType },
                                { "ProblemLatitude", problemLat },
                                { "ProblemLongitude", problemLng },
                                { "ProblemDescription", JavaScriptEncoder.Default.Encode(Application.Current.Properties["ProblemDescription"] as String) },
                                { "ProblemLocation", Application.Current.Properties["ProblemLocation"] as String },
                                { "ProblemStreet", Application.Current.Properties["ProblemUSRN"] as String },
                                { "ProblemEmail", problemEmail },
                                { "ProblemPhone", problemText },
                                { "ProblemName", Application.Current.Properties["SettingsName"] as String },
                                { "ProblemUsedGPS", Application.Current.Properties["UsedLatLng"] as String },
                                { "ProblemUsedImage", Application.Current.Properties["ProblemUsedImage"] as String },
                                { "StreetLightID", streetLightID },
                            });
                            await DisplayAlert("Error", "Sorry, there has been a system issue. This has been reported to our Digital Service, please try again later.", "OK");
                            await Navigation.PopAsync();
                        }
                    }
                }
                catch (Exception error)
                {
                Crashes.TrackError(error, new Dictionary<string, string>
                {              
                    { "DeviceID", DeviceInfo.Platform.ToString() },
                    { "ProblemNumber", problemType },
                    { "ProblemLatitude", problemLat },
                    { "ProblemLongitude", problemLng },
                    { "ProblemDescription", JavaScriptEncoder.Default.Encode(Application.Current.Properties["ProblemDescription"] as String) },
                    { "ProblemLocation", Application.Current.Properties["ProblemLocation"] as String },
                    { "ProblemStreet", Application.Current.Properties["ProblemUSRN"] as String },
                    { "ProblemEmail", problemEmail },
                    { "ProblemPhone", problemText },
                    { "ProblemName", Application.Current.Properties["SettingsName"] as String },
                    { "ProblemUsedGPS", Application.Current.Properties["UsedLatLng"] as String },
                    { "ProblemUsedImage", Application.Current.Properties["ProblemUsedImage"] as String },
                    { "StreetLightID", streetLightID },
                });
                    await Task.Delay(5000);
                    await DisplayAlert("Error", "Sorry, there has been a system crash. This has been reported to our Digital Service, please try again later.", "OK");
                    await Navigation.PopAsync();
                }
            }
            else
            {
                Analytics.TrackEvent("No internet", new Dictionary<string, string>
                {
                    { "Function", "ReportIt" },
                    { "Method", "SendProblemToCRM" },
                    { "DeviceID", DeviceInfo.Platform.ToString() },
                    { "ProblemNumber", problemType },
                    { "ProblemLatitude", problemLat },
                    { "ProblemLongitude", problemLng },
                    { "ProblemDescription", JavaScriptEncoder.Default.Encode(Application.Current.Properties["ProblemDescription"] as String) },
                    { "ProblemLocation", Application.Current.Properties["ProblemLocation"] as String },
                    { "ProblemStreet", Application.Current.Properties["ProblemUSRN"] as String },
                    { "ProblemEmail", problemEmail },
                    { "ProblemPhone", problemText },
                    { "ProblemName", Application.Current.Properties["SettingsName"] as String },
                    { "ProblemUsedGPS", Application.Current.Properties["UsedLatLng"] as String },
                    { "ProblemUsedImage", Application.Current.Properties["ProblemUsedImage"] as String },
                    { "StreetLightID", streetLightID },
                });
                await Task.Delay(5000);
                await DisplayAlert("No Connectivity", "Your device does not currently have an internet connection, please try again later.", "OK");
                await Navigation.PopAsync();
            }
        }

        private async void GetCollectionDetails(String postCode)
        {
            Analytics.TrackEvent("CollectionFinder - Submission Attempted");
            await Task.Delay(1000);
            NetworkAccess connectivity = Connectivity.NetworkAccess;

            if (connectivity == NetworkAccess.Internet)
            {
                JObject propertiesJSONobject = null;
                Boolean noPostcodeFound = false;
                WebRequest streetRequest = WebRequest.Create(string.Format(@"https://api.northampton.digital/vcc/getbindetails?postcode={0}", postCode));
                streetRequest.ContentType = "application/json";
                streetRequest.Method = "GET";

                try
                {
                    using (HttpWebResponse response = streetRequest.GetResponse() as HttpWebResponse)
                    {
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            Analytics.TrackEvent("CollectionFinder - Server Error", new Dictionary<string, string>
                            {
                               { "Postcode", postCode },
                               { "StatusCode", response.StatusCode.ToString() },
                            });
                            await Task.Delay(5000);
                            if (Navigation.NavigationStack.Count > 1)
                            {
                                Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
                            }
                            await DisplayAlert("Error", "Sorry, there has been a system error (" + response.StatusCode + "). This has been reported to our Digital Service, please try again later.", "OK");
                            await Navigation.PopAsync();
                        }
                        else
                        {
                            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                            {
                                var content = reader.ReadToEnd();
                                if (string.IsNullOrWhiteSpace(content))
                                {
                                    Analytics.TrackEvent("CollectionFinder - Server Response Empty", new Dictionary<string, string>
                                    {
                                      { "Postcode", postCode }
                                    });
                                    await Task.Delay(5000);
                                    if (Navigation.NavigationStack.Count > 1)
                                    {
                                        Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
                                    }
                                    await DisplayAlert("Error", "Sorry, there has been a system issue. This has been reported to our Digital Service, please try again later.", "OK");
                                    await Navigation.PopAsync();
                                }
                                else
                                {                                    
                                    Application.Current.Properties["JsonProperties"] = content;
                                    await Application.Current.SavePropertiesAsync();
                                    propertiesJSONobject = JObject.Parse(content);
                                    String temp = (string)propertiesJSONobject.SelectToken("rounds");
                                    if (!((string)propertiesJSONobject.SelectToken("result")).Equals("success"))
                                    {
                                        noPostcodeFound = true;
                                    }
                                }
                            }
                        }    
                    }
                    if (noPostcodeFound)
                    {
                        Analytics.TrackEvent("CollectionFinder - No details found", new Dictionary<string, string>
                    {
                        { "Postcode", postCode }
                    });
                        await DisplayAlert("Missing Information", "No collection details found for postcode '" + postCode + "', please check postcode and try again", "OK");
                        await Navigation.PopAsync();
                    }
                    else
                    {
                        switch ((string)propertiesJSONobject.SelectToken("rounds"))
                        {
                            case "single":
                                await Navigation.PushAsync(new CollectionFinderResultPage(postCode, (String)propertiesJSONobject.SelectToken("day"), (String)propertiesJSONobject.SelectToken("type")));
                                break;
                            case "multiple":
                                await Navigation.PushAsync(new CollectionFinderPropertyPage(postCode));
                                break;
                            default:
                                Analytics.TrackEvent("CollectionFinder - Unexpected Round", new Dictionary<string, string>
                            {
                                { "Postcode", postCode }
                            });
                                await DisplayAlert("Error", "Sorry, there has been an enexpected response. This has been reported to our Digital Service, please try again later.", "OK");
                                await Navigation.PopAsync();
                                break;
                        }
                        if (Navigation.NavigationStack.Count > 1)
                        {
                            Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
                        }
                    }
                }
                catch (Exception error)
                {
                    Crashes.TrackError(error, new Dictionary<string, string>{
                        { "Postcode", postCode }
                    });
                    await DisplayAlert("Error", "Sorry, there has been a system crash. This has been reported to our Digital Service, please try again later.", "OK");
                    await Navigation.PopAsync();
                }
            }
            else
            {
                Analytics.TrackEvent("No Internet", new Dictionary<string, string>
                    {
                        { "Function", "CollectionFinder" },
                        { "Method", "GetCollectionDetails" },
                        { "Postcode", postCode },
                    });
                await Task.Delay(5000);
                if (Navigation.NavigationStack.Count > 1)
                {
                    Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
                }
                await DisplayAlert("No Connectivity", "Your device does not currently have an internet connection, please try again later.", "OK");
                await Navigation.PopAsync();
            }
        }
    }
}
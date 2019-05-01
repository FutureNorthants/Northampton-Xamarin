using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;
using Plugin.Media.Abstractions;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Northampton
{
    public partial class WebServiceHandlerPage : ContentPage
    {

        private String pageTitle = "";
        private String pageDescription = "";

        public WebServiceHandlerPage(String callingPage, MediaFile imageData)
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
                    SendProblemToCRM(imageData);
                    break;
                default:
                    Console.WriteLine("Error4 - callingPage not found");
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
            Xamarin.Essentials.Location currentLocation = null;
            Boolean noStreetsFound = false;
            try
            {
                var locationRequest = new GeolocationRequest(GeolocationAccuracy.Best);
                currentLocation = await Geolocation.GetLocationAsync(locationRequest);

                if (currentLocation != null)
                {
                    Console.WriteLine($"Latitude: {currentLocation.Latitude}, Longitude: {currentLocation.Longitude}, Altitude: {currentLocation.Altitude}");
                    Application.Current.Properties["ProblemLat"] = currentLocation.Latitude.ToString();
                    Application.Current.Properties["ProblemLng"] = currentLocation.Longitude.ToString();
                    await Application.Current.SavePropertiesAsync();
                }

            }
            catch (FeatureNotSupportedException error)
            {
                Console.WriteLine("Error1 - " + error);
            }
            catch (FeatureNotEnabledException error)
            {
                Console.WriteLine("Error2 - " + error);
            }
            catch (PermissionException error)
            {
                Console.WriteLine("Error3 - " + error);
            }
            catch (Exception error)
            {
                Console.WriteLine("Error4 - " + error);
            }

            WebRequest streetRequest = WebRequest.Create(string.Format(@"https://veolia-test.northampton.digital/api/GetStreetByLatLng?lat={0}&lng={1}", currentLocation.Latitude, currentLocation.Longitude));
            streetRequest.ContentType = "application/json";
            streetRequest.Method = "GET";

            using (HttpWebResponse response = streetRequest.GetResponse() as HttpWebResponse)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                    Console.Out.WriteLine("Error fetching data. Server returned status code: {0}", response.StatusCode);
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    var content = reader.ReadToEnd();
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        Console.Out.WriteLine("Response contained empty body...");
                    }
                    else
                    {
                        Console.Out.WriteLine("Response Body: \r\n {0}", content);
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
                await DisplayAlert("Missing Information", "No streets found at this location, please try again", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                await Navigation.PushAsync(new ReportDetailsPage(true));
                Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
            }
        }

        async void GetLocationByStreet(String streetName)
        {
            Boolean noStreetsFound = false;
            WebRequest streetRequest = WebRequest.Create(string.Format(@"https://veolia-test.northampton.digital/api/GetStreetByName?StreetName={0}", streetName));
            streetRequest.ContentType = "application/json";
            streetRequest.Method = "GET";

            using (HttpWebResponse response = streetRequest.GetResponse() as HttpWebResponse)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                    Console.Out.WriteLine("Error fetching data. Server returned status code: {0}", response.StatusCode);
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    var content = reader.ReadToEnd();
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        Console.Out.WriteLine("Response contained empty body...");
                    }
                    else
                    {
                        Console.Out.WriteLine("Response Body: \r\n {0}", content);
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
                await DisplayAlert("Missing Information", "No streets found with the name '" + streetName + "', please try again", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                await Navigation.PushAsync(new ReportDetailsPage(false));
                if (Navigation.NavigationStack.Count > 1)
                {
                    Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
                }
            }

        }

        async void SendProblemToCRM(MediaFile imageData)
        {
            String problemType = "";
            String problemLat = "";
            String problemLng = "";
            String problemEmail = "";
            String problemText = "";

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
            if (Application.Current.Properties.ContainsKey("ProblemUpdates"))
            {
                String problemUpdates = Application.Current.Properties["ProblemUpdates"] as String;
                switch (problemUpdates)
                {
                    case "email":
                        //Email
                        if (Application.Current.Properties.ContainsKey("SettingsEmail"))
                        {
                            problemEmail = Application.Current.Properties["SettingsEmail"] as String;
                        }
                        break;
                    case "text":
                        //Text
                        if (Application.Current.Properties.ContainsKey("SettingsPhoneNumber"))
                        {
                            problemText = Application.Current.Properties["SettingsPhoneNumber"] as String;
                        }
                        break;
                    case "none":
                        //No Updates;
                        break;
                    default:
                        Console.WriteLine("Updates setting not found");
                        break;
                }
            }

            var client = new HttpClient();
            String usedLatLng = Application.Current.Properties["UsedLatLng"] as String;
            //client.DefaultRequestHeaders.Add("Content-Type", "image/png");
            client.DefaultRequestHeaders.Add("dataSource", "xamarin");
            client.DefaultRequestHeaders.Add("DeviceID", DeviceInfo.Platform.ToString());
            client.DefaultRequestHeaders.Add("ProblemNumber", problemType);
            client.DefaultRequestHeaders.Add("ProblemLatitude", problemLat);
            client.DefaultRequestHeaders.Add("ProblemLongitude", problemLng);
            client.DefaultRequestHeaders.Add("ProblemDescription", Application.Current.Properties["ProblemDescription"] as String);
            client.DefaultRequestHeaders.Add("ProblemLocation", Application.Current.Properties["ProblemLocation"] as String);
            client.DefaultRequestHeaders.Add("ProblemEmail", problemEmail);
            client.DefaultRequestHeaders.Add("ProblemPhone", problemText);
            client.DefaultRequestHeaders.Add("UsedLatLng", Application.Current.Properties["UsedLatLng"] as String);
            if (Application.Current.Properties["ProblemUsedImage"].ToString().Equals("true"))
            {
                client.DefaultRequestHeaders.Add("includesImage", "true");

            }
            else
            {
                client.DefaultRequestHeaders.Add("includesImage", "false");
            }


            client.BaseAddress = new Uri("https://mycouncil-test.northampton.digital");

            String jsonData = "";

            //MediaFile imageData = Application.Current.Properties["ProblemImage"] as MediaFile;

            Stream imageStream = imageData.GetStream();
            var content = new StringContent(imageStream.ToString(), Encoding.UTF8, "image/png");
            HttpResponseMessage response = await client.PostAsync("/CreateCall?", content);

            String jsonResult = await response.Content.ReadAsStringAsync();
            JObject crmJSONobject = JObject.Parse(jsonResult);
            if (((string)crmJSONobject.SelectToken("result")).Equals("success"))
            {
                await Navigation.PushAsync(new ReportResultPage((string)crmJSONobject.SelectToken("callNumber"), (string)crmJSONobject.SelectToken("slaDate")));
                Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
            }
            else
            {
                await DisplayAlert("Error", "No response from server, please try again later", "OK");
                await Navigation.PopAsync();
            }
        }
    }
}

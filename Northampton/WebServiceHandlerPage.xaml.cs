using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
                CancellationTokenSource source = new CancellationTokenSource();
                CancellationToken cancelToken = source.Token;
                var locationRequest = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(20));
                currentLocation = await Geolocation.GetLocationAsync(locationRequest, cancelToken);

                if (currentLocation != null)
                {
                    Console.WriteLine($"Latitude: {currentLocation.Latitude}, Longitude: {currentLocation.Longitude}, Altitude: {currentLocation.Altitude}");
                    Application.Current.Properties["ProblemLat"] = currentLocation.Latitude.ToString();
                    Application.Current.Properties["ProblemLng"] = currentLocation.Longitude.ToString();
                    await Application.Current.SavePropertiesAsync();
                }
                NetworkAccess connectivity = Connectivity.NetworkAccess;

                if (connectivity == NetworkAccess.Internet)
                {
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

                        if (Navigation.NavigationStack.Count > 1)
                        {
                            Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
                        }
                    }
                }
                else
                {
                    await DisplayAlert("No Connectivity", "Your device does not currently have an internet connection, please try again later.", "OK");
                    await Navigation.PopAsync();
                }
            }
            catch (Exception)
            {
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
            await Task.Delay(1000);
            NetworkAccess connectivity = Connectivity.NetworkAccess;

            if (connectivity == NetworkAccess.Internet)
            {
                Boolean noStreetsFound = false;
                WebRequest streetRequest = WebRequest.Create(string.Format(@"https://veolia-test.northampton.digital/api/GetStreetByName?StreetName={0}", streetName));
                streetRequest.ContentType = "application/json";
                streetRequest.Method = "GET";

                try
                {
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
                }
                catch (Exception error)
                {
                    await DisplayAlert("Error", error.ToString(), "OK");
                    await Navigation.PopAsync();
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
            else
            {
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

            NetworkAccess connectivity = Connectivity.NetworkAccess;

            if (connectivity == NetworkAccess.Internet)
            {
                HttpClient client = new HttpClient();

                client.DefaultRequestHeaders.Add("dataSource", "xamarin");
                client.DefaultRequestHeaders.Add("DeviceID", DeviceInfo.Platform.ToString());
                client.DefaultRequestHeaders.Add("ProblemNumber", problemType);
                client.DefaultRequestHeaders.Add("ProblemLatitude", problemLat);
                client.DefaultRequestHeaders.Add("ProblemLongitude", problemLng);
                client.DefaultRequestHeaders.Add("ProblemDescription", WebUtility.UrlEncode(Application.Current.Properties["ProblemDescription"] as String));
                client.DefaultRequestHeaders.Add("ProblemLocation", Application.Current.Properties["ProblemLocation"] as String);
                client.DefaultRequestHeaders.Add("ProblemStreet", Application.Current.Properties["ProblemUSRN"] as String);
                client.DefaultRequestHeaders.Add("ProblemEmail", problemEmail);
                client.DefaultRequestHeaders.Add("ProblemPhone", problemText);
                client.DefaultRequestHeaders.Add("ProblemName", Application.Current.Properties["SettingsName"] as String);
                client.DefaultRequestHeaders.Add("ProblemUsedGPS", Application.Current.Properties["UsedLatLng"] as String);
                HttpContent content = null;
                if (Application.Current.Properties["ProblemUsedImage"].ToString().Equals("true"))
                {
                    client.DefaultRequestHeaders.Add("includesImage", "true");
                    MediaFile imageData = Application.Current.Properties["ProblemImage"] as MediaFile;
                    Stream imageStream = imageData.GetStream();
                    var bytes = new byte[imageStream.Length];
                    await imageStream.ReadAsync(bytes, 0, (int)imageStream.Length);
                    string imageBase64 = Convert.ToBase64String(bytes);
                    content = new StreamContent(imageData.GetStream());
                }
                else
                {
                    client.DefaultRequestHeaders.Add("includesImage", "false");
                }

                client.BaseAddress = new Uri("https://mycouncil-test.northampton.digital");

                try
                {
                    HttpResponseMessage response = await client.PostAsync("/CreateCall?", content);
                    String jsonResult = await response.Content.ReadAsStringAsync();
                    if (jsonResult.Contains("HTTP Status "))
                    {
                        int errorIndex = jsonResult.IndexOf("HTTP Status ", StringComparison.Ordinal);
                        await DisplayAlert("Error", "Error " + jsonResult.Substring(errorIndex + 12, 3) + " from server, please try again later", "OK");
                        await Navigation.PopAsync();
                    }
                    else
                    {
                        JObject crmJSONobject = JObject.Parse(jsonResult);
                        if (((string)crmJSONobject.SelectToken("result")).Equals("success"))
                        {
                            await Navigation.PushAsync(new ReportResultPage((string)crmJSONobject.SelectToken("callNumber"), (string)crmJSONobject.SelectToken("slaDate")));
                            if (Navigation.NavigationStack.Count > 1)
                            {
                                Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
                            }
                        }
                        else
                        {
                            await DisplayAlert("Error", "No response from server, please try again later", "OK");
                            await Navigation.PopAsync();
                        }
                    }
                }
                catch (Exception)
                {
                    await Task.Delay(5000);
                    await DisplayAlert("No Connectivity", "Your device does not currently have an internet connection, please try again later.", "OK");
                    await Navigation.PopAsync();
                }
            }
            else
            {
                await Task.Delay(5000);
                await DisplayAlert("No Connectivity", "Your device does not currently have an internet connection, please try again later.", "OK");
                await Navigation.PopAsync();
            }
        }

        async void GetCollectionDetails(String postCode)
        {
            await Task.Delay(1000);
            NetworkAccess connectivity = Connectivity.NetworkAccess;

            if (connectivity == NetworkAccess.Internet)
            {
                JObject propertiesJSONobject = null;
                Boolean noPostcodeFound = false;
                WebRequest streetRequest = WebRequest.Create(string.Format(@"https://mycouncil.northampton.digital/BinRoundFinder?postcode={0}", postCode));
                streetRequest.ContentType = "application/json";
                streetRequest.Method = "GET";

                try
                {
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
                catch (Exception error)
                {
                    await DisplayAlert("Error", error.ToString(), "OK");
                    await Navigation.PopAsync();
                }
                if (noPostcodeFound)
                {
                    await DisplayAlert("Missing Information", "No collection details found for postcode '" + postCode + "', please check postcode and try again", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    switch ((string)propertiesJSONobject.SelectToken("rounds"))
                    {
                        case "single":
                            await Navigation.PushAsync(new CollectionFinderResultPage((String)propertiesJSONobject.SelectToken("day"), (String)propertiesJSONobject.SelectToken("type")));                        
                            break;
                        case "multiple":
                            //await Navigation.PushAsync(new ReportDetailsPage(false));                           
                            break;
                        default:
                            await DisplayAlert("Error", "Unable to find collection information", "OK");
                            await Navigation.PopAsync();
                            break;
                    }
                    if (Navigation.NavigationStack.Count > 1)
                    {
                        Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
                    }
                }
            }
            else
            {
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

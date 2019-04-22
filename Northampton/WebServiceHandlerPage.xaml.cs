using System;
using System.IO;
using System.Net;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Northampton
{
    public partial class WebServiceHandlerPage : ContentPage
    {

        private string pageTitle = "";
        private string pageDescription = "";

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
            Location currentLocation = null;
            try
            {
                var locationRequest = new GeolocationRequest(GeolocationAccuracy.Best);
                currentLocation = await Geolocation.GetLocationAsync(locationRequest);

                if (currentLocation != null)
                {
                    Console.WriteLine($"Latitude: {currentLocation.Latitude}, Longitude: {currentLocation.Longitude}, Altitude: {currentLocation.Altitude}");
                    Application.Current.Properties["Latitude"] = currentLocation.Latitude;
                    Application.Current.Properties["Longtitude"] = currentLocation.Longitude;
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
                    }
                }
            }
            await Navigation.PushAsync(new ReportDetailsPage());
            Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
        }

        async void GetLocationByStreet(String streetName)
        {
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
                    }
                }
            }
            await Navigation.PushAsync(new ReportDetailsPage());
            Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
        }
    }
}

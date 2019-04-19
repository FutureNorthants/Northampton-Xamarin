using System;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Northampton;

namespace Northampton
{
    public partial class ReportCurrentLocationPage : ContentPage
    {
        public ReportCurrentLocationPage()
        {
            InitializeComponent();
            //BindingContext = new ReportCurrentLocationViewModel();
            //Task<Location> locationTask = GetCurrentLocation();
            GetCurrentLocation();
        }

        //async Task<Location> GetCurrentLocation()
        async void GetCurrentLocation()
        {
            Console.WriteLine("Checking for location...");
            Location currentLocation=null;
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
            Console.WriteLine("returning");


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
                        Application.Current.Properties["JsonStreet"] = content;
                        await Application.Current.SavePropertiesAsync();
                    }
                }
            }

            //return currentLocation;
        }

        void OnPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            //var picker = (Picker)sender;
            int selectedIndex = picker.SelectedIndex;

            if (selectedIndex != -1)
            {
                //monkeyNameLabel.Text = (string)picker.ItemsSource[selectedIndex];
            }
        }

        async void SubmitButtonClicked(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new ReportDetailsPage());
        }
    }
}

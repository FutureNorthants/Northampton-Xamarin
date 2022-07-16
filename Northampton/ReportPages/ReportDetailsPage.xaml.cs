﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json.Linq;
using Plugin.Media.Abstractions;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Northampton
{
    public partial class ReportDetailsPage : ContentPage
    {
        private int streetPickerIndex = -1;
        private int typePickerIndex = -1;
        private int updatesPickerIndex = -1;
        private String streetLightID = null;
        private String problemDescription = "";
        private readonly Boolean isMapVisible = false;
        private Boolean includesImage = false;
        private MediaFile imageData = null;
        readonly IList<Street> storedStreets = new List<Street>();
        readonly IList<Problem> storedProblems = new List<Problem>();
        readonly String problemLat = "";
        readonly String problemLng = "";
        private Boolean useGPS = false;

        public ReportDetailsPage()
        {
            InitializeComponent();
            Application.Current.Properties["ProblemDescription"] = "";
        }

        public ReportDetailsPage(Boolean usingGPS)
        {
            loadingStack.IsVisible = false;
            //loadingLabel.isVisible = false;
            //loadingIndicator.isVisible = false;
            IsBusy = false;
            if (usingGPS)
            {
                isMapVisible = true;
                if (Application.Current.Properties.ContainsKey("ProblemLat"))
                {
                    problemLat = Application.Current.Properties["ProblemLat"] as String;
                }
                if (Application.Current.Properties.ContainsKey("ProblemLng"))
                {
                    problemLng = Application.Current.Properties["ProblemLng"] as String;
                }
            }
            else
            {
                isMapVisible = false;
            }
            InitializeComponent();
            TakePhotoButton.IsVisible = true;
            UsePhotoButton.IsVisible = true;
            PhotoImage.IsVisible = false;
            if (usingGPS)
            {
                var position = new Position(double.Parse(problemLat), double.Parse(problemLng));
                ProblemMap.MoveToRegion(new MapSpan(position, 0.001, 0.001));
            }
            storedProblems.Add(new Problem(0, "Litter", "Litter"));
            storedProblems.Add(new Problem(1, "Flytip", "Flytipping"));
            storedProblems.Add(new Problem(2, "bodily_fluids", "Bodily Fluids"));
            storedProblems.Add(new Problem(3, "broken_glass", "Broken Glass"));
            storedProblems.Add(new Problem(19, "broken_street_lighting", "Broken Street Lighting"));
            storedProblems.Add(new Problem(4, "dead_animal", "Dead Animal"));
            storedProblems.Add(new Problem(5, "dog_fouling", "Dog Fouling"));
            storedProblems.Add(new Problem(6, "drug_paraphernalia", "Drug Paraphernalia"));
            storedProblems.Add(new Problem(7, "non_offensive_flyposting", "Flyposting - Non Offensive"));
            storedProblems.Add(new Problem(8, "offensive_flyposting", "Flyposting - Offensive"));
            storedProblems.Add(new Problem(9, "non_offensive_graffiti", "Graffiti - Non Offensive"));
            storedProblems.Add(new Problem(10, "offensive_graffiti", "Graffiti - Offensive"));
            storedProblems.Add(new Problem(11, "gum_removal", "Gum Removal"));
            storedProblems.Add(new Problem(12, "leaf_clearance", "Leaf Clearance"));
            storedProblems.Add(new Problem(13, "overflowing_litter_bin", "Litter Bin Overflowing"));
            storedProblems.Add(new Problem(14, "street_sign_clean", "Street Sign Cleaning"));
            storedProblems.Add(new Problem(15, "street_washing", "Street Washing"));
            storedProblems.Add(new Problem(16, "sweeper_bags_not_collected", "Sweeper Bags Not Collected"));
            storedProblems.Add(new Problem(17, "sweeping_required", "Sweeping Required"));
            storedProblems.Add(new Problem(18, "weed_clearance", "Weed Clearance"));

            typePicker.SelectedIndexChanged += (sender, args) =>
            {
                if (typePicker.SelectedIndex == 4)
                {
                    Navigation.PushAsync(new ReportStreetLightPage());
                }
            };
            BindingContext = this;
        }

        public int StreetPickerIndex
        {
            get
            {
                return streetPickerIndex;
            }
            set
            {
                streetPickerIndex = value;
                ScrollView.ScrollToAsync(typePicker, ScrollToPosition.MakeVisible, true);
            }
        }

        public int TypePickerIndex
        {
            get
            {
                return typePickerIndex;
            }
            set
            {
                typePickerIndex = value;
                ScrollView.ScrollToAsync(updatesPicker, ScrollToPosition.MakeVisible, true);
                if (value != 4)
                {
                    streetLightButton.IsVisible = false;
                    streetLightLabel.IsVisible = false;
                    Application.Current.Properties["StreetLightID"] = null;
                    Application.Current.SavePropertiesAsync();
                }              
            }
        }

        void ToggledGPS(object sender, ToggledEventArgs e)
        {
            useGPS = !useGPS;
            if (useGPS)
            {
                Analytics.TrackEvent("ReportIt - AtThisLocation Started");
                Application.Current.Properties["UsedLatLng"] = "true";
                Application.Current.Properties["WebServiceHandlerPageTitle"] = "Report a problem";
                Application.Current.Properties["WebServiceHandlerPageDescription"] = "Please wait whilst we find nearby streets";
                Application.Current.SavePropertiesAsync();
                //Navigation.PushAsync(new WebServiceHandlerPage("ReportMenuByLocation"));
                GetLocationByGPS();
            }
            else
            {
                DisplayAlert("Debug", "Using Search", "OK");
            }
        }

        async void EntryCompleted(object sender, EventArgs e)
        {
            String streetName = ((Entry)sender).Text;
            if (streetName.Equals(""))
            {
                await DisplayAlert("Missing Information", "Please enter a street name", "OK");
            }
            else
            {
                Application.Current.Properties["ProblemStreetSearch"] = streetName;
                await Application.Current.SavePropertiesAsync();
                await Navigation.PushAsync(new WebServiceHandlerPage("ReportMenuByStreet"));
            }
        }

        void EditorCompleted(object sender, EventArgs e)
        {
            problemDescription = ((Editor)sender).Text;
            Application.Current.Properties["ProblemDescription"] = ((Editor)sender).Text;
        }

        public int UpdatesPickerIndex
        {
            get
            {
                if (Application.Current.Properties.ContainsKey("SettingsPreferredUpdateChannel"))
                {
                    string paramValue = Application.Current.Properties["SettingsPreferredUpdateChannel"] as String;
                    Int32.TryParse(paramValue, out updatesPickerIndex);
                }
                return updatesPickerIndex;
            }
            set
            {
                updatesPickerIndex = value;
                ScrollView.ScrollToAsync(submitButton, ScrollToPosition.MakeVisible, true);
            }
        }

        private async void StreetLightButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ReportStreetLightPage());
        }

        private async void TakePhotoButtonClicked(object sender, EventArgs e)
        {
            try
            {
                MediaFile photo = await Plugin.Media.CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    PhotoSize = PhotoSize.Medium
                });

                if (photo != null)
                {
                    imageData = photo;
                    includesImage = true;
                    TakePhotoButton.IsVisible = false;
                    UsePhotoButton.IsVisible = false;
                    PhotoImage.Source = ImageSource.FromStream(() => { return photo.GetStream(); });
                    PhotoImage.IsVisible = true;
                    await Task.Delay(500);
                    await ScrollView.ScrollToAsync(submitButton, ScrollToPosition.MakeVisible, true);
                }
            }
            catch(MediaPermissionException error)
            {
                Crashes.TrackError(error, new Dictionary<string, string> { });
                await DisplayAlert("No permissions", "Sorry, we need permission to access your camera.", "OK");
            }
        }

        private async void UsePhotoButtonClicked(object sender, EventArgs e)
        {
            try
            {
                MediaFile photo = await Plugin.Media.CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
                {
                    PhotoSize = PhotoSize.Medium
                });

                if (photo != null)
                {
                    imageData = photo;
                    includesImage = true;
                    TakePhotoButton.IsVisible = false;
                    UsePhotoButton.IsVisible = false;
                    PhotoImage.Source = ImageSource.FromStream(() => { return photo.GetStream(); });
                    PhotoImage.IsVisible = true;
                    await Task.Delay(500);
                    await ScrollView.ScrollToAsync(submitButton, ScrollToPosition.MakeVisible, true);
                }
            }
            catch (MediaPermissionException error)
            {
                Crashes.TrackError(error, new Dictionary<string, string> { });
                await DisplayAlert("No permissions", "Sorry, we need permission to access your photo library.", "OK");
            }
        }

        async void SubmitButtonClicked(object sender, EventArgs args)
        {
            if (streetPickerIndex == -1)
            {
                await DisplayAlert("Missing Information", "Please confirm the street nearest the problem", "OK");
            }
            else if (typePickerIndex == -1)
            {
                await DisplayAlert("Missing Information", "Please confirm the type of problem", "OK");
            }
            else if (problemDescription.Length == 0)
            {
                await DisplayAlert("Missing Information", "Please provide as many details as possible, it helps us to fix the problem first time", "OK");
            }
            else if (updatesPickerIndex == -1)
            {
                await DisplayAlert("Missing Information", "Please confirm if you would like updates on resolving this problem", "OK");
            }
            else
            {
                Application.Current.Properties["ProblemType"] = storedProblems[typePickerIndex].ProblemName;
                Analytics.TrackEvent("ReportIt - " + storedProblems[typePickerIndex].ProblemDescription);
                switch (updatesPickerIndex)
                {
                    case 0:
                        Application.Current.Properties["ProblemUpdates"] = "email";
                        break;
                    case 1:
                        Application.Current.Properties["ProblemUpdates"] = "text";
                        break;
                    case 2:
                        Application.Current.Properties["ProblemUpdates"] = "none";
                        break;
                    default:
                        Analytics.TrackEvent("ReportIt - Unexpected ProblemUpdates", new Dictionary<string, string>
                            {
                                { "ProblemUpdates", updatesPicker.ToString() }
                            });
                        Application.Current.Properties["ProblemUpdates"] = "none";
                        break;
                }
                Application.Current.Properties["ProblemLocation"] = storedStreets[streetPickerIndex].StreetName;
                Application.Current.Properties["ProblemUSRN"] = storedStreets[streetPickerIndex].USRN;
                if (Application.Current.Properties["ProblemLat"].ToString().Equals(""))
                {
                    Analytics.TrackEvent("ReportIt - Used GPS");
                    Application.Current.Properties["ProblemLat"] = storedStreets[streetPickerIndex].Latitude;
                    Application.Current.Properties["ProblemLng"] = storedStreets[streetPickerIndex].Longtitude;
                }
                string tempIncludesImage;
                if (includesImage)
                {
                    Analytics.TrackEvent("ReportIt - Used Photo");
                    tempIncludesImage = "true";
                    Application.Current.Properties["ProblemImage"] = imageData;
                }
                else 
                {
                    tempIncludesImage = "false"; 
                }
                Application.Current.Properties["ProblemUsedImage"] = tempIncludesImage;

                await Application.Current.SavePropertiesAsync();
                switch (updatesPickerIndex)
                {
                    case 0:
                        Analytics.TrackEvent("ReportIt - Updates via Email");
                        if (Application.Current.Properties.ContainsKey("SettingsEmail"))
                        {
                            if (Application.Current.Properties.ContainsKey("SettingsName"))
                            {
                                await Navigation.PushAsync(new WebServiceHandlerPage("SendProblemToCRM"));
                            }
                            else
                            {
                                await Navigation.PushAsync(new SettingsNamePage(false));
                            }
                        }
                        else
                        {
                            await Navigation.PushAsync(new SettingsEmailPage(false));
                        }
                        break;
                    case 1:
                        Analytics.TrackEvent("ReportIt - Updates via Text");
                        if (Application.Current.Properties.ContainsKey("SettingsPhoneNumber"))
                        {
                            if (Application.Current.Properties.ContainsKey("SettingsName"))
                            {
                                await Navigation.PushAsync(new WebServiceHandlerPage("SendProblemToCRM"));
                            }
                            else
                            {
                                await Navigation.PushAsync(new SettingsNamePage(false));
                            }
                        }
                        else
                        {
                            await Navigation.PushAsync(new SettingsPhoneNumberPage(false));
                        }
                        break;
                    default:
                        Analytics.TrackEvent("ReportIt - Updates Not Requested");
                        if (Application.Current.Properties.ContainsKey("SettingsName"))
                        {
                            await Navigation.PushAsync(new WebServiceHandlerPage("SendProblemToCRM"));
                        }
                        else
                        {
                            await Navigation.PushAsync(new SettingsNamePage(false));
                        }
                        break;
                }
            }
        }

        public IList<String> Streets
        {
            get
            {
                String streetsJson = null;
                if (Application.Current.Properties.ContainsKey("JsonStreets"))
                {
                    streetsJson = Application.Current.Properties["JsonStreets"] as String;
                }
                IList<String> tempStreets = new List<string>();
                JObject streetsJSONobject = JObject.Parse(streetsJson);
                JArray resultsArray = (JArray)streetsJSONobject["results"];
                for (int currentResult = 0; currentResult < resultsArray.Count; currentResult++)
                {
                    tempStreets.Add(resultsArray[currentResult][1].ToString());
                    if (Application.Current.Properties["ProblemLat"].ToString().Equals(""))
                    {
                        storedStreets.Add(new Street(resultsArray[currentResult][0].ToString(),
                                                     resultsArray[currentResult][1].ToString(),
                                                     resultsArray[currentResult][2].ToString(),
                                                     resultsArray[currentResult][3].ToString()
                                                     ));
                    }
                    else
                    {
                        storedStreets.Add(new Street(resultsArray[currentResult][0].ToString(),
                                                     resultsArray[currentResult][1].ToString(),
                                                     "",
                                                     ""
                                                     ));
                    }

                }
                if (resultsArray.Count == 1)
                {
                    streetPickerIndex = 1;
                }
                return tempStreets;
            }
        }

        public IList<String> Problems
        {
            get
            {
                IList<String> tempProblems = new List<string>();
                for (int currentProblem = 0; currentProblem < storedProblems.Count; currentProblem++)
                {
                    tempProblems.Add(storedProblems[currentProblem].ProblemDescription);
                }
                return tempProblems;
            }
        }

        public Boolean IsMapVisible
        {
            get
            {
                return isMapVisible;
            }
        }

        protected override void OnAppearing()
        {
            DisplayAlert("Debug", "Appeared", "OK");
            if (typePicker.SelectedIndex == 4)
            {
                if (Application.Current.Properties.ContainsKey("StreetLightID"))
                {
                    streetLightID = Application.Current.Properties["StreetLightID"] as String;
                }
                if (streetLightID is null || streetLightID.Equals(""))
                {
                    streetLightButton.IsVisible = true;
                    streetLightLabel.IsVisible = false;
                }
                else
                {
                    streetLightID = "Street Light Number " + streetLightID;
                    streetLightLabel.Text = streetLightID;
                    streetLightButton.IsVisible = false;
                    streetLightLabel.IsVisible = true;
                }
            }
            else
            {
                streetLightButton.IsVisible = false;
                streetLightLabel.IsVisible = false;
                Application.Current.Properties["StreetLightID"] = null;
                Application.Current.SavePropertiesAsync();
            }
        }

        async void GetLocationByGPS()
        {
            Analytics.TrackEvent("ReportIt - GetLocationByGPS Attempted");
            IsBusy = true;
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
                    Analytics.TrackEvent("ReportIt - Location Not Found", new Dictionary<string, string> { });
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
                        IsBusy = false;
                        //await Navigation.PushAsync(new ReportDetailsPage(true));
                        if (Navigation.NavigationStack.Count > 1)
                        {
                            Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 1]);
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
                Crashes.TrackError(error, new Dictionary<string, string> { });
                Application.Current.Properties["ProblemLat"] = "";
                Application.Current.Properties["ProblemLng"] = "";
                Application.Current.Properties["UsedLatLng"] = "false";
                Application.Current.Properties["WebServiceHandlerPageTitle"] = "Report a problem";
                Application.Current.Properties["WebServiceHandlerPageDescription"] = "Please wait whilst we find that street";
                await Application.Current.SavePropertiesAsync();
                await Navigation.PushAsync(new ReportStreetNamePage());
            }
        }
    }

    public class Street
    {
        public String USRN { get; set; }
        public String StreetName { get; set; }
        public String Latitude { get; set; }
        public String Longtitude { get; set; }
        public Street(String usrn, String streetName, String latitude, String longtitude)
        {
            USRN = usrn;
            StreetName = streetName;
            Latitude = latitude;
            Longtitude = longtitude;
        }

    }

    public class Problem
    {
        public int ProblemNo { get; set; }
        public String ProblemName { get; set; }
        public String ProblemDescription { get; set; }
        public Problem(int ProblemNo, String ProblemName, String ProblemDescription)
        {
            this.ProblemNo = ProblemNo;
            this.ProblemName = ProblemName;
            this.ProblemDescription = ProblemDescription;
        }

    }
}

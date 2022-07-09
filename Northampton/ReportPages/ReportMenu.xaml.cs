using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using ExifLib;
using Microsoft.AppCenter.Analytics;
using Xamarin.Forms;

namespace Northampton
{
    public partial class ReportMenu : ContentPage
    {
        public ReportMenu()
        {
            InitializeComponent();
            Application.Current.Properties["ProblemDescription"] = "";
        }

        async void AtThisLocationButtonClicked(object sender, EventArgs args)
        {
            Analytics.TrackEvent("ReportIt - AtThisLocation Started");
            Application.Current.Properties["UsedLatLng"] = "true";
            Application.Current.Properties["WebServiceHandlerPageTitle"] = "Report a problem";
            Application.Current.Properties["WebServiceHandlerPageDescription"] = "Please wait whilst we find nearby streets";
            await Application.Current.SavePropertiesAsync();
            await Navigation.PushAsync(new WebServiceHandlerPage("ReportMenuByLocation"));
        }

        async void ByStreetNameButtonClicked(object sender, EventArgs args)
        {
            Analytics.TrackEvent("ReportIt - ByStreetName Started");
            Application.Current.Properties["ProblemLat"] = "";
            Application.Current.Properties["ProblemLng"] = "";
            Application.Current.Properties["UsedLatLng"] = "false";
            Application.Current.Properties["WebServiceHandlerPageTitle"] = "Report a problem";
            Application.Current.Properties["WebServiceHandlerPageDescription"] = "Please wait whilst we find that street";
            await Application.Current.SavePropertiesAsync();
            await Navigation.PushAsync(new ReportStreetNamePage());
        }

        async void UsingPhotoButtonClicked(object sender, EventArgs args)
        {
            Analytics.TrackEvent("ReportIt - ByPhoto Started");
            await Application.Current.SavePropertiesAsync();
            (sender as Button).IsEnabled = false;

            Stream stream = await DependencyService.Get<IPhotoPickerService>().GetImageStreamAsync();

            if (stream != null)
            {
                var picInfo = ExifReader.ReadJpeg(stream);
                double[] lat = picInfo.GpsLatitude;
                double[] lon = picInfo.GpsLongitude;
                ImageSource photoImage = ImageSource.FromStream(() => stream);
                //IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(stream);
                //foreach (var directory in directories)
                //    foreach (var tag in directory.Tags)
                //        Console.WriteLine($"{directory.Name} - {tag.Name} = {tag.Description}");

               

            }
           
            await DisplayAlert("Alert", "You have been alerted", "OK");
            (sender as Button).IsEnabled = true;
        }

        async void btnPopupButton_Clicked(object sender, EventArgs args)
        {
            Analytics.TrackEvent("ReportIt - Popup Acknowledged");
            await Application.Current.SavePropertiesAsync();
        }
    }
}

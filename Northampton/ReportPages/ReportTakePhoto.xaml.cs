using System;

using Xamarin.Forms;

namespace Northampton
{
    public partial class ReportTakePhoto : ContentPage
    {
        public ReportTakePhoto()
        {
            InitializeComponent();
            CameraButton.Clicked += CameraButton_Clicked;
        }

        private async void CameraButton_Clicked(object sender, EventArgs e)
        {
            var photo = await Plugin.Media.CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions() { });

            if (photo != null)
                PhotoImage.Source = ImageSource.FromStream(() => { return photo.GetStream(); });
        }
    }
}

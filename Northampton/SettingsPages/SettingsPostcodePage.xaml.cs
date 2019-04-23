using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Northampton
{
    public partial class SettingsPostcodePage : ContentPage
    {
        public SettingsPostcodePage()
        {
            InitializeComponent();
        }

        void Entry_Completed(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }
    }
}

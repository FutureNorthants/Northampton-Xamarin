﻿using System;
using Xamarin.Forms;

namespace Northampton
{
    public partial class SettingsNamePage : ContentPage
    {
        public SettingsNamePage()
        {
            InitializeComponent();
        }

        void Entry_Completed(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }
    }
}

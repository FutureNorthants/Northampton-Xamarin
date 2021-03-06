﻿using System;
using Xamarin.Forms;

namespace Northampton
{
    public partial class SettingsEmailPage : ContentPage
    {
        Boolean callingFromMenu = false;
 
        public SettingsEmailPage(Boolean callingFromMenu)
        {
            this.callingFromMenu = callingFromMenu;
            InitializeComponent();
        }

        void EntryCompleted(object sender, EventArgs e)
        {
            if (callingFromMenu)
            {
                Navigation.PopAsync();
            }
            else
            {
                if (Application.Current.Properties.ContainsKey("SettingsName"))
                {
                    Navigation.PushAsync(new WebServiceHandlerPage("SendProblemToCRM"));
                }
                else
                {
                    Navigation.PushAsync(new SettingsNamePage(false));
                }
            }
        }
    }
}

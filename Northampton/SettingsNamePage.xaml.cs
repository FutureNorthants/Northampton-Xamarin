using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Northampton
{
    public partial class SettingsNamePage : ContentPage
    {
        public SettingsNamePage()
        {
            InitializeComponent();
            var text = MyEntry.Text;
        }

        void Entry_Completed(object sender, EventArgs e)
        {
            var text = ((Entry)sender).Text; //cast sender to access the properties of the Entry
        }
    }
}

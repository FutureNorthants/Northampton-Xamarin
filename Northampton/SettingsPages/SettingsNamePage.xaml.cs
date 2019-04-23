using System;
using Xamarin.Forms;

namespace Northampton
{
    public partial class SettingsNamePage : ContentPage
    {
        Boolean callingFromMenu = false;

        public SettingsNamePage(Boolean callingFromMenu)
        {
            this.callingFromMenu = callingFromMenu;
            InitializeComponent(); 
        }

        void Entry_Completed(object sender, EventArgs e)
        {
            if (callingFromMenu)
            {
                Navigation.PopAsync();
            }
            else
            {
                Navigation.PushAsync(new WebServiceHandlerPage("SendProblemToCRM"));
            }
        }
    }
}

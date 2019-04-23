using System;
using Xamarin.Forms;

namespace Northampton
{
    public partial class SettingsContactPreferencesPage : ContentPage
    {
        private int updatesPickerIndex = -1;

        public SettingsContactPreferencesPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        public int UpdatesPickerIndex
        {
            get
            {
                if (Application.Current.Properties.ContainsKey("SettingsPreferredUpdateChannel"))
                {
                    String paramValue = "";
                    paramValue = Application.Current.Properties["SettingsPreferredUpdateChannel"] as String;
                    Int32.TryParse(paramValue, out updatesPickerIndex);
                }
                return updatesPickerIndex;
            }
            set
            {
                updatesPickerIndex = value;
                Application.Current.Properties["SettingsPreferredUpdateChannel"] = updatesPickerIndex.ToString();
                Application.Current.SavePropertiesAsync();
                Navigation.PopAsync();
            }
        }
    }
}

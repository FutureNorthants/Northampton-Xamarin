using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;

namespace Northampton.ViewModels
{
    class ReportCurrentLocationViewModel : INotifyPropertyChanged
    {
        String settingsName = "";
        IList<String> streets;

        public event PropertyChangedEventHandler PropertyChanged;

        public ReportCurrentLocationViewModel()
        {
            streets.Add("A");
            streets.Add("B");
            streets.Add("C");
            if (Application.Current.Properties.ContainsKey("SettingsName"))
            {
                settingsName = Application.Current.Properties["SettingsName"] as String;
            }

        }

        //public IList<Monkey> Street
        //{
        //    set
        //    {
        //        if (settingsName != value)
        //        {
        //            settingsName = value;
        //            Application.Current.Properties["SettingsName"] = settingsName;
        //            Application.Current.SavePropertiesAsync();
        //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SettingsName"));
        //        }
        //    }
        //    get
        //    {
        //        return settingsName;
        //    }
        //}
    }

    public class Monkey
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public string Details { get; set; }
        public string ImageUrl { get; set; }
    }
}

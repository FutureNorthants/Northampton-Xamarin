using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace Northampton
{
    public class ReportCurrentLocationViewModel : INotifyPropertyChanged
    {
       // String settingsName = "";
        public IList<String> streets
        {
            get
            {
                //IsLoading = true;
                IList<String> tempStreets = new List<string>();
                tempStreets.Add("A");
                tempStreets.Add("B");
                tempStreets.Add("C");
                return tempStreets;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        //      public ReportCurrentLocationViewModel()
        //      {
        //streets.Add("A");
        //streets.Add("B");
        //streets.Add("C");
        //           if (Application.Current.Properties.ContainsKey("SettingsName"))
        //         {
        //           settingsName = Application.Current.Properties["SettingsName"] as String;
        //     }

        //    }

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

        //protected void OnPropertyChanged(string propertyName)
        //{
        //    if (PropertyChanged != null)
        //    {
        //        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        //    }
        //}

        //private bool _isLoading;
        //public bool IsLoading
        //{
        //    get { return _isLoading; }

        //    private set
        //    {
        //        if (_isLoading != value)
        //        {
        //            _isLoading = value;
        //            //OnPropertyChanged();
        //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsLoading"));
        //        }
        //    }
        //}

    }


}

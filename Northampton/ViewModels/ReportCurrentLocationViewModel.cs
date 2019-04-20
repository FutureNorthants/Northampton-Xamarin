using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace Northampton
{
    public class ReportCurrentLocationViewModel : INotifyPropertyChanged
    {
        public IList<String> streets
        {
            get
            {
                IList<String> tempStreets = new List<string>();
                tempStreets.Add("A");
                tempStreets.Add("B");
                tempStreets.Add("C");
                return tempStreets;
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}

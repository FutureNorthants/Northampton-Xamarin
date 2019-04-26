using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace Northampton
{
    [DesignTimeVisible(true)]
    public partial class MainPage :Xamarin.Forms.TabbedPage
    {
        public MainPage()
        {
            InitializeComponent();
            this.SetValue(BarBackgroundColorProperty, "Color.Black");
            On<Android>().SetToolbarPlacement(ToolbarPlacement.Bottom)
             .SetBarItemColor(Color.Gray)
             .SetBarSelectedItemColor(Color.White);

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Northampton
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainTab : TabbedPage
    {
        public MainTab()
        {
            InitializeComponent();

                //var navigationPage = new NavigationPage(new SchedulePageCS());
                //navigationPage.Icon = "schedule.png";
                //navigationPage.Title = "Schedule";           
                //Children.Add(new TodayPageCS());
                //Children.Add(navigationPage);
        }
    }
}

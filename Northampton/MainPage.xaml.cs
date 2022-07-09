using System.ComponentModel;
using System.IO;
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
            On<Android>().SetToolbarPlacement(ToolbarPlacement.Bottom);
            On<Android>().SetBarItemColor(Color.Gray);
            On<Android>().SetBarSelectedItemColor(Color.White);
        }
    }
}

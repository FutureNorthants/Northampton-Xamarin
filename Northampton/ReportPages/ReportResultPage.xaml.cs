using System;
using System.Linq;
using Xamarin.Forms;

namespace Northampton
{
    public partial class ReportResultPage : ContentPage
    {
        String referenceNumber = "";
        String slaText = "";

        public ReportResultPage(String referenceNumber, String slaText)
        {
            InitializeComponent();
            this.referenceNumber = referenceNumber;
            this.slaText = slaText;
            BindingContext = this;
        }

        public string ReferenceNumber
        {
            get
            {
                return referenceNumber;
            }
        }

        public string SLAText
        {
            get
            {
                return slaText;
            }
        }

        void ReportAnotherButtonClicked(object sender, EventArgs args)
        {
            if (Navigation.NavigationStack.Count > 0)
            {
                Navigation.PopToRootAsync();
            }
        }
    }
}

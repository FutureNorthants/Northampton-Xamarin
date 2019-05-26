using System;
using Xamarin.Forms;

namespace Northampton
{
    public partial class CollectionFinderResultPage : ContentPage
    {
        String collectionDay = "";
        String collectionType = "";

        public CollectionFinderResultPage(String collectionDay, String collectionType)
        {
            InitializeComponent();
            this.collectionDay = collectionDay;
            switch (collectionType)
            {
                case "black":
                    this.collectionType = "Black Wheelie Bin";
                    break;
                case "brown":
                    this.collectionType = "Brown Wheelie Bin";
                    break;
                case "bags":
                    this.collectionType = "Green Bags";
                    break;
                default:
                    break;
            }
            BindingContext = this;
        }

        public string CollectionDay
        {
            get
            {
                return collectionDay;
            }
        }

        public string CollectionType
        {
            get
            {
                return collectionType;
            }
        }

        void CheckAnotherCollectionButtonClicked(object sender, EventArgs args)
        {
            if (Navigation.NavigationStack.Count > 0)
            {
                Navigation.PopToRootAsync();
            }
        }
    }
}

﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace Northampton
{
    public partial class ReportDetailsPage : ContentPage
    {
        private int streetPickerIndex = -1;
        private int typePickerIndex = -1;
        private int updatesPickerIndex = -1;
        private String problemDescription = "";
        IList<Street> storedStreets = new List<Street>();
        IList<Problem> storedProblems = new List<Problem>();

        public ReportDetailsPage()
        {
            InitializeComponent();
            storedProblems.Add(new Problem(0, "litter", "Litter"));
            storedProblems.Add(new Problem(1, "flytip", "Flytipping"));
            storedProblems.Add(new Problem(2, "bodily_fluids", "Bodily Fluids"));
            storedProblems.Add(new Problem(3, "a dead animal s", "Broken Glass"));
            storedProblems.Add(new Problem(4, "dead_animal", "Dead Animal"));
            storedProblems.Add(new Problem(5, "dog_fouling", "Dog Fouling"));
            storedProblems.Add(new Problem(6, "drug_paraphernalia", "Drug Paraphernalia"));
            storedProblems.Add(new Problem(7, "non_offensive_flyposting", "Flyposting - Non Offensive"));
            storedProblems.Add(new Problem(8, "offensive_flyposting", "Flyposting - Offensive"));
            storedProblems.Add(new Problem(9, "non_offensive_graffiti", "Graffiti - Non Offensive"));
            storedProblems.Add(new Problem(10, "offensive_graffiti", "Graffiti - Offensive"));
            storedProblems.Add(new Problem(11, "gum_removal", "Gum Removal"));
            storedProblems.Add(new Problem(12, "leaf_clearance", "Leaf Clearance"));
            storedProblems.Add(new Problem(13, "overflowing_litter_bin", "Litter Bin Overflowing"));
            storedProblems.Add(new Problem(14, "street_furniture_cleaning", "Street Sign Cleaning"));
            storedProblems.Add(new Problem(15, "street_washing", "Street Washing"));
            storedProblems.Add(new Problem(16, "sweeper_bags_not_collected", "Sweeper Bags Not Collected"));
            storedProblems.Add(new Problem(17, "sweeping_required", "Sweeping Required"));
            storedProblems.Add(new Problem(18, "weed_clearance", "Weed Clearance"));
            BindingContext = this;
        }

        public int StreetPickerIndex
        {
            get
            {
                return streetPickerIndex;
            }
            set
            {
                streetPickerIndex = value;
            }
        }

        public int TypePickerIndex
        {
            get
            {
                return typePickerIndex;
            }
            set
            {
                typePickerIndex = value;
            }
        }

        void EditorCompleted(object sender, EventArgs e)
        {
            problemDescription = ((Editor)sender).Text;
            Application.Current.Properties["ProblemDescription"] = ((Editor)sender).Text;
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
            }
        }

        async void SubmitButtonClicked(object sender, EventArgs args)
        {
            if (streetPickerIndex == -1)
            {
                await DisplayAlert("Missing Information", "Please confirm the street nearest the problem", "OK");
            }
            else if (typePickerIndex == -1)
            {
                await DisplayAlert("Missing Information", "Please confirm the type of problem", "OK");
            }
            else if (updatesPickerIndex == -1)
            {
                await DisplayAlert("Missing Information", "Please confirm if you would like updates on resolving this problem", "OK");
            }
            else
            {
                Application.Current.Properties["ProblemType"] = storedProblems[typePickerIndex].ProblemName;
                switch (updatesPickerIndex)
                {
                    case 0:
                        //Email
                        Application.Current.Properties["ProblemUpdates"] = "email";
                        break;
                    case 1:
                        //Text
                        Application.Current.Properties["ProblemUpdates"] = "text";
                        break;
                    case 2:
                        Application.Current.Properties["ProblemUpdates"] = "none";
                        //No Updates;
                        break;
                    default:
                        Console.WriteLine("Updates setting not found");
                        break;
                }

                Application.Current.Properties["ProblemLocation"] = storedStreets[streetPickerIndex].StreetName;
                if (Application.Current.Properties["ProblemLat"].ToString().Equals(""))
                {
                    Application.Current.Properties["ProblemLat"] = storedStreets[streetPickerIndex].Latitude;
                    Application.Current.Properties["ProblemLng"] = storedStreets[streetPickerIndex].Longtitude;
                }

                await Application.Current.SavePropertiesAsync();
                switch (updatesPickerIndex)
                {
                    case 0:
                        //Email
                        if (Application.Current.Properties.ContainsKey("SettingsEmail"))
                        {
                            if (Application.Current.Properties.ContainsKey("SettingsName"))
                            {
                                await Navigation.PushAsync(new WebServiceHandlerPage("SendProblemToCRM"));
                            }
                            else
                            {
                                await Navigation.PushAsync(new SettingsNamePage(false));
                            }
                        }
                        else
                        {
                            await Navigation.PushAsync(new SettingsEmailPage(false));
                        }
                        break;
                    case 1:
                        //Text
                        if (Application.Current.Properties.ContainsKey("SettingsPhoneNumber"))
                        {
                            if (Application.Current.Properties.ContainsKey("SettingsName"))
                            {
                                await Navigation.PushAsync(new WebServiceHandlerPage("SendProblemToCRM"));
                            }
                            else
                            {
                                await Navigation.PushAsync(new SettingsNamePage(false));
                            }
                        }
                        else
                        {
                            await Navigation.PushAsync(new SettingsPhoneNumberPage(false));
                        }
                        break;
                    default:
                        //None
                        if (Application.Current.Properties.ContainsKey("SettingsName"))
                        {
                            await Navigation.PushAsync(new WebServiceHandlerPage("SendProblemToCRM"));
                        }
                        else
                        {
                            await Navigation.PushAsync(new SettingsNamePage(false));
                        }
                        break;
                }
            }
        }

        public IList<String> streets
        {
            get
            {
                String streetsJson = null;
                if (Application.Current.Properties.ContainsKey("JsonStreets"))
                {
                    streetsJson = Application.Current.Properties["JsonStreets"] as String;
                }
                IList<String> tempStreets = new List<string>();
                JObject streetsJSONobject = JObject.Parse(streetsJson);
                JArray resultsArray = (JArray)streetsJSONobject["results"];
                for (int currentResult = 0; currentResult < resultsArray.Count; currentResult++)
                {
                    tempStreets.Add(resultsArray[currentResult][1].ToString());
                    if (Application.Current.Properties["ProblemLat"].ToString().Equals(""))
                    {
                        storedStreets.Add(new Street(resultsArray[currentResult][0].ToString(),
                                                     resultsArray[currentResult][1].ToString(),
                                                     resultsArray[currentResult][2].ToString(),
                                                     resultsArray[currentResult][3].ToString()
                                                     ));
                    }
                    else
                    {
                        storedStreets.Add(new Street(resultsArray[currentResult][0].ToString(),
                                                     resultsArray[currentResult][1].ToString(),
                                                     "",
                                                     ""
                                                     ));
                    }

                }
                if (resultsArray.Count == 1)
                {
                    streetPickerIndex = 1;
                }
                return tempStreets;
            }
        }

        public IList<String> problems
        {
            get
            {
                IList<String> tempProblems = new List<string>();
                for (int currentProblem = 0; currentProblem < storedProblems.Count; currentProblem++)
                {
                    tempProblems.Add(storedProblems[currentProblem].ProblemDescription);
                }
                return tempProblems;
            }
        }
    }

    public class Street
    {
        public String USRN { get; set; }
        public String StreetName { get; set; }
        public String Latitude { get; set; }
        public String Longtitude { get; set; }
        public Street(String usrn, String streetName, String latitude, String longtitude)
        {
            USRN = usrn;
            StreetName = streetName;
            Latitude = latitude;
            Longtitude = longtitude;
        }

    }

    public class Problem
    {
        public int ProblemNo { get; set; }
        public String ProblemName { get; set; }
        public String ProblemDescription { get; set; }
        public Problem(int ProblemNo, String ProblemName, String ProblemDescription)
        {
            this.ProblemNo = ProblemNo;
            this.ProblemName = ProblemName;
            this.ProblemDescription = ProblemDescription;
        }

    }
}

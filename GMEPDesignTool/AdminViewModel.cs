﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mysqlx.Crud;

namespace GMEPDesignTool
{
    class AdminViewModel : INotifyPropertyChanged
    {

        public string projectNo;

        public string ProjectNo
        {
            get => projectNo;
            set
            {
                if (projectNo != value)
                {
                    projectNo = value;
                    OnPropertyChanged(nameof(ProjectNo));
                }
            }
        }


        public string projectName;

        public string ProjectName
        {
            get => projectName;
            set
            {
                if (projectName != value)
                {
                    projectName = value;
                    OnPropertyChanged(nameof(ProjectName));
                }
            }
        }

        public string streetaddress;
        public string StreetAddress
        {
            get => streetaddress;
            set
            {
                if (streetaddress != value)
                {
                    streetaddress = value;
                    OnPropertyChanged(nameof(StreetAddress));
                }
            }
        }
        public string city;
        public string City
        {
            get => city;
            set
            {
                if (city != value)
                {
                    city = value;
                    OnPropertyChanged(nameof(City));
                }
            }
        }

        public ObservableCollection<string> States { get; set; } =
        new ObservableCollection<string> { "AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "FL", "GA",
                                          "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MD",
                                          "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ",
                                          "NM", "NY", "NC", "ND", "OH", "OK", "OR", "PA", "RI", "SC",
                                          "SD", "TN", "TX", "UT", "VT", "VA", "WA", "WV", "WI", "WY"};


        public string state;
        public string State
        {
            get => state;
            set
            {
                if (state != value)
                {
                    state = value;
                    OnPropertyChanged(nameof(State));
                }
            }
        }
        public string postalCode;
        public string PostalCode
        {
            get => postalCode;
            set
            {
                if (postalCode != value)
                {
                    postalCode = value;
                    OnPropertyChanged(nameof(PostalCode));
                }
            }
        }

        public string fileDictionary;
        public string FileDictionary
        {
            get => fileDictionary;
            set
            {
                if (fileDictionary != value)
                {
                    fileDictionary = value;
                    OnPropertyChanged(nameof(FileDictionary));
                }
            }
        }


        public AdminViewModel(string projectId)

        {
            LoadProjectInfoAsync(projectId);
        }

        private async void LoadProjectInfoAsync(string projectId)
        {
            var db = new Database.Database(GMEPDesignTool.Properties.Settings.Default.ConnectionString);
            AdminModel ProjectInfo = await db.GetAdminByProjectId(projectId);
            ProjectNo = ProjectInfo.ProjectNo;
            ProjectName = ProjectInfo.ProjectName;
            StreetAddress = ProjectInfo.StreetAddress;
            City = ProjectInfo.City;
            State = ProjectInfo.State;
            PostalCode = ProjectInfo.PostalCode;
            FileDictionary = ProjectInfo.Directory;
            Console.WriteLine("LoadProjectInfoAsync complete : " + ProjectName);
        }



        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
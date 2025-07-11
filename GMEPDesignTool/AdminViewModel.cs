using System;
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
        private string projectNo;

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

        private string projectName;

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
        private string client;

        public string Client
        {
            get => client;
            set
            {
                if (client != value)
                {
                    client = value;
                    OnPropertyChanged(nameof(Client));
                }
            }
        }

        private string architect;

        public string Architect
        {
            get => architect;
            set
            {
                if (architect != value)
                {
                    architect = value;
                    OnPropertyChanged(nameof(Architect));
                }
            }
        }

        private string streetaddress;
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
        private string city;
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
            new ObservableCollection<string>
            {
                "AL",
                "AK",
                "AZ",
                "AR",
                "CA",
                "CO",
                "CT",
                "DE",
                "FL",
                "GA",
                "HI",
                "ID",
                "IL",
                "IN",
                "IA",
                "KS",
                "KY",
                "LA",
                "ME",
                "MD",
                "MA",
                "MI",
                "MN",
                "MS",
                "MO",
                "MT",
                "NE",
                "NV",
                "NH",
                "NJ",
                "NM",
                "NY",
                "NC",
                "ND",
                "OH",
                "OK",
                "OR",
                "PA",
                "RI",
                "SC",
                "SD",
                "TN",
                "TX",
                "UT",
                "VT",
                "VA",
                "WA",
                "WV",
                "WI",
                "WY",
            };

        private string state;
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
        private string postalCode;
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

        private string fileDictionary;
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

        private bool isCheckedS;
        public bool IsCheckedS
        {
            get => isCheckedS;
            set
            {
                if (isCheckedS != value)
                {
                    isCheckedS = value;
                    OnPropertyChanged(nameof(IsCheckedS));
                }
            }
        }

        private bool isCheckedM;
        public bool IsCheckedM
        {
            get => isCheckedM;
            set
            {
                if (isCheckedM != value)
                {
                    isCheckedM = value;
                    OnPropertyChanged(nameof(IsCheckedM));
                }
            }
        }
        private bool isCheckedE;
        public bool IsCheckedE
        {
            get => isCheckedE;
            set
            {
                if (isCheckedE != value)
                {
                    isCheckedE = value;
                    OnPropertyChanged(nameof(IsCheckedE));
                }
            }
        }
        private bool isCheckedP;
        public bool IsCheckedP
        {
            get => isCheckedP;
            set
            {
                if (isCheckedP != value)
                {
                    isCheckedP = value;
                    OnPropertyChanged(nameof(IsCheckedP));
                }
            }
        }
        private string descriptions;
        public string Descriptions
        {
            get => descriptions;
            set
            {
                if (descriptions != value)
                {
                    descriptions = value;
                    OnPropertyChanged(nameof(Descriptions));
                }
            }
        }

        public AdminViewModel(string projectId)
        {
            LoadProjectInfoAsync(projectId);
        }
        private async void LoadProjectInfoAsync(string projectId)
        {
            var db = new Database.Database(
                GMEPDesignTool.Properties.Settings.Default.ConnectionString
            );
            AdminModel ProjectInfo = await db.GetAdminByProjectId(projectId);
            ProjectNo = ProjectInfo.ProjectNo;
            ProjectName = ProjectInfo.ProjectName;
            Client = ProjectInfo.Client;
            Architect = ProjectInfo.Architect;
            StreetAddress = ProjectInfo.StreetAddress;
            City = ProjectInfo.City;
            State = ProjectInfo.State;
            PostalCode = ProjectInfo.PostalCode;
            FileDictionary = ProjectInfo.Directory;
            IsCheckedS = ProjectInfo.IsCheckedS;
            IsCheckedM = ProjectInfo.IsCheckedM;
            IsCheckedE = ProjectInfo.IsCheckedE;
            IsCheckedP = ProjectInfo.IsCheckedP;
            Descriptions = ProjectInfo.Descriptions;


        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

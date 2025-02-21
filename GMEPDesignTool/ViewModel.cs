using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace GMEPDesignTool
{
    public class ViewModel : ViewModelBase
    {
        private LoginResponse loginResponse;
        private List<Project> _projects = new List<Project>();
        public List<Project> Projects
        {
            get => _projects;
            set => SetProperty(ref _projects, value);
        }

        private static Dictionary<string, string> StandardColorScheme = new Dictionary<
            string,
            string
        >()
        {
            { "Background1", "White" },
            { "Background2", "Gainsboro" },
            { "Foreground1", "Black" },
            { "Foreground2", "White" },
            { "Link", "Blue" },
            { "LinkDisabled", "Gray" },
            { "ReadOnlyTextBox", "LightYellow" },
            { "CustomColor0", "White" },
            { "CustomColor1", "LightCoral" },
            { "CustomColor2", "LightSalmon" },
            { "CustomColor3", "NavajoWhite" },
            { "CustomColor4", "Khaki" },
            { "CustomColor5", "YellowGreen" },
            { "CustomColor6", "LimeGreen" },
            { "CustomColor7", "MediumTurquoise" },
            { "CustomColor8", "LightSkyBlue" },
            { "CustomColor9", "Thistle" },
            { "CustomColor10", "Plum" },
        };

        private static Dictionary<string, Dictionary<string, string>> ColorSchemes = new Dictionary<
            string,
            Dictionary<string, string>
        >()
        {
            { "Standard", StandardColorScheme },
            { "Dark", StandardColorScheme },
        };

        private Dictionary<string, string> _colors = StandardColorScheme;
        public Dictionary<string, string> Colors
        {
            get => _colors;
            set => SetProperty(ref _colors, value);
        }

        private string _name = "";
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _emailAddress = "";
        public string EmailAddress
        {
            get => _emailAddress;
            set => SetProperty(ref _emailAddress, value);
        }

        private string _phoneNumber = "";
        public string PhoneNumber
        {
            get => _phoneNumber;
            set => SetProperty(ref _phoneNumber, value);
        }

        private List<Dictionary<string, string>> _searchResults =
            new List<Dictionary<string, string>>();
        public List<Dictionary<string, string>> SearchResults
        {
            get => _searchResults;
            set => SetProperty(ref _searchResults, value);
        }

        private List<string> _searchResultKeys = new List<string>();
        public List<string> SearchResultKeys
        {
            get => _searchResultKeys;
            set => SetProperty(ref _searchResultKeys, value);
        }

        private Visibility _AdminMenuVisible;
        public Visibility AdminMenuVisible
        {
            get => _AdminMenuVisible;
            set
            {
                if (_AdminMenuVisible != value)
                {
                    _AdminMenuVisible = value;
                }
            }
        }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    if (value.Length > 5)
                    {
                        GetSearchResultsCommand.Execute(this);
                    }
                }
            }
        }

        private readonly DelegateCommand _getSearchResultsCommand;
        public ICommand GetSearchResultsCommand => _getSearchResultsCommand;

        private readonly DelegateCommand _openEmployeesWindowCommand;
        public ICommand OpenEmployeesWindowCommand => _openEmployeesWindowCommand;
        public ObservableCollection<TabItem> Tabs { get; set; }

        public ViewModel(LoginResponse loginResponse)
        {
            this.loginResponse = loginResponse;
            Tabs = new ObservableCollection<TabItem>();
            _getSearchResultsCommand = new DelegateCommand(GetSearchResults, CanGetSearchResults);
            _openEmployeesWindowCommand = new DelegateCommand(
                OpenEmployeesWindow,
                CanOpenEmployeesWindow
            );
            Name = loginResponse.FirstName + " " + loginResponse.LastName;
            EmailAddress = loginResponse.EmailAddress;
            PhoneNumber = loginResponse.PhoneNumber.ToString();
            if (loginResponse.Extension != null && loginResponse.Extension != 0)
            {
                PhoneNumber += " ext. " + loginResponse.Extension.ToString();
            }
            AdminMenuVisible = Visibility.Collapsed;
            if (loginResponse.AccessLevelId == 1)
            {
                AdminMenuVisible = Visibility.Visible;
            }
        }

        private void GetSearchResults(object commandParameter)
        {
            SearchResults = new List<Dictionary<string, string>>();
            SearchResultKeys = new List<string>();
            SearchResultKeys.Add(SearchText);
        }

        private bool CanGetSearchResults(object commandParameter)
        {
            return true;
        }

        private void OpenEmployeesWindow(object commandParameter)
        {
            EmployeesWindow employeesWindow = new EmployeesWindow(loginResponse);
            employeesWindow.Show();
        }

        private bool CanOpenEmployeesWindow(object commandParameter)
        {
            if (loginResponse.AccessLevelId == 1)
            {
                return true;
            }
            return false;
        }

        public async void OpenProject(string projectNo)
        {
            foreach (TabItem tab in Tabs)
            {
                if ((string)tab.Header == projectNo)
                {
                    return;
                }
            }

            LoadingScreen loadingScreen = new LoadingScreen();
            TabItem newTab = new TabItem
            {
                Header = projectNo,
                Content = loadingScreen,
                IsSelected = true,
            };
            Tabs.Add(newTab);
            var projectControl = new ProjectControl();
            await projectControl.InitializeProject(projectNo, loginResponse);
            newTab.Content = projectControl;
        }

        public void CloseProject(TabItem projectTab)
        {
            Tabs.Remove(projectTab);
        }
    }
}

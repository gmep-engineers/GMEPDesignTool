using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace GMEPDesignTool
{
    public class ViewModel : ViewModelBase
    {
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
        public ObservableCollection<TabItem> Tabs { get; set; }

        public ViewModel()
        {
            Tabs = new ObservableCollection<TabItem>();
            _getSearchResultsCommand = new DelegateCommand(GetSearchResults, CanGetSearchResults);
            Name = "My Name";
        }

        private void GetSearchResults(object commandParameter)
        {
            SearchResults = new List<Dictionary<string, string>>();
            SearchResultKeys = new List<string>();
            Dictionary<string, string> thing = new Dictionary<string, string>();
            thing["24-100"] = "Z:\\lskdjfksljf";
            SearchResults.Add(thing);
            SearchResultKeys.Add(SearchText);
        }

        private bool CanGetSearchResults(object commandParameter)
        {
            return true;
        }

        public void OpenProject(string projectName)
        {
            foreach (TabItem tab in Tabs)
            {
                if ((string)tab.Header == projectName)
                {
                    return;
                }
            }
            Tabs.Add(
                new TabItem
                {
                    Header = projectName,
                    Content = new ProjectControl(),
                    IsSelected = true,
                }
            );
        }

        public void CloseProject(TabItem projectTab)
        {
            Tabs.Remove(projectTab);
        }
    }
}

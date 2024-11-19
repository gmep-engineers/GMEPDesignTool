using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace GMEPDesignTool
{
  public class ViewModel : ViewModelBase
  {
    private string _name = "";
    public string Name
    {
      get => _name;
      set => SetProperty(ref _name, value);
    }

    private List<Dictionary<string, string>> _searchResults = new List<Dictionary<string, string>>();
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

    public ViewModel()
    {
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
  }
}

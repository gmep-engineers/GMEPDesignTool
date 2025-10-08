using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
  public class ProposalsViewModel : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string name)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
      if (name != "WindowTitle")
      {
        Saved = false;
      }
    }

    private bool _Saved = true;
    public bool Saved
    {
      get => _Saved;
      set
      {
        _Saved = value;
        if (_Saved == true)
        {
          WindowTitle = "Proposals";
        }
        else
        {
          WindowTitle = "Proposals*";
        }
      }
    }
    public ObservableCollection<string> YearSelectionOptions { get; set; }

    Database.Database Database { get; set; }

    public ObservableCollection<Proposal> FilteredProposals { get; set; }

    public List<Client> Clients { get; set; }

    public List<Employee> Employees { get; set; }

    private string windowTitle = "Proposals";
    public string WindowTitle
    {
      get => windowTitle;
      set
      {
        if (windowTitle != value)
        {
          windowTitle = value;
          OnPropertyChanged(nameof(WindowTitle));
        }
      }
    }

    public ProposalsViewModel(LoginResponse loginResponse)
    {
      int currentYear = DateTime.Now.Year;
      YearSelectionOptions = new ObservableCollection<string>();
      while (currentYear >= 2010)
      {
        YearSelectionOptions.Add(currentYear.ToString());
        currentYear--;
      }
      Database = new Database.Database(loginResponse.SqlConnectionString);

      Clients = Database.GetClients();

      FilteredProposals = new ObservableCollection<Proposal>();

      Employees = new List<Employee>();

      FilterDataGridByYear(DateTime.Now.Year.ToString());

      Saved = true;
    }

    public void FilterDataGridByYear(string year)
    {
      FilteredProposals.Clear();
      ObservableCollection<Proposal> proposals = Database.GetProposalsByYear(year);
      foreach (Proposal proposal in proposals)
      {
        proposal.ProposalsViewModel = this;
        FilteredProposals.Add(proposal);
      }

      Employees.Clear();
      List<Employee> employees = Database.GetAdminEmployeesByYear(year);
      foreach (Employee employee in employees)
      {
        Employees.Add(employee);
      }
    }

    public void Save()
    {
      List<Proposal> proposals = new List<Proposal>();
      foreach (Proposal proposal in FilteredProposals)
      {
        if (proposal.Modified)
        {
          Database.SaveProposal(proposal);
        }
      }
    }
  }
}

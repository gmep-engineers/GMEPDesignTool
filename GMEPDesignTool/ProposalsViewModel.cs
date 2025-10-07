using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
  public class ProposalsViewModel : ViewModelBase
  {
    public ObservableCollection<string> YearSelectionOptions { get; set; }

    Database.Database Database { get; set; }

    public ObservableCollection<Proposal> FilteredProposals { get; set; }

    public List<Client> Clients { get; set; }

    public List<Employee> Employees { get; set; }

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

      FilteredProposals = Database.GetProposalsByYear(DateTime.Now.Year.ToString());

      Clients = Database.GetClients();

      Employees = Database.GetAdminEmployeesByYear(DateTime.Now.Year.ToString());
    }

    public void FilterDataGridByYear(string year)
    {
      FilteredProposals.Clear();
      ObservableCollection<Proposal> proposals = Database.GetProposalsByYear(year);
      foreach (Proposal proposal in proposals)
      {
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

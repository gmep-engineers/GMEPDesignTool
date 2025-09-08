using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
  public class AddEditContactViewModel : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private string _CompanyId;
    public string CompanyId
    {
      get => _CompanyId;
      set => _CompanyId = value;
    }

    private string _CompanyName;
    public string CompanyName
    {
      get => _CompanyName;
      set => _CompanyName = value;
    }

    Database.Database Database { get; set; }

    public ObservableCollection<Contact> CompanyContacts { get; set; }

    public AddEditContactViewModel(
      LoginResponse loginResponse,
      string companyId,
      string companyName
    )
    {
      _CompanyId = companyId;
      _CompanyName = companyName;
      Database = new Database.Database(loginResponse.SqlConnectionString);
      CompanyContacts = new ObservableCollection<Contact>(Database.GetContacts(companyId));
    }

    public void CreateContact() { }
  }
}

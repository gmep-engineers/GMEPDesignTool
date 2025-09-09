using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
    public Contact? SelectedContact { get; set; }

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
      foreach (Contact contact in CompanyContacts)
      {
        contact.New = false;
      }
    }

    public void Save()
    {
      List<Contact> deletedContacts = new List<Contact>();
      foreach (Contact contact in CompanyContacts)
      {
        contact.CompanyId = _CompanyId;
        if (contact.Delete)
        {
          Database.DeleteContact(contact);
          deletedContacts.Add(contact);
        }
        else if (contact.Modified)
        {
          Database.SaveContact(contact);
          contact.Modified = false;
        }
      }
      foreach (Contact contact in deletedContacts)
      {
        CompanyContacts.Remove(contact);
      }
    }

    public void SaveContactOnEnter()
    {
      foreach (Contact contact in CompanyContacts)
      {
        contact.CompanyId = _CompanyId;
        if (contact.Modified)
        {
          Database.SaveContact(contact);
          contact.Modified = false;
        }
      }
    }

    public void FlagForDeletion()
    {
      if (SelectedContact != null)
      {
        SelectedContact.Delete = !SelectedContact.Delete;
      }
    }
  }
}

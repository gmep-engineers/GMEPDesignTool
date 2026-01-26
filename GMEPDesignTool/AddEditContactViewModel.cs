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

    Proposal Proposal { get; set; }

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
      string companyName,
      string companyId,
      LoginResponse loginResponse,
      Proposal proposal
    // HERE add list of client contacts, same data type as in the client contact column
    )
    {
      _CompanyId = companyId;
      _CompanyName = companyName;
      Database = new Database.Database(loginResponse.SqlConnectionString);
      CompanyContacts = new ObservableCollection<Contact>(Database.GetContacts(_CompanyId));
      foreach (Contact contact in CompanyContacts)
      {
        contact.New = false;
      }
      Proposal = proposal;
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
          var proposalContact = Proposal.Contacts.FirstOrDefault((p) => p.Id == contact.Id);
          if (proposalContact != null)
          {
            proposalContact.FullName = contact.FirstName + " " + contact.LastName;
          }
        }
      }
      foreach (Contact contact in deletedContacts)
      {
        CompanyContacts.Remove(contact);
        var proposalContact = Proposal.Contacts.FirstOrDefault((p) => p.Id == contact.Id);
        if (proposalContact != null)
        {
          Proposal.Contacts.Remove(proposalContact);
        }
      }
    }

    public void SaveContactOnEnter()
    {
      foreach (Contact contact in CompanyContacts)
      {
        contact.CompanyId = _CompanyId;
        if (contact.Modified)
        {
          if (contact.New && Proposal != null)
          {
            Proposal.Contacts.Add(
              new ProposalContact()
              {
                FullName = contact.FirstName + " " + contact.LastName,
                Id = contact.Id,
              }
            );
          }
          Database.SaveContact(contact);
          contact.Modified = false;
          if (Proposal != null)
          {
            ProposalContact? proposalContact = Proposal.Contacts.FirstOrDefault(
              (p) => p.Id == contact.Id
            );
            if (proposalContact != null)
            {
              proposalContact.FullName = contact.FirstName + " " + contact.LastName;
            }
          }
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

    public void SetPrimaryContact()
    {
      if (SelectedContact != null)
      {
        Database.SetPrimaryContact(SelectedContact);
      }
    }
  }
}

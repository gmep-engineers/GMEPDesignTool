using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GMEPDesignTool
{
    public class Employee
    {
        private string _Id = String.Empty;
        public string Id
        {
            get => _Id;
            set => _Id = value;
        }
        private string _LastName = String.Empty;
        public string LastName
        {
            get => _LastName;
            set => _LastName = value;
        }
        private string _FirstName = String.Empty;
        public string FirstName
        {
            get => _FirstName;
            set => _FirstName = value;
        }
        private int _TitleId;
        public int TitleId
        {
            get => _TitleId;
            set => _TitleId = value;
        }
        private int _DepartmentId;
        public int DeparmentId
        {
            get => _DepartmentId;
            set => _DepartmentId = value;
        }

        private string _EmailAddressId = String.Empty;
        public string EmailAddressId
        {
            get => _EmailAddressId;
            set => _EmailAddressId = value;
        }

        private string _EmailAddress = String.Empty;
        public string EmailAddress
        {
            get => _EmailAddress;
            set => _EmailAddress = value;
        }
        private string _PhoneNumberId = String.Empty;
        public string PhoneNumberId
        {
            get => _PhoneNumberId;
            set => _PhoneNumberId = value;
        }

        private string _PhoneNumber = String.Empty;
        public string PhoneNumber
        {
            get => _PhoneNumber;
            set => _PhoneNumber = value;
        }
        private string _Extension = String.Empty;
        public string Extension
        {
            get => _Extension;
            set => _Extension = value;
        }
        private string _HireDate = String.Empty;
        public string HireDate
        {
            get => _HireDate;
            set => _HireDate = value;
        }
        private string _TerminationDate = String.Empty;
        public string TerminationDate
        {
            get => _TerminationDate;
            set => _TerminationDate = value;
        }
        private string _Username = String.Empty;
        public string Username
        {
            get => _Username;
            set => _Username = value;
        }

        public Employee(
            string id,
            string lastName,
            string firstName,
            int titleId,
            int deparmentId,
            string emailAddress,
            string emailAddressId,
            string phoneNumber,
            string phoneNumberId,
            string extension,
            string hireDate,
            string terminationDate,
            string username
        )
        {
            Id = id;
            LastName = lastName;
            FirstName = firstName;
            TitleId = titleId;
            DeparmentId = deparmentId;
            EmailAddress = emailAddress;
            EmailAddressId = emailAddressId;
            PhoneNumber = "";
            if (!String.IsNullOrEmpty(phoneNumber))
            {
                PhoneNumber =
                    $"({phoneNumber.Substring(0, 3)}) {phoneNumber.Substring(3, 3)}-{phoneNumber.Substring(6, 4)}";
            }
            PhoneNumberId = phoneNumberId;
            Extension = extension;
            HireDate = hireDate.Substring(0, 10);
            TerminationDate = "";
            if (!String.IsNullOrEmpty(terminationDate))
            {
                TerminationDate = terminationDate.Substring(0, 10);
            }
            Username = username;
        }
    }

    public enum Departments
    {
        Structural,
        Mechanical,
        Electrical,
        Plumbing,
        Administration,
    }

    public enum Titles
    {
        Engineer,
        Manager,
    }

    class EmployeesViewModel : ViewModelBase
    {
        public List<Employee> Employees { get; set; }
        public Database.Database Database { get; set; }

        public Employee? SelectedEmployee { get; set; }

        public EmployeesViewModel(LoginResponse loginResponse)
        {
            Database = new Database.Database(loginResponse.SqlConnectionString);
            Employees = Database.GetEmployees();
        }

        public void OpenUpdatePasswordWindow()
        {
            UpdatePasswordWindow updatePasswordWindow = new UpdatePasswordWindow(
                Database,
                SelectedEmployee.Id
            );
            updatePasswordWindow.ShowDialog();
        }
    }
}

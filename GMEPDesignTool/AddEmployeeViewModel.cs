using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
    internal class AddEmployeeViewModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ulong? PhoneNumber { get; set; }
        public uint? Extension { get; set; }
        public DateTime HireDate { get; set; }
        public Database.Database Database { get; set; }

        private string _EmployeeId;

        private string _EntityId;

        private string _ContactId;

        private string _EmailAddressId;

        private string _PhoneNumberId;

        public AddEmployeeViewModel(LoginResponse loginResponse)
        {
            Database = new Database.Database(loginResponse.SqlConnectionString);
            HireDate = DateTime.Today;
            _EntityId = Guid.NewGuid().ToString();
            _ContactId = Guid.NewGuid().ToString();
            _EmailAddressId = Guid.NewGuid().ToString();
            _PhoneNumberId = Guid.NewGuid().ToString();
            _EmployeeId = Guid.NewGuid().ToString();
        }

        public bool CreateEmployee()
        {
            return Database.CreateEmployee(
                Username,
                Password,
                EmailAddress,
                FirstName,
                LastName,
                PhoneNumber,
                Extension,
                HireDate,
                _EmployeeId,
                _EntityId,
                _ContactId,
                _EmailAddressId,
                _PhoneNumberId
            );
        }

        public Employee GetEmployeeObject()
        {
            return new Employee(
                _EmployeeId,
                _ContactId,
                _EntityId,
                LastName,
                FirstName,
                0,
                0,
                EmailAddress,
                _EmailAddressId,
                PhoneNumber,
                _PhoneNumberId,
                Extension,
                HireDate,
                null,
                Username
            );
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
    public class Proposal
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public DateTime DateCreated { get; set; }
        public string Type { get; set; }
        public string EmployeeUsername { get; set; }
        public string Pdf_name { get; set; }
        public Proposal() { }

        public Proposal(
            string id,
            string projectId,
            DateTime dateCreated,
            string type,
            string employeeUsername,
            string pdf_name

        )
        {
            Id = id;
            ProjectId = projectId;
            DateCreated = dateCreated;
            Type = type;
            EmployeeUsername = employeeUsername;
            Pdf_name = pdf_name;
        }
    }
}

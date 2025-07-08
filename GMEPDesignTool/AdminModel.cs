using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
    public class AdminModel
    {
        public string ProjectId { get; set; }
        public string ProjectNo { get; set; }
        public string Client { get; set; }
        public string Architect { get; set; }
        public string ProjectName { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Directory { get; set; }
        public bool IsCheckedS { get; set; }
        public bool IsCheckedM { get; set; }
        public bool IsCheckedE { get; set; }
        public bool IsCheckedP { get; set; }
        public string Descriptions { get; set; }

    }
}
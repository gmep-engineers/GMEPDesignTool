using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Amazon.S3.Model;
using Google.Protobuf.WellKnownTypes;
using Org.BouncyCastle.Asn1.Cmp;
using Org.BouncyCastle.Pqc.Crypto.Lms;


namespace GMEPDesignTool
{
    /// <summary>
    /// Interaction logic for ProjectControl.xaml
   public partial class ProjectControl : UserControl
    {
        //public string ProjectNo { get; set; }

        //public ElectricalProject ElectricalProject { get; set; }

        
        public ProjectControl(string projectNo)
        {
            InitializeComponent();
            var viewModel = new ProjectControlViewModel();
            this.DataContext = viewModel;

            Dictionary<int, string> projectIds = viewModel.database.GetProjectIds(projectNo);

            string projectId = projectIds[1];

            ElectricalTab.Content = new ElectricalProject(projectId, viewModel);
            AdminTab.Content = new Admin();
        }

       
    }
}

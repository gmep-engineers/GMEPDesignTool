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

        ProjectControlViewModel viewModel;
        public ProjectControl(string projectNo)
        {
            InitializeComponent();
            viewModel = new ProjectControlViewModel(projectNo);
            this.DataContext = viewModel;

            //Dictionary<int, string> projectIds = viewModel.database.GetProjectIds(projectNo);

            string projectId = viewModel.ProjectIds[1];

            viewModel.ActiveElectricalProject = new ElectricalProject(projectId, viewModel);

            ElectricalTab.Content =  viewModel.ActiveElectricalProject;
            AdminTab.Content = new Admin();
        }

        private void AddVersion_Click(object sender, RoutedEventArgs e)
        {
            if (VersionComboBox.SelectedItem is KeyValuePair<int, string> selectedPair)
            {
                string projectId = selectedPair.Value;
                viewModel.ProjectIds = viewModel.database.AddProjectVersions(viewModel.ProjectNo, projectId);
                VersionComboBox.SelectedValue = viewModel.ProjectIds.Keys.Last();
            }
        }
        private void DeleteVersion_Click(object sender, RoutedEventArgs e)
        {
            if (VersionComboBox.SelectedItem is KeyValuePair<int, string> selectedPair)
            {
                string projectId = selectedPair.Value;
                viewModel.ProjectIds = viewModel.database.DeleteProjectVersions(viewModel.ProjectNo, projectId);
                VersionComboBox.SelectedValue = viewModel.ProjectIds.Keys.Last();
            }
        }
        private void Version_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VersionComboBox.SelectedItem is KeyValuePair<int, string> selectedPair)
            {
                string newprojectId = selectedPair.Value;
                viewModel.ActiveElectricalProject = new ElectricalProject(newprojectId, viewModel);
                ElectricalTab.Content = viewModel.ActiveElectricalProject;

            }
        }
    }
}

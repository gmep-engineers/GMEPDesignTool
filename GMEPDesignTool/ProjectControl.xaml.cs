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
using System.Windows.Controls.Primitives;
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
using Mysqlx.Crud;
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
        public bool Saving = false;
        public bool Loading = false;
        public string EmployeeId = string.Empty;
        public string SessionId = string.Empty;

        public ProjectControl()
        {
            InitializeComponent();
            //InitializeProject(projectNo);
        }

        public async Task InitializeProject(string projectNo, LoginResponse loginResponse)
        {
            viewModel = new ProjectControlViewModel(projectNo, loginResponse);
            await viewModel.InitializeProjectControlViewModel();
            this.DataContext = viewModel;
            EmployeeId = loginResponse.EmployeeId;
            SessionId = loginResponse.SessionId;
            string projectId = viewModel.ProjectIds.First().Value;
            viewModel.SelectedVersion = viewModel.ProjectIds.First().Key;
            Application.Current.Deactivated += Application_Deactivated;
            Application.Current.Activated += Application_Activated;
            

        }

        private async void AddVersion_Click(object sender, RoutedEventArgs e)
        {
            if (VersionComboBox.SelectedItem is KeyValuePair<int, string> selectedPair)
            {
                string projectId = selectedPair.Value;
                viewModel.ProjectIds = await viewModel.database.AddProjectVersions(
                    viewModel.ProjectNo,
                    projectId
                );
                VersionComboBox.SelectedValue = viewModel.ProjectIds.Keys.Last();
                CopyPopup.IsOpen = false;
            }
        }

        private async void DeleteVersion_Click(object sender, RoutedEventArgs e)
        {
            if (VersionComboBox.SelectedItem is KeyValuePair<int, string> selectedPair)
            {
                string projectId = selectedPair.Value;
                viewModel.ProjectIds = await viewModel.database.DeleteProjectVersions(
                    viewModel.ProjectNo,
                    projectId
                );
                VersionComboBox.SelectedValue = viewModel.ProjectIds.Keys.Last();
                DeletePopup.IsOpen = false;
            }
        }

        private async void Version_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VersionComboBox.SelectedItem is KeyValuePair<int, string> selectedPair)
            {
                var loadingScreen = new LoadingScreen();
                ElectricalTab.Content = loadingScreen;
                //Electrical Tab
                string newprojectId = selectedPair.Value;
                viewModel.ActiveElectricalProject = new ElectricalProject(
                    newprojectId,
                    viewModel,
                    this,
                    EmployeeId,
                    SessionId
                );
                await viewModel.ActiveElectricalProject.InitializeAsync();
                ElectricalTab.Content = viewModel.ActiveElectricalProject;

                //Admin Tab
                AdminTab.Content = new Admin();
                //Plumbing Tab
                
                var plumbingControl = new PlumbingProject("1");
                PlumbingTab.Content = plumbingControl;

            }
        }

        private void Application_Deactivated(object sender, EventArgs e)
        {
            //if (viewModel.SaveText != "LOCKED")
            //{
            //    Save(sender, e);
            //}
        }

        private void Application_Activated(object sender, EventArgs e)
        {
            if (viewModel.SaveText != "LOCKED")
            {
                // Uncomment this to reinstate auto-reload on window focus
                //ReloadElectricalProject();
            }
        }

        private async void Save(object sender, EventArgs e)
        {
            if (!Saving && !Loading)
            {
                Saving = true;
                if (viewModel?.ActiveElectricalProject != null)
                {
                    try
                    {
                        await viewModel.ActiveElectricalProject.SaveProject();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                Saving = false;
            }
        }

        public async void ReloadElectricalProject()
        {
            if (!Loading && !Saving)
            {
                Loading = true;
                if (viewModel?.ActiveElectricalProject != null)
                {
                    var loadingScreen = new LoadingScreen();
                    ElectricalTab.Content = loadingScreen;

                    string saveText = viewModel.SaveText;
                    string projectId = viewModel.ActiveElectricalProject.ProjectId;
                    int sectionIndex = viewModel.ActiveElectricalProject.SectionTabs.SelectedIndex;
                    int equiplightingIndex = viewModel
                        .ActiveElectricalProject
                        .EquipmentLightingTabs
                        .SelectedIndex;
                    int serviceTransPanelIndex = viewModel
                        .ActiveElectricalProject
                        .ServiceTransPanelTabs
                        .SelectedIndex;

                    viewModel.ActiveElectricalProject = new ElectricalProject(
                        projectId,
                        viewModel,
                        this,
                        EmployeeId,
                        SessionId
                    );
                    try
                    {
                        await viewModel.ActiveElectricalProject.InitializeAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    viewModel.ActiveElectricalProject.SectionTabs.SelectedIndex = sectionIndex;
                    viewModel.ActiveElectricalProject.EquipmentLightingTabs.SelectedIndex =
                        equiplightingIndex;
                    viewModel.ActiveElectricalProject.ServiceTransPanelTabs.SelectedIndex =
                        serviceTransPanelIndex;
                    viewModel.SaveText = saveText;
                    ElectricalTab.Content = viewModel.ActiveElectricalProject;
                }
                Loading = false;
            }
        }

        private void ProjectControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)
            {
                Save(sender, e);
            }
        }

        private void Refresh(object sender, RoutedEventArgs e)
        {
            ReloadElectricalProject();
            //Reload Structural
            //Reload Mechanical
            //Reload Plumbing
        }

        private void CopyPopup_Click(object sender, RoutedEventArgs e)
        {
            CopyPopup.IsOpen = true;
        }

        private void CloseCopyPopup_Click(object sender, RoutedEventArgs e)
        {
            CopyPopup.IsOpen = false;
        }

        private void DeletePopup_Click(object sender, RoutedEventArgs e)
        {
            DeletePopup.IsOpen = true;
        }

        private void CloseDeletePopup_Click(object sender, RoutedEventArgs e)
        {
            DeletePopup.IsOpen = false;
        }
    }
}

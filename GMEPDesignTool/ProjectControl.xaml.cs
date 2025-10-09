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
using GMEPDesignTool.Database;
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
    private TabItem TabItem = new TabItem();

    public ProjectControl()
    {
      InitializeComponent();
      //InitializeProject(projectNo);
    }

    private LoginResponse _loginResponse;

    public async Task InitializeProject(string projectNo, LoginResponse loginResponse, TabItem tab)
    {
      viewModel = new ProjectControlViewModel(projectNo, loginResponse);
      await viewModel.InitializeProjectControlViewModel();
      this.DataContext = viewModel;
      TabItem = tab;
      _loginResponse = loginResponse;
      EmployeeId = loginResponse.EmployeeId;
      SessionId = loginResponse.SessionId;

      var loadingScreen = new LoadingScreen();

      ElectricalTab.Content = loadingScreen;

      Dictionary<int, string> projectIds = await viewModel.database.GetProjectIds(projectNo);

      List<string> electricalProjectIds = viewModel.database.GetAllElectricalProjectVersionIds(
        projectNo
      );

      string latestElectricalProjectId = projectIds.First().Value;

      if (electricalProjectIds.Count > 0)
      {
        latestElectricalProjectId = electricalProjectIds.Last();
      }

      viewModel.ActiveElectricalProject = new ElectricalProject(
        projectIds.First().Value,
        latestElectricalProjectId,
        viewModel,
        this,
        EmployeeId,
        SessionId,
        loginResponse
      );

      await viewModel.ActiveElectricalProject.InitializeAsync();
      ElectricalTab.Content = viewModel.ActiveElectricalProject;

      AdminTab.Content = new AdminProject(projectIds.First().Value, loginResponse, tab);

      PlumbingTab.Content = new PlumbingProject(projectIds.First().Value);

      Application.Current.Deactivated += Application_Deactivated;
      Application.Current.Activated += Application_Activated;
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

    public async Task Save()
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

    public async void Save_Click(object sender, EventArgs e)
    {
      await Save();
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
          string electricalProjectId = viewModel.database.GetLatestElectricalProjectId(projectId);
          viewModel.ActiveElectricalProject = new ElectricalProject(
            projectId,
            electricalProjectId,
            viewModel,
            this,
            EmployeeId,
            SessionId,
            _loginResponse
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
        Save();
      }
    }

    public void FocusAdminTab()
    {
      DisciplineTabControl.SelectedIndex = 4;
    }

    private void Refresh(object sender, RoutedEventArgs e)
    {
      ReloadElectricalProject();
      //Reload Structural
      //Reload Mechanical
      //Reload Plumbing
    }
  }
}

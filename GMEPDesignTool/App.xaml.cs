using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using GMEPDesignTool.Properties;
using MySql.Data.MySqlClient;

namespace GMEPDesignTool
{
    public partial class App : Application
    {
        // Import kernel32.dll to allow console window allocation
        //[DllImport("kernel32.dll")]
        //private static extern bool AllocConsole();

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //AllocConsole();

            Console.WriteLine("Console window opened!");
            var vm = new ProposalCommercialViewModel();
            var proposalCommercialWindow = new ProposalCommercialWindow(vm);
            proposalCommercialWindow.Show();


            Console.WriteLine("end");

        }
    }
}
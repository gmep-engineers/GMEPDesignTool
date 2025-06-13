using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using MySql.Data.MySqlClient;

namespace GMEPDesignTool
{
    public partial class App : Application
    {
        // Import kernel32.dll to allow console window allocation
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AllocConsole();

            Console.WriteLine("Console window opened!");
            //string connectionString =
            //    "server=gmep-design-tool-test.ch8c88cauy2x.us-west-1.rds.amazonaws.com;port=3306;user id=admin;password=f51Vu3o5Qlc140r;database=gmep-design-tool;pooling=false";
            //var db = new Database.Database(connectionString);

            //var fixtures = await db.GetPlumbingFixturesByProjectId("1");

            //foreach (var fixture in fixtures)
            //{
            //    Console.WriteLine($"{fixture.Name} -{fixture.Description} - {fixture.HotWater}- {fixture.Model}");
            //}

            Console.WriteLine(new PlumbingProject("0"));

            Console.WriteLine("end");

        }
    }
}

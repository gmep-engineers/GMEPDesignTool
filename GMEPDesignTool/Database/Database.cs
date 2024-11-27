using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Xml.Linq;
using MySql.Data.MySqlClient;

namespace GMEPDesignTool.Database
{
    public class Database
    {
        public string ConnectionString { get; set; }
        public MySqlConnection Connection { get; set; }

        public Database()
        {
            ConnectionString = Properties.Settings.Default.ConnectionString;
            Connection = new MySqlConnection(ConnectionString);
        }

        public void OpenConnection()
        {
            if (Connection.State == System.Data.ConnectionState.Closed)
            {
                Connection.Open();
            }
        }

        public void CloseConnection()
        {
            if (Connection.State == System.Data.ConnectionState.Open)
            {
                Connection.Close();
            }
        }

        public ObservableCollection<ElectricalService> GetProjectServices(string projectName)
        {
            ObservableCollection<ElectricalService> ElectricalServices =
                new ObservableCollection<ElectricalService>();
            string query =
                "SELECT s.* FROM electrical_services s JOIN projects pr ON s.project_id = pr.id WHERE pr.gmep_project_name = @projectName";
            OpenConnection();
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectName", projectName);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                ElectricalServices.Add(
                    new ElectricalService(
                        reader.GetString("id"),
                        reader.GetString("project_id"),
                        reader.GetString("name"),
                        "0",
                        reader.GetInt32("amp")
                    )
                );
            }
            CloseConnection();
            return ElectricalServices;
        }

        public ObservableCollection<ElectricalPanel> GetProjectPanels(string projectName)
        {
            ObservableCollection<ElectricalPanel> ElectricalPanels =
                new ObservableCollection<ElectricalPanel>();
            string query =
                "SELECT ep.* FROM electrical_panels ep JOIN electrical_services s ON ep.fed_from_id = s.id JOIN projects pr ON s.project_id = pr.id WHERE pr.gmep_project_name = @projectName";
            OpenConnection();
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectName", projectName);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                ElectricalPanels.Add(
                    new ElectricalPanel(
                        reader.GetString("id"),
                        reader.GetInt32("bus"),
                        reader.GetInt32("main"),
                        false,
                        reader.GetBoolean("is_distribution"),
                        reader.GetString("name"),
                        reader.GetInt32("color_index"),
                        reader.GetString("fed_from_id"),
                        false
                    )
                );
            }
            CloseConnection();
            return ElectricalPanels;
        }
    }
}

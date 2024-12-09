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

        public string GetProjectId(string projectName)
        {
            string query = "SELECT id FROM projects WHERE gmep_project_name = @projectName";
            OpenConnection();
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectName", projectName);
            MySqlDataReader reader = command.ExecuteReader();

            string id = null;
            if (reader.Read())
            {
                id = reader.GetString("id");
            }
            reader.Close();

            if (id == null)
            {
                // Project name does not exist, insert a new entry with a generated ID
                id = Guid.NewGuid().ToString();
                string insertQuery =
                    "INSERT INTO projects (id, gmep_project_name) VALUES (@id, @projectName)";
                MySqlCommand insertCommand = new MySqlCommand(insertQuery, Connection);
                insertCommand.Parameters.AddWithValue("@id", id);
                insertCommand.Parameters.AddWithValue("@projectName", projectName);
                insertCommand.ExecuteNonQuery();
            }

            CloseConnection();
            return id;
        }

        public void UpdateProject(
            string projectId,
            ObservableCollection<ElectricalService> services,
            ObservableCollection<ElectricalPanel> panels,
            ObservableCollection<ElectricalEquipment> equipments
        )
        {
            OpenConnection();

            // Update Electrical Services
            string selectServicesQuery =
                "SELECT id FROM electrical_services WHERE project_id = @projectId";
            MySqlCommand selectServicesCommand = new MySqlCommand(selectServicesQuery, Connection);
            selectServicesCommand.Parameters.AddWithValue("@projectId", projectId);
            MySqlDataReader servicesReader = selectServicesCommand.ExecuteReader();
            HashSet<string> existingServiceIds = new HashSet<string>();
            while (servicesReader.Read())
            {
                existingServiceIds.Add(servicesReader.GetString("id"));
            }
            servicesReader.Close();

            foreach (var service in services)
            {
                if (existingServiceIds.Contains(service.Id))
                {
                    // Update existing service
                    string updateServiceQuery =
                        "UPDATE electrical_services SET name = @name, electrical_service_amp = @amp, electrical_service_type = @type, electrical_service_meter_config = @config WHERE id = @id";
                    MySqlCommand updateServiceCommand = new MySqlCommand(
                        updateServiceQuery,
                        Connection
                    );
                    updateServiceCommand.Parameters.AddWithValue("@name", service.Name);
                    updateServiceCommand.Parameters.AddWithValue("@amp", service.Amp);
                    updateServiceCommand.Parameters.AddWithValue("@id", service.Id);
                    updateServiceCommand.Parameters.AddWithValue("@type", service.Type);
                    updateServiceCommand.Parameters.AddWithValue("@config", service.Config);
                    updateServiceCommand.ExecuteNonQuery();
                    existingServiceIds.Remove(service.Id);
                }
                else
                {
                    // Insert new service
                    string insertServiceQuery =
                        "INSERT INTO electrical_services (id, project_id, name, electrical_service_amp, electrical_service_type, electrical_service_meter_config) VALUES (@id, @projectId, @name, @amp, @type, @config)";
                    MySqlCommand insertServiceCommand = new MySqlCommand(
                        insertServiceQuery,
                        Connection
                    );
                    insertServiceCommand.Parameters.AddWithValue("@id", service.Id);
                    insertServiceCommand.Parameters.AddWithValue("@projectId", projectId);
                    insertServiceCommand.Parameters.AddWithValue("@name", service.Name);
                    insertServiceCommand.Parameters.AddWithValue("@amp", service.Amp);
                    insertServiceCommand.Parameters.AddWithValue("@type", service.Type);
                    insertServiceCommand.Parameters.AddWithValue("@config", service.Config);

                    insertServiceCommand.ExecuteNonQuery();
                }
            }

            // Remove services that no longer exist
            foreach (var serviceId in existingServiceIds)
            {
                string deleteServiceQuery = "DELETE FROM electrical_services WHERE id = @id";
                MySqlCommand deleteServiceCommand = new MySqlCommand(
                    deleteServiceQuery,
                    Connection
                );
                deleteServiceCommand.Parameters.AddWithValue("@id", serviceId);
                deleteServiceCommand.ExecuteNonQuery();
            }

            // Update Electrical Panels
            string selectPanelsQuery =
                "SELECT id FROM electrical_panels WHERE project_id = @projectId";
            MySqlCommand selectPanelsCommand = new MySqlCommand(selectPanelsQuery, Connection);
            selectPanelsCommand.Parameters.AddWithValue("@projectId", projectId);
            MySqlDataReader panelsReader = selectPanelsCommand.ExecuteReader();
            HashSet<string> existingPanelIds = new HashSet<string>();
            while (panelsReader.Read())
            {
                existingPanelIds.Add(panelsReader.GetString("id"));
            }
            panelsReader.Close();

            foreach (var panel in panels)
            {
                if (existingPanelIds.Contains(panel.Id))
                {
                    // Update existing panel
                    string updatePanelQuery =
                        "UPDATE electrical_panels SET bus = @bus, main = @main, is_distribution = @is_distribution, num_breakers = @numBreakers, distance_from_parent = @distanceFromParent, aic_rating = @aicRating, name = @name, color_index = @color_index, fed_from_id = @fed_from_id WHERE id = @id";
                    MySqlCommand updatePanelCommand = new MySqlCommand(
                        updatePanelQuery,
                        Connection
                    );
                    updatePanelCommand.Parameters.AddWithValue("@bus", panel.BusSize);
                    updatePanelCommand.Parameters.AddWithValue("@main", panel.MainSize);
                    updatePanelCommand.Parameters.AddWithValue(
                        "@is_distribution",
                        panel.IsDistribution
                    );
                    updatePanelCommand.Parameters.AddWithValue("@name", panel.Name);
                    updatePanelCommand.Parameters.AddWithValue("@color_index", panel.ColorIndex);
                    updatePanelCommand.Parameters.AddWithValue("@fed_from_id", panel.FedFromId);
                    updatePanelCommand.Parameters.AddWithValue("@id", panel.Id);
                    updatePanelCommand.Parameters.AddWithValue("@aicRating", panel.AicRating);
                    updatePanelCommand.Parameters.AddWithValue(
                        "@distanceFromParent",
                        panel.DistanceFromParent
                    );
                    updatePanelCommand.Parameters.AddWithValue("@numBreakers", panel.NumBreakers);
                    updatePanelCommand.ExecuteNonQuery();
                    existingPanelIds.Remove(panel.Id);
                }
                else
                {
                    // Insert new panel
                    string insertPanelQuery =
                        "INSERT INTO electrical_panels (id, project_id, bus, main, is_distribution, name, color_index, fed_from_id, num_breakers, distance_from_parent, aic_rating) VALUES (@id, @projectId, @bus, @main, @is_distribution, @name, @color_index, @fed_from_id, @numBreakers, @distanceFromParent, @AicRating)";
                    MySqlCommand insertPanelCommand = new MySqlCommand(
                        insertPanelQuery,
                        Connection
                    );
                    insertPanelCommand.Parameters.AddWithValue("@id", panel.Id);
                    insertPanelCommand.Parameters.AddWithValue("@projectId", projectId);
                    insertPanelCommand.Parameters.AddWithValue("@bus", panel.BusSize);
                    insertPanelCommand.Parameters.AddWithValue("@main", panel.MainSize);
                    insertPanelCommand.Parameters.AddWithValue(
                        "@is_distribution",
                        panel.IsDistribution
                    );
                    insertPanelCommand.Parameters.AddWithValue("@name", panel.Name);
                    insertPanelCommand.Parameters.AddWithValue("@color_index", panel.ColorIndex);
                    insertPanelCommand.Parameters.AddWithValue("@fed_from_id", panel.FedFromId);
                    insertPanelCommand.Parameters.AddWithValue("@AicRating", panel.AicRating);
                    insertPanelCommand.Parameters.AddWithValue(
                        "@distanceFromParent",
                        panel.DistanceFromParent
                    );
                    insertPanelCommand.Parameters.AddWithValue("@numBreakers", panel.NumBreakers);
                    insertPanelCommand.ExecuteNonQuery();
                }
            }

            // Remove panels that no longer exist
            foreach (var panelId in existingPanelIds)
            {
                string deletePanelQuery = "DELETE FROM electrical_panels WHERE id = @id";
                MySqlCommand deletePanelCommand = new MySqlCommand(deletePanelQuery, Connection);
                deletePanelCommand.Parameters.AddWithValue("@id", panelId);
                deletePanelCommand.ExecuteNonQuery();
            }

            // Update Electrical Equipment
            string selectEquipmentsQuery =
                "SELECT id FROM electrical_equipment WHERE project_id = @projectId";
            MySqlCommand selectEquipmentsCommand = new MySqlCommand(
                selectEquipmentsQuery,
                Connection
            );
            selectEquipmentsCommand.Parameters.AddWithValue("@projectId", projectId);
            MySqlDataReader equipmentsReader = selectEquipmentsCommand.ExecuteReader();
            HashSet<string> existingEquipmentIds = new HashSet<string>();
            while (equipmentsReader.Read())
            {
                existingEquipmentIds.Add(equipmentsReader.GetString("id"));
            }
            equipmentsReader.Close();

            foreach (var equipment in equipments)
            {
                if (existingEquipmentIds.Contains(equipment.Id))
                {
                    // Update existing equipment
                    string updateEquipmentQuery =
                        "UPDATE electrical_equipment SET owner_id = @owner, equip_no = @equip_no, qty = @qty, panel_id = @panel_id, voltage = @voltage, amp = @amp, is_three_phase = @is_3ph, spec_sheet_id = @spec_sheet_id, aic_rating = @aic_rating, spec_sheet_from_client = @spec_sheet_from_client WHERE id = @id";
                    MySqlCommand updateEquipmentCommand = new MySqlCommand(
                        updateEquipmentQuery,
                        Connection
                    );
                    updateEquipmentCommand.Parameters.AddWithValue("@owner", equipment.Owner);
                    updateEquipmentCommand.Parameters.AddWithValue("@equip_no", equipment.EquipNo);
                    updateEquipmentCommand.Parameters.AddWithValue("@qty", equipment.Qty);
                    updateEquipmentCommand.Parameters.AddWithValue("@panel_id", equipment.PanelId);
                    updateEquipmentCommand.Parameters.AddWithValue("@voltage", equipment.Voltage);
                    updateEquipmentCommand.Parameters.AddWithValue("@amp", equipment.Amp);
                    updateEquipmentCommand.Parameters.AddWithValue("@is_3ph", equipment.Is3Ph);
                    updateEquipmentCommand.Parameters.AddWithValue(
                        "@spec_sheet_id",
                        equipment.SpecSheetId
                    );
                    updateEquipmentCommand.Parameters.AddWithValue(
                        "@aic_rating",
                        equipment.AicRating
                    );
                    updateEquipmentCommand.Parameters.AddWithValue(
                        "@spec_sheet_from_client",
                        equipment.SpecSheetFromClient
                    );
                    updateEquipmentCommand.Parameters.AddWithValue("@id", equipment.Id);
                    updateEquipmentCommand.ExecuteNonQuery();
                    existingEquipmentIds.Remove(equipment.Id);
                }
                else
                {
                    // Insert new equipment
                    string insertEquipmentQuery =
                        "INSERT INTO electrical_equipment (id, project_id, owner_id, equip_no, qty, panel_id, voltage, amp, is_three_phase, spec_sheet_id, aic_rating, spec_sheet_from_client) VALUES (@id, @projectId, @owner, @equip_no, @qty, @panel_id, @voltage, @amp, @is_3ph, @spec_sheet_id, @aic_rating, @spec_sheet_from_client)";
                    MySqlCommand insertEquipmentCommand = new MySqlCommand(
                        insertEquipmentQuery,
                        Connection
                    );
                    insertEquipmentCommand.Parameters.AddWithValue("@id", equipment.Id);
                    insertEquipmentCommand.Parameters.AddWithValue("@projectId", projectId);
                    insertEquipmentCommand.Parameters.AddWithValue("@owner", equipment.Owner);
                    insertEquipmentCommand.Parameters.AddWithValue("@equip_no", equipment.EquipNo);
                    insertEquipmentCommand.Parameters.AddWithValue("@qty", equipment.Qty);
                    insertEquipmentCommand.Parameters.AddWithValue("@panel_id", equipment.PanelId);
                    insertEquipmentCommand.Parameters.AddWithValue("@voltage", equipment.Voltage);
                    insertEquipmentCommand.Parameters.AddWithValue("@amp", equipment.Amp);
                    insertEquipmentCommand.Parameters.AddWithValue("@is_3ph", equipment.Is3Ph);
                    insertEquipmentCommand.Parameters.AddWithValue(
                        "@spec_sheet_id",
                        equipment.SpecSheetId
                    );
                    insertEquipmentCommand.Parameters.AddWithValue(
                        "@aic_rating",
                        equipment.AicRating
                    );
                    insertEquipmentCommand.Parameters.AddWithValue(
                        "@spec_sheet_from_client",
                        equipment.SpecSheetFromClient
                    );
                    insertEquipmentCommand.ExecuteNonQuery();
                }
            }

            // Remove equipment that no longer exist
            foreach (var equipmentId in existingEquipmentIds)
            {
                string deleteEquipmentQuery = "DELETE FROM electrical_equipment WHERE id = @id";
                MySqlCommand deleteEquipmentCommand = new MySqlCommand(
                    deleteEquipmentQuery,
                    Connection
                );
                deleteEquipmentCommand.Parameters.AddWithValue("@id", equipmentId);
                deleteEquipmentCommand.ExecuteNonQuery();
            }

            CloseConnection();
        }

        public ObservableCollection<ElectricalService> GetProjectServices(string projectId)
        {
            ObservableCollection<ElectricalService> services =
                new ObservableCollection<ElectricalService>();
            string query = "SELECT * FROM electrical_services WHERE project_id = @projectId";
            OpenConnection();
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectId", projectId);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                services.Add(
                    new ElectricalService(
                        reader.GetString("id"),
                        reader.GetString("project_id"),
                        reader.GetString("name"),
                        reader.GetInt32("electrical_service_type"),
                        reader.GetInt32("electrical_service_amp"),
                        reader.GetString("electrical_service_meter_config")
                    )
                );
            }
            reader.Close();
            CloseConnection();
            return services;
        }

        public ObservableCollection<ElectricalPanel> GetProjectPanels(string projectId)
        {
            ObservableCollection<ElectricalPanel> panels =
                new ObservableCollection<ElectricalPanel>();
            string query = "SELECT * FROM electrical_panels WHERE project_id = @projectId";
            OpenConnection();
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectId", projectId);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                panels.Add(
                    new ElectricalPanel(
                        reader.GetString("id"),
                        reader.GetString("project_id"),
                        reader.GetInt32("bus"),
                        reader.GetInt32("main"),
                        false,
                        reader.GetBoolean("is_distribution"),
                        reader.GetString("name"),
                        reader.GetInt32("color_index"),
                        reader.GetString("fed_from_id"),
                        reader.GetInt32("num_breakers"),
                        reader.GetInt32("distance_from_parent"),
                        reader.GetInt32("aic_rating")
                    )
                );
            }
            reader.Close();
            CloseConnection();
            return panels;
        }

        public ObservableCollection<ElectricalEquipment> GetProjectEquipment(string projectId)
        {
            ObservableCollection<ElectricalEquipment> equipments =
                new ObservableCollection<ElectricalEquipment>();
            string query = "SELECT * FROM electrical_equipment WHERE project_id = @projectId";
            OpenConnection();
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectId", projectId);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                equipments.Add(
                    new ElectricalEquipment(
                        reader.GetString("id"),
                        reader.GetString("project_id"),
                        reader.GetString("owner_id"),
                        reader.GetString("equip_no"),
                        reader.GetInt32("qty"),
                        reader.GetString("panel_id"),
                        reader.GetInt32("voltage"),
                        reader.GetFloat("amp"),
                        reader.GetInt32("voltage") * reader.GetFloat("amp"),
                        reader.GetBoolean("is_three_phase"),
                        reader.GetString("spec_sheet_id"),
                        reader.GetInt32("aic_rating"),
                        reader.GetBoolean("spec_sheet_from_client")
                    )
                );
            }
            reader.Close();
            CloseConnection();
            return equipments;
        }
    }
}

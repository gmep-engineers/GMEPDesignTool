using System;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Xml.Linq;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using BCrypt.Net;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;

namespace GMEPDesignTool.Database
{
    public class Database
    {
        public string ConnectionString { get; set; }
        public MySqlConnection Connection { get; set; }

        public Database(string sqlConnectionString)
        {
            ConnectionString = sqlConnectionString;
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

        public async Task OpenConnectionAsync()
        {
            if (Connection.State == System.Data.ConnectionState.Closed)
            {
                await Connection.OpenAsync();
            }
        }

        public async Task CloseConnectionAsync()
        {
            if (Connection.State == System.Data.ConnectionState.Open)
            {
                await Connection.CloseAsync();
            }
        }

        string GetSafeString(MySqlDataReader reader, string fieldName)
        {
            int index = reader.GetOrdinal(fieldName);
            if (!reader.IsDBNull(index))
            {
                return reader.GetString(index);
            }
            return string.Empty;
        }

        DateTime? GetUnsafeDate(MySqlDataReader reader, string fieldName)
        {
            int index = reader.GetOrdinal(fieldName);
            if (!reader.IsDBNull(index))
            {
                return reader.GetDateTime(index);
            }
            return null;
        }

        int GetSafeInt(MySqlDataReader reader, string fieldName)
        {
            int index = reader.GetOrdinal(fieldName);
            if (!reader.IsDBNull(index))
            {
                return reader.GetInt32(index);
            }
            return 0;
        }

        uint? GetUnsafeUInt(MySqlDataReader reader, string fieldName)
        {
            int index = reader.GetOrdinal(fieldName);
            if (!reader.IsDBNull(index))
            {
                return reader.GetUInt32(index);
            }
            return null;
        }

        ulong? GetUnsafeULong(MySqlDataReader reader, string fieldName)
        {
            int index = reader.GetOrdinal(fieldName);
            if (!reader.IsDBNull(index))
            {
                return reader.GetUInt64(index);
            }
            return null;
        }

        float GetSafeFloat(MySqlDataReader reader, string fieldName)
        {
            int index = reader.GetOrdinal(fieldName);
            if (!reader.IsDBNull(index))
            {
                return reader.GetFloat(index);
            }
            return 0;
        }

        bool GetSafeBoolean(MySqlDataReader reader, string fieldName)
        {
            int index = reader.GetOrdinal(fieldName);
            if (!reader.IsDBNull(index))
            {
                return reader.GetBoolean(index);
            }
            return false;
        }

        public bool LoginUser(string userName, string password)
        {
            string query =
                @"
            SELECT e.passhash
            FROM employees e 
            WHERE e.username = @username";
            OpenConnection();
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@username", userName);
            MySqlDataReader reader = command.ExecuteReader();

            string hashedPassword = "";
            bool result = false;
            if (reader.Read())
            {
                hashedPassword = reader.GetString("passhash");
                result = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            CloseConnection();
            return result;
        }

        public List<Employee> GetEmployees()
        {
            List<Employee> employees = new List<Employee>();
            string query =
                @"
                SELECT 
                employees.id as employee_id,
                contacts.id as contact_id,
                entities.id as entity_id,
                last_name,
                first_name,
                email_address,
                email_addresses.id as email_address_id,
                phone_number,
                phone_numbers.id as phone_number_id,
                extension,
                hire_date,
                termination_date,
                employee_department_id,
                employee_title_id,
                username
                FROM employees
                LEFT JOIN contacts ON contacts.id = employees.contact_id
                LEFT JOIN entities ON contacts.entity_id = entities.id
                LEFT JOIN email_addr_entity_rel ON email_addr_entity_rel.entity_id = entities.id
                LEFT JOIN email_addresses ON email_addr_entity_rel.email_address_id = email_addresses.id
                LEFT JOIN phone_number_entity_rel ON phone_number_entity_rel.entity_id = entities.id
                LEFT JOIN phone_numbers ON phone_numbers.id = phone_number_entity_rel.phone_number_id
                ORDER BY last_name ASC
                ";
            OpenConnection();
            MySqlCommand command = new MySqlCommand(query, Connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                employees.Add(
                    new Employee(
                        GetSafeString(reader, "employee_id"),
                        GetSafeString(reader, "contact_id"),
                        GetSafeString(reader, "entity_id"),
                        GetSafeString(reader, "last_name"),
                        GetSafeString(reader, "first_name"),
                        GetSafeInt(reader, "employee_title_id"),
                        GetSafeInt(reader, "employee_department_id"),
                        GetSafeString(reader, "email_address"),
                        GetSafeString(reader, "email_address_id"),
                        GetUnsafeULong(reader, "phone_number"),
                        GetSafeString(reader, "phone_number_id"),
                        GetUnsafeUInt(reader, "extension"),
                        GetUnsafeDate(reader, "hire_date"),
                        GetUnsafeDate(reader, "termination_date"),
                        GetSafeString(reader, "username")
                    )
                );
            }
            CloseConnection();
            return employees;
        }

        public void SaveEmployee(Employee employee)
        {
            Trace.WriteLine(employee.FirstName);
            OpenConnection();
            var query =
                @"
                    UPDATE employees SET
                    employee_title_id = @titleId,
                    employee_department_id = @departmentId,
                    hire_date = @hireDate,
                    termination_date = @terminationDate,
                    username = @username
                    WHERE id = @id
                    ";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@titleId", employee.TitleId);
            command.Parameters.AddWithValue("@departmentId", employee.DepartmentId);
            command.Parameters.AddWithValue("@hireDate", employee.HireDate);
            if (employee.TerminationDate == DateTime.MinValue)
            {
                command.Parameters.AddWithValue("@terminationDate", null);
            }
            else
            {
                command.Parameters.AddWithValue("@terminationDate", employee.TerminationDate);
            }
            command.Parameters.AddWithValue("@username", employee.Username);
            command.Parameters.AddWithValue("@id", employee.Id);
            command.ExecuteNonQuery();
            if (employee.NewEmailAddress)
            {
                string emailAddressId = Guid.NewGuid().ToString();
                string emailAddressRelId = Guid.NewGuid().ToString();
                query =
                    @"
                    INSERT INTO email_addresses (id, email_address)
                    VALUES (@id, @emailAddress)
                    ";
                command = new MySqlCommand(query, Connection);
                command.Parameters.AddWithValue("@id", emailAddressId);
                command.Parameters.AddWithValue("@emailAddress", employee.EmailAddress);
                command.ExecuteNonQuery();
                query =
                    @"
                    INSERT INTO email_addresses_entity_rel (id, email_address_id, entity_id, is_primary)
                    VALUES (@id, @emailAddressId, @entityId, 1)
                    ";
                command = new MySqlCommand(query, Connection);
                command.Parameters.AddWithValue("@id", emailAddressRelId);
                command.Parameters.AddWithValue("@emailAddressId", emailAddressId);
                command.Parameters.AddWithValue("@entityId", employee.EntityId);
                command.ExecuteNonQuery();
                employee.EmailAddressId = emailAddressId;
                employee.NewEmailAddress = false;
            }
            else
            {
                query =
                    @"
                    UPDATE email_addresses
                    SET email_address = @emailAddress
                    WHERE id = @emailAddressId
                    ";
                command = new MySqlCommand(query, Connection);
                command.Parameters.AddWithValue("@emailAddress", employee.EmailAddress);
                command.Parameters.AddWithValue("@emailAddressId", employee.EmailAddressId);
                command.ExecuteNonQuery();
            }
            if (employee.NewPhoneNumber)
            {
                string phoneNumberId = Guid.NewGuid().ToString();
                string phoneNumberRelId = Guid.NewGuid().ToString();
                query =
                    @"
                    INSERT INTO phone_numbers (id, phone_number, extension, calling_code)
                    VALUES (@id, @phoneNumber, @extension, 1)
                    ";
                command = new MySqlCommand(query, Connection);
                command.Parameters.AddWithValue("@id", phoneNumberId);
                command.Parameters.AddWithValue("@phoneNumber", employee.PhoneNumber);
                command.Parameters.AddWithValue(
                    "@extension",
                    employee.Extension == 0 ? null : employee.Extension
                );
                command.ExecuteNonQuery();
                query =
                    @"
                    INSERT INTO phone_number_entity_rel (id, phone_number_id, entity_id, is_primary)
                    VALUES (@id, @phoneNumberId, @entityId, 1)
                    ";
                command = new MySqlCommand(query, Connection);
                command.Parameters.AddWithValue("@id", phoneNumberRelId);
                command.Parameters.AddWithValue("@phoneNumberId", phoneNumberId);
                command.Parameters.AddWithValue("@entityId", employee.EntityId);
                command.ExecuteNonQuery();
                employee.PhoneNumberId = phoneNumberId;
                employee.NewPhoneNumber = false;
            }
            else
            {
                query =
                    @"
                    UPDATE phone_numbers
                    SET phone_number = @phoneNumber
                    WHERE id = @phoneNumberId
                    ";
                command = new MySqlCommand(query, Connection);
                command.Parameters.AddWithValue("@phoneNumber", employee.PhoneNumber);
                command.Parameters.AddWithValue("@phoneNumberId", employee.PhoneNumberId);
                command.ExecuteNonQuery();
            }
            query =
                @"
                    UPDATE contacts
                    SET
                    first_name = @firstName,
                    last_name = @lastName
                    WHERE id = @contactId
                    ";
            command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@firstName", employee.FirstName);
            command.Parameters.AddWithValue("@lastName", employee.LastName);
            command.Parameters.AddWithValue("@contactId", employee.ContactId);
            command.ExecuteNonQuery();

            CloseConnection();
        }

        public void SetEmployeePassword(string employeeId, string password)
        {
            string query = "UPDATE employees SET passhash = @passhash WHERE id = @id";
            string salt = BCrypt.Net.BCrypt.GenerateSalt(10);
            string passhash = BCrypt.Net.BCrypt.HashPassword(password, salt);
            OpenConnection();
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@passhash", passhash);
            command.Parameters.AddWithValue("@id", employeeId);
            command.ExecuteNonQuery();
            CloseConnection();
        }

        public async Task<Dictionary<int, string>> GetProjectIds(string projectNo)
        {
            string query = "SELECT id, version FROM projects WHERE gmep_project_no = @projectNo";
            await OpenConnectionAsync();
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectNo", projectNo);
            MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();

            Dictionary<int, string> projectIds = new Dictionary<int, string>();
            while (reader.Read())
            {
                projectIds.Add(reader.GetInt32("version"), reader.GetString("id"));
            }
            await reader.CloseAsync();

            if (!projectIds.Any())
            {
                // Project name does not exist, insert a new entry with a generated ID
                var id = Guid.NewGuid().ToString();
                string insertQuery =
                    "INSERT INTO projects (id, gmep_project_no) VALUES (@id, @projectNo)";
                MySqlCommand insertCommand = new MySqlCommand(insertQuery, Connection);
                insertCommand.Parameters.AddWithValue("@id", id);
                insertCommand.Parameters.AddWithValue("@projectNo", projectNo);
                await insertCommand.ExecuteNonQueryAsync();
                projectIds.Add(1, id);
            }

            await CloseConnectionAsync();
            projectIds = projectIds.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            return projectIds;
        }

        public async Task<Dictionary<int, string>> AddProjectVersions(
            string projectNo,
            string projectId
        )
        {
            string query = "SELECT id, version FROM projects WHERE gmep_project_no = @projectNo";
            await OpenConnectionAsync();
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectNo", projectNo);
            MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();

            Dictionary<int, string> projectIds = new Dictionary<int, string>();
            while (await reader.ReadAsync())
            {
                projectIds.Add(reader.GetInt32("version"), reader.GetString("id"));
            }
            await reader.CloseAsync();

            if (!projectIds.Any())
            {
                // Project name does not exist, insert a new entry with a generated ID
                var id = Guid.NewGuid().ToString();
                string insertQuery =
                    "INSERT INTO projects (id, gmep_project_no) VALUES (@id, @projectNo)";
                MySqlCommand insertCommand = new MySqlCommand(insertQuery, Connection);
                insertCommand.Parameters.AddWithValue("@id", id);
                insertCommand.Parameters.AddWithValue("@projectNo", projectNo);
                await insertCommand.ExecuteNonQueryAsync();
                projectIds.Add(1, id);
            }
            else
            {
                var id = Guid.NewGuid().ToString();
                projectIds = projectIds.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                string insertQuery =
                    "INSERT INTO projects (id, gmep_project_no, version) VALUES (@id, @projectNo, @version)";
                MySqlCommand insertCommand = new MySqlCommand(insertQuery, Connection);
                insertCommand.Parameters.AddWithValue("@id", id);
                insertCommand.Parameters.AddWithValue("@projectNo", projectNo);
                insertCommand.Parameters.AddWithValue("@version", projectIds.Last().Key + 1);
                await insertCommand.ExecuteNonQueryAsync();
                await CloneElectricalProject(projectId, id);
                projectIds.Add(projectIds.Last().Key + 1, id);
            }

            CloseConnection();
            return projectIds;
        }

        public async Task<Dictionary<int, string>> DeleteProjectVersions(
            string projectNo,
            string projectId
        )
        {
            await OpenConnectionAsync();
            string[] tables = new string[]
            {
                "electrical_panels",
                "electrical_transformers",
                "electrical_equipment",
                "electrical_lighting_locations",
                "electrical_lighting",
                "electrical_services",
                "panel_notes",
                "custom_circuits",
            };
            string query;
            MySqlCommand command;
            foreach (string table in tables)
            {
                query = $"DELETE FROM {table} WHERE project_id = @projectId";
                command = new MySqlCommand(query, Connection);
                command.Parameters.AddWithValue("@projectId", projectId);
                await command.ExecuteNonQueryAsync();
            }

            query = "DELETE FROM projects WHERE id = @projectId";
            command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectId", projectId);
            await command.ExecuteNonQueryAsync();

            query = "SELECT id, version FROM projects WHERE gmep_project_no = @projectNo";
            command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectNo", projectNo);
            Dictionary<int, string> projectIds = new Dictionary<int, string>();
            MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                projectIds.Add(reader.GetInt32("version"), reader.GetString("id"));
            }
            await reader.CloseAsync();

            if (!projectIds.Any())
            {
                // Project name does not exist, insert a new entry with a generated ID
                var id = Guid.NewGuid().ToString();
                string insertQuery =
                    "INSERT INTO projects (id, gmep_project_no) VALUES (@id, @projectNo)";
                MySqlCommand insertCommand = new MySqlCommand(insertQuery, Connection);
                insertCommand.Parameters.AddWithValue("@id", id);
                insertCommand.Parameters.AddWithValue("@projectNo", projectNo);
                await insertCommand.ExecuteNonQueryAsync();
                projectIds.Add(1, id);
            }
            await CloseConnectionAsync();
            return projectIds;
        }

        public async Task<Dictionary<string, string>> getOwners()
        {
            var owners = new Dictionary<string, string>();

            try
            {
                await OpenConnectionAsync();

                string query = "SELECT id, name FROM owners";

                using (MySqlCommand cmd = new MySqlCommand(query, Connection))
                {
                    using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            string id = reader.GetString("id");
                            string name = reader.GetString("name");
                            owners.Add(id, name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            finally
            {
                await CloseConnectionAsync();
            }

            return owners;
        }

        //Update Project Functions
        public async Task UpdateProject(
            string projectId,
            ObservableCollection<ElectricalService> services,
            ObservableCollection<ElectricalPanel> panels,
            ObservableCollection<ElectricalEquipment> equipments,
            ObservableCollection<ElectricalTransformer> transformers,
            ObservableCollection<ElectricalLighting> lightings,
            ObservableCollection<Location> locations,
            ObservableCollection<Note> panelNotes,
            ObservableCollection<Circuit> customCircuits
        )
        {
            await OpenConnectionAsync();

            await UpdateServices(projectId, services);
            await UpdatePanels(projectId, panels);
            await UpdateEquipments(projectId, equipments);
            await UpdateTransformers(projectId, transformers);
            await UpdateLightings(projectId, lightings);
            await UpdateLightingLocations(projectId, locations);
            await UpdatePanelNotes(projectId, panelNotes);
            await UpdateCustomCircuits(projectId, customCircuits);

            await CloseConnectionAsync();
        }

        private async Task UpdateServices(
            string projectId,
            ObservableCollection<ElectricalService> services
        )
        {
            var existingServiceIds = await GetExistingIds(
                "electrical_services",
                "project_id",
                projectId
            );

            foreach (var service in services)
            {
                if (existingServiceIds.Contains(service.Id))
                {
                    await UpdateService(service);
                    existingServiceIds.Remove(service.Id);
                }
                else
                {
                    await InsertService(projectId, service);
                }
            }

            await DeleteRemovedItems("electrical_services", existingServiceIds);
        }

        private async Task UpdatePanels(
            string projectId,
            ObservableCollection<ElectricalPanel> panels
        )
        {
            var existingPanelIds = await GetExistingIds(
                "electrical_panels",
                "project_id",
                projectId
            );

            foreach (var panel in panels)
            {
                if (existingPanelIds.Contains(panel.Id))
                {
                    await UpdatePanel(panel);
                    existingPanelIds.Remove(panel.Id);
                }
                else
                {
                    await InsertPanel(projectId, panel);
                }
            }

            await DeleteRemovedItems("electrical_panels", existingPanelIds);
        }

        private async Task UpdatePanelNotes(string projectId, ObservableCollection<Note> panelNotes)
        {
            var existingPanelIds = await GetExistingIds("panel_notes", "project_id", projectId);

            var panelNotesCopy = panelNotes.ToList(); // Create a copy of the collection

            foreach (var note in panelNotesCopy)
            {
                if (existingPanelIds.Contains(note.Id))
                {
                    await UpdatePanelNote(note);
                    existingPanelIds.Remove(note.Id);
                }
                else
                {
                    await InsertPanelNote(projectId, note);
                }
            }

            await DeleteRemovedItems("panel_notes", existingPanelIds);
        }
        private async Task UpdateCustomCircuits(string projectId, ObservableCollection<Circuit> customCircuits)
        {
            var existingPanelIds = await GetExistingIds("custom_circuits", "project_id", projectId);

            var customCircuitsCopy = customCircuits.ToList(); // Create a copy of the collection

            foreach (var circuit in customCircuitsCopy)
            {
                if (existingPanelIds.Contains(circuit.Id))
                {
                    await UpdateCustomCircuit(circuit);
                    existingPanelIds.Remove(circuit.Id);
                }
                else
                {
                    await InsertCustomCircuit(projectId, circuit);
                }
            }

            await DeleteRemovedItems("custom_circuits", existingPanelIds);
        }


        private async Task UpdateTransformers(
            string projectId,
            ObservableCollection<ElectricalTransformer> transformers
        )
        {
            var existingTransformerIds = await GetExistingIds(
                "electrical_transformers",
                "project_id",
                projectId
            );

            foreach (var transformer in transformers)
            {
                if (existingTransformerIds.Contains(transformer.Id))
                {
                    await UpdateTransformer(transformer);
                    existingTransformerIds.Remove(transformer.Id);
                }
                else
                {
                    await InsertTransformer(projectId, transformer);
                }
            }

            await DeleteRemovedItems("electrical_transformers", existingTransformerIds);
        }

        private async Task UpdateEquipments(
            string projectId,
            ObservableCollection<ElectricalEquipment> equipments
        )
        {
            var existingEquipmentIds = await GetExistingIds(
                "electrical_equipment",
                "project_id",
                projectId
            );

            foreach (var equipment in equipments)
            {
                if (existingEquipmentIds.Contains(equipment.Id))
                {
                    await UpdateEquipment(equipment);
                    existingEquipmentIds.Remove(equipment.Id);
                }
                else
                {
                    await InsertEquipment(projectId, equipment);
                }
            }

            await DeleteRemovedItems("electrical_equipment", existingEquipmentIds);
        }

        private async Task UpdateLightings(
            string projectId,
            ObservableCollection<ElectricalLighting> lightings
        )
        {
            var existingLightingIds = await GetExistingIds(
                "electrical_lighting",
                "project_id",
                projectId
            );

            foreach (var lighting in lightings)
            {
                if (existingLightingIds.Contains(lighting.Id))
                {
                    await UpdateLighting(lighting);
                    existingLightingIds.Remove(lighting.Id);
                }
                else
                {
                    await InsertLighting(projectId, lighting);
                }
            }

            await DeleteRemovedItems("electrical_lighting", existingLightingIds);
        }

        private async Task UpdateLightingLocations(
            string projectId,
            ObservableCollection<Location> locations
        )
        {
            var existingLocationIds = await GetExistingIds(
                "electrical_lighting_locations",
                "project_id",
                projectId
            );

            foreach (var location in locations)
            {
                if (existingLocationIds.Contains(location.Id))
                {
                    await UpdateLocation(location);
                    existingLocationIds.Remove(location.Id);
                }
                else
                {
                    await InsertLocation(projectId, location);
                }
            }

            await DeleteRemovedItems("electrical_lighting_locations", existingLocationIds);
        }

        private async Task<HashSet<string>> GetExistingIds(
            string tableName,
            string columnName,
            string projectId
        )
        {
            var idType = "id";

            string query = $"SELECT {idType} FROM {tableName} WHERE {columnName} = @projectId";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectId", projectId);
            MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
            HashSet<string> ids = new HashSet<string>();
            while (await reader.ReadAsync())
            {
                ids.Add(reader.GetString($"{idType}"));
            }
            await reader.CloseAsync();
            return ids;
        }

        private async Task UpdateService(ElectricalService service)
        {
            string query =
                "UPDATE electrical_services SET name = @name, electrical_service_amp_rating_id = @amp, electrical_service_voltage_id = @type, electrical_service_meter_config_id = @config, color_code = @color_code, aic_rating = @aicRating, parent_id = @parentId WHERE id = @id";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@name", service.Name);
            command.Parameters.AddWithValue("@amp", service.Amp);
            command.Parameters.AddWithValue("@id", service.Id);
            command.Parameters.AddWithValue("@type", service.Type);
            command.Parameters.AddWithValue("@config", service.Config);
            command.Parameters.AddWithValue("@color_code", service.ColorCode);
            command.Parameters.AddWithValue("@aicRating", service.AicRating);
            command.Parameters.AddWithValue("@parentId", service.ParentId);
            await command.ExecuteNonQueryAsync();
        }

        private async Task InsertService(string projectId, ElectricalService service)
        {
            string query =
                "INSERT INTO electrical_services (id, project_id, name, electrical_service_amp_rating_id, electrical_service_voltage_id, electrical_service_meter_config_id, color_code, aic_rating, parent_id) VALUES (@id, @projectId, @name, @amp, @type, @config, @color_code, @aicRating, @parentId)";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", service.Id);
            command.Parameters.AddWithValue("@projectId", projectId);
            command.Parameters.AddWithValue("@name", service.Name);
            command.Parameters.AddWithValue("@amp", service.Amp);
            command.Parameters.AddWithValue("@type", service.Type);
            command.Parameters.AddWithValue("@config", service.Config);
            command.Parameters.AddWithValue("@color_code", service.ColorCode);
            command.Parameters.AddWithValue("@aicRating", service.AicRating);
            command.Parameters.AddWithValue("@parentId", service.ParentId);
            await command.ExecuteNonQueryAsync();
        }

        private async Task UpdatePanel(ElectricalPanel panel)
        {
            string query =
                "UPDATE electrical_panels SET bus_amp_rating_id = @bus, main_amp_rating_id = @main, is_distribution = @is_distribution, voltage_id = @type, num_breakers = @numBreakers, parent_distance = @distanceFromParent, aic_rating = @aicRating, name = @name, color_code = @color_code, parent_id = @parent_id, is_recessed = @is_recessed, is_mlo = @is_mlo, circuit_no = @circuit_no, is_hidden_on_plan = @is_hidden_on_plan, location = @location, high_leg_phase = @highLegPhase, load_amperage = @amp, kva = @kva WHERE id = @id";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@bus", panel.BusSize);
            command.Parameters.AddWithValue("@main", panel.MainSize);
            command.Parameters.AddWithValue("@is_distribution", panel.IsDistribution);
            command.Parameters.AddWithValue("@name", panel.Name);
            command.Parameters.AddWithValue("@color_code", panel.ColorCode);
            command.Parameters.AddWithValue("@parent_id", panel.ParentId);
            command.Parameters.AddWithValue("@id", panel.Id);
            command.Parameters.AddWithValue("@aicRating", panel.AicRating);
            command.Parameters.AddWithValue("@distanceFromParent", panel.DistanceFromParent);
            command.Parameters.AddWithValue("@numBreakers", panel.NumBreakers);
            command.Parameters.AddWithValue("@type", panel.Type);
            command.Parameters.AddWithValue("@is_recessed", panel.IsRecessed);
            command.Parameters.AddWithValue("@is_mlo", panel.IsMlo);
            command.Parameters.AddWithValue("@circuit_no", panel.CircuitNo);
            command.Parameters.AddWithValue("@is_hidden_on_plan", panel.IsHiddenOnPlan);
            command.Parameters.AddWithValue("@location", panel.Location);
            command.Parameters.AddWithValue("@highLegPhase", panel.HighLegPhase);
            command.Parameters.AddWithValue("@amp", panel.Amp);
            command.Parameters.AddWithValue("@kva", panel.Kva);
            await command.ExecuteNonQueryAsync();
        }

        private async Task InsertPanel(string projectId, ElectricalPanel panel)
        {
            string query =
                "INSERT INTO electrical_panels (id, project_id, bus_amp_rating_id, main_amp_rating_id, is_distribution, name, color_code, parent_id, num_breakers, parent_distance, aic_rating, voltage_id, is_recessed, is_mlo, circuit_no, is_hidden_on_plan, location, high_leg_phase, load_amperage, kva) VALUES (@id, @projectId, @bus, @main, @is_distribution, @name, @color_code, @parent_id, @numBreakers, @distanceFromParent, @AicRating, @type, @is_recessed, @is_mlo, @circuit_no, @is_hidden_on_plan, @location, @highLegPhase, @amp, @kva)";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", panel.Id);
            command.Parameters.AddWithValue("@projectId", projectId);
            command.Parameters.AddWithValue("@bus", panel.BusSize);
            command.Parameters.AddWithValue("@main", panel.MainSize);
            command.Parameters.AddWithValue("@is_distribution", panel.IsDistribution);
            command.Parameters.AddWithValue("@name", panel.Name);
            command.Parameters.AddWithValue("@color_code", panel.ColorCode);
            command.Parameters.AddWithValue("@parent_id", panel.ParentId);
            command.Parameters.AddWithValue("@AicRating", panel.AicRating);
            command.Parameters.AddWithValue("@distanceFromParent", panel.DistanceFromParent);
            command.Parameters.AddWithValue("@numBreakers", panel.NumBreakers);
            command.Parameters.AddWithValue("@type", panel.Type);
            command.Parameters.AddWithValue("@is_recessed", panel.IsRecessed);
            command.Parameters.AddWithValue("@is_mlo", panel.IsMlo);
            command.Parameters.AddWithValue("@circuit_no", panel.CircuitNo);
            command.Parameters.AddWithValue("@is_hidden_on_plan", panel.IsHiddenOnPlan);
            command.Parameters.AddWithValue("@location", panel.Location);
            command.Parameters.AddWithValue("@highLegPhase", panel.HighLegPhase);
            command.Parameters.AddWithValue("@amp", panel.Amp);
            command.Parameters.AddWithValue("@kva", panel.Kva);
            await command.ExecuteNonQueryAsync();
        }

        private async Task UpdatePanelNote(Note panelNotes)
        {
            string query =
                "UPDATE panel_notes SET number = @number, circuit_no = @circuitNo, length = @length, description = @description, stack = @stack WHERE id = @id";
            MySqlCommand command = new MySqlCommand(query, Connection);

            command.Parameters.AddWithValue("@id", panelNotes.Id);
            command.Parameters.AddWithValue("@number", panelNotes.Number);
            command.Parameters.AddWithValue("@circuitNo", panelNotes.CircuitNo);
            command.Parameters.AddWithValue("@length", panelNotes.Length);
            command.Parameters.AddWithValue("@description", panelNotes.Description);
            command.Parameters.AddWithValue("@stack", panelNotes.Stack);

            await command.ExecuteNonQueryAsync();
        }

        private async Task InsertPanelNote(string projectId, Note panelNotes)
        {
            string query =
                "INSERT INTO panel_notes (id, number, panel_id, project_id, circuit_no, length, description, group_id, stack) VALUES (@id, @number, @panelId, @projectId, @circuitNo, @length, @description, @groupId, @stack)";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", panelNotes.Id);
            command.Parameters.AddWithValue("@number", panelNotes.Number);
            command.Parameters.AddWithValue("@panelId", panelNotes.PanelId);
            command.Parameters.AddWithValue("@projectId", panelNotes.ProjectId);
            command.Parameters.AddWithValue("@circuitNo", panelNotes.CircuitNo);
            command.Parameters.AddWithValue("@length", panelNotes.Length);
            command.Parameters.AddWithValue("@description", panelNotes.Description);
            command.Parameters.AddWithValue("@groupId", panelNotes.GroupId);
            command.Parameters.AddWithValue("@stack", panelNotes.Stack);
            await command.ExecuteNonQueryAsync();
        }

        private async Task UpdateCustomCircuit(Circuit customCircuit)
        {
            string query =
                "UPDATE custom_circuits SET number = @number, breaker_size = @breakerSize, description = @description, load_category = @loadCategory, va = @va, custom_breaker_size = @customBreakerSize, custom_description = @customDescription WHERE id = @id";
            MySqlCommand command = new MySqlCommand(query, Connection);

            command.Parameters.AddWithValue("@id", customCircuit.Id);
            command.Parameters.AddWithValue("@number", customCircuit.Number);
            command.Parameters.AddWithValue("@breakerSize", customCircuit.BreakerSize);
            command.Parameters.AddWithValue("@loadCategory", customCircuit.LoadCategory);
            command.Parameters.AddWithValue("@description", customCircuit.Description);
            command.Parameters.AddWithValue("@va", customCircuit.Va);
            command.Parameters.AddWithValue("@customBreakerSize", customCircuit.CustomBreakerSize);
            command.Parameters.AddWithValue("@customDescription", customCircuit.CustomDescription);

            await command.ExecuteNonQueryAsync();
        }

        private async Task InsertCustomCircuit(string projectId, Circuit customCircuit)
        {
            string query =
                "INSERT INTO custom_circuits (id, panel_id, project_id, number, breaker_size, description, load_category, va, custom_breaker_size, custom_description) VALUES (@id, @panelId, @projectId, @number, @breakerSize, @description, @loadCategory, @va, @customBreakerSize, @customDescription)";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", customCircuit.Id);
            command.Parameters.AddWithValue("@projectId", projectId);
            command.Parameters.AddWithValue("@panelId", customCircuit.PanelId);
            command.Parameters.AddWithValue("@number", customCircuit.Number);
            command.Parameters.AddWithValue("@breakerSize", customCircuit.BreakerSize);
            command.Parameters.AddWithValue("@loadCategory", customCircuit.LoadCategory);
            command.Parameters.AddWithValue("@description", customCircuit.Description);
            command.Parameters.AddWithValue("@va", customCircuit.Va);
            command.Parameters.AddWithValue("@customBreakerSize", customCircuit.CustomBreakerSize);
            command.Parameters.AddWithValue("@customDescription", customCircuit.CustomDescription);
            await command.ExecuteNonQueryAsync();
        }


        private async Task UpdateEquipment(ElectricalEquipment equipment)
        {
            string query =
                "UPDATE electrical_equipment SET description = @description, equip_no = @equip_no, parent_id = @parent_id, owner_id = @owner, voltage_id = @voltage, fla = @fla, is_three_phase = @is_3ph, spec_sheet_id = @spec_sheet_id, aic_rating = @aic_rating, spec_sheet_from_client = @spec_sheet_from_client, parent_distance=@distanceFromParent, category_id=@category, color_code = @color_code, connection_type_id = @connection, mca = @mca, hp = @hp, has_plug = @has_plug, locking_connector = @locking_connector, width=@width, depth=@depth, height=@height, circuit_no=@circuit_no, is_hidden_on_plan=@is_hidden_on_plan, load_type = @loadType, order_no = @order_no, va=@va WHERE id = @id";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", equipment.Id);
            command.Parameters.AddWithValue("@equip_no", equipment.EquipNo);
            command.Parameters.AddWithValue("@parent_id", equipment.ParentId);
            command.Parameters.AddWithValue("@voltage", equipment.Voltage);
            command.Parameters.AddWithValue("@fla", equipment.Fla);
            command.Parameters.AddWithValue("@is_3ph", equipment.Is3Ph);
            command.Parameters.AddWithValue("@spec_sheet_id", equipment.SpecSheetId);
            command.Parameters.AddWithValue("@aic_rating", equipment.AicRating);
            command.Parameters.AddWithValue("@description", equipment.Description);
            command.Parameters.AddWithValue(
                "@spec_sheet_from_client",
                equipment.SpecSheetFromClient
            );
            command.Parameters.AddWithValue("@distanceFromParent", equipment.DistanceFromParent);
            command.Parameters.AddWithValue("@category", equipment.Category);
            command.Parameters.AddWithValue("@color_code", equipment.ColorCode);
            command.Parameters.AddWithValue("@owner", equipment.Owner);
            command.Parameters.AddWithValue("@connection", equipment.Connection);
            command.Parameters.AddWithValue("@mca", equipment.McaId);
            command.Parameters.AddWithValue("@hp", equipment.Hp);
            command.Parameters.AddWithValue("@has_plug", equipment.HasPlug);
            command.Parameters.AddWithValue("@locking_connector", equipment.LockingConnector);
            command.Parameters.AddWithValue("@width", equipment.Width);
            command.Parameters.AddWithValue("@depth", equipment.Depth);
            command.Parameters.AddWithValue("@height", equipment.Height);
            command.Parameters.AddWithValue("@circuit_no", equipment.CircuitNo);
            command.Parameters.AddWithValue("@is_hidden_on_plan", equipment.IsHiddenOnPlan);
            command.Parameters.AddWithValue("@loadType", equipment.LoadType);
            command.Parameters.AddWithValue("@order_no", equipment.OrderNo);
            command.Parameters.AddWithValue("@va", equipment.Va);
            await command.ExecuteNonQueryAsync();
        }

        private async Task InsertEquipment(string projectId, ElectricalEquipment equipment)
        {
            string query =
                "INSERT INTO electrical_equipment (id, project_id, equip_no, parent_id, owner_id, voltage_id, fla, is_three_phase, spec_sheet_id, aic_rating, spec_sheet_from_client, parent_distance, category_id, color_code, connection_type_id, description, mca, hp, has_plug, locking_connector, width, depth, height, circuit_no, is_hidden_on_plan, load_type, order_no, va) VALUES (@id, @projectId, @equip_no, @parent_id, @owner, @voltage, @fla, @is_3ph, @spec_sheet_id, @aic_rating, @spec_sheet_from_client, @distanceFromParent, @category, @color_code, @connection, @description, @mca, @hp, @has_plug, @locking_connector, @width, @depth, @height, @circuit_no, @is_hidden_on_plan, @loadType, @order_no, @va)";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", equipment.Id);
            command.Parameters.AddWithValue("@projectId", projectId);
            command.Parameters.AddWithValue("@owner", equipment.Owner);
            command.Parameters.AddWithValue("@equip_no", equipment.EquipNo);
            command.Parameters.AddWithValue("@parent_id", equipment.ParentId);
            command.Parameters.AddWithValue("@voltage", equipment.Voltage);
            command.Parameters.AddWithValue("@fla", equipment.Fla);
            command.Parameters.AddWithValue("@is_3ph", equipment.Is3Ph);
            command.Parameters.AddWithValue("@spec_sheet_id", equipment.SpecSheetId);
            command.Parameters.AddWithValue("@aic_rating", equipment.AicRating);
            command.Parameters.AddWithValue(
                "@spec_sheet_from_client",
                equipment.SpecSheetFromClient
            );
            command.Parameters.AddWithValue("@distanceFromParent", equipment.DistanceFromParent);
            command.Parameters.AddWithValue("@category", equipment.Category);
            command.Parameters.AddWithValue("@color_code", equipment.ColorCode);
            command.Parameters.AddWithValue("@connection", equipment.Connection);
            command.Parameters.AddWithValue("@description", equipment.Description);
            command.Parameters.AddWithValue("@mca", equipment.McaId);
            command.Parameters.AddWithValue("@hp", equipment.Hp);
            command.Parameters.AddWithValue("@has_plug", equipment.HasPlug);
            command.Parameters.AddWithValue("@locking_connector", equipment.LockingConnector);
            command.Parameters.AddWithValue("@width", equipment.Width);
            command.Parameters.AddWithValue("@depth", equipment.Depth);
            command.Parameters.AddWithValue("@height", equipment.Height);
            command.Parameters.AddWithValue("@circuit_no", equipment.CircuitNo);
            command.Parameters.AddWithValue("@is_hidden_on_plan", equipment.IsHiddenOnPlan);
            command.Parameters.AddWithValue("@loadType", equipment.LoadType);
            command.Parameters.AddWithValue("@order_no", equipment.OrderNo);
            command.Parameters.AddWithValue("@va", equipment.Va);
            await command.ExecuteNonQueryAsync();
        }

        private async Task UpdateLighting(ElectricalLighting lighting)
        {
            string query =
                "UPDATE electrical_lighting SET notes = @notes, model_no = @model_no, parent_id = @parent_id, voltage_id = @voltageId, color_code = @colorCode, mounting_type_id = @mountingType, occupancy=@occupancy, manufacturer = @manufacturer, wattage = @wattage, em_capable = @em_capable, tag = @tag, symbol_id = @symbolId, description=@description, driver_type_id = @driverTypeId, spec_sheet_from_client=@specFromClient, spec_sheet_id=@specSheetId, qty = @qty, has_photocell = @hasPhotoCell, location_id = @locationId WHERE id = @id";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@model_no", lighting.ModelNo);
            command.Parameters.AddWithValue("@parent_id", lighting.ParentId);
            command.Parameters.AddWithValue("@id", lighting.Id);
            command.Parameters.AddWithValue("@manufacturer", lighting.Manufacturer);
            command.Parameters.AddWithValue("@occupancy", lighting.Occupancy);
            command.Parameters.AddWithValue("@wattage", lighting.Wattage);
            command.Parameters.AddWithValue("@em_capable", lighting.EmCapable);
            command.Parameters.AddWithValue("@mountingType", lighting.MountingType);
            command.Parameters.AddWithValue("@tag", lighting.Tag);
            command.Parameters.AddWithValue("@notes", lighting.Notes);
            command.Parameters.AddWithValue("@voltageId", lighting.VoltageId);
            command.Parameters.AddWithValue("@symbolId", lighting.SymbolId);
            command.Parameters.AddWithValue("@colorCode", lighting.colorCode);
            command.Parameters.AddWithValue("@description", lighting.Description);
            command.Parameters.AddWithValue("@driverTypeId", lighting.DriverTypeId);
            command.Parameters.AddWithValue("@specFromClient", lighting.SpecSheetFromClient);
            command.Parameters.AddWithValue("@specSheetId", lighting.SpecSheetId);
            command.Parameters.AddWithValue("@qty", lighting.Qty);
            command.Parameters.AddWithValue("@hasPhotoCell", lighting.HasPhotoCell);
            command.Parameters.AddWithValue("@locationId", lighting.LocationId);

            await command.ExecuteNonQueryAsync();
        }

        private async Task InsertLighting(string projectId, ElectricalLighting lighting)
        {
            string query =
                "INSERT INTO electrical_lighting (id, project_id, notes, model_no, parent_id, voltage_id, color_code, mounting_type_id, occupancy, manufacturer, wattage, em_capable, tag, symbol_id, description, driver_type_id, spec_sheet_from_client, spec_sheet_id, qty, has_photocell, location_id) VALUES (@id, @project_id, @notes, @model_no, @parent_id, @voltageId, @colorCode, @mountingType, @occupancy, @manufacturer, @wattage, @em_capable, @tag, @symbolId, @description, @driverTypeId, @specFromClient, @specSheetId, @qty, @hasPhotoCell, @locationId)";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", lighting.Id);
            command.Parameters.AddWithValue("@project_id", projectId);
            command.Parameters.AddWithValue("@model_no", lighting.ModelNo);
            command.Parameters.AddWithValue("@parent_id", lighting.ParentId);
            command.Parameters.AddWithValue("@manufacturer", lighting.Manufacturer);
            command.Parameters.AddWithValue("@occupancy", lighting.Occupancy);
            command.Parameters.AddWithValue("@wattage", lighting.Wattage);
            command.Parameters.AddWithValue("@em_capable", lighting.EmCapable);
            command.Parameters.AddWithValue("@mountingType", lighting.MountingType);
            command.Parameters.AddWithValue("@tag", lighting.Tag);
            command.Parameters.AddWithValue("@notes", lighting.Notes);
            command.Parameters.AddWithValue("@voltageId", lighting.VoltageId);
            command.Parameters.AddWithValue("@symbolId", lighting.SymbolId);
            command.Parameters.AddWithValue("@colorCode", lighting.colorCode);
            command.Parameters.AddWithValue("@description", lighting.Description);
            command.Parameters.AddWithValue("@driverTypeId", lighting.DriverTypeId);
            command.Parameters.AddWithValue("@specFromClient", lighting.SpecSheetFromClient);
            command.Parameters.AddWithValue("@specSheetId", lighting.SpecSheetId);
            command.Parameters.AddWithValue("@qty", lighting.Qty);
            command.Parameters.AddWithValue("@hasPhotoCell", lighting.HasPhotoCell);
            command.Parameters.AddWithValue("@locationId", lighting.LocationId);
            await command.ExecuteNonQueryAsync();
        }

        private async Task UpdateTransformer(ElectricalTransformer transformer)
        {
            string query =
                "UPDATE electrical_transformers SET parent_id = @parent_id, voltage_id = @voltage, project_id = @project_id, kva_id = @kva, parent_distance = @distanceFromParent, color_code = @color_code, name = @name, circuit_no = @circuitNo, is_hidden_on_plan = @is_hidden_on_plan, is_wall_mounted = @isWallMounted, aic_rating = @aicRating WHERE id = @id";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@parent_id", transformer.ParentId);
            command.Parameters.AddWithValue("@id", transformer.Id);
            command.Parameters.AddWithValue("@project_id", transformer.ProjectId);
            command.Parameters.AddWithValue("@voltage", transformer.Voltage);
            command.Parameters.AddWithValue("@distanceFromParent", transformer.DistanceFromParent);
            command.Parameters.AddWithValue("@kva", transformer.Kva);
            command.Parameters.AddWithValue("@color_code", transformer.ColorCode);
            command.Parameters.AddWithValue("@name", transformer.Name);
            command.Parameters.AddWithValue("@circuitNo", transformer.CircuitNo);
            command.Parameters.AddWithValue("@is_hidden_on_plan", transformer.IsHiddenOnPlan);
            command.Parameters.AddWithValue("@isWallMounted", transformer.IsWallMounted);
            command.Parameters.AddWithValue("@aicRating", transformer.AicRating);
            await command.ExecuteNonQueryAsync();
        }

        private async Task InsertTransformer(string projectId, ElectricalTransformer transformer)
        {
            string query =
                "INSERT INTO electrical_transformers (id, project_id, parent_id, voltage_id, parent_distance, color_code, kva_id, name, circuit_no, is_hidden_on_plan, is_wall_mounted, aic_rating) VALUES (@id, @project_id, @parent_id, @voltage, @distanceFromParent, @color_code, @kva, @name, @circuitNo, @isHiddenOnPlan, @isWallMounted, @aicRating)";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", transformer.Id);
            command.Parameters.AddWithValue("@project_id", transformer.ProjectId);
            command.Parameters.AddWithValue("@parent_id", transformer.ParentId);
            command.Parameters.AddWithValue("@distanceFromParent", transformer.DistanceFromParent);
            command.Parameters.AddWithValue("@color_code", transformer.ColorCode);
            command.Parameters.AddWithValue("@kva", transformer.Kva);
            command.Parameters.AddWithValue("@name", transformer.Name);
            command.Parameters.AddWithValue("@voltage", transformer.Voltage);
            command.Parameters.AddWithValue("@circuitNo", transformer.CircuitNo);
            command.Parameters.AddWithValue("@isHiddenOnPlan", transformer.IsHiddenOnPlan);
            command.Parameters.AddWithValue("@isWallMounted", transformer.IsWallMounted);
            command.Parameters.AddWithValue("@aicRating", transformer.AicRating);
            await command.ExecuteNonQueryAsync();
        }

        private async Task UpdateLocation(Location location)
        {
            string query =
                "UPDATE electrical_lighting_locations SET location = @locationDescription, outdoor = @isOutside WHERE id = @id";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", location.Id);
            command.Parameters.AddWithValue("@locationDescription", location.LocationDescription);
            command.Parameters.AddWithValue("@isOutside", location.IsOutside);

            await command.ExecuteNonQueryAsync();
        }

        private async Task InsertLocation(string projectId, Location location)
        {
            string query =
                "INSERT INTO electrical_lighting_locations (id, project_id, location, outdoor) VALUES (@id, @projectId, @locationDescription, @isOutside)";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", location.Id);
            command.Parameters.AddWithValue("@projectId", projectId);
            command.Parameters.AddWithValue("@locationDescription", location.LocationDescription);
            command.Parameters.AddWithValue("@isOutside", location.IsOutside);

            await command.ExecuteNonQueryAsync();
        }

        private async Task DeleteRemovedItems(string tableName, HashSet<string> ids)
        {
            var idType = "id";

            foreach (var id in ids)
            {
                string query = $"DELETE FROM {tableName} WHERE {idType} = @id";
                MySqlCommand command = new MySqlCommand(query, Connection);
                command.Parameters.AddWithValue("@id", id);
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<ObservableCollection<ElectricalService>> GetProjectServices(
            string projectId
        )
        {
            ObservableCollection<ElectricalService> services =
                new ObservableCollection<ElectricalService>();
            string query = "SELECT * FROM electrical_services WHERE project_id = @projectId";
            await OpenConnectionAsync();
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectId", projectId);
            MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                services.Add(
                    new ElectricalService(
                        reader.GetString("id"),
                        reader.GetString("project_id"),
                        reader.GetString("name"),
                        reader.GetInt32("electrical_service_voltage_id"),
                        reader.GetInt32("electrical_service_amp_rating_id"),
                        reader.GetInt32("electrical_service_meter_config_id"),
                        reader.GetString("color_code"),
                        reader.GetInt32("aic_rating"),
                        reader.GetString("parent_id")
                    )
                );
            }
            await reader.CloseAsync();
            await CloseConnectionAsync();
            return services;
        }

        public async Task<ObservableCollection<ElectricalPanel>> GetProjectPanels(string projectId)
        {
            ObservableCollection<ElectricalPanel> panels =
                new ObservableCollection<ElectricalPanel>();
            string query = "SELECT * FROM electrical_panels WHERE project_id = @projectId";
            await OpenConnectionAsync();
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectId", projectId);
            MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                panels.Add(
                    new ElectricalPanel(
                        reader.GetString("id"),
                        reader.GetString("project_id"),
                        reader.IsDBNull(reader.GetOrdinal("bus_amp_rating_id"))
                            ? 0
                            : reader.GetInt32("bus_amp_rating_id"),
                        reader.IsDBNull(reader.GetOrdinal("main_amp_rating_id"))
                            ? 0
                            : reader.GetInt32("main_amp_rating_id"),
                        reader.GetBoolean("is_mlo"),
                        reader.GetBoolean("is_distribution"),
                        reader.GetString("name"),
                        reader.GetString("color_code"),
                        reader.GetString("parent_id"),
                        reader.IsDBNull(reader.GetOrdinal("num_breakers"))
                            ? 0
                            : reader.GetInt32("num_breakers"),
                        reader.IsDBNull(reader.GetOrdinal("parent_distance"))
                            ? 0
                            : reader.GetInt32("parent_distance"),
                        reader.IsDBNull(reader.GetOrdinal("aic_rating"))
                            ? 0
                            : reader.GetInt32("aic_rating"),
                        reader.GetFloat("load_amperage"),
                        reader.GetFloat("kva"),
                        reader.IsDBNull(reader.GetOrdinal("voltage_id"))
                            ? 0
                            : reader.GetInt32("voltage_id"),
                        false,
                        reader.GetBoolean("is_recessed"),
                        reader.IsDBNull(reader.GetOrdinal("circuit_no"))
                            ? 0
                            : reader.GetInt32("circuit_no"),
                        reader.GetBoolean("is_hidden_on_plan"),
                        reader.GetString("location"),
                        reader.GetChar("high_leg_phase")
                    )
                );
            }
            await reader.CloseAsync();
            await CloseConnectionAsync();
            return panels;
        }

        public async Task<ObservableCollection<Note>> GetProjectPanelNotes(string projectId)
        {
            ObservableCollection<Note> panelNotes = new ObservableCollection<Note>();
            string query = "SELECT * FROM panel_notes WHERE project_id = @projectId";
            await OpenConnectionAsync();
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectId", projectId);
            MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                Note note = new Note();
                note.Id = reader.GetString("id");
                note.Number = reader.GetInt32("number");
                note.PanelId = reader.GetString("panel_id");
                note.ProjectId = reader.GetString("project_id");
                note.circuitNo = reader.GetInt32("circuit_no");
                note.length = reader.GetInt32("length");
                note.Description = reader.GetString("description");
                note.GroupId = reader.GetString("group_id");
                note.Stack = reader.GetInt32("stack");
                panelNotes.Add(note);
            }
            await reader.CloseAsync();
            await CloseConnectionAsync();
            return panelNotes;
        }
        public async Task<ObservableCollection<Circuit>> GetProjectCustomCircuits(string projectId)
        {
            ObservableCollection<Circuit> customCircuits = new ObservableCollection<Circuit>();
            string query = "SELECT * FROM custom_circuits WHERE project_id = @projectId";
            await OpenConnectionAsync();
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectId", projectId);
            MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                customCircuits.Add(
                    new Circuit(
                     reader.GetString("id"),
                     reader.GetString("panel_id"),
                     reader.GetString("project_id"),
                     reader.GetInt32("number"),
                     reader.GetInt32("va"),
                     reader.GetInt32("breaker_size"),
                     reader.GetString("description"),
                     reader.GetInt32("load_category"),
                     reader.GetBoolean("custom_breaker_size"),
                     reader.GetBoolean("custom_description")
                    )
                );
            await reader.CloseAsync();
            await CloseConnectionAsync();
            return customCircuits;
        }

        public async Task<ObservableCollection<ElectricalEquipment>> GetProjectEquipment(
            string projectId
        )
        {
            ObservableCollection<ElectricalEquipment> equipments =
                new ObservableCollection<ElectricalEquipment>();
            string query =
                "SELECT * FROM electrical_equipment WHERE project_id = @projectId ORDER BY order_no";
            await OpenConnectionAsync();
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectId", projectId);
            MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                equipments.Add(
                    new ElectricalEquipment(
                        GetSafeString(reader, "id"),
                        GetSafeString(reader, "project_id"),
                        GetSafeString(reader, "owner_id"),
                        GetSafeString(reader, "equip_no"),
                        0,
                        GetSafeString(reader, "parent_id"),
                        GetSafeInt(reader, "voltage_id"),
                        GetSafeFloat(reader, "fla"),
                        GetSafeInt(reader, "va"),
                        GetSafeBoolean(reader, "is_three_phase"),
                        GetSafeString(reader, "spec_sheet_id"),
                        GetSafeInt(reader, "aic_rating"),
                        GetSafeBoolean(reader, "spec_sheet_from_client"),
                        GetSafeInt(reader, "parent_distance"),
                        GetSafeInt(reader, "category_id"),
                        GetSafeString(reader, "color_code"),
                        false,
                        GetSafeInt(reader, "connection_type_id"),
                        GetSafeString(reader, "description"),
                        GetSafeInt(reader, "mca"),
                        GetSafeString(reader, "hp"),
                        GetSafeBoolean(reader, "has_plug"),
                        GetSafeBoolean(reader, "locking_connector"),
                        GetSafeFloat(reader, "width"),
                        GetSafeFloat(reader, "depth"),
                        GetSafeFloat(reader, "height"),
                        GetSafeInt(reader, "circuit_no"),
                        GetSafeBoolean(reader, "is_hidden_on_plan"),
                        GetSafeInt(reader, "load_type"),
                        GetSafeInt(reader, "order_no")
                    )
                );

            await reader.CloseAsync();
            await CloseConnectionAsync();
            return equipments;

            float idToVoltage(int voltageId)
            {
                int voltage = 0;
                switch (voltageId)
                {
                    case (1):
                        voltage = 115;
                        break;
                    case (2):
                        voltage = 120;
                        break;
                    case (3):
                        voltage = 208;
                        break;
                    case (4):
                        voltage = 230;
                        break;
                    case (5):
                        voltage = 240;
                        break;
                    case (6):
                        voltage = 277;
                        break;
                    case (7):
                        voltage = 460;
                        break;
                    case (8):
                        voltage = 480;
                        break;
                }
                return voltage;
            }
        }

        public async Task<ObservableCollection<ElectricalLighting>> GetProjectLighting(
            string projectId
        )
        {
            ObservableCollection<ElectricalLighting> lightings =
                new ObservableCollection<ElectricalLighting>();
            string query = "SELECT * FROM electrical_lighting WHERE project_id = @projectId";
            await OpenConnectionAsync();
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectId", projectId);
            MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                lightings.Add(
                    new ElectricalLighting(
                        reader.GetString("id"),
                        reader.GetString("project_id"),
                        reader.GetString("parent_id"),
                        reader.GetString("manufacturer"),
                        reader.GetString("model_no"),
                        reader.IsDBNull(reader.GetOrdinal("qty")) ? 0 : reader.GetInt32("qty"),
                        reader.IsDBNull(reader.GetOrdinal("occupancy"))
                            ? false
                            : reader.GetBoolean("occupancy"),
                        reader.IsDBNull(reader.GetOrdinal("wattage"))
                            ? 0
                            : reader.GetInt32("wattage"),
                        reader.IsDBNull(reader.GetOrdinal("em_capable"))
                            ? false
                            : reader.GetBoolean("em_capable"),
                        reader.IsDBNull(reader.GetOrdinal("mounting_type_id"))
                            ? 0
                            : reader.GetInt32("mounting_type_id"),
                        reader.GetString("tag"),
                        reader.GetString("notes"),
                        reader.IsDBNull(reader.GetOrdinal("voltage_id"))
                            ? 0
                            : reader.GetInt32("voltage_id"),
                        reader.IsDBNull(reader.GetOrdinal("symbol_id"))
                            ? 0
                            : reader.GetInt32("symbol_id"),
                        reader.GetString("color_code"),
                        false,
                        reader.GetString("description"),
                        reader.IsDBNull(reader.GetOrdinal("driver_type_id"))
                            ? 0
                            : reader.GetInt32("driver_type_id"),
                        reader.IsDBNull(reader.GetOrdinal("spec_sheet_from_client"))
                            ? false
                            : reader.GetBoolean("spec_sheet_from_client"),
                        reader.GetString("spec_sheet_id"),
                        reader.IsDBNull(reader.GetOrdinal("has_photocell"))
                            ? false
                            : reader.GetBoolean("has_photocell"),
                        reader.GetString("location_id")
                    )
                );
            }

            await reader.CloseAsync();
            await CloseConnectionAsync();
            return lightings;
        }

        public async Task<ObservableCollection<ElectricalTransformer>> GetProjectTransformers(
            string projectId
        )
        {
            ObservableCollection<ElectricalTransformer> transformers =
                new ObservableCollection<ElectricalTransformer>();
            string query = "SELECT * FROM electrical_transformers WHERE project_id = @projectId";
            await OpenConnectionAsync();
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectId", projectId);
            MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                transformers.Add(
                    new ElectricalTransformer(
                        reader.GetString("id"),
                        reader.GetString("project_id"),
                        reader.GetString("parent_id"),
                        reader.IsDBNull(reader.GetOrdinal("parent_distance")) ? 0 : reader.GetInt32("parent_distance"),
                        reader.GetString("color_code"),
                        reader.IsDBNull(reader.GetOrdinal("voltage_id")) ? 0 : reader.GetInt32("voltage_id"),
                        reader.GetString("name"),
                        reader.IsDBNull(reader.GetOrdinal("kva_id")) ? 0 : reader.GetInt32("kva_id"),
                        false,
                        reader.IsDBNull(reader.GetOrdinal("circuit_no")) ? 0 : reader.GetInt32("circuit_no"),
                        reader.GetBoolean("is_hidden_on_plan"),
                        reader.GetBoolean("is_wall_mounted"),
                        reader.IsDBNull(reader.GetOrdinal("aic_rating")) ? 0 : reader.GetInt32("aic_rating")
                    )
                );
            }
            await reader.CloseAsync();
            await CloseConnectionAsync();
            return transformers;
        }

        public async Task<ObservableCollection<Location>> GetLightingLocations(string projectId)
        {
            ObservableCollection<Location> locations = new ObservableCollection<Location>();
            string query =
                "SELECT * FROM electrical_lighting_locations WHERE project_id = @projectId";
            await OpenConnectionAsync();
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectId", projectId);
            MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var location = new Location();
                location.Id = reader.GetString("id");
                location.isOutside = reader.GetBoolean("outdoor");
                location.LocationDescription = reader.GetString("location");
                locations.Add(location);
            }
            await reader.CloseAsync();
            await CloseConnectionAsync();
            return locations;
        }

        public async Task CloneElectricalProject(string projectId, string newProjectId)
        {
            var services = await GetProjectServices(projectId);
            var panels = await GetProjectPanels(projectId);
            var equipments = await GetProjectEquipment(projectId);
            var lightings = await GetProjectLighting(projectId);
            var transformers = await GetProjectTransformers(projectId);
            var locations = await GetLightingLocations(projectId);
            var panelNotes = await GetProjectPanelNotes(projectId);
            var customCircuits = await GetProjectCustomCircuits(projectId);

            Dictionary<string, string> parentIdSwitch = new Dictionary<string, string>();
            Dictionary<string, string> locationIdSwitch = new Dictionary<string, string>();
            Dictionary<string, string> panelNoteIdSwitch = new Dictionary<string, string>();

            foreach (var service in services)
            {
                string Id = Guid.NewGuid().ToString();
                parentIdSwitch.Add(service.Id, Id);
                service.Id = Id;
                service.ProjectId = newProjectId;
            }
            foreach (var panel in panels)
            {
                string Id = Guid.NewGuid().ToString();
                parentIdSwitch.Add(panel.Id, Id);
                panel.Id = Id;
                panel.ProjectId = newProjectId;
            }
            foreach (var transformer in transformers)
            {
                string Id = Guid.NewGuid().ToString();
                parentIdSwitch.Add(transformer.Id, Id);
                transformer.Id = Id;
                transformer.ProjectId = newProjectId;
            }
            foreach (var equipment in equipments)
            {
                string Id = Guid.NewGuid().ToString();
                equipment.Id = Id;
                equipment.ProjectId = newProjectId;
            }
            foreach (var location in locations)
            {
                string Id = Guid.NewGuid().ToString();
                locationIdSwitch.Add(location.Id, Id);
                location.Id = Id;
            }
            foreach (var lighting in lightings)
            {
                string Id = Guid.NewGuid().ToString();
                lighting.Id = Id;
                lighting.ProjectId = newProjectId;
                if (!string.IsNullOrEmpty(lighting.LocationId) && lighting.LocationId != "0")
                {
                    lighting.LocationId = locationIdSwitch[lighting.LocationId];
                }
            }
            foreach (var note in panelNotes)
            {
                string Id = Guid.NewGuid().ToString();
                note.Id = Id;
                note.projectId = newProjectId;
                note.PanelId = parentIdSwitch[note.PanelId];
            }
            foreach (var circuit in customCircuits)
            {
                string Id = Guid.NewGuid().ToString();
                circuit.Id = Id;
                circuit.projectId = newProjectId;
                circuit.PanelId = parentIdSwitch[circuit.PanelId];
            }

            foreach (var panel in panels)
            {
                if (!string.IsNullOrEmpty(panel.parentId))
                {
                    panel.ParentId = parentIdSwitch[panel.ParentId];
                }
            }
            foreach (var service in services)
            {
                if (!string.IsNullOrEmpty(service.parentId))
                {
                    service.ParentId = parentIdSwitch[service.ParentId];
                }
            }
            foreach (var transformer in transformers)
            {
                if (!string.IsNullOrEmpty(transformer.parentId))
                {
                    transformer.ParentId = parentIdSwitch[transformer.ParentId];
                }
            }
            foreach (var equipment in equipments)
            {
                if (!string.IsNullOrEmpty(equipment.parentId))
                {
                    equipment.ParentId = parentIdSwitch[equipment.ParentId];
                }
            }
            await UpdateProject(
                newProjectId,
                services,
                panels,
                equipments,
                transformers,
                lightings,
                locations,
                panelNotes,
                customCircuits
            );
        }
    }

    public class S3
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3()
        {
            _bucketName = Properties.Settings.Default.bucket_name;
            _s3Client = new AmazonS3Client(
                Properties.Settings.Default.aws_access_key_id,
                Properties.Settings.Default.aws_secret_access_key,
                RegionEndpoint.USEast1
            );
        }

        public async Task UploadFileAsync(string keyName, string filePath)
        {
            try
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = keyName,
                    FilePath = filePath,
                    ContentType = "application/pdf",
                };

                PutObjectResponse response = await _s3Client.PutObjectAsync(putRequest);
                Console.WriteLine("File uploaded successfully.");
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine(
                    "Error encountered on server. Message:'{0}' when writing an object",
                    e.Message
                );
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    "Unknown encountered on server. Message:'{0}' when writing an object",
                    e.Message
                );
            }
        }

        public async Task DownloadAndOpenFileAsync(string keyName, string downloadFilePath)
        {
            try
            {
                var getRequest = new GetObjectRequest { BucketName = _bucketName, Key = keyName };

                using (GetObjectResponse response = await _s3Client.GetObjectAsync(getRequest))
                using (Stream responseStream = response.ResponseStream)
                using (
                    FileStream fileStream = new FileStream(
                        downloadFilePath,
                        FileMode.Create,
                        FileAccess.Write
                    )
                )
                {
                    await responseStream.CopyToAsync(fileStream);
                    Console.WriteLine("File downloaded successfully.");
                }

                Process.Start(
                    new ProcessStartInfo { FileName = downloadFilePath, UseShellExecute = true }
                );
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine(
                    "Error encountered on server. Message:'{0}' when reading an object.",
                    e.Message
                );
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    "Unknown encountered on server. Message:'{0}' when reading an object.",
                    e.Message
                );
            }
        }

        public async Task DeleteFileAsync(string keyName)
        {
            try
            {
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = keyName,
                };

                DeleteObjectResponse response = await _s3Client.DeleteObjectAsync(deleteRequest);
                Console.WriteLine("File deleted successfully.");
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine(
                    "Error encountered on server. Message:'{0}' when deleting an object",
                    e.Message
                );
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    "Unknown encountered on server. Message:'{0}' when deleting an object",
                    e.Message
                );
            }
        }
    }
}

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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Xml.Linq;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using BCrypt.Net;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using Org.BouncyCastle.Crypto.Generators;

namespace GMEPDesignTool.Database
{
    public class Database
    {
        public string ConnectionString { get; set; }
        public MySqlConnection Connection { get; set; }

        public MySqlConnection SessionConnection { get; set; }

        public Database(string sqlConnectionString)
        {
            ConnectionString = sqlConnectionString;
            Connection = new MySqlConnection(ConnectionString);
            SessionConnection = new MySqlConnection(ConnectionString);
        }

        public void OpenConnection(MySqlConnection conn)
        {
            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }
        }

        public void CloseConnection(MySqlConnection conn)
        {
            if (conn.State == System.Data.ConnectionState.Open)
            {
                conn.Close();
            }
        }

        public async Task OpenConnectionAsync(MySqlConnection conn)
        {
            if (conn.State == System.Data.ConnectionState.Closed)
            {
                await conn.OpenAsync();
            }
        }

        public async Task CloseConnectionAsync(MySqlConnection conn)
        {
            if (conn.State == System.Data.ConnectionState.Open)
            {
                await conn.CloseAsync();
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

        char GetSafeChar(MySqlDataReader reader, string fieldName)
        {
            int index = reader.GetOrdinal(fieldName);
            if (!reader.IsDBNull(index))
            {
                return reader.GetChar(index);
            }
            return char.MinValue;
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

        DateTime GetSafeDateTime(MySqlDataReader reader, string fieldName)
        {
            int index = reader.GetOrdinal(fieldName);
            if (!reader.IsDBNull(index))
            {
                return reader.GetDateTime(index);
            }
            return DateTime.MinValue;
        }


        public async Task<AdminModel> GetAdminByProjectNumber(string projectNo)
        {
            AdminModel adminModel = null;
            string query = @"
                        SELECT gmep_project_no,
                        gmep_project_name,
                        street_address,
                        city,
                        state,
                        postal_code, 
                        directory 
                        FROM projects WHERE gmep_project_no = @projectNo";
            await OpenConnectionAsync(Connection);
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectNo", projectNo);
            using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    adminModel = new AdminModel
                    {
                        ProjectNo = reader["gmep_project_no"]?.ToString(),
                        ProjectName = reader["gmep_project_name"]?.ToString(),
                        StreetAddress = reader["street_address"]?.ToString(),
                        City = reader["city"]?.ToString(),
                        State = reader["state"]?.ToString(),
                        PostalCode = reader["postal_code"]?.ToString(),
                        Directory = reader["directory"]?.ToString()
                    };
                }
            }

            await CloseConnectionAsync(Connection);
            return adminModel;
        }


        public async Task<ObservableCollection<PlumbingFixtureDisplay>> GetPlumbingFixturesByProjectId(string projectId)
        {
            ObservableCollection<PlumbingFixtureDisplay> fixtures =
                new ObservableCollection<PlumbingFixtureDisplay>();
            string query = @"
        SELECT 
            plumbing_fixture_types.abbreviation,
            plumbing_fixtures.number,
            plumbing_fixture_catalog.description,
            plumbing_fixture_types.name,
            plumbing_fixture_catalog.make,
            plumbing_fixture_catalog.model,
            plumbing_fixture_catalog.trap,
            plumbing_fixture_catalog.waste,
            plumbing_fixture_catalog.vent,
            plumbing_fixture_catalog.cold_water,
            plumbing_fixture_catalog.hot_water,
            plumbing_fixture_catalog.fixture_demand,
            plumbing_fixture_catalog.hot_demand,
            plumbing_fixture_catalog.dfu
        FROM plumbing_fixtures
        LEFT JOIN plumbing_fixture_catalog ON plumbing_fixture_catalog.id = plumbing_fixtures.catalog_id
        LEFT JOIN plumbing_fixture_types ON plumbing_fixture_types.id = plumbing_fixture_catalog.type_id
        WHERE plumbing_fixtures.project_id = @projectId";
            await OpenConnectionAsync(Connection);
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectId", projectId);
            MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                fixtures.Add(
                    new PlumbingFixtureDisplay
                    {
                        Abbreviation = GetSafeString(reader, "abbreviation"),
                        Number = GetSafeInt(reader, "number"),
                        Description = GetSafeString(reader, "description"),
                        Name = GetSafeString(reader, "name"),
                        Make = GetSafeString(reader, "make"),
                        Model = GetSafeString(reader, "model"),
                        Trap = GetSafeFloat(reader, "trap"),
                        Waste = GetSafeFloat(reader, "waste"),
                        Vent = GetSafeFloat(reader, "vent"),
                        ColdWater = GetSafeFloat(reader, "cold_water"),
                        HotWater = GetSafeFloat(reader, "hot_water"),
                        FixtureDemand = GetSafeFloat(reader, "fixture_demand"),
                        HotDemand = GetSafeFloat(reader, "hot_demand"),
                        DFU = GetSafeInt(reader, "dfu")
                    });
            }

            await reader.CloseAsync();
            await CloseConnectionAsync(Connection);
            return fixtures;
        }


        public bool LoginUser(string userName, string password)
        {
            string query =
                @"
            SELECT e.passhash
            FROM employees e 
            WHERE e.username = @username";
            OpenConnection(Connection);
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
            CloseConnection(Connection);
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
            OpenConnection(Connection);
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
            CloseConnection(Connection);
            return employees;
        }

        public void SaveEmployee(Employee employee)
        {
            OpenConnection(Connection);
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

            CloseConnection(Connection);
        }

        public void SetEmployeePassword(string employeeId, string password)
        {
            string query = "UPDATE employees SET passhash = @passhash WHERE id = @id";
            string salt = BCrypt.Net.BCrypt.GenerateSalt(10);
            string passhash = BCrypt.Net.BCrypt.HashPassword(password, salt);
            OpenConnection(Connection);
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@passhash", passhash);
            command.Parameters.AddWithValue("@id", employeeId);
            command.ExecuteNonQuery();
            CloseConnection(Connection);
        }

        public async Task<Dictionary<int, string>> GetProjectIds(string projectNo)
        {
            string query = "SELECT id, version FROM projects WHERE gmep_project_no = @projectNo";
            await OpenConnectionAsync(Connection);
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

            await CloseConnectionAsync(Connection);
            projectIds = projectIds.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            return projectIds;
        }

        public async Task<Dictionary<int, string>> AddProjectVersions(
            string projectNo,
            string projectId
        )
        {
            string query = "SELECT id, version FROM projects WHERE gmep_project_no = @projectNo";
            await OpenConnectionAsync(Connection);
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

            CloseConnection(Connection);
            return projectIds;
        }

        public async Task<Dictionary<int, string>> DeleteProjectVersions(
            string projectNo,
            string projectId
        )
        {
            await OpenConnectionAsync(Connection);
            string[] tables = new string[]
            {
                "electrical_panels",
                "electrical_transformers",
                "electrical_equipment",
                "electrical_lighting_locations",
                "electrical_lighting",
                "electrical_services",
                "electrical_panel_notes",
                "electrical_panel_note_panel_rel",
                "electrical_panel_custom_circuits",
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
            await CloseConnectionAsync(Connection);
            return projectIds;
        }

        public async Task<Dictionary<string, string>> getOwners()
        {
            var owners = new Dictionary<string, string>();

            try
            {
                await OpenConnectionAsync(Connection);

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
                await CloseConnectionAsync(Connection);
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
            ObservableCollection<ElectricalLightingControl> lightingControls,
            ObservableCollection<Location> locations,
            ObservableCollection<ElectricalPanelNote> electricalPanelNotes,
            ObservableCollection<ElectricalPanelNoteRel> electricalPanelNoteRels,
            ObservableCollection<Circuit> customCircuits,
            ObservableCollection<TimeClock> timeClocks
        )
        {
            await OpenConnectionAsync(Connection);
            await UpdateServices(projectId, services);
            await UpdatePanels(projectId, panels);
            await UpdateEquipments(projectId, equipments);
            await UpdateTransformers(projectId, transformers);
            await UpdateLightings(projectId, lightings);
            await UpdateLightingControls(projectId, lightingControls);
            await UpdateLightingLocations(projectId, locations);
            await UpdateElectricalPanelNotes(projectId, electricalPanelNotes);
            await UpdateElectricalPanelNoteRels(projectId, electricalPanelNoteRels);
            await UpdateCustomCircuits(projectId, customCircuits);
            await UpdateTimeClocks(projectId, timeClocks);

            await CloseConnectionAsync(Connection);
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

        private async Task UpdateElectricalPanelNotes(
            string projectId,
            ObservableCollection<ElectricalPanelNote> panelNotes
        )
        {
            var existingPanelIds = await GetExistingIds(
                "electrical_panel_notes",
                "project_id",
                projectId
            );

            var panelNotesCopy = panelNotes.ToList(); // Create a copy of the collection

            foreach (var note in panelNotesCopy)
            {
                if (existingPanelIds.Contains(note.Id))
                {
                    await UpdateElectricalPanelNote(note);
                    existingPanelIds.Remove(note.Id);
                }
                else
                {
                    await InsertElectricalPanelNote(projectId, note);
                }
            }

            await DeleteRemovedItems("electrical_panel_notes", existingPanelIds);
        }

        private async Task UpdateElectricalPanelNoteRels(
            string projectId,
            ObservableCollection<ElectricalPanelNoteRel> noteRels
        )
        {
            var existingNoteRelIds = await GetExistingIds(
                "electrical_panel_note_panel_rel",
                "project_id",
                projectId
            );
            var noteRelsCopy = noteRels.ToList(); // Create a copy of the collection

            foreach (var note in noteRelsCopy)
            {
                if (existingNoteRelIds.Contains(note.Id))
                {
                    await UpdateElectricalPanelNoteRel(note);
                    existingNoteRelIds.Remove(note.Id);
                }
                else
                {
                    await InsertElectricalPanelNoteRel(projectId, note);
                }
            }
            await DeleteRemovedItems("electrical_panel_note_panel_rel", existingNoteRelIds);
        }

        private async Task UpdateCustomCircuits(
            string projectId,
            ObservableCollection<Circuit> customCircuits
        )
        {
            var existingPanelIds = await GetExistingIds(
                "electrical_panel_custom_circuits",
                "project_id",
                projectId
            );

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

            await DeleteRemovedItems("electrical_panel_custom_circuits", existingPanelIds);
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

        private async Task UpdateLightingControls(
            string projectId,
            ObservableCollection<ElectricalLightingControl> controls
        )
        {
            var existingLightingControlIds = await GetExistingIds(
                "electrical_lighting_controls",
                "project_id",
                projectId
            );

            foreach (var control in controls)
            {
                if (existingLightingControlIds.Contains(control.Id))
                {
                    await UpdateLightingControl(control);
                    existingLightingControlIds.Remove(control.Id);
                }
                else
                {
                    await InsertLightingControl(projectId, control);
                }
            }

            await DeleteRemovedItems("electrical_lighting_controls", existingLightingControlIds);
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

        private async Task UpdateTimeClocks(
            string projectId,
            ObservableCollection<TimeClock> clocks
        )
        {
            var existingLocationIds = await GetExistingIds(
                "electrical_lighting_timeclocks",
                "project_id",
                projectId
            );
            foreach (var clock in clocks)
            {
                if (existingLocationIds.Contains(clock.Id))
                {
                    await UpdateClock(clock);
                    existingLocationIds.Remove(clock.Id);
                }
                else
                {
                    await InsertClock(projectId, clock);
                }
            }
            await DeleteRemovedItems("electrical_lighting_timeclocks", existingLocationIds);
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
                "UPDATE electrical_services SET name = @name, order_no = @order_no, electrical_service_amp_rating_id = @amp, electrical_service_voltage_id = @type, electrical_service_meter_config_id = @config, color_code = @color_code, aic_rating = @aicRating, parent_id = @parentId WHERE id = @id";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@name", service.Name);
            command.Parameters.AddWithValue("@amp", service.Amp);
            command.Parameters.AddWithValue("@id", service.Id);
            command.Parameters.AddWithValue("@type", service.Type);
            command.Parameters.AddWithValue("@config", service.Config);
            command.Parameters.AddWithValue("@color_code", service.ColorCode);
            command.Parameters.AddWithValue("@aicRating", service.AicRating);
            command.Parameters.AddWithValue("@parentId", service.ParentId);
            command.Parameters.AddWithValue("@order_no", service.OrderNo);
            await command.ExecuteNonQueryAsync();
        }

        private async Task InsertService(string projectId, ElectricalService service)
        {
            string query =
                "INSERT INTO electrical_services (id, project_id, name, electrical_service_amp_rating_id, electrical_service_voltage_id, electrical_service_meter_config_id, color_code, aic_rating, parent_id, order_no) VALUES (@id, @projectId, @name, @amp, @type, @config, @color_code, @aicRating, @parentId, @order_no)";
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
            command.Parameters.AddWithValue("@order_no", service.OrderNo);
            await command.ExecuteNonQueryAsync();
        }

        private async Task UpdatePanel(ElectricalPanel panel)
        {
            string query =
                "UPDATE electrical_panels SET bus_amp_rating_id = @bus, main_amp_rating_id = @main, order_no = @order_no, is_distribution = @is_distribution, voltage_id = @type, num_breakers = @numBreakers, parent_distance = @distanceFromParent, aic_rating = @aicRating, name = @name, color_code = @color_code, parent_id = @parent_id, is_recessed = @is_recessed, is_mlo = @is_mlo, circuit_no = @circuit_no, is_hidden_on_plan = @is_hidden_on_plan, location = @location, high_leg_phase = @highLegPhase, load_amperage = @amp, kva = @kva, status_id = @statusId WHERE id = @id";
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
            command.Parameters.AddWithValue("@order_no", panel.OrderNo);
            command.Parameters.AddWithValue("@statusId", panel.StatusId);
            await command.ExecuteNonQueryAsync();
        }

        private async Task InsertPanel(string projectId, ElectricalPanel panel)
        {
            string query =
                "INSERT INTO electrical_panels (id, project_id, bus_amp_rating_id, main_amp_rating_id, is_distribution, name, color_code, parent_id, num_breakers, parent_distance, aic_rating, voltage_id, is_recessed, is_mlo, circuit_no, is_hidden_on_plan, location, high_leg_phase, load_amperage, kva, order_no) VALUES (@id, @projectId, @bus, @main, @is_distribution, @name, @color_code, @parent_id, @numBreakers, @distanceFromParent, @AicRating, @type, @is_recessed, @is_mlo, @circuit_no, @is_hidden_on_plan, @location, @highLegPhase, @amp, @kva, @order_no)";
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
            command.Parameters.AddWithValue("@order_no", panel.OrderNo);
            await command.ExecuteNonQueryAsync();
        }

        private async Task UpdateElectricalPanelNote(ElectricalPanelNote note)
        {
            string query = "UPDATE electrical_panel_notes SET note = @note WHERE id = @id";
            MySqlCommand command = new MySqlCommand(query, Connection);

            command.Parameters.AddWithValue("@note", note.Note);
            command.Parameters.AddWithValue("@id", note.Id);

            await command.ExecuteNonQueryAsync();
        }

        private async Task UpdateElectricalPanelNoteRel(ElectricalPanelNoteRel noteRel)
        {
            string query =
                "UPDATE electrical_panel_note_panel_rel SET panel_id = @panelId, note_id = @noteId, circuit_no = @circuitNo, length = @length, stack = @stack WHERE id = @id";
            MySqlCommand command = new MySqlCommand(query, Connection);

            command.Parameters.AddWithValue("@id", noteRel.Id);
            command.Parameters.AddWithValue("@panelId", noteRel.PanelId);
            command.Parameters.AddWithValue("@noteId", noteRel.NoteId);
            command.Parameters.AddWithValue("@circuitNo", noteRel.CircuitNo);
            command.Parameters.AddWithValue("@length", noteRel.Length);
            command.Parameters.AddWithValue("@stack", noteRel.Stack);

            await command.ExecuteNonQueryAsync();
        }

        private async Task InsertElectricalPanelNote(string projectId, ElectricalPanelNote note)
        {
            if (String.IsNullOrEmpty(note.Note))
            {
                return;
            }
            string query =
                @"
                INSERT IGNORE INTO electrical_panel_notes (id, project_id, note, date)
                VALUES
                (@id, @projectId, @note, @date)
                ";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", note.Id);
            command.Parameters.AddWithValue("@projectId", note.ProjectId);
            command.Parameters.AddWithValue("@note", note.Note);
            command.Parameters.AddWithValue("@date", note.DateCreated);
            await command.ExecuteNonQueryAsync();
        }

        private async Task InsertElectricalPanelNoteRel(
            string projectId,
            ElectricalPanelNoteRel noteRel
        )
        {
            string query =
                @"
                INSERT IGNORE INTO electrical_panel_note_panel_rel
                (id, project_id, panel_id, note_id, circuit_no, length, stack)
                VALUES
                (@id, @projectId, @panelId, @noteId, @circuitNo, @length, @stack)
                ";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", noteRel.Id);
            command.Parameters.AddWithValue("@projectId", noteRel.ProjectId);
            command.Parameters.AddWithValue("@panelId", noteRel.PanelId);
            command.Parameters.AddWithValue("@noteId", noteRel.NoteId);
            command.Parameters.AddWithValue("@circuitNo", noteRel.CircuitNo);
            command.Parameters.AddWithValue("@length", noteRel.Length);
            command.Parameters.AddWithValue("@stack", noteRel.Stack);

            await command.ExecuteNonQueryAsync();
        }

        private async Task UpdateCustomCircuit(Circuit customCircuit)
        {
            string query =
                "UPDATE electrical_panel_custom_circuits SET number = @number, equip_id = @equipId, breaker_size = @breakerSize, description = @description, load_category = @loadCategory, va = @va, custom_breaker_size = @customBreakerSize, custom_description = @customDescription WHERE id = @id";
            MySqlCommand command = new MySqlCommand(query, Connection);

            command.Parameters.AddWithValue("@id", customCircuit.Id);
            command.Parameters.AddWithValue("@number", customCircuit.Number);
            command.Parameters.AddWithValue("@equipId", customCircuit.EquipId);
            command.Parameters.AddWithValue("@breakerSize", customCircuit.BreakerSize);
            command.Parameters.AddWithValue("@loadCategory", customCircuit.LoadCategory);
            command.Parameters.AddWithValue("@description", customCircuit.Description);
            command.Parameters.AddWithValue("@va", customCircuit.Va);
            command.Parameters.AddWithValue("@customBreakerSize", customCircuit.CustomBreakerSize);
            command.Parameters.AddWithValue("@customDescription", customCircuit.CustomDescription);

            await command.ExecuteNonQueryAsync();
        }

        private async Task UpdateClock(TimeClock clock)
        {
            string query =
                "UPDATE electrical_lighting_timeclocks SET name = @name, bypass_switch_name = @bypassSwitchName, bypass_switch_location = @bypassSwitchLocation, voltage_id = @voltageId, adjacent_panel_id = @adjacentPanelId WHERE id = @id";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", clock.Id);
            command.Parameters.AddWithValue("@name", clock.Name);
            command.Parameters.AddWithValue("@bypassSwitchName", clock.BypassSwitchName);
            command.Parameters.AddWithValue("@bypassSwitchLocation", clock.BypassSwitchLocation);
            command.Parameters.AddWithValue("@voltageId", clock.VoltageId);
            command.Parameters.AddWithValue("@adjacentPanelId", clock.AdjacentPanelId);

            await command.ExecuteNonQueryAsync();
        }

        private async Task InsertCustomCircuit(string projectId, Circuit customCircuit)
        {
            string query =
                "INSERT INTO electrical_panel_custom_circuits (id, panel_id, project_id, equip_id, number, breaker_size, description, load_category, va, custom_breaker_size, custom_description) VALUES (@id, @panelId, @projectId, @equipId, @number, @breakerSize, @description, @loadCategory, @va, @customBreakerSize, @customDescription)";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", customCircuit.Id);
            command.Parameters.AddWithValue("@projectId", projectId);
            command.Parameters.AddWithValue("@panelId", customCircuit.PanelId);
            command.Parameters.AddWithValue("@equipId", customCircuit.EquipId);
            command.Parameters.AddWithValue("@number", customCircuit.Number);
            command.Parameters.AddWithValue("@breakerSize", customCircuit.BreakerSize);
            command.Parameters.AddWithValue("@loadCategory", customCircuit.LoadCategory);
            command.Parameters.AddWithValue("@description", customCircuit.Description);
            command.Parameters.AddWithValue("@va", customCircuit.Va);
            command.Parameters.AddWithValue("@customBreakerSize", customCircuit.CustomBreakerSize);
            command.Parameters.AddWithValue("@customDescription", customCircuit.CustomDescription);
            await command.ExecuteNonQueryAsync();
        }

        public async Task UpdateEquipment(
            ElectricalEquipment equipment,
            MySqlConnection conn = null
        )
        {
            if (conn == null)
            {
                conn = Connection;
            }
            string query =
                "UPDATE electrical_equipment SET description = @description, equip_no = @equip_no, parent_id = @parent_id, owner_id = @owner, voltage_id = @voltage, fla = @fla, is_three_phase = @is_3ph, spec_sheet_id = @spec_sheet_id, aic_rating = @aic_rating, spec_sheet_from_client = @spec_sheet_from_client, parent_distance=@distanceFromParent, category_id=@category, color_code = @color_code, connection_type_id = @connection, mocp_id = @mocpId, hp = @hp, has_plug = @has_plug, locking_connector = @locking_connector, width=@width, depth=@depth, height=@height, circuit_no=@circuit_no, is_hidden_on_plan=@is_hidden_on_plan, load_type = @loadType, order_no = @order_no, va=@va, status_id = @statusId, connection_symbol_id = @connectionSymbolId, num_conv_duplex = @numConvDuplex, circuit_half = @circuitHalf, phase_a_va = @phaseAVa, phase_b_va = @phaseBVa, phase_c_va = @phaseCVa WHERE id = @id";
            MySqlCommand command = new MySqlCommand(query, conn);
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
            command.Parameters.AddWithValue("@mocpId", equipment.MocpId);
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
            command.Parameters.AddWithValue("@statusId", equipment.StatusId);
            command.Parameters.AddWithValue("@connectionSymbolId", equipment.ConnectionSymbolId);
            command.Parameters.AddWithValue("@numConvDuplex", equipment.NumConvDuplex);
            command.Parameters.AddWithValue("@circuitHalf", equipment.circuitHalf);
            command.Parameters.AddWithValue("@phaseAVa", equipment.PhaseAVA);
            command.Parameters.AddWithValue("@phaseBVa", equipment.PhaseBVA);
            command.Parameters.AddWithValue("@phaseCVa", equipment.PhaseCVA);
            await command.ExecuteNonQueryAsync();
        }

        public async Task InsertEquipment(
            string projectId,
            ElectricalEquipment equipment,
            MySqlConnection conn = null
        )
        {
            if (conn == null)
            {
                conn = Connection;
            }
            string query =
                "INSERT INTO electrical_equipment (id, project_id, equip_no, parent_id, owner_id, voltage_id, fla, is_three_phase, spec_sheet_id, aic_rating, spec_sheet_from_client, parent_distance, category_id, color_code, connection_type_id, description, mca, hp, has_plug, locking_connector, width, depth, height, circuit_no, is_hidden_on_plan, load_type, order_no, va, date_created, status_id, connection_symbol_id, num_conv_duplex, phase_a_va, phase_b_va, phase_c_va, mocp_id) VALUES (@id, @projectId, @equip_no, @parent_id, @owner, @voltage, @fla, @is_3ph, @spec_sheet_id, @aic_rating, @spec_sheet_from_client, @distanceFromParent, @category, @color_code, @connection, @description, @mocp_id, @hp, @has_plug, @locking_connector, @width, @depth, @height, @circuit_no, @is_hidden_on_plan, @loadType, @order_no, @va, @dateCreated, @statusId, @connectionSymbolId, @numConvDuplex, @phaseAVa, @phaseBVa, @phaseCVa, @mocp_id)";
            MySqlCommand command = new MySqlCommand(query, conn);
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
            command.Parameters.AddWithValue("@mocp_id", equipment.MocpId);
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
            command.Parameters.AddWithValue(
                "@dateCreated",
                equipment.DateCreated.ToString("yyyy-MM-dd HH:mm:ss.fff")
            );
            command.Parameters.AddWithValue("@statusId", equipment.StatusId);
            command.Parameters.AddWithValue("@connectionSymbolId", equipment.ConnectionSymbolId);
            command.Parameters.AddWithValue("@numConvDuplex", equipment.NumConvDuplex);
            command.Parameters.AddWithValue("@phaseAVa", equipment.PhaseAVA);
            command.Parameters.AddWithValue("@phaseBVa", equipment.PhaseBVA);
            command.Parameters.AddWithValue("@phaseCVa", equipment.PhaseCVA);
            command.Parameters.AddWithValue("@mocpId", equipment.MocpId);
            await command.ExecuteNonQueryAsync();
        }

        private async Task UpdateLighting(ElectricalLighting lighting)
        {
            string query =
                "UPDATE electrical_lighting SET notes = @notes, model_no = @model_no, order_no = @order_no, parent_id = @parent_id, voltage_id = @voltageId, color_code = @colorCode, mounting_type_id = @mountingType, occupancy=@occupancy, manufacturer = @manufacturer, wattage = @wattage, em_capable = @em_capable, tag = @tag, symbol_id = @symbolId, description=@description, driver_type_id = @driverTypeId, spec_sheet_from_client=@specFromClient, spec_sheet_id=@specSheetId, qty = @qty, has_photocell = @hasPhotoCell, location_id = @locationId WHERE id = @id";
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
            command.Parameters.AddWithValue("@order_no", lighting.OrderNo);

            await command.ExecuteNonQueryAsync();
        }

        private async Task UpdateLightingControl(ElectricalLightingControl control)
        {
            string query =
                @"
                UPDATE electrical_lighting_controls SET driver_type_id = @driverTypeId, occupancy = @occupancy, name = @name WHERE id = @id
                ";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@driverTypeId", control.DriverTypeId);
            command.Parameters.AddWithValue("@occupancy", control.Occupancy);
            command.Parameters.AddWithValue("@id", control.Id);
            command.Parameters.AddWithValue("@name", control.Tag);
            await command.ExecuteNonQueryAsync();
        }

        private async Task InsertLighting(string projectId, ElectricalLighting lighting)
        {
            string query =
                "INSERT INTO electrical_lighting (id, project_id, notes, model_no, parent_id, voltage_id, color_code, mounting_type_id, occupancy, manufacturer, wattage, em_capable, tag, symbol_id, description, driver_type_id, spec_sheet_from_client, spec_sheet_id, qty, has_photocell, location_id, order_no) VALUES (@id, @project_id, @notes, @model_no, @parent_id, @voltageId, @colorCode, @mountingType, @occupancy, @manufacturer, @wattage, @em_capable, @tag, @symbolId, @description, @driverTypeId, @specFromClient, @specSheetId, @qty, @hasPhotoCell, @locationId, @order_no)";
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
            command.Parameters.AddWithValue("@order_no", lighting.OrderNo);
            await command.ExecuteNonQueryAsync();
        }

        private async Task InsertLightingControl(
            string projectId,
            ElectricalLightingControl control
        )
        {
            string query =
                @"
                INSERT INTO electrical_lighting_controls (id, project_id, driver_type_id, occupancy, name) VALUES (@id, @projectId, @driverTypeId, @occupancy, @name)
                ";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", control.Id);
            command.Parameters.AddWithValue("@projectId", projectId);
            command.Parameters.AddWithValue("@driverTypeId", control.DriverTypeId);
            command.Parameters.AddWithValue("@occupancy", control.Occupancy);
            command.Parameters.AddWithValue("@name", control.Tag);
            await command.ExecuteNonQueryAsync();
        }

        private async Task UpdateTransformer(ElectricalTransformer transformer)
        {
            string query =
                "UPDATE electrical_transformers SET parent_id = @parent_id, voltage_id = @voltage, kva_id = @kva, parent_distance = @distanceFromParent, color_code = @color_code, name = @name, circuit_no = @circuitNo, is_hidden_on_plan = @is_hidden_on_plan, is_wall_mounted = @isWallMounted, aic_rating = @aicRating, order_no = @order_no WHERE id = @id";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@parent_id", transformer.ParentId);
            command.Parameters.AddWithValue("@id", transformer.Id);
            command.Parameters.AddWithValue("@voltage", transformer.Voltage);
            command.Parameters.AddWithValue("@distanceFromParent", transformer.DistanceFromParent);
            command.Parameters.AddWithValue("@kva", transformer.Kva);
            command.Parameters.AddWithValue("@color_code", transformer.ColorCode);
            command.Parameters.AddWithValue("@name", transformer.Name);
            command.Parameters.AddWithValue("@circuitNo", transformer.CircuitNo);
            command.Parameters.AddWithValue("@is_hidden_on_plan", transformer.IsHiddenOnPlan);
            command.Parameters.AddWithValue("@isWallMounted", transformer.IsWallMounted);
            command.Parameters.AddWithValue("@aicRating", transformer.AicRating);
            command.Parameters.AddWithValue("@order_no", transformer.OrderNo);
            await command.ExecuteNonQueryAsync();
        }

        private async Task InsertTransformer(string projectId, ElectricalTransformer transformer)
        {
            string query =
                "INSERT INTO electrical_transformers (id, project_id, parent_id, voltage_id, parent_distance, color_code, kva_id, name, circuit_no, is_hidden_on_plan, is_wall_mounted, aic_rating, order_no) VALUES (@id, @project_id, @parent_id, @voltage, @distanceFromParent, @color_code, @kva, @name, @circuitNo, @isHiddenOnPlan, @isWallMounted, @aicRating, @order_no)";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", transformer.Id);
            command.Parameters.AddWithValue("@project_id", projectId);
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
            command.Parameters.AddWithValue("@order_no", transformer.OrderNo);
            await command.ExecuteNonQueryAsync();
        }

        private async Task InsertClock(string projectId, TimeClock clock)
        {
            string query =
                "INSERT INTO electrical_lighting_timeclocks (id, project_id, name, bypass_switch_name, bypass_switch_location, voltage_id, adjacent_panel_id) VALUES (@id, @projectId, @name, @bypassSwitchName, @bypassSwitchLocation, @voltageId, @adjacentPanelId)";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", clock.Id);
            command.Parameters.AddWithValue("@name", clock.Name);
            command.Parameters.AddWithValue("@bypassSwitchName", clock.BypassSwitchName);
            command.Parameters.AddWithValue("@bypassSwitchLocation", clock.BypassSwitchLocation);
            command.Parameters.AddWithValue("@voltageId", clock.VoltageId);
            command.Parameters.AddWithValue("@adjacentPanelId", clock.AdjacentPanelId);
            command.Parameters.AddWithValue("@projectId", projectId);

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
            if (String.IsNullOrEmpty(location.LocationDescription))
            {
                return;
            }
            string query =
                "INSERT INTO electrical_lighting_locations (id, project_id, location, outdoor) VALUES (@id, @projectId, @locationDescription, @isOutside)";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", location.Id);
            command.Parameters.AddWithValue("@projectId", projectId);
            command.Parameters.AddWithValue("@locationDescription", location.LocationDescription);
            command.Parameters.AddWithValue("@isOutside", location.IsOutside);

            await command.ExecuteNonQueryAsync();

            query =
                "INSERT INTO electrical_lighting_timeclock_control_relays (id, project_id, name, outdoor) VALUES (@id, @projectId, @locationDescription, @isOutside)";
            command = new MySqlCommand(query, Connection);
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
            string query =
                "SELECT * FROM electrical_services WHERE project_id = @projectId ORDER BY order_no";
            await OpenConnectionAsync(Connection);
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
                        reader.GetString("parent_id"),
                        reader.GetInt32("order_no")
                    )
                );
            }
            await reader.CloseAsync();
            await CloseConnectionAsync(Connection);
            return services;
        }

        public async Task<ObservableCollection<ElectricalPanel>> GetProjectPanels(string projectId)
        {
            ObservableCollection<ElectricalPanel> panels =
                new ObservableCollection<ElectricalPanel>();
            string query =
                "SELECT * FROM electrical_panels WHERE project_id = @projectId ORDER BY order_no";
            await OpenConnectionAsync(Connection);
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectId", projectId);
            MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                panels.Add(
                    new ElectricalPanel(
                        GetSafeString(reader, "id"),
                        GetSafeString(reader, "project_id"),
                        GetSafeInt(reader, "bus_amp_rating_id"),
                        GetSafeInt(reader, "main_amp_rating_id"),
                        GetSafeBoolean(reader, "is_mlo"),
                        GetSafeBoolean(reader, "is_distribution"),
                        GetSafeString(reader, "name"),
                        GetSafeString(reader, "color_code"),
                        GetSafeString(reader, "parent_id"),
                        GetSafeInt(reader, "num_breakers"),
                        GetSafeInt(reader, "parent_distance"),
                        GetSafeInt(reader, "aic_rating"),
                        GetSafeFloat(reader, "load_amperage"),
                        GetSafeFloat(reader, "kva"),
                        GetSafeInt(reader, "voltage_id"),
                        false,
                        GetSafeBoolean(reader, "is_recessed"),
                        GetSafeInt(reader, "circuit_no"),
                        GetSafeBoolean(reader, "is_hidden_on_plan"),
                        GetSafeString(reader, "location"),
                        GetSafeInt(reader, "voltage_id") == 4
                            ? GetSafeChar(reader, "high_leg_phase")
                            : '-',
                        GetSafeInt(reader, "order_no"),
                        GetSafeInt(reader, "status_id"),
                        this
                    )
                );
            }
            await reader.CloseAsync();
            await CloseConnectionAsync(Connection);
            return panels;
        }

        public ObservableCollection<ElectricalPanelNoteRel> GetElectricalPanelNoteRels(
            string panelId
        )
        {
            ObservableCollection<ElectricalPanelNoteRel> noteRels =
                new ObservableCollection<ElectricalPanelNoteRel>();
            string query =
                @"
                SELECT 
                electrical_panel_note_panel_rel.id,
                electrical_panel_note_panel_rel.project_id,
                electrical_panel_note_panel_rel.panel_id,
                electrical_panel_note_panel_rel.note_id,
                electrical_panel_note_panel_rel.circuit_no,
                electrical_panel_note_panel_rel.length,
                electrical_panel_note_panel_rel.stack,
                electrical_panel_notes.note
                FROM electrical_panel_note_panel_rel
                LEFT JOIN electrical_panel_notes ON electrical_panel_notes.id = electrical_panel_note_panel_rel.note_id
                WHERE electrical_panel_note_panel_rel.panel_id = @panelId
                ORDER BY electrical_panel_notes.date
                ";
            OpenConnection(Connection);
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@panelId", panelId);
            MySqlDataReader reader = (MySqlDataReader)command.ExecuteReader();
            List<string> noteIds = new List<string>();
            while (reader.Read())
            {
                string noteId = GetSafeString(reader, "note_id");
                if (!noteIds.Contains(noteId))
                {
                    noteIds.Add(noteId);
                }
                ElectricalPanelNoteRel noteRel = new ElectricalPanelNoteRel(
                    GetSafeString(reader, "id"),
                    GetSafeString(reader, "project_id"),
                    GetSafeString(reader, "panel_id"),
                    GetSafeString(reader, "note_id"),
                    GetSafeString(reader, "note"),
                    GetSafeInt(reader, "circuit_no"),
                    GetSafeInt(reader, "length"),
                    GetSafeInt(reader, "stack"),
                    (noteIds.IndexOf(noteId) + 1).ToString()
                );
                noteRels.Add(noteRel);
            }
            reader.Close();
            CloseConnection(Connection);
            return noteRels;
        }

        public async Task<
            ObservableCollection<ElectricalPanelNoteRel>
        > GetProjectElectricalPanelNoteRels(string projectId)
        {
            ObservableCollection<ElectricalPanelNoteRel> noteRels =
                new ObservableCollection<ElectricalPanelNoteRel>();
            string query =
                @"
                SELECT 
                electrical_panel_note_panel_rel.id,
                electrical_panel_note_panel_rel.project_id,
                electrical_panel_note_panel_rel.panel_id,
                electrical_panel_note_panel_rel.note_id,
                electrical_panel_note_panel_rel.circuit_no,
                electrical_panel_note_panel_rel.length,
                electrical_panel_note_panel_rel.stack,
                electrical_panel_notes.note
                FROM electrical_panel_note_panel_rel
                LEFT JOIN electrical_panel_notes ON electrical_panel_notes.id = electrical_panel_note_panel_rel.note_id
                WHERE electrical_panel_note_panel_rel.project_id = @projectId
                ORDER BY electrical_panel_note_panel_rel.panel_id, electrical_panel_notes.date
                ";
            await OpenConnectionAsync(Connection);
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectId", projectId);
            MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
            List<string> noteIds = new List<string>();
            string currentPanelId = "";
            while (await reader.ReadAsync())
            {
                string noteId = GetSafeString(reader, "note_id");
                if (currentPanelId != GetSafeString(reader, "panel_id"))
                {
                    noteIds.Clear();
                    currentPanelId = GetSafeString(reader, "panel_id");
                }
                if (!noteIds.Contains(noteId))
                {
                    noteIds.Add(noteId);
                }
                ElectricalPanelNoteRel noteRel = new ElectricalPanelNoteRel(
                    GetSafeString(reader, "id"),
                    GetSafeString(reader, "project_id"),
                    currentPanelId,
                    GetSafeString(reader, "note_id"),
                    GetSafeString(reader, "note"),
                    GetSafeInt(reader, "circuit_no"),
                    GetSafeInt(reader, "length"),
                    GetSafeInt(reader, "stack"),
                    (noteIds.IndexOf(noteId) + 1).ToString()
                );
                noteRels.Add(noteRel);
            }
            await reader.CloseAsync();
            await CloseConnectionAsync(Connection);
            return noteRels;
        }

        public async Task<ObservableCollection<ElectricalPanelNote>> GetProjectElectricalPanelNotes(
            string projectId
        )
        {
            ObservableCollection<ElectricalPanelNote> notes =
                new ObservableCollection<ElectricalPanelNote>();
            string query =
                @"
                SELECT * 
                FROM electrical_panel_notes
                WHERE project_id = @projectId
                ORDER BY id
                ";
            await OpenConnectionAsync(Connection);
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectId", projectId);
            MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
            List<string> noteIds = new List<string>();
            while (await reader.ReadAsync())
            {
                string noteId = GetSafeString(reader, "id");
                if (!noteIds.Contains(noteId))
                {
                    noteIds.Add(noteId);
                }
                ElectricalPanelNote note = new ElectricalPanelNote(
                    GetSafeString(reader, "id"),
                    GetSafeString(reader, "project_id"),
                    GetSafeString(reader, "note"),
                    (noteIds.IndexOf(noteId) + 1).ToString()
                );
                notes.Add(note);
            }
            await reader.CloseAsync();
            await CloseConnectionAsync(Connection);
            return notes;
        }

        public ObservableCollection<ElectricalPanelNote> GetElectricalPanelNotes(string panelId)
        {
            ObservableCollection<ElectricalPanelNote> notes =
                new ObservableCollection<ElectricalPanelNote>();
            string query =
                @"
                SELECT 
                electrical_panel_notes.id as note_id,
                electrical_panel_notes.project_id,
                electrical_panel_notes.note,
                electrical_panel_notes.date,
                electrical_panel_note_panel_rel.id as rel_id,
                electrical_panel_note_panel_rel.panel_id,
                electrical_panel_note_panel_rel.circuit_no
                FROM electrical_panel_notes
                LEFT JOIN electrical_panel_note_panel_rel
                ON electrical_panel_note_panel_rel.note_id = electrical_panel_notes.id
                WHERE electrical_panel_note_panel_rel.panel_id = @panelId
                ORDER BY electrical_panel_notes.date
                ";
            OpenConnection(Connection);
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@panelId", panelId);
            MySqlDataReader reader = (MySqlDataReader)command.ExecuteReader();
            List<string> noteIds = new List<string>();
            while (reader.Read())
            {
                string noteId = GetSafeString(reader, "note_id");
                if (!noteIds.Contains(noteId))
                {
                    noteIds.Add(noteId);
                    ElectricalPanelNote note = new ElectricalPanelNote(
                        GetSafeString(reader, "note_id"),
                        GetSafeString(reader, "project_id"),
                        GetSafeString(reader, "note"),
                        (noteIds.IndexOf(noteId) + 1).ToString()
                    );
                    note.DateCreated = GetSafeDateTime(reader, "date")
                        .ToString("yyyy-MM-dd HH:mm:ss.fff");
                    notes.Add(note);
                }
            }
            reader.Close();
            CloseConnection(Connection);
            return notes;
        }

        public async Task<ObservableCollection<Circuit>> GetProjectCustomCircuits(string projectId)
        {
            ObservableCollection<Circuit> customCircuits = new ObservableCollection<Circuit>();
            string query =
                "SELECT * FROM electrical_panel_custom_circuits WHERE project_id = @projectId";
            await OpenConnectionAsync(Connection);
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectId", projectId);
            MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                customCircuits.Add(
                    new Circuit(
                        GetSafeString(reader, "id"),
                        GetSafeString(reader, "panel_id"),
                        GetSafeString(reader, "project_id"),
                        GetSafeString(reader, "equip_id"),
                        GetSafeInt(reader, "number"),
                        GetSafeInt(reader, "va"),
                        GetSafeInt(reader, "breaker_size"),
                        GetSafeString(reader, "description"),
                        GetSafeInt(reader, "load_category"),
                        GetSafeBoolean(reader, "custom_breaker_size"),
                        GetSafeBoolean(reader, "custom_description")
                    )
                );
            await reader.CloseAsync();
            await CloseConnectionAsync(Connection);
            return customCircuits;
        }

        public async Task<ObservableCollection<Circuit>> GetProjectElectricalPanelMiniBreakers(
            string projectId
        )
        {
            ObservableCollection<Circuit> miniBreakers = new ObservableCollection<Circuit>();
            string query =
                @"
                SELECT
                electrical_panel_mini_breakers.id,
                electrical_panel_mini_breakers.panel_id,
                electrical_panel_mini_breakers.circuit_no,
                electrical_panel_mini_breakers.equip_a_id,
                electrical_panel_mini_breakers.equip_b_id,
                electrical_panel_mini_breakers.breaker_size_a,
                electrical_panel_mini_breakers.breaker_size_b,
                electrical_panel_mini_breakers.interlock_a_to_next_b,
                electrical_panel_mini_breakers.interlock_b_to_next_a,
                equip_a.va as va_a,
                equip_b.va as va_b,
                equip_a.description as description_a,
                equip_b.description as description_b,
                equip_a.equip_no as equip_no_a,
                equip_b.equip_no as equip_no_b
                FROM electrical_panel_mini_breakers
                LEFT JOIN electrical_equipment as equip_a ON equip_a.id = electrical_panel_mini_breakers.equip_a_id
                LEFT JOIN electrical_equipment as equip_b ON equip_b.id = electrical_panel_mini_breakers.equip_b_id
                WHERE electrical_panel_mini_breakers.project_id = @projectId
                ";
            await OpenConnectionAsync(Connection);
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectId", projectId);
            MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                miniBreakers.Add(
                    new Circuit(
                        GetSafeString(reader, "id"),
                        GetSafeString(reader, "panel_id"),
                        projectId,
                        string.Empty,
                        GetSafeInt(reader, "circuit_no"),
                        GetSafeInt(reader, "va_a") + GetSafeInt(reader, "va_b"),
                        2020,
                        GetSafeString(reader, "equip_no_a")
                            + "-"
                            + GetSafeString(reader, "description_a")
                            + ";"
                            + GetSafeString(reader, "equip_no_b")
                            + "-"
                            + GetSafeString(reader, "description_b"),
                        0,
                        false,
                        false,
                        GetSafeString(reader, "equip_a_id"),
                        GetSafeString(reader, "equip_b_id"),
                        GetSafeInt(reader, "breaker_size_a"),
                        GetSafeInt(reader, "breaker_size_b"),
                        GetSafeBoolean(reader, "interlock_a_to_next_b"),
                        GetSafeBoolean(reader, "interlock_b_to_next_a"),
                        GetSafeInt(reader, "va_a"),
                        GetSafeInt(reader, "va_b")
                    )
                );
            }
            await reader.CloseAsync();
            await CloseConnectionAsync(Connection);
            return miniBreakers;
        }

        public async Task<ObservableCollection<ElectricalEquipment>> GetProjectEquipment(
            string projectId
        )
        {
            ObservableCollection<ElectricalEquipment> equipments =
                new ObservableCollection<ElectricalEquipment>();
            string query =
                "SELECT * FROM electrical_equipment WHERE project_id = @projectId ORDER BY order_no, date_created";
            await OpenConnectionAsync(Connection);
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
                        GetSafeFloat(reader, "va"),
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
                        GetSafeInt(reader, "mocp_id"),
                        GetSafeString(reader, "hp"),
                        GetSafeBoolean(reader, "has_plug"),
                        GetSafeBoolean(reader, "locking_connector"),
                        GetSafeFloat(reader, "width"),
                        GetSafeFloat(reader, "depth"),
                        GetSafeFloat(reader, "height"),
                        GetSafeInt(reader, "circuit_no"),
                        GetSafeBoolean(reader, "is_hidden_on_plan"),
                        GetSafeInt(reader, "load_type"),
                        GetSafeInt(reader, "order_no"),
                        GetSafeInt(reader, "status_id"),
                        GetSafeInt(reader, "connection_symbol_id"),
                        GetSafeInt(reader, "num_conv_duplex"),
                        GetSafeInt(reader, "circuit_half"),
                        GetSafeFloat(reader, "phase_a_va"),
                        GetSafeFloat(reader, "phase_b_va"),
                        GetSafeFloat(reader, "phase_c_va")
                    )
                );

            await reader.CloseAsync();
            await CloseConnectionAsync(Connection);
            return equipments;
        }

        public async Task<ObservableCollection<ElectricalLighting>> GetProjectLighting(
            string projectId
        )
        {
            ObservableCollection<ElectricalLighting> lightings =
                new ObservableCollection<ElectricalLighting>();
            string query =
                "SELECT * FROM electrical_lighting WHERE project_id = @projectId ORDER BY order_no";
            await OpenConnectionAsync(Connection);
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
                            : reader.GetFloat("wattage"),
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
                        reader.GetString("location_id"),
                        reader.IsDBNull(reader.GetOrdinal("order_no"))
                            ? 0
                            : reader.GetInt32("order_no")
                    )
                );
            }

            await reader.CloseAsync();
            await CloseConnectionAsync(Connection);
            return lightings;
        }

        public async Task<
            ObservableCollection<ElectricalLightingControl>
        > GetProjectLightingControls(string projectId)
        {
            ObservableCollection<ElectricalLightingControl> controls =
                new ObservableCollection<ElectricalLightingControl>();
            string query =
                "SELECT * FROM electrical_lighting_controls WHERE project_id = @projectId";
            await OpenConnectionAsync(Connection);
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectId", projectId);
            MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                controls.Add(
                    new ElectricalLightingControl(
                        GetSafeString(reader, "id"),
                        GetSafeInt(reader, "driver_type_id"),
                        GetSafeBoolean(reader, "occupancy"),
                        GetSafeString(reader, "name")
                    )
                );
            }
            await reader.CloseAsync();
            await CloseConnectionAsync(Connection);
            return controls;
        }

        public async Task<ObservableCollection<ElectricalTransformer>> GetProjectTransformers(
            string projectId
        )
        {
            ObservableCollection<ElectricalTransformer> transformers =
                new ObservableCollection<ElectricalTransformer>();
            string query =
                "SELECT * FROM electrical_transformers WHERE project_id = @projectId ORDER BY order_no";
            await OpenConnectionAsync(Connection);
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
                        reader.IsDBNull(reader.GetOrdinal("parent_distance"))
                            ? 0
                            : reader.GetInt32("parent_distance"),
                        reader.GetString("color_code"),
                        reader.IsDBNull(reader.GetOrdinal("voltage_id"))
                            ? 0
                            : reader.GetInt32("voltage_id"),
                        reader.GetString("name"),
                        reader.IsDBNull(reader.GetOrdinal("kva_id"))
                            ? 0
                            : reader.GetInt32("kva_id"),
                        false,
                        reader.IsDBNull(reader.GetOrdinal("circuit_no"))
                            ? 0
                            : reader.GetInt32("circuit_no"),
                        reader.GetBoolean("is_hidden_on_plan"),
                        reader.GetBoolean("is_wall_mounted"),
                        reader.IsDBNull(reader.GetOrdinal("aic_rating"))
                            ? 0
                            : reader.GetInt32("aic_rating"),
                        reader.IsDBNull(reader.GetOrdinal("order_no"))
                            ? 0
                            : reader.GetInt32("order_no")
                    )
                );
            }
            await reader.CloseAsync();
            await CloseConnectionAsync(Connection);
            return transformers;
        }

        public async Task<ObservableCollection<Location>> GetLightingLocations(string projectId)
        {
            ObservableCollection<Location> locations = new ObservableCollection<Location>();
            string query =
                "SELECT * FROM electrical_lighting_locations WHERE project_id = @projectId";
            await OpenConnectionAsync(Connection);
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectId", projectId);
            MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var location = new Location();
                location.Id = GetSafeString(reader, "id");
                location.isOutside = GetSafeBoolean(reader, "outdoor");
                location.LocationDescription = GetSafeString(reader, "location");
                locations.Add(location);
            }
            await reader.CloseAsync();
            await CloseConnectionAsync(Connection);
            return locations;
        }

        public async Task<ObservableCollection<TimeClock>> GetLightingTimeClocks(string projectId)
        {
            ObservableCollection<TimeClock> clocks = new ObservableCollection<TimeClock>();
            string query =
                "SELECT * FROM electrical_lighting_timeclocks WHERE project_id = @projectId";
            await OpenConnectionAsync(Connection);
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@projectId", projectId);
            MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var clock = new TimeClock();
                clock.Id = reader.GetString("id");
                clock.Name = reader.GetString("name");
                clock.BypassSwitchName = reader.GetString("bypass_switch_name");
                clock.BypassSwitchLocation = reader.GetString("bypass_switch_location");
                clock.VoltageId = reader.GetInt32("voltage_id");
                clock.AdjacentPanelId = reader.GetString("adjacent_panel_id");
                clocks.Add(clock);
            }
            await reader.CloseAsync();
            await CloseConnectionAsync(Connection);
            return clocks;
        }

        public string CreateElectricalPanelMiniBreaker(string panelId, int circuitNo)
        {
            string projectId = string.Empty;
            string id = Guid.NewGuid().ToString();
            string query = "SELECT project_id FROM electrical_panels WHERE id = @panelId";
            OpenConnection(Connection);
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("panelId", panelId);
            MySqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                projectId = GetSafeString(reader, "project_id");
            }
            reader.Close();
            if (string.IsNullOrEmpty(projectId))
            {
                CloseConnection(Connection);
                return string.Empty;
            }
            query =
                @"
                INSERT INTO
                electrical_panel_mini_breakers
                (id, project_id, panel_id, circuit_no) 
                VALUES
                (@id, @projectId, @panelId, @circuitNo)";
            command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("id", id);
            command.Parameters.AddWithValue("projectId", projectId);
            command.Parameters.AddWithValue("panelId", panelId);
            command.Parameters.AddWithValue("circuitNo", circuitNo);
            command.ExecuteNonQuery();
            CloseConnection(Connection);
            return id;
        }

        public string GetElectricalPanelMiniBreakerId(string panelId, int circuitNo)
        {
            string id = string.Empty;
            string query =
                "SELECT id FROM electrical_panel_mini_breakers WHERE panel_id = @panelId AND circuit_no = @circuitNo";
            OpenConnection(Connection);
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("panelId", panelId);
            command.Parameters.AddWithValue("circuitNo", circuitNo);
            MySqlDataReader reader = (MySqlDataReader)command.ExecuteReader();
            if (reader.Read())
            {
                id = GetSafeString(reader, "id");
            }
            reader.Close();
            CloseConnection(Connection);
            return id;
        }

        public (string, string, int, int, int, int) UpdateElectricalPanelMiniBreaker(
            string id,
            string equipAId,
            string equipBId,
            int breakerSizeA,
            int breakerSizeB,
            bool interlockA,
            bool interlockB,
            int circuitNo
        )
        {
            string query =
                @"
                UPDATE electrical_panel_mini_breakers
                SET 
                equip_a_id = @equipAId,
                equip_b_id = @equipBId,
                breaker_size_a = @breakerSizeA,
                breaker_size_b = @breakerSizeB,
                interlock_a_to_next_b = @interlockA,
                interlock_b_to_next_a = @interlockB
                WHERE id = @id";
            OpenConnection(Connection);
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("equipAId", equipAId);
            command.Parameters.AddWithValue("equipBId", equipBId);
            command.Parameters.AddWithValue("breakerSizeA", breakerSizeA);
            command.Parameters.AddWithValue("breakerSizeB", breakerSizeB);
            command.Parameters.AddWithValue("interlockA", interlockA);
            command.Parameters.AddWithValue("interlockB", interlockB);
            command.Parameters.AddWithValue("id", id);
            command.ExecuteNonQuery();
            query =
                @"UPDATE electrical_equipment SET circuit_no = @circuitNo, circuit_half = @circuitHalf WHERE id = @equipAId";
            command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("circuitNo", circuitNo);
            command.Parameters.AddWithValue("circuitHalf", 1);
            command.Parameters.AddWithValue("@equipAId", equipAId);
            command.ExecuteNonQuery();
            query =
                @"UPDATE electrical_equipment SET circuit_no = @circuitNo, circuit_half = @circuitHalf WHERE id = @equipBId";
            command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("circuitNo", circuitNo);
            command.Parameters.AddWithValue("circuitHalf", 2);
            command.Parameters.AddWithValue("@equipBId", equipBId);
            command.ExecuteNonQuery();

            query =
                @"
                SELECT
                equip_a.equip_no as no_a,
                equip_a.description as desc_a,
                equip_a.va as va_a,
                equip_a.voltage_id as volt_a,
                equip_b.equip_no as no_b,
                equip_b.description as desc_b,
                equip_b.va as va_b,
                equip_b.voltage_id as volt_b
                FROM electrical_panel_mini_breakers
                LEFT JOIN electrical_equipment as equip_a ON electrical_panel_mini_breakers.equip_a_id = equip_a.id
                LEFT JOIN electrical_equipment as equip_b ON electrical_panel_mini_breakers.equip_b_id = equip_b.id
                WHERE equip_a.id = @equipAId AND equip_b.id = @equipBId
                ";
            command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("equipAId", equipAId);
            command.Parameters.AddWithValue("equipBId", equipBId);
            MySqlDataReader reader = command.ExecuteReader();
            string noA = string.Empty;
            string descA = string.Empty;
            int vaA = 0;
            int voltA = 0;
            string noB = string.Empty;
            string descB = string.Empty;
            int vaB = 0;
            int voltB = 0;
            if (reader.Read())
            {
                noA = GetSafeString(reader, "no_a");
                descA = GetSafeString(reader, "desc_a");
                vaA = GetSafeInt(reader, "va_a");
                voltA = GetSafeInt(reader, "volt_a");
                noB = GetSafeString(reader, "no_b");
                descB = GetSafeString(reader, "desc_b");
                vaB = GetSafeInt(reader, "va_b");
                voltB = GetSafeInt(reader, "volt_b");
            }
            reader.Close();
            CloseConnection(Connection);
            return ($"{noA}-{descA}", $"{noB}-{descB}", vaA, vaB, voltA, voltB);
        }

        public void DeleteElectricalPanelMiniBreaker(string panelId, int circuitNo)
        {
            string query =
                @"
                DELETE FROM electrical_panel_mini_breakers WHERE panel_id = @panelId AND circuit_no = @circuitNo
                ";
            OpenConnection(Connection);
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("panelId", panelId);
            command.Parameters.AddWithValue("circuitNo", circuitNo);
            command.ExecuteNonQuery();
            query =
                @"
                UPDATE electrical_equipment SET circuit_no = 0, circuit_half = 0 WHERE parent_id = @panelId AND circuit_no = @circuitNo
                ";
            command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("panelId", panelId);
            command.Parameters.AddWithValue("circuitNo", circuitNo);
            command.ExecuteNonQuery();
            CloseConnection(Connection);
        }

        public async Task CloneElectricalProject(string projectId, string newProjectId)
        {
            var services = await GetProjectServices(projectId);
            var panels = await GetProjectPanels(projectId);
            var equipments = await GetProjectEquipment(projectId);
            var lightings = await GetProjectLighting(projectId);
            var lightingControls = await GetProjectLightingControls(projectId);
            var transformers = await GetProjectTransformers(projectId);
            var locations = await GetLightingLocations(projectId);
            var electricalPanelNotes = await GetProjectElectricalPanelNotes(projectId);
            var electricalPanelNoteRels = await GetProjectElectricalPanelNoteRels(projectId);
            var customCircuits = await GetProjectCustomCircuits(projectId);
            var clocks = await GetLightingTimeClocks(projectId);

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
            foreach (var clock in clocks)
            {
                string Id = Guid.NewGuid().ToString();
                //locationIdSwitch.Add(location.Id, Id);
                clock.Id = Id;
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
            foreach (var control in lightingControls)
            {
                string Id = Guid.NewGuid().ToString();
                control.Id = Id;
            }
            foreach (var note in electricalPanelNotes)
            {
                string Id = Guid.NewGuid().ToString();
                note.Id = Id;
                note.ProjectId = newProjectId;
            }
            foreach (var note in electricalPanelNoteRels)
            {
                string Id = Guid.NewGuid().ToString();
                note.Id = Id;
                note.ProjectId = newProjectId;
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
                lightingControls,
                locations,
                electricalPanelNotes,
                electricalPanelNoteRels,
                customCircuits,
                clocks
            );
        }

        public async Task<(string, string)> CheckActiveSessionOnDiscipline(
            string projectNo,
            int disciplineId
        )
        {
            string activeUserName = string.Empty;
            string activeUserId = string.Empty;
            string query =
                @"
                  SELECT last_accessed, employee_id, first_name, last_name
                  FROM sessions
                  LEFT JOIN employees ON employees.id = sessions.employee_id
                  LEFT JOIN contacts ON employees.contact_id = contacts.id
                  WHERE project_no = @projectNo AND discipline_id = @disciplineId 
                  ORDER BY last_accessed DESC
                ";
            await OpenConnectionAsync(SessionConnection);
            MySqlCommand command = new MySqlCommand(query, SessionConnection);
            command.Parameters.AddWithValue("projectNo", projectNo);
            command.Parameters.AddWithValue("disciplineId", disciplineId);

            MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
            if (reader.Read())
            {
                DateTime lastAccessed = GetSafeDateTime(reader, "last_accessed");

                if ((DateTime.Now - lastAccessed).TotalSeconds < 16)
                {
                    activeUserId = GetSafeString(reader, "employee_id");
                    activeUserName =
                        GetSafeString(reader, "first_name")
                        + " "
                        + GetSafeString(reader, "last_name");

                    await reader.CloseAsync();
                }
                else
                {
                    await reader.CloseAsync();
                    query =
                        @"DELETE FROM sessions WHERE project_no = @projectNo AND discipline_id = @disciplineId";
                    command = new MySqlCommand(query, SessionConnection);
                    command.Parameters.AddWithValue("projectNo", projectNo);
                    command.Parameters.AddWithValue("disciplineId", disciplineId);
                    await command.ExecuteNonQueryAsync();
                }
            }
            else
            {
                await reader.CloseAsync();
            }
            await CloseConnectionAsync(SessionConnection);

            return (activeUserId, activeUserName);
        }

        public async Task UpdateSession(string sessionId, string projectNo, int disciplineId)
        {
            string id = Guid.NewGuid().ToString();
            string query =
                @"
                UPDATE sessions SET discipline_id = @disciplineId, project_no = @projectNo
                WHERE id = @sessionId
                ";
            await OpenConnectionAsync(SessionConnection);
            MySqlCommand command = new MySqlCommand(query, SessionConnection);
            command.Parameters.AddWithValue("disciplineId", disciplineId);
            command.Parameters.AddWithValue("projectNo", projectNo);
            command.Parameters.AddWithValue("sessionId", sessionId);
            await command.ExecuteNonQueryAsync();
            await CloseConnectionAsync(SessionConnection);
        }

        public async Task UpdateSessionLastAccessedDate(string sessionId)
        {
            string query =
                @"
                UPDATE sessions SET last_accessed = @lastAccessed WHERE id = @id
                ";
            await OpenConnectionAsync(SessionConnection);
            MySqlCommand command = new MySqlCommand(query, SessionConnection);
            command.Parameters.AddWithValue("id", sessionId);
            command.Parameters.AddWithValue(
                "lastAccessed",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            );
            await command.ExecuteNonQueryAsync();
            await CloseConnectionAsync(SessionConnection);
        }

        public void UpdateEquipmentSync(ElectricalEquipment equipment)
        {
            OpenConnection(Connection);
            string query =
                "UPDATE electrical_equipment SET description = @description, equip_no = @equip_no, parent_id = @parent_id, owner_id = @owner, voltage_id = @voltage, fla = @fla, is_three_phase = @is_3ph, spec_sheet_id = @spec_sheet_id, aic_rating = @aic_rating, spec_sheet_from_client = @spec_sheet_from_client, parent_distance=@distanceFromParent, category_id=@category, color_code = @color_code, connection_type_id = @connection, mocp_id = @mocpId, hp = @hp, has_plug = @has_plug, locking_connector = @locking_connector, width=@width, depth=@depth, height=@height, circuit_no=@circuit_no, is_hidden_on_plan=@is_hidden_on_plan, load_type = @loadType, order_no = @order_no, va=@va, status_id = @statusId, connection_symbol_id = @connectionSymbolId, num_conv_duplex = @numConvDuplex, circuit_half = @circuitHalf WHERE id = @id";
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
            command.Parameters.AddWithValue("@mocpId", equipment.MocpId);
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
            command.Parameters.AddWithValue("@statusId", equipment.StatusId);
            command.Parameters.AddWithValue("@connectionSymbolId", equipment.ConnectionSymbolId);
            command.Parameters.AddWithValue("@numConvDuplex", equipment.NumConvDuplex);
            command.Parameters.AddWithValue("@circuitHalf", equipment.circuitHalf);
            command.ExecuteNonQuery();

            CloseConnection(Connection);
        }

        public void InsertEquipmentSync(string projectId, ElectricalEquipment equipment)
        {
            OpenConnection(Connection);
            string query =
                "INSERT INTO electrical_equipment (id, project_id, equip_no, parent_id, owner_id, voltage_id, fla, is_three_phase, spec_sheet_id, aic_rating, spec_sheet_from_client, parent_distance, category_id, color_code, connection_type_id, description, mca, hp, has_plug, locking_connector, width, depth, height, circuit_no, is_hidden_on_plan, load_type, order_no, va, date_created, status_id, connection_symbol_id, num_conv_duplex) VALUES (@id, @projectId, @equip_no, @parent_id, @owner, @voltage, @fla, @is_3ph, @spec_sheet_id, @aic_rating, @spec_sheet_from_client, @distanceFromParent, @category, @color_code, @connection, @description, @mocp_id, @hp, @has_plug, @locking_connector, @width, @depth, @height, @circuit_no, @is_hidden_on_plan, @loadType, @order_no, @va, @dateCreated, @statusId, @connectionSymbolId, @numConvDuplex)";
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
            command.Parameters.AddWithValue("@mocp_id", equipment.MocpId);
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
            command.Parameters.AddWithValue(
                "@dateCreated",
                equipment.DateCreated.ToString("yyyy-MM-dd HH:mm:ss.fff")
            );
            command.Parameters.AddWithValue("@statusId", equipment.StatusId);
            command.Parameters.AddWithValue("@connectionSymbolId", equipment.ConnectionSymbolId);
            command.Parameters.AddWithValue("@numConvDuplex", equipment.NumConvDuplex);
            command.ExecuteNonQuery();

            CloseConnection(Connection);
        }

        public bool CreateEmployee(
            string username,
            string password,
            string emailAddr,
            string firstName,
            string lastName,
            ulong? phoneNumber,
            uint? extension,
            DateTime hireDate,
            string employeeId,
            string entityId,
            string contactId,
            string emailAddressId,
            string phoneNumberId
        )
        {
            OpenConnection(Connection);
            string query = "SELECT username FROM employees WHERE username = @username";

            MySqlCommand command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@username", username);

            MySqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                MessageBox.Show($"A entry for {username} already exists.");
                reader.Close();
                return false;
            }
            reader.Close();

            var salt = BCrypt.Net.BCrypt.GenerateSalt(10);
            var passhash = BCrypt.Net.BCrypt.HashPassword(password, salt);

            string emailAddrEntityRelId = Guid.NewGuid().ToString();
            string phoneNumberEntityRelId = Guid.NewGuid().ToString();

            query = "INSERT INTO entities (id) VALUES (@entityId)";
            command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@entityId", entityId);
            command.ExecuteNonQuery();

            string gmepCompanyId = "cbc78dfd-7728-4162-a5f2-17e71b112f53";
            query =
                @"
                INSERT INTO contacts (id, entity_id, first_name, last_name, company_id)
                VALUES (@id, @entityId, @firstName, @lastName, @companyId)
                ";
            command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", contactId);
            command.Parameters.AddWithValue("@entityId", entityId);
            command.Parameters.AddWithValue("@firstName", firstName);
            command.Parameters.AddWithValue("@lastName", lastName);
            command.Parameters.AddWithValue("@companyId", gmepCompanyId);
            command.ExecuteNonQuery();

            query =
                @"
                INSERT INTO email_addresses (id, email_address)
                VALUES (@id, @emailAddress)
                ";
            command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", emailAddressId);
            command.Parameters.AddWithValue("@emailAddress", emailAddr);
            command.ExecuteNonQuery();

            query =
                @"
                INSERT INTO email_addr_entity_rel (id, email_address_id, entity_id, is_primary)
                VALUES (@id, @emailAddressId, @entityId, @isPrimary)
                ";
            command = new MySqlCommand(query, Connection);
            command.Parameters.AddWithValue("@id", emailAddrEntityRelId);
            command.Parameters.AddWithValue("@emailAddressId", emailAddressId);
            command.Parameters.AddWithValue("@entityId", entityId);
            command.Parameters.AddWithValue("@isPrimary", 1);
            command.ExecuteNonQuery();

            if (phoneNumber != null && phoneNumber != 0)
            {
                query =
                    @"
                INSERT INTO phone_numbers (id, phone_number, calling_code, extension)
                VALUES (@id, @phoneNumber, @callingCode, @extension)
                ";
                command = new MySqlCommand(query, Connection);
                command.Parameters.AddWithValue("@id", phoneNumberId);
                command.Parameters.AddWithValue("@phoneNumber", phoneNumber);
                command.Parameters.AddWithValue("@callingCode", 1);
                command.Parameters.AddWithValue("@extension", extension);
                command.ExecuteNonQuery();

                query =
                    @"
                INSERT INTO phone_number_entity_rel (id, phone_number_id, entity_id, is_primary)
                VALUES (@id, @phoneNumberId, @entityId, @isPrimary)
                ";
                command = new MySqlCommand(query, Connection);
                command.Parameters.AddWithValue("@id", phoneNumberEntityRelId);
                command.Parameters.AddWithValue("@phoneNumberId", phoneNumberId);
                command.Parameters.AddWithValue("@entityId", entityId);
                command.Parameters.AddWithValue("@isPrimary", 1);
                command.ExecuteNonQuery();
            }

            query =
                @"
                INSERT INTO employees (id, contact_id, employee_title_id, employee_access_level_id, hire_date, username, passhash)
                VALUES (@id, @contactId, @employeeTitleId, @employeeAccessLevel, @hireDate, @username, @passhash)
                ";

            command = new MySqlCommand(query, Connection);

            command.Parameters.AddWithValue("@id", employeeId);
            command.Parameters.AddWithValue("@contactId", contactId);
            command.Parameters.AddWithValue("@employeeTitleId", 0);
            command.Parameters.AddWithValue("@employeeAccessLevel", 0);
            command.Parameters.AddWithValue("@hireDate", hireDate);
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@passhash", passhash);
            command.ExecuteNonQuery();
            CloseConnection(Connection);
            return true;
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

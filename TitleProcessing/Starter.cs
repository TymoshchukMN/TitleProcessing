namespace TitleProcessing
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json;
    using Npgsql;
    using TitleProcessing.Json;

    /// <summary>
    /// Started class.
    /// </summary>
    public static class Starter
    {
        /// <summary>
        /// Luonch programm.
        /// </summary>
        public static void Run()
        {
            Loging log = new Loging();

            log.WriteEvent("Start program..");

            const string ConfFilePath = "N:\\Personal\\TymoshchukMN\\TitleProcessingConfigs";

            var configFile = File.ReadAllText(ConfFilePath);
            var configJSON = JsonConvert.DeserializeObject<Config>(configFile);

            PostgresDB pgDB = new PostgresDB(
                configJSON.DataBaseConfig.Server,
                configJSON.DataBaseConfig.UserName,
                configJSON.DataBaseConfig.DBname,
                configJSON.DataBaseConfig.Port);

            LDAP ldap = new LDAP();
            Email email = new Email(
                configJSON.MailConfig.FromAddress,
                configJSON.MailConfig.ToAddress,
                configJSON.MailConfig.MailServer,
                configJSON.MailConfig.FromPass,
                configJSON.MailConfig.Port);

            List<string> currentTitlesList = new List<string>();

            try
            {
                ldap.GetAllUsers(currentTitlesList);
            }
            catch (ExeptionEmptyLDAPquery ex)
            {
                string message = string.Format(
                    $"Error\n{ex.Message}");

                log.WriteEvent(ex.Message);
                email.SendMail(message);

                return;
            }

            using (NpgsqlConnection connection
                    = new NpgsqlConnection(pgDB.ConnectionString))
            {
                try
                {
                    connection.Open();
                }
                catch (Exception ex)
                {
                    log.WriteEvent(ex.Message);
                    string message = string.Format(
                        $"Error. Cannont connect to DB\n{ex.Message}");

                    log.WriteEvent(ex.Message);
                    email.SendMail(message);

                    return;
                }

                log.WriteEvent("Connected do DB");

                NpgsqlDataReader data;

                pgDB.ProcessingOldTitles(connection);
                pgDB.ProcessingNewTitles(connection, currentTitlesList);

                bool isRowsExist = pgDB.CompareTitles(connection, out data);

                if (isRowsExist)
                {
                    log.WriteEvent("Founded changed Titles");
                    List<string> userWithChangedTitles = new List<string>();

                    while (data.Read())
                    {
                        string row = string.Format(
                            $"{data.GetString(0)};" +
                            $"{data.GetString(1)};" +
                            $"{data.GetString(2)}");

                        userWithChangedTitles.Add(row);
                        log.WriteEvent(row);
                    }

                    // close data reader. Otherwise we'll get error
                    // "A command is already in progress:"
                    data.Close();

                    string[] usersTbl = userWithChangedTitles.ToArray();
                    pgDB.CheckAccessToSystems(
                        connection, usersTbl);

                    email.ProcessEmailBody(usersTbl);
                }
                else
                {
                    log.WriteEvent("No job changes");
                }
            }

            log.WriteEvent("Finish programm");
        }
    }
}

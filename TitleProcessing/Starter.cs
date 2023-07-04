namespace TitleProcessing
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json;
    using Npgsql;
    using TitleProcessing.Encription;
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

            const string ConfFilePathDB = "N:\\Personal\\TymoshchukMN\\TitleProcessingConfigs\\DBconfigFile.json";
            const string ConfFilePathMail = "N:\\Personal\\TymoshchukMN\\TitleProcessingConfigs\\MailConfigFile.json";

            string dbConfigFile = File.ReadAllText(ConfFilePathDB);
            DBConfigJSON dbConfigJSON = JsonConvert.DeserializeObject<DBConfigJSON>(dbConfigFile);

            string mailConfigFile = File.ReadAllText(ConfFilePathMail);
            MailConfigJSON mailConfigJSON = JsonConvert.DeserializeObject<MailConfigJSON>(mailConfigFile);

            PostgresDB pgDB = new PostgresDB(
                dbConfigJSON.DBConfig.Server,
                dbConfigJSON.DBConfig.UserName,
                dbConfigJSON.DBConfig.DBname,
                dbConfigJSON.DBConfig.Port);

            LDAP ldap = new LDAP();
            Email email = new Email(
                mailConfigJSON.MailConfig.FromAddress,
                mailConfigJSON.MailConfig.ToAddress,
                mailConfigJSON.MailConfig.MailServer,
                Decrypt.DecryptPass(mailConfigJSON.MailConfig.FromPass),
                mailConfigJSON.MailConfig.Port);

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

namespace TitleProcessing
{
    using System;
    using System.Collections.Generic;
    using Npgsql;

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
            Console.WriteLine("Start program..");

            PostgresDB pgDB = new PostgresDB();
            LDAP ldap = new LDAP();
            Email email = new Email();

            List<string> currentTitlesList = new List<string>();

            try
            {
                ldap.GetAllUsers(currentTitlesList, log);
            }
            catch (ExeptionEmptyLDAPquery ex)
            {
                string message = string.Format("Error\n{0}"
                    , ex.Message);

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
                    string message = string.Format("Error. Cannont connect to DB\n{0}"
                        , ex.Message);

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
                        string row = string.Format("{0};{1};{2}"
                       , data.GetString(0)
                       , data.GetString(1)
                       , data.GetString(2));

                        userWithChangedTitles.Add(row);
                        log.WriteEvent(row);
                    }

                    // close data reader. Otherwise we'll get error
                    // "A command is already in progress:"
                    data.Close();


                    string[] usersTbl = userWithChangedTitles.ToArray();
                    pgDB.CheckAccessToSystems(connection
                                , usersTbl);

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

//////////////////////////////////////////
// Author : Tymoshchuk Maksym
// Created On : 10/04/2023
// Last Modified On : 14/04/2023
// Description: Workking with Postgres
// Project: TitleProcessing
//////////////////////////////////////////


using System;
using System.Collections.Generic;
using Npgsql;
using System.Data;

namespace TitleProcessing
{
    internal class PostgresDB
    {

        const string SERVER = "192.168.220.102";
        const ushort PORT = 5432;
        const string DB_NAME = "Access_list";
        const string USER_NAME = "access_mng";

        #region FIELDS

        private string _connectionString;
        private string[] _DBtables = { "1C7Dymerka", "1C7Shops", "1C7Torg"
                , "Zoom"};

        #endregion FIELDS


        #region PROPERTIES

        /// <summary>
        /// Get DB-connection string
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
        }

        #endregion PROPERTIES

        #region CTORs

        /// <summary>
        /// Default ctor
        /// </summary>
        public PostgresDB()
        {
            _connectionString = string.Format(
                    "Server={0};Username={1};Database={2};Port={3};Password={4}",
                    SERVER,
                    USER_NAME,
                    DB_NAME,
                    PORT,
                    string.Empty);           
        }


        #endregion CTORs


        #region METHODS

        /// <summary>
        /// Processing old titles in DB
        /// </summary>
        public void ProcessingOldTitles(NpgsqlConnection connection)
        {
            NpgsqlCommand npgsqlCommand = connection.CreateCommand();
            npgsqlCommand.CommandText =
                @"
                    BEGIN;
                    TRUNCATE oldTitles;


                    INSERT INTO oldTitles
                    SELECT titles.samaccountname ,
                           titles.title
                    FROM titles;

                    COMMIT;
                ";
            npgsqlCommand.ExecuteNonQuery();
        }

        /// <summary>
        /// Processing current Titles
        /// </summary>
        /// <param name="connection">
        /// DB-connection
        /// </param>
        public void ProcessingNewTitles(NpgsqlConnection connection
                , List<string> currentTitlesList)
        {
            NpgsqlCommand npgsqlCommand = connection.CreateCommand();

            npgsqlCommand.CommandText =
                @"
                    BEGIN;
                    TRUNCATE titles;
                ";
            npgsqlCommand.ExecuteNonQuery();

            for (int i = 0; i < currentTitlesList.Count; ++i)
            {
                string samAccountName = currentTitlesList[i].Split(';')[0];
                string title = currentTitlesList[i].Split(';')[1].Replace("'", "`");

                npgsqlCommand.CommandText
                    = string.Format("INSERT INTO titles VALUES ('{0}','{1}');"
                    , samAccountName, title);

                npgsqlCommand.ExecuteNonQuery();
            }
            npgsqlCommand.CommandText = "COMMIT;";
            npgsqlCommand.ExecuteNonQuery();
        }                

        /// <summary>
        /// Compare old and new titles
        /// </summary>
        /// <param name="connection">
        /// DB-connection
        /// </param>
        public bool CompareTitles(NpgsqlConnection connection
                , out NpgsqlDataReader data)
        {
            NpgsqlCommand npgsqlCommand = connection.CreateCommand();

            npgsqlCommand.CommandText =
                @"
                CREATE TEMP TABLE tmpTbl AS
                SELECT t1.samaccountname,
                       oldtitles.title AS ""oldTitle"" ,
                       t1.title AS ""newTitle""
                FROM
                  (SELECT samaccountname,
                          title
                   FROM titles
                   EXCEPT SELECT samaccountname,
                                 title
                   FROM oldtitles) AS t1
                INNER JOIN oldTitles ON t1.samaccountname = oldtitles.samaccountname;
                ";

            npgsqlCommand.ExecuteNonQuery();
            npgsqlCommand.CommandText = "SELECT * FROM tmpTbl;";

            data = npgsqlCommand.ExecuteReader();
            
            return data.HasRows;
           
        }

        /// <summary>
        /// Check access to additional systems
        /// </summary>
        /// <param name="connection">
        /// DB-connector
        /// </param>
        /// <param name="usersTbl">
        /// Table with user`s information
        /// </param>
        public void CheckAccessToSystems(NpgsqlConnection connection
            , string [] usersTbl)
        {
            NpgsqlCommand npgsqlCommand = connection.CreateCommand();
            NpgsqlDataReader data;

           

            string samaccountname = usersTbl[0].Substring(0
                    , usersTbl[0].IndexOf(";"));

            for (int j = 0; j < usersTbl.Length; j++)
            {
                string systemsWithAccess = string.Empty;

                for (ushort i = 0; i < _DBtables.Length; ++i)
                {
                    string command = string.Format(
                    @"SELECT CASE
                    WHEN
	                    (
	                    (SELECT EXISTS
		                    (SELECT *
		                    FROM ""{0}""
		                    WHERE samaccountname = '{1}') = TRUE
	                    )
	                AND
	                    (
		                    SELECT ""isEnable""
		                    FROM  ""{0}""
		                    WHERE samaccountname ='{1}') = TRUE
	                    ) 
                    THEN 'exist'
                    ELSE 'NOT exist'
                    END;", _DBtables[i], samaccountname);

                    npgsqlCommand.CommandText = command;

                    data = npgsqlCommand.ExecuteReader();

                    DataTable isAccessExist = new DataTable();
                    isAccessExist.Load(data);
                    //data.Rows
                    if ((string)isAccessExist.Rows[0].ItemArray[0] == "exist")
                    {
                        systemsWithAccess += _DBtables[i] + ", ";
                    }

                    data.Close();
                }

                usersTbl[j] = usersTbl[j] + ";" + systemsWithAccess;
            }                     
        }



        /// <summary>
        /// Print result of query to console
        /// </summary>
        /// <param name="npgsqlCommand">
        /// SQL-query
        /// </param>
        public void PrintResultSet(NpgsqlCommand npgsqlCommand)
        {
            NpgsqlDataReader reader = npgsqlCommand.ExecuteReader();
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            while (reader.Read())
            {
                Console.WriteLine($"{reader.GetString(0)}\t" +
                    $"{reader.GetString(1)}");
            }

        }

        #endregion METHODS

    }
}

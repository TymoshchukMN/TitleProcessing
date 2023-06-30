//////////////////////////////////////////
// Author : Tymoshchuk Maksym
// Created On : 10/04/2023
// Last Modified On : 14/04/2023
// Description: Workking with Postgres
// Project: TitleProcessing
//////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using TitleProcessing.DB;

namespace TitleProcessing
{
    internal class PostgresDB
    {
        #region FIELDS

        private readonly string[] _dBtables =
        {
            "1C7Dymerka",
            "1C7Shops",
            "1C7Torg",
            "Zoom",
        };

        private string _connectionString;
        #endregion FIELDS

        #region CTORs

        public PostgresDB(
            string server,
            string userName,
            string dataBase,
            int port)
        {
            _connectionString = string.Format(
                    $"Server={server};" +
                    $"Username={userName};" +
                    $"Database={dataBase};" +
                    $"Port={port};" +
                    $"Password={string.Empty}");
        }

        #endregion CTORs

        #region PROPERTIES

        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
        }

        #endregion PROPERTIES

        #region METHODS

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

        public void ProcessingNewTitles(
            NpgsqlConnection connection,
            List<string> currentTitlesList)
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
                string title =
                    currentTitlesList[i].Split(';')[1].Replace("'", "`");

                npgsqlCommand.CommandText
                    = string.Format(
                        $"INSERT INTO titles VALUES " +
                        $"('{samAccountName}','{title}');");

                npgsqlCommand.ExecuteNonQuery();
            }

            npgsqlCommand.CommandText = "COMMIT;";
            npgsqlCommand.ExecuteNonQuery();
        }

        public bool CompareTitles(
            NpgsqlConnection connection,
            out NpgsqlDataReader data)
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
        /// Check access to additional systems.
        /// </summary>
        /// <param name="connection">
        /// DB-connector.
        /// </param>
        /// <param name="usersTbl">
        /// Table with user`s information.
        /// </param>
        public void CheckAccessToSystems(
            NpgsqlConnection connection,
            string[] usersTbl)
        {
            NpgsqlCommand npgsqlCommand = connection.CreateCommand();
            NpgsqlDataReader data;

            string samaccountname = usersTbl[0].Substring(
                0,
                usersTbl[0].IndexOf(";"));

            VerifiableDB verifiableDB = VerifiableDB.GetInstance();
            for (int j = 0; j < usersTbl.Length; j++)
            {
                string systemsWithAccess = string.Empty;

                for (ushort i = 0; i < verifiableDB.VerifiableDBValue.Length; ++i)
                {
                    string command = string.Format(
                    $"SELECT CASE" +
                    $"WHEN" +
                    $"(" +
                    $"  (SELECT EXISTS" +
                    $"      (SELECT *" +
                    $"      FROM \"{verifiableDB.VerifiableDBValue[i]}\"" +
                    $"       WHERE samaccountname = '{samaccountname}') = TRUE" +
                    $"   )" +
                    $"AND" +
                    $"   (" +
                    $"   SELECT \"isEnable\"" +
                    $"   FROM  \"{verifiableDB.VerifiableDBValue[i]}\"" +
                    $"   WHERE samaccountname ='{samaccountname}') = TRUE" +
                    $"  )" +
                    $"THEN 'exist'" +
                    $"ELSE 'NOT exist'" +
                    $"END;");

                    npgsqlCommand.CommandText = command;

                    data = npgsqlCommand.ExecuteReader();

                    DataTable isAccessExist = new DataTable();
                    isAccessExist.Load(data);

                    if ((string)isAccessExist.Rows[0].ItemArray[0] == "exist")
                    {
                        systemsWithAccess += verifiableDB.VerifiableDBValue[i] + ", ";
                    }

                    data.Close();
                }


                //for (ushort i = 0; i < _dBtables.Length; ++i)
                //{
                //    string command = string.Format(
                //    $"SELECT CASE" +
                //    $"WHEN" +
                //    $"(" +
                //    $"  (SELECT EXISTS" +
                //    $"      (SELECT *" +
                //    $"      FROM \"{_dBtables[i]}\"" +
                //    $"       WHERE samaccountname = '{samaccountname}') = TRUE" +
                //    $"   )" +
                //    $"AND" +
                //    $"   (" +
                //    $"   SELECT \"isEnable\"" +
                //    $"   FROM  \"{_dBtables[i]}\"" +
                //    $"   WHERE samaccountname ='{samaccountname}') = TRUE" +
                //    $"  )" +
                //    $"THEN 'exist'" +
                //    $"ELSE 'NOT exist'" +
                //    $"END;");

                //    npgsqlCommand.CommandText = command;

                //    data = npgsqlCommand.ExecuteReader();

                //    DataTable isAccessExist = new DataTable();
                //    isAccessExist.Load(data);

                //    if ((string)isAccessExist.Rows[0].ItemArray[0] == "exist")
                //    {
                //        systemsWithAccess += _dBtables[i] + ", ";
                //    }

                //    data.Close();
                //}

                usersTbl[j] = usersTbl[j] + ";" + systemsWithAccess;
            }
        }

        /// <summary>
        /// Print result of query to console.
        /// </summary>
        /// <param name="npgsqlCommand">
        /// SQL-query.
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

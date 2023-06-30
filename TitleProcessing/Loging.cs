//////////////////////////////////////////
// Author : Tymoshchuk Maksym
// Created On : 12/04/2023
// Last Modified On : 
// Description: Logging software working
// Project: TitleProcessing
//////////////////////////////////////////

using System;
using System.Text;
using System.IO;

namespace TitleProcessing
{
    internal class Loging
    {
        const string LOG_FILE_PATH = "C:\\swsetup\\TitleProcessing.log";
        private StreamWriter _sw;

        public Loging()
        {
            CreateLogFile();
        }

        ~Loging()
        {
            _sw.Close();
        }

        /// <summary>
        /// Check if log-file exist, and create if it doesn`t
        /// </summary>
        private void CreateLogFile()
        {
            if (File.Exists(LOG_FILE_PATH))
            {
                File.WriteAllText(LOG_FILE_PATH, string.Empty, Encoding.UTF8);
                _sw = new StreamWriter(LOG_FILE_PATH);                
            }
            else
            {
                File.Create(LOG_FILE_PATH);
                _sw = new StreamWriter(LOG_FILE_PATH);
            }
        }

        /// <summary>
        /// Write Event to log-file
        /// </summary>
        /// <param name="message">
        /// Event for logging
        /// </param>
        public void WriteEvent(string message) 
        {
            string log = string.Format("{0}\t{1}",DateTime.Now, message);

            _sw.WriteLine(log,true,Encoding.UTF8);
            
        }
    }
}

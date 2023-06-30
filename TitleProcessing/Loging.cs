//////////////////////////////////////////
// Author : Tymoshchuk Maksym
// Created On : 12/04/2023
// Last Modified On :
// Description: Logging software working
// Project: TitleProcessing
//////////////////////////////////////////

using System;
using System.IO;
using System.Text;

namespace TitleProcessing
{
    internal class Loging
    {
        const string LogfilePath = "C:\\swsetup\\TitleProcessing.log";
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
        /// Write Event to log-file.
        /// </summary>
        /// <param name="message">
        /// Event for logging.
        /// </param>
        public void WriteEvent(string message)
        {
            string log = string.Format("{0}\t{1}", DateTime.Now, message);

            _sw.WriteLine(log, true, Encoding.UTF8);
        }

        /// <summary>
        /// Check if log-file exist, and create if it doesn`t.
        /// </summary>
        private void CreateLogFile()
        {
            if (File.Exists(LogfilePath))
            {
                File.WriteAllText(LogfilePath, string.Empty, Encoding.UTF8);
                _sw = new StreamWriter(LogfilePath);
            }
            else
            {
                File.Create(LogfilePath);
                _sw = new StreamWriter(LogfilePath);
            }
        }
    }
}

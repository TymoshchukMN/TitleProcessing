//////////////////////////////////////////
// Author : Tymoshchuk Maksym
// Created On : 11/04/2023
// Last Modified On : 14/04/2023
// Description: Sending an email
// Project: TitleProcessing
//////////////////////////////////////////

using System.Collections.Generic;
using System.IO.Packaging;
using System.Net;
using System.Net.Mail;

namespace TitleProcessing
{
    internal class Email
    {
        #region COSNTANTS

        const string MailSubject = "Changed Titles Report";

        #endregion COSNTANTS

        #region FIELDS

        private string _body =
            @"
            <p>Titles was chanched for users:<br></p>

            <table border='1' align='Left' cellpadding='2' cellspacing='0' style='color:black;font-family:arial,helvetica,sans-serif;text-align:Ledt;'>
            <tr style = 'font-size:12px;font-weight: normal;background: #E673E8;background-color: #E673E8;' >
                <th align = Center>
                    <b>
                        Login
                    </b>
                </th>
                <th align = Center >
                    <b>
                        new Title
                    </b>
                </th>
                <th align = Center >
                    <b>
                        old Title
                    </b>
                </th >
                <th align = Center >
                    <b>
                        access to systems
                    </b>
                </th >
             </tr > ";

        private SmtpClient _smtp;
        private MailAddress _fromAddress;
        private MailAddress _toAddress;

        #endregion FIELDS

        #region CTORs

        public Email(
            string fromddress,
            string toAddress,
            string mailServer,
            string fromPass,
            int port)
        {
            _fromAddress = new MailAddress(fromddress);
            _toAddress = new MailAddress(toAddress);
            _smtp = new SmtpClient
            {
                Host = mailServer,
                Port = port,
                EnableSsl = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials =
                    new NetworkCredential(fromddress, fromPass),
            };
        }

        #endregion CTORs

        ~Email()
        {
            _smtp.Dispose();
        }

        #region METHODS

        /// <summary>
        /// Send en email.
        /// </summary>
        /// <param name="body">
        /// Body message.
        /// </param>
        public void SendMail(string body)
        {
            using
            (
                MailMessage message = new MailMessage(_fromAddress, _toAddress)
                {
                    Subject = MailSubject,
                    Body = body,
                    IsBodyHtml = true,
                })
            {
                _smtp.Send(message);
            }
        }

        /// <summary>
        /// Processing mai.
        /// </summary>
        /// <param name="usersTbl">
        /// Table with users.
        /// </param>
        public void ProcessEmailBody(string[] usersTbl)
        {
            for (int i = 0; i < usersTbl.Length; i++)
            {
                string sasAMAccountName = usersTbl[i].Split(';')[0];
                string oldTitle = usersTbl[i].Split(';')[1];
                string newTitle = usersTbl[i].Split(';')[2];
                string systems = usersTbl[i].Split(';')[3];

                string row = string.Format(
                    $"" +
                    $"<tr style='font-size:12px;background-color:#FFFFFF'>" +
                    $"  <td>{sasAMAccountName}</td>" +
                    $"  <td>{newTitle}</td>" +
                    $"  <td>{oldTitle}</td>" +
                    $"  <td>{systems}</td>" +
                    $"</tr > ");

                _body += row;
            }

            SendMail(_body);
        }

        #endregion METHODS

    }
}

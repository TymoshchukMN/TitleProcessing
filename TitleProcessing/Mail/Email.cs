﻿//////////////////////////////////////////
// Author : Tymoshchuk Maksym
// Created On : 11/04/2023
// Last Modified On : 14/04/2023
// Description: Sending an email
// Project: TitleProcessing
//////////////////////////////////////////

using System.Collections.Generic;
using System.Net;
using System.Net.Mail;

namespace TitleProcessing
{
    internal class Email
    {
        #region COSNTANTS

        const string FromAddress = "ReportSender@comfy.ua";
        const string ToAddress = "TymoshchukMN@comfy.ua";
        const string MailServer = "172.16.5.8";
        const string FromPass = "pnIioQRN";
        const string MailSubject = "Changed Titles Report";
        const ushort Port = 465;

        #endregion COSNTANTS

        #region FIELDS

        private string _body =
            @"
            <p>Titles was chanched for users:<br></p>

            <table border='1' align='Left' cellpadding='2' cellspacing='0' style='color:black;font-family:arial,helvetica,sans-serif;text-align:Ledt;'>
            <tr style = 'font-size:12px;font-weight: normal;background: #FFFFFF;background-color: #32CD32;' >
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

        public Email()
        {
            _fromAddress = new MailAddress(FromAddress);
            _toAddress = new MailAddress(ToAddress);
            _smtp = new SmtpClient
            {
                Host = MailServer,
                Port = Port,
                EnableSsl = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials =
                    new NetworkCredential(_fromAddress.Address, FromPass),
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
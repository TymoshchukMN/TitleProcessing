//////////////////////////////////////////
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

        const string FROM_ADDRESS   = "ReportSender@comfy.ua";
        const string TO_ADDRESS     = "TymoshchukMN@comfy.ua";
        const string MAIL_SERVER    = "172.16.5.8";
        const string FROM_PASSWORD  = "pnIioQRN";
        const string MAIL_SUBJECT   = "Changed Titles Report";
        const ushort PORT           = 465;

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
            _fromAddress = new MailAddress(FROM_ADDRESS);
            _toAddress = new MailAddress(TO_ADDRESS);
            _smtp = new SmtpClient
            {
                Host = MAIL_SERVER,
                Port = PORT,
                EnableSsl = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_fromAddress.Address, FROM_PASSWORD)
            };
        }

        #endregion CTORs

        /// <summary>
        /// destructor
        /// </summary>
        ~Email() 
        {
            _smtp.Dispose();
        }

        #region METHODS

        /// <summary>
        /// Send en email
        /// </summary>
        /// <param name="body">
        /// Body message
        /// </param>
        public void SendMail(string body)
        {
            using
            (
                MailMessage message = new MailMessage(_fromAddress, _toAddress)
                {
                    Subject = MAIL_SUBJECT,
                    Body = body,
                    IsBodyHtml = true
                }
            )
            {
                _smtp.Send(message);
                
            }
        }

        /// <summary>
        /// Processing mai;
        /// </summary>
        /// <param name="userWithChangedTitles"></param>
        public void ProcessEmailBody(string []usersTbl)
        {

            for (int i = 0; i < usersTbl.Length; i++)
            {
                string sasAMAccountName = usersTbl[i].Split(';')[0];
                string oldTitle = usersTbl[i].Split(';')[1];
                string newTitle = usersTbl[i].Split(';')[2];
                string systems = usersTbl[i].Split(';')[3];

                string row = string.Format(
                    @"
                        <tr style='font-size:12px;background-color:#FFFFFF'>
                            <td>{0}</td>
                            <td>{1}</td>
                            <td>{2}</td>
                            <td>{3}</td>
                        </tr > ", sasAMAccountName, newTitle, oldTitle, systems);

                _body += row;
            }

            SendMail(_body);
        }

        #endregion METHODS

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitleProcessing.Json
{
    public class MailConfig
    {
        public string FromAddress { get; set; }

        public string ToAddress { get; set; }

        public string MailServer { get; set; }

        public string FromPass { get; set; }

        public int Port { get; set; }
    }
}

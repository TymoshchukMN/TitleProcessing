using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitleProcessing
{
    public class ExeptionEmptyLDAPquery:Exception
    {

        public override string Message
        {
            get
            {
                return "LDAP query returned empty";
            }
        }

    }
}

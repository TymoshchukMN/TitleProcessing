using System;

namespace TitleProcessing
{
    public class ExeptionEmptyLDAPquery : Exception
    {
        public ExeptionEmptyLDAPquery()
        {
        }

        public ExeptionEmptyLDAPquery(string message)
            : base(message)
        {
        }

        public override string Message
        {
            get
            {
                return "LDAP query returned empty";
            }
        }
    }
}

//////////////////////////////////////////
// Author : Tymoshchuk Maksym
// Created On : 11/04/2023
// Last Modified On :
// Description: Work with LDAP
// Project: TitleProcessing
//////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;

namespace TitleProcessing
{
    internal class LDAP
    {
        #region FIELDS

        /// <summary>
        /// Current domain name.
        /// </summary>
        private string _domainName;

        #endregion FIELDS

        #region CTORs

        public LDAP()
        {
            _domainName = GetCurrentDomainPath();
        }

        #endregion CTORs

        #region METHODS

        /// <summary>
        /// Get users from active directory.
        /// </summary>
        /// <param name="currentTitles">
        /// List for filleing titles and logins.
        /// </param>
        public void GetAllUsers(List<string> currentTitles)
        {
            SearchResultCollection results;
            DirectorySearcher directorySearcher;
            DirectoryEntry directoryEntry = new DirectoryEntry(_domainName);

            directorySearcher = new DirectorySearcher(directoryEntry);

            directorySearcher.Filter = "(&(&(&(&(&(&(&(&(objectCategory=user)" +
                "(!objectClass=contact)" +
                "(!((userAccountControl:1.2.840.113556.1.4.803:=2)))" +
                "(!sAMAccountName=**_**)(&(Title=*))))))))))";

            const int PAGE_SIZE = 1000;
            directorySearcher.PageSize = PAGE_SIZE;
            directorySearcher.PropertiesToLoad.Add("sAMAccountName");
            directorySearcher.PropertiesToLoad.Add("title");

            results = directorySearcher.FindAll();

            if (results.Count == 0)
            {
                throw new ExeptionEmptyLDAPquery("LDAP query returned empty");
            }

            foreach (SearchResult sr in results)
            {
                if (!sr.Path.Contains("OU=Trash")
                    && !sr.Path.Contains("OU=HRFutureStaff")
                    && !sr.Path.Contains("OU=HRMaternity")
                    && !sr.Path.Contains("OU=HRMobilized"))
                {
                    currentTitles.Add(
                   string.Format(
                       $"{sr.Properties["sAMAccountName"][0]};" +
                       $"{sr.Properties["title"][0]}"));
                }
            }
        }

        /// <summary>
        /// Ger root domain.
        /// </summary>
        /// <returns>
        /// Domain name.
        /// </returns>
        private string GetCurrentDomainPath()
        {
            DirectoryEntry de = new DirectoryEntry("LDAP://RootDSE");

            return "LDAP://" + de.Properties["defaultNamingContext"][0].ToString();
        }
        #endregion METHODS
    }
}

// <copyright file="DkimSettingsHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Helpers
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Helper for checking if domain name is matching with applicable domain name.
    /// </summary>
    public class DkimSettingsHelper
    {
        /// <summary>
        /// Check if domain name matches with applicable domain name.
        /// </summary>
        /// <param name="domainName">The domain name.</param>
        /// <param name="applicableDomainNameList">The applicable domain list.</param>
        /// <returns>returns true if domain name matches with applicable domain name.</returns>
        public static bool IsDomainMatchesWithAnyApplicableDomain(string domainName, List<string> applicableDomainNameList)
        {
            return ApplicableDomainNameEndsWithDomainName(domainName, applicableDomainNameList) || ApplicableDomainNameEndsWithWildCard(domainName, applicableDomainNameList);
        }

        /// <summary>
        /// Check if any applicable domain name ends with domain name.
        /// </summary>
        /// <param name="domainName">The domain name.</param>
        /// <param name="applicableDomainNameList">The applicable domain list.</param>
        /// <returns>returns true if domain name matches with applicable domain name.</returns>
        public static bool ApplicableDomainNameEndsWithDomainName(string domainName, List<string> applicableDomainNameList)
        {
            return applicableDomainNameList.Any(s => !domainName.Contains('*') && s.EndsWith(domainName));
        }

        /// <summary>
        /// Check if applicable domain name contains a wild card and if it matches with the domain name.
        /// </summary>
        /// <param name="domainName">The domain name.</param>
        /// <param name="applicableDomainNameList">The applicable domain list.</param>
        /// <returns>returns true if domain name matches with applicable domain name.</returns>
        public static bool ApplicableDomainNameEndsWithWildCard(string domainName, List<string> applicableDomainNameList)
        {
            return applicableDomainNameList.Any(s => s.Contains('*') && domainName.EndsWith(s.Substring(s.LastIndexOf('*') + 1, s.Length - 1)));
        }
    }
}

// <copyright file="InternetAddressHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Helpers
{
    using System.Collections.Generic;
    using MimeKit;

    /// <summary>
    /// Static helper for handling Internet Address.
    /// </summary>
    public class InternetAddressHelper
    {
        public static IEnumerable<InternetAddress> ConvertEmailAddressesToMailBoxAddresses(IEnumerable<string> emailAddresses)
        {
            List<InternetAddress> mailboxAddresses = new List<InternetAddress>();
            foreach (var emailAddress in emailAddresses)
            {
                if (emailAddress.Contains(';'))
                {
                    string[] emails = emailAddress.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var email in emails)
                    {
                        mailboxAddresses.Add(InternetAddress.Parse(email.Trim()));
                    }
                }
                else
                {
                    mailboxAddresses.Add(InternetAddress.Parse(emailAddress.Trim()));
                }
            }

            return mailboxAddresses;
        }
    }
}

// <copyright file="EmailViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Report
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DotLiquid;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel.Email;

    /// <summary>
    /// Email view model for dot liquid template.
    /// </summary>
    public class EmailViewModel : Drop
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailViewModel"/> class.
        /// </summary>
        /// <param name="emailDetail">The email data.</param>
        /// <param name="invitationLinkHost">The email invitation link host value in app setting.</param>
        public EmailViewModel(IEmailDetails emailDetail, string invitationLinkHost)
        {
            this.To = emailDetail.Recipient.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            this.From = emailDetail.From;
            this.CC = emailDetail.CC.IsNotNullOrEmpty()
                ? emailDetail.CC.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();
            this.BCC = emailDetail.BCC.IsNotNullOrEmpty()
                ? emailDetail.BCC.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();
            this.Subject = emailDetail.Subject;
            this.SentDate = emailDetail.CreatedTimestamp.ToLocalDateInAet().ToMMDDYYYWithSlashes();
            this.SentTime = emailDetail.CreatedTimestamp.ToLocalTimeInAet().To12HrFormat();
            this.HtmlBody = emailDetail.HtmlMessage.EscapeDoubleQuotesByDoubling();
            this.PlainTextBody = emailDetail.PlainMessage.EscapeDoubleQuotesByDoubling();
            this.ProductName = emailDetail.ProductName;
            this.ProductEnvironment = emailDetail.Environment != DeploymentEnvironment.None
                ? emailDetail.Environment.ToString() : string.Empty;
            this.QuoteType = emailDetail.Quote?.Type.ToString() ?? string.Empty;
            this.QuoteReference = emailDetail.Quote?.QuoteNumber ?? string.Empty;
            this.PolicyNumber = emailDetail.Policy?.PolicyNumber ?? string.Empty;
            this.ClaimNumber = emailDetail.Claim?.ClaimNumber ?? string.Empty;
            this.Attachments
                = emailDetail.EmailAttachments.Select(a => new EmailAttachmentViewModel(a, invitationLinkHost));
            this.User = emailDetail.User != null ? new EmailUserViewModel(emailDetail.User) : null;
            this.Customer = emailDetail.Customer != null ? new EmailCustomerViewModel(emailDetail.Customer) : null;
        }

        /// <summary>
        /// Gets the recipients of the email.
        /// </summary>
        public IEnumerable<string> To { get; }

        /// <summary>
        /// Gets the sender of the email.
        /// </summary>
        public string From { get; }

        /// <summary>
        /// Gets the CCs of the email.
        /// </summary>
        public IEnumerable<string> CC { get; }

        /// <summary>
        /// Gets the BCCs of the email.
        /// </summary>
        public IEnumerable<string> BCC { get; }

        /// <summary>
        /// Gets the subject of the email.
        /// </summary>
        public string Subject { get; }

        /// <summary>
        /// Gets the sent date of the email.
        /// </summary>
        public string SentDate { get; }

        /// <summary>
        /// Gets the sent time of the email.
        /// </summary>
        public string SentTime { get; }

        /// <summary>
        /// Gets the html body of the email.
        /// </summary>
        public string HtmlBody { get; }

        /// <summary>
        /// Gets the plain text body of the email.
        /// </summary>
        public string PlainTextBody { get; }

        /// <summary>
        /// Gets the product name of the email.
        /// </summary>
        public string ProductName { get; }

        /// <summary>
        /// Gets the environment of the email.
        /// </summary>
        public string ProductEnvironment { get; }

        /// <summary>
        /// Gets the quote type.
        /// </summary>
        public string QuoteType { get; }

        /// <summary>
        /// Gets the reference number.
        /// </summary>
        public string QuoteReference { get; }

        /// <summary>
        /// Gets the policy number.
        /// </summary>
        public string PolicyNumber { get; }

        /// <summary>
        /// Gets the claim number.
        /// </summary>
        public string ClaimNumber { get; }

        /// <summary>
        /// Gets the email attachments of the email.
        /// </summary>
        public IEnumerable<EmailAttachmentViewModel> Attachments { get; }

        /// <summary>
        /// Gets user of the email.
        /// </summary>
        public EmailUserViewModel User { get; }

        /// <summary>
        /// Gets the customer of the email.
        /// </summary>
        public EmailCustomerViewModel Customer { get; }
    }
}

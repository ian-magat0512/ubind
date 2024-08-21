// <copyright file="SystemEmailTemplateData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using System.Reflection;
    using System.Resources;
    using UBind.Domain.Entities;

    /// <summary>
    /// Data for generating and sending a system email.
    /// </summary>
    public class SystemEmailTemplateData
    {
        private const string NoReplyEmailAddress = "no-reply@ubind.io";
        private const string UserEmailLiquidTemplate = "{{User.Email}}";
        private const string PersonEmailLiquidTemplate = "{{Person.Email}}";

        private static readonly Lazy<ResourceManager> DefaultResourceManager = new Lazy<ResourceManager>(
            () => new ResourceManager(
                SystemEmailTemplateDataHelper.ResouceFileLocation, Assembly.GetExecutingAssembly()));

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemEmailTemplateData"/> class.
        /// </summary>
        /// <param name="subject">A dot liquid template for generating the subject.</param>
        /// <param name="fromAddress">A dot liquid template for generating the from address.</param>
        /// <param name="toAddress">A dot liquid template for generating the to address.</param>
        /// <param name="cc">A comma-separated list of emails to CC the email to.</param>
        /// <param name="bcc">A comma-separated list of emails to BCC the email to.</param>
        /// <param name="htmlBody">A dot liquid template for generating the html body.</param>
        /// <param name="plainTextBody">A dot liquid template for generating the plain text body.</param>
        /// <param name="smtpServerHost">The host of the smtp server to use to send the email.</param>
        /// <param name="smtpServerPort">The port of the smtp server to use to send the email.</param>
        public SystemEmailTemplateData(
            string subject,
            string fromAddress,
            string toAddress,
            string cc,
            string bcc,
            string htmlBody,
            string plainTextBody,
            string smtpServerHost,
            int? smtpServerPort)
        {
            this.Subject = subject;
            this.FromAddress = fromAddress;
            this.ToAddress = toAddress;
            this.Cc = cc;
            this.Bcc = bcc;
            this.HtmlBody = htmlBody;
            this.PlainTextBody = plainTextBody;
            this.SmtpServerHost = smtpServerHost;
            this.SmtpServerPort = smtpServerPort;
        }

        // Parameterless constructor for EF.
        private SystemEmailTemplateData()
        {
        }

        /// <summary>
        /// Gets the default template data for an activation email.
        /// </summary>
        public static SystemEmailTemplateData DefaultActivationData =>
            new SystemEmailTemplateData(
                DefaultResourceManager.Value.GetString(SystemEmailTemplateDataHelper.DefaultActivationTitle),
                NoReplyEmailAddress,
                UserEmailLiquidTemplate,
                null,
                null,
                DefaultResourceManager.Value.GetString(SystemEmailTemplateDataHelper.DefaultActivationHtmlBody),
                DefaultResourceManager.Value.GetString(SystemEmailTemplateDataHelper.DefaultActivationPlainTextBody),
                null,
                null);

        /// <summary>
        /// Gets the default template data for an activation email.
        /// </summary>
        public static SystemEmailTemplateData DefaultAccountAlreadyActivatedData =>
            new SystemEmailTemplateData(
                DefaultResourceManager.Value.GetString(SystemEmailTemplateDataHelper.DefaultAccountAlreadyActivatedTitle),
                NoReplyEmailAddress,
                UserEmailLiquidTemplate,
                null,
                null,
                DefaultResourceManager.Value.GetString(SystemEmailTemplateDataHelper.DefaultAccountAlreadyActivatedHtmlBody),
                DefaultResourceManager.Value.GetString(SystemEmailTemplateDataHelper.DefaultAccountAlreadyActivatedTextBody),
                null,
                null);

        /// <summary>
        /// Gets the default template data for an password reset email.
        /// </summary>
        public static SystemEmailTemplateData DefaultPasswordResetData =>
            new SystemEmailTemplateData(
                DefaultResourceManager.Value.GetString(SystemEmailTemplateDataHelper.DefaultPasswordResetTitle),
                NoReplyEmailAddress,
                UserEmailLiquidTemplate,
                null,
                null,
                DefaultResourceManager.Value.GetString(SystemEmailTemplateDataHelper.DefaultPasswordResetHtmlBody),
                DefaultResourceManager.Value.GetString(SystemEmailTemplateDataHelper.DefaultPasswordResetPlainTextBody),
                null,
                null);

        /// <summary>
        /// Gets the default template data for an password expired reset email.
        /// </summary>
        public static SystemEmailTemplateData DefaultPasswordExpiredEmailTemplateData =>
            new SystemEmailTemplateData(
                DefaultResourceManager.Value.GetString(SystemEmailTemplateDataHelper.DefaultPasswordExpiredTitle),
                NoReplyEmailAddress,
                UserEmailLiquidTemplate,
                null,
                null,
                DefaultResourceManager.Value.GetString(SystemEmailTemplateDataHelper.DefaultPasswordExpiredHtmlBody),
                DefaultResourceManager.Value.GetString(SystemEmailTemplateDataHelper.DefaultPasswordExpiredPlainTextBody),
                null,
                null);

        /// <summary>
        /// Gets the default template data for an renewal email.
        /// </summary>
        public static SystemEmailTemplateData DefaultRenewalInvitationData =>
            new SystemEmailTemplateData(
                DefaultResourceManager.Value.GetString(SystemEmailTemplateDataHelper.DefaultRenewalInvitationTitle),
                NoReplyEmailAddress,
                PersonEmailLiquidTemplate,
                null,
                null,
                DefaultResourceManager.Value.GetString(SystemEmailTemplateDataHelper.DefaultRenewalInvitationHtmlBody),
                DefaultResourceManager.Value.GetString(SystemEmailTemplateDataHelper.DefaultRenewalInvitationTextBody),
                null,
                null);

        /// <summary>
        /// Gets the default template data for quote association invitation email.
        /// </summary>
        public static SystemEmailTemplateData DefaultQuoteAssociationInvitationData =>
            new SystemEmailTemplateData(
                DefaultResourceManager.Value.GetString(SystemEmailTemplateDataHelper.DefaultQuoteAssociationInvitationTitle),
                NoReplyEmailAddress,
                UserEmailLiquidTemplate,
                null,
                null,
                DefaultResourceManager.Value.GetString(SystemEmailTemplateDataHelper.DefaultQuoteAssociationInvitationHtmlBody),
                DefaultResourceManager.Value.GetString(SystemEmailTemplateDataHelper.DefaultQuoteAssociationInvitationTextBody),
                null,
                null);

        /// <summary>
        /// Gets a dot liquid template for generating the subject.
        /// </summary>
        public string Subject { get; private set; }

        /// <summary>
        /// Gets a dot liquid template for generating the from address.
        /// </summary>
        public string FromAddress { get; private set; }

        /// <summary>
        /// Gets a dot liquid template for generating for generating the recipient address.
        /// </summary>
        public string ToAddress { get; private set; }

        /// <summary>
        /// Gets the comma-separated list of emails to CC the email to.
        /// </summary>
        public string Cc { get; private set; }

        /// <summary>
        /// Gets the comma-separated list of emails to CC the email to.
        /// </summary>
        public string Bcc { get; private set; }

        /// <summary>
        /// Gets a dot liquid template for generating for generating the email body in html format.
        /// </summary>
        public string HtmlBody { get; private set; }

        /// <summary>
        /// Gets a dot liquid template for generating for generating the email body in plain text format.
        /// </summary>
        public string PlainTextBody { get; private set; }

        /// <summary>
        /// Gets the host of the SMTP server to use to send the email.
        /// </summary>
        public string SmtpServerHost { get; private set; }

        /// <summary>
        /// Gets the port of the SMTP server to use to send the email.
        /// </summary>
        public int? SmtpServerPort { get; private set; }

        /// <summary>
        /// Override template data with more specifif template data.
        /// </summary>
        /// <param name="other">The other template data to override with.</param>
        public void Override(SystemEmailTemplateData other)
        {
            this.Subject = other.Subject ?? this.Subject;
            this.FromAddress = other.FromAddress ?? this.FromAddress;
            this.ToAddress = other.ToAddress ?? this.ToAddress;
            this.Cc = other.Cc ?? this.Cc;
            this.Bcc = other.Bcc ?? this.Bcc;
            this.HtmlBody = other.HtmlBody ?? this.HtmlBody;
            this.PlainTextBody = other.PlainTextBody ?? this.PlainTextBody;
            this.SmtpServerHost = other.SmtpServerHost ?? this.SmtpServerHost;
            this.SmtpServerPort = other.SmtpServerPort ?? this.SmtpServerPort;
        }
    }
}

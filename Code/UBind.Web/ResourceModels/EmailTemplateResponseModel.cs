// <copyright file="EmailTemplateResponseModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Humanizer;
    using Newtonsoft.Json;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Web.Filters;
    using UBind.Web.Validation;

    /// <summary>
    /// A view model for a email template setting.
    /// </summary>
    public class EmailTemplateResponseModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailTemplateResponseModel"/> class.
        /// </summary>
        /// <param name="setting">The template setting.</param>
        public EmailTemplateResponseModel(SystemEmailTemplate setting)
        {
            if (setting != null)
            {
                this.Id = setting.Id;
                this.PortalId = setting.PortalId;
                this.Name = setting.Type.Humanize();
                this.TenantId = setting.TenantId;
                this.ProductId = setting.ProductId;
                this.CreatedDateTime = setting.CreatedTimestamp.ToExtendedIso8601String();
                this.Subject = setting.Data.Subject;
                this.FromAddress = setting.Data.FromAddress;
                this.ToAddress = setting.Data.ToAddress;
                this.Cc = setting.Data.Cc;
                this.Bcc = setting.Data.Bcc;
                this.HtmlBody = setting.Data.HtmlBody;
                this.PlainTextBody = setting.Data.PlainTextBody;
                this.SmtpServerHost = setting.Data.SmtpServerHost;
                this.SmtpServerPort = setting.Data.SmtpServerPort;
                this.Disabled = !setting.Enabled;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailTemplateResponseModel"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for JSON deserializer.
        /// .</remarks>
        [JsonConstructor]
        public EmailTemplateResponseModel()
        {
        }

        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the TenantId.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        public Guid TenantId { get; set; }

        /// <summary>
        /// Gets or sets the Portal ID.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        public Guid? PortalId { get; set; }

        /// <summary>
        /// Gets or sets the ProductId.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        public Guid? ProductId { get; set; }

        /// <summary>
        /// Gets or sets the template name.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        [Required(ErrorMessage = "Template name is required.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets Disable.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        public int Disable { get; set; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        [DotnetLiquidPattern(ErrorMessage = "The field subject must contain a valid Liquid template")]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the sender.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        [DotnetLiquidPattern(ErrorMessage = "The field from address must contain a valid Liquid template")]
        public string FromAddress { get; set; }

        /// <summary>
        /// Gets or sets the recipient.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        [DotnetLiquidPattern(ErrorMessage = "The field to address must contain a valid Liquid template")]
        public string ToAddress { get; set; }

        /// <summary>
        /// Gets or sets the recipient (carbon copy).
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        [EmailListValidation(ErrorMessage = "The field cc must contain one or more valid email addresses separated by commas.")]
        public string Cc { get; set; }

        /// <summary>
        /// Gets or sets the recipient (blind carbon copy).
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        [EmailListValidation(ErrorMessage = "The field bcc must contain one or more valid email addresses separated by commas.")]
        public string Bcc { get; set; }

        /// <summary>
        /// Gets or sets the body in html format.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        [DotnetLiquidPattern(ErrorMessage = "The field html body must contain a valid Liquid template")]
        public string HtmlBody { get; set; }

        /// <summary>
        /// Gets or sets the body in plain text format.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        [DotnetLiquidPattern(ErrorMessage = "The field plain text body must contain a valid Liquid template")]
        public string PlainTextBody { get; set; }

        /// <summary>
        /// Gets or sets the SMTP Server host.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        [SmtpServerHostValidation(ErrorMessage = "The field smtpHost must contain a valid host address.")]
        public string SmtpServerHost { get; set; }

        /// <summary>
        /// Gets or sets the SMTP Server port.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        [PortNumberValidation(ErrorMessage = "The field smtpPort must contain a valid port number.")]
        public int? SmtpServerPort { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether template is disabled.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        public bool Disabled { get; set; }

        /// <summary>
        /// Gets or sets the time the template details was created.
        /// </summary>
        [JsonProperty]
        public string CreatedDateTime { get; set; }

        /// <summary>
        /// Create a new instance of <see cref="SystemEmailTemplateData"/> from the model data.
        /// </summary>
        /// <returns>A new instance of <see cref="SystemEmailTemplateData"/>.</returns>
        public SystemEmailTemplateData GetData()
        {
            return new SystemEmailTemplateData(
                this.Subject.ToNullIfWhitespace(),
                this.FromAddress.ToNullIfWhitespace(),
                this.ToAddress.ToNullIfWhitespace(),
                this.Cc.ToNullIfWhitespace(),
                this.Bcc.ToNullIfWhitespace(),
                this.HtmlBody.ToNullIfWhitespace(),
                this.PlainTextBody.ToNullIfWhitespace(),
                this.SmtpServerHost.ToNullIfWhitespace(),
                this.SmtpServerPort);
        }
    }
}

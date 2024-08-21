// <copyright file="SmsDetailSetModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel.Email;
    using UBind.Domain.ReadModel.Sms;

    /// <summary>
    /// Resource model for serving sms details.
    /// </summary>
    public class SmsDetailSetModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmsDetailSetModel"/> class.
        /// </summary>
        /// <param name="model">The dto model.</param>
        public SmsDetailSetModel(ISmsDetails model)
        {
            this.TenantId = model.TenantId;
            this.OrganisationId = model.OrganisationId;
            this.ProductId = model.ProductId;
            this.Id = model.Id;
            this.To = model.To;
            this.From = model.From;
            this.Message = model.Message;
            this.Customer = model.Customer;
            this.User = model.User;
            this.Quote = model.Quote;
            this.Claim = model.Claim;
            this.Policy = model.Policy;
            this.Organisation = model.Organisation;
            this.PolicyTransaction = model.PolicyTransaction;
            this.CreatedDateTime = model.CreatedTimestamp.ToExtendedIso8601String();
        }

        public Guid TenantId { get; set; }

        public Guid OrganisationId { get; set; }

        public Guid ProductId { get; set; }

        public Guid Id { get; set; }

        public string To { get; set; }

        public string From { get; set; }

        public string Message { get; set; }

        public CustomerData Customer { get; set; }

        public PolicyData Policy { get; set; }

        public QuoteData Quote { get; set; }

        public ClaimData Claim { get; set; }

        public PolicyTransactionData PolicyTransaction { get; set; }

        public UserData User { get; set; }

        public OrganisationData Organisation { get; set; }

        public string CreatedDateTime { get; set; }
    }
}

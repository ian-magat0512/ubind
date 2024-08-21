// <copyright file="CustomerDetailsModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Dto;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.ValueTypes;
    using UBind.Web.Accounting.ResourceModels;
    using UBind.Web.ResourceModels.Claim;

    /// <summary>
    /// For representing customer details.
    /// </summary>
    public class CustomerDetailsModel : PersonDetailsModel, IAdditionalPropertyValues
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerDetailsModel"/> class.
        /// </summary>
        /// <param name="customer">The customer to represent.</param>
        /// <param name="paramModel">The collection of lists associated with this customer.</param>
        /// <param name="people">The list of people related to customer.</param>
        /// <param name="portal">The portal this customer is associated with, if any.</param>
        /// <param name="time">The time to used for calculating statuses etc.</param>
        /// <param name="additionalPropertyValueModels">Additional property models.</param>
        public CustomerDetailsModel(
            CustomerReadModelDetail customer,
            CustomerRecordsModel paramModel,
            IEnumerable<IPersonReadModelSummary> people,
            PortalReadModel portal,
            Instant time,
            List<AdditionalPropertyValueDto> additionalPropertyValueModels)
        {
            this.Status = paramModel.Policies.Any(p => Domain.PolicyStatus.IssuedOrActive.HasFlag(p.GetPolicyStatus(time)))
                ? CustomerStatus.Active.ToString()
                : CustomerStatus.Inactive.ToString();
            this.Policies = paramModel.Policies.Select(policy => new PolicySetModel(policy, time)).ToList();
            this.Id = customer.Id;
            this.OrganisationName = customer.OrganisationName;
            this.OrganisationId = customer.OrganisationId;
            this.TenantId = customer.TenantId;
            this.PrimaryPersonId = customer.PrimaryPersonId;
            this.PortalId = customer.PortalId;
            this.PortalName = portal?.Name;

            var personCommonProperties = new PersonCommonProperties();
            personCommonProperties.FullName = customer.FullName;
            personCommonProperties.PreferredName = customer.PreferredName;
            personCommonProperties.NamePrefix = customer.NamePrefix;
            personCommonProperties.FirstName = customer.FirstName;
            personCommonProperties.MiddleNames = customer.MiddleNames;
            personCommonProperties.LastName = customer.LastName;
            personCommonProperties.NameSuffix = customer.NameSuffix;
            personCommonProperties.SetNameComponentsFromFullNameIfNoneAlreadySet();

            this.FullName = personCommonProperties.FullName;
            this.PreferredName = personCommonProperties.PreferredName;
            this.NamePrefix = personCommonProperties.NamePrefix;
            this.FirstName = personCommonProperties.FirstName;
            this.MiddleNames = personCommonProperties.MiddleNames;
            this.LastName = personCommonProperties.LastName;
            this.NameSuffix = personCommonProperties.NameSuffix;
            this.Email = customer.Email;
            this.AlternativeEmail = customer.AlternativeEmail;
            this.MobilePhoneNumber = customer.MobilePhoneNumber;
            this.HomePhoneNumber = customer.HomePhoneNumber;
            this.WorkPhoneNumber = customer.WorkPhoneNumber;
            this.CreatedDateTime = customer.CreatedTimestamp.ToExtendedIso8601String();
            this.UserStatus = this.GetStatus(customer);
            this.Quotes = paramModel.Quotes.Select(quote => new QuoteSetModel(quote));
            this.Claims = paramModel.Claims
                .Select(claim => new ClaimSetModel(claim))
                .OrderByDescending(dt => dt.CreatedDateTime);
            this.People = people?.Select(person => new PersonDetailsModel(person));

            if (customer.OwnerUserId != default)
            {
                this.OwnerId = customer.OwnerUserId;
            }

            this.OwnerFullName = customer.OwnerFullName;
            this.Company = customer.Company;
            this.Title = customer.Title;
            this.SetRepeatingFieldsValues(paramModel.Person);
            this.CheckCreateRepeatingFieldsForBackwardCompatibility();

            this.CheckAndFixRepeatingFieldsDefaults();
            List<AccountingTransactionHistoryItemModel> transactions = new List<AccountingTransactionHistoryItemModel>();

            foreach (var payment in paramModel.Payments)
            {
                transactions.Add(new AccountingTransactionHistoryItemModel(payment.Id, payment.TransactionTimestamp, AccountingDocumentType.Payment, payment.Amount, payment.ReferenceNumber.PadLeft(6, '0'), true));
            }

            foreach (var payment in paramModel.Refunds)
            {
                transactions.Add(new AccountingTransactionHistoryItemModel(payment.Id, payment.TransactionTimestamp, AccountingDocumentType.Refund, payment.Amount, payment.ReferenceNumber.PadLeft(6, '0')));
            }

            this.Transactions = transactions.OrderByDescending(x => x.TransactionDateTime);

            var accountBalance = this.GetAccountBalance(transactions);
            var subtext = this.GetAccountBalanceSubtext(accountBalance);

            this.AccountBalance = Math.Abs(accountBalance).ToDollarsAndCents();
            this.AccountBalanceSubtext = subtext;
            this.LastModifiedDateTime = customer.LastModifiedTimestamp.ToExtendedIso8601String();
            this.AdditionalPropertyValues = additionalPropertyValueModels != null
                && additionalPropertyValueModels.Any() ?
                additionalPropertyValueModels.Select(apvm => new AdditionalPropertyValueModel(apvm)).ToList() :
                new List<AdditionalPropertyValueModel>();
        }

        [JsonConstructor]
        private CustomerDetailsModel()
        {
        }

        /// <summary>
        /// Gets the organisation name.
        /// </summary>
        [JsonProperty]
        public string OrganisationName { get; private set; }

        /// <summary>
        /// Gets the portal ID.
        /// </summary>
        [JsonProperty]
        public Guid? PortalId { get; private set; }

        /// <summary>
        /// Gets the portal name.
        /// </summary>
        [JsonProperty]
        public string PortalName { get; private set; }

        /// <summary>
        /// Gets the ID of the person.
        /// </summary>
        [JsonProperty]
        public Guid PrimaryPersonId { get; private set; }

        /// <summary>
        /// Gets the customer's account status.
        /// </summary>
        [JsonProperty]
        public string UserStatus { get; private set; }

        /// <summary>
        /// Gets the quotes belonging to the customer.
        /// </summary>
        [JsonProperty]
        public IEnumerable<QuoteSetModel> Quotes { get; private set; }

        /// <summary>
        /// Gets the policies of the customer.
        /// </summary>
        [JsonProperty]
        public IEnumerable<PolicySetModel> Policies { get; private set; }

        /// <summary>
        /// Gets the transaction history of the customer.
        /// </summary>
        [JsonProperty]
        public IEnumerable<AccountingTransactionHistoryItemModel> Transactions { get; private set; }

        /// <summary>
        /// Gets the claims of the customer.
        /// </summary>
        [JsonProperty]
        public IEnumerable<ClaimSetModel> Claims { get; private set; }

        /// <summary>
        /// Gets the people related to the customer.
        /// </summary>
        [JsonProperty]
        public IEnumerable<PersonDetailsModel> People { get; private set; }

        /// <summary>
        /// Gets the total account balance of the customer.
        /// </summary>
        [JsonProperty]
        public string AccountBalance { get; private set; }

        /// <summary>
        /// Gets the total account balance displayed subtext.
        /// </summary>
        [JsonProperty]
        public string AccountBalanceSubtext { get; private set; }

        [JsonProperty]
        public List<AdditionalPropertyValueModel> AdditionalPropertyValues { get; private set; }

        /// <summary>
        /// Returns the account status of the customer.
        /// </summary>
        /// <param name="customer">The read model.</param>
        /// <returns>The current status of the customer's account.</returns>
        private string GetStatus(ICustomerReadModelSummary customer)
        {
            if (!customer.UserIsBlocked && !customer.UserHasBeenInvitedToActivate)
            {
                return Domain.UserStatus.New.ToString();
            }

            if (!customer.UserIsBlocked && customer.UserHasBeenInvitedToActivate && !customer.UserHasBeenActivated)
            {
                return Domain.UserStatus.Invited.ToString();
            }

            if (!customer.UserIsBlocked && customer.UserHasBeenActivated)
            {
                return Domain.UserStatus.Active.ToString();
            }

            return Domain.UserStatus.Deactivated.ToString();
        }

        private decimal GetAccountBalance(List<AccountingTransactionHistoryItemModel> transactions)
        {
            HashSet<int> paymentsAndCreditNotes = new HashSet<int>
            {
                (int)AccountingDocumentType.Payment,
                (int)AccountingDocumentType.CreditNote,
            };

            var totalPaymentsAndCreditNotes = transactions.Where(t => paymentsAndCreditNotes.Contains(t.TransactionHistoryType)).Sum(x => x.Amount.Amount);

            HashSet<int> invoicesAndRefunds = new HashSet<int>
            {
                (int)AccountingDocumentType.Invoice,
                (int)AccountingDocumentType.Refund,
            };

            var totalInvoicesAndRefunds = transactions.Where(t => invoicesAndRefunds.Contains(t.TransactionHistoryType)).Sum(x => x.Amount.Amount);

            decimal balance = totalInvoicesAndRefunds - totalPaymentsAndCreditNotes;

            return balance;
        }

        private string GetAccountBalanceSubtext(decimal accountBalance)
        {
            if (accountBalance >= 0)
            {
                return "Balance Due";
            }
            else
            {
                return "Account Credit";
            }
        }
    }
}

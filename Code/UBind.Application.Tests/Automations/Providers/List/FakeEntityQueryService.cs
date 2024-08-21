// <copyright file="FakeEntityQueryService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.List
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Application.Automation.Providers.List;
    using UBind.Domain;
    using UBind.Domain.Automation;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Sms;

    public class FakeEntityQueryService : IEntityQueryService
    {
        public IEnumerable<string> ClaimsIncludedProperties { get; private set; }

        public List<IClaimReadModelWithRelatedEntities> Claims { get; set; }
            = new List<IClaimReadModelWithRelatedEntities>();

        public IEnumerable<string> ClaimVersionsIncludedProperties { get; private set; }

        public List<IClaimVersionReadModelWithRelatedEntities> ClaimVersions { get; set; }
            = new List<IClaimVersionReadModelWithRelatedEntities>();

        public IEnumerable<string> CustomersIncludedProperties { get; private set; }

        public List<ICustomerReadModelWithRelatedEntities> Customers { get; set; }
            = new List<ICustomerReadModelWithRelatedEntities>();

        public IEnumerable<string> DocumentsIncludedProperties { get; private set; }

        public List<IDocumentReadModelWithRelatedEntities> Documents { get; set; }
            = new List<IDocumentReadModelWithRelatedEntities>();

        public IEnumerable<string> EmailsIncludedProperties { get; private set; }

        public List<IEmailReadModelWithRelatedEntities> Emails { get; set; }
            = new List<IEmailReadModelWithRelatedEntities>();

        public List<ISmsReadModelWithRelatedEntities> Sms { get; set; }
            = new List<ISmsReadModelWithRelatedEntities>();

        public IEnumerable<string> OrganisationsIncludedProperties { get; private set; }

        public List<IOrganisationReadModelWithRelatedEntities> Organisations { get; set; }
            = new List<IOrganisationReadModelWithRelatedEntities>();

        public IEnumerable<string> PoliciesIncludedProperties { get; private set; }

        public List<IPolicyReadModelWithRelatedEntities> Policies { get; set; }
            = new List<IPolicyReadModelWithRelatedEntities>();

        public IEnumerable<string> PolicyTransactionsIncludedProperties { get; private set; }

        public List<IPolicyTransactionReadModelWithRelatedEntities> PolicyTransactions { get; set; }
            = new List<IPolicyTransactionReadModelWithRelatedEntities>();

        public IEnumerable<string> PortalsIncludedProperties { get; private set; }

        public List<IPortalWithRelatedEntities> Portals { get; set; }
            = new List<IPortalWithRelatedEntities>();

        public IEnumerable<string> ProductsIncludedProperties { get; private set; }

        public List<IProductWithRelatedEntities> Products { get; set; }
            = new List<IProductWithRelatedEntities>();

        public IEnumerable<string> QuotesIncludedProperties { get; private set; }

        public List<IQuoteReadModelWithRelatedEntities> Quotes { get; set; }
            = new List<IQuoteReadModelWithRelatedEntities>();

        public IEnumerable<string> QuoteVersionsIncludedProperties { get; private set; }

        public List<IQuoteVersionReadModelWithRelatedEntities> QuoteVersions { get; set; }
            = new List<IQuoteVersionReadModelWithRelatedEntities>();

        public IEnumerable<string> TenantsIncludedProperties { get; private set; }

        public List<ITenantWithRelatedEntities> Tenants { get; set; }
            = new List<ITenantWithRelatedEntities>();

        public IEnumerable<string> UsersIncludedProperties { get; private set; }

        public List<IUserReadModelWithRelatedEntities> Users { get; set; }
            = new List<IUserReadModelWithRelatedEntities>();

        public IEnumerable<string> SmsIncludedProperties { get; private set; }

        public IQueryable<IClaimReadModelWithRelatedEntities> QueryClaims(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> includedProperties)
        {
            this.ClaimsIncludedProperties = includedProperties;
            return this.Claims.AsQueryable();
        }

        public IQueryable<IClaimVersionReadModelWithRelatedEntities> QueryClaimVersions(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> includedProperties)
        {
            this.ClaimVersionsIncludedProperties = includedProperties;
            return this.ClaimVersions.AsQueryable();
        }

        public IQueryable<ICustomerReadModelWithRelatedEntities> QueryCustomers(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> includedProperties)
        {
            this.CustomersIncludedProperties = includedProperties;
            return this.Customers.AsQueryable();
        }

        public IQueryable<IDocumentReadModelWithRelatedEntities> QueryDocuments(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> includedProperties)
        {
            this.DocumentsIncludedProperties = includedProperties;
            return this.Documents.AsQueryable();
        }

        public IQueryable<IEmailReadModelWithRelatedEntities> QueryEmails(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> includedProperties)
        {
            this.EmailsIncludedProperties = includedProperties;
            return this.Emails.AsQueryable();
        }

        public IQueryable<ISmsReadModelWithRelatedEntities> QuerySms(
            Guid tenantId, IEnumerable<string> includedProperties)
        {
            this.SmsIncludedProperties = includedProperties;
            return this.Sms.AsQueryable();
        }

        public IQueryable<Event> QueryEvents(Guid tenantId)
        {
            throw new NotImplementedException();
        }

        public IQueryable<IOrganisationReadModelWithRelatedEntities> QueryOrganisations(
            Guid tenantId, IEnumerable<string> includedProperties)
        {
            this.OrganisationsIncludedProperties = includedProperties;
            return this.Organisations.AsQueryable();
        }

        public IQueryable<IPolicyReadModelWithRelatedEntities> QueryPolicies(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> includedProperties)
        {
            this.PoliciesIncludedProperties = includedProperties;
            return this.Policies.AsQueryable();
        }

        public IQueryable<IPolicyTransactionReadModelWithRelatedEntities> QueryPolicyTransactions(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> includedProperties)
        {
            this.PolicyTransactionsIncludedProperties = includedProperties;
            return this.PolicyTransactions.AsQueryable();
        }

        public IQueryable<IPortalWithRelatedEntities> QueryPortals(
            Guid tenantId, IEnumerable<string> includedProperties)
        {
            this.PortalsIncludedProperties = includedProperties;
            return this.Portals.AsQueryable();
        }

        public IQueryable<IProductWithRelatedEntities> QueryProducts(
            Guid tenantId, IEnumerable<string> includedProperties)
        {
            this.ProductsIncludedProperties = includedProperties;
            return this.Products.AsQueryable();
        }

        public IQueryable<IQuoteReadModelWithRelatedEntities> QueryQuotes(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> includedProperties)
        {
            this.QuotesIncludedProperties = includedProperties;
            return this.Quotes.AsQueryable();
        }

        public IQueryable<IQuoteVersionReadModelWithRelatedEntities> QueryQuoteVersions(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> includedProperties)
        {
            this.QuoteVersionsIncludedProperties = includedProperties;
            return this.QuoteVersions.AsQueryable();
        }

        public IQueryable<ITenantWithRelatedEntities> QueryTenants(
            Guid tenantId, IEnumerable<string> includedProperties)
        {
            this.TenantsIncludedProperties = includedProperties;
            return this.Tenants.AsQueryable();
        }

        public IQueryable<IUserReadModelWithRelatedEntities> QueryUsers(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> includedProperties)
        {
            this.UsersIncludedProperties = includedProperties;
            return this.Users.AsQueryable();
        }
    }
}

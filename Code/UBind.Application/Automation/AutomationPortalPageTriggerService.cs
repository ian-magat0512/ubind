// <copyright file="AutomationPortalPageTriggerService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Domain;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.ReadModel.Product;
    using UBind.Domain.ReadModel.Sms;
    using UBind.Domain.Repositories;
    using UBind.Domain.SerialisedEntitySchemaObject;
    using Claim = UBind.Domain.SerialisedEntitySchemaObject.Claim;
    using Policy = UBind.Domain.SerialisedEntitySchemaObject.Policy;
    using Portal = UBind.Domain.SerialisedEntitySchemaObject.Portal;
    using Product = UBind.Domain.SerialisedEntitySchemaObject.Product;

    public sealed class AutomationPortalPageTriggerService : IAutomationPortalPageTriggerService
    {
        private readonly IServiceProvider serviceProvider;

        public AutomationPortalPageTriggerService(
            IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetEntityList(IEntityListReference entry, IEnumerable<string> relatedEntities)
        {
            relatedEntities = relatedEntities ?? Enumerable.Empty<string>();
            var listMap = new Dictionary<Type, Func<IEntityListReference, Task<IEnumerable<Dictionary<string, object>>>>>()
            {
                { typeof(Customer), async (p) => await this.GetCustomerList(p, relatedEntities) },
                { typeof(Quote), async (p) => await this.GetQuoteList(p, relatedEntities) },
                { typeof(Policy), async (p) => await this.GetPolicyList(p, relatedEntities) },
                { typeof(Claim), async (p) => await this.GetClaimList(p, relatedEntities) },
                { typeof(Message), async (p) => await this.GetMessageList(p, relatedEntities) },
                { typeof(Organisation), async (p) => await this.GetOrganisationList(p, relatedEntities) },
                { typeof(User), async (p) => await this.GetUserList(p, relatedEntities) },
                { typeof(Role), async (p) => await this.GetRoleList(p, relatedEntities) },
                { typeof(Portal), async (p) => await this.GetPortalList(p, relatedEntities) },
                { typeof(Product), async (p) => await this.GetProductList(p, relatedEntities) },
            };

            Type entityType = entry.GetGenericType();
            if (listMap.TryGetValue(entityType, out Func<IEntityListReference, Task<IEnumerable<Dictionary<string, object>>>> factory))
            {
                return await factory.Invoke(entry);
            }

            return null;
        }

        public Dictionary<string, object> GetEntityDisplay(PortalPageData portalPageData)
        {
            var entityId = portalPageData.EntityId;
            var tenantId = portalPageData.TenantId;
            var entityType = portalPageData.EntityType;
            var userId = portalPageData.UserId;
            var listMap =
            new Dictionary<EntityType, Func<Guid, Guid, Dictionary<string, object>>>()
            {
                { EntityType.Customer, (t, i) => this.GetCustomerDisplay(portalPageData) },
                { EntityType.Quote, (t, i) => this.GetQuoteDisplay(portalPageData) },
                { EntityType.Claim, (t, i) => this.GetClaimDisplay(portalPageData) },
                { EntityType.Policy, (t, i) => this.GetPolicyDisplay(portalPageData) },
                { EntityType.EmailMessage, (t, i) => this.GetEmailMessageDisplay(portalPageData) },
                { EntityType.SmsMessage, (t, i) => this.GetSmsMessageDisplay(portalPageData) },
                { EntityType.Organisation, (t, i) => this.GetOrganisationDisplay(portalPageData) },
                { EntityType.User, (t, i) => this.GetUserDisplay(portalPageData) },
                { EntityType.Role, (t, i) => this.GetRoleDisplay(portalPageData) },
                { EntityType.Portal, (t, i) => this.GetPortalDisplay(portalPageData) },
                { EntityType.Product, (t, i) => this.GetProductDisplay(portalPageData) },
                { EntityType.QuoteVersion, (t, i) => this.GetQuoteVersionDisplay(portalPageData) },
                { EntityType.PolicyTransaction, (t, i) => this.GetPolicyTransactionDisplay(portalPageData) },
                { EntityType.ClaimVersion, (t, i) => this.GetClaimVersionDisplay(portalPageData) },
                { EntityType.Person, (t, i) => this.GetPersonDisplay(portalPageData) },
            };

            var entities = new Dictionary<string, object>();
            if (listMap.TryGetValue(
                entityType, out Func<Guid, Guid, Dictionary<string, object>> factory))
            {
                entities = factory.Invoke(tenantId, entityId);
            }

            // Add performing user
            var performingUser = new User(userId.GetValueOrDefault());
            entities["performingUser"] = performingUser;
            return entities;
        }

        private static void AddContextEntity(
            Dictionary<string, object> context, string entityName, object entity, Guid? id)
        {
            if (id.GetValueOrDefault() != default)
            {
                context[entityName] = entity;
            }
        }

        private Dictionary<string, object> GetCustomerDisplay(PortalPageData portalPageData)
        {
            var customerId = portalPageData.EntityId;
            var repo = this.serviceProvider.GetService<ICustomerReadModelRepository>();
            var entity = repo.GetCustomerById(portalPageData.TenantId, customerId);

            var context = new Dictionary<string, object>();
            context["customer"] = new Customer(customerId);
            AddContextEntity(context, "owner", new User(entity.OwnerUserId.GetValueOrDefault()), entity.OwnerUserId);

            return context;
        }

        private Dictionary<string, object> GetPersonDisplay(PortalPageData portalPageData)
        {
            var personId = portalPageData.EntityId;
            var repo = this.serviceProvider.GetService<IPersonReadModelRepository>();
            var entity = repo.GetPersonById(portalPageData.TenantId, personId);

            var custRepo = this.serviceProvider.GetService<ICustomerReadModelRepository>();
            var customer = custRepo.GetCustomerById(portalPageData.TenantId, entity.CustomerId.GetValueOrDefault());
            var context = new Dictionary<string, object>
            {
                ["person"] = new Person(personId),
                ["customer"] = new Customer(entity.CustomerId.GetValueOrDefault()),
            };
            AddContextEntity(context, "owner", new User(customer.OwnerUserId.GetValueOrDefault()), customer.OwnerUserId);

            return context;
        }

        private Dictionary<string, object> GetQuoteDisplay(PortalPageData portalPageData)
        {
            var quoteId = portalPageData.EntityId;
            var quoteRepo = this.serviceProvider.GetRequiredService<IQuoteReadModelRepository>();
            var entity = quoteRepo.GetQuoteSummary(portalPageData.TenantId, quoteId);

            var context = new Dictionary<string, object>
            {
                ["quote"] = new Quote(quoteId),
                ["customer"] = new Customer(entity.CustomerId.GetValueOrDefault()),
                ["product"] = new Product(entity.ProductId),
            };
            AddContextEntity(context, "owner", new User(entity.OwnerUserId.GetValueOrDefault()), entity.OwnerUserId);

            return context;
        }

        private Dictionary<string, object> GetQuoteVersionDisplay(PortalPageData portalPageData)
        {
            var quoteVersionId = portalPageData.EntityId;
            var repo = this.serviceProvider.GetService<IQuoteVersionReadModelRepository>();
            var entity = repo.GetQuoteVersionWithRelatedEntities(
                portalPageData.TenantId, portalPageData.Environment, quoteVersionId, new string[] { "product" });
            var ownerUserId = entity.QuoteVersion.OwnerUserId;
            var context = new Dictionary<string, object>
            {
                ["quoteVersion"] = new QuoteVersion(quoteVersionId),
                ["quote"] = new Quote(entity.QuoteVersion.QuoteId),
                ["customer"] = new Customer(entity.QuoteVersion.CustomerId.GetValueOrDefault()),
                ["product"] = new Product(entity.Product.Id),
            };
            AddContextEntity(context, "owner", new User(ownerUserId.GetValueOrDefault()), ownerUserId);

            return context;
        }

        private Dictionary<string, object> GetPolicyDisplay(PortalPageData portalPageData)
        {
            var policyId = portalPageData.EntityId;
            var repo = this.serviceProvider.GetService<IPolicyReadModelRepository>();
            var entity = repo.GetById(portalPageData.TenantId, policyId);

            var context = new Dictionary<string, object>
            {
                ["policy"] = new Policy(policyId),
                ["product"] = new Product(entity.ProductId),
            };

            AddContextEntity(context, "customer", new Customer(entity.CustomerId.GetValueOrDefault()), entity.CustomerId);
            AddContextEntity(context, "owner", new User(entity.OwnerUserId.GetValueOrDefault()), entity.OwnerUserId);

            return context;
        }

        private Dictionary<string, object> GetPolicyTransactionDisplay(PortalPageData portalPageData)
        {
            var policyTransactionId = portalPageData.EntityId;
            var repo = this.serviceProvider.GetService<IPolicyTransactionReadModelRepository>();
            var entity = repo.GetPolicyTransactionWithRelatedEntities(
                portalPageData.TenantId, portalPageData.Environment, policyTransactionId, new string[] { "Policy" });

            var context = new Dictionary<string, object>
            {
                ["policyTransaction"] = new PolicyTransaction(policyTransactionId),
                ["policy"] = new Policy(entity.Policy.Id),
                ["product"] = new Product(entity.Policy.ProductId),
            };

            AddContextEntity(
                context, "owner", new User(entity.Policy.OwnerUserId.GetValueOrDefault()), entity.Policy.OwnerUserId);
            AddContextEntity(
                context, "customer", new Customer(entity.Policy.CustomerId.GetValueOrDefault()), entity.Policy.CustomerId);
            return context;
        }

        private Dictionary<string, object> GetClaimDisplay(PortalPageData portalPageData)
        {
            var claimId = portalPageData.EntityId;
            var claimRepo = this.serviceProvider.GetService<IClaimReadModelRepository>();
            var entity = claimRepo.GetClaimDetails(portalPageData.TenantId, claimId);

            var context = new Dictionary<string, object>
            {
                ["claim"] = new Claim(claimId),
                ["product"] = new Product(entity.ProductId),
            };

            AddContextEntity(context, "owner", new User(entity.OwnerUserId.GetValueOrDefault()), entity.OwnerUserId);
            AddContextEntity(context, "customer", new Customer(entity.CustomerId.GetValueOrDefault()), entity.CustomerId);
            AddContextEntity(context, "policy", new Policy(entity.PolicyId.GetValueOrDefault()), entity.PolicyId);

            return context;
        }

        private Dictionary<string, object> GetClaimVersionDisplay(PortalPageData portalPageData)
        {
            var claimVersionId = portalPageData.EntityId;
            var claimRepo = this.serviceProvider.GetService<IClaimVersionReadModelRepository>();
            var entity = claimRepo.GetClaimVersionWithRelatedEntities(
                portalPageData.TenantId, portalPageData.Environment, claimVersionId, new string[] { "Claim" });

            var context = new Dictionary<string, object>
            {
                ["claimVersion"] = new ClaimVersion(claimVersionId),
                ["claim"] = new Claim(entity.Claim.Id),
                ["product"] = new Product(entity.Claim.ProductId),
            };

            AddContextEntity(
                context, "owner", new User(entity.Claim.OwnerUserId.GetValueOrDefault()), entity.Claim.OwnerUserId);
            AddContextEntity(
                context, "customer", new Customer(entity.Claim.CustomerId.GetValueOrDefault()), entity.Claim.CustomerId);
            AddContextEntity(
                context, "policy", new Policy(entity.Claim.PolicyId.GetValueOrDefault()), entity.Claim.PolicyId);

            return context;
        }

        private Dictionary<string, object> GetEmailMessageDisplay(PortalPageData portalPageData)
        {
            var message = new EmailMessage(portalPageData.EntityId);
            return new Dictionary<string, object> { ["message"] = message };
        }

        private Dictionary<string, object> GetSmsMessageDisplay(PortalPageData portalPageData)
        {
            var message = new SmsMessage(portalPageData.EntityId);
            return new Dictionary<string, object> { ["message"] = message };
        }

        private Dictionary<string, object> GetOrganisationDisplay(PortalPageData portalPageData)
        {
            return new Dictionary<string, object> { ["organisation"] = new Organisation(portalPageData.EntityId) };
        }

        private Dictionary<string, object> GetUserDisplay(PortalPageData portalPageData)
        {
            return new Dictionary<string, object> { ["user"] = new User(portalPageData.EntityId) };
        }

        private Dictionary<string, object> GetRoleDisplay(PortalPageData portalPageData)
        {
            return new Dictionary<string, object> { ["role"] = new Role(portalPageData.EntityId) };
        }

        private Dictionary<string, object> GetProductDisplay(PortalPageData portalPageData)
        {
            var product = new Product(portalPageData.EntityId);
            return new Dictionary<string, object> { ["product"] = product };
        }

        private Dictionary<string, object> GetPortalDisplay(PortalPageData portalPageData)
        {
            return new Dictionary<string, object>
            {
                ["portal"] = new Portal(portalPageData.EntityId),
            };
        }

        private async Task<IEnumerable<Dictionary<string, object>>> GetCustomerList(
            IEntityListReference entityListReference, IEnumerable<string> relatedEntities)
        {
            var repo = this.serviceProvider.GetService<ICustomerReadModelRepository>();
            var customers = repo.GetCustomersWithRelatedEntities(
                entityListReference.TenantId, entityListReference.Filters, relatedEntities);
            return await this.ResolveEntityList<CustomerReadModelWithRelatedEntities>(customers, relatedEntities);
        }

        private async Task<IEnumerable<Dictionary<string, object>>> GetQuoteList(
            IEntityListReference entityListReference, IEnumerable<string> relatedEntities)
        {
            var repo = this.serviceProvider.GetService<IQuoteReadModelRepository>();

            var quotes = repo.GetQuotesWithRelatedEntities(
                entityListReference.TenantId, (QuoteReadModelFilters)entityListReference.Filters, relatedEntities);
            return await this.ResolveEntityList<QuoteReadModelWithRelatedEntities>(quotes, relatedEntities);
        }

        private async Task<IEnumerable<Dictionary<string, object>>> GetPolicyList(
            IEntityListReference entityListReference, IEnumerable<string> relatedEntities)
        {
            var repo = this.serviceProvider.GetService<IPolicyReadModelRepository>();

            var policies = repo.GetPoliciesWithRelatedEntities(
                entityListReference.TenantId, (PolicyReadModelFilters)entityListReference.Filters, relatedEntities);
            return await this.ResolveEntityList<PolicyReadModelWithRelatedEntities>(policies, relatedEntities);
        }

        private async Task<IEnumerable<Dictionary<string, object>>> GetClaimList(
            IEntityListReference entityListReference, IEnumerable<string> relatedEntities)
        {
            var repo = this.serviceProvider.GetService<IClaimReadModelRepository>();
            var claims = repo.GetClaimsWithRelatedEntities(
                entityListReference.TenantId, entityListReference.Environment, entityListReference.Filters, relatedEntities);
            return await this.ResolveEntityList<ClaimReadModelWithRelatedEntities>(claims, relatedEntities);
        }

        private async Task<IEnumerable<Dictionary<string, object>>> GetMessageList(
            IEntityListReference entityListReference, IEnumerable<string> relatedEntities)
        {
            var result = new List<Dictionary<string, object>>();
            var emailRepo = this.serviceProvider.GetService<IEmailRepository>();
            var emails = emailRepo.GetEmailsWithRelatedEntities(
                entityListReference.TenantId, entityListReference.Environment, entityListReference.Filters, relatedEntities);
            result.AddRange(await this.ResolveEntityList<EmailReadModelWithRelatedEntities>(emails, relatedEntities));

            var smsRepo = this.serviceProvider.GetService<ISmsRepository>();
            var sms = smsRepo.GetSmsListWithRelatedEntities(
                entityListReference.TenantId, entityListReference.Filters, relatedEntities);
            result.AddRange(await this.ResolveEntityList<SmsReadModelWithRelatedEntities>(sms, relatedEntities));

            return result;
        }

        private async Task<IEnumerable<Dictionary<string, object>>> GetOrganisationList(
            IEntityListReference entityListReference, IEnumerable<string> relatedEntities)
        {
            var orgRepo = this.serviceProvider.GetService<IOrganisationReadModelRepository>();
            var orgs1 = orgRepo.Get(entityListReference.TenantId, (OrganisationReadModelFilters)entityListReference.Filters);
            var orgs = orgRepo.GetOrganisationsWithRelatedEntities(
                entityListReference.TenantId, (OrganisationReadModelFilters)entityListReference.Filters, relatedEntities);
            return await this.ResolveEntityList<OrganisationReadModelWithRelatedEntities>(orgs, relatedEntities);
        }

        private async Task<IEnumerable<Dictionary<string, object>>> GetUserList(
            IEntityListReference entityListReference, IEnumerable<string> relatedEntities)
        {
            var userRepo = this.serviceProvider.GetService<IUserReadModelRepository>();
            var userFilters = (UserReadModelFilters)entityListReference.Filters;
            userFilters.Environment = null;
            var users = userRepo.GetUsersWithRelatedEntities(
                entityListReference.TenantId, userFilters, relatedEntities);
            return await this.ResolveEntityList<UserReadModelWithRelatedEntities>(users, relatedEntities);
        }

        private Task<IEnumerable<Dictionary<string, object>>> GetRoleList(
            IEntityListReference entityListReference, IEnumerable<string> relatedEntities)
        {
            var roleRepo = this.serviceProvider.GetService<IRoleRepository>();
            var roles = roleRepo.GetRoles(entityListReference.TenantId, (RoleReadModelFilters)entityListReference.Filters);
            var data = from r in roles
                       let entity = new Role(r)
                       select entity.ToReadOnlyDictionary();
            return Task.FromResult(data.AsEnumerable());
        }

        private async Task<IEnumerable<Dictionary<string, object>>> GetPortalList(
            IEntityListReference entityListReference, IEnumerable<string> relatedEntities)
        {
            var portalRepo = this.serviceProvider.GetService<IPortalReadModelRepository>();
            var portals = portalRepo.GetPortalsWithRelatedEntities(
                entityListReference.TenantId, entityListReference.Filters, relatedEntities);
            return await this.ResolveEntityList<PortalWithRelatedEntities>(portals, relatedEntities);
        }

        private async Task<IEnumerable<Dictionary<string, object>>> GetProductList(
            IEntityListReference entityListReference, IEnumerable<string> relatedEntities)
        {
            var productRepo = this.serviceProvider.GetService<IProductRepository>();
            var products = productRepo.GetProductsWithRelatedEntities(
                entityListReference.TenantId, (ProductReadModelFilters)entityListReference.Filters, relatedEntities);
            return await this.ResolveEntityList<ProductWithRelatedEntities>(products, relatedEntities);
        }

        private async Task<IEnumerable<Dictionary<string, object>>> ResolveEntityList<T>(
            IEnumerable<IEntityReadModelWithRelatedEntities> data, IEnumerable<string> includedProperties)
        {
            var serialisedEntityFactory = this.serviceProvider.GetService<ISerialisedEntityFactory>();
            var list = new List<Dictionary<string, object>>();
            foreach (var row in data)
            {
                var entity = await serialisedEntityFactory.Create(row, includedProperties);
                list.Add(entity.ToReadOnlyDictionary());
            }

            return list;
        }
    }
}

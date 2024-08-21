// <copyright file="AutomationDataContextManager.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Context
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Humanizer;
    using Microsoft.Extensions.DependencyInjection;
    using StackExchange.Profiling;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Helper;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Application.ExtensionMethods;
    using UBind.Domain;
    using UBind.Domain.Configuration;
    using UBind.Domain.Events;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.SerialisedEntitySchemaObject;
    using ProviderEntity = UBind.Domain.SerialisedEntitySchemaObject;

    /// <summary>
    /// Specifies the automation data context.
    /// Usually found here are relevant entities and other infromation of triggering events
    /// or application information like current environment or current timestamp.
    /// Information here is vital to retreive when you want to provide additional information to providers
    /// ( like liquid providers to get the quote id and its related properties like alias of the triggering event )
    /// using the automation.
    /// You can retreive related entities by just calling the property of the same name.
    /// Example: if you want to retreive the triggering Quote Id of the QuoteMigrationEvent
    /// you can access here via Quote.Id.
    /// </summary>
    public class AutomationDataContextManager
    {
        private static readonly Dictionary<EntityType, Func<PortalPageData, dynamic>> SupportedEntityTypes = new Dictionary<EntityType, Func<PortalPageData, dynamic>>
        {
            { EntityType.Customer, data => new EntityListReference<Customer>(data) },
            { EntityType.Quote, data => new EntityListReference<Quote>(data) },
            { EntityType.Policy, data => new EntityListReference<Policy>(data) },
            { EntityType.Claim, data => new EntityListReference<Claim>(data) },
            { EntityType.Message, data => new EntityListReference<Message>(data) },
            { EntityType.Organisation, data => new EntityListReference<Organisation>(data) },
            { EntityType.User, data => new EntityListReference<User>(data) },
            { EntityType.Role, data => new EntityListReference<Role>(data) },
            { EntityType.Portal, data => new EntityListReference<ProviderEntity.Portal>(data) },
            { EntityType.Product, data => new EntityListReference<Product>(data) },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationDataContextManager"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="organisationId">The organisation ID.</param>
        /// <param name="environment">The deployment environment.</param>
        public AutomationDataContextManager(
            Guid tenantId,
            Guid? organisationId,
            Dictionary<string, object> contextEntries)
        {
            this.Entries = contextEntries;

            var tenant = new ProviderEntity.Tenant(tenantId);
            this.SetContextEntity<ProviderEntity.Tenant>("tenant", tenant);

            if (organisationId != null)
            {
                var organisation = new Organisation(organisationId.Value);
                this.SetContextEntity<Organisation>("organisation", organisation);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationDataContextManager"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="organisationId">The Id of the organisation.</param>
        /// <param name="productId">The Id of the product.</param>
        /// <param name="environment">The deployment environment.</param>
        public AutomationDataContextManager(
            Guid tenantId,
            Guid? organisationId,
            Guid? productId,
            Guid? productReleaseId,
            Dictionary<string, object> contextEntries)
             : this(tenantId, organisationId, contextEntries)
        {
            if (productId.HasValue)
            {
                var product = new Product(productId.Value);
                this.SetContextEntity<Product>("product", product);
            }

            if (productReleaseId != null)
            {
                var productRelease = new ProductRelease(productReleaseId.Value);
                this.SetContextEntity<ProductRelease>("productRelease", productRelease);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationDataContextManager"/> class.
        /// </summary>
        /// <remarks>This is used during JSON Deserialisation of AutomationData.</remarks>
        public AutomationDataContextManager(Dictionary<string, object> contextEntries)
        {
            this.Entries = contextEntries;
        }

        /// <summary>
        /// Gets the tenant from the entities.
        /// </summary>
        public ProviderEntity.Tenant Tenant => (ProviderEntity.Tenant)this.Entries.GetValueOrDefault("tenant");

        /// <summary>
        /// Gets the organisation from the entities.
        /// </summary>
        public ProviderEntity.Organisation Organisation
            => (ProviderEntity.Organisation)this.Entries.GetValueOrDefault(key: "organisation");

        /// <summary>
        /// Gets the product from the entities.
        /// </summary>
        public ProviderEntity.Product Product => (ProviderEntity.Product)this.Entries.GetValueOrDefault(key: "product");

        /// <summary>
        /// Gets the performingUser from the entities.
        /// </summary>
        public ProviderEntity.User PerformingUser => (ProviderEntity.User)this.Entries.GetValueOrDefault(key: "performingUser");

        public ProviderEntity.ProductRelease ProductRelease
            => (ProviderEntity.ProductRelease)this.Entries.GetValueOrDefault(key: "productRelease");

        /// <summary>
        /// Gets or the entities.
        /// </summary>
        private Dictionary<string, object> Entries { get; }

        /// <summary>
        /// Updates the context with the system events metadata.
        /// </summary>
        /// <param name="relationships">The events relationships.</param>
        public void SetContextFromEventRelationships(IEnumerable<Domain.ReadWriteModel.Relationship> relationships)
        {
            if (relationships == null || !relationships.Any())
            {
                return;
            }

            foreach (var relationship in relationships)
            {
                switch (relationship.Type)
                {
                    case RelationshipType.QuoteEvent:
                        var quote = new Quote(relationship.FromEntityId);
                        this.SetContextEntity<Quote>("quote", quote);
                        break;
                    case RelationshipType.QuoteVersionEvent:
                        var quoteVersion = new QuoteVersion(relationship.FromEntityId);
                        this.SetContextEntity<QuoteVersion>("quoteVersion", quoteVersion);
                        break;
                    case RelationshipType.PolicyEvent:
                        var policy = new ProviderEntity.Policy(relationship.FromEntityId);
                        this.SetContextEntity<ProviderEntity.Policy>("policy", policy);
                        break;
                    case RelationshipType.PolicyTransactionEvent:
                        var policyTransaction = new PolicyTransaction(relationship.FromEntityId);
                        this.SetContextEntity<PolicyTransaction>("policyTransaction", policyTransaction);
                        break;
                    case RelationshipType.CustomerEvent:
                        var customer = new Customer(relationship.FromEntityId);
                        this.SetContextEntity<Customer>("customer", customer);
                        break;
                    case RelationshipType.EventPerformingUser:
                        var performingUser = new User(relationship.ToEntityId);
                        this.SetContextEntity<User>("performingUser", performingUser);
                        break;
                    case RelationshipType.ClaimEvent:
                        var claim = new Claim(relationship.FromEntityId);
                        this.SetContextEntity<Claim>("claim", claim);
                        break;
                    case RelationshipType.ClaimVersionEvent:
                        var claimVersion = new ClaimVersion(relationship.FromEntityId);
                        this.SetContextEntity<ClaimVersion>("claimVersion", claimVersion);
                        break;
                    case RelationshipType.EmailEvent:
                        var email = new EmailMessage(relationship.FromEntityId);
                        this.SetContextEntity<EmailMessage>("email", email);
                        break;
                    case RelationshipType.UserEvent:
                        var user = new User(relationship.FromEntityId);
                        this.SetContextEntity<User>("user", user);
                        break;
                    case RelationshipType.OrganisationEvent:
                        var organisation = new Organisation(relationship.FromEntityId);
                        this.SetContextEntity<Organisation>("organisation", organisation);
                        break;
                    case RelationshipType.PortalEvent:
                        var portal = new ProviderEntity.Portal(relationship.FromEntityId);
                        this.SetContextEntity<Organisation>("portal", portal);
                        break;
                    case RelationshipType.TenantEvent:
                        // unimplemented yet.
                        break;
                    case RelationshipType.ProductEvent:
                        // unimplemented yet.
                        break;
                    default:
                        break;
                }
            }
        }

        public void SetContextFromSystemEvent(SystemEvent systemEvent)
        {
            var eventObject = new Event(systemEvent);
            this.SetContextEntity<Event>("event", eventObject);
        }

        public void SetContextFromPortalPageTrigger(Dictionary<string, object> entities)
        {
            foreach (var entity in entities)
            {
                this.Entries[entity.Key] = entity.Value;
            }
        }

        public void SetContextFromPortalPageTrigger(PortalPageData portalPageData)
        {
            if (portalPageData.PageType == Enums.PageType.List)
            {
                var entityType = portalPageData.EntityType;

                if (SupportedEntityTypes.ContainsKey(entityType))
                {
                    var key = entityType.ToString().ToLower().Pluralize();
                    var data = this.CreateEntityListReference(entityType, portalPageData);
                    this.Entries[key] = data;
                }
            }

            if (portalPageData.UserId.HasValue)
            {
                this.Entries["performingUser"] = new User(portalPageData.UserId.Value);
            }
        }

        public void SetContextEntity(EntityType entityType, IEntity entity)
        {
            // adds or updates the entry.
            this.Entries[entityType.ToCamelCaseString()] = entity;
        }

        public void SetContextEntity<TEntity>(string entityKey, IEntity entity)
            where TEntity : IEntity
        {
            // adds or updates the entry.
            this.Entries[entityKey] = entity;
        }

        /// <summary>
        /// Adds the quote context into the automation data.
        /// </summary>
        /// <param name="quote">The quote to add to the context.</param>
        /// <param name="productConfig">The product configuration.</param>
        /// <param name="formDataPrettifier">The form data prettifier.</param>
        public void SetContextEntity(
            Domain.Aggregates.Quote.Quote quote,
            IProductConfiguration productConfig,
            IFormDataPrettifier formDataPrettifier)
        {
            // set quote context.
            if (quote != null)
            {
                var serializedQuote = new Quote(quote, productConfig, formDataPrettifier);
                this.SetContextEntity(EntityType.Quote, serializedQuote);
            }

            if (quote?.Aggregate?.CustomerId != null)
            {
                var customer = new Customer(quote.Aggregate.CustomerId.Value);
                this.SetContextEntity(EntityType.Customer, customer);
            }
        }

        /// <summary>
        /// Loads the given entity identified via the path.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <param name="dataPath">the data path.</param>
        public async Task LoadEntityAtPath(
            IProviderContext providerContext, string dataPath, IEnumerable<string>? similarPaths = null)
        {
            if (!dataPath.StartsWith("/context") && !dataPath.StartsWith("context."))
            {
                return;
            }

            char delimiter = dataPath.StartsWith("/context/") ? '/' :
                dataPath.StartsWith("context.") ? '.' : default;

            var segments = dataPath.Split(new char[] { delimiter }, options: StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length < 2)
            {
                // if accessing context. resolve all entries/entities.
                for (int i = 0; i < this.Entries.Count; i++)
                {
                    var item = this.Entries.ElementAt(i);
                    var entityItem = (IEntity)item.Value;
                    if (!entityItem.IsLoaded)
                    {
                        await this.FullyLoadEntity(item.Key, providerContext, entityItem);
                    }
                }

                return;
            }

            string pathPropertyName = segments[1];
            string property = segments.Length > 2 ? segments[2] : string.Empty;
            if (property == "id")
            {
                // if we just need the id and not the actual entity, then don't bother loading it
                return;
            }

            if (!this.Entries.ContainsKey(pathPropertyName))
            {
                // that entity doesn't exist in the context, however we won't raise an error here since it will be raised elsewhere
                return;
            }

            var entry = this.Entries[pathPropertyName];

            if (entry is IEntityListReference entityListReference)
            {
                var relatedEntities = PathHelper.GetRelatedEntities(pathPropertyName, similarPaths);

                this.FullyLoadEntityList(providerContext, entityListReference, pathPropertyName, relatedEntities);
                return;
            }

            if (!(entry is IEntity))
            {
                // it's not a lazy loadable type
                return;
            }

            IEntity entity = (IEntity)entry;
            if (entity.IsLoaded)
            {
                // no need to load it, it's already loaded.
                return;
            }

            await this.FullyLoadEntity(pathPropertyName, providerContext, entity);
        }

        private async Task FullyLoadEntity(string pathPropertyName, IProviderContext providerContext, IEntity entity)
        {
            using (MiniProfiler.Current.Step(nameof(AutomationDataContextManager) + "." + nameof(this.FullyLoadEntity) + "(" + entity.GetType().Name.ToCamelCase() + ")"))
            {
                var id = entity.Id;
                if (providerContext.AutomationData.ServiceProvider == null)
                {
                    var debugContext = await providerContext.GetDebugContext();
                    debugContext.Add(ErrorDataKey.EntityType, entity.GetType().Name);
                    debugContext.Add(ErrorDataKey.EntityId, id);
                    debugContext.Add("lazyLoadEntity", pathPropertyName);
                    throw new ErrorException(Errors.Automation.ServiceProviderNotFound(debugContext));
                }

                var entityIdBuilder = new StaticBuilder<Data<string>>() { Value = id.ToString() };
                var entityTypeBuilder = new StaticBuilder<Data<string>>() { Value = entity.GetType().Name.ToCamelCase() };
                var dynamicEntityProviderConfigModel = new DynamicEntityProviderConfigModel()
                {
                    EntityType = entityTypeBuilder,
                    EntityId = entityIdBuilder,
                };
                var dynamicEntityProvider
                    = dynamicEntityProviderConfigModel.Build(providerContext.AutomationData.ServiceProvider);

                if (entity is Quote quote)
                {
                    dynamicEntityProvider.IncludedProperties?.AddRange(quote.IncludedProperties?.ToList() ?? new List<string>());
                }

                var fullyLoadedEntity = (await dynamicEntityProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
                this.Entries[pathPropertyName] = fullyLoadedEntity;
            }
        }

        private void FullyLoadEntityList(
            IProviderContext providerContext,
            IEntityListReference entityListReference,
            string pathPropertyName,
            IEnumerable<string> relatedEntities)
        {
            if (providerContext.AutomationData.ServiceProvider == null)
            {
                var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;
                var productId = providerContext.AutomationData.ContextManager.Product?.Id;
                var environment = providerContext.AutomationData.System.Environment;
                throw new ErrorException(Errors.Automation.ServiceProviderNotFound(
                    GenericErrorDataHelper.GetGeneralErrorDetails(tenantId, productId, environment)));
            }

            var portalPageTriggerService =
                providerContext.AutomationData.ServiceProvider.GetRequiredService<IAutomationPortalPageTriggerService>();
            this.Entries[pathPropertyName] = portalPageTriggerService.GetEntityList(entityListReference, relatedEntities);
        }

        private dynamic CreateEntityListReference(EntityType entityType, PortalPageData portalPageData)
        {
            if (SupportedEntityTypes.TryGetValue(entityType, out var createEntityList))
            {
                return createEntityList(portalPageData);
            }

            throw new NotSupportedException($"Entity type {entityType} is not supported.");
        }
    }
}

// <copyright file="OrganisationService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Notification;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using OrganisationAggregate = UBind.Domain.Aggregates.Organisation.Organisation;

    /// <inheritdoc/>
    public class OrganisationService : IOrganisationService
    {
        private readonly IOrganisationAggregateRepository organisationAggregateRepository;
        private readonly IOrganisationReadModelRepository organisationReadModelRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IAdditionalPropertyValueService additionalPropertyValueService;
        private readonly ITenantRepository tenantRepository;
        private readonly IClock clock;
        private readonly ICqrsMediator mediator;
        private readonly ICachingResolver cachingResolver;
        private readonly IProductRepository productRepository;
        private readonly IProductFeatureSettingRepository productFeatureSettingRepository;
        private readonly IProductOrganisationSettingRepository productOrganisationSettingRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganisationService"/> class.
        /// </summary>
        /// <param name="organisationAggregateRepository">The organisation aggregate repository.</param>
        /// <param name="organisationReadModelRepository">The organisation read model repository.</param>
        /// <param name="tenantRepository">The tenant repository.</param>
        /// <param name="additionalPropertyValueService">Additional property value service.</param>
        /// <param name="httpContextPropertiesResolver">The performing user resolver.</param>
        /// <param name="clock">Represents a clock which can return the current time as an Instant.</param>
        public OrganisationService(
            IOrganisationAggregateRepository organisationAggregateRepository,
            IOrganisationReadModelRepository organisationReadModelRepository,
            ITenantRepository tenantRepository,
            IAdditionalPropertyValueService additionalPropertyValueService,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            ICqrsMediator mediator,
            ICachingResolver cachingResolver,
            IClock clock,
            IProductRepository productRepository,
            IProductFeatureSettingRepository productFeatureSettingRepository,
            IProductOrganisationSettingRepository productOrganisationSettingRepository)
        {
            this.organisationAggregateRepository = organisationAggregateRepository;
            this.organisationReadModelRepository = organisationReadModelRepository;
            this.tenantRepository = tenantRepository;
            this.clock = clock;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.mediator = mediator;
            this.cachingResolver = cachingResolver;
            this.additionalPropertyValueService = additionalPropertyValueService;
            this.productRepository = productRepository;
            this.productFeatureSettingRepository = productFeatureSettingRepository;
            this.productOrganisationSettingRepository = productOrganisationSettingRepository;
        }

        public async Task<OrganisationAggregate> CreateOrganisation(
            Guid tenantId,
            string? alias,
            string name,
            Guid? managingOrganisationId,
            List<AdditionalPropertyValueUpsertModel>? properties = null,
            List<LinkedIdentity>? linkedIdentities = null)
        {
            var now = this.clock.Now();
            var tenant = await this.cachingResolver.GetTenantOrThrow(tenantId);
            await this.ThrowIfHasDuplicateName(tenant.Id, name);
            if (!string.IsNullOrEmpty(alias))
            {
                await this.ThrowIfHasDuplicateAlias(tenant.Id, alias);
            }
            else
            {
                alias = this.GenerateAlias(tenantId, name);
            }

            if (managingOrganisationId != null)
            {
                var managingOrganisation = await this.cachingResolver.GetOrganisationOrThrow(
                    tenant, new GuidOrAlias(managingOrganisationId));
            }

            var organisationAggregate = Organisation.CreateNewOrganisation(
                tenantId,
                alias,
                name,
                managingOrganisationId,
                this.httpContextPropertiesResolver.PerformingUserId,
                now);

            if (linkedIdentities != null)
            {
                foreach (var identity in linkedIdentities)
                {
                    if (!string.IsNullOrEmpty(identity.UniqueId))
                    {
                        await this.ThrowIfHasDuplicateLinkedIdentity(
                            tenantId, identity.AuthenticationMethodId, identity.UniqueId);

                        organisationAggregate.LinkIdentity(
                        identity.AuthenticationMethodId,
                        identity.UniqueId,
                        this.httpContextPropertiesResolver.PerformingUserId,
                        now);
                    }
                }
            }

            if (properties != null && properties.Any())
            {
                await this.additionalPropertyValueService.UpsertValuesForOrganisation(
                    properties,
                    organisationAggregate,
                    this.httpContextPropertiesResolver.PerformingUserId,
                    now);
            }

            await this.organisationAggregateRepository.ApplyChangesToDbContext(organisationAggregate);
            await this.UpdateProductOrganisationSettings(tenantId, organisationAggregate.Id);
            return organisationAggregate;
        }

        /// <inheritdoc/>
        public IOrganisationReadModelSummary GetOrganisationSummaryForTenantAliasAndOrganisationAlias(
            string tenantAlias, string organisationAlias)
        {
            var tenant = this.tenantRepository.GetTenantByAlias(tenantAlias);
            if (tenant == null)
            {
                throw new ErrorException(Errors.Tenant.NotFound(tenantAlias));
            }

            var tenantOrganisationId = tenant.Details.DefaultOrganisationId;
            OrganisationReadModel? organisation = null;
            if (string.IsNullOrEmpty(organisationAlias))
            {
                organisation = this.organisationReadModelRepository.Get(tenant.Id, tenantOrganisationId);
                if (organisation == null)
                {
                    throw new ErrorException(Errors.Organisation.NotFound(tenantOrganisationId));
                }
            }
            else
            {
                organisation = this.organisationReadModelRepository.GetByAlias(tenant.Id, organisationAlias);
                if (organisation == null)
                {
                    throw new ErrorException(Errors.Organisation.NotFound(organisationAlias));
                }
            }

            return GetSummary(organisation, tenantOrganisationId);
        }

        /// <inheritdoc/>
        public async Task<IOrganisationReadModelSummary> GetOrganisationSummaryForTenantIdAndOrganisationId(
            Guid tenantId, Guid organisationId)
        {
            var tenant = await this.cachingResolver.GetTenantOrThrow(tenantId);
            var organisation = await this.cachingResolver.GetOrganisationOrThrow(tenant.Id, organisationId);
            return GetSummary(organisation, tenant.Details.DefaultOrganisationId);
        }

        /// <inheritdoc/>
        public async Task<bool> IsOrganisationDefaultForTenant(Guid tenantId, Guid organisationId)
        {
            var tenant = await this.cachingResolver.GetTenantOrThrow(tenantId);
            await this.cachingResolver.GetOrganisationOrThrow(tenant.Id, organisationId);
            return tenant.Details.DefaultOrganisationId == organisationId;
        }

        /// <inheritdoc/>
        public async Task<bool> IsOrganisationDefaultForTenant(Guid tenantId, string organisationAlias)
        {
            var tenant = await this.cachingResolver.GetTenantOrThrow(tenantId);
            var organisation = await this.cachingResolver.GetOrganisationOrThrow(tenant.Id, new GuidOrAlias(organisationAlias));
            return tenant.Details.DefaultOrganisationId == organisation.Id;
        }

        /// <inheritdoc/>
        public IOrganisationReadModelSummary GetActiveOrganisationById(Guid tenantId, Guid organisationId)
        {
            var tenant = this.tenantRepository.GetTenantById(tenantId);
            if (tenant == null)
            {
                throw new ErrorException(Errors.Organisation.TenantNotFound(tenantId));
            }

            var organisationReadModel = this.organisationReadModelRepository.Get(tenantId, organisationId);
            if (organisationReadModel == null)
            {
                throw new ErrorException(Errors.Organisation.NotFound(organisationId));
            }
            else if (!organisationReadModel.IsActive)
            {
                throw new ErrorException(Errors.Organisation.Disabled(organisationReadModel.Name));
            }

            var defaultOrganisationId = tenant.Details.DefaultOrganisationId;
            return GetSummary(organisationReadModel, defaultOrganisationId);
        }

        /// <inheritdoc/>
        public IOrganisationReadModelSummary GetDefaultOrganisationForTenant(Guid tenantId)
        {
            var tenant = this.tenantRepository.GetTenantById(tenantId);
            if (tenant == null)
            {
                throw new ErrorException(Errors.Organisation.TenantNotFound(tenantId));
            }

            var defaultOrganisationId = tenant.Details.DefaultOrganisationId;
            var organisationReadModel = this.organisationReadModelRepository.Get(tenantId, defaultOrganisationId);
            if (organisationReadModel == null)
            {
                throw new ErrorException(Errors.Organisation.NotFound(defaultOrganisationId));
            }

            return GetSummary(organisationReadModel, defaultOrganisationId);
        }

        /// <inheritdoc/>
        public Tenant GetTenantFromOrganisationId(Guid tenantId, Guid organisationId)
        {
            var organisation = this.organisationAggregateRepository.GetById(tenantId, organisationId);
            if (organisation == null)
            {
                throw new ErrorException(Errors.Organisation.NotFound(organisationId));
            }

            var tenant = this.tenantRepository.GetTenantById(organisation.TenantId);
            if (tenant == null)
            {
                throw new ErrorException(Errors.Tenant.NotFound(organisation.TenantId));
            }

            return tenant;
        }

        /// <inheritdoc/>
        public async Task ValidateOrganisationBelongsToTenantAndIsActive(Guid tenantId, Guid organisationId)
        {
            await this.ValidateTenant(tenantId);
            var organisation = await this.cachingResolver.GetOrganisationOrThrow(tenantId, organisationId);
            if (!organisation.IsActive)
            {
                throw new ErrorException(Errors.Organisation.Login.Disabled(organisation.Name));
            }
        }

        public void ValidateOrganisationIsActive(OrganisationReadModel organisation, Guid organisationId)
        {
            if (organisation == null)
            {
                throw new ErrorException(Errors.Organisation.NotFound(organisationId));
            }
            else if (!organisation.IsActive)
            {
                throw new ErrorException(Errors.Organisation.Login.Disabled(organisation.Name));
            }
        }

        /// <inheritdoc/>
        public async Task<IOrganisationReadModelSummary> CreateActiveNonDefaultAsync(
            Guid tenantId,
            string alias,
            string name,
            List<AdditionalPropertyValueUpsertModel>? properties)
        {
            var tenant = this.tenantRepository.GetTenantById(tenantId);
            tenant = EntityHelper.ThrowIfNotFound(tenant, tenantId, "tenant");
            await this.ValidateTenant(tenantId, tenant, alias, name);
            this.ThrowIfOrganisationAliasIsNull(alias);

            async Task<Organisation> CreateAndSave()
            {
                var organisationAggregate = Organisation.CreateNewOrganisation(
                    tenant.Id,
                    alias,
                    name,
                    null,
                    this.httpContextPropertiesResolver.PerformingUserId,
                    this.clock.GetCurrentInstant());

                if (properties != null && properties.Any())
                {
                    await this.additionalPropertyValueService.UpsertValuesForOrganisation(
                        properties,
                        organisationAggregate,
                        this.httpContextPropertiesResolver.PerformingUserId,
                        this.clock.GetCurrentInstant());
                }

                await this.organisationAggregateRepository.Save(organisationAggregate);

                return organisationAggregate;
            }

            var organisation = await ConcurrencyPolicy.ExecuteWithRetriesAsync(CreateAndSave);
            var organisationReadModel = this.organisationReadModelRepository.Get(tenantId, organisation.Id);
            organisationReadModel = EntityHelper.ThrowIfNotFound(organisationReadModel, organisation.Id, "organisation");
            return GetSummary(organisationReadModel, tenant.Details.DefaultOrganisationId);
        }

        /// <inheritdoc/>
        public async Task<IOrganisationReadModelSummary> CreateDefaultAsync(
            Guid tenantId, string alias, string name)
        {
            var tenant = this.tenantRepository.GetTenantById(tenantId);
            tenant = EntityHelper.ThrowIfNotFound(tenant, tenantId, "tenant");
            await this.ValidateTenant(tenantId, tenant, alias, name);

            async Task<Organisation> CreateAndSave()
            {
                var organisationAggregate = Organisation.CreateNewOrganisation(
                    tenant.Id,
                    alias,
                    name,
                    null, this.httpContextPropertiesResolver.PerformingUserId,
                    this.clock.GetCurrentInstant());
                organisationAggregate.SetDefault(true, null, this.clock.GetCurrentInstant());
                await this.organisationAggregateRepository.Save(organisationAggregate);

                tenant.SetDefaultOrganisation(organisationAggregate.Id, this.clock.GetCurrentInstant());
                this.tenantRepository.SaveChanges();

                return organisationAggregate;
            }

            var organisation = await ConcurrencyPolicy.ExecuteWithRetriesAsync(
                CreateAndSave,
                () => tenant = this.tenantRepository.GetTenantById(tenantId));
            var organisationReadModel = this.organisationReadModelRepository.Get(tenantId, organisation.Id);
            organisationReadModel = EntityHelper.ThrowIfNotFound(organisationReadModel, organisation.Id, "organisation");
            return GetSummary(organisationReadModel, tenant.Details.DefaultOrganisationId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<IOrganisationReadModelSummary>> ListOrganisationsForUser(
            Guid tenantId, OrganisationReadModelFilters filters)
        {
            var tenant = await this.cachingResolver.GetTenantOrThrow(tenantId);
            ThrowIfTenantNotFound(tenantId, tenant);

            var tenantDefaultOrganisationId = tenant.Details.DefaultOrganisationId;

            var organisations = this.organisationReadModelRepository.Get(tenant.Id, filters);
            return organisations.Select(o => GetSummary(o, tenantDefaultOrganisationId));
        }

        /// <inheritdoc/>
        public async Task<IOrganisationReadModelSummary> GetOrganisationById(
            Guid tenantId, Guid organisationAggregateId)
        {
            var tenant = await this.cachingResolver.GetTenantOrThrow(tenantId);
            tenant = EntityHelper.ThrowIfNotFound(tenant, tenantId, "tenant");

            var organisation = this.organisationReadModelRepository.Get(tenant.Id, organisationAggregateId);
            organisation = EntityHelper.ThrowIfNotFound(organisation, organisationAggregateId, "organisation");

            return GetSummary(organisation, tenant.Details.DefaultOrganisationId);
        }

        public async Task<OrganisationAggregate> UpdateOrganisation(
            Guid tenantId,
            Guid organisationId,
            string organisationName,
            string organisationAlias,
            List<AdditionalPropertyValueUpsertModel>? properties = null,
            List<LinkedIdentity>? linkedIdentities = null)
        {
            var now = this.clock.Now();
            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
            var tenant = await this.cachingResolver.GetTenantOrThrow(tenantId);
            this.ThrowIfOrganisationAliasIsNull(organisationAlias);
            await this.ThrowIfHasDuplicateAlias(tenant.Id, organisationAlias, organisationId);
            await this.ThrowIfHasDuplicateName(tenant.Id, organisationName, organisationId);
            var organisationAggregate = this.organisationAggregateRepository.GetById(tenantId, organisationId);
            organisationAggregate = EntityHelper.ThrowIfNotFound(organisationAggregate, organisationId);

            if (organisationAggregate.Alias != organisationAlias)
            {
                var onAliasChange = new OrganisationAliasChangedDomainEvent(
                    tenant.Id,
                    organisationAggregate.Id,
                    organisationAggregate.Alias,
                    organisationAlias,
                    performingUserId,
                    now);
                await this.mediator.Publish(onAliasChange);
            }

            organisationAggregate.Update(
                organisationAlias,
                organisationName,
                performingUserId,
                this.clock.GetCurrentInstant());

            if (properties != null)
            {
                await this.additionalPropertyValueService.UpsertValuesForOrganisation(
                    properties,
                    organisationAggregate,
                    performingUserId,
                    now);
            }

            if (linkedIdentities != null)
            {
                foreach (var identity in linkedIdentities)
                {
                    await this.ThrowIfHasDuplicateLinkedIdentity(
                        tenantId, identity.AuthenticationMethodId, identity.UniqueId, organisationId);

                    var existingIdentity
                        = organisationAggregate.LinkedIdentities
                            .FirstOrDefault(i => i.AuthenticationMethodId == identity.AuthenticationMethodId);
                    if (existingIdentity == null)
                    {
                        if (!string.IsNullOrEmpty(identity.UniqueId))
                        {
                            organisationAggregate.LinkIdentity(
                            identity.AuthenticationMethodId, identity.UniqueId, performingUserId, now);
                        }
                    }
                    else
                    {
                        if (existingIdentity.UniqueId != identity.UniqueId)
                        {
                            organisationAggregate.UpdateLinkedIdentity(
                                identity.AuthenticationMethodId, identity.UniqueId, performingUserId, now);
                        }
                    }
                }
            }

            await this.organisationAggregateRepository.ApplyChangesToDbContext(organisationAggregate);
            return organisationAggregate;
        }

        /// <inheritdoc/>
        public async Task<IOrganisationReadModelSummary> MarkAsDeleted(
            Guid tenantId, Guid organisationId)
        {
            var organisationAggregate = this.organisationAggregateRepository.GetById(tenantId, organisationId);
            organisationAggregate = EntityHelper.ThrowIfNotFound(organisationAggregate, organisationId, "organisation");
            var tenant = await this.cachingResolver.GetTenantOrThrow(tenantId);

            organisationAggregate.MarkAsDeleted(this.httpContextPropertiesResolver.PerformingUserId, this.clock.GetCurrentInstant());
            await this.organisationAggregateRepository.ApplyChangesToDbContext(organisationAggregate);
            this.cachingResolver.RemoveCachedOrganisations(tenantId, organisationId, new List<string> { organisationAggregate.Alias });

            return GetSummary(organisationAggregate, tenant.Details.DefaultOrganisationId);
        }

        /// <inheritdoc/>
        public async Task<IOrganisationReadModelSummary> Activate(
            Guid tenantId, Guid organisationId)
        {
            var organisationAggregate = this.organisationAggregateRepository.GetById(tenantId, organisationId);
            organisationAggregate = EntityHelper.ThrowIfNotFound(organisationAggregate, organisationId, "organisation");
            var tenant = this.tenantRepository.GetTenantById(tenantId);
            tenant = EntityHelper.ThrowIfNotFound(tenant, tenantId, "tenant");

            async Task<Organisation> SetAsActive()
            {
                organisationAggregate.Activate(this.httpContextPropertiesResolver.PerformingUserId, this.clock.GetCurrentInstant());
                await this.organisationAggregateRepository.Save(organisationAggregate);
                this.cachingResolver.RemoveCachedOrganisations(tenantId, organisationId, new List<string> { organisationAggregate.Alias });
                return organisationAggregate;
            }

            var organisation = await ConcurrencyPolicy.ExecuteWithRetriesAsync(
                SetAsActive,
                () => organisationAggregate = this.organisationAggregateRepository.GetById(tenantId, organisationId));
            var organisationReadModel = this.organisationReadModelRepository.Get(tenant.Id, organisation.Id);
            organisationReadModel = EntityHelper.ThrowIfNotFound(organisationReadModel, organisation.Id, "organisation");
            return GetSummary(organisationReadModel, tenant.Details.DefaultOrganisationId);
        }

        /// <inheritdoc/>
        public async Task<IOrganisationReadModelSummary> Disable(
            Guid tenantId, Guid organisationId)
        {
            var organisationAggregate = this.organisationAggregateRepository.GetById(tenantId, organisationId);
            organisationAggregate = EntityHelper.ThrowIfNotFound(organisationAggregate, organisationId, "organisation");
            var tenant = this.tenantRepository.GetTenantById(tenantId);
            tenant = EntityHelper.ThrowIfNotFound(tenant, tenantId, "tenant");

            async Task<Organisation> SetAsDisabled()
            {
                organisationAggregate.Disable(this.httpContextPropertiesResolver.PerformingUserId, this.clock.GetCurrentInstant());
                await this.organisationAggregateRepository.Save(organisationAggregate);
                this.cachingResolver.RemoveCachedOrganisations(tenantId, organisationId, new List<string> { organisationAggregate.Alias });
                return organisationAggregate;
            }

            var organisation = await ConcurrencyPolicy.ExecuteWithRetriesAsync(
                SetAsDisabled,
                () => organisationAggregate = this.organisationAggregateRepository.GetById(tenantId, organisationId));
            var organisationReadModel = this.organisationReadModelRepository.Get(tenant.Id, organisation.Id);
            organisationReadModel = EntityHelper.ThrowIfNotFound(organisationReadModel, organisation.Id, "organisation");
            return GetSummary(organisationReadModel, tenant.Details.DefaultOrganisationId);
        }

        public void ThrowIfOrganisationAliasIsNull(string alias)
        {
            if (!string.IsNullOrEmpty(alias) && alias.ToLower() == "null")
            {
                throw new ErrorException(Errors.Organisation.AliasIsNull(alias));
            }
        }

        public async Task ThrowIfHasDuplicateName(Guid tenantId, string organisationName, Guid? organisationId = null)
        {
            if (this.organisationReadModelRepository.IsNameInUse(tenantId, organisationName, organisationId))
            {
                var tenant = await this.cachingResolver.GetTenantOrNull(tenantId);
                var tenantAlias = tenant?.Details.Alias ?? tenantId.ToString();
                throw new ErrorException(Errors.Organisation.NameUnderTenantAlreadyExists(tenantAlias, organisationName));
            }
        }

        public async Task ThrowIfHasDuplicateAlias(Guid tenantId, string organisationAlias, Guid? organisationId = null)
        {
            if (this.organisationReadModelRepository.IsAliasInUse(tenantId, organisationAlias, organisationId))
            {
                var tenant = await this.cachingResolver.GetTenantOrNull(tenantId);
                var tenantAlias = tenant?.Details.Alias ?? tenantId.ToString();
                throw new ErrorException(Errors.Organisation.AliasUnderTenantAlreadyExists(tenantAlias, organisationAlias));
            }
        }

        public async Task ThrowIfHasDuplicateLinkedIdentity(Guid tenantId, Guid authenticationMethodId, string organisationExternalId, Guid? excludedOrganisationId = null)
        {
            var organisation = this.organisationReadModelRepository.GetLinkedOrganisation(tenantId, authenticationMethodId, organisationExternalId, excludedOrganisationId);
            if (organisation != null)
            {
                var tenant = await this.cachingResolver.GetTenantOrNull(tenantId);
                var tenantAlias = tenant?.Details.Alias;
                var data = new JObject()
                {
                    { "tenantAlias", tenantAlias },
                    { "authenticationMethodId", authenticationMethodId },
                    { "organisationExternalId", organisationExternalId },
                    { "existingOrganisationId", organisation.Id },
                };
                throw new ErrorException(Errors.Organisation.LinkedIdentityAlreadyExists(organisationExternalId, data));
            }
        }

        private static IOrganisationReadModelSummary GetSummary(
            IOrganisationReadModelSummary organisation, Guid tenantDefaultOrganisationId)
        {
            return new OrganisationReadModelSummary
            {
                TenantId = organisation.TenantId,
                Id = organisation.Id,
                Alias = organisation.Alias,
                Name = organisation.Name,
                IsActive = organisation.IsActive,
                IsDeleted = organisation.IsDeleted,
                IsDefault = organisation.Id == tenantDefaultOrganisationId,
                CreatedTimestamp = organisation.CreatedTimestamp,
                LastModifiedTimestamp = organisation.LastModifiedTimestamp,
            };
        }

        private static IOrganisationReadModelSummary GetSummary(
            Organisation organisation, Guid defaultOrganisationId)
        {
            return new OrganisationReadModelSummary
            {
                TenantId = organisation.TenantId,
                Id = organisation.Id,
                Alias = organisation.Alias,
                Name = organisation.Name,
                IsActive = organisation.IsActive,
                IsDeleted = organisation.IsDeleted,
                IsDefault = organisation.Id == defaultOrganisationId,
                CreatedTimestamp = organisation.CreatedTimestamp,
                LastModifiedTimestamp = organisation.LastModifiedTimestamp,
            };
        }

        private static void ThrowIfTenantNotFound(Guid tenantId, Tenant tenant)
        {
            if (tenant == null)
            {
                throw new ErrorException(Errors.Organisation.TenantNotFound(tenantId));
            }
        }

        private static void ThrowIfTenantIsDisabled(Tenant tenant)
        {
            if (tenant.Details.Disabled)
            {
                throw new ErrorException(Errors.Tenant.Disabled(tenant.Details.Name));
            }
        }

        private async Task ValidateTenant(Guid tenantId)
        {
            var tenant = await this.cachingResolver.GetTenantOrThrow(tenantId);
            ThrowIfTenantIsDisabled(tenant);
        }

        private async Task ValidateTenant(Guid tenantId, Tenant tenant, string alias, string name)
        {
            ThrowIfTenantNotFound(tenantId, tenant);
            await this.ThrowIfHasDuplicateAlias(tenantId, alias);
            await this.ThrowIfHasDuplicateName(tenantId, name);
        }

        private async Task UpdateProductOrganisationSettings(Guid tenantId, Guid organisationId)
        {
            var products = this.productRepository.GetAllProductsForTenant(tenantId).ToList();
            foreach (var product in products)
            {
                if (product.Details.Disabled)
                {
                    continue;
                }

                var productFeature = this.productFeatureSettingRepository.GetProductFeatureSetting(tenantId, product.Id);
                if (productFeature != null && productFeature.AllowQuotesForNewOrganisations)
                {
                    await this.productOrganisationSettingRepository.UpdateProductSetting(tenantId, organisationId, product.Id, true);
                }
            }
        }

        private string GenerateAlias(Guid tenantId, string name)
        {
            string alias = Converter.NameToAlias(name);
            string indexedAlias = alias;
            int index = 0;

            // if it's taken, add an index to the end
            while (this.organisationReadModelRepository.IsAliasInUse(tenantId, indexedAlias))
            {
                ++index;
                indexedAlias = $"{alias}-{index}";
            }

            return indexedAlias;
        }
    }
}

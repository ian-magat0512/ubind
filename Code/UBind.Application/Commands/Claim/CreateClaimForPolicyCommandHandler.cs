// <copyright file="CreateClaimForPolicyCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Claim
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Exceptions;
    using UBind.Domain.NumberGenerators;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.Services.AdditionalPropertyValue;

    /// <summary>
    /// Creates a claim against a policy.
    /// </summary>
    public class CreateClaimForPolicyCommandHandler : ICommandHandler<CreateClaimForPolicyCommand, Guid>
    {
        private readonly IConfigurationService configurationService;
        private readonly ICachingResolver cachingResolver;
        private readonly IProductFeatureSettingRepository productFeatureRepository;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly ICustomerAggregateRepository customerAggregateRepository;
        private readonly IPersonAggregateRepository personAggregateRepository;
        private readonly IClaimAggregateRepository claimAggregateRepository;
        private readonly IClaimReferenceNumberGenerator claimReferenceNumberGenerator;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;
        private readonly IAdditionalPropertyValueService additionalPropertyValueService;
        private readonly ICqrsMediator cqrsMediator;

        public CreateClaimForPolicyCommandHandler(
            IConfigurationService configurationService,
            ICachingResolver cachingResolver,
            IProductFeatureSettingRepository productFeatureRepository,
            IQuoteAggregateRepository quoteAggregateRepository,
            ICustomerAggregateRepository customerAggregateRepository,
            IPersonAggregateRepository personAggregateRepository,
            IClaimAggregateRepository claimAggregateRepository,
            IClaimReferenceNumberGenerator claimReferenceNumberGenerator,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock,
            IAdditionalPropertyValueService additionalPropertyValueService,
            ICqrsMediator cqrsMediator)
        {
            this.configurationService = configurationService;
            this.cachingResolver = cachingResolver;
            this.productFeatureRepository = productFeatureRepository;
            this.claimAggregateRepository = claimAggregateRepository;
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.customerAggregateRepository = customerAggregateRepository;
            this.personAggregateRepository = personAggregateRepository;
            this.claimReferenceNumberGenerator = claimReferenceNumberGenerator;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
            this.additionalPropertyValueService = additionalPropertyValueService;
            this.cqrsMediator = cqrsMediator;
        }

        /// <inheritdoc/>
        public async Task<Guid> Handle(CreateClaimForPolicyCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var quoteAggregate = this.quoteAggregateRepository.GetById(request.TenantId, request.PolicyId);

            // check that claims are enabled for this product
            ProductFeatureSetting productFeature =
                this.productFeatureRepository.GetProductFeatureSetting(request.TenantId, quoteAggregate.ProductId);
            if (!productFeature.IsClaimsEnabled)
            {
                var product = await this.cachingResolver.GetProductOrThrow(request.TenantId, quoteAggregate.ProductId);
                throw new ErrorException(Errors.ProductFeatureSetting.ProductFeatureDisabledForTheOperation(
                    product.Id, product.Details.Name, "Claim", "create claim"));
            }

            // check the product has a claim component deployed to this environment
            if (!this.configurationService.DoesConfigurationExist(quoteAggregate.ProductContext, WebFormAppType.Claim))
            {
                throw new ErrorException(Errors.Product.Component.NotFound(quoteAggregate.ProductContext, WebFormAppType.Claim));
            }

            Guid? primaryPersonId = null;
            string personFullName = null;
            string personPreferredName = null;
            if (quoteAggregate.HasCustomer)
            {
                var customer = this.customerAggregateRepository.GetById(request.TenantId, quoteAggregate.CustomerId.Value);
                primaryPersonId = customer.PrimaryPersonId;
                var person = this.personAggregateRepository.GetById(request.TenantId, customer.PrimaryPersonId);
                personFullName = person.FullName;
                personPreferredName = person.PreferredName;
            }

            this.claimReferenceNumberGenerator.SetProperties(
                quoteAggregate.TenantId, quoteAggregate.ProductId, quoteAggregate.Environment);
            var claimReference = this.claimReferenceNumberGenerator.Generate();
            var claimAggregate = ClaimAggregate.CreateForPolicy(
                claimReference,
                quoteAggregate,
                primaryPersonId,
                personFullName,
                personPreferredName,
                this.httpContextPropertiesResolver.PerformingUserId,
                this.clock.GetCurrentInstant());

            var ownerUserId = request.OwnerUserId ?? quoteAggregate.OwnerUserId;

            if (ownerUserId.HasValue)
            {
                claimAggregate.AssignToOwner(ownerUserId.Value, this.httpContextPropertiesResolver.PerformingUserId, this.clock.GetCurrentInstant());
            }

            await this.claimAggregateRepository.Save(claimAggregate);
            return await Task.FromResult(claimAggregate.Id);
        }
    }
}

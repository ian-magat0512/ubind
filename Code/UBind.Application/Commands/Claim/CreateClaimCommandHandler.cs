// <copyright file="CreateClaimCommandHandler.cs" company="uBind">
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
    using UBind.Domain.Exceptions;
    using UBind.Domain.NumberGenerators;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;

    public class CreateClaimCommandHandler : ICommandHandler<CreateClaimCommand, Guid>
    {
        private readonly ICqrsMediator cqrsMediator;
        private readonly IConfigurationService configurationService;
        private readonly ICachingResolver cachingResolver;
        private readonly IProductFeatureSettingRepository productFeatureRepository;
        private readonly ICustomerAggregateRepository customerAggregateRepository;
        private readonly IPersonAggregateRepository personAggregateRepository;
        private readonly IClaimAggregateRepository claimAggregateRepository;
        private readonly IClaimReferenceNumberGenerator claimReferenceNumberGenerator;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;

        public CreateClaimCommandHandler(
            IConfigurationService configurationService,
            ICachingResolver cachingResolver,
            IProductFeatureSettingRepository productFeatureRepository,
            ICustomerAggregateRepository customerAggregateRepository,
            IPersonAggregateRepository personAggregateRepository,
            IClaimAggregateRepository claimAggregateRepository,
            IClaimReferenceNumberGenerator claimReferenceNumberGenerator,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock,
            ICqrsMediator cqrsMediator)
        {
            this.cqrsMediator = cqrsMediator;
            this.configurationService = configurationService;
            this.cachingResolver = cachingResolver;
            this.productFeatureRepository = productFeatureRepository;
            this.claimAggregateRepository = claimAggregateRepository;
            this.customerAggregateRepository = customerAggregateRepository;
            this.personAggregateRepository = personAggregateRepository;
            this.claimReferenceNumberGenerator = claimReferenceNumberGenerator;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public async Task<Guid> Handle(CreateClaimCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // check that claims are enabled for this product
            ProductFeatureSetting productFeature =
                this.productFeatureRepository.GetProductFeatureSetting(request.TenantId, request.ProductId);
            if (!productFeature.IsClaimsEnabled)
            {
                var product = await this.cachingResolver.GetProductOrThrow(request.TenantId, request.ProductId);
                throw new ErrorException(Errors.ProductFeatureSetting.ProductFeatureDisabledForTheOperation(
                    product.Id, product.Details.Name, "Claim", "create claim"));
            }

            // check that we are allowed to create standalone claims for this product (not against a policy)
            if (productFeature.MustCreateClaimAgainstPolicy)
            {
                var product = await this.cachingResolver.GetProductOrThrow(request.TenantId, request.ProductId);
                throw new ErrorException(Errors.Claim.MustCreateAgainstPolicy(
                    product.Details.Alias, product.Details.Name, "create claim"));
            }

            // check the product has a claim component deployed to this environment
            ProductContext productContext = new ProductContext(
                request.TenantId, request.ProductId, request.Environment);
            if (!this.configurationService.DoesConfigurationExist(productContext, WebFormAppType.Claim))
            {
                throw new ErrorException(Errors.Product.Component.NotFound(productContext, WebFormAppType.Claim));
            }

            Guid? primaryPersonId = null;
            string personFullName = null;
            string personPreferredName = null;
            if (request.CustomerId.HasValue)
            {
                var customer = this.customerAggregateRepository.GetById(request.TenantId, request.CustomerId.Value);
                primaryPersonId = customer.PrimaryPersonId;
                var person = this.personAggregateRepository.GetById(request.TenantId, customer.PrimaryPersonId);
                personFullName = person.FullName;
                personPreferredName = person.PreferredName;
            }

            this.claimReferenceNumberGenerator.SetProperties(
                request.TenantId, request.ProductId, request.Environment);
            var claimReference = this.claimReferenceNumberGenerator.Generate();
            var claimAggregate = ClaimAggregate.CreateWithoutPolicy(
                request.TenantId,
                request.OrganisationId,
                request.ProductId,
                request.Environment,
                claimReference,
                request.IsTestData,
                request.CustomerId,
                primaryPersonId,
                personFullName,
                personPreferredName,
                this.httpContextPropertiesResolver.PerformingUserId,
                this.clock.GetCurrentInstant(),
                request.TimeZone);

            if (request.OwnerUserId.HasValue)
            {
                claimAggregate.AssignToOwner(
                    request.OwnerUserId.Value,
                    this.httpContextPropertiesResolver.PerformingUserId,
                    this.clock.GetCurrentInstant());
            }

            await this.claimAggregateRepository.Save(claimAggregate);
            return await Task.FromResult(claimAggregate.Id);
        }
    }
}

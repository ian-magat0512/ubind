// <copyright file="ClaimService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Aggregates.Claim.Workflow;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReferenceNumbers;

    /// <inheritdoc/>
    public class ClaimService : IClaimService
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IClaimWorkflowProvider claimWorkflowProvider;
        private readonly IClaimReadModelRepository claimReadModelRepository;
        private readonly IClaimAggregateRepository claimAggregateRepository;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly ICustomerAggregateRepository customerAggregateRepository;
        private readonly IPersonAggregateRepository personAggregateRepository;
        private readonly IClaimNumberRepository claimNumberRepository;
        private readonly IClock clock;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly ISystemAlertService systemAlertService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimService"/> class.
        /// </summary>
        /// <param name="claimWorkflowProvider">The Claim workflow provider.</param>
        /// <param name="claimAggregateRepository">The claim aggregate repository.</param>
        /// <param name="claimReadModelRepository">The claim read model repository.</param>
        /// <param name="quoteAggregateRepository">The quote aggregate repository.</param>
        /// <param name="customerAggregateRepository">The customer aggregate repository.</param>
        /// <param name="personAggregateRepository">The person aggregate repository.</param>
        /// <param name="claimNumberRepository">The claim number repository.</param>
        /// <param name="httpContextPropertiesResolver">The User information.</param>
        /// <param name="systemAlertService">The system alert service.</param>
        /// <param name="clock">A clock for obtaining current time.</param>
        public ClaimService(
            IClaimWorkflowProvider claimWorkflowProvider,
            IClaimAggregateRepository claimAggregateRepository,
            IClaimReadModelRepository claimReadModelRepository,
            IQuoteAggregateRepository quoteAggregateRepository,
            ICustomerAggregateRepository customerAggregateRepository,
            IPersonAggregateRepository personAggregateRepository,
            IClaimNumberRepository claimNumberRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            ISystemAlertService systemAlertService,
            ICachingResolver cachingResolver,
            IClock clock)
        {
            this.cachingResolver = cachingResolver;
            this.claimWorkflowProvider = claimWorkflowProvider;
            this.claimAggregateRepository = claimAggregateRepository;
            this.claimReadModelRepository = claimReadModelRepository;
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.customerAggregateRepository = customerAggregateRepository;
            this.personAggregateRepository = personAggregateRepository;
            this.claimNumberRepository = claimNumberRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.systemAlertService = systemAlertService;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public IEnumerable<IClaimReadModelSummary> GetClaims(
            Guid tenantId, EntityListFilters filters)
        {
            return this.claimReadModelRepository.ListClaims(
                tenantId,
                filters);
        }

        /// <inheritdoc/>
        public bool HasClaimsFromPolicy(Guid tenantId, Guid policyId)
        {
            return this.claimReadModelRepository.HasClaimsByPolicyId(tenantId, policyId);
        }

        /// <inheritdoc />
        public bool HasClaimsForCustomer(EntityListFilters filters, IEnumerable<Guid> excludedClaimIds)
        {
            return this.claimReadModelRepository.HasClaimsForCustomer(filters, excludedClaimIds);
        }

        /// <inheritdoc/>
        public IEnumerable<ClaimReadModel> GetClaimsFromPolicy(Guid tenantId, Guid policyId)
        {
            return this.claimReadModelRepository.ListClaimsByPolicyWithoutJoiningProducts(tenantId, policyId);
        }

        /// <inheritdoc/>
        public async Task<ClaimAggregate> ChangeClaimState(ReleaseContext releaseContext, ClaimAggregate claimAggregate, ClaimActions operation, string? formDataJson)
        {
            if (formDataJson != null)
            {
                claimAggregate.UpdateFormData(formDataJson, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
            }

            if (operation == ClaimActions.Actualise)
            {
                claimAggregate.Actualise(this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
            }

            var claimWorkflow
                = await this.claimWorkflowProvider.GetConfigurableClaimWorkflow(releaseContext);
            claimAggregate.ChangeClaimState(
                operation, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now(), claimWorkflow);
            return claimAggregate;
        }

        /// <inheritdoc/>
        public async Task<ClaimAggregate> AssignClaimNumber(
            Guid tenantId,
            Guid claimId,
            string newClaimNumber,
            DeploymentEnvironment environment,
            bool isRestoreToList = false)
        {
            ClaimAggregate claimAggregate = this.claimAggregateRepository.GetById(tenantId, claimId);
            string oldClaimNumber = claimAggregate.Claim.ClaimNumber;
            if (this.claimReadModelRepository.IsClaimNumberInUseByOtherClaim(
                       claimAggregate.TenantId, claimAggregate.ProductId, newClaimNumber)
                && !string.IsNullOrEmpty(newClaimNumber))
            {
                throw new InvalidOperationException(
                    $"The claim number {newClaimNumber} is already assigned to a different claim.");
            }

            if (string.IsNullOrWhiteSpace(newClaimNumber))
            {
                newClaimNumber = this.claimNumberRepository.ConsumeForProduct(
                    claimAggregate.TenantId,
                    claimAggregate.ProductId,
                    environment);

                await this.systemAlertService.QueueClaimNumberThresholdAlertCheck(
                    claimAggregate.TenantId,
                    claimAggregate.ProductId,
                    environment);
            }
            else
            {
                newClaimNumber = this.claimNumberRepository.AssignClaimNumber(
                    claimAggregate.TenantId,
                    claimAggregate.ProductId,
                    claimAggregate.Claim.ClaimNumber,
                    newClaimNumber,
                    environment);
            }

            if (isRestoreToList)
            {
                if (!string.IsNullOrEmpty(oldClaimNumber))
                {
                    this.claimNumberRepository.ReuseOldClaimNumber(
                        claimAggregate.TenantId,
                        claimAggregate.ProductId,
                        oldClaimNumber,
                        environment);
                }
                else
                {
                    throw new InvalidOperationException($"Cannot reuse claim number with no value.");
                }
            }

            claimAggregate
                .AssignClaimNumber(newClaimNumber, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
            await this.claimAggregateRepository.Save(claimAggregate);
            return claimAggregate;
        }

        /// <inheritdoc/>
        public async Task<ClaimAggregate> UnassignClaimNumber(
            Guid tenantId, Guid claimId, DeploymentEnvironment environment, bool isRestoreToList)
        {
            ClaimAggregate claimAggregate = this.claimAggregateRepository.GetById(tenantId, claimId);
            string oldClaimNumber = claimAggregate.Claim.ClaimNumber;
            this.claimNumberRepository.UnassignClaimNumber(
                tenantId, claimAggregate.ProductId, claimAggregate.Claim.ClaimNumber, environment, isRestoreToList);

            if (isRestoreToList)
            {
                if (!string.IsNullOrEmpty(oldClaimNumber))
                {
                    this.claimNumberRepository.ReuseOldClaimNumber(
                        claimAggregate.TenantId,
                        claimAggregate.ProductId,
                        oldClaimNumber,
                        environment);
                }
                else
                {
                    throw new InvalidOperationException($"Cannot reuse claim number with no value.");
                }
            }

            claimAggregate.UnassignClaimNumber(this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
            await this.claimAggregateRepository.Save(claimAggregate);
            return claimAggregate;
        }

        /// <inheritdoc/>
        public async Task<ClaimAggregate> UpdateFormData(Guid tenantId, Guid claimId, string formDataJson)
        {
            var claimAggregate = this.claimAggregateRepository.GetById(tenantId, claimId);
            if (!string.IsNullOrEmpty(formDataJson))
            {
                claimAggregate
                    .UpdateFormData(formDataJson, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
            }

            await this.claimAggregateRepository.Save(claimAggregate);
            return claimAggregate;
        }

        /// <inheritdoc/>
        public async Task<ClaimAggregate> CreateVersion(Guid tenantId, Guid claimId, string formDataJson)
        {
            ClaimAggregate claimAggregate = this.claimAggregateRepository.GetById(tenantId, claimId);
            if (claimAggregate.Claim.PolicyId.HasValue)
            {
                var quoteAggregate = this.quoteAggregateRepository.GetById(tenantId, claimAggregate.Claim.PolicyId.Value);
            }

            async Task<ClaimAggregate> CreateAndSaveVersion()
            {
                claimAggregate = this.claimAggregateRepository.GetById(tenantId, claimId);
                var timestamp = this.clock.Now();
                claimAggregate.UpdateFormData(formDataJson, this.httpContextPropertiesResolver.PerformingUserId, timestamp);
                claimAggregate.CreateVersion(this.httpContextPropertiesResolver.PerformingUserId, timestamp);
                await this.claimAggregateRepository.Save(claimAggregate);
                return claimAggregate;
            }

            var returnClaimAggregate = await ConcurrencyPolicy.ExecuteWithRetriesAsync(CreateAndSaveVersion);
            return returnClaimAggregate;
        }

        /// <inheritdoc/>
        public async Task<ClaimAggregate> AssociateClaimWithPolicyAsync(Guid tenantId, Guid policyId, Guid claimId)
        {
            await this.ThrowIfClaimAlreadyAssociated(tenantId, policyId, claimId);
            var quoteAggregate = this.quoteAggregateRepository.GetById(tenantId, policyId);
            var claimAggregate = this.claimAggregateRepository.GetById(tenantId, claimId);
            PersonAggregate personAggregate = null;
            if (quoteAggregate.CustomerId != null && quoteAggregate.CustomerId.Value != default)
            {
                var customerAggregate = this.customerAggregateRepository.GetById(tenantId, quoteAggregate.CustomerId.Value);
                if (customerAggregate == null)
                {
                    throw new NotFoundException(
                        Errors.General.NotFound("customer", quoteAggregate.CustomerId));
                }

                personAggregate = this.personAggregateRepository.GetById(customerAggregate.TenantId, customerAggregate.PrimaryPersonId);
            }

            async Task<ClaimAggregate> AssociateClaimWithPolicy()
            {
                claimAggregate.AssociateClaimWithPolicy(
                    claimId,
                    quoteAggregate,
                    personAggregate,
                    this.httpContextPropertiesResolver.PerformingUserId,
                    this.clock.Now());
                await this.claimAggregateRepository.Save(claimAggregate);
                return claimAggregate;
            }

            return await ConcurrencyPolicy.ExecuteWithRetriesAsync(
                AssociateClaimWithPolicy,
                () => claimAggregate = this.claimAggregateRepository.GetById(tenantId, claimId));
        }

        /// <inheritdoc/>
        public async Task<ClaimAggregate?> DisassociateClaimWithPolicyAsync(Guid tenantId, Guid claimId, Guid policyId)
        {
            var claimAggregate = this.claimAggregateRepository.GetById(tenantId, claimId);

            async Task<ClaimAggregate?> DisassociateClaimWithPolicy()
            {
                if (claimAggregate == null)
                {
                    return claimAggregate;
                }

                claimAggregate.DisassociateClaimWithPolicy(
                    claimId,
                    policyId,
                    this.httpContextPropertiesResolver.PerformingUserId,
                    this.clock.Now());
                await this.claimAggregateRepository.ApplyChangesToDbContext(claimAggregate);
                return claimAggregate;
            }

            return await ConcurrencyPolicy.ExecuteWithRetriesAsync(
                DisassociateClaimWithPolicy,
                () => claimAggregate = this.claimAggregateRepository.GetById(tenantId, claimId));
        }

        /// <inheritdoc/>
        public IFileContentReadModel GetClaimDocumentContent(Guid tenantId, Guid documentId, Guid claimId)
        {
            var claim = this.claimReadModelRepository.GetSummaryById(tenantId, claimId);
            if (claim == null)
            {
                throw new ErrorException(Errors.General.NotFound("claim", claimId));
            }

            var aggregate = this.claimAggregateRepository.GetById(tenantId, claimId);
            if (aggregate == null)
            {
                throw new ErrorException(Errors.General.NotFound("claim aggregate", claimId, "aggregate ID"));
            }

            var document = claim.Documents.Where(d => d.Id == documentId).FirstOrDefault();
            IFileContentReadModel content;
            if (document != null)
            {
                var claimOrClaimVersionId = document.ClaimOrClaimVersionId;
                content = this.claimReadModelRepository.GetDocumentContent(tenantId, documentId, claimOrClaimVersionId);
            }
            else
            {
                content = this.claimReadModelRepository.GetDocumentContent(tenantId, documentId, claimId);
            }

            if (content == null)
            {
                throw new ErrorException(Errors.General.NotFound("claim document", documentId));
            }

            return content;
        }

        private async Task ThrowIfClaimAlreadyAssociated(Guid tenantId, Guid policyId, Guid claimId)
        {
            var claimAggregate = this.claimAggregateRepository.GetById(tenantId, claimId);
            var policy = this.quoteAggregateRepository.GetById(tenantId, policyId);
            if (policy == null)
            {
                throw new ErrorException(Errors.General.NotFound("Policy", policyId));
            }

            if (claimAggregate.PolicyId == policyId)
            {
                var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(claimAggregate.TenantId);
                var isMutual = TenantHelper.IsMutual(tenantAlias);
                throw new ErrorException(Errors.Claim.AlreadyAssociated(claimId, policyId, isMutual));
            }
        }
    }
}

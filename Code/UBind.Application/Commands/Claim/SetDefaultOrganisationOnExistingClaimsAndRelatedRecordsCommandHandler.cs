// <copyright file="SetDefaultOrganisationOnExistingClaimsAndRelatedRecordsCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Claim
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Command handler for generating default organisation to existing claims and its related records based from tenancy.
    /// </summary>
    public class SetDefaultOrganisationOnExistingClaimsAndRelatedRecordsCommandHandler
        : ICommandHandler<SetDefaultOrganisationOnExistingClaimsAndRelatedRecordsCommand, Unit>
    {
        private readonly ILogger<SetDefaultOrganisationOnExistingClaimsAndRelatedRecordsCommandHandler> logger;
        private readonly ITenantRepository tenantRepository;
        private readonly IClaimReadModelRepository claimReadModelRepository;
        private readonly IClaimAggregateRepository claimAggregateRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetDefaultOrganisationOnExistingClaimsAndRelatedRecordsCommandHandler"/> class.
        /// </summary>
        /// <param name="tenantRepository">The repository for tenants.</param>
        /// <param name="claimReadModelRepository">The repository for claim read models.</param>
        /// <param name="claimAggregateRepository">The repository for claim aggregate repository.</param>
        /// <param name="httpContextPropertiesResolver">The resolver for performing user.</param>
        /// <param name="clock">Represents the clock which can return the time as <see cref="Instant"/>.</param>
        public SetDefaultOrganisationOnExistingClaimsAndRelatedRecordsCommandHandler(
            ITenantRepository tenantRepository,
            IClaimReadModelRepository claimReadModelRepository,
            IClaimAggregateRepository claimAggregateRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            ILogger<SetDefaultOrganisationOnExistingClaimsAndRelatedRecordsCommandHandler> logger,
            IClock clock)
        {
            this.logger = logger;
            this.tenantRepository = tenantRepository;
            this.claimReadModelRepository = claimReadModelRepository;
            this.claimAggregateRepository = claimAggregateRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(
            SetDefaultOrganisationOnExistingClaimsAndRelatedRecordsCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var tenants = this.tenantRepository.GetTenants();
            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;

            foreach (var tenant in tenants)
            {
                this.logger.LogInformation($"Set Default Organisation for Claims of tenant {tenant.Id}");

                var claims = this.claimReadModelRepository.GetAllClaimsAsQueryable()
                    .Where(x => x.TenantId == tenant.Id).ToList();

                foreach (var claim in claims)
                {
                    int retryTimes = 0;
                    async Task AssignOrganisationIdToClaim()
                    {
                        try
                        {
                            var claimAggregate = this.claimAggregateRepository.GetById(claim.TenantId, claim.Id);

                            if (claimAggregate != null && (claimAggregate?.OrganisationId == default || claim.OrganisationId == default))
                            {
                                var organisationId = claim.OrganisationId;
                                if (organisationId == Guid.Empty)
                                {
                                    organisationId = tenant.Details.DefaultOrganisationId;
                                }

                                var currentTimestamp = this.clock.GetCurrentInstant();

                                claimAggregate.RecordOrganisationMigration(organisationId, performingUserId, currentTimestamp);
                                this.logger.LogInformation($"Set Default Organisation to Claim {claim.Id} for tenant {tenant.Id}, retryTimes = " + retryTimes);
                                await this.claimAggregateRepository.Save(claimAggregate);
                            }
                        }
                        catch (Exception e)
                        {
                            this.logger.LogError($"ERROR: Claim {claim.Id} for tenant {tenant.Id}, retryTimes = {retryTimes}, errorMessage: {e.Message}-{e.InnerException?.Message}");
                            throw;
                        }

                        await Task.Delay(300, cancellationToken);
                    }

                    await RetryPolicyHelper.ExecuteAsync<Exception>(() => AssignOrganisationIdToClaim(), maxJitter: 1500);
                }
            }

            return Unit.Value;
        }
    }
}

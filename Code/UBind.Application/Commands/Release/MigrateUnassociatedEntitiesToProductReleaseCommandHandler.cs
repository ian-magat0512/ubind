// <copyright file="MigrateUnassociatedEntitiesToProductReleaseCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Release;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NodaTime;
using UBind.Domain.Exceptions;
using UBind.Domain;
using UBind.Domain.Patterns.Cqrs;
using Errors = Domain.Errors;
using Hangfire;
using UBind.Domain.Aggregates.Quote;
using Microsoft.Extensions.Logging;
using Release = Domain.Release;
using UBind.Domain.ReadModel;

public class MigrateUnassociatedEntitiesToProductReleaseCommandHandler
    : ICommandHandler<MigrateUnassociatedEntitiesToProductReleaseCommand>
{
    private readonly IBackgroundJobClient backgroundJobClient;
    private readonly IQuoteAggregateRepository quoteAggregateRepository;
    private readonly IReleaseRepository releaseRepository;
    private readonly ILogger<MigrateUnassociatedEntitiesToProductReleaseCommandHandler> logger;
    private readonly IClock clock;
    private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
    private readonly IQuoteReadModelRepository quoteReadModelRepository;
    private readonly IPolicyReadModelRepository policyReadModelRepository;

    public MigrateUnassociatedEntitiesToProductReleaseCommandHandler(
        IBackgroundJobClient backgroundJobClient,
        ILogger<MigrateUnassociatedEntitiesToProductReleaseCommandHandler> logger,
        IClock clock,
        IHttpContextPropertiesResolver httpContextPropertiesResolver,
        ICachingResolver cachingResolver,
        IQuoteReadModelRepository quoteReadModelRepository,
        IPolicyReadModelRepository policyReadModelRepository,
        IQuoteAggregateRepository quoteAggregateRepository,
        IReleaseRepository releaseRepository)
    {
        this.backgroundJobClient = backgroundJobClient;
        this.logger = logger;
        this.clock = clock;
        this.httpContextPropertiesResolver = httpContextPropertiesResolver;
        this.quoteReadModelRepository = quoteReadModelRepository;
        this.policyReadModelRepository = policyReadModelRepository;
        this.quoteAggregateRepository = quoteAggregateRepository;
        this.releaseRepository = releaseRepository;
    }

    public Task<Unit> Handle(
        MigrateUnassociatedEntitiesToProductReleaseCommand command, CancellationToken cancellationToken)
    {
        this.backgroundJobClient.Enqueue<MigrateUnassociatedEntitiesToProductReleaseCommandHandler>(handler =>
            handler.MigrateUnassociatedEntitiesToProductRelease(
                command.TenantId, command.ProductId, command.NewReleaseId, command.Environment));
        return Task.FromResult(Unit.Value);
    }

    public async Task MigrateUnassociatedEntitiesToProductRelease(Guid tenantId, Guid productId, Guid newReleaseId, DeploymentEnvironment environment)
    {
        Release newRelease = this.GetReleaseOrThrow(tenantId, newReleaseId);

        var quoteAggregateIds = this.quoteReadModelRepository.GetUnassociatedQuoteAggregateIds(tenantId, productId, environment);
        var policyAggregateIds = this.policyReadModelRepository.GetUnassociatedPolicyTransactionAggregateIds(tenantId, productId, environment);
        var aggregateIds = quoteAggregateIds.Concat(policyAggregateIds).Distinct();

        this.logger.LogInformation($"Start migrating unassociated quotes and policy transactions "
                        + $"to new release {newRelease.Number}.{newRelease.MinorNumber}. Total QuoteAggregates: {aggregateIds.Count()}.");
        var currentInstant = this.clock.GetCurrentInstant();
        foreach (var aggregateId in aggregateIds)
        {
            var quoteAggregate = this.quoteAggregateRepository.GetById(tenantId, aggregateId);
            this.logger.LogInformation($"Migrating QuoteAggregate {aggregateId} to new release {newRelease.Number}.{newRelease.MinorNumber}.");
            quoteAggregate.MigrateUnassociatedEntitiesToProductRelease(
                productId, newRelease.Id, this.httpContextPropertiesResolver.PerformingUserId, currentInstant);
            await this.quoteAggregateRepository.Save(quoteAggregate);
            this.logger.LogInformation($"Migrated QuoteAggregate {aggregateId} to new release {newRelease.Number}.{newRelease.MinorNumber}.");

            await Task.Delay(200);
        }
        this.logger.LogInformation($"Done migrating quotes and policy transactions to new release.");
    }

    private Release GetReleaseOrThrow(Guid tenantId, Guid releaseId)
    {
        var release = this.releaseRepository.GetReleaseWithoutAssetFileContents(tenantId, releaseId);
        if (release == null)
        {
            throw new ErrorException(Errors.General.NotFound("release", releaseId));
        }

        return release;
    }
}

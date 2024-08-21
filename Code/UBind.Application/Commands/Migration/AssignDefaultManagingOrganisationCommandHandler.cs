// <copyright file="AssignDefaultManagingOrganisationCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NodaTime;
using UBind.Domain.Aggregates.Organisation;
using UBind.Domain.Extensions;
using UBind.Domain.Helpers;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ReadModel;
using UBind.Domain.Repositories;

public class AssignDefaultManagingOrganisationCommandHandler :
    ICommandHandler<AssignDefaultManagingOrganisationCommand, Unit>
{
    private readonly IOrganisationAggregateRepository organisationAggregateRepository;
    private readonly IOrganisationReadModelRepository organisationRepository;
    private readonly ITenantRepository tenantRepository;
    private readonly ILogger<AssignDefaultManagingOrganisationCommandHandler> logger;
    private readonly IClock clock;

    public AssignDefaultManagingOrganisationCommandHandler(
        IOrganisationReadModelRepository organisationRepository,
        IOrganisationAggregateRepository organisationAggregateRepository,
        ITenantRepository tenantRepository,
        ILogger<AssignDefaultManagingOrganisationCommandHandler> logger,
        IClock clock)
    {
        this.organisationAggregateRepository = organisationAggregateRepository;
        this.organisationRepository = organisationRepository;
        this.tenantRepository = tenantRepository;
        this.logger = logger;
        this.clock = clock;
    }

    /// <inheritdoc/>
    public async Task<Unit> Handle(AssignDefaultManagingOrganisationCommand request, CancellationToken cancellationToken)
    {
        var availableTenants = this.tenantRepository.GetActiveTenants();
        foreach (var tenant in availableTenants)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (tenant.IsMasterTenant)
            {
                // master tenant does not have organisations
                continue;
            }
            var defaultOrganisationId = tenant.Details.DefaultOrganisationId;
            var subOrganisation = this.organisationRepository.GetOrganisations(tenant.Id)
                .Where(org => org.Id != defaultOrganisationId).ToList();
            this.logger.LogInformation($"Assigning default organisation {defaultOrganisationId} as managing organisation for sub-organisations under tenant {tenant.Id}.");

            foreach (var organisation in subOrganisation)
            {
                async Task AssignManagingOrganisation()
                {
                    var aggregate = this.organisationAggregateRepository.GetById(tenant.Id, organisation.Id);
                    try
                    {
                        if (aggregate != null && aggregate.ManagingOrganisationId == null)
                        {
                            aggregate.SetManagingOrganisation(defaultOrganisationId, null, this.clock.Now());
                            await this.organisationAggregateRepository.Save(aggregate);
                        }
                    }
                    catch (Exception e)
                    {
                        this.logger.LogError($"Error: Organisation {aggregate?.Alias} cannot be assigned with managing organisation.  Error = {e.Message}");
                        throw;
                    }

                    await Task.Delay(300);
                }

                await RetryPolicyHelper.ExecuteAsync<Exception>(() => AssignManagingOrganisation(), 3);
            }
        }

        return Unit.Value;
    }
}

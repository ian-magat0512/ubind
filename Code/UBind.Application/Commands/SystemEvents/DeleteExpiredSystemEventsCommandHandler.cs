// <copyright file="DeleteExpiredSystemEventsCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.SystemEvents;

using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using UBind.Application.StartupJobs;
using UBind.Domain;
using UBind.Domain.Exceptions;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Services;

/// <summary>
/// Handler for deleting expired system events.
/// The deletion is done by batch and recurred
/// depending on the scheduler configuration.
/// See <see cref="SystemEventDeletionScheduler"/> class
/// for the scheduler configuration.
/// </summary>
public class DeleteExpiredSystemEventsCommandHandler : ICommandHandler<DeleteExpiredSystemEventsCommand, Unit>
{
    private const int BatchSize = 5;
    private readonly IStartupJobRepository startupJobRepository;
    private readonly ILogger<DeleteExpiredSystemEventsCommandHandler> logger;
    private readonly ISystemEventDeletionService systemEventDeletionService;

    public DeleteExpiredSystemEventsCommandHandler(
            ILogger<DeleteExpiredSystemEventsCommandHandler> logger,
            IStartupJobRepository startupJobRepository,
            ISystemEventDeletionService systemEventDeletionService)
    {
        this.logger = logger;
        this.startupJobRepository = startupJobRepository;
        this.systemEventDeletionService = systemEventDeletionService;
    }

    public async Task<Unit> Handle(DeleteExpiredSystemEventsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var startupJobAlias = nameof(StartupJobRegistry.UpdateSystemEventsExpiryTimeStamp_20230901);
            var startupJob = this.startupJobRepository
                .GetStartupJobByAlias(startupJobAlias);
            if (startupJob == null)
            {
                throw new ErrorException(Errors.General.NotFound("startup job", startupJobAlias, "alias"));
            }

            if (!startupJob.Complete)
            {
                this.logger.LogInformation("Stopping this operation as system events update job" +
                    $" with an alias of {startupJobAlias} has not yet completed." +
                    " Since this is a recurring job, there is no need to retry.");
            }
            else
            {
                this.logger.LogInformation("Starting to execute system event records deletion by batch size.");
                await this.systemEventDeletionService.ExecuteDeletionInBatches(BatchSize, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            this.logger.LogInformation("Operation was cancelled.");
            throw;
        }

        return Unit.Value;
    }
}

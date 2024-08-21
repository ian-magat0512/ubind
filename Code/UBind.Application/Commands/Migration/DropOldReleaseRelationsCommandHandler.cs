// <copyright file="DropOldReleaseRelationsCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration;

using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using UBind.Domain.Helpers;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Repositories;

public class DropOldReleaseRelationsCommandHandler : ICommandHandler<DropOldReleaseRelationsCommand, Unit>
{
    private readonly IUBindDbContext dbContext;
    private readonly ILogger<RestructureReleaseDataCommandHandler> logger;

    public DropOldReleaseRelationsCommandHandler(
        IUBindDbContext dbContext,
        ILogger<RestructureReleaseDataCommandHandler> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public async Task<Unit> Handle(DropOldReleaseRelationsCommand request, CancellationToken cancellationToken)
    {
        this.logger.LogInformation($"Dropping index dbo.ReleaseDescriptions.Release_Id");
        await this.dbContext.Database.ExecuteSqlCommandAsync(SqlHelper.DropIndexIfExists("dbo.ReleaseDescriptions", "Release_Id"));

        this.logger.LogInformation($"Dropping index dbo.ReleaseDescriptions.DevRelease_Id");
        await this.dbContext.Database.ExecuteSqlCommandAsync(SqlHelper.DropIndexIfExists("dbo.ReleaseDescriptions", "DevRelease_Id"));

        this.logger.LogInformation($"Dropping index dbo.ReleaseEvents.ClaimDetails_Id");
        await this.dbContext.Database.ExecuteSqlCommandAsync(SqlHelper.DropIndexIfExists("dbo.ReleaseEvents", "ClaimDetails_Id"));

        this.logger.LogInformation($"Dropping index dbo.ReleaseEvents.QuoteDetails_Id");
        await this.dbContext.Database.ExecuteSqlCommandAsync(SqlHelper.DropIndexIfExists("dbo.ReleaseEvents", "QuoteDetails_Id"));

        this.logger.LogInformation($"Dropping index dbo.ReleaseEvents.Release_Id");
        await this.dbContext.Database.ExecuteSqlCommandAsync(SqlHelper.DropIndexIfExists("dbo.ReleaseEvents", "Release_Id"));

        this.logger.LogInformation($"Dropping index dbo.ReleaseEvents.DevRelease_Id");
        await this.dbContext.Database.ExecuteSqlCommandAsync(SqlHelper.DropIndexIfExists("dbo.ReleaseEvents", "DevRelease_Id"));

        this.logger.LogInformation($"Dropping column dbo.ReleaseDetails.SpreadsheetPath");
        await this.dbContext.Database.ExecuteSqlCommandAsync(SqlHelper.DropColumnIfExists("dbo.ReleaseDetails", "SpreadsheetPath"));

        this.logger.LogInformation($"Dropping column dbo.ReleaseDetails.OutboundEmailServersJson");
        await this.dbContext.Database.ExecuteSqlCommandAsync(SqlHelper.DropColumnIfExists("dbo.ReleaseDetails", "OutboundEmailServersJson"));

        this.logger.LogInformation($"Dropping column dbo.ReleaseDetails.OutboundEmailServersJson");
        await this.dbContext.Database.ExecuteSqlCommandAsync(SqlHelper.DropColumnIfExists("dbo.DevReleases", "DevFilesHashString"));

        this.logger.LogInformation($"Dropping table dbo.ReleaseDescriptions");
        await this.dbContext.Database.ExecuteSqlCommandAsync(SqlHelper.DropTableIfExists("dbo.ReleaseDescriptions"));

        this.logger.LogInformation($"Dropping table dbo.ReleaseEvents");
        await this.dbContext.Database.ExecuteSqlCommandAsync(SqlHelper.DropTableIfExists("dbo.ReleaseEvents"));

        return Unit.Value;
    }
}

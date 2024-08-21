// <copyright file="AddPasswordToUserLoginEmailsCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration;

using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using UBind.Domain.Aggregates.User;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Repositories;

public class AddPasswordToUserLoginEmailsCommandHandler : ICommandHandler<AddPasswordToUserLoginEmailsCommand, Unit>
{
    private readonly IUBindDbContext dbContext;
    private readonly IUserAggregateRepository userAggregateRepository;
    private readonly ITenantRepository tenantRepository;
    private readonly ILogger<AddPasswordToUserLoginEmailsCommandHandler> logger;

    public AddPasswordToUserLoginEmailsCommandHandler(
        IUBindDbContext dbContext,
        IUserAggregateRepository userAggregateRepository,
        ITenantRepository tenantRepository,
        ILogger<AddPasswordToUserLoginEmailsCommandHandler> logger)
    {
        this.dbContext = dbContext;
        this.userAggregateRepository = userAggregateRepository;
        this.tenantRepository = tenantRepository;
        this.logger = logger;
    }

    public async Task<Unit> Handle(AddPasswordToUserLoginEmailsCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var tenants = this.tenantRepository.GetActiveTenants().ToList();
        foreach (var tenant in tenants)
        {
            int processed = 0;
            var userIds = this.dbContext.UserLoginEmails
                .Where(u => u.TenantId == tenant.Id && u.SaltedHashedPassword == null)
                .Select(u => u.Id).ToList();
            foreach (var userId in userIds)
            {
                var userAggregate = this.userAggregateRepository.GetById(tenant.Id, userId);
                if (userAggregate == null)
                {
                    this.logger.LogError(
                        $"Could not find a user aggregate with id {userId} for tenant {tenant.Details.Alias}. Skipping");
                    continue;
                }

                var userLogin = this.dbContext.UserLoginEmails.FirstOrDefault(ul => ul.TenantId == tenant.Id && ul.Id == userId);
                if (userLogin == null)
                {
                    // this is unexpected, but we won't stop the migration, we'll just log it and continue
                    this.logger.LogError(
                        $"Could not find a user login with id {userId} for tenant {tenant.Details.Alias} even when a "
                        + "user aggregate with the same ID was found. This is a data inconsistency which should be "
                        + "investigated.");
                    continue;
                }

                userLogin.SaltedHashedPassword = userAggregate.CurrentSaltedHashedPassword;
                processed++;
                if (processed % 50 == 0)
                {
                    this.dbContext.SaveChanges();
                    this.logger.LogInformation($"Processed {processed}/{userIds.Count} users for tenant {tenant.Details.Alias}");
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Delay(1000, cancellationToken);
                }
            }

            this.logger.LogInformation($"Finished processing tenant {tenant.Details.Alias}, with a total of {processed} users");
        }

        this.dbContext.SaveChanges();
        this.logger.LogInformation($"Finished processing all tenants.");
        return Unit.Value;
    }
}

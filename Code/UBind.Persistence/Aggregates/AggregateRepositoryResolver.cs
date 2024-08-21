// <copyright file="AggregateRepositoryResolver.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Aggregates;

using System;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using UBind.Domain;
using UBind.Domain.Aggregates;
using UBind.Domain.Aggregates.Claim;
using UBind.Domain.Aggregates.Customer;
using UBind.Domain.Aggregates.Organisation;
using UBind.Domain.Aggregates.Person;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Aggregates.Report;
using UBind.Domain.Aggregates.User;
using UBind.Domain.Exceptions;

public class AggregateRepositoryResolver : IAggregateRepositoryResolver
{
    private readonly IServiceProvider serviceProvider;
    private readonly Dictionary<AggregateEntityType, Type> repositoryMappings = new Dictionary<AggregateEntityType, Type>
    {
        { AggregateEntityType.Claim, typeof(IClaimAggregateRepository) },
        { AggregateEntityType.Customer, typeof(ICustomerAggregateRepository) },
        { AggregateEntityType.Organisation, typeof(IOrganisationAggregateRepository) },
        { AggregateEntityType.Person, typeof(IPersonAggregateRepository) },
        { AggregateEntityType.Quote, typeof(IQuoteAggregateRepository) },
        { AggregateEntityType.Policy, typeof(IQuoteAggregateRepository) },
        { AggregateEntityType.Report, typeof(IReportAggregateRepository) },
        { AggregateEntityType.User, typeof(IUserAggregateRepository) },
    };

    public AggregateRepositoryResolver(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public IReplayableAggregateRepository<Guid> Resolve(AggregateEntityType entityType)
    {
        if (this.repositoryMappings.TryGetValue(entityType, out var repositoryType))
        {
            return (IReplayableAggregateRepository<Guid>)this.serviceProvider.GetRequiredService(repositoryType);
        }

        string typeName = entityType.Humanize(LetterCasing.Title);
        throw new ErrorException(Errors.General.NotFound("AggregateRepository", typeName, "type"));
    }
}

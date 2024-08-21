// <copyright file="GetAllNumbersFromPoolQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Number;

using System.Security.Claims;
using UBind.Domain;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ReadModel;

/// <summary>
/// Query for getting the numbers from the pool.
/// </summary>
public class GetAllNumbersFromPoolQuery : IQuery<NumberPoolSummaryModel>
{
    public GetAllNumbersFromPoolQuery(
        ClaimsPrincipal user,
        string tenant,
        string product,
        string numberPoolName,
        DeploymentEnvironment environment)
    {
        this.User = user;
        this.Tenant = tenant;
        this.Product = product;
        this.NumberPoolName = numberPoolName;
        this.Environment = environment;
    }

    public ClaimsPrincipal User { get; }

    public string Tenant { get; }

    public string Product { get; }

    /// <summary>
    /// The name of the number pool. (e.g. "Invoice, Policy, CreditNote, Claim")
    /// </summary>
    public string NumberPoolName { get; }

    public DeploymentEnvironment Environment { get; }
}

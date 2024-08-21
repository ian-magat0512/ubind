// <copyright file="ThirdPartyDataSetsDbaRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence;

using Microsoft.Extensions.Logging;
using System.Data.Entity;
using UBind.Domain.Repositories;
using UBind.Persistence.ThirdPartyDataSets;

/// <summary>
/// DBA Repository for ThirdPartyDatasets.
/// </summary>
public class ThirdPartyDataSetsDbaRepository : BaseDbaRepository, IDbaRepository<ThirdPartyDataSetsDbContext>
{
    public ThirdPartyDataSetsDbaRepository(IServiceProvider provider, ILogger<ThirdPartyDataSetsDbaRepository> logger, ThirdPartyDataSetsDbContext dbContext)
       : base(dbContext as DbContext, provider, logger)
    {
    }
}
// <copyright file="UBindDbaRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence;

using Microsoft.Extensions.Logging;
using System;
using System.Data.Entity;
using UBind.Domain.Repositories;

public class UBindDbaRepository : BaseDbaRepository, IDbaRepository<UBindDbContext>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseDbaRepository"/> class.
    /// </summary>
    /// <param name="provider">ServiceProvider parameter.</param>
    /// /// <param name="logger">Logging parameter.</param>
    public UBindDbaRepository(IServiceProvider provider, ILogger<UBindDbaRepository> logger, IUBindDbContext ubindDbContext)
        : base(ubindDbContext as DbContext, provider, logger)
    {
    }
}
// <copyright file="DvaDbaRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence;
using System;
using System.Data.Entity;
using Microsoft.Extensions.Logging;
using UBind.Domain.Repositories;
using UBind.Persistence.Clients.DVA.Migrations;

public class DvaDbaRepository : BaseDbaRepository, IDbaRepository<DvaDbContext>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DvaDbaRepository"/> class.
    /// </summary>
    /// <param name="provider">ServiceProvider parameter.</param>
    /// /// <param name="logger">Logging parameter.</param>
    public DvaDbaRepository(IServiceProvider provider, ILogger<DvaDbaRepository> logger, DvaDbContext dvaDbContext)
    : base(dvaDbContext as DbContext, provider, logger)
    {
    }
}

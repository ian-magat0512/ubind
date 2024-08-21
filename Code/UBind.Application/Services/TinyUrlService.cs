// <copyright file="TinyUrlService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services;

using System;
using NodaTime;
using UBind.Domain;
using UBind.Domain.Extensions;
using UBind.Domain.Models;
using UBind.Domain.NumberGenerators;
using UBind.Domain.Repositories;

public class TinyUrlService : ITinyUrlService
{
    public const string TinyUrlRoute = "t";
    private readonly IUrlTokenGenerator urlTokenGenerator;
    private readonly ITinyUrlRepository tinyUrlRepository;
    private readonly IClock clock;
    private readonly IBaseUrlResolver baseUrlResolver;
    private readonly ICachingResolver cachingResolver;

    public TinyUrlService(
        IUrlTokenGenerator urlTokenGenerator,
        ITinyUrlRepository tinyUrlRepository,
        IClock clock,
        IBaseUrlResolver baseUrlResolver,
        ICachingResolver cachingResolver)
    {
        this.urlTokenGenerator = urlTokenGenerator;
        this.tinyUrlRepository = tinyUrlRepository;
        this.clock = clock;
        this.baseUrlResolver = baseUrlResolver;
        this.cachingResolver = cachingResolver;
    }

    /// <inheritdoc/>
    public async Task<string> GenerateAndPersistUrl(Guid tenantId, DeploymentEnvironment environment, string redirectUrl)
    {
        var tenant = await this.cachingResolver.GetTenantOrThrow(tenantId);
        var sequenceNumber = await this.NextSequenceNumber();
        var token = this.urlTokenGenerator.Generate(sequenceNumber);
        var tinyUrl = this.CreateAndPersistTinyUrl(tenant.Id, environment, token, sequenceNumber, redirectUrl);
        return await this.GetFormattedUrl(tinyUrl);
    }

    /// <inheritdoc/>
    public async Task<string?> GetRedirectUrl(string token)
    {
        var url = await this.tinyUrlRepository.GetByToken(token);
        if (url == null)
        {
            return null;
        }

        if (Uri.IsWellFormedUriString(url.RedirectUrl, UriKind.Absolute))
        {
            return url.RedirectUrl;
        }

        var tenant = await this.cachingResolver.GetTenantOrThrow(url.TenantId);
        string appBaseUrl = this.baseUrlResolver.GetBaseUrl(tenant).TrimEnd('/');
        return $"{appBaseUrl}{url.RedirectUrl}";
    }

    private async Task<string> GetFormattedUrl(TinyUrl tinyUrl)
    {
        var tenant = await this.cachingResolver.GetTenantOrThrow(tinyUrl.TenantId);
        string appBaseUrl = this.baseUrlResolver.GetBaseUrl(tenant).TrimEnd('/');
        return $"{appBaseUrl}/{TinyUrlService.TinyUrlRoute}/{tinyUrl.Token}";
    }

    private TinyUrl CreateAndPersistTinyUrl(Guid tenantId, DeploymentEnvironment environment, string token, long sequenceNumber, string redirectUrl)
    {
        var tinyUrl = new TinyUrl(tenantId, environment, redirectUrl, token, sequenceNumber, this.clock.Now());
        this.tinyUrlRepository.Insert(tinyUrl);
        this.tinyUrlRepository.SaveChanges();
        return tinyUrl;
    }

    private async Task<long> NextSequenceNumber()
    {
        var maxNumber = await this.tinyUrlRepository.GetMaxSequenceNumber();
        if (!maxNumber.HasValue)
        {
            return 0;
        }
        return maxNumber.Value + 1;
    }
}

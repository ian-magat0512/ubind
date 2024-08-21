// <copyright file="GetRedirectUrlQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.TinyUrl;

using StackExchange.Profiling;
using System.Threading;
using System.Threading.Tasks;
using UBind.Application.Services;
using UBind.Domain.Patterns.Cqrs;

/// <summary>
/// Handler for <see cref="GetRedirectUrlQuery"/>.
/// </summary>
public class GetRedirectUrlQueryHandler : IQueryHandler<GetRedirectUrlQuery, string?>
{
    private readonly ITinyUrlService tinyUrlService;

    public GetRedirectUrlQueryHandler(ITinyUrlService tinyUrlService)
    {
        this.tinyUrlService = tinyUrlService;
    }

    public async Task<string?> Handle(GetRedirectUrlQuery request, CancellationToken cancellationToken)
    {
        using (MiniProfiler.Current.Step($"{this.GetType().Name}.{nameof(this.Handle)}"))
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await this.tinyUrlService.GetRedirectUrl(request.Token);
        }
    }
}

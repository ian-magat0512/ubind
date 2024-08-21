// <copyright file="DetermineRequestRateLimitCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.RequestRateLimit
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Memory;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;

    public class DetermineRequestRateLimitCommandHandler : ICommandHandler<DetermineRequestRateLimitCommand, RateLimitModel>
    {
        private readonly IRequestRateLimitCache rateLimitCache;

        public DetermineRequestRateLimitCommandHandler(IRequestRateLimitCache rateLimitCache)
        {
            this.rateLimitCache = rateLimitCache;
        }

        /// <inheritdoc/>
        public Task<RateLimitModel> Handle(DetermineRequestRateLimitCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var key = $"{command.ClientIPCode}_{command.Endpoint}";
            var isExist = this.rateLimitCache.MemoryCache.TryGetValue(key, out RateLimitModel rateLimitValue);
            if (!isExist)
            {
                var rateLimit = new RateLimitModel()
                {
                    Period = command.Period,
                    Limit = command.Limit,
                    PeriodTimestamp = DateTime.UtcNow,
                    IsBlocked = false,
                };

                rateLimit.Limit--;
                this.SetRateLimitCache(key, rateLimit, command.PeriodType);

                return Task.FromResult(rateLimit);
            }
            else
            {
                if (rateLimitValue.Limit > 0)
                {
                    rateLimitValue.Period = rateLimitValue.PeriodTimestamp.RemainingPeriod(rateLimitValue.Period);
                    rateLimitValue.IsBlocked = false;

                    if (rateLimitValue.Period > 0)
                    {
                        rateLimitValue.Limit--;
                        rateLimitValue.PeriodTimestamp = DateTime.UtcNow;
                        this.rateLimitCache.MemoryCache.Remove(key);
                        this.SetRateLimitCache(key, rateLimitValue, command.PeriodType);
                    }

                    return Task.FromResult(rateLimitValue);
                }
                else
                {
                    var retryAfter = rateLimitValue.PeriodTimestamp.RetryAfterFrom(rateLimitValue.Period);
                    var rateLimitResponse = new RateLimitModel()
                    {
                        Period = rateLimitValue.Period,
                        Limit = rateLimitValue.Limit,
                        PeriodTimestamp = rateLimitValue.PeriodTimestamp,
                        IsBlocked = true,
                        RetryAfter = retryAfter,
                    };
                    return Task.FromResult(rateLimitResponse);
                }
            }
        }

        private void SetRateLimitCache(string key, RateLimitModel rateLimit, RateLimitPeriodType periodType)
        {
            this.rateLimitCache.MemoryCacheEntryOptions.SetAbsoluteExpiration(rateLimit.Period.ToTimeSpan(periodType));
            this.rateLimitCache.MemoryCache.Set(key, rateLimit, this.rateLimitCache.MemoryCacheEntryOptions);
        }
    }
}

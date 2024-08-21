// <copyright file="DetermineRequestRateLimitCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.RequestRateLimit
{
    using UBind.Domain.Attributes;
    using UBind.Domain.Enums;
    using UBind.Domain.Patterns.Cqrs;

    // Normally a command is assumed to have a RequestIntent of ReadWrite, but this command doesn't
    // write to the database, it only writes to a local in memory cache, so it can be marked as ReadOnly.
    [RequestIntent(RequestIntent.ReadOnly)]
    public class DetermineRequestRateLimitCommand : ICommand<RateLimitModel>
    {
        public DetermineRequestRateLimitCommand(string clientIPCode, string endpoint, int period, RateLimitPeriodType periodType, int limit)
        {
            this.ClientIPCode = clientIPCode;
            this.Endpoint = endpoint;
            this.Period = period;
            this.PeriodType = periodType;
            this.Limit = limit;
        }

        public string ClientIPCode { get; set; }

        public string Endpoint { get; set; }

        public int Period { get; set; }

        public RateLimitPeriodType PeriodType { get; set; }

        public int Limit { get; set; }
    }
}

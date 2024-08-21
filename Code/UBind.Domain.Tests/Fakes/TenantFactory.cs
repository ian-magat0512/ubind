// <copyright file="TenantFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Fakes
{
    using System;
    using NodaTime;
    using UBind.Domain.Extensions;

    public static class TenantFactory
    {
        public const string DefaultName = "Fake Tenant";
        public const string DefaultAlias = "fake-tenant";

        public static readonly Guid DefaultId = new Guid("ccae2079-2ebc-4200-879d-866fc82e6afa");

        public static IClock Clock { get; set; } = SystemClock.Instance;

        public static Tenant Create(Guid? tenantId = null, string alias = null)
        {
            return new Tenant(
                tenantId ?? DefaultId,
                DefaultName,
                alias ?? tenantId.ToString(),
                null,
                default,
                default,
                Clock.Now());
        }
    }
}

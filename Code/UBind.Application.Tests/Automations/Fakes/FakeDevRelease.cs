// <copyright file="FakeDevRelease.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Fakes
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// Fale Dev Release class.
    /// </summary>
    internal class FakeDevRelease : DevRelease
    {
        public FakeDevRelease(Guid tenantId, Guid productId, string fileName, string content, NodaTime.Instant createdTimestamp)
            : base(tenantId, productId, createdTimestamp)
        {
            var e = string.Empty;
            var assets = new List<Asset>();
            var fileContent = FileContent.CreateFromBase64String(tenantId, Guid.NewGuid(), content);
            assets.Add(new Asset(tenantId, fileName, createdTimestamp, fileContent, createdTimestamp));
            var quoteDetails = new ReleaseDetails(WebFormAppType.Quote, e, e, e, e, e, e, e, e, assets, assets, default(byte[]), createdTimestamp);
            var claimDetails = new ReleaseDetails(WebFormAppType.Claim, e, e, e, e, e, e, e, e, assets, assets, default(byte[]), createdTimestamp);
        }
    }
}

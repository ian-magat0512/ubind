// <copyright file="ReleaseDetailsFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.Factories;

using System;
using System.Collections.Generic;
using System.Text;
using Humanizer;
using NodaTime;
using UBind.Domain;
using UBind.Domain.ReadWriteModel;
using UBind.Domain.Tests.Fakes;

public class ReleaseDetailsFactory
{
    public static ReleaseDetails GenerateReleaseDetails(WebFormAppType appType, Instant createdTimestamp)
    {
        var appTypeString = appType.Humanize().ToLower();
        var privateFiles = new List<Asset>
        {
            new Asset(
            TenantFactory.DefaultId,
            $"test-{appTypeString}-private-file1.txt",
            createdTimestamp,
            FileContent.CreateFromBytes(TenantFactory.DefaultId, Guid.NewGuid(), Encoding.UTF8.GetBytes($"This is test {appTypeString} private file one.")),
            createdTimestamp),
        };
        if (appType == WebFormAppType.Claim)
        {
            privateFiles.Add(new Asset(
            TenantFactory.DefaultId,
            $"test-{appTypeString}-private-file2.txt",
            createdTimestamp,
            FileContent.CreateFromBytes(TenantFactory.DefaultId, Guid.NewGuid(), Encoding.UTF8.GetBytes($"This is test {appTypeString} private file two.")),
            createdTimestamp));
        }
        var publicFiles = new List<Asset>
        {
            new Asset(
            TenantFactory.DefaultId,
            $"test-{appTypeString}-public-asset1.txt",
            createdTimestamp,
            FileContent.CreateFromBytes(TenantFactory.DefaultId, Guid.NewGuid(), Encoding.UTF8.GetBytes($"This is test {appTypeString} public asset one.")),
            createdTimestamp),
        };
        if (appType == WebFormAppType.Claim)
        {
            publicFiles.Add(new Asset(
            TenantFactory.DefaultId,
            $"test-{appTypeString}-public-asset2.txt",
            createdTimestamp,
            FileContent.CreateFromBytes(TenantFactory.DefaultId, Guid.NewGuid(), Encoding.UTF8.GetBytes($"This is test {appTypeString} public asset two.")),
            createdTimestamp));
        }
        var releaseDetails = new ReleaseDetails(
            appType,
            "{}",
            "{}",
            null,
            null,
            null,
            null,
            null,
            null,
            privateFiles,
            publicFiles,
            Array.Empty<byte>(),
            createdTimestamp);
        return releaseDetails;
    }
}

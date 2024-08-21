// <copyright file="DevReleaseRepositoryTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.Repositories;

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FluentAssertions;
using UBind.Domain;
using UBind.Domain.Extensions;
using UBind.Domain.ReadWriteModel;
using UBind.Domain.Repositories;
using UBind.Domain.Tests.Fakes;
using UBind.Persistence.Repositories;
using UBind.Persistence.Tests.Factories;
using UBind.Persistence.Tests.Fakes;
using Xunit;

[Collection(DatabaseCollection.Name)]
public class DevReleaseRepositoryTests
{
    private readonly IUBindDbContext dbContext;
    private readonly IDevReleaseRepository devReleaseRepository;
    private readonly IFileContentRepository fileContentRepository;
    private readonly TestClock clock = new TestClock();

    public DevReleaseRepositoryTests()
    {
        this.dbContext = new TestUBindDbContext(DatabaseFixture.TestConnectionString);
        this.fileContentRepository = new FileContentRepository(this.dbContext);
        this.devReleaseRepository = new DevReleaseRepository(this.dbContext, this.fileContentRepository);
    }

    [Fact]
    public async Task SaveChanges_WithReleaseDetailsChangeTracker_PersistsNewDevRelease()
    {
        // Arrange
        var tracker = this.GenerateReleaseChangeTrackerWithNewDevReleaseAndQuoteDetails();

        // Act
        await this.devReleaseRepository.SaveChanges(tracker);

        // Assert
        var devRelease = this.dbContext.DevReleases
            .FirstOrDefault(dr => dr.TenantId == tracker.DevRelease.TenantId
                && dr.Id == tracker.DevRelease.Id);
        devRelease.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDevReleaseByIdWithFileContents_Returns_CorrectFilesWithContents()
    {
        // Arrange
        var tracker = this.GenerateReleaseChangeTrackerWithNewDevReleaseAndQuoteDetails();
        await this.devReleaseRepository.SaveChanges(tracker);

        // Act
        var devRelease = this.devReleaseRepository
            .GetDevReleaseByIdWithFileContents(tracker.DevRelease.TenantId, tracker.DevRelease.Id);

        // Assert
        devRelease.ClaimDetails.Should().BeNull();
        devRelease.QuoteDetails.Should().NotBeNull();
        devRelease.QuoteDetails.Files.Should().HaveCount(tracker.ReleaseDetails.Files.Count);
        devRelease.QuoteDetails.Assets.Should().HaveCount(tracker.ReleaseDetails.Assets.Count);
        devRelease.QuoteDetails.Files.First().Name.Should().Be(tracker.ReleaseDetails.Files.First().Name);
        devRelease.QuoteDetails.Files.First().FileContentId.Should().NotBeEmpty();
        devRelease.QuoteDetails.Files.First().FileContent.Should().NotBeNull();
        devRelease.QuoteDetails.Files.First().FileContent.Content.Should()
            .BeEquivalentTo(tracker.ReleaseDetails.Files.First().FileContent.Content);
    }

    [Fact]
    public async Task SaveChanges_PersistsNewClaimDetails_WhenClaimDetailsAreAdded()
    {
        // Arrange
        var tracker = this.GenerateReleaseChangeTrackerWithNewDevReleaseAndQuoteDetails();
        await this.devReleaseRepository.SaveChanges(tracker);
        tracker.IsDevReleaseNew = false;
        var claimDetails = ReleaseDetailsFactory.GenerateReleaseDetails(WebFormAppType.Claim, this.clock.Now());
        tracker.ReleaseDetails = claimDetails;
        tracker.AddedPrivateFiles = claimDetails.Files.ToList();
        tracker.AddedPublicFiles = claimDetails.Assets.ToList();
        tracker.DevRelease.ClaimDetails = claimDetails;
        tracker.ComponentType = WebFormAppType.Claim;
        tracker.ReleaseDetails = claimDetails;
        tracker.IsReleaseDetailsNew = true;

        // Act
        await this.devReleaseRepository.SaveChanges(tracker);

        // Assert
        var devRelease = this.devReleaseRepository
            .GetDevReleaseByIdWithFileContents(tracker.DevRelease.TenantId, tracker.DevRelease.Id);
        devRelease.ClaimDetails.Should().NotBeNull();
        devRelease.ClaimDetails.Files.Should().HaveCount(tracker.ReleaseDetails.Files.Count);
        devRelease.ClaimDetails.Assets.Should().HaveCount(tracker.ReleaseDetails.Assets.Count);
        var files = devRelease.ClaimDetails.Files.OrderBy(x => x.Name);
        files.First().Name.Should().Be(tracker.ReleaseDetails.Files.First().Name);
        files.First().FileContentId.Should().NotBeEmpty();
        files.First().FileContent.Should().NotBeNull();
        files.First().FileContent.Content.Should()
            .BeEquivalentTo(tracker.ReleaseDetails.Files.First().FileContent.Content);
    }

    [Fact]
    public async Task GetDevReleaseForProductWithoutAssetFileContents_Returns_CorrectFilesWithoutContents()
    {
        // Arrange
        var tracker = this.GenerateReleaseChangeTrackerWithNewDevReleaseAndQuoteDetails(Guid.NewGuid());
        await this.devReleaseRepository.SaveChanges(tracker);

        // Act
        var devRelease = this.devReleaseRepository
            .GetDevReleaseForProductWithoutAssetFileContents(tracker.DevRelease.TenantId, tracker.DevRelease.ProductId);

        // Assert
        devRelease.ClaimDetails.Should().BeNull();
        devRelease.QuoteDetails.Should().NotBeNull();
        devRelease.QuoteDetails.Files.Should().HaveCount(tracker.ReleaseDetails.Files.Count);
        devRelease.QuoteDetails.Assets.Should().HaveCount(tracker.ReleaseDetails.Assets.Count);
        devRelease.QuoteDetails.Files.First().Name.Should().Be(tracker.ReleaseDetails.Files.First().Name);
        devRelease.QuoteDetails.Files.First().FileContentId.Should().NotBeEmpty();
        devRelease.QuoteDetails.Files.First().FileContent.Should().BeNull();
    }

    [Fact]
    public async Task GetDevReleaseForProductWithoutAssets_Returns_CorrectReleaseDetailsWithoutAssets()
    {
        // Arrange
        var tracker = this.GenerateReleaseChangeTrackerWithNewDevReleaseAndClaimDetails(Guid.NewGuid());
        await this.devReleaseRepository.SaveChanges(tracker);

        // Act
        var devRelease = this.devReleaseRepository
            .GetDevReleaseForProductWithoutAssets(tracker.DevRelease.TenantId, tracker.DevRelease.ProductId);

        // Assert
        devRelease.ClaimDetails.Should().NotBeNull();
        devRelease.QuoteDetails.Should().BeNull();
        devRelease.ClaimDetails.Files.Should().HaveCount(0);
        devRelease.ClaimDetails.Assets.Should().HaveCount(0);
    }

    [Fact]
    public async Task GetDevReleaseByIdWithFileContents_NewQueryShouldReturnIdenticalValue_WhenComparedToTheOldQuery()
    {
        // Arrange
        ReleaseDetailsChangeTracker tracker = this.GenerateReleaseChangeTrackerWithNewDevReleaseAndQuoteDetails();
        await this.devReleaseRepository.SaveChanges(tracker);

        // Act
        var devReleaseNew = this.devReleaseRepository
            .GetDevReleaseByIdWithFileContents(tracker.DevRelease.TenantId, tracker.DevRelease.Id);
        var devReleaseOld = this.OldQuery_GetDevReleaseByIdWithFileContents(tracker.DevRelease.TenantId, tracker.DevRelease.Id);

        // Assert
        devReleaseNew.Should().NotBeNull();
        devReleaseOld.Should().NotBeNull();
        devReleaseNew.Should().BeEquivalentTo(devReleaseOld, options => options.ComparingByMembers<DevRelease>());
    }

    [Fact]
    public async Task GetDevReleaseForProductWithoutAssetFileContents_NewQueryShouldReturnIdenticalValue_WhenComparedToTheOldQuery()
    {
        // Arrange
        ReleaseDetailsChangeTracker tracker = this.GenerateReleaseChangeTrackerWithNewDevReleaseAndQuoteDetails();
        await this.devReleaseRepository.SaveChanges(tracker);

        // Act
        var devReleaseNew = this.devReleaseRepository
            .GetDevReleaseForProductWithoutAssetFileContents(tracker.DevRelease.TenantId, tracker.DevRelease.ProductId);
        var devReleaseOld = this.OldQuery_GetDevReleaseForProductWithoutAssetFileContents(tracker.DevRelease.TenantId, tracker.DevRelease.ProductId);

        // Assert
        devReleaseNew.Should().NotBeNull();
        devReleaseOld.Should().NotBeNull();
        devReleaseNew.Should().BeEquivalentTo(devReleaseOld, options => options.ComparingByMembers<DevRelease>());
    }

    [Fact]
    public async Task GetDevReleaseWithoutAssetFileContents_NewQueryShouldReturnIdenticalValue_WhenComparedToTheOldQuery()
    {
        // Arrange
        ReleaseDetailsChangeTracker tracker = this.GenerateReleaseChangeTrackerWithNewDevReleaseAndQuoteDetails();
        await this.devReleaseRepository.SaveChanges(tracker);

        // Act
        var devReleaseNew = this.devReleaseRepository
            .GetDevReleaseWithoutAssetFileContents(tracker.DevRelease.TenantId, tracker.DevRelease.Id);
        var devReleaseOld = this.OldQuery_GetDevReleaseWithoutAssetFileContents(tracker.DevRelease.TenantId, tracker.DevRelease.Id);

        // Assert
        devReleaseNew.Should().NotBeNull();
        devReleaseOld.Should().NotBeNull();
        devReleaseNew.Should().BeEquivalentTo(devReleaseOld, options => options.ComparingByMembers<DevRelease>());
    }

    private ReleaseDetailsChangeTracker GenerateReleaseChangeTrackerWithNewDevReleaseAndQuoteDetails(Guid? productId = null)
    {
        productId = productId ?? ProductFactory.DefaultId;
        var devRelease = new DevRelease(TenantFactory.DefaultId, productId.Value, this.clock.Now());
        var tracker = new ReleaseDetailsChangeTracker(WebFormAppType.Quote);
        tracker.DevRelease = devRelease;
        tracker.IsDevReleaseNew = true;
        var quoteDetails = ReleaseDetailsFactory.GenerateReleaseDetails(WebFormAppType.Quote, this.clock.Now());
        tracker.ReleaseDetails = quoteDetails;
        tracker.AddedPrivateFiles = quoteDetails.Files.ToList();
        tracker.AddedPublicFiles = quoteDetails.Assets.ToList();
        devRelease.QuoteDetails = quoteDetails;
        return tracker;
    }

    private ReleaseDetailsChangeTracker GenerateReleaseChangeTrackerWithNewDevReleaseAndClaimDetails(Guid? productId = null)
    {
        productId = productId ?? ProductFactory.DefaultId;
        var devRelease = new DevRelease(TenantFactory.DefaultId, productId.Value, this.clock.Now());
        var tracker = new ReleaseDetailsChangeTracker(WebFormAppType.Claim);
        tracker.DevRelease = devRelease;
        tracker.IsDevReleaseNew = true;
        var claimDetails = ReleaseDetailsFactory.GenerateReleaseDetails(WebFormAppType.Claim, this.clock.Now());
        tracker.ReleaseDetails = claimDetails;
        tracker.AddedPrivateFiles = claimDetails.Files.ToList();
        tracker.AddedPublicFiles = claimDetails.Assets.ToList();
        devRelease.ClaimDetails = claimDetails;
        return tracker;
    }

    private DevRelease? OldQuery_GetDevReleaseByIdWithFileContents(Guid tenantId, Guid releaseId)
    {
        var connection = this.dbContext.Database.Connection;
        var sql = @"
                -- Fetch DevRelease with QuoteDetails and ClaimDetails
                SELECT TOP(1) dr.*, qd.*, cd.*
                FROM DevReleases dr
                LEFT JOIN ReleaseDetails qd ON dr.QuoteDetails_Id = qd.Id
                LEFT JOIN ReleaseDetails cd ON dr.ClaimDetails_Id = cd.Id
                WHERE dr.TenantId = @TenantId AND dr.Id = @ReleaseId;

                -- Fetch all Assets associated with the ReleaseDetails of a specific Release
                SELECT a.*, fc.*, 
                       rd.Id AS ReleaseDetailsId,
                       CASE 
                           WHEN a.ReleaseDetails_Id IS NOT NULL THEN 1 
                           ELSE 0 
                       END AS IsPublic
                FROM Assets a
                LEFT JOIN FileContents fc ON a.FileContentId = fc.Id
                JOIN ReleaseDetails rd ON a.ReleaseDetails_Id = rd.Id OR a.ReleaseDetails_Id1 = rd.Id
                WHERE rd.Id IN 
                    (SELECT QuoteDetails_Id FROM DevReleases WHERE TenantId = @TenantId AND Id = @ReleaseId)
                    OR rd.Id IN 
                    (SELECT ClaimDetails_Id FROM DevReleases WHERE TenantId = @TenantId AND Id = @ReleaseId);
            ";

        using (var results = connection.QueryMultiple(sql, new { TenantId = tenantId, ReleaseId = releaseId }))
        {
            var devReleases = results.Read<DevRelease, ReleaseDetails, ReleaseDetails, DevRelease>(
                (dr, qd, cd) =>
                {
                    dr.QuoteDetails = qd;
                    dr.ClaimDetails = cd;
                    return dr;
                },
                splitOn: "Id, Id").ToList();

            var assetsWithContent = results.Read<Asset, FileContent, Guid, int, Asset>(
                (asset, fileContent, releaseDetailsId, isPublicInt) =>
                {
                    asset.FileContent = fileContent;
                    var isPublic = isPublicInt == 1;
                    var releaseDetails = devReleases.SelectMany(dr => new[] { dr.QuoteDetails, dr.ClaimDetails })
                        .FirstOrDefault(rd => rd != null && rd.Id == releaseDetailsId);
                    if (releaseDetails != null)
                    {
                        if (isPublic)
                        {
                            releaseDetails.Assets.Add(asset);
                        }
                        else
                        {
                            releaseDetails.Files.Add(asset);
                        }
                    }
                    return asset;
                },
                splitOn: "Id, ReleaseDetailsId, IsPublic").ToList();
            return devReleases.FirstOrDefault();
        }
    }

    private DevRelease? OldQuery_GetDevReleaseForProductWithoutAssetFileContents(Guid tenantId, Guid productId)
    {
        var connection = this.dbContext.Database.Connection;
        var sql = @"
                -- Fetch DevRelease with QuoteDetails and ClaimDetails
                SELECT TOP(1) dr.*, qd.*, cd.*
                FROM DevReleases dr
                LEFT JOIN ReleaseDetails qd ON dr.QuoteDetails_Id = qd.Id
                LEFT JOIN ReleaseDetails cd ON dr.ClaimDetails_Id = cd.Id
                WHERE dr.TenantId = @TenantId AND dr.ProductId = @ProductId;

                 -- Fetch Assets for QuoteDetails and ClaimDetails
                SELECT a.*, rd.Id AS ReleaseDetailsId,
                    CASE WHEN a.ReleaseDetails_Id IS NOT NULL THEN 1 ELSE 0 END AS IsPublic 
                FROM Assets a
                LEFT JOIN ReleaseDetails rd ON a.ReleaseDetails_Id = rd.Id OR a.ReleaseDetails_Id1 = rd.Id
                WHERE rd.Id IN 
                    (SELECT QuoteDetails_Id FROM DevReleases WHERE TenantId = @TenantId AND ProductId = @ProductId)
                    OR rd.Id IN 
                    (SELECT ClaimDetails_Id FROM DevReleases WHERE TenantId = @TenantId AND ProductId = @ProductId);";

        using (var results = connection.QueryMultiple(sql, new { TenantId = tenantId, ProductId = productId }))
        {
            var devReleases = results.Read<DevRelease, ReleaseDetails, ReleaseDetails, DevRelease>(
                (dr, qd, cd) =>
                {
                    dr.QuoteDetails = qd;
                    dr.ClaimDetails = cd;
                    return dr;
                },
                splitOn: "Id, Id").ToList();

            results.Read<Asset, Guid, int, Asset>(
                (asset, releaseDetailsId, isPublicInt) =>
                {
                    var isPublic = isPublicInt == 1;
                    var releaseDetails = devReleases.SelectMany(dr => new[] { dr.QuoteDetails, dr.ClaimDetails })
                        .FirstOrDefault(rd => rd?.Id == releaseDetailsId);
                    if (releaseDetails != null)
                    {
                        if (isPublic)
                        {
                            releaseDetails.Assets.Add(asset);
                        }
                        else
                        {
                            releaseDetails.Files.Add(asset);
                        }
                    }
                    return asset;
                },
                splitOn: "ReleaseDetailsId, IsPublic").ToList();

            var devRelease = devReleases.FirstOrDefault();
            return devRelease;
        }
    }

    private DevRelease? OldQuery_GetDevReleaseWithoutAssetFileContents(Guid tenantId, Guid productReleaseId)
    {
        var connection = this.dbContext.Database.Connection;
        var sql = @"
                -- Fetch DevRelease with QuoteDetails and ClaimDetails
                SELECT TOP(1) dr.*, qd.*, cd.*
                FROM DevReleases dr
                LEFT JOIN ReleaseDetails qd ON dr.QuoteDetails_Id = qd.Id
                LEFT JOIN ReleaseDetails cd ON dr.ClaimDetails_Id = cd.Id
                WHERE dr.TenantId = @TenantId AND dr.Id = @ProductReleaseId;

                 -- Fetch Assets for QuoteDetails and ClaimDetails
                SELECT a.*, rd.Id AS ReleaseDetailsId, CASE WHEN a.ReleaseDetails_Id IS NOT NULL THEN 1 ELSE 0 END AS IsPublic 
                FROM Assets a
                LEFT JOIN ReleaseDetails rd ON a.ReleaseDetails_Id = rd.Id OR a.ReleaseDetails_Id1 = rd.Id
                WHERE rd.Id IN 
                    (SELECT QuoteDetails_Id FROM DevReleases WHERE TenantId = @TenantId AND Id = @ProductReleaseId)
                    OR rd.Id IN 
                    (SELECT ClaimDetails_Id FROM DevReleases WHERE TenantId = @TenantId AND Id = @ProductReleaseId);";

        using (var results = connection.QueryMultiple(sql, new { TenantId = tenantId, ProductReleaseId = productReleaseId }))
        {
            var devReleases = results.Read<DevRelease, ReleaseDetails, ReleaseDetails, DevRelease>(
                (dr, qd, cd) =>
                {
                    dr.QuoteDetails = qd;
                    dr.ClaimDetails = cd;
                    return dr;
                },
                splitOn: "Id, Id").ToList();

            results.Read<Asset, Guid, int, Asset>(
                (asset, releaseDetailsId, isPublicInt) =>
                {
                    var isPublic = isPublicInt == 1;
                    var releaseDetails = devReleases.SelectMany(dr => new[] { dr.QuoteDetails, dr.ClaimDetails })
                        .FirstOrDefault(rd => rd?.Id == releaseDetailsId);
                    if (releaseDetails != null)
                    {
                        if (isPublic)
                        {
                            releaseDetails.Assets.Add(asset);
                        }
                        else
                        {
                            releaseDetails.Files.Add(asset);
                        }
                    }
                    return asset;
                },
                splitOn: "ReleaseDetailsId, IsPublic").ToList();

            var devRelease = devReleases.FirstOrDefault();
            return devRelease;
        }
    }
}

// <copyright file="ReleaseRepositoryTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.Repositories;

using Dapper;
using FluentAssertions;
using System.Diagnostics;
using UBind.Domain;
using UBind.Domain.Extensions;
using UBind.Domain.ReadWriteModel;
using UBind.Domain.Repositories;
using UBind.Domain.Tests.Fakes;
using UBind.Persistence.Tests.Factories;
using UBind.Persistence.Tests.Fakes;
using Xunit;

[Collection(DatabaseCollection.Name)]
public class ReleaseRepositoryTests
{
    private readonly IUBindDbContext dbContext;
    private readonly IReleaseRepository releaseRepository;
    private readonly TestClock clock = new TestClock();

    public ReleaseRepositoryTests()
    {
        this.dbContext = new TestUBindDbContext(DatabaseFixture.TestConnectionString);
        this.releaseRepository = new ReleaseRepository(this.dbContext);
    }

    [Fact]
    public async Task GetReleaseByIdWithFileContents_Returns_CorrectReleaseDetailsWithFileContents()
    {
        // Arrange
        var release = this.GenerateReleaseWithQuoteDetails(Guid.NewGuid());
        this.releaseRepository.Insert(release);
        await this.dbContext.SaveChangesAsync();

        // Act
        var result = this.releaseRepository.GetReleaseByIdWithFileContents(release.TenantId, release.Id);

        // Assert
        result.ClaimDetails.Should().BeNull();
        result.QuoteDetails.Should().NotBeNull();
        result.QuoteDetails.Files.Should().HaveCount(release.QuoteDetails.Files.Count);
        result.QuoteDetails.Assets.Should().HaveCount(release.QuoteDetails.Assets.Count);
        result.QuoteDetails.Files.First().Name.Should().Be(release.QuoteDetails.Files.First().Name);
        result.QuoteDetails.Files.First().FileContentId.Should().NotBeEmpty();
        result.QuoteDetails.Files.First().FileContent.Should().NotBeNull();
        result.QuoteDetails.Files.First().FileContent.Content.Should()
            .BeEquivalentTo(release.QuoteDetails.Files.First().FileContent.Content);
    }

    [Fact]
    public async Task GetReleaseWithoutAssetFileContents_Returns_CorrectReleaseDetailsWithoutFileContents()
    {
        // Arrange
        var release = this.GenerateReleaseWithQuoteDetails(Guid.NewGuid());
        this.releaseRepository.Insert(release);
        await this.dbContext.SaveChangesAsync();

        // Act
        var result = this.releaseRepository.GetReleaseWithoutAssetFileContents(release.TenantId, release.Id);

        // Assert
        result.ClaimDetails.Should().BeNull();
        result.QuoteDetails.Should().NotBeNull();
        result.QuoteDetails.Files.Should().HaveCount(release.QuoteDetails.Files.Count);
        result.QuoteDetails.Assets.Should().HaveCount(release.QuoteDetails.Assets.Count);
        result.QuoteDetails.Files.First().Name.Should().Be(release.QuoteDetails.Files.First().Name);
        result.QuoteDetails.Files.First().FileContentId.Should().NotBeEmpty();
        result.QuoteDetails.Files.First().FileContent.Should().BeNull();
    }

    [Fact]
    public async Task GetReleaseWithoutAssets_Returns_CorrectReleaseDetailsWithoutFiles()
    {
        // Arrange
        var release = this.GenerateReleaseWithQuoteDetails(Guid.NewGuid());
        this.releaseRepository.Insert(release);
        await this.dbContext.SaveChangesAsync();

        // Act
        var result = this.releaseRepository.GetReleaseWithoutAssets(release.TenantId, release.Id);

        // Assert
        result.ClaimDetails.Should().BeNull();
        result.QuoteDetails.Should().NotBeNull();
        result.QuoteDetails.Files.Should().HaveCount(0);
        result.QuoteDetails.Assets.Should().HaveCount(0);
    }

    [Fact]
    public async Task GetReleaseWithoutAssetFileContents_NewQueryShouldReturnIdenticalValue_WhenComparedToTheOldQuery()
    {
        // Arrange
        int releaseCount = 15;
        var productId = Guid.NewGuid();
        await this.PopulateReleases(releaseCount, productId);
        Release release = this.GenerateReleaseWithQuoteAndClaimDetails(productId, 100001);
        this.releaseRepository.Insert(release);

        await this.dbContext.SaveChangesAsync();

        // Act
        var releaseUsingNewQuery = this.releaseRepository.GetReleaseWithoutAssetFileContents(release.TenantId, release.Id);
        var releaseUsingOldQuery = this.OldQuery_GetReleaseWithoutAssetFileContents(release.TenantId, release.Id);

        // Assert
        releaseUsingNewQuery.Should().NotBeNull();
        releaseUsingOldQuery.Should().NotBeNull();
        releaseUsingNewQuery.Should().BeEquivalentTo(releaseUsingOldQuery, options => options.ComparingByMembers<Release>());
    }

    [Fact]
    public async Task GetReleaseByIdWithFileContents_NewQueryShouldReturnIdenticalValue_WhenComparedToTheOldQuery()
    {
        // Arrange
        int releaseCount = 10;
        var productId = Guid.NewGuid();
        await this.PopulateReleases(releaseCount, productId);
        Release release = this.GenerateReleaseWithQuoteAndClaimDetails(productId, 100001);
        this.releaseRepository.Insert(release);

        await this.dbContext.SaveChangesAsync();

        // Act
        var releaseUsingNewQuery = this.releaseRepository.GetReleaseByIdWithFileContents(release.TenantId, release.Id);
        var releaseUsingOldQuery = this.OldQuery_GetReleaseByIdWithFileContents(release.TenantId, release.Id);

        // Assert
        releaseUsingNewQuery.Should().NotBeNull();
        releaseUsingOldQuery.Should().NotBeNull();
        releaseUsingNewQuery.Should().BeEquivalentTo(releaseUsingOldQuery, options => options.ComparingByMembers<Release>());
    }

    private Release GenerateReleaseWithQuoteDetails(Guid? productId = null, int releaseNo = 1)
    {
        var release = this.CreateRelease(productId, releaseNo);
        release.QuoteDetails = ReleaseDetailsFactory.GenerateReleaseDetails(WebFormAppType.Quote, this.clock.Now());
        return release;
    }

    private Release GenerateReleaseWithQuoteAndClaimDetails(Guid? productId = null, int releaseNo = 1)
    {
        var release = this.CreateRelease(productId, releaseNo);
        release.QuoteDetails = ReleaseDetailsFactory.GenerateReleaseDetails(WebFormAppType.Quote, this.clock.Now());
        release.ClaimDetails = ReleaseDetailsFactory.GenerateReleaseDetails(WebFormAppType.Claim, this.clock.Now());
        return release;
    }

    private async Task PopulateReleases(int count, Guid? productId = null)
    {
        for (int i = 0; i < count; i++)
        {
            var release = this.GenerateReleaseWithQuoteAndClaimDetails(productId, i + 1);
            this.releaseRepository.Insert(release);
        }

        await this.dbContext.SaveChangesAsync();
    }

    private Release CreateRelease(Guid? productId = null, int releaseNo = 1)
    {
        productId = productId ?? ProductFactory.DefaultId;
        return new Release(
            TenantFactory.DefaultId,
            productId.Value,
            releaseNo,
            1,
            $"A release for testing {releaseNo}",
            ReleaseType.Major,
            this.clock.Now());
    }

    private Release? OldQuery_GetReleaseWithoutAssetFileContents(Guid tenantId, Guid productReleaseId)
    {
        var connection = this.dbContext.Database.Connection;
        var sql = @"
                    -- Fetch Release with QuoteDetails and ClaimDetails
                    SELECT TOP(1) r.*, qd.*, cd.*
                    FROM Releases r
                    LEFT JOIN ReleaseDetails qd ON r.QuoteDetails_Id = qd.Id
                    LEFT JOIN ReleaseDetails cd ON r.ClaimDetails_Id = cd.Id
                    WHERE r.TenantId = @TenantId AND r.Id = @ProductReleaseId;

                    -- Fetch Assets for QuoteDetails and ClaimDetails
                    SELECT a.*, rd.Id AS ReleaseDetailsId, CASE WHEN a.ReleaseDetails_Id IS NOT NULL THEN 1 ELSE 0 END AS IsPublic 
                    FROM Assets a
                    LEFT JOIN ReleaseDetails rd ON a.ReleaseDetails_Id = rd.Id OR a.ReleaseDetails_Id1 = rd.Id
                    WHERE rd.Id IN 
                        (SELECT QuoteDetails_Id FROM Releases WHERE TenantId = @TenantId AND Id = @ProductReleaseId)
                        OR rd.Id IN 
                        (SELECT ClaimDetails_Id FROM Releases WHERE TenantId = @TenantId AND Id = @ProductReleaseId);";

        using (var results = connection.QueryMultiple(sql, new { TenantId = tenantId, ProductReleaseId = productReleaseId }))
        {
            var releases = results.Read<Release, ReleaseDetails, ReleaseDetails, Release>(
                (r, qd, cd) =>
                {
                    r.QuoteDetails = qd;
                    r.ClaimDetails = cd;
                    return r;
                },
                splitOn: "Id, Id").ToList();

            results.Read<Asset, Guid, int, Asset>(
                (asset, releaseDetailsId, isPublicInt) =>
                {
                    var isPublic = isPublicInt == 1;
                    var releaseDetails = releases.SelectMany(r => new[] { r.QuoteDetails, r.ClaimDetails })
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

            var release = releases.FirstOrDefault();
            return release;
        }
    }

    private Release? OldQuery_GetReleaseByIdWithFileContents(Guid tenantId, Guid productReleaseId)
    {
        var connection = this.dbContext.Database.Connection;
        var sql = @"
                    -- Fetch Release with QuoteDetails and ClaimDetails
                    SELECT TOP(1) r.*, qd.*, cd.*
                    FROM Releases r
                    LEFT JOIN ReleaseDetails qd ON r.QuoteDetails_Id = qd.Id
                    LEFT JOIN ReleaseDetails cd ON r.ClaimDetails_Id = cd.Id
                    WHERE r.TenantId = @TenantId AND r.Id = @ProductReleaseId;

                    -- Fetch Assets for QuoteDetails and ClaimDetails
                    SELECT a.*, fc.*, rd.Id AS ReleaseDetailsId, CASE WHEN a.ReleaseDetails_Id IS NOT NULL THEN 1 ELSE 0 END AS IsPublic 
                    FROM Assets a
                    LEFT JOIN FileContents fc ON a.FileContentId = fc.Id
                    JOIN ReleaseDetails rd ON a.ReleaseDetails_Id = rd.Id OR a.ReleaseDetails_Id1 = rd.Id
                    WHERE rd.Id IN 
                        (SELECT QuoteDetails_Id FROM Releases WHERE TenantId = @TenantId AND Id = @ProductReleaseId)
                        OR rd.Id IN 
                        (SELECT ClaimDetails_Id FROM Releases WHERE TenantId = @TenantId AND Id = @ProductReleaseId);";

        using (var results = connection.QueryMultiple(sql, new { TenantId = tenantId, ProductReleaseId = productReleaseId }))
        {
            var releases = results.Read<Release, ReleaseDetails, ReleaseDetails, Release>(
                (r, qd, cd) =>
                {
                    r.QuoteDetails = qd;
                    r.ClaimDetails = cd;
                    return r;
                },
                splitOn: "Id, Id").ToList();

            var assetsWithContent = results.Read<Asset, FileContent, Guid, int, Asset>(
                (asset, fileContent, releaseDetailsId, isPublicInt) =>
                {
                    asset.FileContent = fileContent;
                    var isPublic = isPublicInt == 1;
                    var releaseDetails = releases.SelectMany(r => new[] { r.QuoteDetails, r.ClaimDetails })
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
            return releases.FirstOrDefault();
        }
    }
}

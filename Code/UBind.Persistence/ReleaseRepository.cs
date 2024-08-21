// <copyright file="ReleaseRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Dapper;
    using LinqKit;
    using StackExchange.Profiling;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;
    using UBind.Persistence.RepositoryResourceScripts;

    /// <inheritdoc/>
    public class ReleaseRepository : IReleaseRepository
    {
        private readonly IUBindDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public ReleaseRepository(IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Guid? GetReleaseIdByReleaseNumber(Guid tenantId, Guid productId, int majorNumber, int? minorNumber)
        {
            return this.dbContext.Releases
                .Where(r => r.TenantId == tenantId
                        && r.ProductId == productId
                        && r.Number == majorNumber
                        && r.MinorNumber == minorNumber)
                .Select(r => (Guid?)r.Id)
                .FirstOrDefault();
        }

        /// <inheritdoc/>
        public Release? GetReleaseByIdWithFileContents(Guid tenantId, Guid productReleaseId)
        {
            using (MiniProfiler.Current.Step($"{nameof(ReleaseRepository)}.{nameof(this.GetReleaseWithoutAssets)}"))
            {
                /* The EF6 Query is incredibly slow, so we switched to dapper below.
                var release = this.dbContext.Releases
                    .Include(r => r.QuoteDetails.Files.Select(f => f.FileContent))
                    .Include(r => r.QuoteDetails.Assets.Select(a => a.FileContent))
                    .Include(r => r.ClaimDetails.Files.Select(f => f.FileContent))
                    .Include(r => r.ClaimDetails.Assets.Select(a => a.FileContent))
                    .Where(r => r.TenantId == tenantId && r.Id == productReleaseId)
                    .FirstOrDefault();
                return release;
                */
                var connection = this.dbContext.Database.Connection;
                using (var results = connection.QueryMultiple(
                    ReleaseRepositoryResourceScript.GetReleaseByIdWithFileContents,
                    new { TenantId = tenantId, ProductReleaseId = productReleaseId }))
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

        public Release? GetReleaseWithoutAssetFileContents(Guid tenantId, Guid productReleaseId)
        {
            using (MiniProfiler.Current.Step($"{nameof(ReleaseRepository)}.{nameof(this.GetReleaseWithoutAssetFileContents)}"))
            {
                var connection = this.dbContext.Database.Connection;
                using (var results = connection.QueryMultiple(
                    ReleaseRepositoryResourceScript.GetReleaseWithoutAssetFileContents,
                    new { TenantId = tenantId, ProductReleaseId = productReleaseId }))
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
        }

        public Release? GetReleaseWithoutAssets(Guid tenantId, Guid productReleaseId)
        {
            using (MiniProfiler.Current.Step($"{nameof(ReleaseRepository)}.{nameof(this.GetReleaseWithoutAssets)}"))
            {
                /* The EF6 Query is incredibly slow, so we switched to dapper below.
                var release = this.dbContext.Releases
                    .Include(r => r.QuoteDetails)
                    .Include(r => r.ClaimDetails)
                    .Where(r => r.TenantId == tenantId && r.Id == productReleaseId)
                    .FirstOrDefault();
                return release;
                */
                var connection = this.dbContext.Database.Connection;
                var sql = @"
                    SELECT TOP(1) r.*, qd.*, cd.*
                    FROM Releases r
                    LEFT JOIN ReleaseDetails qd ON r.QuoteDetails_Id = qd.Id
                    LEFT JOIN ReleaseDetails cd ON r.ClaimDetails_Id = cd.Id
                    WHERE r.TenantId = @TenantId AND r.Id = @ProductReleaseId";

                var release = connection.Query<Release, ReleaseDetails, ReleaseDetails, Release>(
                    sql,
                    (r, qd, cd) =>
                    {
                        r.QuoteDetails = qd;
                        r.ClaimDetails = cd;
                        return r;
                    },
                    new { TenantId = tenantId, ProductReleaseId = productReleaseId },
                    splitOn: "Id, Id").FirstOrDefault();
                return release;
            }
        }

        /// <summary>
        /// gets the highest release number for product.
        /// </summary>
        /// <param name="tenantId">tenantId id.</param>
        /// <param name="productId">product id.</param>
        /// <returns>release number model.</returns>
        public ReleaseNumberModel GetHighestReleaseNumberForProduct(Guid tenantId, Guid productId)
        {
            int majorRelease = this.dbContext.Releases
                .Where(r => r.ProductId == productId && r.TenantId == tenantId)
                .Select(r => r.Number)
                .DefaultIfEmpty(0)
                .Max();

            int minorRelease = this.dbContext.Releases
                .Where(r => r.ProductId == productId && r.TenantId == tenantId && r.Number == majorRelease)
                .Select(r => r.MinorNumber)
                .DefaultIfEmpty(0)
                .Max();

            return new ReleaseNumberModel(majorRelease, minorRelease);
        }

        /// <inheritdoc/>
        public IEnumerable<Release> GetReleases(Guid tenantId, EntityListFilters filters)
        {
            var query = this.dbContext.Releases
                .Where(r => (tenantId == Tenant.MasterTenantId) || (r.TenantId == tenantId))
                .OrderByDescending(r => r.CreatedTicksSinceEpoch)
                .Take(10);

            if (filters.SearchTerms.Any())
            {
                var searchExpression = PredicateBuilder.New<Release>(false);
                foreach (var searchTerm in filters.SearchTerms)
                {
                    searchExpression.Or(r => r.ProductId.ToString().Contains(searchTerm) ||
                        r.TenantId.ToString().Contains(searchTerm) ||
                        r.Description.Contains(searchTerm) ||
                        r.Number.ToString().Contains(searchTerm));
                }

                query = query.Where(searchExpression);
            }

            if (filters.DateIsAfterTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                query = query.Where(release => release.CreatedTicksSinceEpoch > filters.DateIsAfterTicks);
            }

            if (filters.DateIsBeforeTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                query = query.Where(release => release.CreatedTicksSinceEpoch < filters.DateIsBeforeTicks);
            }

            if (!string.IsNullOrEmpty(filters.SortBy))
            {
                query = query.AsQueryable().Order(filters.SortBy, filters.SortOrder);
            }

            return query.AsQueryable();
        }

        /// <inheritdoc/>
        public IEnumerable<Release> GetReleasesForProduct(Guid tenantId, Guid productId, EntityListFilters filters)
        {
            var query = this.dbContext.Releases
                .Where(r => r.TenantId == tenantId)
                .Where(r => r.ProductId == productId);

            if (filters.SearchTerms.Any())
            {
                var searchExpression = PredicateBuilder.New<Release>(false);
                foreach (var searchTerm in filters.SearchTerms)
                {
                    // As per GPT4, in EF6, this .Contains method translates to SQL as `LIKE "$text%"`, which is
                    // what we are intending to do here. Additionally, the case sensitivity here depends
                    // on whether the collation of the database is case sensitive or not. In our case, our
                    // database is case insensitive, which is why, we don't need to handle it in the code
                    // anymore.
                    // TODO: Once we have upgraded to EF Core, check whether this code is still valid or not.
                    // If it is not, the, kindly update this code and make sure that it works the same, if not better,
                    // than this current code
                    searchExpression.Or(r => r.Description.Contains(searchTerm)
                        || r.Number.ToString().Contains(searchTerm));
                }

                query = query.Where(searchExpression);
            }

            if (filters.DateIsAfterTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                query = query.Where(release => release.CreatedTicksSinceEpoch > filters.DateIsAfterTicks);
            }

            if (filters.DateIsBeforeTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                query = query.Where(release => release.CreatedTicksSinceEpoch < filters.DateIsBeforeTicks);
            }

            if (!string.IsNullOrEmpty(filters.SortBy))
            {
                query = query.Order(filters.SortBy, filters.SortOrder);
            }

            return query.Paginate(filters).ToList();
        }

        /// <inheritdoc/>
        public void Insert(Release release)
        {
            this.dbContext.Releases.Add(release);
        }

        public void Delete(Release release)
        {
            // Since the release was fetched without using EF6, we need to attach it to the context before we can delete it.
            this.dbContext.Releases.Attach(release);

            this.dbContext.Releases.Remove(release);
        }

        /// <inheritdoc/>
        public void SaveChanges()
        {
            try
            {
                this.dbContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.InnerException?.Message.Contains(UBindDbContext.ReleaseProductAndNumberIndex) == true)
                {
                    throw new DuplicateReleaseNumberException("Release for that product with that number already exists.", ex);
                }

                throw;
            }
        }
    }
}

// <copyright file="EagerLoadingExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using UBind.Domain;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.ReadWriteModel.Email;

    /// <summary>
    /// Extension methods to support eager loading of all an entity's navigation properties.
    /// </summary>
    public static class EagerLoadingExtensions
    {
        /// <summary>
        /// Eagerly load all Product's navigation properties.
        /// </summary>
        /// <param name="query">The query being built.</param>
        /// <returns>The query with eager loading applied.</returns>
        public static IQueryable<Product> IncludeAllProperties(this IQueryable<Product> query)
        {
            return query
                .Include(p => p.DetailsCollection)
                .Include(p => p.Events);
        }

        /// <summary>
        /// Eagerly load all QuoteEmailModel's navigation properties.
        /// </summary>
        /// <param name="query">The query being built.</param>
        /// <returns>The query with eager loading applied.</returns>
        public static IQueryable<Email> IncludeAllProperties(this IQueryable<Email> query)
        {
            return query
                .Include(q => q.EmailAttachments)
                .Include(q => q.EmailAttachments.Select(e => e.DocumentFile).Select(d => d.FileContent));
        }

        /// <summary>
        /// Eagerly load all QuoteEmailReadModel's navigation properties.
        /// </summary>
        /// <param name="query">The query being built.</param>
        /// <returns>The query with eager loading applied.</returns>
        public static IQueryable<QuoteEmailReadModel> IncludeAllProperties(this IQueryable<QuoteEmailReadModel> query)
        {
            return query
                .Include(q => q.QuoteEmailSendings);
        }

        /// <summary>
        /// Eagerly load all CustomerReadModel's navigation properties.
        /// </summary>
        /// <param name="query">The query being built.</param>
        /// <returns>The query with eager loading applied.</returns>
        public static IQueryable<CustomerReadModel> IncludeAllProperties(this IQueryable<CustomerReadModel> query)
        {
            return query
                .Include(e => e.People);
        }

        /// <summary>
        /// Eagerly load all PersonReadModel's navigation properties.
        /// </summary>
        /// <param name="query">The query being build.</param>
        /// <returns>The query with eager loading applied.</returns>
        public static IQueryable<PersonReadModel> IncludeAllProperties(this IQueryable<PersonReadModel> query)
        {
            return query
                .Include(e => e.EmailAddresses)
                .Include(p => p.PhoneNumbers)
                .Include(a => a.StreetAddresses)
                .Include(w => w.WebsiteAddresses)
                .Include(m => m.MessengerIds)
                .Include(s => s.SocialMediaIds);
        }

        /// <summary>
        /// Eagerly load all Release's navigation properties.
        /// </summary>
        /// <param name="query">The query being built.</param>
        /// <returns>The query with eager loading applied.</returns>
        public static IQueryable<ReportReadModel> IncludeAllProperties(this IQueryable<ReportReadModel> query)
        {
            return query
                .Include(r => r.Products.Select(p => p.DetailsCollection));
        }

        /// <summary>
        /// Eagerly load all Release's navigation properties.
        /// </summary>
        /// <param name="query">The query being built.</param>
        /// <returns>The query with eager loading applied.</returns>
        public static IEnumerable<DevRelease> IncludeDetails(this IQueryable<DevRelease> query)
        {
            var newQuery = query
                .Select(r => new
                {
                    Release = r,
                    Details = r.QuoteDetails,
                })
                .AsEnumerable()
                .Select(projection => projection.Release);
            return newQuery;
        }

        /// <summary>
        /// Eagerly load product dev release details.
        /// </summary>
        /// <param name="query">The query being built.</param>
        /// <returns>The query with eager loading applied.</returns>
        public static IEnumerable<ReleaseDetails> IncludeReleaseDetails(this IQueryable<DevRelease> query)
        {
            var releaseDetails = query.Select(r => new
            {
                Release = r,
                Details = r.QuoteDetails,
            })
                .AsEnumerable()
                .Select(projection => projection.Release.QuoteDetails);
            return releaseDetails;
        }

        /// <summary>
        /// Eagerly load product dev release details.
        /// </summary>
        /// <param name="query">The query being built.</param>
        /// <returns>The query with eager loading applied.</returns>
        public static IEnumerable<ReleaseDetails> IncludeQuoteReleaseDetails(this IQueryable<DevRelease> query)
        {
            var releaseDetails = query.Select(r => new
            {
                Release = r,
                Details = r.QuoteDetails,
            })
                .AsEnumerable()
                .Select(projection => projection.Release.QuoteDetails);
            return releaseDetails;
        }

        /// <summary>
        /// Eagerly load product dev release details.
        /// </summary>
        /// <param name="query">The query being built.</param>
        /// <returns>The query with eager loading applied.</returns>
        public static IEnumerable<ReleaseDetails> IncludeClaimReleaseDetails(this IQueryable<DevRelease> query)
        {
            var releaseDetails = query.Select(r => new
            {
                Release = r,
                Details = r.ClaimDetails,
            })
                .AsEnumerable()
                .Select(projection => projection.Release.ClaimDetails);
            return releaseDetails;
        }

        /// <summary>
        /// Eagerly load all Dev Release's navigation properties.
        /// </summary>
        /// <param name="query">The query being built.</param>
        /// <returns>The query with eager loading applied.</returns>
        public static IEnumerable<DevRelease> IncludeAllProperties(this IOrderedQueryable<DevRelease> query)
        {
            var newQuery = query
                .Select(r => new
                {
                    Release = r,
                    Details = r.QuoteDetails,
                    QuoteFiles = r.QuoteDetails.Files,
                    QuoteAssets = r.QuoteDetails.Assets,
                    QuoteFilesContent = r.QuoteDetails.Files.Select(f => f.FileContent),
                    QuoteAssetsContent = r.QuoteDetails.Assets.Select(a => a.FileContent),
                    ClaimDetails = r.ClaimDetails,
                    ClaimFiles = r.ClaimDetails.Files,
                    ClaimAssets = r.ClaimDetails.Assets,
                    ClaimFilesContent = r.ClaimDetails.Files.Select(f => f.FileContent),
                    ClaimAssetsContent = r.ClaimDetails.Assets.Select(f => f.FileContent),
                })
                .AsEnumerable()
                .Select(projection => projection.Release);
            return newQuery;
        }

        /// <summary>
        /// Eagerly load all Dev Release's navigation properties.
        /// </summary>
        /// <param name="query">The query being built.</param>
        /// <returns>The query with eager loading applied.</returns>
        public static IEnumerable<DevRelease> IncludeAllProperties(this IQueryable<DevRelease> query)
        {
            var newQuery = query
                .Select(r => new
                {
                    Release = r,
                    Details = r.QuoteDetails,
                    QuoteFiles = r.QuoteDetails.Files,
                    QuoteAssets = r.QuoteDetails.Assets,
                    QuoteAssetsContent = r.QuoteDetails.Assets.Select(a => a.FileContent),
                    QuoteFilesContent = r.QuoteDetails.Files.Select(a => a.FileContent),
                    ClaimDetails = r.ClaimDetails,
                    ClaimFiles2 = r.ClaimDetails.Files,
                    ClaimAssets = r.ClaimDetails.Assets,
                    ClaimAssetsContent = r.ClaimDetails.Assets.Select(a => a.FileContent),
                    ClaimFilesContent = r.ClaimDetails.Files.Select(a => a.FileContent),
                })
                .AsEnumerable()
                .Select(projection => projection.Release);
            return newQuery;
        }

        /// <summary>
        /// Eagerly load all Deployment's navigation properties, including files.
        /// WARNING: this is slow. it loads all files and assets from the database that are associated with a release.
        /// Instead, you probably want to use IncludeMinimalProperties(...).
        /// </summary>
        /// <param name="query">The query being built.</param>
        /// <returns>The query with eager loading applied.</returns>
        public static IEnumerable<Deployment> IncludeAllProperties(this IQueryable<Deployment> query)
        {
            var newQuery = query
              .Where(d => d.Release != null)
              .Select(d => new
              {
                  Deployment = d,
                  Release = d.Release,
                  Details = d.Release.QuoteDetails,
                  Files = d.Release.QuoteDetails.Files,
                  Assets = d.Release.QuoteDetails.Assets,
                  QuoteAssetsContent = d.Release.QuoteDetails.Assets.Select(a => a.FileContent),
                  QuoteFilesContent = d.Release.QuoteDetails.Files.Select(a => a.FileContent),
                  ClaimDetails = d.Release.ClaimDetails,
                  ClaimFiles = d.Release.ClaimDetails.Files,
                  ClaimAssets = d.Release.ClaimDetails.Assets,
                  ClaimAssetsContent = d.Release.ClaimDetails.Assets.Select(a => a.FileContent),
                  ClaimFilesContent = d.Release.ClaimDetails.Files.Select(a => a.FileContent),
              })
              .AsEnumerable()
              .Select(projection => projection.Deployment);
            return newQuery;
        }

        /// <summary>
        /// Eagerly load the Deployment's navigation properties excluding files/assets.
        /// </summary>
        /// <param name="query">The query being built.</param>
        /// <returns>The query with eager loading applied.</returns>
        public static IEnumerable<Deployment> IncludeMinimalProperties(this IQueryable<Deployment> query)
        {
            var newQuery = query
              .Where(d => d.Release != null)
              .Select(d => new
              {
                  Deployment = d,
                  Release = d.Release,
                  Details = d.Release.QuoteDetails,
                  ClaimDetails = d.Release.ClaimDetails,
              })
              .AsEnumerable()
              .Select(projection => projection.Deployment);
            return newQuery;
        }

        /// <summary>
        /// Eagerly load the Deployment's navigation properties including files/assets but not including the
        /// file contents.
        /// </summary>
        /// <param name="query">The query being built.</param>
        /// <returns>The query with eager loading applied.</returns>
        public static IEnumerable<Deployment> IncludeMostProperties(this IQueryable<Deployment> query)
        {
            var newQuery = query
              .Where(d => d.Release != null)
              .Select(d => new
              {
                  Deployment = d,
                  Release = d.Release,
                  QuoteDetails = d.Release.QuoteDetails,
                  QuoteFiles = d.Release.QuoteDetails.Files,
                  QuoteAssets = d.Release.QuoteDetails.Assets,
                  ClaimDetails = d.Release.ClaimDetails,
                  ClaimFiles = d.Release.ClaimDetails.Files,
                  ClaimAssets = d.Release.ClaimDetails.Assets,
              })
              .AsEnumerable()
              .Select(projection => projection.Deployment);
            return newQuery;
        }

        /// <summary>
        /// Eagerly load all Tenant's navigation properties.
        /// </summary>
        /// <param name="query">The query being built.</param>
        /// <returns>The query with eager loading applied.</returns>
        public static IQueryable<Tenant> IncludeAllProperties(this IOrderedQueryable<Tenant> query)
        {
            return query
                .Include(p => p.DetailsCollection);
        }

        /// <summary>
        /// Eagerly load all portal's navigation properties.
        /// </summary>
        /// <param name="query">The query being built.</param>
        /// <returns>The query with portal loading applied.</returns>
        public static IQueryable<Portal> IncludeAllProperties(this IOrderedQueryable<Portal> query)
        {
            return query
                .Include(p => p.PortalDetailsCollection)
                .Include(p => p.DeploymentTargetCollection)
                .Include(p => p.DeploymentTargetCollection.Select(c => c.DeploymentTargetDetailsCollection))
                .Include(p => p.Tenant);
        }

        /// <summary>
        /// Eagerly load all setting's navigation properties.
        /// </summary>
        /// <param name="query">The query being built.</param>
        /// <returns>The query with setting loading applied.</returns>
        public static IQueryable<Setting> IncludeAllProperties(this IOrderedQueryable<Setting> query)
        {
            return query
                .Include(p => p.DetailsCollection)
                .Include(p => p.PortalSettingsCollection)
                .Include(p => p.PortalSettingsCollection.Select(c => c.DetailCollection))
                .Include(u => u.DetailsCollection.Select(c => c.Tenant));
        }

        /// <summary>
        /// Eagerly load all Users Roles.
        /// </summary>
        /// <param name="query">The query being built.</param>
        /// <returns>The query with eager loading applied.</returns>
        public static IQueryable<UserReadModel> IncludeRoles(this IQueryable<UserReadModel> query)
        {
            return query.Include(u => u.Roles);
        }

        /// <summary>
        /// Eagerly load the property definition of an additional property value.
        /// </summary>
        /// <param name="query">The query being built.</param>
        /// <returns>The query with eager loading applied.</returns>
        public static IQueryable<IAdditionalPropertyValueReadModel> IncludeAllProperties(this IQueryable<IAdditionalPropertyValueReadModel> query)
        {
            return query.Include(tpv => tpv.AdditionalPropertyDefinition);
        }

        public static IQueryable<UserReadModel> IncludeLinkedIdentities(this IQueryable<UserReadModel> query)
        {
            return query.Include(u => u.LinkedIdentities);
        }
    }
}

// <copyright file="EntityHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Helpers
{
    using System;
    using Humanizer;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Aggregates.Portal;
    using UBind.Domain.Exceptions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Organisation;

    public static class EntityHelper
    {
        public static TEntity ThrowIfNotFound<TEntity>(TEntity? entity, string propertyValue, string propertyName, string? entityName = null)
        {
            if (string.IsNullOrEmpty(entityName))
            {
                entityName = typeof(TEntity).Name.Humanize(LetterCasing.Sentence).ToLowerInvariant();
            }

            if (entity == null)
            {
                throw new ErrorException(Errors.General.NotFound(entityName, propertyValue, propertyName));
            }

            return entity;
        }

        public static TEntity ThrowIfNotFound<TEntity>(TEntity? entity, GuidOrAlias guidOrAlias, string? entityName = null)
        {
            if (guidOrAlias.Guid.HasValue)
            {
                return ThrowIfNotFound(entity, guidOrAlias.Guid.Value, entityName);
            }
            else
            {
                return ThrowIfNotFound(entity, guidOrAlias.Alias, entityName);
            }
        }

        public static TEntity ThrowIfNotFound<TEntity>(TEntity? entity, Guid id, string? entityName = null)
        {
            if (string.IsNullOrEmpty(entityName))
            {
                entityName = typeof(TEntity).Name.Humanize(LetterCasing.Sentence).ToLowerInvariant();
            }

            if (entity == null)
            {
                throw new ErrorException(Errors.General.NotFound(entityName, id));
            }

            return entity;
        }

        public static TEntity ThrowIfNotFound<TEntity>(TEntity? entity, string alias, string? entityName = null)
        {
            if (string.IsNullOrEmpty(entityName))
            {
                entityName = typeof(TEntity).Name.Humanize(LetterCasing.Sentence).ToLowerInvariant();
            }

            if (entity == null)
            {
                throw new ErrorException(Errors.General.NotFound(entityName, alias, "alias"));
            }

            return entity;
        }

        public static IClaimReadModelSummary ThrowIfNotFound(IClaimReadModelSummary claim, Guid claimId)
        {
            return ThrowIfNotFound(claim, claimId, "claim");
        }

        public static ClaimVersionReadModel ThrowIfNotFound(ClaimVersionReadModel claimVersion, Guid claimVersionId)
        {
            return ThrowIfNotFound(claimVersion, claimVersionId, "claim version");
        }

        public static QuoteVersionReadModel ThrowIfNotFound(QuoteVersionReadModel quoteVersion, Guid quoteVersionId)
        {
            return ThrowIfNotFound(quoteVersion, quoteVersionId, "quote version");
        }

        public static PortalAggregate ThrowIfNotFound(PortalAggregate portalAggregate, Guid portalId)
        {
            return ThrowIfNotFound(portalAggregate, portalId, "portal");
        }

        public static Organisation ThrowIfNotFound(Organisation? organisationAggregate, Guid portalId)
        {
            return ThrowIfNotFound(organisationAggregate, portalId, "organisation");
        }

        public static TEntity ThrowIfNotFoundOrCouldHaveBeenDeleted<TEntity>(TEntity? entity, Guid id, string entityName)
        {
            if (entity == null)
            {
                throw new ErrorException(Errors.General.NotFoundOrCouldHaveBeenDeleted(entityName, id));
            }
            return entity;
        }

        public static IAuthenticationMethodReadModelSummary ThrowIfNotFound(
            Guid tenantId,
            Guid authenticationMethodId,
            IAuthenticationMethodReadModelSummary? authenticationMethod)
        {
            if (authenticationMethod == null)
            {
                throw new ErrorException(Errors.Authentication.ConfigurationNotFound(tenantId, authenticationMethodId));
            }

            return authenticationMethod;
        }
    }
}

// <copyright file="AuthenticationMethodReadModelRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels.Organisation
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Humanizer;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Organisation;
    using UBind.Domain.Repositories;

    public class AuthenticationMethodReadModelRepository : IAuthenticationMethodReadModelRepository
    {
        private readonly IUBindDbContext dbContext;

        public AuthenticationMethodReadModelRepository(IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        private Expression<Func<AuthenticationMethodReadModelSummary, IAuthenticationMethodReadModelSummary>> SummarySelector =>
            method => new AuthenticationMethodReadModelSummary
            {
                TenantId = method.TenantId,
                Id = method.Id,
                Name = method.Name,
                TypeName = method.TypeName,
                CreatedTicksSinceEpoch = method.CreatedTicksSinceEpoch,
                LastModifiedTicksSinceEpoch = method.LastModifiedTicksSinceEpoch,
                IncludeSignInButtonOnPortalLoginPage = method.IncludeSignInButtonOnPortalLoginPage,
                SignInButtonBackgroundColor = method.SignInButtonBackgroundColor,
                SignInButtonIconUrl = method.SignInButtonIconUrl,
                SignInButtonLabel = method.SignInButtonLabel,
                Disabled = method.Disabled,
            };

        public IList<AuthenticationMethodReadModelSummary> Get(Guid tenantId, EntityListFilters filters)
        {
            var query = this.Query(tenantId, filters);
            return query.ToList();
        }

        public IList<IAuthenticationMethodReadModelSummary> GetSummaries(Guid tenantId, EntityListFilters filters)
        {
            var entities = this.Query(tenantId, filters).ToList();
            var summaries = entities.Select(a => (IAuthenticationMethodReadModelSummary)a).ToList();
            return summaries;
        }

        public AuthenticationMethodReadModelSummary? Get(Guid tenantId, Guid authenticationMethodId)
        {
            return this.dbContext.AuthenticationMethods
                .Where(a => a.TenantId == tenantId && a.Id == authenticationMethodId)
                .SingleOrDefault();
        }

        public bool HasLocalAccountRecordForOrganisation(Guid tenantId, Guid organisationId)
        {
            string localAccounTypeName = AuthenticationMethodType.LocalAccount.Humanize();
            return this.dbContext.AuthenticationMethods
                .Any(a => a.TenantId == tenantId
                    && a.OrganisationId == organisationId
                    && a.TypeName == localAccounTypeName);
        }

        public LocalAccountAuthenticationMethodReadModel? GetLocalAccountRecordForOrganisation(
            Guid tenantId, Guid organisationId)
        {
            string localAccountTypeName = AuthenticationMethodType.LocalAccount.Humanize();
            var result = this.dbContext.AuthenticationMethods
                .FirstOrDefault(a => a.TenantId == tenantId
                    && a.OrganisationId == organisationId
                    && a.TypeName == localAccountTypeName);

            return result != null
                ? (LocalAccountAuthenticationMethodReadModel)result
                : null;
        }

        public IList<AuthenticationMethodReadModelSummary> GetMany(
            Guid tenantId,
            IEnumerable<Guid> authenticationMethodIds)
        {
            return this.dbContext.AuthenticationMethods
                .Where(a => a.TenantId == tenantId && authenticationMethodIds.Contains(a.Id))
                .ToList();
        }

        public IList<AuthenticationMethodReadModelSummary> GetUserLinked(Guid tenantId, Guid userId)
        {
            var authenticationMethods = this.dbContext.AuthenticationMethods
                .Join(this.dbContext.UserLinkedIdentities,
                    method => method.Id,
                    link => link.AuthenticationMethodId,
                    (method, link) => new { AuthenticationMethod = method, UserLinkedIdentity = link })
                .Where(t => t.UserLinkedIdentity.UserId == userId
                    && t.UserLinkedIdentity.TenantId == tenantId
                    && t.AuthenticationMethod.TenantId == tenantId)
                .Select(t => t.AuthenticationMethod)
                .ToList();
            return authenticationMethods;
        }

        private IQueryable<AuthenticationMethodReadModelSummary> Query(Guid tenantId, EntityListFilters filters)
        {
            var query = this.dbContext.AuthenticationMethods
                            .Where(a => a.TenantId == tenantId);
            bool hasOrganisationFilters = filters.OrganisationIds != null && filters.OrganisationIds.Any();
            if (hasOrganisationFilters)
            {
                query = query.Where(a => filters.OrganisationIds.Contains(a.OrganisationId));
            }

            return query;
        }
    }
}

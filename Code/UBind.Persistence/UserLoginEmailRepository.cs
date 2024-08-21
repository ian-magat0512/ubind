// <copyright file="UserLoginEmailRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using LinqKit;
    using UBind.Domain;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Repositories;
    using UBind.Persistence.Extensions;

    public class UserLoginEmailRepository : IUserLoginEmailRepository
    {
        private readonly IUBindDbContext dbContext;

        public UserLoginEmailRepository(IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public UserLoginEmail? GetUserLoginByEmail(Guid tenantId, Guid organisationId, string email)
        {
            return this.dbContext.UserLoginEmails.WhereNotDeleted()
                .Where(e => e.TenantId == tenantId && e.LoginEmail == email)
                .OrderByDescending(e => e.OrganisationId == organisationId)
                .FirstOrDefault();
        }

        public List<UserLoginEmail> GetUserLoginsByEmail(Guid tenantId, string email)
        {
            return this.dbContext.UserLoginEmails
                .Where(e => e.TenantId == tenantId && e.LoginEmail == email)
                .ToList();
        }

        public void Add(UserLoginEmail userLoginEmail)
        {
            this.dbContext.UserLoginEmails.Add(userLoginEmail);
        }

        public UserLoginEmail GetUserLoginEmailByEmail(Guid tenantId, Guid organisationId, PortalUserType portalUserType, string loginEmail)
        {
            ExpressionStarter<UserLoginEmail> expression =
                PredicateBuilder.New<UserLoginEmail>(record => record.LoginEmail == loginEmail && !record.IsDeleted);
            return this.GetUserLoginEmail(tenantId, organisationId, portalUserType, expression);
        }

        public UserLoginEmail GetUserLoginEmailById(Guid tenantId, Guid organisationId, PortalUserType portalUserType, Guid id)
        {
            ExpressionStarter<UserLoginEmail> expression =
                PredicateBuilder.New<UserLoginEmail>(record => record.Id == id);
            return this.GetUserLoginEmail(tenantId, organisationId, portalUserType, expression);
        }

        private UserLoginEmail GetUserLoginEmail(Guid tenantId, Guid organisationId, PortalUserType portalUserType, ExpressionStarter<UserLoginEmail> emailOrIdCondition)
        {
            emailOrIdCondition = emailOrIdCondition.And(record => record.TenantId == tenantId);
            if (portalUserType == PortalUserType.Customer)
            {
                emailOrIdCondition = emailOrIdCondition.And(record => record.OrganisationId == organisationId);
            }

            // Check if the record exists in the local context
            var userLoginEmailQuery = this.dbContext.UserLoginEmails.Local.Where(emailOrIdCondition);
            var userLoginEmail = userLoginEmailQuery.SingleOrDefault();

            // If the record doesn't exist in the local context, query the database
            if (userLoginEmail == null)
            {
                userLoginEmailQuery = this.dbContext.UserLoginEmails.Where(emailOrIdCondition);
                userLoginEmail = userLoginEmailQuery.SingleOrDefault();
            }

            return userLoginEmail;
        }
    }
}

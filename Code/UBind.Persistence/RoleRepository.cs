// <copyright file="RoleRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CSharpFunctionalExtensions;
    using LinqKit;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Persistence.Extensions;

    /// <inheritdoc/>
    public class RoleRepository : IRoleRepository
    {
        private readonly IUBindDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoleRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public RoleRepository(IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public void Insert(Role role)
        {
            var existRole = this.dbContext.Roles.Where(w => w.Name == role.Name && w.TenantId == role.TenantId).FirstOrDefault();
            if (existRole != null)
            {
                throw new ErrorException(
                    Errors.General.DuplicatePropertyValue("role", "name", role.Name));
            }

            this.dbContext.Roles.Add(role);
            this.dbContext.SaveChanges();
        }

        /// <inheritdoc/>
        public IReadOnlyList<Role> GetRoles(Guid tenantId, RoleReadModelFilters filters)
        {
            var query = this.dbContext.Roles.AsQueryable().Where(w => w.TenantId == tenantId);

            // Filter by name ignoring case. We don't use any extension method for the comparison to leverage LINQ to entities
            foreach (var name in filters.Names)
            {
                query = query.Where(a => a.Name.ToLower() == name.ToLower());
            }

            // Note: Organisation filter is removed,
            // because roles doesnt work like other entities,
            // all roles are shared through out the tenacy, regardless of organisation.. atleast for now.
            if (filters.DateIsAfterTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                query = query.Where(ExpressionHelper.GreaterThanExpression<Role>(filters.DateFilteringPropertyName, filters.DateIsAfterTicks));
            }

            if (filters.DateIsBeforeTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                query = query.Where(ExpressionHelper.LessThanExpression<Role>(filters.DateFilteringPropertyName, filters.DateIsBeforeTicks));
            }

            if (filters.SearchTerms.Any())
            {
                var searchExpression = PredicateBuilder.New<Role>(false);
                foreach (var searchTerm in filters.SearchTerms)
                {
                    searchExpression.Or(r => r.Name.ToLower().Contains(searchTerm.ToLower()) ||
                        r.Description.ToLower().Contains(searchTerm.ToLower()));
                }

                query = query.Where(searchExpression);
            }

            // perform another filter as needed but for now this is what we need
            if (!string.IsNullOrEmpty(filters.SortBy))
            {
                query = query.Order(filters.SortBy, filters.SortOrder);
            }

            return query.Paginate(filters).ToList();
        }

        /// <inheritdoc/>
        public Role GetRoleById(Guid tenantId, Guid roleId)
        {
            var role = this.dbContext.Roles.AsQueryable()
                .Where(w => w.TenantId == tenantId && w.Id == roleId).FirstOrDefault();

            if (role == null)
            {
                throw new NotFoundException(Errors.General.NotFound("role", roleId));
            }

            return role;
        }

        /// <inheritdoc/>
        public Maybe<Role> TryGetRoleByName(Guid tenantId, string roleName)
        {
            var role = this.dbContext.Roles.AsQueryable()
                .Where(w => w.Name == roleName && w.TenantId == tenantId).FirstOrDefault();
            return role == null
                ? Maybe<Role>.None
                : Maybe<Role>.From(role);
        }

        /// <inheritdoc/>
        public Role GetRoleByNameOrThrow(Guid tenantId, string roleName)
        {
            var role = this.dbContext.Roles.AsQueryable().Where(w => w.Name == roleName && w.TenantId == tenantId).FirstOrDefault();

            if (role == null)
            {
                throw new NotFoundException(Errors.General.NotFound("role", roleName, "name"));
            }

            return role;
        }

        /// <inheritdoc/>
        public Role GetMasterAdminRole()
        {
            var role = this.dbContext.Roles.AsQueryable().Where(
                w =>
                    w.Type == RoleType.Master
                    && w.TenantId == Tenant.MasterTenantId
                    && w.IsAdmin)
                .FirstOrDefault();

            if (role == null)
            {
                throw new NotFoundException(Errors.General.NotFound("role", Enum.GetName(typeof(RoleType), RoleType.Master), "type"));
            }

            return role;
        }

        /// <inheritdoc/>
        public Role GetCustomerRoleForTenant(Guid tenantId)
        {
            if (tenantId == Tenant.MasterTenantId)
            {
                throw new InvalidOperationException("There can be no customer role in the master tenant.");
            }

            return this.dbContext.Roles
                .Where(r => r.TenantId == tenantId)
                .Where(r => r.Type == RoleType.Customer)
                .Single();
        }

        /// <inheritdoc/>
        public Role GetAdminRoleForTenant(Guid tenantId)
        {
            return this.dbContext.Roles
                .Where(r => r.TenantId == tenantId)
                .Where(r => r.IsAdmin)
                .Where(r => r.Name == "Tenant Admin")
                .Single();
        }

        /// <inheritdoc/>
        public bool IsNameInUse(Guid tenantId, string name, Guid roleId = default)
        {
            return this.dbContext.Roles
                .Where(r => r.TenantId == tenantId)
                .Where(r => r.Id != roleId)
                .Any(r => r.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <inheritdoc/>
        public bool Delete(Role role)
        {
            this.dbContext.Roles.Remove(role);
            this.dbContext.SaveChanges();
            return true;
        }

        /// <inheritdoc/>
        public void SaveChanges()
        {
            this.dbContext.SaveChanges();
        }
    }
}

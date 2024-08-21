// <copyright file="UpdateRolesWithNotExistingOrganisationCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration
{
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading;
    using Hangfire;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    public class UpdateRolesWithNotExistingOrganisationCommandHandler
        : ICommandHandler<UpdateRolesWithNotExistingOrganisationCommand, Unit>
    {
        private readonly ILogger<UpdateRolesWithNotExistingOrganisationCommandHandler> logger;
        private readonly IUBindDbContext dbContext;

        public UpdateRolesWithNotExistingOrganisationCommandHandler(
            ILogger<UpdateRolesWithNotExistingOrganisationCommandHandler> logger,
            IUBindDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        [JobDisplayName("Update Roles With Not Existing Organisation")]
        public async Task<Unit> Handle(UpdateRolesWithNotExistingOrganisationCommand request, CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Begin: Update roles with not existing organisation");
            var affectedRoles = this.GetAllAffectedRoles(cancellationToken);

            this.logger.LogInformation($"Found {affectedRoles.Count()} Affected Roles to Update");
            if (affectedRoles != null)
            {
                await this.InitializeBatchUpdate(affectedRoles);
            }

            return Unit.Value;
        }

        /// <summary>
        /// Initialize batch update of found affected roles so that limited number of records
        /// will be locked at a time during update
        /// </summary>
        /// <param name="affectedRoles"></param>
        /// <returns></returns>
        private async Task InitializeBatchUpdate(IEnumerable<AffectedRole> affectedRoles, CancellationToken cancellationToken = default)
        {
            if (!affectedRoles.Any())
            {
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();
            const int batchSize = 10;
            int totalBatches = (int)Math.Ceiling((double)affectedRoles.Count() / batchSize);
            this.logger.LogInformation($"Total Batches to Run: {totalBatches}");

            for (int i = 0; i < totalBatches; i++)
            {
                var batch = affectedRoles.Skip(i * batchSize).Take(batchSize).ToList();
                this.logger.LogInformation($"Batch #{i + 1} Started {DateTime.Now}");
                await this.UpdateBatch(batch);
            }
        }

        /// <summary>
        /// Get all affected roles with not existing organisation
        /// and the correct organisation id from their tenant default organisation
        /// that does not have the same role
        /// </summary>
        /// <returns></returns>
        private IEnumerable<AffectedRole> GetAllAffectedRoles(CancellationToken cancellationToken)
        {
            string sql =
                  @$"SELECT 
	                    RoleId = a.Id,
	                    NewOrganisationId = b.DefaultOrganisationId
                    FROM Roles a WITH(NOLOCK)
                    OUTER APPLY (
	                    SELECT TOP 1 DefaultOrganisationId, Deleted From TenantDetails bb WITH(NOLOCK)
	                    WHERE a.TenantId = bb.Tenant_Id AND bb.Deleted = 0 ORDER BY CreatedTicksSinceEpoch DESC ) b 
                    OUTER APPLY (
	                    SELECT Name as OrganisationName FROM OrganisationReadModels cc WITH(NOLOCK)
	                    WHERE a.OrganisationId = cc.Id ) c
                    OUTER APPLY (
	                    SELECT Name as OrganisationName FROM OrganisationReadModels dd WITH(NOLOCK)
	                    WHERE b.DefaultOrganisationId = dd.Id ) d
                    OUTER APPLY ( 
	                    SELECT COUNT(1) as SameRoleNameFromDefaultOrgCount FROM Roles ee WITH(NOLOCK)
	                    WHERE a.TenantId = ee.TenantId AND a.Name = ee.Name 
	                    AND a.IsAdmin = ee.IsAdmin AND b.DefaultOrganisationId = ee.OrganisationId ) e
                    WHERE c.OrganisationName IS NULL
                    AND a.OrganisationId != '00000000-0000-0000-0000-000000000000'
                    AND a.OrganisationId != b.DefaultOrganisationId
                    AND d.OrganisationName IS NOT NULL
                    AND e.SameRoleNameFromDefaultOrgCount = 0";
            cancellationToken.ThrowIfCancellationRequested();
            return this.dbContext.Database.SqlQuery<AffectedRole>(sql).ToList();
        }

        /// <summary>
        /// Update the given batch of roles
        /// </summary>
        /// <returns></returns>
        private async Task UpdateBatch(IEnumerable<AffectedRole> affectedRoles)
        {
            string affectedRolesForLogMessage = string.Join(", ", affectedRoles.Select(x => x.RoleId.ToString()));
            using var tx = this.dbContext.Database.BeginTransaction(IsolationLevel.ReadUncommitted);
            try
            {
                this.logger.LogInformation($"Processing batch with Role Ids: {affectedRolesForLogMessage}");
                foreach (var affectedRole in affectedRoles)
                {
                    await this.UpdateAffectedRole(affectedRole);
                }
                tx.Commit();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
                this.logger.LogInformation($"Failed updating Role Ids: {affectedRolesForLogMessage}");
                tx.Rollback();
                throw;
            }
        }

        private async Task UpdateAffectedRole(AffectedRole affectedRole)
        {
            string sql = "UPDATE Roles SET OrganisationId = @NewOrganisationId WHERE Id = @RoleId";
            await this.dbContext.Database.ExecuteSqlCommandAsync(sql, new SqlParameter[]
            {
                new SqlParameter("@RoleId", affectedRole.RoleId),
                new SqlParameter("@NewOrganisationId", affectedRole.NewOrganisationId),
            });
            this.logger.LogInformation($"Updated Role: {affectedRole.RoleId}");
        }

        private class AffectedRole
        {
            public Guid RoleId { get; set; }

            public Guid NewOrganisationId { get; set; }
        }
    }
}

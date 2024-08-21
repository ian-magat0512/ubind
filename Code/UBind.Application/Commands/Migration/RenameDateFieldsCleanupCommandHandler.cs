// <copyright file="RenameDateFieldsCleanupCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.Repositories;

    public class RenameDateFieldsCleanupCommandHandler : ICommandHandler<RenameDateFieldsCleanupCommand, Unit>
    {
        private readonly IUBindDbContext dbContext;
        private readonly IPolicyReadModelRepository policyReadModelRepository;
        private readonly ITenantRepository tenantRepository;
        private readonly IPolicyTransactionReadModelRepository policyTransactionReadModelRepository;

        public RenameDateFieldsCleanupCommandHandler(
            IPolicyReadModelRepository policyReadModelRepository,
            IPolicyTransactionReadModelRepository policyTransactionReadModelRepository,
            IUBindDbContext dbContext,
            ITenantRepository tenantRepository)
        {
            this.dbContext = dbContext;
            this.policyReadModelRepository = policyReadModelRepository;
            this.tenantRepository = tenantRepository;
            this.policyTransactionReadModelRepository = policyTransactionReadModelRepository;
        }

        public Task<Unit> Handle(RenameDateFieldsCleanupCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // add the tenantId to the policy transactions table and also update the latest transaction effective date on the policy.
            this.UpdatePolicyLatestPolicyPeriodStartTicksSinceEpochAndPolicyTransactionTenantId();

            // drop IX_PolicyReadModels_CreationDate index, although it should have already been dropped, it seems to be coming back
            // it was replaced with IX_PolicyReadModels_LastModifiedDate_CreatedDate.
            this.dbContext.ExecuteSqlScript(SqlHelper.DropIndexIfExists("PolicyReadModels", "IX_PolicyReadModels_CreationDate"));

            // Drop columns
            this.dbContext.ExecuteSqlScript(SqlHelper.DropColumnWithConstraintsIfExists("ClaimReadModels", "IncidentDateAsDateTime"));
            this.dbContext.ExecuteSqlScript(SqlHelper.DropColumnWithConstraintsIfExists("PolicyTransactions", "CancellationTimeAsTicksSinceEpoch"));

            return Task.FromResult(Unit.Value);
        }

        /// <summary>
        /// We added the LatestPolicyPeriodStartTicksSinceEpoch column to PolicyReadModel, so here we'll
        /// grab the effective timestamp of the latest policy period transaction and set it on the policy read model.
        /// We'll also set the tenant ID on the policy transaction.
        /// </summary>
        private void UpdatePolicyLatestPolicyPeriodStartTicksSinceEpochAndPolicyTransactionTenantId()
        {
            var tenants = this.tenantRepository.GetTenants();
            var policyReadModelFilters = new PolicyReadModelFilters()
            {
                PageSize = 500,
            };

            foreach (var tenant in tenants)
            {
                IEnumerable<PolicyReadModel> policies = null;
                while (policies == null || policies.Count() == policyReadModelFilters.PageSize)
                {
                    policies = this.policyReadModelRepository.QueryPolicyReadModels(tenant.Id, policyReadModelFilters)
                        .Where(p => p.LatestPolicyPeriodStartTicksSinceEpoch == 0)
                        .ToList();
                    foreach (var policy in policies)
                    {
                        var policyDbModel = this.policyReadModelRepository.GetById(policy.TenantId, policy.Id);
                        var transactions = this.policyTransactionReadModelRepository
                            .GetByPolicyId(policyDbModel.Id).ToList();
                        foreach (var transaction in transactions)
                        {
                            // Set the tenant ID as it's a new column
                            transaction.TenantId = policyDbModel.TenantId;
                        }

                        // find the latest transaction that defines the policy period start time.
                        var latestTransaction = transactions.OrderByDescending(t => t.EffectiveTicksSinceEpoch)
                            .Where(t => t is NewBusinessTransaction || t is RenewalTransaction)
                            .FirstOrDefault();

                        policyDbModel.LatestPolicyPeriodStartDateTime = latestTransaction.EffectiveDateTime;
                        policyDbModel.LatestPolicyPeriodStartTicksSinceEpoch = latestTransaction.EffectiveTicksSinceEpoch;

                        // find the latest cancellation transactio
                        var latestCancellationTransaction = transactions.OrderByDescending(t => t.EffectiveTicksSinceEpoch)
                                                    .Where(t => t is CancellationTransaction)
                                                    .FirstOrDefault();
                        if (latestCancellationTransaction != null)
                        {
                            policyDbModel.CancellationEffectiveDateTime = latestTransaction.EffectiveDateTime;
                            policyDbModel.CancellationEffectiveTicksSinceEpoch = latestTransaction.EffectiveTicksSinceEpoch;
                        }

                        // find the latest adjustment transactio
                        var latestAdjustmentTransaction = transactions.OrderByDescending(t => t.EffectiveTicksSinceEpoch)
                                                    .Where(t => t is CancellationTransaction)
                                                    .FirstOrDefault();
                        if (latestAdjustmentTransaction != null)
                        {
                            policyDbModel.AdjustmentEffectiveDateTime = latestTransaction.EffectiveDateTime;
                            policyDbModel.AdjustmentEffectiveTicksSinceEpoch = latestTransaction.EffectiveTicksSinceEpoch;
                        }

                        this.dbContext.SaveChanges();

                        // Sleep so we don't overload the database
                        Thread.Sleep(500);
                    }
                }
            }
        }
    }
}

// <copyright file="PolicyNumberRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System.Data.Entity;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.ReferenceNumbers;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Repository for policy numbers.
    /// </summary>
    public class PolicyNumberRepository : NumberPoolRepositoryBase<PolicyNumber>, IPolicyNumberRepository
    {
        private readonly IUBindDbContext dbContext;
        private readonly DbSet<PolicyNumber> dbSet;
        private readonly IClock clock;

        public PolicyNumberRepository(IUBindDbContext dbContext, IConnectionConfiguration connectionConfiguration, IClock clock)
            : base(dbContext, connectionConfiguration, clock)
        {
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<PolicyNumber>();
            this.clock = clock;
        }

        /// <inheritdoc/>
        public override string Prefix => "P-";

        /// <inheritdoc/>
        public string ConsumePolicyNumber(Guid tenantId, Guid productId, string newNumber, DeploymentEnvironment environment)
        {
            var newNumberRecord = this.GetPolicyNumber(tenantId, productId, newNumber, environment);
            if (newNumberRecord == null)
            {
                var newPolicyNumber = new PolicyNumber(tenantId, productId, environment, newNumber, this.clock.GetCurrentInstant());
                newPolicyNumber.Consume();
                this.dbSet.Add(newPolicyNumber);
            }
            else
            {
                newNumberRecord.Consume();
            }

            return newNumber;
        }

        /// <inheritdoc/>
        public void ReturnOldPolicyNumberToPool(Guid tenantId, Guid productId, string oldPolicyNumber, DeploymentEnvironment environment)
        {
            var policyNumberRecord = this.GetPolicyNumber(tenantId, productId, oldPolicyNumber, environment);
            if (policyNumberRecord != null && policyNumberRecord.IsAssigned)
            {
                policyNumberRecord.UnConsume();
            }
        }

        /// <inheritdoc/>
        public void DeletePolicyNumber(Guid tenantId, Guid productId, string policyNumber, DeploymentEnvironment environment)
        {
            var policyNumberRecord = this.GetPolicyNumber(tenantId, productId, policyNumber, environment);
            if (policyNumberRecord != null && policyNumberRecord.IsAssigned)
            {
                this.dbSet.Remove(policyNumberRecord);
            }
        }

        private PolicyNumber GetPolicyNumber(Guid tenantId, Guid productId, string number, DeploymentEnvironment environment)
        {
            return this.dbSet
                .Where(cn =>
                    cn.TenantId == tenantId &&
                    cn.ProductId == productId &&
                    cn.Environment == environment &&
                    cn.Number == number)
                .FirstOrDefault();
        }
    }
}

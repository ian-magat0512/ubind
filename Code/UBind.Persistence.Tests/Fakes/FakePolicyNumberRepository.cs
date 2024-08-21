// <copyright file="FakePolicyNumberRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.Fakes
{
    using System;
    using System.Collections.Generic;
    using Moq;
    using UBind.Domain;
    using UBind.Domain.Product;
    using UBind.Domain.ReferenceNumbers;

    public class FakePolicyNumberRepository : IPolicyNumberRepository
    {
        private readonly IPolicyNumberRepository realRepository;
        private readonly IPolicyNumberRepository mockRepository;
        private readonly IList<Guid> autoServedTenants = new List<Guid>();

        public FakePolicyNumberRepository(IPolicyNumberRepository realRepository)
        {
            this.realRepository = realRepository;
            var mock = new Mock<IPolicyNumberRepository>();
            mock
                .Setup(r => r.ConsumeForProduct(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>()))
                .Returns(() => Guid.NewGuid().ToString());
            this.mockRepository = mock.Object;
        }

        /// <inheritdoc/>
        public string Prefix => "P-";

        public void AutoServeTenant(Guid tenantId)
        {
            this.autoServedTenants.Add(tenantId);
        }

        public string? ConsumeForProduct(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment,
            bool shouldPersist)
        {
            return this.autoServedTenants.Contains(tenantId)
                ? this.mockRepository.ConsumeForProduct(tenantId, productId, environment)
                : this.realRepository.ConsumeForProduct(tenantId, productId, environment);
        }

        public string? ConsumeForProduct(Guid tenantId, Guid productId, DeploymentEnvironment environment)
        {
            return this.ConsumeForProduct(tenantId, productId, environment, true);
        }

        public IReadOnlyList<string> DeleteForProduct(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment,
            IEnumerable<string> numbers)
        {
            if (this.autoServedTenants.Contains(tenantId))
            {
                return this.mockRepository.DeleteForProduct(tenantId, productId, environment, numbers);
            }
            else
            {
                return this.realRepository.DeleteForProduct(tenantId, productId, environment, numbers);
            }
        }

        public IReadOnlyList<string> GetAllForProduct(Guid tenantId, Guid productId, DeploymentEnvironment environment)
        {
            return this.autoServedTenants.Contains(tenantId)
                ? this.mockRepository.GetAllForProduct(tenantId, productId, environment)
                : this.realRepository.GetAllForProduct(tenantId, productId, environment);
        }

        public IReadOnlyList<string> GetAvailableForProduct(Guid tenantId, Guid productId, DeploymentEnvironment environment)
        {
            return this.autoServedTenants.Contains(tenantId)
                ? this.mockRepository.GetAvailableForProduct(tenantId, productId, environment)
                : this.realRepository.GetAvailableForProduct(tenantId, productId, environment);
        }

        public NumberPoolAddResult LoadForProduct(Guid tenantId, Guid productId, DeploymentEnvironment environment, IEnumerable<string> numbers)
        {
            if (this.autoServedTenants.Contains(tenantId))
            {
                return this.mockRepository.LoadForProduct(tenantId, productId, environment, numbers);
            }
            else
            {
                return this.realRepository.LoadForProduct(tenantId, productId, environment, numbers);
            }
        }

        public void PurgeForProduct(Guid tenantId, Guid productId, DeploymentEnvironment environment)
        {
            if (this.autoServedTenants.Contains(tenantId))
            {
                this.mockRepository.PurgeForProduct(tenantId, productId, environment);
            }
            else
            {
                this.realRepository.PurgeForProduct(tenantId, productId, environment);
            }
        }

        public void ReturnOldPolicyNumberToPool(Guid tenantId, Guid productId, string oldpolicyNumber, DeploymentEnvironment environment)
        {
            throw new NotImplementedException();
        }

        public void Seed(Guid tenantId, Guid productId, DeploymentEnvironment environment)
        {
            if (this.autoServedTenants.Contains(tenantId))
            {
                this.mockRepository.Seed(tenantId, productId, environment);
            }
            else
            {
                this.realRepository.Seed(tenantId, productId, environment);
            }
        }

        public void DeletePolicyNumber(Guid tenantId, Guid productId, string policyNumber, DeploymentEnvironment environment)
        {
            throw new NotImplementedException();
        }

        public string UpdatePolicyNumber(Guid tenantId, Guid productId, string oldNumber, string newNumber, DeploymentEnvironment environment)
        {
            throw new NotImplementedException();
        }

        public int GetAvailableReferenceNumbersCount(Guid tenantId, Guid productId, DeploymentEnvironment environment)
        {
            throw new NotImplementedException();
        }

        public string ConsumeAndSave(IProductContext productContext)
        {
            return this.autoServedTenants.Contains(productContext.TenantId)
               ? this.mockRepository.ConsumeForProduct(productContext.TenantId, productContext.ProductId, productContext.Environment)
               : this.realRepository.ConsumeForProduct(productContext.TenantId, productContext.ProductId, productContext.Environment);
        }

        public void Unconsume(IProductContext productContext, string number)
        {
            throw new NotImplementedException();
        }

        public void UnconsumeAndSave(IProductContext productContext, string number)
        {
            throw new NotImplementedException();
        }

        string IPolicyNumberRepository.ConsumePolicyNumber(Guid tenantId, Guid productId, string newNumber, DeploymentEnvironment environment)
        {
            throw new NotImplementedException();
        }
    }
}

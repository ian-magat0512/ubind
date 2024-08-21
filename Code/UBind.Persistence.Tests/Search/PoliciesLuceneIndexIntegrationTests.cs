// <copyright file="PoliciesLuceneIndexIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.Search
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using FluentAssertions;
    using UBind.Application.Tests;
    using UBind.Domain;
    using UBind.Domain.Product;
    using UBind.Domain.Search;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class PoliciesLuceneIndexIntegrationTests : IDisposable
    {
        private List<DirectoryInfo> directoriesToDelete = new List<DirectoryInfo>();

        public void Dispose()
        {
            this.directoriesToDelete.Clear();
        }

        [Fact]
        public void GeneratePolicyIndexes_AddItemsToIndex_ShouldHaveLatestLiveDirectoryIndex()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var tenantProductModel = this.SetupTenantAndProduct(tenantId, productId);

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.MockMediator.GetTenantByIdOrAliasQuery(tenantProductModel.Tenant);
                var suffixPath = this.GetSuffixPath(DeploymentEnvironment.Staging, tenantProductModel.Tenant.Details.Alias);
                var directory = stack.LucenePolicyRepository.CreateNewLuceneDirectory(suffixPath);
                this.directoriesToDelete.Add(directory);
                using (var indexWriter = stack.LucenePolicyRepository.CreateIndexWriter(directory))
                {
                    List<PolicySearchIndexWriteModel> writeModels = this.GetWriteModels(productId);

                    stack.LucenePolicyRepository.AddItemsToIndex(tenantProductModel.Tenant, DeploymentEnvironment.Staging, writeModels);
                }

                var lucenePolicyDirectory = stack.LucenePolicyRepository.GetLatestLiveIndexDirectory(suffixPath);
                lucenePolicyDirectory.Should().NotBeNull();
            }
        }

        [Fact]
        public void RegeneratePolicyIndexes_AddItemsToRegenerationIndex_ShouldMoveTheRegenerationIndexToLiveIndex()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var tenantProductModel = this.SetupTenantAndProduct(tenantId, productId);

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.MockMediator.GetTenantByIdOrAliasQuery(tenantProductModel.Tenant);
                var suffixPath = this.GetSuffixPath(DeploymentEnvironment.Staging, tenantProductModel.Tenant.Details.Alias);
                var directory = stack.LucenePolicyRepository.CreateRegenerationIndexDirectory(suffixPath);
                using (var indexWriter = stack.LucenePolicyRepository.CreateIndexWriter(directory))
                {
                    List<PolicySearchIndexWriteModel> writeModels = this.GetWriteModels(productId);

                    stack.LucenePolicyRepository.AddItemsToRegenerationIndex(tenantProductModel.Tenant, DeploymentEnvironment.Staging, writeModels);
                    indexWriter.Commit();
                }

                var luceneRegenerationPolicyDirectory = stack.LucenePolicyRepository.GetLatestRegenerationIndexDirectory(suffixPath);
                stack.LucenePolicyRepository.MakeRegenerationIndexTheLiveIndex(tenantProductModel.Tenant, DeploymentEnvironment.Staging);

                var lucenePolicyDirectory = stack.LucenePolicyRepository.GetLatestLiveIndexDirectory(suffixPath);
                luceneRegenerationPolicyDirectory.Should().NotBeNull();
                lucenePolicyDirectory.Should().NotBeNull();
                lucenePolicyDirectory.Name.Should().Be(luceneRegenerationPolicyDirectory.Name);
                this.directoriesToDelete.Add(lucenePolicyDirectory);
            }
        }

        private List<PolicySearchIndexWriteModel> GetWriteModels(Guid productId)
        {
            var writeModels = new List<PolicySearchIndexWriteModel>();

            for (int i = 0; i < 10; i++)
            {
                var policyId = Guid.NewGuid();
                var tenantId = Guid.NewGuid();

                var policyTransactionList = new List<IPolicyTransactionSearchIndexWriteModel>();
                var policyTransaction = new PolicyTransactionSearchIndexWriteModel()
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    QuoteId = Guid.NewGuid(),
                    OrganisationId = Guid.NewGuid(),
                    LastModifiedTicksSinceEpoch = 2000 + i,
                    EffectiveTicksSinceEpoch = 3000 + i,
                    ExpiryTicksSinceEpoch = 4000 + i,
                    PolicyData_FormData = "{Property1:1, Property2:2}",
                    PolicyData_SerializedCalculationResult = "{Property1:1, Property2:2}",
                    ProductId = productId,
                    Discriminator = "NewBusinessTransaction",
                    QuoteNumber = string.Format("quote{0}", i),
                    PolicyId = policyId,
                };

                policyTransactionList.Add(policyTransaction);

                writeModels.Add(new PolicySearchIndexWriteModel()
                {
                    Id = policyId,
                    TenantId = tenantId,
                    OrganisationId = Guid.NewGuid(),
                    LastModifiedTicksSinceEpoch = 1000 + i,
                    InceptionTicksSinceEpoch = 10001 + i,
                    ExpiryTicksSinceEpoch = 10002 + i,
                    CancellationEffectiveTicksSinceEpoch = 10003 + i,
                    IssuedTicksSinceEpoch = 10004 + i,
                    SerializedCalculationResult = "{Property1:1, Property2:2}",
                    CustomerFullName = string.Format("Customer Number:{0}", i),
                    ProductId = productId,
                    CustomerEmail = string.Format("elvien.bustamante+10{0}@ubind.io", i),
                    PolicyNumber = string.Format("policy{0}", i),
                    PolicyTransactionModel = policyTransactionList,
                    ProductName = "Test Product",
                });
            }

            return writeModels;
        }

        private string GetSuffixPath(DeploymentEnvironment environment, string tenantId)
        {
            return string.Format(@"{0}\{1}", tenantId, environment);
        }

        private TenantProductModel SetupTenantAndProduct(Guid tenantId, Guid productId)
        {
            var tenant = TenantFactory.Create(tenantId);
            var product = ProductFactory.Create(tenantId, productId)
                .WithExpirySettings();
            using (var stack0 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack0.TenantRepository.Insert(tenant);
                stack0.ProductRepository.Insert(product);
                stack0.DbContext.SaveChanges();
            }

            return new TenantProductModel
            {
                Tenant = tenant,
                Product = product,
            };
        }

        private class TenantProductModel
        {
#pragma warning disable SA1401 // Fields should be private
            public Tenant Tenant;
            public Product Product;
#pragma warning restore SA1401 // Fields should be private
        }
    }
}

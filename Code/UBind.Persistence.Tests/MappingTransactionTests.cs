// <copyright file="MappingTransactionTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Services.Imports;
    using UBind.Application.Services.Imports.MappingObjects;
    using UBind.Application.Tests;
    using UBind.Application.Tests.Services.Import;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Imports;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class MappingTransactionTests
    {
        private ApplicationStack appStack;

        public MappingTransactionTests()
        {
            var tenant = TenantFactory.Create();
            var product = ProductFactory.Create(tenant.Id, Guid.NewGuid());
            this.appStack = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
        }

        [Fact]
        public async Task MappingTransactionService_CreateExistingPolicy_ThrowException()
        {
            using (var appStack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                ImportBaseParam baseParam = new ImportBaseParam(
                    Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DeploymentEnvironment.Development);
                this.CreateTenantProduct(appStack, baseParam);

                var json = ImportTestData.GeneratePolicyCompleteImportJson("P01");
                var data = JsonConvert.DeserializeObject<ImportData>(json);
                var config = JsonConvert.DeserializeObject<ImportConfiguration>(json);
                var email = "brayden.matters@uol.com.br";
                await this.HandlePolicyAsync(data, config, appStack, baseParam);

                // Act
                Func<Task> act = () => this.HandlePolicyAsync(data, config, appStack, baseParam);

                // Assert
                Assert.True(appStack.DbContext.PersonReadModels.Any(c => c.Email == email));
                Assert.True(appStack.DbContext.Policies.Any());
                await act.Should().ThrowAsync<NotSupportedException>();
            }
        }

        [Fact]
        public async Task MappingTransactionService_CreateCustomerAndPolicyAndClaim_Complete()
        {
            using (var appStack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                ImportBaseParam baseParam = new ImportBaseParam(
                     Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DeploymentEnvironment.Development);
                this.CreateTenantProduct(appStack, baseParam);
                var json = ImportTestData.GenerateCustomerPolicyClaimCompleteImportJson("P02");
                var data = JsonConvert.DeserializeObject<ImportData>(json);
                var config = JsonConvert.DeserializeObject<ImportConfiguration>(json);
                await this.HandleCustomerAsync(data, config, appStack, baseParam);
                await this.HandlePolicyAsync(data, config, appStack, baseParam);
                await this.HandleClaimsAsync(data, config, appStack, baseParam);

                var email = "brayden.matters@uol.com.br";
                Assert.True(appStack.DbContext.PersonReadModels.Any(c => c.Email == email));
                Assert.True(appStack.DbContext.Policies.Any());
                Assert.True(appStack.DbContext.ClaimReadModels.Any());
            }
        }

        [Fact]
        public async Task MappingTransactionService_CreateCustomerAndPolicy_Complete()
        {
            using (var appStack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                ImportBaseParam baseParam = new ImportBaseParam(
                     Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DeploymentEnvironment.Development);
                this.CreateTenantProduct(appStack, baseParam);
                var json = ImportTestData.GenerateCustomerPolicyCompleteImportJson("P03");
                var data = JsonConvert.DeserializeObject<ImportData>(json);
                var config = JsonConvert.DeserializeObject<ImportConfiguration>(json);
                await this.HandleCustomerAsync(data, config, appStack, baseParam);
                await this.HandlePolicyAsync(data, config, appStack, baseParam);

                var email = "brayden.matters@uol.com.br";
                Assert.True(appStack.DbContext.PersonReadModels.Any(c => c.Email == email));
                Assert.True(appStack.DbContext.Policies.Any());
            }
        }

        [Fact]
        public async Task MappingTransactionService_CreateCustomer_Complete()
        {
            using (var appStack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                ImportBaseParam baseParam = new ImportBaseParam(
                     Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DeploymentEnvironment.Development);
                this.CreateTenantProduct(appStack, baseParam);
                var json = ImportTestData.GenerateCustomerCompleteImportJson("asdf@asdf4.com");
                var data = JsonConvert.DeserializeObject<ImportData>(json);
                var config = JsonConvert.DeserializeObject<ImportConfiguration>(json);
                await this.HandleCustomerAsync(data, config, appStack, baseParam);

                var email = "brayden.matters@uol.com.br";
                Assert.True(appStack.DbContext.PersonReadModels.Any(c => c.Email == email));
            }
        }

        [Fact]
        public async Task MappingTransactionService_CreatePolicy_Complete()
        {
            using (var appStack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                ImportBaseParam baseParam = new ImportBaseParam(
                     Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DeploymentEnvironment.Development);
                this.CreateTenantProduct(appStack, baseParam);
                var json = ImportTestData.GeneratePolicyCompleteImportJson("P05");
                var data = JsonConvert.DeserializeObject<ImportData>(json);
                var config = JsonConvert.DeserializeObject<ImportConfiguration>(json);
                await this.HandlePolicyAsync(data, config, appStack, baseParam);

                var email = "brayden.matters@uol.com.br";
                Assert.True(appStack.DbContext.PersonReadModels.Any(c => c.Email == email));
                Assert.True(appStack.DbContext.Policies.Any());
            }
        }

        [Fact]
        public async Task MappingTransactionService_CreateClaim_Complete()
        {
            using (var appStack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                ImportBaseParam baseParam = new ImportBaseParam(
                     Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DeploymentEnvironment.Development);
                this.CreateTenantProduct(appStack, baseParam);
                var policyJson = ImportTestData.GeneratePolicyCompleteImportJson("P06");
                var policyData = JsonConvert.DeserializeObject<ImportData>(policyJson);
                var policyConfig = JsonConvert.DeserializeObject<ImportConfiguration>(policyJson);
                await this.HandlePolicyAsync(policyData, policyConfig, appStack, baseParam);

                var claimJson = ImportTestData.GenerateClaimCompleteImportJson("P06");
                var claimData = JsonConvert.DeserializeObject<ImportData>(claimJson);
                var claimConfig = JsonConvert.DeserializeObject<ImportConfiguration>(claimJson);
                await this.HandleClaimsAsync(claimData, claimConfig, appStack, baseParam);

                var email = "brayden.matters@uol.com.br";

                Assert.True(appStack.DbContext.PersonReadModels.Any(c => c.Email == email));
                Assert.True(appStack.DbContext.Policies.Any());
                Assert.True(appStack.DbContext.ClaimReadModels.Any());
            }
        }

        [Fact]
        public async Task MappingTransactionService_UpdateClaim_Complete()
        {
            using (var appStack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                ImportBaseParam baseParam = new ImportBaseParam(
                     Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DeploymentEnvironment.Development);
                this.CreateTenantProduct(appStack, baseParam);
                var policyJson = ImportTestData.GeneratePolicyCompleteImportJson("P07");
                var policyData = JsonConvert.DeserializeObject<ImportData>(policyJson);
                var policyConfig = JsonConvert.DeserializeObject<ImportConfiguration>(policyJson);
                await this.HandlePolicyAsync(policyData, policyConfig, appStack, baseParam);

                var claimJson = ImportTestData.GenerateClaimCompleteImportJson("P07");
                var claimData = JsonConvert.DeserializeObject<ImportData>(claimJson);
                var claimConfig = JsonConvert.DeserializeObject<ImportConfiguration>(claimJson);
                await this.HandleClaimsAsync(claimData, claimConfig, appStack, baseParam);

                var claimReadModels = this.appStack.DbContext.ClaimReadModels
                    .Where(x => x.TenantId == baseParam.TenantId
                        && x.ProductId == baseParam.ProductId
                        && x.Description == "Sample description for P0003").ToList();
                var oldDescription = claimReadModels
                    .LastOrDefault()
                    .Description;
                var oldIncidentDate = claimReadModels
                    .LastOrDefault()
                    .IncidentDateTime;

                var updatedClaimJson = ImportTestData.GenerateClaimCompleteImportJsonV2("P07");
                var updatedClaimData = JsonConvert.DeserializeObject<ImportData>(updatedClaimJson);
                var updatedClaimConfig = JsonConvert.DeserializeObject<ImportConfiguration>(updatedClaimJson);
                await this.HandleClaimsAsync(updatedClaimData, updatedClaimConfig, appStack, baseParam);

                claimReadModels = this.appStack.DbContext.ClaimReadModels
                     .Where(x => x.TenantId == baseParam.TenantId
                         && x.ProductId == baseParam.ProductId
                         && x.Description == "New description").ToList();
                var newDescription = claimReadModels
                    .LastOrDefault()
                    .Description;
                var newIncidentDate = claimReadModels
                    .LastOrDefault()
                    .IncidentDateTime;

                var email = "brayden.matters@uol.com.br";
                Assert.True(appStack.DbContext.PersonReadModels.Any(c => c.Email == email));
                Assert.True(appStack.DbContext.Policies.Any());
                Assert.True(appStack.DbContext.ClaimReadModels.Any());
                Assert.NotEqual(oldDescription, newDescription);
                Assert.NotEqual(oldIncidentDate, newIncidentDate);
            }
        }

        [Fact]
        public async Task HandlePolicyAsync_ShouldAssignOwner_WhenAgentEmailIsAvailable()
        {
            using (var appStack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Arrange
                ImportBaseParam baseParam = new ImportBaseParam(
                     Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DeploymentEnvironment.Development);
                this.CreateTenantProduct(appStack, baseParam);
                var agentEmail = "leon.tayson@ubind.io";
                var policyNumber = "P0322";
                var json = ImportTestData.GeneratePolicyJson(policyNumber, agentEmail);
                var data = JsonConvert.DeserializeObject<ImportData>(json);
                var config = JsonConvert.DeserializeObject<ImportConfiguration>(json);

                var (user, person) = await this.CreateUser(baseParam, agentEmail, UserType.Client);

                // Act
                await this.HandleCustomerAsync(data, config, appStack, baseParam);
                await this.HandlePolicyAsync(data, config, appStack, baseParam);

                // Assert
                var createdPolicy = appStack.DbContext.Policies.First(p => p.PolicyNumber == policyNumber);
                createdPolicy.Should().NotBeNull();
                createdPolicy.OwnerUserId.Should().Be(user.Id);
                createdPolicy.OwnerPersonId.Should().Be(user.PersonId);
                createdPolicy.OwnerFullName.Should().Be(person.FullName);
            }
        }

        public async Task HandlePolicyAsync_ShouldThrowException_WhenAgentEmailIsNotAnAgentUserType()
        {
            using (var appStack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Arrange
                ImportBaseParam baseParam = new ImportBaseParam(
                     Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DeploymentEnvironment.Development);
                this.CreateTenantProduct(appStack, baseParam);
                var agentEmail = "leon.tayson@ubind.io";
                var policyNumber = "P0322";
                var json = ImportTestData.GeneratePolicyJson(policyNumber, agentEmail);
                var data = JsonConvert.DeserializeObject<ImportData>(json);
                var config = JsonConvert.DeserializeObject<ImportConfiguration>(json);

                var (user, person) = await this.CreateUser(baseParam, agentEmail, UserType.Customer);

                // Act & Assert
                await this.HandleCustomerAsync(data, config, appStack, baseParam);
                await Assert.ThrowsAsync<ErrorException>(() => this.HandlePolicyAsync(data, config, appStack, baseParam));
            }
        }

        public async Task HandlePolicyAsync_ShouldNotAssignOwner_WhenAgentEmailIsNotProvided()
        {
            using (var appStack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Arrange
                ImportBaseParam baseParam = new ImportBaseParam(
                     Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DeploymentEnvironment.Development);
                this.CreateTenantProduct(appStack, baseParam);
                var agentEmail = string.Empty;
                var policyNumber = "P0322";
                var json = ImportTestData.GeneratePolicyJson(policyNumber, agentEmail);
                var data = JsonConvert.DeserializeObject<ImportData>(json);
                var config = JsonConvert.DeserializeObject<ImportConfiguration>(json);

                var (user, person) = await this.CreateUser(baseParam, agentEmail, UserType.Customer);

                // Act
                await this.HandleCustomerAsync(data, config, appStack, baseParam);
                await this.HandlePolicyAsync(data, config, appStack, baseParam);

                // Assert
                var createdPolicy = appStack.DbContext.Policies.First(p => p.PolicyNumber == policyNumber);
                createdPolicy.Should().NotBeNull();
                createdPolicy.OwnerUserId.Should().BeNull();
                createdPolicy.OwnerPersonId.Should().BeNull();
                createdPolicy.OwnerFullName.Should().BeNull();
            }
        }

        private async Task<(UserAggregate, PersonAggregate)> CreateUser(
            ImportBaseParam baseParam, string emailAddress, UserType userType)
        {
            var performingUserId = this.appStack.HttpContextPropertiesResolver.PerformingUserId;
            var personOne = this.CreatePerson(baseParam.TenantId, emailAddress);
            var personAggregate = PersonAggregate.CreatePersonFromPersonalDetails(
                    baseParam.TenantId, Guid.NewGuid(), personOne, performingUserId, this.appStack.Clock.Now());
            await this.appStack.PersonAggregateRepository.Save(personAggregate);
            var userAggregate = UserAggregate.CreateUser(
                baseParam.TenantId, Guid.NewGuid(), UserType.Customer, personAggregate, performingUserId, null, this.appStack.Clock.Now());
            userAggregate.SetLoginEmail(emailAddress, performingUserId, this.appStack.Clock.Now());
            userAggregate.ChangeUserType(userType, performingUserId, this.appStack.Clock.Now(), true);
            await this.appStack.UserAggregateRepository.Save(userAggregate);
            return (userAggregate, personAggregate);
        }

        private FakePersonalDetails CreatePerson(Guid tenantId, string emailAddress)
        {
            var person = new FakePersonalDetails
            {
                TenantId = tenantId,
                FullName = "Customer test One",
                FirstName = "Customer",
                MiddleNames = "test",
                LastName = "One",
                NamePrefix = "Dr",
                NameSuffix = "Jr",
                PreferredName = "customerOne",
                Email = emailAddress,
            };

            return person;
        }

        private async Task HandleCustomerAsync(ImportData importData, ImportConfiguration importConfiguration, ApplicationStack appStack, ImportBaseParam baseParam)
        {
            foreach (JObject entry in importData.Data)
            {
                CustomerImportData customerData = null;
                if (importConfiguration.CustomerMapping != null)
                {
                    customerData = new CustomerImportData(baseParam.TenantId, baseParam.OrganisationId, entry, importConfiguration.CustomerMapping);
                }

                await appStack.MappingTransactionService.HandleCustomers(
                    appStack.ProgressLogger, baseParam, customerData);
            }
        }

        private async Task HandlePolicyAsync(ImportData importData, ImportConfiguration importConfiguration, ApplicationStack appStack, ImportBaseParam baseParam)
        {
            foreach (JObject entry in importData.Data)
            {
                PolicyImportData policyImportData = null;
                if (importConfiguration.PolicyMapping != null)
                {
                    policyImportData = new PolicyImportData(entry, importConfiguration.PolicyMapping);
                }

                await appStack.MappingTransactionService.HandlePolicies(
                    appStack.ProgressLogger, baseParam, policyImportData);
            }
        }

        private async Task HandleClaimsAsync(ImportData importData, ImportConfiguration importConfiguration, ApplicationStack appStack, ImportBaseParam baseParam)
        {
            foreach (JObject entry in importData.Data)
            {
                ClaimImportData claimImportData = null;
                if (importConfiguration.ClaimMapping != null)
                {
                    claimImportData = new ClaimImportData(entry, importConfiguration.ClaimMapping);
                }

                await appStack.MappingTransactionService.HandleClaims(
                    appStack.ProgressLogger, baseParam, claimImportData);
            }
        }

        private void CreateTenantProduct(ApplicationStack appStack, ImportBaseParam baseParam)
        {
            var tenant = new Tenant(baseParam.TenantId, default, baseParam.TenantId.ToString(), null, default, default, appStack.Clock.Now());
            appStack.TenantRepository.Insert(tenant);
            var product = new Domain.Product.Product(tenant.Id, baseParam.ProductId, default, baseParam.ProductId.ToString(), appStack.Clock.Now());
            appStack.ProductRepository.Insert(product);
            appStack.TenantRepository.SaveChanges();
            appStack.ProductRepository.SaveChanges();
            appStack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
            appStack.MockMediator.GetProductByIdOrAliasQuery(product);
        }
    }
}

// <copyright file="ImportMappingTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Services.Import
{
    using System;
    using FluentAssertions;
    using Newtonsoft.Json;
    using UBind.Application.Services.Imports;
    using UBind.Application.Services.Imports.MappingObjects;
    using UBind.Domain;
    using Xunit;

    public class ImportMappingTests
    {
        private readonly ImportBaseParam baseParam = new ImportBaseParam(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DeploymentEnvironment.Development);

        [Fact]
        public void JsonMapping_MapsCustomer_WhenJsonPayloadMeetsAllRequirements()
        {
            var json = ImportTestData.GenerateCustomerCompleteImportJson();
            var data = JsonConvert.DeserializeObject<ImportData>(json);
            var config = JsonConvert.DeserializeObject<ImportConfiguration>(json);
            var container = new ImportDataContainer(this.baseParam, data, config);

            Assert.NotNull(data);
            Assert.NotNull(config.CustomerMapping);
            Assert.Null(config.PolicyMapping);
            Assert.Null(config.ClaimMapping);
            Assert.NotNull(container);
        }

        [Fact]
        public void JsonMapping_ThrowExceptionOnMapCustomer_WhenJsonPayloadDoesntMeetAllRequirements()
        {
            // Arrange
            var json = ImportTestData.GenerateCustomerIncompleteImportJson();
            var data = JsonConvert.DeserializeObject<ImportData>(json);

            // Act
            Func<ImportConfiguration> act = () => JsonConvert.DeserializeObject<ImportConfiguration>(json);

            // Assert
            Assert.NotNull(data);
            act.Should().Throw<JsonSerializationException>();
        }

        [Fact]
        public void JsonMapping_MapsPolicy_WhenJsonPayloadMeetsAllRequirements()
        {
            var json = ImportTestData.GeneratePolicyCompleteImportJson();
            var data = JsonConvert.DeserializeObject<ImportData>(json);
            var config = JsonConvert.DeserializeObject<ImportConfiguration>(json);
            var container = new ImportDataContainer(this.baseParam, data, config);

            Assert.NotNull(data);
            Assert.Null(config.CustomerMapping);
            Assert.NotNull(config.PolicyMapping);
            Assert.Null(config.ClaimMapping);
            Assert.NotNull(container);
        }

        [Fact]
        public void JsonMapping_ThrowExceptionOnMapPolicy_WhenJsonPayloadDoesntMeetAllRequirements()
        {
            // Arrange
            var json = ImportTestData.GeneratePolicyIncompleteImportJson();
            var data = JsonConvert.DeserializeObject<ImportData>(json);

            // Act
            Func<ImportConfiguration> act = () => JsonConvert.DeserializeObject<ImportConfiguration>(json);

            // Assert
            Assert.NotNull(data);
            act.Should().Throw<JsonSerializationException>();
        }

        [Fact]
        public void JsonMapping_MapsClaim_WhenJsonPayloadMeetsAllRequirements()
        {
            var json = ImportTestData.GenerateClaimCompleteImportJson();
            var data = JsonConvert.DeserializeObject<ImportData>(json);
            var config = JsonConvert.DeserializeObject<ImportConfiguration>(json);
            var container = new ImportDataContainer(this.baseParam, data, config);

            Assert.NotNull(data);
            Assert.NotNull(config.ClaimMapping);
            Assert.NotNull(container);
        }

        [Fact]
        public void JsonMapping_ThrowExceptionOnMapClaim_WhenJsonPayloadDoesntMeetAllRequirements()
        {
            // Arrange
            var json = ImportTestData.GenerateClaimIncompleteImportJson();
            var data = JsonConvert.DeserializeObject<ImportData>(json);

            // Act
            Func<ImportConfiguration> act = () => JsonConvert.DeserializeObject<ImportConfiguration>(json);

            // Assert
            Assert.NotNull(data);
            act.Should().Throw<JsonSerializationException>();
        }
    }
}

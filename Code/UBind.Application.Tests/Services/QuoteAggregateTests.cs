// <copyright file="QuoteAggregateTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Services
{
    using System;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Application.Services.Imports;
    using UBind.Application.Services.Imports.MappingObjects;
    using UBind.Application.Tests.Services.Import;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Imports;
    using UBind.Domain.Loggers;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    /// <summary>
    /// Tests for QuoteAggregates.
    /// </summary>
    public class QuoteAggregateTests
    {
        private readonly Guid? performingUserId = Guid.NewGuid();
        private readonly ImportBaseParam baseParam = new ImportBaseParam(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DeploymentEnvironment.Development);

        private readonly Mock<IPolicyTransactionTimeOfDayScheme> limitTimes = new Mock<IPolicyTransactionTimeOfDayScheme>();

        private IProgressLogger progressLogger;

        public QuoteAggregateTests()
        {
            this.progressLogger = new Mock<IProgressLogger>().Object;
        }

        [Fact]
        public void QuoteAggregate_CreateImportedPolicy_Should_Include_CustomerId()
        {
            // Arrange
            var json = ImportTestData.GeneratePolicyCompleteImportJson();
            var data = JsonConvert.DeserializeObject<ImportData>(json);
            var config = JsonConvert.DeserializeObject<ImportConfiguration>(json);
            var customerId = Guid.NewGuid();
            var policyImportData = new PolicyImportData((JObject)data.Data[0], config.PolicyMapping);

            // Act
            var aggregate = QuoteAggregate.CreateImportedPolicy(
                    this.baseParam.TenantId,
                    this.baseParam.OrganisationId,
                    this.baseParam.ProductId,
                    this.baseParam.Environment,
                    Guid.NewGuid(),
                    customerId,
                    It.IsAny<IPersonalDetails>(),
                    policyImportData,
                    Timezones.AET,
                    this.limitTimes.Object,
                    this.performingUserId,
                    Instant.MinValue,
                    Guid.NewGuid());

            // Assert
            aggregate.CustomerId.Should().Be(customerId);
        }
    }
}

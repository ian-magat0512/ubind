// <copyright file="IQueryableExtensionsTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.ReadWriteModel.Email;
    using UBind.Domain.Tests.Attributes;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class IQueryableExtensionsTests
    {
        [Fact]
        public void Paginate_ShouldReturnExpectedPageItemCount_WhenPageSizeAndPageWasDefined()
        {
            // Arrange
            var emailTagRelationshipItems = new List<EmailTagRelationshipModelTest>();
            int totalItem = 15;

            for (int i = 0; i < totalItem; i++)
            {
                var emailAddress = "xxx+1@email.com";
                var sampleStringList = new List<string>() { emailAddress };
                var email = new Email(
                    TenantFactory.DefaultId,
                    Guid.NewGuid(),
                    null,
                    DeploymentEnvironment.Development,
                    Guid.NewGuid(),
                    sampleStringList,
                    emailAddress,
                    sampleStringList,
                    sampleStringList,
                    sampleStringList,
                    "test",
                    "test",
                    "test",
                    null,
                    new TestClock().Timestamp);
                emailTagRelationshipItems.Add(new EmailTagRelationshipModelTest()
                {
                    Email = email,
                    Relationships = Enumerable.Empty<Relationship>(),
                    Tags = Enumerable.Empty<Tag>(),
                });
            }

            var pageOnefilter = new EntityListFilters
            {
                PageSize = 10,
                Page = 1,
            };

            var pageTwofilter = new EntityListFilters
            {
                PageSize = 10,
                Page = 2,
            };

            var queryableItems = emailTagRelationshipItems.AsQueryable();

            // Act
            var pageOneFilteResult = queryableItems.Paginate(pageOnefilter);
            var pageTwoFilterResult = queryableItems.Paginate(pageTwofilter);

            // Assert
            pageOneFilteResult.Count().Should().Be(10);
            pageTwoFilterResult.Count().Should().Be(5);
        }

        [Fact]
        public void Paginate_ShouldReturnDefaultPageSize_WhenPageSizeAndPageWasNotDefined()
        {
            // Arrange
            var emailTagRelationshipItems = new List<EmailTagRelationshipModelTest>();
            int totalItem = 1010;

            for (int i = 0; i < totalItem; i++)
            {
                var emailAddress = "xxx+1@email.com";
                var sampleStringList = new List<string>() { emailAddress };
                var email = new Email(
                    TenantFactory.DefaultId,
                    Guid.NewGuid(),
                    null,
                    DeploymentEnvironment.Development,
                    Guid.NewGuid(),
                    sampleStringList,
                    emailAddress,
                    sampleStringList,
                    sampleStringList,
                    sampleStringList,
                    "test",
                    "test",
                    "test",
                    null,
                    new TestClock().Timestamp);
                emailTagRelationshipItems.Add(new EmailTagRelationshipModelTest()
                {
                    Email = email,
                    Relationships = Enumerable.Empty<Relationship>(),
                    Tags = Enumerable.Empty<Tag>(),
                });
            }

            var filter = new EntityListFilters();

            var queryableItems = emailTagRelationshipItems.AsQueryable();

            // Act
            var filterResult = queryableItems.Paginate(filter);

            // Assert
            filterResult.Count().Should().Be(1000);
        }

        [SkipDuringLeapDay]
        public async Task Order_ReturnTheCorrectSortByExpiryDate_WhenOrderByDescending()
        {
            // Arrange
            var formDataJson = string.Empty;
            var calculationResultJson = string.Empty;

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var tenant = TenantFactory.Create(Guid.NewGuid());
                var product = ProductFactory.Create(tenant.Id, null);
                stack.CreateTenant(tenant);
                stack.CreateProduct(product);

                var date = stack.Clock.Now().ToLocalDateInAet();
                var inceptionDate = new LocalDate(date.Year, date.Month, date.Day);

                // Act (IssuePolicy() call inside CreateNewPolicy.)
                QuoteFactory.Clock = stack.Clock;

                tenant = stack.TenantRepository.GetTenantById(tenant.Id);
                var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null, Guid.NewGuid(),
                    stack.Clock.GetCurrentInstant());
                tenant.SetDefaultOrganisation(organisation.Id, stack.Clock.Now().Plus(Duration.FromMinutes(1)));
                stack.TenantRepository.SaveChanges();
                await stack.OrganisationAggregateRepository.Save(organisation);

                for (int i = 0; i < 10; i++)
                {
                    if (i < 2)
                    {
                        formDataJson = FormDataJsonFactory.GetSampleWithStartAndEndDates(inceptionDate, 60);
                        calculationResultJson = CalculationResultJsonFactory.Create(startDate: inceptionDate, durationInDays: 1825);
                    }
                    else if (i < 4)
                    {
                        formDataJson = FormDataJsonFactory.GetSampleWithStartAndEndDates(inceptionDate, 48);
                        calculationResultJson = CalculationResultJsonFactory.Create(startDate: inceptionDate, durationInDays: 1460);
                    }
                    else if (i < 6)
                    {
                        formDataJson = FormDataJsonFactory.GetSampleWithStartAndEndDates(inceptionDate, 36);
                        calculationResultJson = CalculationResultJsonFactory.Create(startDate: inceptionDate, durationInDays: 1095);
                    }
                    else if (i < 8)
                    {
                        formDataJson = FormDataJsonFactory.GetSampleWithStartAndEndDates(inceptionDate, 24);
                        calculationResultJson = CalculationResultJsonFactory.Create(startDate: inceptionDate, durationInDays: 730);
                    }
                    else
                    {
                        formDataJson = FormDataJsonFactory.GetSampleWithStartAndEndDates(inceptionDate, 12);
                        calculationResultJson = CalculationResultJsonFactory.Create(startDate: inceptionDate, durationInDays: 365);
                    }

                    var aggregate = QuoteFactory.CreateNewPolicy(
                         tenant.Id,
                         product.Id,
                         formDataJson: formDataJson,
                         calculationResultJson: calculationResultJson,
                         organisationId: organisation.Id);
                    await stack.QuoteAggregateRepository.Save(aggregate);
                }

                // Act
                var fakeFilter = new PolicyReadModelFilters
                {
                    TenantId = tenant.Id,
                    Page = 1,
                    PageSize = 50,
                    IncludeTestData = true,
                    Environment = DeploymentEnvironment.Staging,
                    SortBy = nameof(PolicyReadModel.ExpiryTicksSinceEpoch),
                    SortOrder = SortDirection.Descending,
                };
                var policyList = stack.PolicyReadModelRepository.ListPolicies(tenant.Id, fakeFilter).ToList();

                // Assert
                policyList.FirstOrDefault().ExpiryDateTime.Value.Date.Should().Be(new LocalDate(date.Year + 5, date.Month, date.Day));
                policyList.LastOrDefault().ExpiryDateTime.Value.Date.Should().Be(new LocalDate(date.Year + 1, date.Month, date.Day));
            }
        }

        [SkipDuringLeapDay]
        public async Task Order_ReturnTheCorrectSortByExpiryDate_WhenOrderByAscending()
        {
            // Arrange
            var tenant = TenantFactory.Create(Guid.NewGuid());
            var product = ProductFactory.Create(tenant.Id, null);
            var formDataJson = string.Empty;
            var calculationResultJson = string.Empty;

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var date = stack.Clock.Now().ToLocalDateInAet();
                var inceptionDate = new LocalDate(date.Year, date.Month, date.Day);

                stack.CreateTenant(tenant);
                stack.CreateProduct(product);

                // Act (IssuePolicy() call inside CreateNewPolicy.)
                QuoteFactory.Clock = stack.Clock;

                tenant = stack.TenantRepository.GetTenantById(tenant.Id);
                var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null, Guid.NewGuid(),
                    stack.Clock.GetCurrentInstant());
                tenant.SetDefaultOrganisation(organisation.Id, stack.Clock.Now().Plus(Duration.FromMinutes(1)));
                stack.TenantRepository.SaveChanges();
                await stack.OrganisationAggregateRepository.Save(organisation);

                for (int i = 0; i < 10; i++)
                {
                    if (i < 2)
                    {
                        formDataJson = FormDataJsonFactory.GetSampleWithStartAndEndDates(inceptionDate, 60);
                        calculationResultJson = CalculationResultJsonFactory.Create(startDate: inceptionDate, durationInDays: 1825);
                    }
                    else if (i < 4)
                    {
                        formDataJson = FormDataJsonFactory.GetSampleWithStartAndEndDates(inceptionDate, 48);
                        calculationResultJson = CalculationResultJsonFactory.Create(startDate: inceptionDate, durationInDays: 1460);
                    }
                    else if (i < 6)
                    {
                        formDataJson = FormDataJsonFactory.GetSampleWithStartAndEndDates(inceptionDate, 36);
                        calculationResultJson = CalculationResultJsonFactory.Create(startDate: inceptionDate, durationInDays: 1095);
                    }
                    else if (i < 8)
                    {
                        formDataJson = FormDataJsonFactory.GetSampleWithStartAndEndDates(inceptionDate, 24);
                        calculationResultJson = CalculationResultJsonFactory.Create(startDate: inceptionDate, durationInDays: 730);
                    }
                    else
                    {
                        formDataJson = FormDataJsonFactory.GetSampleWithStartAndEndDates(inceptionDate, 12);
                        calculationResultJson = CalculationResultJsonFactory.Create(startDate: inceptionDate, durationInDays: 365);
                    }

                    var aggregate = QuoteFactory.CreateNewPolicy(
                         tenant.Id,
                         product.Id,
                         formDataJson: formDataJson,
                         calculationResultJson: calculationResultJson,
                         organisationId: organisation.Id);
                    await stack.QuoteAggregateRepository.Save(aggregate);
                }

                // Act
                var fakeFilter = new PolicyReadModelFilters
                {
                    TenantId = tenant.Id,
                    Page = 1,
                    PageSize = 50,
                    IncludeTestData = true,
                    Environment = DeploymentEnvironment.Staging,
                    SortBy = nameof(PolicyReadModel.ExpiryTicksSinceEpoch),
                    SortOrder = SortDirection.Ascending,
                };
                var policyList = stack.PolicyReadModelRepository.ListPolicies(tenant.Id, fakeFilter).ToList();

                // Assert
                policyList.FirstOrDefault().ExpiryDateTime.Value.Date
                    .Should().Be(new LocalDate(date.Year + 1, date.Month, date.Day));
                policyList.LastOrDefault().ExpiryDateTime.Value.Date
                    .Should().Be(new LocalDate(date.Year + 5, date.Month, date.Day));
            }
        }

        private class EmailTagRelationshipModelTest
        {
            public Email Email { get; set; }

            public IEnumerable<Relationship> Relationships { get; set; }

            public IEnumerable<Tag> Tags { get; set; }
        }
    }
}

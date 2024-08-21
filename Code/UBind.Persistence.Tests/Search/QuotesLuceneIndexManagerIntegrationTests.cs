// <copyright file="QuotesLuceneIndexManagerIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Search.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Lucene.Net.Index;
    using NodaTime;
    using UBind.Application.Tests;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Search;
    using UBind.Domain.Tests.Fakes;
    using UBind.Domain.Tests.Helpers;
    using UBind.Domain.ValueTypes;
    using UBind.Persistence.Tests;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class QuotesLuceneIndexManagerIntegrationTests
    {
        private static readonly string QuoteStateExpired = StandardQuoteStates.Expired.ToLower();
        private static readonly string QuoteStateComplete = StandardQuoteStates.Complete.ToLower();
        private static readonly string QuoteStateIncomplete = StandardQuoteStates.Incomplete.ToLower();
        private List<DirectoryInfo> directoriesToDelete = new List<DirectoryInfo>();

        public void Dispose()
        {
            this.directoriesToDelete.ForEach(d => d.Delete(true));
            this.directoriesToDelete.Clear();
        }

        [Fact(Skip = "This is intermittenly failing on the CI server, due to a database failure. Created ticket UB-9156 to address it.")]
        public void GenerateIndexes_AllowsSearch_WhileDoingIndexUpdates()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var tenantProductModel = this.SetupTenantAndProduct(tenantId, productId);
            Guid? performingUserId = Guid.NewGuid();

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.MockMediator.GetTenantByIdOrAliasQuery(tenantProductModel.Tenant);
                var suffixPath = this.GetSuffixPath(tenantProductModel.Tenant.Details.Alias, DeploymentEnvironment.Staging);
                var directory = stack.LuceneQuoteRepository.CreateNewLuceneDirectory(suffixPath);
                this.directoriesToDelete.Add(directory);
                using (var indexWriter = stack.LuceneQuoteRepository.CreateIndexWriter(directory))
                {
                    List<QuoteSearchIndexWriteModel> writeModels = this.GetWriteModels(productId);

                    stack.LuceneQuoteRepository.AddItemsToIndex(tenantProductModel.Tenant, DeploymentEnvironment.Staging, writeModels);
                    this.UpdateWriteModels(writeModels, "complete");

                    IEnumerable<IQuoteSearchResultItemReadModel> searchResults;
                    var filters = new QuoteReadModelFilters()
                    {
                        Statuses = new List<string> { "Incomplete", "Complete" },
                    };

                    directory = stack.LuceneQuoteRepository.GetLatestLiveIndexDirectory(suffixPath);
                    IEnumerable<IQuoteSearchResultItemReadModel> incompletes;
                    var indexReader = stack.LuceneQuoteRepository.CreateIndexSearcher(directory);
                    searchResults = stack.LuceneQuoteRepository.Search(tenantProductModel.Tenant, DeploymentEnvironment.Staging, filters);
                    searchResults.Any(x => x.QuoteState == "complete").Should().BeFalse();

                    incompletes = searchResults.Where(x => x.QuoteState == "incomplete");

                    incompletes.Should().HaveCount(20);

                    // Act
                    var taskUpdateIndexes = Task.Run((System.Action)(() =>
                    {
                        stack.LuceneQuoteRepository.AddItemsToIndex(tenantProductModel.Tenant, DeploymentEnvironment.Staging, writeModels);
                    }));

                    var taskSearchIndexes = Task.Run(() =>
                    {
                        var indexReader = stack.LuceneQuoteRepository.CreateIndexSearcher(directory);
                        searchResults = stack.LuceneQuoteRepository.Search(tenantProductModel.Tenant, DeploymentEnvironment.Staging, filters);
                    });

                    // Assert
                    searchResults.Any(x => x.QuoteState == "complete").Should().BeFalse();

                    incompletes = searchResults.Where(x => x.QuoteState == "incomplete");
                    incompletes.Should().HaveCount(20);

                    Task.WaitAll(taskUpdateIndexes, taskSearchIndexes);

                    indexReader = stack.LuceneQuoteRepository.CreateIndexSearcher(directory);
                    searchResults = stack.LuceneQuoteRepository.Search(tenantProductModel.Tenant, DeploymentEnvironment.Staging, filters);

                    searchResults.Any(x => x.QuoteState == "incomplete").Should().BeFalse();

                    var completes = searchResults.Where(x => x.QuoteState == "complete");
                    completes.Should().HaveCount(20);
                }
            }
        }

        [Fact]
        public void Search_Return_DataBySearchTerm()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var tenantProductModel = this.SetupTenantAndProduct(tenantId, productId);

            var organisationId = Guid.NewGuid();
            var fakeWriteModels = new List<QuoteSearchIndexWriteModel>
            {
                new QuoteSearchIndexWriteModel()
                {
                    Id = Guid.NewGuid(),
                    LastModifiedTicksSinceEpoch = 10000,
                    LastModifiedByUserTicksSinceEpoch = 10000 + 10,
                    CreatedTicksSinceEpoch = 10000,
                    FormDataJson = "{Property1:1, Property2:2, Property3:'my.email@ubind.io'}",
                    ProductId = productId,
                    QuoteState = QuoteStateComplete,
                    QuoteType = QuoteType.NewBusiness,
                    QuoteNumber = "TUWBOI",
                    OrganisationId = organisationId,
                    ProductName = "Test Product",
                    PolicyNumber = null,
                },

                new QuoteSearchIndexWriteModel()
                {
                    Id = Guid.NewGuid(),
                    LastModifiedTicksSinceEpoch = 10001,
                    LastModifiedByUserTicksSinceEpoch = 10001 + 10,

                    CreatedTicksSinceEpoch = 10000,
                    FormDataJson = "{Property1:'self-employed', Property2:2, Property3:'my.email123@ubind.io'}",
                    ProductId = productId,
                    QuoteState = QuoteStateComplete,
                    QuoteType = QuoteType.NewBusiness,
                    QuoteNumber = "PETE",
                    OrganisationId = Guid.NewGuid(),
                    ProductName = "Test Product",
                    PolicyNumber = "P-0001",
                },

                new QuoteSearchIndexWriteModel()
                {
                    Id = Guid.NewGuid(),
                    LastModifiedTicksSinceEpoch = 10002,
                    CreatedTicksSinceEpoch = 10000,
                    LastModifiedByUserTicksSinceEpoch = 10002 + 10,
                    FormDataJson = "{Property1:1, Property2:2, Property3:'kelly.chiu@test.com'}",
                    ExpiryTicksSinceEpoch = 100000000000000,
                    ProductId = productId,
                    QuoteState = QuoteStateIncomplete,
                    QuoteType = QuoteType.NewBusiness,
                    QuoteNumber = "VHALEN",
                    OrganisationId = Guid.NewGuid(),
                    ProductName = "Test Product",
                    PolicyNumber = "P-0002",
                },
            };

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.MockMediator.GetProductByIdOrAliasQuery(tenantProductModel.Product);
                stack.MockMediator.GetTenantByIdOrAliasQuery(tenantProductModel.Tenant);

                // Arrange
                var suffixPath = this.GetSuffixPath(tenantProductModel.Tenant.Details.Alias, DeploymentEnvironment.Staging);
                var directory = stack.LuceneQuoteRepository.CreateNewLuceneDirectory(suffixPath);
                this.directoriesToDelete.Add(directory);
                using (var indexWriter = stack.LuceneQuoteRepository.CreateIndexWriter(directory))
                {
                    stack.LuceneQuoteRepository.AddItemsToIndex(tenantProductModel.Tenant, DeploymentEnvironment.Staging, fakeWriteModels);

                    // Searching by exact policy number should return only one result
                    var searcher = stack.LuceneQuoteRepository.CreateIndexSearcher(directory);
                    var filters = new QuoteReadModelFilters()
                    {
                        SearchTerms = new List<string> { "P-0001" },
                    };
                    var searchResults = stack.LuceneQuoteRepository.Search(tenantProductModel.Tenant, DeploymentEnvironment.Staging, filters).ToList();
                    searchResults.Should().HaveCount(1);

                    // Searching by partial policy number should return two result
                    searcher = stack.LuceneQuoteRepository.CreateIndexSearcher(directory);
                    filters = new QuoteReadModelFilters()
                    {
                        SearchTerms = new List<string> { "P-00" },
                    };
                    searchResults = stack.LuceneQuoteRepository.Search(tenantProductModel.Tenant, DeploymentEnvironment.Staging, filters).ToList();
                    searchResults.Should().HaveCount(2);

                    // Searching by exact email address should return only one result
                    searcher = stack.LuceneQuoteRepository.CreateIndexSearcher(directory);
                    filters = new QuoteReadModelFilters()
                    {
                        SearchTerms = new List<string> { "my.email@ubind.io" },
                    };
                    searchResults = stack.LuceneQuoteRepository.Search(tenantProductModel.Tenant, DeploymentEnvironment.Staging, filters).ToList();
                    searchResults.Should().HaveCount(1);

                    // Searching by partial email address should return 2 result
                    searcher = stack.LuceneQuoteRepository.CreateIndexSearcher(directory);
                    filters = new QuoteReadModelFilters()
                    {
                        SearchTerms = new List<string> { "my.email" },
                    };
                    searchResults = stack.LuceneQuoteRepository.Search(tenantProductModel.Tenant, DeploymentEnvironment.Staging, filters).ToList();
                    searchResults.Should().HaveCount(2);
                }
            }
        }

        [Fact]
        public void Search_Return_ProperValues()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var tenantProductModel = this.SetupTenantAndProduct(tenantId, productId);

            var organisationId = Guid.NewGuid();
            var fakeWriteModels = new List<QuoteSearchIndexWriteModel>
            {
                new QuoteSearchIndexWriteModel()
                {
                    Id = Guid.NewGuid(),
                    LastModifiedTicksSinceEpoch = 10000,
                    LastModifiedByUserTicksSinceEpoch = 10000 + 10,
                    CreatedTicksSinceEpoch = 10000,
                    FormDataJson = "{Property1:1, Property2:2}",
                    CustomerFullname = "Donna summers",
                    ProductId = productId,
                    CustomerEmail = "isagani.lastra+10donna@ubind.io",
                    QuoteState = QuoteStateComplete,
                    QuoteType = QuoteType.NewBusiness,
                    QuoteNumber = "TUWBOI",
                    OrganisationId = organisationId,
                    ProductName = "Test Product",
                },

                new QuoteSearchIndexWriteModel()
                {
                    Id = Guid.NewGuid(),
                    LastModifiedTicksSinceEpoch = 10001,
                    LastModifiedByUserTicksSinceEpoch = 10001 + 10,

                    CreatedTicksSinceEpoch = 10000,
                    FormDataJson = "{Property1:'self-employed', Property2:2}",
                    CustomerFullname = "PETE SAMPRAS",
                    ProductId = productId,
                    CustomerEmail = "isagani.lastra+10PETE@ubind.io",
                    QuoteState = QuoteStateComplete,
                    QuoteType = QuoteType.NewBusiness,
                    QuoteNumber = "PETE",
                    OrganisationId = Guid.NewGuid(),
                    ProductName = "Test Product",
                },

                new QuoteSearchIndexWriteModel()
                {
                    Id = Guid.NewGuid(),
                    LastModifiedTicksSinceEpoch = 10002,
                    CreatedTicksSinceEpoch = 10000,
                    LastModifiedByUserTicksSinceEpoch = 10002 + 10,
                    FormDataJson = "{Property1:1, Property2:2}",
                    CustomerFullname = "VAN HALEN",
                    ExpiryTicksSinceEpoch = 100000000000000,
                    ProductId = productId,
                    CustomerEmail = "isagani.lastra+10HALEN@ubind.io",
                    QuoteState = QuoteStateExpired,
                    QuoteType = QuoteType.NewBusiness,
                    QuoteNumber = "VHALEN",
                    OrganisationId = Guid.NewGuid(),
                    ProductName = "Test Product",
                },
            };

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.MockMediator.GetProductByIdOrAliasQuery(tenantProductModel.Product);
                stack.MockMediator.GetTenantByIdOrAliasQuery(tenantProductModel.Tenant);

                // Arrange
                var suffixPath = this.GetSuffixPath(tenantProductModel.Tenant.Details.Alias, DeploymentEnvironment.Staging);
                var directory = stack.LuceneQuoteRepository.CreateNewLuceneDirectory(suffixPath);
                this.directoriesToDelete.Add(directory);
                using (var indexWriter = stack.LuceneQuoteRepository.CreateIndexWriter(directory))
                {
                    stack.LuceneQuoteRepository.AddItemsToIndex(tenantProductModel.Tenant, DeploymentEnvironment.Staging, fakeWriteModels);

                    // All write models should be indexed.
                    var searcher = stack.LuceneQuoteRepository.CreateIndexSearcher(directory);
                    var filters = new QuoteReadModelFilters()
                    {
                        Statuses = new List<string> { "Incomplete", "Complete", "Declined", "Expired" },
                    };

                    var searchResults = stack.LuceneQuoteRepository.Search(tenantProductModel.Tenant, DeploymentEnvironment.Staging, filters).ToList();

                    QuoteSearchIndexWriteModel fakeModel = fakeWriteModels[2];
                    searchResults.Should().HaveCount(3);
                    searchResults.Should().Contain(m => m.OrganisationId == fakeModel.OrganisationId);
                    searchResults.Should().Contain(m => m.QuoteNumber == fakeModel.QuoteNumber);
                    searchResults.Should().Contain(m => m.CustomerFullName == fakeModel.CustomerFullname);
                    searchResults.Should().Contain(m => m.QuoteState == fakeModel.QuoteState);
                    searchResults.Should().Contain(
                        m => m.CreatedTicksSinceEpoch == fakeModel.CreatedTicksSinceEpoch);
                    searchResults.Should().Contain(
                        m => m.LastModifiedTicksSinceEpoch == fakeModel.LastModifiedByUserTicksSinceEpoch);

                    // Searching for the term "employed" should return the proper document
                    searcher = stack.LuceneQuoteRepository.CreateIndexSearcher(directory);
                    filters = new QuoteReadModelFilters()
                    {
                        Statuses = new List<string> { "Incomplete", "Complete", "Declined" },
                        SearchTerms = new List<string> { "employed" },
                    };

                    searchResults = stack.LuceneQuoteRepository.Search(tenantProductModel.Tenant, DeploymentEnvironment.Staging, filters).ToList();

                    fakeModel = fakeWriteModels[1];
                    searchResults.Should().HaveCount(1);
                    searchResults.Should().Contain(m => m.OrganisationId == fakeModel.OrganisationId);
                    searchResults.Should().Contain(m => m.QuoteNumber == fakeModel.QuoteNumber);
                    searchResults.Should().Contain(m => m.CustomerFullName == fakeModel.CustomerFullname);
                    searchResults.Should().Contain(m => m.QuoteState == fakeModel.QuoteState);
                    searchResults.Should().Contain(
                        m => m.CreatedTicksSinceEpoch == fakeModel.CreatedTicksSinceEpoch);
                    searchResults.Should().Contain(
                        m => m.LastModifiedTicksSinceEpoch == fakeModel.LastModifiedByUserTicksSinceEpoch);

                    // searching for complete quotes will return two documents
                    searcher = stack.LuceneQuoteRepository.CreateIndexSearcher(directory);
                    filters = new QuoteReadModelFilters()
                    {
                        Statuses = new List<string> { "Complete" },
                    };

                    searchResults = stack.LuceneQuoteRepository.Search(tenantProductModel.Tenant, DeploymentEnvironment.Staging, filters).ToList();

                    fakeModel = fakeWriteModels[1];
                    searchResults.Should().HaveCount(2);
                    searchResults.Should().Contain(m => m.OrganisationId == fakeModel.OrganisationId);
                    searchResults.Should().Contain(m => m.QuoteNumber == fakeModel.QuoteNumber);
                    searchResults.Should().Contain(m => m.CustomerFullName == fakeModel.CustomerFullname);
                    searchResults.Should().Contain(m => m.QuoteState == fakeModel.QuoteState);
                    searchResults.Should().Contain(
                        m => m.CreatedTicksSinceEpoch == fakeModel.CreatedTicksSinceEpoch);
                    searchResults.Should().Contain(
                        m => m.LastModifiedTicksSinceEpoch == fakeModel.LastModifiedByUserTicksSinceEpoch);

                    // searching for expired quotes will return one documents
                    searcher = stack.LuceneQuoteRepository.CreateIndexSearcher(directory);
                    filters = new QuoteReadModelFilters()
                    {
                        Statuses = new List<string> { "Expired" },
                    };
                    searchResults = stack.LuceneQuoteRepository.Search(tenantProductModel.Tenant, DeploymentEnvironment.Staging, filters).ToList();
                    fakeModel = fakeWriteModels[2];
                    searchResults.Should().HaveCount(1);
                    searchResults.Should().Contain(m => m.OrganisationId == fakeModel.OrganisationId);
                    searchResults.Should().Contain(m => m.QuoteNumber == fakeModel.QuoteNumber);
                    searchResults.Should().Contain(m => m.CustomerFullName == fakeModel.CustomerFullname);
                    searchResults.Should().Contain(m => m.QuoteState == fakeModel.QuoteState);
                    searchResults.Should().Contain(
                        m => m.CreatedTicksSinceEpoch == fakeModel.CreatedTicksSinceEpoch);
                    searchResults.Should().Contain(
                        m => m.LastModifiedTicksSinceEpoch == fakeModel.LastModifiedByUserTicksSinceEpoch);

                    // searching for a specific organisation should return only one result
                    searcher = stack.LuceneQuoteRepository.CreateIndexSearcher(directory);
                    filters = new QuoteReadModelFilters()
                    {
                        Statuses = new List<string> { "Incomplete", "Complete", "Declined", "Expired" },
                        OrganisationIds = new Guid[] { fakeWriteModels[1].OrganisationId },
                    };

                    searchResults = stack.LuceneQuoteRepository.Search(tenantProductModel.Tenant, DeploymentEnvironment.Staging, filters).ToList();
                    fakeModel = fakeWriteModels[1];
                    searchResults.Should().HaveCount(1);
                    searchResults.Should().Contain(m => m.OrganisationId == fakeModel.OrganisationId);
                    searchResults.Should().Contain(m => m.QuoteNumber == fakeModel.QuoteNumber);
                    searchResults.Should().Contain(m => m.CustomerFullName == fakeModel.CustomerFullname);
                    searchResults.Should().Contain(m => m.QuoteState == fakeModel.QuoteState);
                    searchResults.Should().Contain(
                        m => m.CreatedTicksSinceEpoch == fakeModel.CreatedTicksSinceEpoch);
                    searchResults.Should().Contain(
                        m => m.LastModifiedTicksSinceEpoch == fakeModel.LastModifiedByUserTicksSinceEpoch);

                    // This checks if the deletion of Lucene Index items works.
                    stack.LuceneQuoteRepository.DeleteItemsFromIndex(
                        tenantProductModel.Tenant, DeploymentEnvironment.Staging, fakeWriteModels.GetRange(0, 2).Select(x => x.Id));
                }

                var searcherOuter = stack.LuceneQuoteRepository.CreateIndexSearcher(directory);
                var filtersOuter = new QuoteReadModelFilters()
                {
                    Statuses = new List<string> { "Incomplete", "Complete", "Declined", "Expired" },
                };

                var searchResultsOuter = stack.LuceneQuoteRepository.Search(tenantProductModel.Tenant, DeploymentEnvironment.Staging, filtersOuter).ToList();
                QuoteSearchIndexWriteModel fakeModelOuter = fakeWriteModels[2];
                searchResultsOuter.Should().HaveCount(1);
                searchResultsOuter.Should().Contain(m => m.OrganisationId == fakeModelOuter.OrganisationId);
                searchResultsOuter.Should().Contain(m => m.QuoteNumber == fakeModelOuter.QuoteNumber);
                searchResultsOuter.Should().Contain(m => m.CustomerFullName == fakeModelOuter.CustomerFullname);
                searchResultsOuter.Should().Contain(m => m.QuoteState == fakeModelOuter.QuoteState);
                searchResultsOuter.Should().Contain(
                    m => m.CreatedTicksSinceEpoch == fakeModelOuter.CreatedTicksSinceEpoch);
                searchResultsOuter.Should().Contain(
                    m => m.LastModifiedTicksSinceEpoch == fakeModelOuter.LastModifiedByUserTicksSinceEpoch);
            }
        }

        [Fact]
        public void Search_ExpiredFilter_ShouldConsider_Time()
        {
            // Arrange
            var ticksTwoHoursAgo = SystemClock.Instance.Now().Minus(Duration.FromHours(2)).ToUnixTimeTicks();
            var ticksOneHourAgo = SystemClock.Instance.Now().Minus(Duration.FromHours(2)).ToUnixTimeTicks();
            var ticksOneHourLater = SystemClock.Instance.Now().Plus(Duration.FromHours(1)).ToUnixTimeTicks();

            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var tenantProductModel = this.SetupTenantAndProduct(tenantId, productId);

            var fakeWriteModels = new List<QuoteSearchIndexWriteModel>
            {
                new QuoteSearchIndexWriteModel()
                {
                    Id = Guid.NewGuid(),
                    LastModifiedTicksSinceEpoch = ticksTwoHoursAgo,
                    CreatedTicksSinceEpoch = ticksTwoHoursAgo,
                    LastModifiedByUserTicksSinceEpoch = ticksTwoHoursAgo,
                    ExpiryTicksSinceEpoch = ticksOneHourAgo,
                    FormDataJson = "{Property1:1, Property2:2}",
                    CustomerFullname = "Donna Winter",
                    ProductId = productId,
                    CustomerEmail = "garry.agum+donna@ubind.io",
                    QuoteState = QuoteStateExpired,
                    QuoteType = QuoteType.NewBusiness,
                    QuoteNumber = "TUWBOI",
                    OrganisationId = Guid.NewGuid(),
                    ProductName = "Test Product",
                },

                new QuoteSearchIndexWriteModel()
                {
                    Id = Guid.NewGuid(),
                    LastModifiedTicksSinceEpoch = ticksTwoHoursAgo,
                    CreatedTicksSinceEpoch = ticksTwoHoursAgo,
                    LastModifiedByUserTicksSinceEpoch = ticksTwoHoursAgo,
                    ExpiryTicksSinceEpoch = ticksTwoHoursAgo,
                    FormDataJson = "{Property1:'self-employed', Property2:2}",
                    CustomerFullname = "Pete Agassi",
                    ProductId = productId,
                    CustomerEmail = "garry.agum+pete@ubind.io",
                    QuoteState = QuoteStateExpired,
                    QuoteType = QuoteType.NewBusiness,
                    QuoteNumber = "PETE",
                    OrganisationId = Guid.NewGuid(),
                    ProductName = "Test Product",
                },

                new QuoteSearchIndexWriteModel()
                {
                    Id = Guid.NewGuid(),
                    LastModifiedTicksSinceEpoch = ticksTwoHoursAgo,
                    CreatedTicksSinceEpoch = ticksTwoHoursAgo,
                    LastModifiedByUserTicksSinceEpoch = ticksTwoHoursAgo,
                    ExpiryTicksSinceEpoch = ticksOneHourLater,
                    FormDataJson = "{Property1:1, Property2:2}",
                    CustomerFullname = "Van Houten",
                    ProductId = productId,
                    CustomerEmail = "garry.agum+van@ubind.io",
                    QuoteState = QuoteStateExpired,
                    QuoteType = QuoteType.NewBusiness,
                    QuoteNumber = "VHALEN",
                    OrganisationId = Guid.NewGuid(),
                    ProductName = "Test Product",
                },
            };

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.MockMediator.GetTenantByIdOrAliasQuery(tenantProductModel.Tenant);
                var suffixPath = this.GetSuffixPath(tenantProductModel.Tenant.Details.Alias, DeploymentEnvironment.Staging);
                var directory = stack.LuceneQuoteRepository.CreateNewLuceneDirectory(suffixPath);
                this.directoriesToDelete.Add(directory);
                using (var indexWriter = stack.LuceneQuoteRepository.CreateIndexWriter(directory))
                {
                    stack.LuceneQuoteRepository.AddItemsToIndex(tenantProductModel.Tenant, DeploymentEnvironment.Staging, fakeWriteModels);

                    var searcher = stack.LuceneQuoteRepository.CreateIndexSearcher(directory);
                    var filters = new QuoteReadModelFilters()
                    {
                        Statuses = new List<string> { "Expired" },
                    };

                    // Act
                    var searchResults = stack.LuceneQuoteRepository.Search(tenantProductModel.Tenant, DeploymentEnvironment.Staging, filters).ToList();

                    // Asserts
                    QuoteSearchIndexWriteModel fakeModel = fakeWriteModels[1];
                    searchResults.Should().HaveCount(2);
                    searchResults.Should().Contain(m => m.OrganisationId == fakeModel.OrganisationId);
                    searchResults.Should().Contain(m => m.QuoteNumber == fakeModel.QuoteNumber);
                    searchResults.Should().Contain(m => m.CustomerFullName == fakeModel.CustomerFullname);
                    searchResults.Should().Contain(m => m.QuoteState == fakeModel.QuoteState);
                    searchResults.Should().Contain(
                        m => m.CreatedTicksSinceEpoch == fakeModel.CreatedTicksSinceEpoch);
                    searchResults.Should().Contain(
                        m => m.LastModifiedTicksSinceEpoch == fakeModel.LastModifiedByUserTicksSinceEpoch);
                }
            }
        }

        [Fact]
        public void SearchQuote_ContainsIncludedTestData_WhenIncludedTestDataInFilter()
        {
            // Arrange
            var ticksTwoHoursAgo = SystemClock.Instance.Now().Minus(Duration.FromHours(2)).ToUnixTimeTicks();
            var ticksOneHourAgo = SystemClock.Instance.Now().Minus(Duration.FromHours(2)).ToUnixTimeTicks();
            var ticksOneHourLater = SystemClock.Instance.Now().Plus(Duration.FromHours(1)).ToUnixTimeTicks();

            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var tenantProductModel = this.SetupTenantAndProduct(tenantId, productId);

            var fakeWriteModels = new List<QuoteSearchIndexWriteModel>
            {
                new QuoteSearchIndexWriteModel()
                {
                    Id = System.Guid.NewGuid(),
                    LastModifiedTicksSinceEpoch = ticksTwoHoursAgo,
                    CreatedTicksSinceEpoch = ticksTwoHoursAgo,
                    LastModifiedByUserTicksSinceEpoch = ticksTwoHoursAgo,
                    ExpiryTicksSinceEpoch = ticksOneHourAgo,
                    FormDataJson = "{Property1:1, Property2:2}",
                    CustomerFullname = "Donna Winter",
                    ProductId = productId,
                    CustomerEmail = "garry.agum+donna@ubind.io",
                    QuoteState = QuoteStateExpired,
                    QuoteType = QuoteType.NewBusiness,
                    QuoteNumber = "TUWBOI",
                    IsTestData = true,
                    ProductName = "Test Product",
                },

                new QuoteSearchIndexWriteModel()
                {
                    Id = System.Guid.NewGuid(),
                    LastModifiedTicksSinceEpoch = ticksTwoHoursAgo,
                    CreatedTicksSinceEpoch = ticksTwoHoursAgo,
                    LastModifiedByUserTicksSinceEpoch = ticksTwoHoursAgo,
                    ExpiryTicksSinceEpoch = ticksTwoHoursAgo,
                    FormDataJson = "{Property1:'self-employed', Property2:2}",
                    CustomerFullname = "Pete Agassi",
                    ProductId = productId,
                    CustomerEmail = "garry.agum+pete@ubind.io",
                    QuoteState = QuoteStateExpired,
                    QuoteType = QuoteType.NewBusiness,
                    QuoteNumber = "PETE",
                    IsTestData = true,
                    ProductName = "Test Product",
                },

                new QuoteSearchIndexWriteModel()
                {
                    Id = System.Guid.NewGuid(),
                    LastModifiedTicksSinceEpoch = ticksTwoHoursAgo,
                    CreatedTicksSinceEpoch = ticksTwoHoursAgo,
                    LastModifiedByUserTicksSinceEpoch = ticksTwoHoursAgo,
                    ExpiryTicksSinceEpoch = ticksOneHourLater,
                    FormDataJson = "{Property1:1, Property2:2}",
                    CustomerFullname = "Van Houten",
                    ProductId = productId,
                    CustomerEmail = "garry.agum+van@ubind.io",
                    QuoteState = QuoteStateExpired,
                    QuoteType = QuoteType.NewBusiness,
                    QuoteNumber = "VHALEN",
                    IsTestData = false,
                    ProductName = "Test Product",
                },
            };

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.MockMediator.GetTenantByIdOrAliasQuery(tenantProductModel.Tenant);

                var suffixPath = this.GetSuffixPath(tenantProductModel.Tenant.Details.Alias, DeploymentEnvironment.Staging);
                var directory = stack.LuceneQuoteRepository.CreateNewLuceneDirectory(suffixPath);
                this.directoriesToDelete.Add(directory);
                using (var indexWriter = stack.LuceneQuoteRepository.CreateIndexWriter(directory))
                {
                    stack.LuceneQuoteRepository.AddItemsToIndex(tenantProductModel.Tenant, DeploymentEnvironment.Staging, fakeWriteModels);

                    var searcher = stack.LuceneQuoteRepository.CreateIndexSearcher(directory);
                    var filters = new QuoteReadModelFilters()
                    {
                        Statuses = new List<string> { "Expired" },
                        IncludeTestData = true,
                    };

                    // Act
                    var searchResults = stack.LuceneQuoteRepository.Search(tenantProductModel.Tenant, DeploymentEnvironment.Staging, filters).ToList();

                    // Asserts
                    QuoteSearchIndexWriteModel fakeModel = fakeWriteModels[1];
                    searchResults.Should().HaveCount(2);
                    searchResults.Should().Contain(m => m.QuoteNumber == fakeModel.QuoteNumber);
                    searchResults.Should().Contain(m => m.CustomerFullName == fakeModel.CustomerFullname);
                    searchResults.Should().Contain(m => m.QuoteState == fakeModel.QuoteState);
                    searchResults.Should().Contain(
                        m => m.CreatedTicksSinceEpoch == fakeModel.CreatedTicksSinceEpoch);
                    searchResults.Should().Contain(
                        m => m.LastModifiedTicksSinceEpoch == fakeModel.LastModifiedByUserTicksSinceEpoch);
                    searchResults.Should().Contain(m => m.IsTestData == fakeModel.IsTestData);
                }
            }
        }

        [Fact]
        public void SearchQuote_ContainsNoTestDataIncluded_WhenNotIncludedTestDataInFilter()
        {
            // Arrange
            var ticksTwoHoursAgo = SystemClock.Instance.Now().Minus(Duration.FromHours(2)).ToUnixTimeTicks();
            var ticksOneHourAgo = SystemClock.Instance.Now().Minus(Duration.FromHours(2)).ToUnixTimeTicks();
            var ticksOneHourLater = SystemClock.Instance.Now().Plus(Duration.FromHours(1)).ToUnixTimeTicks();

            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var tenantProductModel = this.SetupTenantAndProduct(tenantId, productId);

            var fakeWriteModels = new List<QuoteSearchIndexWriteModel>
            {
                new QuoteSearchIndexWriteModel()
                {
                    Id = System.Guid.NewGuid(),
                    LastModifiedTicksSinceEpoch = ticksTwoHoursAgo,
                    CreatedTicksSinceEpoch = ticksTwoHoursAgo,
                    LastModifiedByUserTicksSinceEpoch = ticksTwoHoursAgo,
                    ExpiryTicksSinceEpoch = ticksOneHourAgo,
                    FormDataJson = "{Property1:1, Property2:2}",
                    CustomerFullname = "Donna Winter",
                    ProductId = productId,
                    CustomerEmail = "garry.agum+donna@ubind.io",
                    QuoteState = QuoteStateExpired,
                    QuoteType = QuoteType.NewBusiness,
                    QuoteNumber = "TUWBOI",
                    IsTestData = false,
                    ProductName = "Test Product",
                },

                new QuoteSearchIndexWriteModel()
                {
                    Id = System.Guid.NewGuid(),
                    LastModifiedTicksSinceEpoch = ticksTwoHoursAgo,
                    CreatedTicksSinceEpoch = ticksTwoHoursAgo,
                    LastModifiedByUserTicksSinceEpoch = ticksTwoHoursAgo,
                    ExpiryTicksSinceEpoch = ticksTwoHoursAgo,
                    FormDataJson = "{Property1:'self-employed', Property2:2}",
                    CustomerFullname = "Pete Agassi",
                    ProductId = productId,
                    CustomerEmail = "garry.agum+pete@ubind.io",
                    QuoteState = QuoteStateExpired,
                    QuoteType = QuoteType.NewBusiness,
                    QuoteNumber = "PETE",
                    IsTestData = false,
                    ProductName = "Test Product",
                },

                new QuoteSearchIndexWriteModel()
                {
                    Id = System.Guid.NewGuid(),
                    LastModifiedTicksSinceEpoch = ticksTwoHoursAgo,
                    CreatedTicksSinceEpoch = ticksTwoHoursAgo,
                    LastModifiedByUserTicksSinceEpoch = ticksTwoHoursAgo,
                    ExpiryTicksSinceEpoch = ticksOneHourLater,
                    FormDataJson = "{Property1:1, Property2:2}",
                    CustomerFullname = "Van Houten",
                    ProductId = productId,
                    CustomerEmail = "garry.agum+van@ubind.io",
                    QuoteState = QuoteStateExpired,
                    QuoteType = QuoteType.NewBusiness,
                    QuoteNumber = "VHALEN",
                    IsTestData = true,
                    ProductName = "Test Product",
                },
            };

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.MockMediator.GetTenantByIdOrAliasQuery(tenantProductModel.Tenant);

                var suffixPath = this.GetSuffixPath(tenantProductModel.Tenant.Details.Alias, DeploymentEnvironment.Staging);

                var directory = stack.LuceneQuoteRepository.CreateNewLuceneDirectory(suffixPath);
                this.directoriesToDelete.Add(directory);
                using (var indexWriter = stack.LuceneQuoteRepository.CreateIndexWriter(directory))
                {
                    stack.LuceneQuoteRepository.AddItemsToIndex(tenantProductModel.Tenant, DeploymentEnvironment.Staging, fakeWriteModels);

                    var searcher = stack.LuceneQuoteRepository.CreateIndexSearcher(directory);
                    var filters = new QuoteReadModelFilters()
                    {
                        Statuses = new List<string> { "Expired" },
                        IncludeTestData = false,
                    };

                    // Act
                    var searchResults = stack.LuceneQuoteRepository.Search(tenantProductModel.Tenant, DeploymentEnvironment.Staging, filters).ToList();

                    // Asserts
                    QuoteSearchIndexWriteModel fakeModel = fakeWriteModels[1];
                    searchResults.Should().HaveCount(2);
                    searchResults.Should().Contain(m => m.QuoteNumber == fakeModel.QuoteNumber);
                    searchResults.Should().Contain(m => m.CustomerFullName == fakeModel.CustomerFullname);
                    searchResults.Should().Contain(m => m.QuoteState == fakeModel.QuoteState);
                    searchResults.Should().Contain(
                        m => m.CreatedTicksSinceEpoch == fakeModel.CreatedTicksSinceEpoch);
                    searchResults.Should().Contain(
                        m => m.LastModifiedTicksSinceEpoch == fakeModel.LastModifiedByUserTicksSinceEpoch);
                    searchResults.Should().Contain(m => m.IsTestData == fakeModel.IsTestData);
                }
            }
        }

        [Fact]
        public void GenerateIndexes_AllowsConcurrentIndexUpdates_ForDifferentDirectories()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var tenantProductModel = this.SetupTenantAndProduct(tenantId, productId);

            HashSet<DeploymentEnvironment> environments = new HashSet<DeploymentEnvironment>
            {
                DeploymentEnvironment.Development,
                DeploymentEnvironment.Staging,
            };

            var filters = new QuoteReadModelFilters()
            {
                Statuses = new List<string> { "Incomplete", "Complete", "Declined" },
            };

            List<IQuoteSearchResultItemReadModel> searchResultsDevelopment = new List<IQuoteSearchResultItemReadModel>();
            List<IQuoteSearchResultItemReadModel> searchResultsStaging = new List<IQuoteSearchResultItemReadModel>();
            Dictionary<DeploymentEnvironment, IndexWriter> indexWritersDictionary = new Dictionary<DeploymentEnvironment, IndexWriter>();

            // Act
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.MockMediator.GetTenantByIdOrAliasQuery(tenantProductModel.Tenant);
                try
                {
                    foreach (var env in environments)
                    {
                        var suffixPath = this.GetSuffixPath(tenantProductModel.Tenant.Details.Alias, env);
                        var directoryInfo = stack.LuceneQuoteRepository.CreateNewLuceneDirectory(suffixPath);
                        this.directoriesToDelete.Add(directoryInfo);
                        var indexWriter = stack.LuceneQuoteRepository.CreateIndexWriter(directoryInfo);
                        indexWritersDictionary.Add(env, indexWriter);
                    }

                    foreach (var env in environments)
                    {
                        List<QuoteSearchIndexWriteModel> writeModels = this.GetWriteModels(productId);
                        if (env == DeploymentEnvironment.Development)
                        {
                            this.UpdateWriteModels(writeModels, "complete");

                            stack.LuceneQuoteRepository.AddItemsToIndex(tenantProductModel.Tenant, DeploymentEnvironment.Development, writeModels);
                        }
                        else
                        {
                            this.UpdateWriteModels(writeModels, "declined");

                            stack.LuceneQuoteRepository.AddItemsToIndex(tenantProductModel.Tenant, DeploymentEnvironment.Staging, writeModels);
                        }
                    }

                    foreach (var env in environments)
                    {
                        var suffixPath = this.GetSuffixPath(tenantProductModel.Tenant.Details.Alias, env);
                        var directoryInfo = stack.LuceneQuoteRepository.GetLatestLiveIndexDirectory(suffixPath);
                        var indexSearcher = stack.LuceneQuoteRepository.CreateIndexSearcher(directoryInfo);
                        if (env == DeploymentEnvironment.Development)
                        {
                            searchResultsDevelopment = stack.LuceneQuoteRepository.Search(tenantProductModel.Tenant, DeploymentEnvironment.Development, filters).ToList();
                        }
                        else
                        {
                            searchResultsStaging = stack.LuceneQuoteRepository.Search(tenantProductModel.Tenant, DeploymentEnvironment.Staging, filters).ToList();
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }

            // Assert
            var completedInDevelopment = searchResultsDevelopment.Count(x => x.QuoteState == "complete");
            var declinedInProduction = searchResultsStaging.Count(x => x.QuoteState == "declined");

            completedInDevelopment.Should().Be(20);
            declinedInProduction.Should().Be(20);
        }

        [Fact]
        public async Task QuoteService_CreateQuoteWithPolicy_CreateUpdateCustomer_ShouldReturnUpdatedCustomerName()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var tenantProductModel = this.SetupTenantAndProduct(tenantId, productId);
            var performingUserId = Guid.NewGuid();

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var tenant = stack.TenantRepository.GetTenantById(tenantId);
                var product = stack.ProductRepository.GetProductById(tenantId, productId);
                stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
                var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null, performingUserId,
                    stack.Clock.GetCurrentInstant());
                tenant.SetDefaultOrganisation(organisation.Id, stack.Clock.Now().Plus(Duration.FromMinutes(1)));
                await stack.OrganisationAggregateRepository.Save(organisation);
                stack.TenantRepository.SaveChanges();

                var role = RoleHelper.CreateCustomerRole(
                    tenantId, tenant.Details.DefaultOrganisationId, SystemClock.Instance.GetCurrentInstant());

                var person = QuoteFactory.CreatePersonAggregate(tenantId);
                var customerAggregate = CustomerAggregate.CreateNewCustomer(
                    tenantId, person, QuoteFactory.DefaultEnvironment, performingUserId, null, stack.Clock.Now());

                var quoteAggregate = QuoteFactory.CreateNewPolicy(
                    tenantId,
                    productId,
                    policyNumber: "POLNUM003",
                    organisationId: organisation.Id);
                var adjustmentQuote = quoteAggregate.WithAdjustmentQuote();
                var quote = quoteAggregate.GetQuoteOrThrow(adjustmentQuote.Id);
                quoteAggregate.WithCalculationResult(quote.Id);
                quoteAggregate.RecordAssociationWithCustomer(customerAggregate, person, performingUserId, stack.Clock.Now());
                stack.RoleRepository.Insert(role);
                await stack.PersonAggregateRepository.Save(person);
                await stack.CustomerAggregateRepository.Save(customerAggregate);
                await stack.QuoteAggregateRepository.Save(quoteAggregate);

                // lucene
                var suffixPath = this.GetSuffixPath(tenantProductModel.Tenant.Details.Alias, DeploymentEnvironment.Staging);
                var directory = stack.LuceneQuoteRepository.CreateNewLuceneDirectory(suffixPath);
                this.directoriesToDelete.Add(directory);
                using (var indexWriter = stack.LuceneQuoteRepository.CreateIndexWriter(directory))
                {
                    var saveQuote = stack.QuoteReadModelRepository.GetQuoteDetails(tenantId, quote.Id);
                    var writeModels = this.GetQuotesForSearchIndexCreation(
                        tenantId, saveQuote.LastModifiedTimestamp.ToUnixTimeTicks()).ToList();

                    var filters = new QuoteReadModelFilters()
                    {
                        Statuses = new List<string> { "Incomplete" },
                        IncludeTestData = true,
                    };

                    stack.LuceneQuoteRepository.AddItemsToIndex(tenant, DeploymentEnvironment.Staging, writeModels);
                    this.UpdateWriteModels(writeModels, "incomplete");

                    IEnumerable<IQuoteSearchResultItemReadModel> searchResults;

                    directory = stack.LuceneQuoteRepository.GetLatestLiveIndexDirectory(suffixPath);
                    var indexReader = stack.LuceneQuoteRepository.CreateIndexSearcher(directory);
                    searchResults = stack.LuceneQuoteRepository.Search(tenant, DeploymentEnvironment.Staging, filters);

                    IEnumerable<IQuoteSearchResultItemReadModel> incompletes
                        = searchResults.Where(x => x.QuoteState.EqualsIgnoreCase(StandardQuoteStates.Incomplete));

                    // test if previous fullname exists
                    var oldFullName = searchResults.FirstOrDefault().CustomerFullName;
                    oldFullName.Should().Be("Noris McWhirter");

                    // Act
                    person = stack.PersonAggregateRepository.GetById(tenant.Id, person.Id);
                    var personCommonProperties = new PersonCommonProperties
                    {
                        FullName = "Updated FullName",
                    };

                    person.Update(new PersonalDetails(tenant.Id, personCommonProperties), performingUserId, stack.Clock.Now());
                    await stack.PersonAggregateRepository.Save(person);

                    writeModels = this.GetQuotesForSearchIndexCreation(tenantId, saveQuote.LastModifiedTimestamp.ToUnixTimeTicks()).ToList();
                    stack.LuceneQuoteRepository.AddItemsToIndex(tenant, DeploymentEnvironment.Staging, writeModels);

                    indexReader = stack.LuceneQuoteRepository.CreateIndexSearcher(directory);
                    searchResults = stack.LuceneQuoteRepository.Search(tenant, DeploymentEnvironment.Staging, filters);

                    // test if the person aggregate saved the updated full name
                    var newFullName = searchResults.FirstOrDefault().CustomerFullName;
                    newFullName.Should().Be("Updated FullName");
                }
            }
        }

        [Fact]
        public async Task GenerateIndexes_IndexesQuoteSuccessfully_WhenQuoteProductIsDeleted()
        {
            // Arrange
            var performingUserId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var tenantProductModel = this.SetupTenantAndProduct(tenantId, productId);

            // Delete product
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var product = stack.ProductRepository.GetProductById(tenantId, productId);
                var newDetails = new ProductDetails(
                    product.Details.Name,
                    product.Details.Alias,
                    product.Details.Disabled,
                    true,
                    SystemClock.Instance.Now(),
                    productQuoteExpirySetting: new QuoteExpirySettings(30, true));
                product.Update(newDetails);
                stack.ProductRepository.SaveChanges();
            }

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var tenant = stack.TenantRepository.GetTenantById(tenantId);
                var product = stack.ProductRepository.GetProductById(tenantId, productId);
                stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
                var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null, performingUserId,
                    stack.Clock.GetCurrentInstant());
                tenant.SetDefaultOrganisation(organisation.Id, stack.Clock.Now().Plus(Duration.FromMinutes(1)));
                await stack.OrganisationAggregateRepository.Save(organisation);
                stack.TenantRepository.SaveChanges();

                var role = RoleHelper.CreateCustomerRole(
                    tenantId, tenant.Details.DefaultOrganisationId, SystemClock.Instance.GetCurrentInstant());

                var person = QuoteFactory.CreatePersonAggregate(tenantId);
                var customerAggregate = CustomerAggregate.CreateNewCustomer(
                    tenantId, person, QuoteFactory.DefaultEnvironment, performingUserId, null, stack.Clock.Now());

                var quoteAggregate = QuoteFactory.CreateNewPolicy(
                    tenantId,
                    productId,
                    policyNumber: "POLNUM002",
                    organisationId: organisation.Id);

                var adjustmentQuote = quoteAggregate.WithAdjustmentQuote();
                var quote = quoteAggregate.GetQuoteOrThrow(adjustmentQuote.Id);
                quoteAggregate.WithCalculationResult(quote.Id);
                quoteAggregate.RecordAssociationWithCustomer(customerAggregate, person, performingUserId, stack.Clock.Now());
                stack.RoleRepository.Insert(role);
                await stack.PersonAggregateRepository.Save(person);
                await stack.CustomerAggregateRepository.Save(customerAggregate);
                await stack.QuoteAggregateRepository.Save(quoteAggregate);

                // lucene
                var suffixPath = this.GetSuffixPath(tenantProductModel.Tenant.Details.Alias, DeploymentEnvironment.Staging);
                var directory = stack.LuceneQuoteRepository.CreateNewLuceneDirectory(suffixPath);
                this.directoriesToDelete.Add(directory);
                using (var indexWriter = stack.LuceneQuoteRepository.CreateIndexWriter(directory))
                {
                    var saveQuote = stack.QuoteReadModelRepository.GetQuoteDetails(tenantId, quote.Id);
                    var writeModels = this.GetQuotesForSearchIndexCreation(tenantId, saveQuote.LastModifiedTimestamp.ToUnixTimeTicks()).ToList();

                    var filters = new QuoteReadModelFilters()
                    {
                        Statuses = new List<string> { "Incomplete" },
                        IncludeTestData = true,
                    };

                    stack.LuceneQuoteRepository.AddItemsToIndex(tenant, DeploymentEnvironment.Staging, writeModels);
                    this.UpdateWriteModels(writeModels, "incomplete");

                    IEnumerable<IQuoteSearchResultItemReadModel> searchResults;

                    directory = stack.LuceneQuoteRepository.GetLatestLiveIndexDirectory(suffixPath);
                    var indexReader = stack.LuceneQuoteRepository.CreateIndexSearcher(directory);
                    searchResults = stack.LuceneQuoteRepository.Search(tenant, DeploymentEnvironment.Staging, filters);

                    IEnumerable<IQuoteSearchResultItemReadModel> incompletes = searchResults.Where(x => x.QuoteState == "incomplete");

                    // test if previous fullname exists
                    var oldFullName = searchResults.FirstOrDefault().CustomerFullName;
                    oldFullName.Should().Be("Noris McWhirter");

                    // Act
                    person = stack.PersonAggregateRepository.GetById(tenant.Id, person.Id);
                    person.UpdateFullName("Updated FullName", performingUserId, stack.Clock.Now());
                    await stack.PersonAggregateRepository.Save(person);

                    writeModels = this.GetQuotesForSearchIndexCreation(tenantId, saveQuote.LastModifiedTimestamp.ToUnixTimeTicks()).ToList();
                    stack.LuceneQuoteRepository.AddItemsToIndex(tenant, DeploymentEnvironment.Staging, writeModels);

                    indexReader = stack.LuceneQuoteRepository.CreateIndexSearcher(directory);
                    searchResults = stack.LuceneQuoteRepository.Search(tenant, DeploymentEnvironment.Staging, filters);

                    // test if the person aggregate saved the updated full name
                    var newFullName = searchResults.FirstOrDefault().CustomerFullName;
                    newFullName.Should().Be("Updated FullName");
                }
            }
        }

        [Fact(Skip = "This is intermittenly failing on the CI server, due to a database failure. Created ticket UB-9156 to address it.")]
        public void RegenerateIndexes_AllowsSearch_WhileDoingRegenerationOfIndexes()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var tenantProductModel = this.SetupTenantAndProduct(tenantId, productId);
            Guid performingUserId = Guid.NewGuid();

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.MockMediator.GetTenantByIdOrAliasQuery(tenantProductModel.Tenant);
                var suffixPath = this.GetSuffixPath(tenantProductModel.Tenant.Details.Alias, DeploymentEnvironment.Staging);
                var directory = stack.LuceneQuoteRepository.CreateNewLuceneDirectory(suffixPath);
                using (var indexWriter = stack.LuceneQuoteRepository.CreateIndexWriter(directory))
                {
                    List<QuoteSearchIndexWriteModel> writeModels = this.GetWriteModels(productId);

                    stack.LuceneQuoteRepository.AddItemsToIndex(tenantProductModel.Tenant, DeploymentEnvironment.Staging, writeModels);
                    this.UpdateWriteModels(writeModels, "complete");

                    IEnumerable<IQuoteSearchResultItemReadModel> searchResults;
                    var filters = new QuoteReadModelFilters()
                    {
                        Statuses = new List<string> { "Incomplete", "Complete" },
                    };

                    directory = stack.LuceneQuoteRepository.GetLatestLiveIndexDirectory(suffixPath);
                    var indexReader = stack.LuceneQuoteRepository.CreateIndexSearcher(directory);
                    searchResults = stack.LuceneQuoteRepository.Search(tenantProductModel.Tenant, DeploymentEnvironment.Staging, filters);

                    searchResults.Any(x => x.QuoteState == "complete").Should().BeFalse();

                    var incompletes = searchResults.Where(x => x.QuoteState == "incomplete");

                    incompletes.Should().HaveCount(20);

                    // Act
                    var taskUpdateIndexes = Task.Run(() =>
                    {
                        stack.LuceneQuoteRepository.AddItemsToIndex(tenantProductModel.Tenant, DeploymentEnvironment.Staging, writeModels);
                    });

                    var taskBackupIndexes = Task.Run(() =>
                    {
                        stack.LuceneQuoteRepository.AddItemsToRegenerationIndex(tenantProductModel.Tenant, DeploymentEnvironment.Staging, writeModels);
                        stack.LuceneQuoteRepository.MakeRegenerationIndexTheLiveIndex(suffixPath);
                    });

                    var taskSearchIndexes = Task.Run(() =>
                    {
                        directory = stack.LuceneQuoteRepository.GetLatestLiveIndexDirectory(suffixPath);
                        var indexReader = stack.LuceneQuoteRepository.CreateIndexSearcher(directory);
                        searchResults = stack.LuceneQuoteRepository.Search(tenantProductModel.Tenant, DeploymentEnvironment.Staging, filters);
                    });

                    // Assert
                    searchResults.Any(x => x.QuoteState == "complete").Should().BeFalse();

                    incompletes = searchResults.Where(x => x.QuoteState == "incomplete");
                    incompletes.Should().HaveCount(20);

                    Task.WaitAll(taskUpdateIndexes, taskBackupIndexes, taskSearchIndexes);

                    directory = stack.LuceneQuoteRepository.GetLatestLiveIndexDirectory(suffixPath);
                    this.directoriesToDelete.Add(directory);
                    indexReader = stack.LuceneQuoteRepository.CreateIndexSearcher(directory);
                    searchResults = stack.LuceneQuoteRepository.Search(tenantProductModel.Tenant, DeploymentEnvironment.Staging, filters);

                    searchResults.Any(x => x.QuoteState == "incomplete").Should().BeFalse();

                    var completes = searchResults.Where(x => x.QuoteState == "complete");
                    completes.Should().HaveCount(20);
                }
            }
        }

        [Fact]
        public void RegenerateTenantLuceneIndex_ShouldRegenerateLiveLuceneIndex_GenerateToLiveLuceneIndex()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var tenantProductModel = this.SetupTenantAndProduct(tenantId, productId);

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.MockMediator.GetTenantByIdOrAliasQuery(tenantProductModel.Tenant);

                // Act
                List<QuoteSearchIndexWriteModel> writeModels = this.GetWriteModels(productId);
                var suffixPath = this.GetSuffixPath(tenantProductModel.Tenant.Details.Alias, DeploymentEnvironment.Staging);
                var liveBaseDirectory = stack.LuceneQuoteRepository.GetLiveIndexBaseDirectory(suffixPath);
                if (!liveBaseDirectory.Exists)
                {
                    stack.LuceneQuoteRepository.CreateNewLuceneDirectory(suffixPath);
                }

                stack.LuceneQuoteRepository.AddItemsToRegenerationIndex(tenantProductModel.Tenant, DeploymentEnvironment.Staging, writeModels);
                liveBaseDirectory = stack.LuceneQuoteRepository.GetLiveIndexBaseDirectory(suffixPath);

                // e.g. "Regeneration/tenant/environment/Quotes/2021-09-01 13:22:32"
                var regenerationDirectory = stack.LuceneQuoteRepository.GetLatestRegenerationIndexDirectory(suffixPath);
                var newLiveDirectory = new DirectoryInfo(System.IO.Path.Combine(liveBaseDirectory.FullName, regenerationDirectory.Name));

                if (newLiveDirectory.Exists)
                {
                    newLiveDirectory.Delete(true);
                }

                // move it
                stack.LuceneQuoteRepository.MakeRegenerationIndexTheLiveIndex(suffixPath);

                // Assert
                var latestLiveDirectory = stack.LuceneQuoteRepository.GetLatestLiveIndexDirectory(suffixPath);
                Assert.True(latestLiveDirectory.Exists);
            }
        }

        [Fact(Skip = "Skip this unit test this will be handle in UB-9275")]
        public async Task RegenerateLuceneIndexes_AddNewQuoteWhileDoingRegeneration_ShouldSearchTheNewAddedQuote()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var tenantProductModel = this.SetupTenantAndProduct(tenantId, productId);
            Guid performingUserId = Guid.NewGuid();

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var tenant = stack.TenantRepository.GetTenantById(tenantId);
                var product = stack.ProductRepository.GetProductById(tenantId, productId);
                stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
                var suffixPath = this.GetSuffixPath(tenantProductModel.Tenant.Details.Alias, DeploymentEnvironment.Staging);
                var directory = stack.LuceneQuoteRepository.CreateNewLuceneDirectory(suffixPath);
                this.directoriesToDelete.Add(directory);
                List<QuoteSearchIndexWriteModel> writeModels = this.GetWriteModels(productId);

                stack.LuceneQuoteRepository.AddItemsToIndex(tenant, DeploymentEnvironment.Staging, writeModels);

                IEnumerable<IQuoteSearchResultItemReadModel> searchResults;

                var filters = new QuoteReadModelFilters()
                {
                    Statuses = new List<string> { "Incomplete", "Complete" },
                };

                directory = stack.LuceneQuoteRepository.GetLatestLiveIndexDirectory(suffixPath);
                var indexReader = stack.LuceneQuoteRepository.CreateIndexSearcher(directory);
                searchResults = stack.LuceneQuoteRepository.Search(tenant, DeploymentEnvironment.Staging, filters);

                searchResults.Any(x => x.QuoteState == "complete").Should().BeFalse();

                var incompletes = searchResults.Where(x => x.QuoteState == "incomplete");

                incompletes.Should().HaveCount(20);

                // Act
                var taskRegenerateIndexes = Task.Run(async () =>
                {
                    this.UpdateWriteModels(writeModels, "complete");
                    stack.LuceneQuoteRepository.AddItemsToRegenerationIndex(tenant, DeploymentEnvironment.Staging, writeModels);

                    stack.LuceneQuoteRepository.MakeRegenerationIndexTheLiveIndex(suffixPath);

                    var person = QuoteFactory.CreatePersonAggregate(tenantId);
                    var customerAggregate = CustomerAggregate.CreateNewCustomer(
                        tenantId, person, QuoteFactory.DefaultEnvironment, performingUserId, null, stack.Clock.Now());

                    var quoteAggregate = QuoteFactory.CreateNewPolicy(tenantId, productId, policyNumber: "POLNUM00003");
                    var adjustmentQuote = quoteAggregate.WithAdjustmentQuote();
                    var quote = quoteAggregate.GetQuoteOrThrow(adjustmentQuote.Id);
                    quoteAggregate.WithCalculationResult(quote.Id);
                    quoteAggregate.RecordAssociationWithCustomer(customerAggregate, person, performingUserId, stack.Clock.Now());
                    await stack.PersonAggregateRepository.Save(person);
                    await stack.CustomerAggregateRepository.Save(customerAggregate);
                    await stack.QuoteAggregateRepository.Save(quoteAggregate);

                    IEnumerable<IQuoteSearchIndexWriteModel> quotesToMigrate = this.GetQuotesForSearchIndexCreation(tenantId, stack.Clock.Now().ToUnixTimeTicks());
                    writeModels.AddRange((List<QuoteSearchIndexWriteModel>)quotesToMigrate);
                    stack.LuceneQuoteRepository.AddItemsToIndex(tenant, DeploymentEnvironment.Staging, writeModels);
                });

                // Assert
                await taskRegenerateIndexes;

                filters = new QuoteReadModelFilters()
                {
                    Statuses = new List<string> { "Incomplete", "Complete" },
                    IncludeTestData = true,
                };

                directory = stack.LuceneQuoteRepository.GetLatestLiveIndexDirectory(suffixPath);
                indexReader = stack.LuceneQuoteRepository.CreateIndexSearcher(directory);
                searchResults = stack.LuceneQuoteRepository.Search(tenant, DeploymentEnvironment.Staging, filters);

                var fullname = searchResults.Where(x => x.CustomerFullName == "Noris McWhirter");
                fullname.Should().HaveCountGreaterOrEqualTo(1);

                var incomplete = searchResults.Where(x => x.QuoteState == "incomplete");
                incomplete.Should().HaveCount(1);

                var complete = searchResults.Where(x => x.QuoteState == "complete");
                complete.Should().HaveCountGreaterOrEqualTo(20);

                searchResults.Should().HaveCountGreaterOrEqualTo(20);
            }
        }

        private string GetSuffixPath(string tenantId, DeploymentEnvironment environment)
        {
            return string.Format(@"{0}\{1}", tenantId, environment);
        }

        private List<QuoteSearchIndexWriteModel> GetWriteModels(Guid productId)
        {
            var writeModels = new List<QuoteSearchIndexWriteModel>();

            for (int i = 0; i < 20; i++)
            {
                writeModels.Add(new QuoteSearchIndexWriteModel()
                {
                    Id = Guid.NewGuid(),
                    LastModifiedTicksSinceEpoch = 1000 + i,
                    LastModifiedByUserTicksSinceEpoch = 1000 + i,
                    FormDataJson = "{Property1:1, Property2:2}",
                    CustomerFullname = string.Format("Customer Number:{0}", i),
                    ProductId = productId,
                    CustomerEmail = string.Format("isagani.lastra+10{0}@ubind.io", i),
                    QuoteState = QuoteStateIncomplete,
                    QuoteType = QuoteType.NewBusiness,
                    QuoteNumber = string.Format("lucene{0}", i),
                    ProductName = "Test Product",
                });
            }

            return writeModels;
        }

        private void UpdateWriteModels(List<QuoteSearchIndexWriteModel> writeModels, string newStatus)
        {
            foreach (var model in writeModels)
            {
                model.CustomerFullname += "Updated";
                model.QuoteState = newStatus;
                model.LastModifiedTicksSinceEpoch += 1000;
                model.LastModifiedByUserTicksSinceEpoch += 1000;
            }
        }

        private List<QuoteSearchIndexWriteModel> GetQuotesForSearchIndexCreation(
            Guid tenantId, long? searchIndexLastUpdatedTicksSinceEpoch = null)
        {
            // Note: using the current connection executeQuery
            // because Dapper used in the QuotereadModelRepository is returning an error when using the test database
            long lastUpdatedTickSinceEpoch = 0;
            if (searchIndexLastUpdatedTicksSinceEpoch.HasValue)
            {
                lastUpdatedTickSinceEpoch = searchIndexLastUpdatedTicksSinceEpoch.Value;
            }

            string sql = @"Select  q.id,
                            q.LastModifiedTicksSinceEpoch,
                            q.LastModifiedByUserTicksSinceEpoch,
                            q.CreatedTicksSinceEpoch,
                            q.[Type] [QuoteType],
                            q.LatestFormData [FormDataJson],
                            q.QuoteNumber,
                            q.QuoteState,
                            q.policyId,
                            q.ExpiryTicksSinceEpoch,
                            q.OwnerUserId,
                            q.OwnerPersonId,
                            q.OwnerFullName,
                            q.CustomerId,
                            q.CustomerFullName,
                            q.CustomerAlternativeEmail,
                            q.CustomerHomePhone,
                            q.CustomerMobilePhone,
                            q.CustomerWorkPhone,
                            q.CustomerPreferredName,
                            q.ProductId,
                            q.productId,
                            q.TenantId,
                            q.tenantId,
                            q.IsTestData
                            from Quotes q
                    where q.LastModifiedTicksSinceEpoch >= @LastModifiedTicksSinceEpoch
                            and q.tenantId = '@TenantId'
                            and q.Environment = 2
                            and q.QuoteState <> 'Nascent'
                            and q.QuoteNumber is not null
                            and q.IsDiscarded = 0
                            ORDER BY q.LastModifiedTicksSinceEpoch";
            sql = sql.Replace("@LastModifiedTicksSinceEpoch", lastUpdatedTickSinceEpoch.ToString())
                .Replace("@TenantId", tenantId.ToString());
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var writeModels = stack.DbContext.Database.SqlQuery<QuoteSearchIndexWriteModel>(sql).ToList();
                writeModels.ForEach(wm => wm.ProductName = "Test Product");
                return writeModels;
            }
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

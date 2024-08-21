// <copyright file="UpdateDataTableFromCsvCommandHandlerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.Commands.DataTable
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Commands.DataTable;
    using UBind.Application.Services;
    using UBind.Application.Services.DelimiterSeparatedValues;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Entities;
    using UBind.Domain.Events;
    using UBind.Domain.Extensions;
    using UBind.Domain.Models.DataTable;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.DataTable;
    using UBind.Persistence.ReadModels;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class UpdateDataTableFromCsvCommandHandlerTests
    {
        private readonly UpdateDataTableFromCsvDataCommandHandler sut;
        private DataTableDefinitionRepository dataTableDefinitionRepository;
        private TenantRepository tenantRepository;
        private OrganisationReadModelRepository organisationReadModelRepository;
        private ProductRepository productRepository;
        private DataTableContentRepository dataTableContentRepository;
        private Mock<ICachingResolver> cachingResolver;
        private Mock<ILogger<DataTableDefinitionService>> mockLogger;
        private Tenant tenant;
        private OrganisationReadModel organisation;
        private Mock<ITenantSystemEventEmitter> mockTenantSystemEventEmitter;
        private Mock<IOrganisationSystemEventEmitter> mockOrganisationSystemEventEmitter;
        private Mock<IWebHostEnvironment> mockWebHostEnvironment;
        private UBindDbContext dbContext;

        public UpdateDataTableFromCsvCommandHandlerTests()
        {
            var dataTableDataTypeRegistry = new DataTableDataTypeSqlSettingsRegistry();
            DataTableDataTypeExtensions.SetDataTypeSqlSettingsRegistry(dataTableDataTypeRegistry);
            this.sut = this.Sut();
        }

        [Fact]
        public async Task UpdateDataTableFromCsvCommand_ShouldUpdateTheDataTableContentRecords()
        {
            // Arrange
            var dataTableDefinitionName = "Tenant Edit DT A";
            var dataTableDefinitionAlias = "tenant-edit-dt-a";
            string jsonTableSchema = @"{""columns"":[{""name"":""Name"",""alias"":""name"",""dataType"":""name""},{""name"":""Age"",""alias"":""age"",""dataType"":""wholeNumber""},{""name"":""Gender"",""alias"":""gender"",""dataType"":""text""}]}";
            var tableSchema = JsonConvert.DeserializeObject<DataTableSchema>(jsonTableSchema);
            string csvData = @"Name,Age,Gender
John,28,Male
Jane,32,Female";
            var createDataTableFromCsvCommand = new CreateDataTableFromCsvDataCommand(
                this.tenant.Id,
                EntityType.Tenant,
                this.tenant.Id,
                dataTableDefinitionName,
                dataTableDefinitionAlias,
                csvData,
                tableSchema,
                false,
                0);
            var createdDataTableDefinition = await this.UpdateDataTableFromCsvCommandSetup(createDataTableFromCsvCommand);
            string newJsonTableSchema = @"{""columns"":[{""name"":""Name"",""alias"":""name"",""dataType"":""name""},{""name"":""Age"",""alias"":""age"",""dataType"":""wholeNumber""},{""name"":""Gender"",""alias"":""gender"",""dataType"":""text""},{""name"":""Address"",""alias"":""address"",""dataType"":""text""}]}";
            var newTableSchema = JsonConvert.DeserializeObject<DataTableSchema>(newJsonTableSchema);
            string newCsvData = @"Name,Age,Gender,Address
John,28,Male,123 Libney St.
Jane,32,Female,849 South Creek St.
David,26,Male,22nd Flr Hampton Bldg";
            var updateCommand = new UpdateDataTableFromCsvDataCommand(
                tenantId: this.tenant.Id,
                definitionId: createdDataTableDefinition.Id,
                dataTableAlias: $"{dataTableDefinitionAlias}-updated",
                dataTableName: "Tenant Edit DT One Updated",
                tableSchema: newTableSchema,
                csvData: newCsvData,
                memoryCachingEnabled: false,
                cacheExpiryInSeconds: 0);

            // Act
            var result = await this.sut.Handle(updateCommand, CancellationToken.None);
            var dataTableContent = await this.dataTableContentRepository.GetAllDataTableContent(result.Id, false, 0);

            // Assert
            dataTableContent.Count().Should().Be(3);
        }

        [Fact]
        public async Task UpdateDataTableFromCsvCommand_ShouldUpdateTheDataTableColumnCount()
        {
            // Arrange
            var dataTableDefinitionName = "Tenant Edit DT B";
            var dataTableDefinitionAlias = "tenant-edit-dt-b";
            string jsonTableSchema = @"{""columns"":[{""name"":""Name"",""alias"":""name"",""dataType"":""name""},{""name"":""Age"",""alias"":""age"",""dataType"":""wholeNumber""},{""name"":""Gender"",""alias"":""gender"",""dataType"":""text""}]}";
            var tableSchema = JsonConvert.DeserializeObject<DataTableSchema>(jsonTableSchema);
            string csvData = @"Name,Age,Gender
John,28,Male
Jane,32,Female";
            var createDataTableFromCsvCommand = new CreateDataTableFromCsvDataCommand(
                this.tenant.Id,
                EntityType.Tenant,
                this.tenant.Id,
                dataTableDefinitionName,
                dataTableDefinitionAlias,
                csvData,
                tableSchema,
                false,
                0);
            var createdDataTableDefinition = await this.UpdateDataTableFromCsvCommandSetup(createDataTableFromCsvCommand);
            string newJsonTableSchema = @"{""columns"":[{""name"":""Name"",""alias"":""name"",""dataType"":""name""},{""name"":""Age"",""alias"":""age"",""dataType"":""wholeNumber""},{""name"":""Gender"",""alias"":""gender"",""dataType"":""text""},{""name"":""Address"",""alias"":""address"",""dataType"":""text""}]}";
            var newTableSchema = JsonConvert.DeserializeObject<DataTableSchema>(newJsonTableSchema);
            string newCsvData = @"Name,Age,Gender,Address
John,28,Male,123 Libney St.
Jane,32,Female,849 South Creek St.";
            var updateCommand = new UpdateDataTableFromCsvDataCommand(
                tenantId: this.tenant.Id,
                definitionId: createdDataTableDefinition.Id,
                dataTableAlias: $"{dataTableDefinitionAlias}-updated",
                dataTableName: $"{dataTableDefinitionName} Updated",
                tableSchema: newTableSchema,
                csvData: newCsvData,
                memoryCachingEnabled: false,
                cacheExpiryInSeconds: 0);

            // Act
            var result = await this.sut.Handle(updateCommand, CancellationToken.None);
            var updatedDataTableDefinition = this.dataTableDefinitionRepository.GetDataTableDefinitionById(this.tenant.Id, createdDataTableDefinition.Id);

            // Assert
            updatedDataTableDefinition.ColumnCount.Should().Be(4);
        }

        private async Task<DataTableDefinition> UpdateDataTableFromCsvCommandSetup(CreateDataTableFromCsvDataCommand createCommand)
        {
            var dataTableDefinitionService = new DataTableDefinitionService(
                this.dataTableDefinitionRepository,
                this.dataTableContentRepository,
                this.tenantRepository,
                this.organisationReadModelRepository,
                this.productRepository,
                this.cachingResolver.Object,
                this.mockWebHostEnvironment.Object,
                this.mockLogger.Object);
            var createDataTableFromCsvCommandHandler = new CreateDataTableFromCsvDataCommandHandler(
                this.dataTableDefinitionRepository,
                this.dataTableContentRepository,
                dataTableDefinitionService,
                SystemClock.Instance,
                this.mockTenantSystemEventEmitter.Object,
                this.mockOrganisationSystemEventEmitter.Object,
                this.dbContext);

            var dataTableDefinition = await createDataTableFromCsvCommandHandler.Handle(createCommand, CancellationToken.None);

            return dataTableDefinition;
        }

        private UpdateDataTableFromCsvDataCommandHandler Sut()
        {
            this.mockOrganisationSystemEventEmitter = new Mock<IOrganisationSystemEventEmitter>();
            this.mockTenantSystemEventEmitter = new Mock<ITenantSystemEventEmitter>();
            this.mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
            this.mockWebHostEnvironment.Setup(m => m.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            this.mockLogger = new Mock<ILogger<DataTableDefinitionService>>();
            this.cachingResolver = new Mock<ICachingResolver>();
            this.tenant = TenantFactory.Create(Guid.NewGuid(), "datatable-tenant-two");
            this.organisation = new OrganisationReadModel(
               this.tenant.Id, Guid.NewGuid(), "datatable-org-one", "Data Table Org One", null, true, false, SystemClock.Instance.Now());
            Product product = ProductFactory.Create(this.tenant.Id, Guid.NewGuid(), "datatable-product-two");
            this.dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);

            this.cachingResolver.Setup(cr => cr.GetTenantOrNull(this.tenant.Id)).Returns(Task.FromResult(this.tenant));
            this.cachingResolver.Setup(cr => cr.GetTenantOrThrow(this.tenant.Id)).Returns(Task.FromResult(this.tenant));
            this.cachingResolver.Setup(cr => cr.GetProductOrNull(this.tenant.Id, product.Id)).Returns(Task.FromResult(product));
            this.cachingResolver.Setup(cr => cr.GetProductOrThrow(this.tenant.Id, product.Id)).Returns(Task.FromResult(product));
            this.cachingResolver.Setup(cr => cr.GetOrganisationOrNull(this.tenant.Id, this.organisation.Id)).Returns(Task.FromResult(this.organisation));
            this.cachingResolver.Setup(cr => cr.GetOrganisationOrThrow(this.tenant.Id, this.organisation.Id)).Returns(Task.FromResult(this.organisation));

            IConnectionConfiguration connectionConfiguration = new ConnectionStrings();
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json")
                .Build();
            var dataTableConfiguration = new DataTableContentDbConfiguration(config.GetConnectionString(DatabaseFixture.TestConnectionStringName));
            var dataTableContentDbFactory = new DataTableContentDbFactory(dataTableConfiguration);

            var delimiterSeparatedValuesFileProvider = new DelimiterSeparatedValuesFileProvider();
            this.dataTableDefinitionRepository = new DataTableDefinitionRepository(this.dbContext);
            this.tenantRepository = new TenantRepository(this.dbContext);
            this.organisationReadModelRepository = new OrganisationReadModelRepository(this.dbContext);
            this.productRepository = new ProductRepository(this.dbContext);
            this.dataTableContentRepository = new DataTableContentRepository(this.dbContext, dataTableContentDbFactory);
            var dataTableDefinitionService = new DataTableDefinitionService(
                this.dataTableDefinitionRepository,
                this.dataTableContentRepository,
                this.tenantRepository,
                this.organisationReadModelRepository,
                this.productRepository,
                this.cachingResolver.Object,
                this.mockWebHostEnvironment.Object,
                this.mockLogger.Object);
            return new UpdateDataTableFromCsvDataCommandHandler(
                this.dataTableDefinitionRepository,
                this.dataTableContentRepository,
                dataTableDefinitionService,
                this.dbContext,
                this.mockTenantSystemEventEmitter.Object,
                this.mockOrganisationSystemEventEmitter.Object);
        }
    }
}

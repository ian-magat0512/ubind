// <copyright file="CreateDataTableFromCsvCommandHandlerTests.cs" company="uBind">
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
    using Humanizer;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualBasic;
    using Moq;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Commands.DataTable;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Events;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Models.DataTable;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.DataTable;
    using UBind.Persistence.ReadModels;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class CreateDataTableFromCsvCommandHandlerTests
    {
        private readonly CreateDataTableFromCsvDataCommandHandler sut;

        private UBindDbContext dbContext;
        private DataTableContentDbFactory dataTableContentDbFactory;
        private DataTableContentRepository dataTableContentRepository;
        private Mock<ICachingResolver> cachingResolver;
        private Tenant tenant;
        private OrganisationReadModel organisation;
        private Product product;
        private Mock<ITenantSystemEventEmitter> mockTenantSystemEventEmitter;
        private Mock<IOrganisationSystemEventEmitter> mockOrganisationSystemEventEmitter;

        public CreateDataTableFromCsvCommandHandlerTests()
        {
            var dataTableDataTypeRegistry = new DataTableDataTypeSqlSettingsRegistry();
            DataTableDataTypeExtensions.SetDataTypeSqlSettingsRegistry(dataTableDataTypeRegistry);

            this.sut = this.Sut();
        }

        [Fact]
        public async Task CreateDataTableFromCsvCommand_ShouldCreateDataTableContent()
        {
            // Arrange
            var dataTableDefinitionName = "Tenant DT Two";
            var dataTableDefinitionAlias = "tenant-dt-two";
            string jsonTableSchema = @"{""columns"":[{""name"":""Name"",""alias"":""name"",""dataType"":""name""},{""name"":""Age"",""alias"":""age"",""dataType"":""wholeNumber""},{""name"":""Gender"",""alias"":""gender"",""dataType"":""text""}]}";
            var tableSchema = JsonConvert.DeserializeObject<DataTableSchema>(jsonTableSchema);
            string csvData = @"Name,Age,Gender
John,28,Male
Jane,32,Female";
            var command = new CreateDataTableFromCsvDataCommand(
                this.tenant.Id,
                EntityType.Tenant,
                this.tenant.Id,
                dataTableDefinitionName,
                dataTableDefinitionAlias,
                csvData,
                tableSchema,
                false,
                0);

            // Act
            var result = await this.sut.Handle(command, CancellationToken.None);
            var dataTableContent = this.dataTableContentRepository.GetAllDataTableContent(result.Id, false, 0);

            // Assert
            dataTableContent.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateDataTableFromCsvCommand_ShouldCreateDataTableContent_WhenConfigurationHasVariousDataTypesWithDefaultValues()
        {
            // Arrange
            var dataTableDefinitionName = "CreateDataTableWithDefaultValues";
            var dataTableDefinitionAlias = "create-dt-with-default-values";
            string jsonTableSchema = @"{""columns"":[{""name"":""Has Value"",""alias"":""hasValue"",""dataType"":""text""},{""name"":""Test Boolean"",""alias"":""testBoolean"",""dataType"":""boolean"",""defaultValue"":true},{""name"":""Test Text"",""alias"":""testText"",""dataType"":""text"",""defaultValue"":""Something with (*&(*#@) 0293849283  special characters""},{""name"":""Test Name"",""alias"":""testName"",""dataType"":""name"",""defaultValue"":""Jim""},{""name"":""Test Full Name"",""alias"":""testFullName"",""dataType"":""fullName"",""defaultValue"":""Jim Jones""},{""name"":""Test Phone Number"",""alias"":""testPhoneNumber"",""dataType"":""phoneNumber"",""defaultValue"":""0412876123""},{""name"":""Test Email Address"",""alias"":""testEmailAddress"",""dataType"":""emailAddress"",""defaultValue"":""jim.jones@test.com.au""},{""name"":""Test Website Address"",""alias"":""testWebsiteAddress"",""dataType"":""websiteAddress"",""defaultValue"":""www.mysite.org""},{""name"":""Test Payment Card Type"",""alias"":""testPaymentCardType"",""dataType"":""paymentCardType"",""defaultValue"":""mastercard""},{""name"":""Test Payment Card Number"",""alias"":""testPaymentCardNumber"",""dataType"":""paymentCardNumber"",""defaultValue"":""4444333322221111""},{""name"":""Test Payment Card Expiry Date"",""alias"":""testPaymentCardExpiryDate"",""dataType"":""paymentCardExpiryDate"",""defaultValue"":""03/2024""},{""name"":""Test Payment Card Verfication Code"",""alias"":""testPaymentCardVerificationCode"",""dataType"":""paymentCardVerificationCode"",""defaultValue"":""123""},{""name"":""Test Bank Account Number"",""alias"":""testBankAccountNumber"",""dataType"":""bankAccountNumber"",""defaultValue"":""1234567823""},{""name"":""Test Bank State Branch Number"",""alias"":""testBankStateBranchNumber"",""dataType"":""bankStateBranchNumber"",""defaultValue"":""042134""},{""name"":""Test Australian Company Number"",""alias"":""testAustralianCompanyNumber"",""dataType"":""australianCompanyNumber"",""defaultValue"":""987234876""},{""name"":""Test Australian Business Number"",""alias"":""testAustralianBusinessNumber"",""dataType"":""australianBusinessNumber"",""defaultValue"":""41123987456""},{""name"":""Test Vehicle Registration Number"",""alias"":""testVehicleRegistrationNumber"",""dataType"":""vehicleRegistrationNumber"",""defaultValue"":""ABC123""},{""name"":""Test Postal Code"",""alias"":""testPostalCode"",""dataType"":""postalCode"",""defaultValue"":""3000""},{""name"":""Test Number"",""alias"":""testNumber"",""dataType"":""number"",""defaultValue"":4566},{""name"":""Test Whole Number"",""alias"":""testWholeNumber"",""dataType"":""wholeNumber"",""defaultValue"":4},{""name"":""Test Decimal Number"",""alias"":""testDecimalNumber"",""dataType"":""decimalNumber"",""defaultValue"":34535345345.0},{""name"":""Test Percentage"",""alias"":""testPercentage"",""dataType"":""percentage"",""defaultValue"":0.567},{""name"":""Test Monetary Amount"",""alias"":""testMonetaryAmount"",""dataType"":""monetaryAmount"",""defaultValue"":122000.00},{""name"":""Test Date"",""alias"":""testDate"",""dataType"":""date"",""defaultValue"":""2023-11-17""},{""name"":""Test Time"",""alias"":""testTime"",""dataType"":""time"",""defaultValue"":""03:26:48.9874123""},{""name"":""Test Date Time"",""alias"":""testDateTime"",""dataType"":""dateTime"",""defaultValue"":""2023-11-17T03:26:48.9874123+10:00""}]}";
            var tableSchema = JsonConvert.DeserializeObject<DataTableSchema>(jsonTableSchema);
            string csvData = @"testBoolean,testText,testName,testFullName,testPhoneNumber,testEmailAddress,testWebsiteAddress,testPaymentCardNumber,testPaymentCardType,testPaymentCardExpiryDate,testPaymentCardVerificationCode,testBankAccountNumber,testBankStateBranchNumber,testAustralianCompanyNumber,testAustralianBusinessNumber,testVehicleRegistrationNumber,testPostalCode,testNumber,testWholeNumber,testDecimalNumber,testPercentage,testMonetaryAmount,testDate,testTime,testDateTime
true,Something with (*&(*#@) 0293849283  special characters,Jim,Jim Jones,412876123,jim.jones@test.com.au,www.mysite.org,4444333322221111,visa,3/2024,123,1234567823,042134,987234876,41123987456,ABC123,3000,4566,4,34535345345.0,0.567,122000.00,2023-11-17,03:26:48.9874123,2023-11-17T03:26:48.9874123+10:00
false,Another one without special chars,Felicity,Felicity Adams,61383746312,felix3847@gmail.com,https://my.space/something.html,371111111111114,mastercard,05/25,8765,234236234556,063888,123789435,88123583423,CUSTOMZ,0800,7656.234,2356666643345,3453.34534534534,1.32345,4,1976-01-14,15:58:01,1976-01-14T15:58:01
";
            var command = new CreateDataTableFromCsvDataCommand(
                this.tenant.Id,
                EntityType.Tenant,
                this.tenant.Id,
                dataTableDefinitionName,
                dataTableDefinitionAlias,
                csvData,
                tableSchema,
                false,
                0);

            // Act
            var result = await this.sut.Handle(command, CancellationToken.None);
            var dataTableContent = this.dataTableContentRepository.GetAllDataTableContent(result.Id, false, 0);

            // Assert
            dataTableContent.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateDataTableFromCsvCommand_ShouldCreateDataTableContent_WhenConfigurationHasIndexes()
        {
            var dataTableDefinitionName = "CreateDataTableWithIndexes";
            var dataTableDefinitionAlias = "create-dt-with-indexes";
            string jsonTableSchema = @"{""columns"":[{""name"":""Test Boolean"",""alias"":""testBoolean"",""dataType"":""boolean""},{""name"":""Test Vehicle Registration Number"",""alias"":""testVehicleRegistrationNumber"",""dataType"":""vehicleRegistrationNumber""},{""name"":""Test Phone Number"",""alias"":""testPhoneNumber"",""dataType"":""phoneNumber""},{""name"":""Test Name"",""alias"":""testName"",""dataType"":""name""},{""name"":""Test Full Name"",""alias"":""testFullName"",""dataType"":""fullName""},{""name"":""Test Address"",""alias"":""testAddress"",""dataType"":""text""}],""clusteredIndex"":{""name"":""Test Phone Index"",""alias"":""testPhoneNumberIndex"",""keyColumns"":[{""columnAlias"":""testPhoneNumber"",""sortOrder"":""asc""}]},""unclusteredIndexes"":[{""name"":""Vehicle"",""alias"":""testVehicle"",""keyColumns"":[{""columnAlias"":""testVehicleRegistrationNumber"",""sortOrder"":""asc""}],""nonKeyColumns"":[""testFullName"",""testAddress""]}]}";
            var tableSchema = JsonConvert.DeserializeObject<DataTableSchema>(jsonTableSchema);
            string csvData = @"testBoolean,testVehicleRegistrationNumber,testPhoneNumber,testName,testFullName,testAddress
false,LKW-954,0412 410 516,Aubry,Aubry Swaffield,6428 Florence Avenue
false,ZMJ-620,0447 666 855,Rebeca,Rebeca Goudie,2080 Elmside Place
false,QEB-616,0465 244 939,Baron,Baron Beddin,52395 Talisman Alley
true,MOY-503,0423 230 140,Lemmy,Lemmy Cowland,19 Warner Circle
false,CTC-947,0455 780 269,Darin,Darin Bussy,88765 Starling Court
";
            var command = new CreateDataTableFromCsvDataCommand(
                this.tenant.Id,
                EntityType.Tenant,
                this.tenant.Id,
                dataTableDefinitionName,
                dataTableDefinitionAlias,
                csvData,
                tableSchema,
                false,
                0);

            // Act
            var result = await this.sut.Handle(command, CancellationToken.None);
            var dataTableContent = this.dataTableContentRepository.GetAllDataTableContent(result.Id, false, 0);

            // Assert
            dataTableContent.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateDataTableFromCsvCommand_ShouldThrowError_WhenAliasIsAlreadyInUse()
        {
            // Arrange
            var dataTableDefinitionName = "Tenant DT Five";
            var dataTableDefinitionAlias = "existing-alias";
            string jsonTableSchema = @"{""columns"":[{""name"":""Name"",""alias"":""name"",""dataType"":""name""},{""name"":""Age"",""alias"":""age"",""dataType"":""wholeNumber""},{""name"":""Gender"",""alias"":""gender"",""dataType"":""text""}]}";
            var tableSchema = JsonConvert.DeserializeObject<DataTableSchema>(jsonTableSchema);
            string csvData = @"Name,Age,Gender
John,28,Male
Jane,32,Female";
            var databaseTableName = $"{this.PascalizeAlias(this.tenant.Details.Alias)}_{this.PascalizeAlias(dataTableDefinitionAlias)}";
            var existingDataTableDefinitionCommand = new CreateDataTableFromCsvDataCommand(
                this.tenant.Id,
                EntityType.Tenant,
                this.tenant.Id,
                dataTableDefinitionName,
                dataTableDefinitionAlias,
                csvData,
                tableSchema,
                false,
                0);
            var existingDataTableDefinitionResult = await this.sut.Handle(existingDataTableDefinitionCommand, CancellationToken.None);
            var dataTableDefinitionNameNew = "Tenant DT Six";
            var dataTableDefinitionAliasNew = dataTableDefinitionAlias;
            var command = new CreateDataTableFromCsvDataCommand(
                this.tenant.Id,
                EntityType.Tenant,
                this.tenant.Id,
                dataTableDefinitionNameNew,
                dataTableDefinitionAliasNew,
                csvData,
                tableSchema,
                false,
                0);

            // Act
            Func<Task> act = async () => await this.sut.Handle(command, CancellationToken.None);

            // Assert
            (await act.Should().ThrowAsync<ErrorException>())
                .And.Error.Code.Should().Be("data.table.definition.alias.in.use");
        }

        [Fact]
        public async Task CreateDataTableFromCsvCommand_ShouldThrowError_WhenNameIsAlreadyInUse()
        {
            // Arrange
            var dataTableDefinitionName = "Tenant DT Seven";
            var dataTableDefinitionAlias = "tenant-dt-seven";
            string jsonTableSchema = @"{""columns"":[{""name"":""Name"",""alias"":""name"",""dataType"":""name""},{""name"":""Gender"",""alias"":""gender"",""dataType"":""text""},{""name"":""Age"",""alias"":""age"",""dataType"":""wholeNumber""}]}";
            var tableSchema = JsonConvert.DeserializeObject<DataTableSchema>(jsonTableSchema);
            string csvData = @"Name,Age,Gender
John,28,Male
Jane,32,Female";
            var databaseTableName = $"{this.PascalizeAlias(this.tenant.Details.Alias)}_{this.PascalizeAlias(dataTableDefinitionAlias)}";
            var existingDataTableDefinitionCommand = new CreateDataTableFromCsvDataCommand(
                this.tenant.Id,
                EntityType.Tenant,
                this.tenant.Id,
                dataTableDefinitionName,
                dataTableDefinitionAlias,
                csvData,
                tableSchema,
                false,
                0);
            var existingDataTableDefinitionResult = await this.sut.Handle(existingDataTableDefinitionCommand, CancellationToken.None);
            var dataTableDefinitionNameNew = dataTableDefinitionName; // reuse name
            var dataTableDefinitionAliasNew = $"{dataTableDefinitionAlias}-new";
            var command = new CreateDataTableFromCsvDataCommand(
                this.tenant.Id,
                EntityType.Tenant,
                this.tenant.Id,
                dataTableDefinitionNameNew,
                dataTableDefinitionAliasNew,
                csvData,
                tableSchema,
                false,
                0);

            // Act
            Func<Task> act = async () => await this.sut.Handle(command, CancellationToken.None);

            // Assert
            (await act.Should().ThrowAsync<ErrorException>())
                .And.Error.Code.Should().Be("data.table.definition.name.in.use");
        }

        [Fact]
        public async Task CreateDataTableFromCsvCommand_ShouldThrowError_WhenEntityIsNotSupported()
        {
            // Arrange
            var dataTableDefinitionName = "Tenant DT Twelve";
            var dataTableDefinitionAlias = "tenant-dt-twelve";
            string jsonTableSchema = @"{""columns"":[{""name"":""Name"",""alias"":""name"",""dataType"":""name""},{""name"":""Gender"",""alias"":""gender"",""dataType"":""text""},{""name"":""Age"",""alias"":""age"",""dataType"":""wholeNumber""}]}";
            var tableSchema = JsonConvert.DeserializeObject<DataTableSchema>(jsonTableSchema);
            string csvData = @"Name,Age,Gender
John,28,Male
Jane,32,Female";
            var databaseTableName = $"{this.PascalizeAlias(this.tenant.Details.Alias)}_{this.PascalizeAlias(dataTableDefinitionAlias)}";
            var command = new CreateDataTableFromCsvDataCommand(
                this.tenant.Id,
                EntityType.Customer,
                Guid.NewGuid(),
                dataTableDefinitionName,
                dataTableDefinitionAlias,
                csvData,
                tableSchema,
                false,
                0);

            // Act
            Func<Task> act = async () => await this.sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotSupportedException>();
        }

        private CreateDataTableFromCsvDataCommandHandler Sut()
        {
            this.cachingResolver = new Mock<ICachingResolver>();
            this.tenant = TenantFactory.Create(Guid.NewGuid(), "datatable-tenant-one");
            this.organisation = new OrganisationReadModel(
                this.tenant.Id, Guid.NewGuid(), "datatable-org-one", "Data Table Org One", null, true, false, SystemClock.Instance.Now());

            this.product = ProductFactory.Create(this.tenant.Id, Guid.NewGuid(), "datatable-product-one");
            this.dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);

            this.cachingResolver.Setup(cr => cr.GetTenantOrNull(this.tenant.Id)).Returns(Task.FromResult(this.tenant));
            this.cachingResolver.Setup(cr => cr.GetTenantOrThrow(this.tenant.Id)).Returns(Task.FromResult(this.tenant));
            this.cachingResolver.Setup(cr => cr.GetProductOrNull(this.tenant.Id, this.product.Id)).Returns(Task.FromResult(this.product));
            this.cachingResolver.Setup(cr => cr.GetProductOrThrow(this.tenant.Id, this.product.Id)).Returns(Task.FromResult(this.product));
            this.cachingResolver.Setup(cr => cr.GetOrganisationOrNull(this.tenant.Id, this.organisation.Id)).Returns(Task.FromResult(this.organisation));
            this.cachingResolver.Setup(cr => cr.GetOrganisationOrThrow(this.tenant.Id, this.organisation.Id)).Returns(Task.FromResult(this.organisation));

            var dataTableConfiguration = new DataTableContentDbConfiguration(DatabaseFixture.TestConnectionString);

            var dataTableDefinitionRepository = new DataTableDefinitionRepository(this.dbContext);
            var tenantRepository = new TenantRepository(this.dbContext);
            var organisationReadModelRepository = new OrganisationReadModelRepository(this.dbContext);
            var productRepository = new ProductRepository(this.dbContext);
            var mockOrganisationSystemEventEmitter = new Mock<IOrganisationSystemEventEmitter>();
            var mockHttpContextPropertiesResolver = new Mock<IHttpContextPropertiesResolver>();
            var mockHostingEnvironment = new Mock<IWebHostEnvironment>();
            var mockLogger = new Mock<ILogger<DataTableDefinitionService>>();
            mockHostingEnvironment.Setup(m => m.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            this.dataTableContentDbFactory = new DataTableContentDbFactory(dataTableConfiguration);
            this.dataTableContentRepository = new DataTableContentRepository(this.dbContext, this.dataTableContentDbFactory);
            var dataTableDefinitionService = new DataTableDefinitionService(
                dataTableDefinitionRepository,
                this.dataTableContentRepository,
                tenantRepository,
                organisationReadModelRepository,
                productRepository,
                this.cachingResolver.Object,
                mockHostingEnvironment.Object,
                mockLogger.Object);
            this.mockTenantSystemEventEmitter = new Mock<ITenantSystemEventEmitter>();
            this.mockOrganisationSystemEventEmitter = new Mock<IOrganisationSystemEventEmitter>();
            return new CreateDataTableFromCsvDataCommandHandler(
                dataTableDefinitionRepository,
                this.dataTableContentRepository,
                dataTableDefinitionService,
                SystemClock.Instance,
                this.mockTenantSystemEventEmitter.Object,
                this.mockOrganisationSystemEventEmitter.Object,
                this.dbContext);
        }

        private string PascalizeAlias(string alias)
        {
            return alias.Replace('-', '_').Pascalize();
        }
    }
}

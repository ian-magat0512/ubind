// <copyright file="DataTableDefinitionServiceTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using CsvHelper;
    using FluentAssertions;
    using Humanizer;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.Extensions;
    using UBind.Domain.Models.DataTable;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class DataTableDefinitionServiceTests
    {
        private readonly Mock<IDataTableDefinitionRepository> dataTableDefinitionRepository = new Mock<IDataTableDefinitionRepository>();
        private readonly Mock<IDataTableContentRepository> dataTableContentRepository = new Mock<IDataTableContentRepository>();
        private readonly Mock<ITenantRepository> tenantRepository = new Mock<ITenantRepository>();
        private readonly Mock<IOrganisationReadModelRepository> organisationReadModelRepository = new Mock<IOrganisationReadModelRepository>();
        private readonly Mock<IProductRepository> productRepository = new Mock<IProductRepository>();
        private readonly Mock<ICachingResolver> cachingResolver = new Mock<ICachingResolver>();
        private Tenant tenant;
        private Product product;
        private OrganisationReadModel organisation;

        public DataTableDefinitionServiceTests()
        {
            this.tenant = TenantFactory.Create(Guid.NewGuid(), "carl");
            this.product = ProductFactory.Create(this.tenant.Id, Guid.NewGuid(), "dev");
            this.organisation = new OrganisationReadModel(
                this.tenant.Id, Guid.NewGuid(), "test", "Test", null, true, false, SystemClock.Instance.Now());

            var dataTableDataTypeRegistry = new DataTableDataTypeSqlSettingsRegistry();
            DataTableDataTypeExtensions.SetDataTypeSqlSettingsRegistry(dataTableDataTypeRegistry);
        }

        [Fact]
        public async Task GenerateDatabaseTableName_ShouldCreateCorrectDataTableName_ForTenantEntity()
        {
            // Arrange
            this.dataTableDefinitionRepository
               .Setup(dtf => dtf.GetDataTableDefinitionsByDatabaseTableName(this.tenant.Id, It.IsAny<string>()))
               .Returns(new List<DataTableDefinition>());
            var sut = this.Sut();
            var dataTableDefinition = this.FakeDataTableDefinition("Contacts", "contacts", EntityType.Tenant, this.tenant.Id);
            var expectedDatabaseTableName = $"{this.PascalizeAlias(this.tenant.Details.Alias)}_{this.PascalizeAlias(dataTableDefinition.Alias)}";

            // Act
            var result = await sut.GenerateDatabaseTableName(dataTableDefinition);

            // Assert
            result.Should().Be(expectedDatabaseTableName);
        }

        [Fact]
        public async Task GenerateDatabaseTableName_ShouldCreateCorrectDataTableName_ForProductEntity()
        {
            // Arrange
            this.dataTableDefinitionRepository
               .Setup(dtf => dtf.GetDataTableDefinitionsByDatabaseTableName(this.tenant.Id, It.IsAny<string>()))
               .Returns(new List<DataTableDefinition>());
            var sut = this.Sut();
            var dataTableDefinition = this.FakeDataTableDefinition("Contacts", "contacts", EntityType.Product, this.product.Id);
            var expectedDatabaseTableName =
                $"{this.PascalizeAlias(this.tenant.Details.Alias)}" +
                $"_Product_{this.PascalizeAlias(this.product.Details.Alias)}_{this.PascalizeAlias(dataTableDefinition.Alias)}";

            // Act
            var result = await sut.GenerateDatabaseTableName(dataTableDefinition);

            // Assert
            result.Should().Be(expectedDatabaseTableName);
        }

        [Fact]
        public async Task GenerateDatabaseTableName_ShouldCreateCorrectDataTableName_ForOrganisationEntity()
        {
            // Arrange
            this.dataTableDefinitionRepository
               .Setup(dtf => dtf.GetDataTableDefinitionsByDatabaseTableName(this.tenant.Id, It.IsAny<string>()))
               .Returns(new List<DataTableDefinition>());
            var sut = this.Sut();
            var dataTableDefinition = this.FakeDataTableDefinition("Contacts", "contacts", EntityType.Organisation, this.organisation.Id);
            var expectedDatabaseTableName =
                $"{this.PascalizeAlias(this.tenant.Details.Alias)}" +
                $"_Organisation_{this.PascalizeAlias(this.organisation.Alias)}_{this.PascalizeAlias(dataTableDefinition.Alias)}";

            // Act
            var result = await sut.GenerateDatabaseTableName(dataTableDefinition);

            // Assert
            result.Should().Be(expectedDatabaseTableName);
        }

        [Fact]
        public async Task GenerateDatabaseTableName_ShouldTruncateTableName_WhenExceed120Characters()
        {
            // Arrange
            this.dataTableDefinitionRepository
               .Setup(dtf => dtf.GetDataTableDefinitionsByDatabaseTableName(this.tenant.Id, It.IsAny<string>()))
               .Returns(new List<DataTableDefinition>());
            var sut = this.Sut();
            var dataTableDefinition = this.FakeDataTableDefinition(
                "Contacts",
                "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcd",
                EntityType.Organisation,
                this.organisation.Id);
            var expectedDatabaseTableName =
                $"{this.PascalizeAlias(this.tenant.Details.Alias)}" +
                $"_Organisation_{this.PascalizeAlias(this.organisation.Alias)}_{this.PascalizeAlias(dataTableDefinition.Alias)}";
            expectedDatabaseTableName = expectedDatabaseTableName.Substring(0, 124);

            // Act
            var result = await sut.GenerateDatabaseTableName(dataTableDefinition);

            // Assert
            result.Should().Be(expectedDatabaseTableName);
        }

        [Fact]
        public async Task GenerateDatabaseTableName_ShouldAddIndex_TableHaveTheSameName()
        {
            // Arrange
            var existingDataTableDefinition = this.FakeDataTableDefinition(
                "Contacts",
                "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcd",
                EntityType.Organisation,
                this.organisation.Id);
            this.dataTableDefinitionRepository
                .Setup(dtf => dtf.GetDataTableDefinitionsByDatabaseTableName(this.tenant.Id, It.IsAny<string>()))
                .Returns(new List<DataTableDefinition>());
            var existingDatabaseTableName = await this.Sut().GenerateDatabaseTableName(existingDataTableDefinition);
            existingDataTableDefinition.UpdateDatabaseTableName(existingDatabaseTableName);
            var existingDataTableDefinitions = new List<DataTableDefinition>();
            existingDataTableDefinitions.Add(existingDataTableDefinition);

            this.dataTableDefinitionRepository
                .Setup(dtf => dtf.GetDataTableDefinitionsByDatabaseTableName(this.tenant.Id, It.IsAny<string>()))
                .Returns(existingDataTableDefinitions);
            var sut = this.Sut();
            var dataTableDefinition = this.FakeDataTableDefinition(
                "Contacts-New",
                "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcd-new",
                EntityType.Organisation,
                this.organisation.Id);
            var expectedDatabaseTableName =
                $"{this.PascalizeAlias(this.tenant.Details.Alias)}" +
                $"_Organisation_{this.PascalizeAlias(this.organisation.Alias)}_{this.PascalizeAlias(dataTableDefinition.Alias)}";
            expectedDatabaseTableName = $"{expectedDatabaseTableName.Substring(0, 124)}0001";

            // Act
            var result = await sut.GenerateDatabaseTableName(dataTableDefinition);

            // Assert
            result.Should().Be(expectedDatabaseTableName);
        }

        [Fact]
        public async Task GenerateDatabaseTableName_ShouldIncremenetIndex_MutipleTableHaveTheSameName()
        {
            // Arrange
            var existingDataTableDefinition = this.FakeDataTableDefinition(
                "Contacts",
                "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcd",
                EntityType.Tenant,
                this.tenant.Id);
            this.dataTableDefinitionRepository
                .Setup(dtf => dtf.GetDataTableDefinitionsByDatabaseTableName(this.tenant.Id, It.IsAny<string>()))
                .Returns(new List<DataTableDefinition>());
            var existingDatabaseTableName = await this.Sut().GenerateDatabaseTableName(existingDataTableDefinition);
            existingDataTableDefinition.UpdateDatabaseTableName(existingDatabaseTableName);
            var existingDataTableDefinitions = new List<DataTableDefinition>();
            existingDataTableDefinitions.Add(existingDataTableDefinition);

            var anotherExistingDataTableDefinition = this.FakeDataTableDefinition(
                "Contacts-One",
               "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcd-one",
                EntityType.Organisation,
                this.organisation.Id);
            this.dataTableDefinitionRepository
                .Setup(dtf => dtf.GetDataTableDefinitionsByDatabaseTableName(this.tenant.Id, It.IsAny<string>()))
                .Returns(existingDataTableDefinitions);
            var anotherExistingDatabaseTableName = await this.Sut().GenerateDatabaseTableName(anotherExistingDataTableDefinition);
            anotherExistingDataTableDefinition.UpdateDatabaseTableName(anotherExistingDatabaseTableName);
            existingDataTableDefinitions.Add(anotherExistingDataTableDefinition);
            existingDataTableDefinitions = existingDataTableDefinitions.OrderByDescending(d => d.CreatedTicksSinceEpoch).ToList();

            this.dataTableDefinitionRepository
                .Setup(dtf => dtf.GetDataTableDefinitionsByDatabaseTableName(this.tenant.Id, It.IsAny<string>()))
                .Returns(existingDataTableDefinitions);
            var sut = this.Sut();
            var dataTableDefinition = this.FakeDataTableDefinition(
                "Contacts-Two",
                "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcd-two",
                EntityType.Organisation,
                this.organisation.Id);
            var expectedDatabaseTableName =
                $"{this.PascalizeAlias(this.tenant.Details.Alias)}" +
                $"_Organisation_{this.PascalizeAlias(this.organisation.Alias)}_{this.PascalizeAlias(dataTableDefinition.Alias)}";
            expectedDatabaseTableName = $"{expectedDatabaseTableName.Substring(0, 124)}0002";

            // Act
            var result = await sut.GenerateDatabaseTableName(dataTableDefinition);

            // Assert
            result.Should().Be(expectedDatabaseTableName);
        }

        [Fact]
        public void ValidateJsonIndexesAndThrow_ShouldNotThrow_WhenJsonConfigurationIsValid()
        {
            // Arrange
            var stringConfig = @"{""columns"":[{""name"":""Test Boolean"",""alias"":""testBoolean"",""dataType"":""boolean"",""unique"":true},{""name"":""Test Text"",""alias"":""testText"",""dataType"":""text""},{""name"":""Test Name"",""alias"":""testName"",""dataType"":""name""},{""name"":""Test Full Name"",""alias"":""testFullName"",""dataType"":""fullName""},{""name"":""Test Address"",""alias"":""testAddress"",""dataType"":""text""}],""clusteredIndex"":{""name"":""Test Full Name Index"",""alias"":""testFullNameIndex"",""keyColumns"":[{""columnAlias"":""testFullName"",""sortOrder"":""asc""}]},""unclusteredIndexes"":[{""name"":""Name\/Text"",""alias"":""nameTextIndex"",""keyColumns"":[{""columnAlias"":""testName"",""sortOrder"":""asc""}],""nonKeyColumns"":[""testAddress"",]}]}";
            var config = this.ParseJsonDataTableSchema(stringConfig);
            var sut = this.Sut();

            // Act & Assert
            var ex = Record.Exception(() => sut.ValidateTableSchemaIndexes(config));
            Assert.Null(ex);
        }

        [Fact]
        public void ValidateJsonIndexesAndThrow_ShouldThrow_WhenClusteredIndexIsInvalid()
        {
            // Arrange
            var stringConfig = @"{""columns"":[{""name"":""Test Boolean"",""alias"":""testBoolean"",""dataType"":""boolean"",""unique"":true},{""name"":""Test Text"",""alias"":""testText"",""dataType"":""text""},{""name"":""Test Name"",""alias"":""testName"",""dataType"":""name""},{""name"":""Test Full Name"",""alias"":""testFullName"",""dataType"":""fullName""},{""name"":""Test Address"",""alias"":""testAddress"",""dataType"":""text""}],""clusteredIndex"":{""name"":""Test Current Address Index"",""alias"":""testCurrentAddressIndex"",""keyColumns"":[{""columnAlias"":""testCurrentAddress"",""sortOrder"":""asc""}]},""unclusteredIndexes"":[{""name"":""Name\/Text"",""alias"":""nameTextIndex"",""keyColumns"":[{""columnAlias"":""testName"",""sortOrder"":""asc""},{""columnAlias"":""testText"",""sortOrder"":""asc""}],""nonKeyColumns"":[""testAddress"",]}]}";
            var config = this.ParseJsonDataTableSchema(stringConfig);
            var sut = this.Sut();

            // Act & Assert
            var ex = Record.Exception(() => sut.ValidateTableSchemaIndexes(config));

            // TODO: check the specific thrown error
            Assert.NotNull(ex);
        }

        [Fact]
        public void ValidateJsonIndexesAndThrow_ShouldThrow_WhenUnclusteredIndexKeyColumnIsInvalid()
        {
            // Arrange
            var stringConfig = @"{""columns"":[{""name"":""Test Boolean"",""alias"":""testBoolean"",""dataType"":""boolean"",""unique"":true},{""name"":""Test Text"",""alias"":""testText"",""dataType"":""text""},{""name"":""Test Name"",""alias"":""testName"",""dataType"":""name""},{""name"":""Test Full Name"",""alias"":""testFullName"",""dataType"":""fullName""},{""name"":""Test Address"",""alias"":""testAddress"",""dataType"":""name""},],""clusteredIndex"":{""name"":""Test Full Name Index"",""alias"":""testFullNameIndex"",""keyColumns"":[{""columnAlias"":""testFullName"",""sortOrder"":""asc""}]},""unclusteredIndexes"":[{""name"":""Nickname/Text"",""alias"":""testNicknameIndex"",""keyColumns"":[{""columnAlias"":""testNickname"",""sortOrder"":""asc""},{""columnAlias"":""testText"",""sortOrder"":""asc""}],""nonKeyColumns"":[""testAddress"",""testBoolean""]}]}";
            var config = this.ParseJsonDataTableSchema(stringConfig);
            var sut = this.Sut();

            // Act & Assert
            var ex = Record.Exception(() => sut.ValidateTableSchemaIndexes(config));

            // TODO: check the specific thrown error
            Assert.NotNull(ex);
        }

        [Fact]
        public void ValidateJsonIndexesAndThrow_ShouldThrow_WhenUnclusteredIndexNonkeyColumnIsInvalid()
        {
            // Arrange
            var stringConfig = @"{""columns"":[{""name"":""Test Boolean"",""alias"":""testBoolean"",""dataType"":""boolean"",""unique"":true},{""name"":""Test Text"",""alias"":""testText"",""dataType"":""text""},{""name"":""Test Name"",""alias"":""testName"",""dataType"":""name""},{""name"":""Test Full Name"",""alias"":""testFullName"",""dataType"":""fullName""},{""name"":""Test Address"",""alias"":""testAddress"",""dataType"":""name""}],""clusteredIndex"":{""name"":""Test Full Name Index"",""alias"":""testFullNameIndex"",""keyColumns"":[{""columnAlias"":""testFullName"",""sortOrder"":""asc""}]},""unclusteredIndexes"":[{""name"":""Name/Text"",""alias"":""testNameIndex"",""keyColumns"":[{""columnAlias"":""testName"",""sortOrder"":""asc""},{""columnAlias"":""testText"",""sortOrder"":""asc""}],""nonKeyColumns"":[""testCurrentAddress"",""testNickname""]}]}";
            var config = this.ParseJsonDataTableSchema(stringConfig);
            var sut = this.Sut();

            // Act & Assert
            var ex = Record.Exception(() => sut.ValidateTableSchemaIndexes(config));
            Assert.NotNull(ex);
        }

        [Fact]
        public void CreateDataTableFromCsvString_ShouldReturnDataTable()
        {
            // Arrange
            string jsonStringConfig = @"{""columns"":[{""name"":""Name"",""alias"":""name"",""dataType"":""name""},{""name"":""Age"",""alias"":""age"",""dataType"":""wholeNumber""},{""name"":""Gender"",""alias"":""gender"",""dataType"":""text""}]}";
            var config = this.ParseJsonDataTableSchema(jsonStringConfig);
            string csvData = @"Name,Age,Gender
John,28,Male
Jane,32,Female";
            var sut = this.Sut();

            // Act
            var dataTable = sut.CreateDataTableFromCsv(config, csvData);

            // Assert
            Assert.IsType<System.Data.DataTable>(dataTable);
        }

        [Fact]
        public void CreateDataTableFromCsvString_ShouldReturnDataTableWithCorrectNumberOfRowsAndColumns()
        {
            // Arrange
            string jsonStringConfig = @"{""columns"":[{""name"":""Name"",""alias"":""name"",""dataType"":""name""},{""name"":""Age"",""alias"":""age"",""dataType"":""wholeNumber""},{""name"":""Gender"",""alias"":""gender"",""dataType"":""text""}]}";
            var config = this.ParseJsonDataTableSchema(jsonStringConfig);
            string csvData = @"Name,Age,Gender
John,28,Male
Jane,32,Female";
            int expectedColumnCount = 3;
            int expectedRowCount = 2;
            var sut = this.Sut();

            // Act
            var dataTable = sut.CreateDataTableFromCsv(config, csvData);

            // Assert
            Assert.True(expectedColumnCount == dataTable.Columns.Count
                && expectedRowCount == dataTable.Rows.Count);
        }

        [Fact]
        public void CreateDataTableFromCsvString_ShouldThrow_WhenRequiredColumnIsMissingFromCsvData()
        {
            // Arrange
            var stringConfig = @"{""columns"":[{""name"":""Name"",""alias"":""name"",""dataType"":""name"",""required"":true},{""name"":""Gender"",""alias"":""gender"",""dataType"":""text""},{""name"":""Age"",""alias"":""age"",""dataType"":""wholeNumber""}]}";
            var invalidCsvData = @"Age,Gender
23,Male";
            var config = this.ParseJsonDataTableSchema(stringConfig);
            var sut = this.Sut();

            // Act & Assert
            var ex = Record.Exception(() => sut.CreateDataTableFromCsv(config, invalidCsvData));
            Assert.NotNull(ex);
        }

        [Fact]
        public void CreateDataTableFromCsvString_ShouldThrow_WhenCsvColumnDoesNotExistsInTheConfig()
        {
            // Arrange
            var stringConfig = @"{""columns"":[{""name"":""Name"",""alias"":""name"",""dataType"":""name""},{""name"":""Gender"",""alias"":""gender"",""dataType"":""text""},{""name"":""Age"",""alias"":""age"",""dataType"":""wholeNumber""}]}";
            var invalidCsvData = @"Name,Age,Gender,Address
Jill,23,Female,123 Fortune St.";
            var config = this.ParseJsonDataTableSchema(stringConfig);
            var sut = this.Sut();

            // Act & Assert
            var ex = Record.Exception(() => sut.CreateDataTableFromCsv(config, invalidCsvData));
            Assert.NotNull(ex);
        }

        [Fact]
        public void ValidateColumnNamesAndAliases_ShouldNotThrow_WhenColumnAliasesAndNamesAreValid()
        {
            // Arrange
            var stringConfig = @"{""columns"":[{""name"":""Name"",""alias"":""name"",""dataType"":""name""},{""name"":""Gender"",""alias"":""gender"",""dataType"":""text""},{""name"":""Age"",""alias"":""age"",""dataType"":""wholeNumber""}]}";
            var config = this.ParseJsonDataTableSchema(stringConfig);
            var sut = this.Sut();

            // Act & Assert
            var ex = Record.Exception(() => sut.ValidateColumns(config));
            Assert.Null(ex);
        }

        [Fact]
        public void ValidateColumnNamesAndAliases_ShouldThrow_WhenThereIsDuplicateColumnAlias()
        {
            // Arrange
            var stringConfig = @"{""columns"":[{""name"":""Name"",""alias"":""name"",""dataType"":""name""},{""name"":""Name2"",""alias"":""naMe"",""dataType"":""name""},{""name"":""Gender"",""alias"":""gender"",""dataType"":""text""},{""name"":""Age"",""alias"":""age"",""dataType"":""wholeNumber""}]}";
            var config = this.ParseJsonDataTableSchema(stringConfig);
            var sut = this.Sut();

            // Act & Assert
            var ex = Record.Exception(() => sut.ValidateColumns(config));
            Assert.NotNull(ex);
        }

        [Fact]
        public void ValidateColumnNamesAndAliases_ShouldThrow_WhenThereIsDuplicateColumnName()
        {
            // Arrange
            var stringConfig = @"{""columns"":[{""name"":""Name"",""alias"":""name"",""dataType"":""name""},{""name"":""NAME"",""alias"":""name2"",""dataType"":""name""},{""name"":""Gender"",""alias"":""gender"",""dataType"":""text""},{""name"":""Age"",""alias"":""age"",""dataType"":""wholeNumber""}]}";
            var config = this.ParseJsonDataTableSchema(stringConfig);
            var sut = this.Sut();

            // Act & Assert
            var ex = Record.Exception(() => sut.ValidateColumns(config));
            Assert.NotNull(ex);
        }

        private DataTableDefinitionService Sut()
        {
            var mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
            var mockLogger = new Mock<ILogger<DataTableDefinitionService>>();
            mockWebHostEnvironment.Setup(m => m.ContentRootPath).Returns(Directory.GetCurrentDirectory());

            this.tenantRepository.Setup(r => r.GetTenantAliasById(this.tenant.Id))
                .ReturnsAsync(this.tenant.Details.Alias);
            this.organisationReadModelRepository.Setup(r => r.GetOrganisationAliasById(
                this.tenant.Id, this.organisation.Id))
                .ReturnsAsync(this.organisation.Alias);
            this.productRepository.Setup(r => r.GetProductAliasById(this.tenant.Id, this.product.Id))
                .ReturnsAsync(this.product.Details.Alias);

            return new DataTableDefinitionService(
                this.dataTableDefinitionRepository.Object,
                this.dataTableContentRepository.Object,
                this.tenantRepository.Object,
                this.organisationReadModelRepository.Object,
                this.productRepository.Object,
                this.cachingResolver.Object,
                mockWebHostEnvironment.Object,
                mockLogger.Object);
        }

        private DataTableDefinition FakeDataTableDefinition(string name, string alias, EntityType entityType, Guid entityId)
        {
            string jsonConfig = @"{""columns"":[{""name"":""Test Boolean"",""alias"":""testBoolean"",""dataType"":""boolean"",""unique"":true},{""name"":""Test Text"",""alias"":""testText"",""dataType"":""text""},{""name"":""Test Name"",""alias"":""testName"",""dataType"":""name""},{""name"":""Test Full Name"",""alias"":""testFullName"",""dataType"":""fullName""},{""name"":""Test Phone Number"",""alias"":""testPhoneNumber"",""dataType"":""phoneNumber""},{""name"":""Test Mobile Phone Number"",""alias"":""testMobilePhoneNumber"",""dataType"":""mobilePhoneNumber""}]}";
            return DataTableDefinition.Create(this.tenant.Id, entityType, entityId, name, alias, false, 0,
               jsonConfig, 6, 0, SystemClock.Instance.Now());
        }

        private DataTableSchema? ParseJsonDataTableSchema(string jsonString)
        {
            return JsonConvert.DeserializeObject<DataTableSchema>(
               jsonString, CustomSerializerSetting.JsonSerializerSettings);
        }

        private System.Data.DataTable ParseCsvStringToDataTable(string csvString)
        {
            System.Data.DataTable dataTable = new System.Data.DataTable();

            using (var reader = new StringReader(csvString))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            using (var dr = new CsvDataReader(csv))
            {
                dataTable.Load(dr);
            }
            return dataTable;
        }

        private string PascalizeAlias(string alias)
        {
            return alias.Replace('-', '_').Pascalize();
        }
    }
}

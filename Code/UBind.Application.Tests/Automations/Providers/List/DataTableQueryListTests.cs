// <copyright file="DataTableQueryListTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.List
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Filters;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Application.Automation.Providers.Expression;
    using UBind.Application.Automation.Providers.List;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.SerialisedEntitySchemaObject;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Helpers;
    using Xunit;

    public class DataTableQueryListTests
    {
        private Domain.Tenant tenant;
        private OrganisationReadModel organisation;
        Mock<IDataTableContentRepository> dataTableContentRepositoryMock = new Mock<IDataTableContentRepository>();

        [Fact]
        public async Task DynamicCollection_Count_ItemsCorrectly()
        {
            // Arrange
            var json = @"{
                            ""entity"": {
                                ""organisation"": {
                                    ""organisationAlias"": ""test""
                                }
                            },
                            ""dataTableAlias"": ""embargo""
                        }";
            var converters = AutomationDeserializationConfiguration.ModelSettings;
            var dataTableQueryListProviderBuilder = JsonConvert.DeserializeObject<DataTableQueryListProviderConfigModel>(json, converters);

            var mockServiceProvider = this.GenerateServiceProvider();
            var dataTableQueryListProvider = dataTableQueryListProviderBuilder.Build(mockServiceProvider);

            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(
                this.tenant.Id, this.organisation.Id);
            Expression<Func<object, bool>> expression = null;
            this.dataTableContentRepositoryMock
                .Setup(r => r.GetDataTableContentWithFilter(It.IsAny<Guid>(), It.IsAny<int?>(), It.IsAny<Expression<Func<object, bool>>>()))
                .Callback<Guid, int?, Expression<Func<object, bool>>>((p1, p2, p3) =>
                {
                    expression = p3;
                })
                .Returns(this.GetFakeDynamicData());

            // Act
            var result = await dataTableQueryListProvider.Resolve(new ProviderContext(automationData));

            // Assert
            int count = result.GetValueOrThrowIfFailed().Count();
            count.Should().Be(3);
            var sqlQueryBuilder = new SqlQueryBuilder("test");
            var query = sqlQueryBuilder.Build(expression);
            query.Should().Be("SELECT * FROM test;\r\n");
        }

        [Fact]
        public async Task DynamicCollection_Where_ShouldReturnFilteredResult_WhenValueExists()
        {
            // Arrange
            var json = @"{
                            ""entity"": {
                                ""organisation"": {
                                    ""organisationAlias"": ""test""
                                }
                            },
                            ""dataTableAlias"": ""embargo""
                        }";
            var converters = AutomationDeserializationConfiguration.ModelSettings;
            var dataTableQueryListProviderBuilder = JsonConvert.DeserializeObject<DataTableQueryListProviderConfigModel>(json, converters);

            var mockServiceProvider = this.GenerateServiceProvider();
            var dataTableQueryListProvider = dataTableQueryListProviderBuilder.Build(mockServiceProvider);

            var alias = "embargo";
            var columnToTest = "postcode";
            var valueToTest = "2150";
            var isEqualFilterProvider = this.EqualFilterProvider(columnToTest, valueToTest);

            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(
                this.tenant.Id, this.organisation.Id);

            var sut = (await dataTableQueryListProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            Expression<Func<object, bool>> expression = null;
            this.dataTableContentRepositoryMock
                .Setup(r => r.GetDataTableContentWithFilter(It.IsAny<Guid>(), It.IsAny<int?>(), It.IsAny<Expression<Func<object, bool>>>()))
                .Callback<Guid, int?, Expression<Func<object, bool>>>((p1, p2, p3) =>
                {
                    expression = p3;
                })
                .Returns(this.GetFakeDynamicData().Where((Func<dynamic, bool>)(x => x.Postcode == "2150")));

            // Act
            var filteredCollection = await sut.Where(alias, isEqualFilterProvider, new ProviderContext(automationData));

            // Asset
            filteredCollection.ToList<object>().Should().HaveCount(1);
            filteredCollection.ToList<object>()[0].Should().BeEquivalentTo(new { Postcode = valueToTest });
            var sqlQueryBuilder = new SqlQueryBuilder("test");
            var query = sqlQueryBuilder.Build(expression);
            query.Should().Be("SELECT * FROM test WHERE (postcode = '2150');\r\n");
        }

        [Fact]
        public async Task DynamicCollection_Where_ShouldReturnEmptyResult_WhenValueDoesNotExists()
        {
            // Arrange
            var json = @"{
                            ""entity"": {
                                ""organisation"": {
                                    ""organisationAlias"": ""test""
                                }
                            },
                            ""dataTableAlias"": ""embargo""
                        }";
            var converters = AutomationDeserializationConfiguration.ModelSettings;
            var dataTableQueryListProviderBuilder = JsonConvert.DeserializeObject<DataTableQueryListProviderConfigModel>(json, converters);
            var mockServiceProvider = this.GenerateServiceProvider();
            var provider = dataTableQueryListProviderBuilder.Build(mockServiceProvider);

            var alias = "embargo";
            var columnToTest = "postcode";
            var valueToTest = "4004";
            var isEqualFilterProvider = this.EqualFilterProvider(columnToTest, valueToTest);

            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(
               this.tenant.Id, this.organisation.Id);

            var sut = (await provider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            Expression<Func<object, bool>> expression = null;
            this.dataTableContentRepositoryMock
                .Setup(r => r.GetDataTableContentWithFilter(It.IsAny<Guid>(), It.IsAny<int?>(), It.IsAny<Expression<Func<object, bool>>>()))
                .Callback<Guid, int?, Expression<Func<object, bool>>>((p1, p2, p3) =>
                {
                    expression = p3;
                })
                .Returns(this.GetFakeDynamicData().Where((Func<dynamic, bool>)(x => x.Postcode == "4004")));

            // Act
            var filteredCollection = await sut.Where(alias, isEqualFilterProvider, new ProviderContext(automationData));

            // Asset
            filteredCollection.ToList<object>().Should().BeEmpty();
            var sqlQueryBuilder = new SqlQueryBuilder("test");
            var query = sqlQueryBuilder.Build(expression);
            query.Should().Be("SELECT * FROM test WHERE (postcode = '4004');\r\n");
        }

        [Fact]
        public async Task DynamicCollection_InsideFilterListItems_ShouldReturnFilteredResult_WhenValueExists()
        {
            // Arrange
            var valueToTest = "2150";
            var json = @"
                        {
                            ""list"": {
                                ""dataTableQueryList"": {
                                    ""entity"": {
                                        ""contextEntity"": ""/tenant""
                                    },
                                    ""dataTableAlias"": ""embargo""
                                }
                            },
                            ""condition"": {
                                ""textIsEqualToCondition"": {
                                    ""text"": {
                                        ""objectPathLookupText"": ""#/embargo/postcode""
                                    },
                                    ""isEqualTo"": """ + valueToTest + @"""
                                }
                            },
                            ""itemAlias"": ""embargo""
                                        }
                        ";

            var converters = AutomationDeserializationConfiguration.ModelSettings;
            var filterListItemProviderBuilder = JsonConvert.DeserializeObject<FilterListItemsListProviderConfigModel>(json, converters);
            var mockServiceProvider = this.GenerateServiceProvider();
            var filterListItemprovider = filterListItemProviderBuilder.Build(mockServiceProvider);

            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(
                this.tenant.Id, this.organisation.Id);
            Expression<Func<object, bool>> expression = null;
            var data = this.GetFakeDynamicData().Where((Func<dynamic, bool>)(x => x.Postcode == "2150"));
            this.dataTableContentRepositoryMock
                .Setup(r => r.GetDataTableContentWithFilter(It.IsAny<Guid>(), It.IsAny<int?>(), It.IsAny<Expression<Func<object, bool>>>()))
                .Callback<Guid, int?, Expression<Func<object, bool>>>((p1, p2, p3) =>
                {
                    expression = p3;
                })
                .Returns(data);

            // Act
            var entityCollection = (await filterListItemprovider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Asset
            entityCollection.ToList().Should().HaveCount(1);
            entityCollection.ToList()[0].Should().BeEquivalentTo(new { Postcode = valueToTest });
            var sqlQueryBuilder = new SqlQueryBuilder("test");
            var query = sqlQueryBuilder.Build(expression);
            query.Should().Be("SELECT * FROM test WHERE (postcode = '2150');\r\n");
        }

        [Fact]
        public async Task DynamicCollection_InsideFilterListItems_ShouldReturnEmptyResult_WhenValueDoesNotExists()
        {
            // Arrange
            var valueToTest = "4003";
            var json = @"
                        {
                            ""list"": {
                                ""dataTableQueryList"": {
                                    ""entity"": {
                                        ""contextEntity"": ""/tenant""
                                    },
                                    ""dataTableAlias"": ""embargo""
                                }
                            },
                            ""condition"": {
                                ""textIsEqualToCondition"": {
                                    ""text"": {
                                        ""objectPathLookupText"": ""#/embargo/postcode""
                                    },
                                    ""isEqualTo"": """ + valueToTest + @"""
                                }
                            },
                            ""itemAlias"": ""embargo""
                                        }
                        ";

            var converters = AutomationDeserializationConfiguration.ModelSettings;
            var filterListItemProviderBuilder = JsonConvert.DeserializeObject<FilterListItemsListProviderConfigModel>(json, converters);
            var mockServiceProvider = this.GenerateServiceProvider();
            var filterListItemprovider = filterListItemProviderBuilder.Build(mockServiceProvider);

            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(
                this.tenant.Id, this.organisation.Id);
            Expression<Func<object, bool>> expression = null;
            this.dataTableContentRepositoryMock
                .Setup(r => r.GetDataTableContentWithFilter(It.IsAny<Guid>(), It.IsAny<int?>(), It.IsAny<Expression<Func<object, bool>>>()))
                .Callback<Guid, int?, Expression<Func<object, bool>>>((p1, p2, p3) =>
                {
                    expression = p3;
                })
                .Returns(this.GetFakeDynamicData().Where((Func<dynamic, bool>)(x => x.Postcode == "4003")));

            // Act
            var entityCollection = (await filterListItemprovider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Asset
            entityCollection.ToList().Should().BeEmpty();
            var sqlQueryBuilder = new SqlQueryBuilder("test");
            var query = sqlQueryBuilder.Build(expression);
            query.ToLower().Should().Be("SELECT * FROM test WHERE (postcode = '4003');\r\n".ToLower());
        }

        [Fact]
        public async Task DynamicCollection_InsideFilterListItems_ShoulProduceProperQuery_ForTime()
        {
            // Arrange
            var valueToTest = "03:26:48.9874123";
            var json = @"
                        {
                            ""list"": {
                                ""dataTableQueryList"": {
                                    ""entity"": {
                                        ""contextEntity"": ""/tenant""
                                    },
                                    ""dataTableAlias"": ""embargo""
                                }
                            },
                            ""condition"": {
                                ""timeIsAfterOrEqualToCondition"": {
                                    ""time"": {
                                        ""objectPathLookupTime"": ""#/embargo/time""
                                    },
                                    ""isAfterOrEqualTo"": """ + valueToTest + @"""
                                }
                            },
                            ""itemAlias"": ""embargo""
                            }
                        ";

            var converters = AutomationDeserializationConfiguration.ModelSettings;
            var filterListItemProviderBuilder = JsonConvert.DeserializeObject<FilterListItemsListProviderConfigModel>(json, converters);
            var mockServiceProvider = this.GenerateServiceProvider();
            var filterListItemprovider = filterListItemProviderBuilder.Build(mockServiceProvider);

            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(
                this.tenant.Id, this.organisation.Id);
            List<(string columnName, string dataType)> columnDataTypeDictionary = new List<(string columnName, string dataType)>
            {
                ("time", "time")
            };
            Expression<Func<object, bool>> expression = null;
            this.dataTableContentRepositoryMock
                .Setup(r => r.GetDataTableContentWithFilter(It.IsAny<Guid>(), It.IsAny<int?>(), It.IsAny<Expression<Func<object, bool>>>()))
                .Callback<Guid, int?, Expression<Func<object, bool>>>((p1, p2, p3) =>
                {
                    expression = p3;
                })
                .Returns(this.GetFakeDynamicData());

            // Act
            var entityCollection = await filterListItemprovider.Resolve(new ProviderContext(automationData));

            // Asset
            var sqlQueryBuilder = new SqlQueryBuilder("test").SetColumnDataTypes(columnDataTypeDictionary);
            var query = sqlQueryBuilder.Build(expression);
            query.ToLower().Should().Be("SELECT * FROM test WHERE (time >= '3:26:48 am');\r\n".ToLower());
        }

        [Fact]
        public async Task DynamicCollection_InsideFilterListItems_ShoulProduceProperQuery_ForDate()
        {
            // Arrange
            var json = @"
                        {
                            ""list"": {
                                ""dataTableQueryList"": {
                                    ""entity"": {
                                        ""contextEntity"": ""/tenant""
                                    },
                                    ""dataTableAlias"": ""embargo""
                                }
                            },
                            ""condition"": {
                                ""dateIsEqualToCondition"": {
                                    ""date"": {
                                        ""objectPathLookupDate"": ""#/embargo/date""
                                    },
                                    ""isEqualTo"": ""2023-11-17"",
                                }
                            },
                            ""itemAlias"": ""embargo""
                            }
                        ";

            var converters = AutomationDeserializationConfiguration.ModelSettings;
            var filterListItemProviderBuilder = JsonConvert.DeserializeObject<FilterListItemsListProviderConfigModel>(json, converters);
            var mockServiceProvider = this.GenerateServiceProvider();
            var filterListItemprovider = filterListItemProviderBuilder.Build(mockServiceProvider);
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(
                this.tenant.Id, this.organisation.Id);
            List<(string columnName, string dataType)> columnDataTypeDictionary = new List<(string columnName, string dataType)>
            {
                ("date", "date")
            };
            Expression<Func<object, bool>> expression = null;
            this.dataTableContentRepositoryMock
                .Setup(r => r.GetDataTableContentWithFilter(It.IsAny<Guid>(), It.IsAny<int?>(), It.IsAny<Expression<Func<object, bool>>>()))
                .Callback<Guid, int?, Expression<Func<object, bool>>>((p1, p2, p3) =>
                {
                    expression = p3;
                })
                .Returns(this.GetFakeDynamicData());

            // Act
            var entityCollection = await filterListItemprovider.Resolve(new ProviderContext(automationData));

            // Asset
            var sqlQueryBuilder = new SqlQueryBuilder("test").SetColumnDataTypes(columnDataTypeDictionary);
            var query = sqlQueryBuilder.Build(expression);
            query.Should().Be("SELECT * FROM test WHERE ((date >= CAST('2023-11-17 00:00:00.0000000' AS DATE)) AND (date <= CAST('2023-11-17 23:59:59.9999999' AS DATE)));\r\n");
        }

        [Fact]
        public async Task DynamicCollection_InsideFilterListItems_ShoulProduceProperQuery_ForDateTime()
        {
            // Arrange
            var json = @"
                        {
                            ""list"": {
                                ""dataTableQueryList"": {
                                    ""entity"": {
                                        ""contextEntity"": ""/tenant""
                                    },
                                    ""dataTableAlias"": ""embargo""
                                }
                            },
                            ""condition"": {
                                ""dateTimeIsInPeriodCondition"": {
                                    ""dateTime"": {
                                        ""objectPathLookupDateTime"": ""#/embargo/datetime""
                                    },
                                    ""isInPeriod"": {
                                        ""lastPeriod"": {
                                            ""periodTypeValueDuration"": {
                                                ""value"": 60,
                                                ""periodType"": ""month""
                                            }
                                        }
                                    }
                                }
                            },
                            ""itemAlias"": ""embargo""
                            }
                        ";

            var converters = AutomationDeserializationConfiguration.ModelSettings;
            var filterListItemProviderBuilder = JsonConvert.DeserializeObject<FilterListItemsListProviderConfigModel>(json, converters);
            var mockServiceProvider = this.GenerateServiceProvider();
            var filterListItemprovider = filterListItemProviderBuilder.Build(mockServiceProvider);
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(
                this.tenant.Id, this.organisation.Id);
            List<(string columnName, string dataType)> columnDataTypeDictionary = new List<(string columnName, string dataType)>
            {
                ("datetime", "datetime")
            };
            Expression<Func<object, bool>> expression = null;
            this.dataTableContentRepositoryMock
                .Setup(r => r.GetDataTableContentWithFilter(It.IsAny<Guid>(), It.IsAny<int?>(), It.IsAny<Expression<Func<object, bool>>>()))
                .Callback<Guid, int?, Expression<Func<object, bool>>>((p1, p2, p3) =>
                {
                    expression = p3;
                })
                .Returns(this.GetFakeDynamicData());

            // Act
            var entityCollection = await filterListItemprovider.Resolve(new ProviderContext(automationData));

            // Asset
            var sqlQueryBuilder = new SqlQueryBuilder("test").SetColumnDataTypes(columnDataTypeDictionary);
            var query = sqlQueryBuilder.Build(expression);

            // the query should look like this, but since the query changes with time, we can't assert the exact query
            // "SELECT * FROM test WHERE ((datetime >= 15501133626116312) AND (datetime < 17078797626116312));\r\n"
            query.Should().Contain("SELECT * FROM test WHERE");
            query.Should().Contain("datetime >=");
            query.Should().Contain("datetime <");
        }

        [Fact]
        public async Task DynamicCollection_InsideFilterListItems_ShoulProduceProperQuery_ForCombinationOfAllTypes()
        {
            // Arrange
            var json = @"
                {
                    ""list"": {
                        ""dataTableQueryList"": {
                            ""entity"": {
                                ""contextEntity"": ""/tenant""
                            },
                            ""dataTableAlias"": ""embargo""
                        }
                    },
                    ""condition"": 
                    {
                        ""andCondition"": [
	                            {
		                            ""textIsEqualToCondition"": {
                                        ""text"": {
                                            ""objectPathLookupText"": ""#/embargo/hasValue""
                                        },
                                        ""isEqualTo"": ""testing dude"",
                                    },
	                            },
	                            {
		                            ""textIsEqualToCondition"": {
                                        ""text"": {
                                            ""objectPathLookupText"": ""#/embargo/testText""
                                        },
                                        ""isEqualTo"": ""Something with (*&(*#@) 0293849283 special characters"",
                                    },
	                            },
	                            {
		                            ""textIsEqualToCondition"": {
                                        ""text"": {
                                            ""objectPathLookupText"": ""#/embargo/testBoolean""
                                        },
                                        ""isEqualTo"": ""true"",
                                    },
	                            },
	                            {
		                            ""textIsEqualToCondition"": {
                                        ""text"": {
                                            ""objectPathLookupText"": ""#/embargo/testNumber""
                                        },
                                        ""isEqualTo"": ""4566"",
                                    },
	                            },
	                            {
		                            ""integerIsEqualToCondition"": {
                                        ""integer"": {
                                            ""objectPathLookupInteger"": ""#/embargo/testNumber""
                                        },
                                        ""isEqualTo"": 4566,
                                    },
	                            },
	                            {
		                            ""textIsEqualToCondition"": {
                                        ""text"": {
                                            ""objectPathLookupText"": ""#/embargo/testPercentage""
                                        },
                                        ""isEqualTo"": ""0.567"",
                                    },
	                            },
	                            {
		                            ""textIsEqualToCondition"": {
                                        ""text"": {
                                            ""objectPathLookupText"": ""#/embargo/testDate""
                                        },
                                        ""isEqualTo"": ""2023-11-17"",
                                    },
	                            },
	                            {
		                            ""dateIsEqualToCondition"": {
                                        ""date"": {
                                            ""objectPathLookupDate"": ""#/embargo/testDate""
                                        },
                                        ""isEqualTo"": ""2023-11-17"",
                                    },
	                            },
	                            {
		                            ""textIsEqualToCondition"": {
                                        ""text"": {
                                            ""objectPathLookupText"": ""#/embargo/testTime""
                                        },
                                        ""isEqualTo"": ""03:26:48.9874123"",
                                    },
	                            },
	                            {
		                            ""timeIsAfterOrEqualToCondition"": {
                                        ""time"": {
                                            ""objectPathLookupTime"": ""#/embargo/testTime""
                                        },
                                        ""isAfterOrEqualTo"": ""03:26:48.9874123"",
                                    },
	                            },
	                            {
		                            ""textIsEqualToCondition"": {
                                        ""text"": {
                                            ""objectPathLookupText"": ""#/embargo/testDateTime""
                                        },
                                        ""isEqualTo"": ""2023-11-17 01:26:48.9874123"",
                                    },
	                            },
	                            {
		                            ""dateTimeIsInPeriodCondition"": {
                                        ""dateTime"": {
                                            ""objectPathLookupDateTime"": ""#/embargo/testDateTime""
                                        },
                                        ""isInPeriod"": {
                                            ""lastPeriod"": {
                                                ""periodTypeValueDuration"": {
                                                    ""value"": 60,
                                                    ""periodType"": ""month""
                                                }
                                            }
                                        }
                                    }
	                            },
                        ]
                    },
                    ""itemAlias"": ""embargo""
                    }
                ";

            var converters = AutomationDeserializationConfiguration.ModelSettings;
            var filterListItemProviderBuilder = JsonConvert.DeserializeObject<FilterListItemsListProviderConfigModel>(json, converters);
            var mockServiceProvider = this.GenerateServiceProvider();
            var filterListItemprovider = filterListItemProviderBuilder.Build(mockServiceProvider);
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(
                this.tenant.Id, this.organisation.Id);
            List<(string columnName, string dataType)> columnDataTypeDictionary = new List<(string columnName, string dataType)>
                {
                    ("type", "varchar"),
                    ("hasValue", "varchar"),
                    ("testText", "varchar"),
                    ("testBoolean", "bit"),
                    ("testNumber", "decimal"),
                    ("testPercentage", "decimal"),
                    ("testDate", "date"),
                    ("testTime", "time"),
                    ("testDateTime", "datetime")
                };
            Expression<Func<object, bool>> expression = null;
            this.dataTableContentRepositoryMock
                .Setup(r => r.GetDataTableContentWithFilter(It.IsAny<Guid>(), It.IsAny<int?>(), It.IsAny<Expression<Func<object, bool>>>()))
                .Callback<Guid, int?, Expression<Func<object, bool>>>((p1, p2, p3) =>
                {
                    expression = p3;
                })
                .Returns(this.GetFakeDynamicData());

            // Act
            var entityCollection = await filterListItemprovider.Resolve(new ProviderContext(automationData));

            // Asset
            var sqlQueryBuilder = new SqlQueryBuilder("test").SetColumnDataTypes(columnDataTypeDictionary);
            var query = sqlQueryBuilder.Build(expression);
            query.ToLower().Should().Be("SELECT * FROM test WHERE ((((((((((((1=1 AND (hasValue = 'testing dude')) AND (testText = 'Something with (*&(*#@) 0293849283 special characters')) AND (testBoolean = 'true')) AND (testNumber = '4566')) AND (testNumber = 4566)) AND (testPercentage = '0.567')) AND (testDate = '2023-11-17')) AND ((testDate >= CAST('2023-11-17 00:00:00.0000000' AS DATE)) AND (testDate <= CAST('2023-11-17 23:59:59.9999999' AS DATE)))) AND (testTime = '03:26:48.9874123')) AND (testTime >= '3:26:48 AM')) AND (testDateTime = '2023-11-17 01:26:48.9874123')) AND ((testDateTime >= -1577664000000000) AND (testDateTime < 0)));\r\n".ToLower());
        }

        private IServiceProvider GenerateServiceProvider()
        {
            this.tenant = TenantFactory.Create(Guid.NewGuid());
            var organisationId = Guid.NewGuid();
            this.organisation = new OrganisationReadModel(
                this.tenant.Id, organisationId, "test", "Test", null, true, false, SystemClock.Instance.Now());
            var fakeOrganisationWithRelatedEnties = new OrganisationReadModelWithRelatedEntities();
            fakeOrganisationWithRelatedEnties.Organisation = this.organisation;
            fakeOrganisationWithRelatedEnties.Tenant = this.tenant;
            fakeOrganisationWithRelatedEnties.TenantDetails = this.tenant.DetailsCollection;

            var organisationSchemaObject = new Domain.SerialisedEntitySchemaObject.Organisation(
                fakeOrganisationWithRelatedEnties, null);

            var fakeOrganisationDataTableDefinition = DataTableDefinition.Create(
                this.tenant.Id,
                EntityType.Organisation,
                organisationId,
                "Embargo",
                "embargo",
                false,
                0,
                this.FakeJSONConfiguration(),
                1,
                3,
                SystemClock.Instance.GetCurrentInstant());

            var fakeTenantDataTableDefinition = DataTableDefinition.Create(
                this.tenant.Id,
                EntityType.Tenant,
                this.tenant.Id,
                "Embargo",
                "embargo",
                false,
                0,
                this.FakeJSONConfiguration(),
                1,
                3,
                SystemClock.Instance.GetCurrentInstant());

            var dataTableDefinitionRepository = new Mock<IDataTableDefinitionRepository>();
            dataTableDefinitionRepository
                .Setup(r => r.GetDataTableDefinitionsByEntityAndAlias(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), EntityType.Organisation, "embargo"))
                .Returns(fakeOrganisationDataTableDefinition);
            dataTableDefinitionRepository
                .Setup(r => r.GetDataTableDefinitionsByEntityAndAlias(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), EntityType.Tenant, "embargo"))
                .Returns(fakeTenantDataTableDefinition);
            var organisationRepository = new Mock<IOrganisationReadModelRepository>();
            organisationRepository
                .Setup(r => r.GetOrganisationWithRelatedEntities(this.tenant.Id, organisationId, new List<string>()))
                .Returns(fakeOrganisationWithRelatedEnties);
            organisationRepository
                .Setup(r => r.GetOrganisationWithRelatedEntities(this.tenant.Id, this.organisation.Alias, It.IsAny<List<string>>()))
                .Returns(fakeOrganisationWithRelatedEnties);

            var serialisedEntityFactory = new Mock<ISerialisedEntityFactory>();
            serialisedEntityFactory
                .Setup(f => f.Create(It.IsAny<IEntityWithRelatedEntities>(), It.IsAny<List<string>>()))
                .Returns(Task.FromResult((IEntity)organisationSchemaObject));

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped(c => dataTableDefinitionRepository.Object);
            serviceCollection.AddScoped(c => this.dataTableContentRepositoryMock.Object);
            serviceCollection.AddScoped(c => organisationRepository.Object);
            serviceCollection.AddScoped(c => serialisedEntityFactory.Object);
            serviceCollection.AddScoped<IClock>(c => TestClock.StaticClock());
            return serviceCollection.BuildServiceProvider();
        }

        private IEnumerable<dynamic> GetFakeDynamicData()
        {
            IReadOnlyDictionary<string, object> data = new Dictionary<string, object>();

            var embargo = new List<dynamic>()
            {
                new { Postcode = "2150" },
                new { Postcode = "3205" },
                new { Postcode = "4002" },
            };

            embargo.Select(d =>
            {
                var dynamicData = new Dictionary<string, object>();

                foreach (var property in d.GetType().GetProperties())
                {
                    dynamicData[property.Name] = property.GetValue(d);
                }

                return dynamicData.ToReadOnlyDictionary();
            });

            return embargo;
        }

        private string FakeJSONConfiguration()
        {
            return @"{""columns"":[{""name"":""Postcode"",""alias"":""postcode"",""dataType"":""postalCode"",""unique"":true}]}";
        }

        private BinaryExpressionFilterProvider EqualFilterProvider(string columnToTest, string valueToTest)
        {
            var firstOperandExpressionProvider = new PropertyExpressionProvider(new StaticProvider<Data<string>>(columnToTest));
            var secondOperandExpressionProvider = new ConstantExpressionProvider(new StaticProvider<Data<string>>(valueToTest));
            var isEqualFilterProvider = new BinaryExpressionFilterProvider(
                (a, b) => Expression.Equal(a, b),
                firstOperandExpressionProvider,
                secondOperandExpressionProvider,
                "textIsEqualToCondition");
            return isEqualFilterProvider;
        }
    }
}

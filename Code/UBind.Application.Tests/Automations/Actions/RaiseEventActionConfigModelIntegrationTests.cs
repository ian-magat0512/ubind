// <copyright file="RaiseEventActionConfigModelIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Actions
{
    using FluentAssertions;
    using Newtonsoft.Json;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Actions;
    using Xunit;

    public class RaiseEventActionConfigModelIntegrationTests
    {
        [Fact]
        public void Constructor_InitializedAllPropertiesCorrectly_WhenTriggeredViaDeserialization()
        {
            // Arrange
            var json = @"
        {
            ""name"": ""Event Action Test2"",
            ""alias"": ""eventActionTest2"",
            ""customEventAlias"": ""MyTestEvent2"",
            ""description"": ""A test of the event action"",
            ""asynchronous"": false,
            ""eventTags"": [ ""somethingqwe"" ],
            ""eventPersistanceDuration"": {
              ""periodTypeValueDuration"": {
                ""value"": 1,
                ""periodType"": ""month""
              }
            },
            ""BeforeRunErrorConditions"": [
              {
                ""condition"": {
                  ""integerIsLessThanCondition"": {
                    ""integer"": {
                      ""countListItemsInteger"": {
                        ""filterListItemsList"": {
                          ""list"": {
                            ""entityQueryList"": {
                              ""entityType"": ""event""
                            }
                          },
                          ""itemAlias"": ""event"",
                          ""condition"": {
                            ""andCondition"": [
                              {
                                ""listCondition"": {
                                  ""list"": {
                                    ""objectPathLookupList"": ""#event.tags""
                                  },
                                  ""itemAlias"": ""tag"",
                                  ""condition"": {
                                    ""textStartsWithCondition"": {
                                      ""text"": {
                                        ""objectPathLookupText"": ""#tag""
                                      },
                                      ""startsWith"": ""b""
                                    }
                                  },
                                  ""matchType"": ""any""
                                }
                              }
                            ]
                          }
                        }
                      }
                    },
                    ""isLessThan"": 1
                  }
                },
                ""error"": {
                  ""code"": ""rate.limit.error"",
                  ""title"": ""Too many requests received."",
                  ""message"": ""Request limit reached"",
                  ""httpStatusCode"": 429
                }
              }
            ]
        }";

            // Act
            var sut = JsonConvert.DeserializeObject<RaiseEventActionConfigModel>(
                json, AutomationDeserializationConfiguration.ModelSettings);

            // Assert
            sut.BeforeRunErrorConditions.Should().NotBeNull();
        }
    }
}

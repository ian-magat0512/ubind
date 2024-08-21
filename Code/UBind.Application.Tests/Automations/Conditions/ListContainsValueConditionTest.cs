// <copyright file="ListContainsValueConditionTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Conditions
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Conditions;
    using UBind.Application.Tests.Automations.Fakes;
    using Xunit;

    public class ListContainsValueConditionTest
    {
        private JsonSerializerSettings settings;
        private IServiceProvider dependencyProvider;

        public ListContainsValueConditionTest()
        {
            this.dependencyProvider = new Mock<IServiceProvider>().AddLoggers().Object;
            this.settings = AutomationDeserializationConfiguration.ModelSettings;
        }

        [Fact]
        public async Task ListContainsValueCondition_ShouldReturnTrue_FromValidList()
        {
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var arrayList = new JArray
            {
                "apple",
                "banana",
                "orange",
            };
            automationData.AddOrUpdateVariableByPath(arrayList, "fruits");
            automationData.AddOrUpdateVariableByPath("apple", "myFruit");

            var json = @"{
                ""listContainsValueCondition"": {
                                ""list"": {
                                    ""objectPathLookupList"": ""/variables/fruits""
                                },
                                ""value"": {
                                    ""objectPathLookupText"": {
                                        ""path"": ""/variables/myFruit"",
                                        ""valueIfNotFound"": """"
                                    }
                        }
                }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ListContainsValueCondition;

            // Assert
            var listContainsValue = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            listContainsValue.DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task ListContainsValueCondition_ShouldReturnTrue_FromValidJsonObject()
        {
            // Arrange
            var contentList = @"{""test"":[""one"",""two"",""three""]}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: contentList);

            var json = @"{
                ""listContainsValueCondition"": {
                        ""list"": {
                                ""objectPathLookupList"": {
                                    ""path"": ""/test"",
                                    ""dataObject"": {
                                            ""jsonObject"": {
                                            ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                                        }
                                    }
                                }
                        },
                        ""value"": ""one""
                   }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ListContainsValueCondition;

            // Assert
            var listContainsValue = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            listContainsValue.DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task ListContainsValueCondition_ShouldReturnTrue_FromValidJsonObjectInteger()
        {
            // Arrange
            var contentList = @"{""test"":[1223,555,5552,552]}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: contentList);

            var json = @"{
                ""listContainsValueCondition"": {
                        ""list"": {
                                ""objectPathLookupList"": {
                                    ""path"": ""/test"",
                                    ""dataObject"": {
                                            ""jsonObject"": {
                                            ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                                        }
                                    }
                                }
                        },
                        ""value"": 552
                   }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ListContainsValueCondition;

            // Assert
            var listContainsValue = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            listContainsValue.DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task ListContainsValueCondition_ShouldReturnTrue_FromValidJsonObjectNumber()
        {
            // Arrange
            var contentList = @"{""test"":[
                                        12.23,
                                        55.5,
                                        55.52,
                                        55.2
                                    ]}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: contentList);

            var json = @"{
                ""listContainsValueCondition"": {
                        ""list"": {
                                ""objectPathLookupList"": {
                                    ""path"": ""/test"",
                                    ""dataObject"": {
                                            ""jsonObject"": {
                                            ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                                        }
                                    }
                                }
                        },
                        ""value"": 55.2
                   }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ListContainsValueCondition;

            // Assert
            var listContainsValue = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            listContainsValue.DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task ListContainsValueCondition_ShouldReturnFalse_FromValidJsonObjectNumber()
        {
            // Arrange
            var contentList = @"{""test"":[
                                        12.23,
                                        55.5,
                                        55.52,
                                        55.2
                                    ]}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: contentList);

            var json = @"{
                ""listContainsValueCondition"": {
                        ""list"": {
                                ""objectPathLookupList"": {
                                    ""path"": ""/test"",
                                    ""dataObject"": {
                                            ""jsonObject"": {
                                            ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                                        }
                                    }
                                }
                        },
                        ""value"": 100.2
                   }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ListContainsValueCondition;

            // Assert
            var listContainsValue = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            listContainsValue.DataValue.Should().BeFalse();
        }

        [Fact]
        public async Task ListContainsValueCondition_ShouldReturnTrue_FromValidJsonObjectBoolean()
        {
            // Arrange
            var contentList = @"{""test"":[
                                        true,
                                        false,
                                        false,
                                        false
                                    ]}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: contentList);

            var json = @"{
                ""listContainsValueCondition"": {
                        ""list"": {
                                ""objectPathLookupList"": {
                                    ""path"": ""/test"",
                                    ""dataObject"": {
                                            ""jsonObject"": {
                                            ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                                        }
                                    }
                                }
                        },
                        ""value"": true
                   }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ListContainsValueCondition;

            // Assert
            var listContainsValue = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            listContainsValue.DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task ListContainsValueCondition_ShouldReturnFalse_FromValidJsonString()
        {
            // Arrange
            var contentList = @"{""test"":[
                                        ""one"",
                                        ""two"",
                                    ]}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: contentList);

            var json = @"{
                ""listContainsValueCondition"": {
                        ""list"": {
                                ""objectPathLookupList"": {
                                    ""path"": ""/test"",
                                    ""dataObject"": {
                                            ""jsonObject"": {
                                            ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                                        }
                                    }
                                }
                        },
                        ""value"": ""three""
                   }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ListContainsValueCondition;

            // Assert
            var listContainsValue = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            listContainsValue.DataValue.Should().BeFalse();
        }

        [Fact]
        public async Task ListContainsValueCondition_ShouldReturnTrue_FromValidJsonListOfObjectValues()
        {
            // Arrange
            var contentList = @"{""test"":[
                                            [
                                            ""apple"",
                                            ""green"",
                                            ]
                                        ]}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: contentList);

            var json = @"{
                ""listContainsValueCondition"": {
                        ""list"": {
                                ""objectPathLookupList"": {
                                    ""path"": ""/test"",
                                    ""dataObject"": {
                                            ""jsonObject"": {
                                            ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                                        }
                                    }
                                }
                        },
                        ""value"": [
                                        ""apple"",
                                        ""green"",
                                    ]
                   }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ListContainsValueCondition;

            // Assert
            var listContainsValue = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            listContainsValue.DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task ListContainsValueCondition_ShouldReturnFalse_FromValidJsonListOfObjectValuesNotMatch()
        {
            // Arrange
            var contentList = @"{""test"":[
                                            [
                                            ""apple"",
                                            ""green"",
                                            ]
                                        ]}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: contentList);

            var json = @"{
                ""listContainsValueCondition"": {
                        ""list"": {
                                ""objectPathLookupList"": {
                                    ""path"": ""/test"",
                                    ""dataObject"": {
                                            ""jsonObject"": {
                                            ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                                        }
                                    }
                                }
                        },
                        ""value"": [
                                        ""apple"",
                                        ""green"",
                                        ""blue"",
                                    ]
                   }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ListContainsValueCondition;

            // Assert
            var listContainsValue = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            listContainsValue.DataValue.Should().BeFalse();
        }

        [Fact]
        public async Task ListContainsValueCondition_ShouldReturnTrue_FromValidJsonListOfObjectsValues()
        {
            // Arrange
            var contentList = @"{""test"":[
                                            [ { ""propertyName"": ""testProperty"", ""value"": ""testValue"" } ]
                                        ]}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: contentList);

            var json = @"{
                ""listContainsValueCondition"": {
                        ""list"": {
                                ""objectPathLookupList"": {
                                    ""path"": ""/test"",
                                    ""dataObject"": {
                                            ""jsonObject"": {
                                            ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                                        }
                                    }
                                }
                        },
                        ""value"": [ { ""propertyName"": ""testProperty"", ""value"": ""testValue"" } ]
                   }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ListContainsValueCondition;

            // Assert
            var listContainsValue = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            listContainsValue.DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task ListContainsValueCondition_ShouldReturnTrue_FromValidInlineList()
        {
            // Arrange
            var contentList = @"{""test"":[""one"",""two"",""three""]}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: contentList);

            var json = @"{
                ""listContainsValueCondition"": {
                        ""list"": [""one"",""two"",""three""],
                        ""value"": ""one""
                   }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as ListContainsValueCondition;

            // Assert
            var listContainsValue = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            listContainsValue.DataValue.Should().BeTrue();
        }
    }
}

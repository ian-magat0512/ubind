// <copyright file="AutomationConfigurationParserTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using UBind.Application.Automation;
    using UBind.Application.Tests.Automations.Fakes;
    using Xunit;

    public class AutomationConfigurationParserTests
    {
        private readonly IServiceProvider dependencyProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationConfigurationParserTests"/> class.
        /// </summary>
        public AutomationConfigurationParserTests()
        {
            this.dependencyProvider = MockAutomationData.GetDefaultServiceProvider();
        }

        [Fact]
        public async Task AutomationConfigurationParser_ShouldParseAutomationConfiguration_FromValidConfigurationJson()
        {
            // Arrange
            var dataContext = await MockAutomationData.CreateDataWithHttpTriggerAndContent();
            var json = @"
{
    ""schemaVersion"": ""1.0.0"",
    ""automations"": [
        {
            ""name"": ""Address Match"",
            ""alias"": ""adressMatch"",
            ""description"": ""configuration for automations pertaining to obtaining matching address references to CoreLogic API"",
            ""triggers"": [
                {
                    ""httpTrigger"": {
                        ""name"": ""GNAF Match trigger"",
                        ""alias"": ""gnafMatchTrigger"",
                        ""description"": ""automation should be triggered by request to addressMatch endpoint"",
                        ""endpoint"": {
                            ""path"": ""addressMatch"",
                            ""httpVerb"": ""POST""
                        },
                        ""httpResponse"": {
                            ""httpStatusCode"" : 200,
                            ""contentType"" : ""application/json"",
                            ""content"" : {
                                ""liquidText"" : {
                                    ""liquidTemplate"": ""{% if suggestions.size == 1 -%}{ 'coreLogicPropertyMatch': 'single','coreLogicPropertyId' : {{suggestions[0].propertyId}} }{% elseif suggestions == empty -%}{ 'coreLogicPropertyMatch': 'none' }{% elseif suggestions.size > 1 -%}{'corelogicPropertyMatch': 'multiple','corelogicPropertyId': ''}{% endif %}"",
                                    ""dataObject"": {
                                        ""jsonObject"": {
                                            ""objectPathLookupText"": ""actions[1].httpResponse.content""
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            ],
            ""actions"": [
                {
                    ""httpRequestAction"": {
                        ""name"": ""Authentication"",
                        ""alias"": ""authentication"",
                        ""description"": ""obtains the necessary authorization to perform next action"",
                        ""asynchronous"": false,
                        ""httpRequest"": {
                            ""httpVerb"": ""GET"",
                            ""url"": ""https://access-uat-api.CoreLogic.asia/access/oauth/token?grant_type=client_credentials&client_id=PARAMETER&client_secret=PARAMETER""
                        }
                    }
                },
                {
                    ""httpRequestAction"": {
                        ""name"": ""Address Match"",
                        ""alias"": ""addressMatch"",
                        ""description"": ""search for applicable  property suggestions and associated IDs for a street address, street name, locality (suburb) and/or postcode. The respones includes a sorted suggestion list that includes location type, unit and body corporate flags and as well as relevant location IDs."",
                        ""asynchronous"": false,
                        ""httpRequest"": {
                            ""httpVerb"": ""GET"",
                            ""url"": {
                                ""liquidText"": {
                                    ""liquidTemplate"" : ""https://property-uat-api.corelogic.asia/bsg-au/v1/suggest.json?suggestionTypes=address&limit=20&includeUnits=true&q={{address}}"",
                                    ""dataObject"":{
                                        ""jsonObject"" : {
                                            ""objectPathLookupText"": ""trigger.httpRequest.content""
                                        }
                                    }
                                }
                            },
                            ""header"": [
                                {
                                    ""Authorization"": {
                                        ""liquidText"" : {
                                            ""liquidTemplate"": ""Bearer {{access_token}}"",
                                            ""dataObject"": {
                                                ""jsonObject"": {
                                                    ""objectPathLookupText"": ""actions[0].httpResponse.content""
                                                }
                                            }
                                        }
                                    }
                                }
                            ]
                        }
                    }
                }
            ]
        },
        {
            ""name"": ""Address Search"",
            ""alias"": ""addressSearch"",
            ""description"": ""configuration for automation for a search of CORELOGIC API database based on passed address"",
            ""triggers"": [
                {
                    ""httpTrigger"": {
                        ""name"": ""GNAF Search Trigger"",
                        ""alias"": ""gnafSearchTrigger"",
                        ""description"": ""automation should be triggered by request to addressSearch endpoint"",
                        ""endpoint"": {
                            ""path"": ""addressSearch"",
                            ""httpVerb"": ""POST""
                        },
                        ""httpResponse"": {
                            ""httpStatusCode"" : 200,
                            ""contentType"" : ""application/json"",
                            ""content"" : {
                                ""liquidText"" : {
                                    ""liquidTemplate"": ""{'options': [{% for item in suggestions %}{'label': '{{item.suggestion}}', 'value': '{{item.propertyId}}'},{% endfor %}]}"",
                                    ""dataObject"": {
                                        ""jsonObject"": {
                                            ""objectPathLookupText"": ""actions[1].httpResponse.content""
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            ],
            ""actions"": [
                {
                    ""httpRequestAction"": {
                        ""name"": ""Authentication"",
                        ""alias"": ""authentication"",
                        ""description"": ""obtains the necessary authorization to perform next action"",
                        ""asynchronous"": false,
                        ""httpRequest"": {
                            ""httpVerb"": ""GET"",
                            ""url"": ""https://access-uat-api.CoreLogic.asia/access/oauth/token?grant_type=client_credentials&client_id=PARAMETER&client_secret=PARAMETER""
                        }
                    }
                },
                {
                    ""httpRequestAction"": {
                        ""name"": ""Address Search"",
                        ""alias"": ""addressSearch"",
                        ""description"": ""search for applicable property and associated IDs for a street address, street name, locality (suburb) and/or postcode. The respones includes a sorted suggestion list that includes location type, unit and body corporate flags and as well as relevant location IDs."",
                        ""asynchronous"": false,
                        ""httpRequest"": {
                            ""httpVerb"": ""GET"",
                            ""url"": {
                                ""liquidText"": {
                                    ""liquidTemplate"" : ""https://property-uat-api.corelogic.asia/bsg-au/v1/suggest.json?suggestionTypes=address&limit=20&includeUnits=true&q={{address}}"",
                                    ""dataObject"":{
                                        ""jsonObject"": {
                                            ""objectPathLookupText"": ""trigger.httpRequest.content""
                                        }
                                    }
                                }
                            },
                            ""header"": [
                                {
                                    ""Authorization"": {
                                        ""liquidText"" : {
                                            ""liquidTemplate"": ""Bearer {{access_token}}"",
                                            ""dataObject"": {
                                                ""jsonObject"": {
                                                    ""objectPathLookupText"": ""actions[0].httpResponse.content""
                                                }
                                            }
                                        }
                                    }
                                }
                            ]
                        }
                    }
                }
            ]
        }
    ]
}";

            // Act
            var model = AutomationConfigurationParser.Parse(json);
            var automationConfiguration = model.Build(this.dependencyProvider);
            var (addressMatchAutomation, trigger) = await automationConfiguration.GetClosestMatchingHttpTrigger(dataContext);

            // Assert
            addressMatchAutomation.Name.Should().Be("Address Match");
        }
    }
}

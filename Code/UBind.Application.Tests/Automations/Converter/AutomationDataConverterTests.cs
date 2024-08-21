// <copyright file="AutomationDataConverterTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations
{
    using System.Text;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Newtonsoft.Json;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Data;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using UBind.Domain.SerialisedEntitySchemaObject;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    [SystemEventTypeExtensionInitialize]
    public class AutomationDataConverterTests
    {
        [Fact]
        public void ReadJson_SuccessfulConversion_WhenSerializingAutomationDataContext()
        {
            // Arrange
            var automationData = MockAutomationData.CreateWithEventTrigger(TenantFactory.DefaultId);

            // Action
            string automationDataJson = JsonConvert.SerializeObject(automationData);
            AutomationData automationDataDeserialized = JsonConvert.DeserializeObject<AutomationData>(automationDataJson, AutomationDeserializationConfiguration.DataSettings);

            // Assert
            automationDataDeserialized.Should().NotBeNull();
            automationDataDeserialized.System.EnvironmentName.Should().Be(automationData.System.EnvironmentName);
            automationDataDeserialized.ContextManager.Tenant.Id.Should().Be(TenantFactory.DefaultId.ToString());
            automationDataDeserialized.ContextManager.Product.Id.Should().Be(ProductFactory.DefaultId.ToString());
        }

        [Fact]
        public void ReadJson_SerializedInitialEntityOnly_WhenSerializing()
        {
            // Arrange
            var automationData = MockAutomationData.CreateWithEventTrigger(TenantFactory.DefaultId);

            // Action
            string automationDataJson = JsonConvert.SerializeObject(automationData);
            AutomationData automationDataDeserialized = JsonConvert.DeserializeObject<AutomationData>(automationDataJson, AutomationDeserializationConfiguration.DataSettings);

            // Assert
            automationDataDeserialized.Should().NotBeNull();
            automationDataDeserialized.System.EnvironmentName.Should().Be(automationData.System.EnvironmentName);
            IEntity tenant = (IEntity)automationDataDeserialized.Context[EntityType.Tenant.ToCamelCaseString()];
            tenant.Id.Should().Be(TenantFactory.DefaultId.ToString());
            tenant.GetType().Name.Should().Be("Tenant");
            IEntity product = (IEntity)automationDataDeserialized.Context[EntityType.Product.ToCamelCaseString()];
            product.Id.Should().Be(ProductFactory.DefaultId.ToString());
            product.GetType().Name.Should().Be("Product");
        }

        [Fact]
        public async Task Serializes_And_Deserializes_Binary_Data_Correctly()
        {
            // Arrange
            var testString = "This is some test binary data";
            byte[] data = Encoding.UTF8.GetBytes(testString);
            AutomationData automationData = await MockAutomationData.CreateWithHttpActionBinaryData(data, "testHttpRequestAction");

            // Action
            string automationDataJson = JsonConvert.SerializeObject(automationData);
            AutomationData automationDataDeserialized
                = JsonConvert.DeserializeObject<AutomationData>(automationDataJson, AutomationDeserializationConfiguration.DataSettings);
            byte[] result
                = await automationDataDeserialized.GetValue<byte[]>("/actions/testHttpRequestAction/httpResponse/content", null);

            // Assert
            Encoding.UTF8.GetString(result).Should().Be(testString);
        }

        [Fact]
        public async Task Serializes_And_Deserializes_Multipart_Data_Correctly()
        {
            // Arrange
            string testStringBinary = "This is some test binary data";
            string testString = "This is a test string";

            byte[] data = Encoding.UTF8.GetBytes(testStringBinary);
            AutomationData automationData = await MockAutomationData.CreateWithHttpActionMultipartData(
                testString,
                data,
                "testHttpRequestAction");

            // Action
            string automationDataJson = JsonConvert.SerializeObject(automationData);
            AutomationData automationDataDeserialized
                = JsonConvert.DeserializeObject<AutomationData>(automationDataJson, AutomationDeserializationConfiguration.DataSettings);

            // Assert
            string stringContent
                = await automationDataDeserialized.GetValue<string>("/actions/testHttpRequestAction/httpResponse/content/0/content", null);
            stringContent.Should().Be(testString);
            byte[] binaryContent
                = await automationDataDeserialized.GetValue<byte[]>("/actions/testHttpRequestAction/httpResponse/content/1/content", null);
            Encoding.UTF8.GetString(binaryContent).Should().Be(testStringBinary);
        }

        [Fact]
        public async Task Serializes_And_Deserializes_HttpRequestActionData_Correctly()
        {
            // Arrange
            string testStringBinary = "This is some test binary data";

            byte[] data = Encoding.UTF8.GetBytes(testStringBinary);
            AutomationData automationData = MockAutomationData.CreateWithHttpRequestActionDataBinaryContent(
                data,
                "testHttpRequestAction");

            // Action
            string automationDataJson = JsonConvert.SerializeObject(automationData);
            AutomationData automationDataDeserialized
                = JsonConvert.DeserializeObject<AutomationData>(automationDataJson, AutomationDeserializationConfiguration.DataSettings);

            // Assert
            byte[] binaryContent
                = await automationDataDeserialized.GetValue<byte[]>("/actions/testHttpRequestAction/httpRequest/content", null);
            Encoding.UTF8.GetString(binaryContent).Should().Be(testStringBinary);
        }
    }
}

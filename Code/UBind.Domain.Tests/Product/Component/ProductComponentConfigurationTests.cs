// <copyright file="ProductComponentConfigurationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Product.Component
{
    using FluentAssertions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using UBind.Domain.Product.Component;
    using UBind.Domain.Product.Component.Form;
    using Xunit;

    public class ProductComponentConfigurationTests
    {
        [Fact]
        public void GetFormDataSchema_DeserializesFieldsIntoTheCorrectSubType_UsingACustomSerializationBinder()
        {
            // Arrange
            QuestionSet questionSet = new QuestionSet
            {
                Name = "Test Question Set",
            };
            questionSet.Fields.Add(new SingleLineTextField
            {
                Name = "Test Single Line Text Field",
                QuestionSet = questionSet,
                DataType = DataType.Text,
                Sensitive = true,
            });
            FieldSerializationBinder fieldSerializationBinder = new FieldSerializationBinder();
            string questionSetJson = JsonConvert.SerializeObject(
                questionSet,
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    SerializationBinder = fieldSerializationBinder,
                    TypeNameHandling = TypeNameHandling.Auto,
                    NullValueHandling = NullValueHandling.Ignore,
                });

            // Act
            QuestionSet result = JsonConvert.DeserializeObject<QuestionSet>(
                questionSetJson,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    SerializationBinder = fieldSerializationBinder,
                });

            // Assert
            result.Fields[0].Should().BeOfType(typeof(SingleLineTextField));
        }
    }
}

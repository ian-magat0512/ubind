// <copyright file="StringEnumHumanizerJsonConverterTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.JsonConverters
{
    using System.ComponentModel;
    using FluentAssertions;
    using Newtonsoft.Json;
    using UBind.Domain.JsonConverters;
    using Xunit;

    public class StringEnumHumanizerJsonConverterTests
    {
        public enum TestEnum
        {
            /// <summary>
            /// The first value
            /// </summary>
            [Description("The first value")]
            FirstValue,

            /// <summary>
            /// The second value
            /// </summary>
            [Description("The 2nd value")]
            SecondValue,

            /// <summary>
            /// The third value
            /// </summary>
            ThirdValue,
        }

        [Fact]
        public void ReadJson_ConvertsStringToEnum_WhenStringMatchesDescriptionAttributeOfEnum()
        {
            // Arrange
            string json = "{\"testEnum\":\"The 2nd value\"}";

            // Act
            TestClass testObject = JsonConvert.DeserializeObject<TestClass>(json);

            // Assert
            testObject.TestEnum.Should().Be(TestEnum.SecondValue);
        }

        [Fact]
        public void WriteJson_ConvertsEnumToString_UsingDescriptionAttributeOfEnum()
        {
            // Arrange
            TestClass testObject = new TestClass
            {
                TestEnum = TestEnum.SecondValue,
            };

            // Act
            string json = JsonConvert.SerializeObject(testObject);

            // Assert
            json.Should().Be("{\"testEnum\":\"The 2nd value\"}");
        }

        public class TestClass
        {
            [JsonProperty("testEnum")]
            [JsonConverter(typeof(StringEnumHumanizerJsonConverter))]
            public TestEnum TestEnum { get; set; }
        }
    }
}

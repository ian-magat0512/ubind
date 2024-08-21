// <copyright file="ErrorExtensionTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

// CS1591 Missing XML comment for publicly visible type or member
// Suppress CS1591. The unit test method must be named correctly instead of adding a comment in it.
#pragma warning disable CS1591

namespace UBind.Web.Tests.Extensions
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Domain;
    using UBind.Web.Extensions;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="ErrorExtensionTests" />.
    /// </summary>
    public class ErrorExtensionTests
    {
        [Fact]
        public void ToJsonResult_SerializationSuccessful_AllPropertiesAreSerializable()
        {
            // Arrange
            var errorCode = "error.test";
            var title = "sometitle";
            var message = "somemessage";
            var statusCode = System.Net.HttpStatusCode.Unauthorized;
            var additionalDetails = new List<string>()
            {
                "something",
                "somethingelse",
            };
            var data = new JObject
            {
                { "property", "value" },
            };

            var error = new Error(
                errorCode,
                title,
                message,
                statusCode,
                additionalDetails,
                data);

            var jsonResult = error.ToProblemJsonResult();
            dynamic value = jsonResult.Value;
            var json = JsonConvert.SerializeObject(value);

            // Act
            var objFromJson = JsonConvert.DeserializeObject<dynamic>(json);

            // Assert
            Assert.Equal(objFromJson.code.ToString(), errorCode);
            Assert.Equal(objFromJson.Title.ToString(), title);
            Assert.Equal(objFromJson.Detail.ToString(), message);
            Assert.Equal(objFromJson.Status.ToString(), ((int)statusCode).ToString());
            List<string> list = JsonConvert.DeserializeObject<List<string>>(objFromJson.additionalDetails.ToString());
            for (int i = 0; i < additionalDetails.Count; i++)
            {
                Assert.Equal(list[i], additionalDetails[i]);
            }

            Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(objFromJson.data.ToString());
            foreach (var row in data)
            {
                Assert.True(dictionary.ContainsKey(row.Key));
                Assert.Equal(row.Value, dictionary[row.Key]);
            }
        }
    }
}

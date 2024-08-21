// <copyright file="PaymentConfigurationParserTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Payment
{
    using UBind.Application.Payment;
    using UBind.Application.Payment.Deft;
    using UBind.Domain;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="PaymentConfigurationParserTests" />.
    /// </summary>
    public class PaymentConfigurationParserTests
    {
        /// <summary>
        /// The Parse_ParsesEwayConfigurationModelCorrectly_WhenJsonIsValid.
        /// </summary>
        [Fact]
        public void Parse_ParsesEwayConfigurationModelCorrectly_WhenJsonIsValid()
        {
            // Arrange
            var json = @"{
    ""type"": ""eway"",
    ""default"": {
        ""endpoint"": ""sandbox"",
        ""apiKey"": ""44DD7CA2qU8XjucDU6YbBXl702SLLyk807ziELJdmmw2Y4jtgo5UJdFz+6imEX5McCzygu"",
        ""password"": ""mIln8mfI"",
        ""clientSideEncryptionKey"": ""ryj1MAyYOwZosMi+SZXoV4hOW95BuDF8kA5zEW6JBD5S9l0cogdt0Hs5K9h0wKlxh6O8dAHgsiTCxvmz0pbKXGVyqSjq1PpjOtMveC4N5C8IGosC1ueLPWUdivfu9T3XjZ9siFYh30Y8oVL3f9eb4KnGvzYJ5UaRk1mUAZL/Ln6MX6op6pHGw2HVbi4GFUq3OfLMzM+hsFMfhGIDNeDitOTNFn1eH2rTNZJkAinCiQ86vT6aX7tHDZIYslukwOKWTaN/kI6fgmdab4LaxE5tErRcM1uhwVnI4UlQqp4uHC/vYS31wQ+1AYftlMG3h1QIFtgww1Ydo8NA4ibLXCluvQ==""
    }
}";

            // Act
            var model = PaymentConfigurationParser.Parse(json);

            // Assert
            Assert.NotNull(model);
            Assert.True(model is EwayConfigurationModel);
            var config = model.Generate(Domain.DeploymentEnvironment.Development);
        }

        /// <summary>
        /// The Parse_ParsesDeftConfigurationModelCorrectly_WhenJsonIsValid.
        /// </summary>
        [Fact]
        public void Parse_ParsesDeftConfigurationModelCorrectly_WhenJsonIsValid()
        {
            // Arrange
            var json = @"{
    ""type"": ""deft"",
    ""default"": {
        ""AuthorizationUrl"": ""Foo"",
        ""ClientId"": ""Bar"",
        ""ClientSecret"": ""Wat"",
        ""PaymentUrl"": ""Wut"",
        ""SurchargeUrl"": ""Wot"",
        ""SurchargeUrl"": ""Wit"",
        ""BillerCode"": ""Waa""
    }
}";

            // Act
            var model = PaymentConfigurationParser.Parse(json);

            // Assert
            Assert.NotNull(model);
            Assert.True(model is DeftConfigurationModel);
        }

        /// <summary>
        /// The Parse_ParsesStripeConfigurationModelCorrectly_WhenJsonIsValid.
        /// </summary>
        [Fact]
        public void Parse_ParsesStripeConfigurationModelCorrectly_WhenJsonIsValid()
        {
            // Arrange
            var json = @"{
    ""type"": ""stripe"",
    ""default"": {
        ""privateApiKey"": ""foobar""
    }
}";

            // Act
            var model = PaymentConfigurationParser.Parse(json);

            // Assert
            Assert.NotNull(model);
            Assert.True(model is StripeConfigurationModel);
        }

        /// <summary>
        /// The Parse_ParsesDeftConfigurationModelCorrectly_WhenProductionOverridesExist.
        /// </summary>
        [Fact]
        public void Parse_ParsesDeftConfigurationModelCorrectly_WhenProductionOverridesExist()
        {
            // Arrange
            var json = @"{
  ""type"": ""deft"",
  ""default"": {
    ""AuthorizationUrl"": ""https://example.com/default/auth"",
    ""ClientId"": ""Alpha"",
    ""ClientSecret"": ""Beta"",
    ""PaymentUrl"": ""https://example.com/default/payment"",
    ""SurchargeUrl"": ""https://example.com/default/surcharge"",
    ""DrnUrl"": ""https://example.com/default/drn"",
    ""BillerCode"": ""Epsilon""
  },
  ""production"": {
    ""AuthorizationUrl"": ""https://example.com/prod/auth"",
    ""ClientId"": ""Gamma"",
    ""ClientSecret"": ""Delta"",
    ""PaymentUrl"": ""https://example.com/prod/payment"",
    ""SurchargeUrl"": ""https://example.com/prod/surcharge"",
    ""DrnUrl"": ""https://example.com/prod/drn"",
    ""BillerCode"": ""Zeta""
  }
        }";

            // Act
            var model = PaymentConfigurationParser.Parse(json);
            var config = model.Generate(DeploymentEnvironment.Production) as DeftConfiguration;

            // Assert
            Assert.Equal("https://example.com/prod/auth", config.AuthorizationUrl);
            Assert.Equal("Gamma", config.ClientId);
            Assert.Equal("Delta", config.ClientSecret);
            Assert.Equal("https://example.com/prod/payment", config.PaymentUrl);
            Assert.Equal("https://example.com/prod/surcharge", config.SurchargeUrl);
            Assert.Equal("https://example.com/prod/drn", config.DrnUrl);
            Assert.Equal("Zeta", config.BillerCode);
        }
    }
}

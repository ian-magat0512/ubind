// <copyright file="IncomingFormDataPatchCommandModelTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

// CS1591 Missing XML comment for publicly visible type or member
// Suppress CS1591. The unit test method must be named correctly instead of adding a comment in it.
#pragma warning disable CS1591
namespace UBind.Web.Tests.ResourceModels
{
    using System;
    using FluentAssertions;
    using Newtonsoft.Json;
    using UBind.Domain.Aggregates.Quote.Commands;
    using UBind.Domain.Json;
    using UBind.Web.Exceptions;
    using UBind.Web.ResourceModels;
    using Xunit;

    public class IncomingFormDataPatchCommandModelTests
    {
        [Fact]
        public void ToCommand_ReturnsGivenValuePatchCommand_WhenTypeIsGivenValue()
        {
            // Arrange
            var json = @"{
    ""Type"" : ""GivenValue"",
    ""targetFormDataPath"": ""foo"",
    ""targetCalculationResultPath"": ""bar"",
    ""NewValue"": ""\""baz\""""
}";
            var sut = JsonConvert.DeserializeObject<IncomingPolicyDataPatchCommandModel>(json);

            // Act
            var command = sut.ToCommand();

            // Assert
            Assert.True(command is GivenValuePolicyDataPatchCommand);
        }

        [Fact]
        public void ToCommand_Throws_WhenTypeIsGivenValueButNewValueIsNull()
        {
            // Arrange
            var json = @"{
    ""Type"" : ""GivenValue"",
    ""targetFormDataPath"": ""foo"",
    ""targetCalculationResultPath"": ""bar""
}";
            var sut = JsonConvert.DeserializeObject<IncomingPolicyDataPatchCommandModel>(json);

            // Act
            Action act = () => sut.ToCommand();

            // Assert
            act.Should().Throw<BadRequestException>();
        }

        [Fact]
        public void ToCommand_Throws_WhenTypeIsGivenValueButSourceEntityIsSupplied()
        {
            // Arrange
            var json = @"{
    ""Type"" : ""GivenValue"",
    ""targetFormDataPath"": ""foo"",
    ""targetCalculationResultPath"": ""bar"",
    ""NewValue"": ""\""baz\"""",
    ""SourceEntity"": ""FirstPolicyTransactionCalculationResult""
}";
            var sut = JsonConvert.DeserializeObject<IncomingPolicyDataPatchCommandModel>(json);

            // Act
            Action act = () => sut.ToCommand();

            // Assert
            act.Should().Throw<BadRequestException>();
        }

        [Fact]
        public void ToCommand_Throws_WhenTypeIsGivenValueButSourcePathIsSupplied()
        {
            // Arrange
            var json = $@"{{
    ""Type"" : ""GivenValue"",
    ""targetFormDataPath"": ""foo"",
    ""targetCalculationResultPath"": ""bar"",
    ""NewValue"": ""\""baz\"""",
    ""SourcePath"": ""qux""
}}";
            var sut = JsonConvert.DeserializeObject<IncomingPolicyDataPatchCommandModel>(json);

            // Act
            Action act = () => sut.ToCommand();

            // Assert
            act.Should().Throw<BadRequestException>();
        }

        [Fact]
        public void ToCommand_ReturnsCopyFieldatchCommand_WhenTypeIsCopyField()
        {
            // Arrange
            var json = $@"{{
    ""Type"" : ""CopyField"",
    ""targetFormDataPath"": ""foo"",
    ""targetCalculationResultPath"": ""bar"",
    ""SourceEntity"": ""FirstPolicyTransactionCalculationResult"",
    ""SourcePath"": ""baz""
}}";
            var sut = JsonConvert.DeserializeObject<IncomingPolicyDataPatchCommandModel>(json);

            // Act
            var command = sut.ToCommand();

            // Assert
            Assert.True(command is CopyFieldPolicyDataPatchCommand);
        }

        [Fact]
        public void ToCommand_Throws_WhenTypeIsCopyFieldButSourceEntityIsMissing()
        {
            // Arrange
            var json = $@"{{
    ""Type"" : ""CopyField"",
    ""targetFormDataPath"": ""foo"",
    ""targetCalculationResultPath"": ""bar"",
    ""SourcePath"": ""baz""
}}";
            var sut = JsonConvert.DeserializeObject<IncomingPolicyDataPatchCommandModel>(json);

            // Act
            Action act = () => sut.ToCommand();

            // Assert
            act.Should().Throw<BadRequestException>();
        }

        [Fact]
        public void ToCommand_Throws_WhenTypeIsCopyFieldButSourceEntityIdIsMissing()
        {
            // Arrange
            var json = $@"{{
    ""Type"" : ""CopyField"",
    ""targetFormDataPath"": ""foo"",
    ""targetCalculationResultPath"": ""bar"",
    ""SourceEntity"": ""FirstPolicyTransactionCalculationResult""
}}";
            var sut = JsonConvert.DeserializeObject<IncomingPolicyDataPatchCommandModel>(json);

            // Act
            Action act = () => sut.ToCommand();

            // Assert
            act.Should().Throw<BadRequestException>();
        }

        [Fact]
        public void ToCommand_Throws_WhenTypeIsCopyFieldButNewValueIsSupplied()
        {
            // Arrange
            var json = $@"{{
    ""Type"" : ""CopyField"",
    ""targetFormDataPath"": ""foo"",
    ""targetCalculationResultPath"": ""bar"",
    ""NewValue"": ""\""baz\""""
}}";
            var sut = JsonConvert.DeserializeObject<IncomingPolicyDataPatchCommandModel>(json);

            // Act
            Action act = () => sut.ToCommand();

            // Assert
            act.Should().Throw<BadRequestException>();
        }

        [Fact]
        public void ToCommand_ReturnsCommandWithGlobalScope_WhenNoneSpecified()
        {
            // Arrange
            var json = @"{
    ""Type"" : ""GivenValue"",
    ""targetFormDataPath"": ""foo"",
    ""targetCalculationResultPath"": ""bar"",
    ""NewValue"": ""\""baz\""""
}";
            var sut = JsonConvert.DeserializeObject<IncomingPolicyDataPatchCommandModel>(json);

            // Act
            var command = sut.ToCommand();

            // Assert
            Assert.True(command.Scope.Type == PatchScopeType.Global);
        }

        [Fact]
        public void ToCommand_ReturnsCommandWithGlobalScope_WhenGlobalScopeIsSpecified()
        {
            // Arrange
            var json = @"{
    ""Type"" : ""GivenValue"",
    ""targetFormDataPath"": ""foo"",
    ""targetCalculationResultPath"": ""bar"",
    ""NewValue"": ""\""baz\"""",
    ""ScopeType"": ""Global""
}";
            var sut = JsonConvert.DeserializeObject<IncomingPolicyDataPatchCommandModel>(json);

            // Act
            var command = sut.ToCommand();

            // Assert
            Assert.True(command.Scope.Type == PatchScopeType.Global);
        }

        [Fact]
        public void ToCommand_Throws_WhenScopeTypeIsGlobalButScopeEntityIdIsSpecified()
        {
            // Arrange
            var scopeEntityId = Guid.NewGuid();
            var json = $@"{{
    ""Type"" : ""GivenValue"",
    ""targetFormDataPath"": ""foo"",
    ""targetCalculationResultPath"": ""bar"",
    ""NewValue"": ""\""baz\"""",
    ""ScopeType"": ""Global"",
    ""ScopeEntityId"": ""{scopeEntityId.ToString()}""
}}";
            var sut = JsonConvert.DeserializeObject<IncomingPolicyDataPatchCommandModel>(json);

            // Act
            Action act = () => sut.ToCommand();

            // Assert
            act.Should().Throw<BadRequestException>();
        }

        [Fact]
        public void ToCommand_Throws_WhenScopeTypeIsGlobalButScopeVersionNumberIsSpecified()
        {
            // Arrange
            var json = $@"{{
    ""Type"" : ""GivenValue"",
    ""targetFormDataPath"": ""foo"",
    ""targetCalculationResultPath"": ""bar"",
    ""NewValue"": ""\""baz\"""",
    ""ScopeType"": ""Global"",
    ""ScopeVersionNumber"": 1
}}";
            var sut = JsonConvert.DeserializeObject<IncomingPolicyDataPatchCommandModel>(json);

            // Act
            Action act = () => sut.ToCommand();

            // Assert
            act.Should().Throw<BadRequestException>();
        }

        [Fact]
        public void ToCommand_ReturnsCommandWithQuoteFullScope_WhenScopeTypeIsQuoteFull()
        {
            // Arrange
            var scopeEntityId = Guid.NewGuid();
            var json = $@"{{
    ""Type"" : ""GivenValue"",
    ""targetFormDataPath"": ""foo"",
    ""targetCalculationResultPath"": ""bar"",
    ""NewValue"": ""\""baz\"""",
    ""ScopeType"": ""QuoteFull"",
    ""ScopeEntityId"": ""{scopeEntityId.ToString()}""
}}";
            var sut = JsonConvert.DeserializeObject<IncomingPolicyDataPatchCommandModel>(json);

            // Act
            var command = sut.ToCommand();

            // Assert
            Assert.True(command.Scope.Type == PatchScopeType.QuoteFull);
            Assert.Equal(scopeEntityId, command.Scope.EntityId);
        }

        [Fact]
        public void ToCommand_Throws_WhenScopeTypeIsQuoteFullBuScopeEntityIdIsMissing()
        {
            // Arrange
            var json = $@"{{
    ""Type"" : ""GivenValue"",
    ""targetFormDataPath"": ""foo"",
    ""targetCalculationResultPath"": ""bar"",
    ""NewValue"": ""\""baz\"""",
    ""ScopeType"": ""QuoteFull""
}}";
            var sut = JsonConvert.DeserializeObject<IncomingPolicyDataPatchCommandModel>(json);

            // Act
            Action act = () => sut.ToCommand();

            // Assert
            act.Should().Throw<BadRequestException>();
        }

        [Fact]
        public void ToCommand_ReturnsCommandWithQuoteLatestScope_WhenScopeTypeIsQuoteLatest()
        {
            // Arrange
            var scopeEntityId = Guid.NewGuid();
            var json = $@"{{
    ""Type"" : ""GivenValue"",
    ""targetFormDataPath"": ""foo"",
    ""targetCalculationResultPath"": ""bar"",
    ""NewValue"": ""\""baz\"""",
    ""ScopeType"": ""QuoteLatest"",
    ""ScopeEntityId"": ""{scopeEntityId.ToString()}""
}}";
            var sut = JsonConvert.DeserializeObject<IncomingPolicyDataPatchCommandModel>(json);

            // Act
            var command = sut.ToCommand();

            // Assert
            Assert.True(command.Scope.Type == PatchScopeType.QuoteLatest);
            Assert.Equal(scopeEntityId, command.Scope.EntityId);
        }

        [Fact]
        public void ToCommand_Throws_WhenScopeTypeIsQuoteLatestBuScopeEntityIdIsMissing()
        {
            // Arrange
            var json = $@"{{
    ""Type"" : ""GivenValue"",
    ""targetFormDataPath"": ""foo"",
    ""targetCalculationResultPath"": ""bar"",
    ""NewValue"": ""\""baz\"""",
    ""ScopeType"": ""QuoteLatest""
}}";
            var sut = JsonConvert.DeserializeObject<IncomingPolicyDataPatchCommandModel>(json);

            // Act
            Action act = () => sut.ToCommand();

            // Assert
            act.Should().Throw<BadRequestException>();
        }

        [Fact]
        public void ToCommand_ReturnsCommandWithPolicyTransactionScope_WhenScopeTypeIsPolicyTransaction()
        {
            // Arrange
            var scopeEntityId = Guid.NewGuid();
            var json = $@"{{
    ""Type"" : ""GivenValue"",
    ""targetFormDataPath"": ""foo"",
    ""targetCalculationResultPath"": ""bar"",
    ""NewValue"": ""\""baz\"""",
    ""ScopeType"": ""PolicyTransaction"",
    ""ScopeEntityId"": ""{scopeEntityId.ToString()}""
}}";
            var sut = JsonConvert.DeserializeObject<IncomingPolicyDataPatchCommandModel>(json);

            // Act
            var command = sut.ToCommand();

            // Assert
            Assert.True(command.Scope.Type == PatchScopeType.PolicyTransaction);
            Assert.Equal(scopeEntityId, command.Scope.EntityId);
        }

        [Fact]
        public void ToCommand_Throws_WhenScopeTypeIsPolicyyTransactionButScopeEntityIdIsMissing()
        {
            // Arrange
            var json = $@"{{
    ""Type"" : ""GivenValue"",
    ""targetFormDataPath"": ""foo"",
    ""targetCalculationResultPath"": ""bar"",
    ""NewValue"": ""\""baz\"""",
    ""ScopeType"": ""PolicyTransaction""
}}";
            var sut = JsonConvert.DeserializeObject<IncomingPolicyDataPatchCommandModel>(json);

            // Act
            Action act = () => sut.ToCommand();

            // Assert
            act.Should().Throw<BadRequestException>();
        }

        [Fact]
        public void ToCommand_ReturnsCommandWithQuoteVersionScope_WhenScopeTypeIsQuoteVersion()
        {
            // Arrange
            var scopeEntityId = Guid.NewGuid();
            var scopeVersionNumber = 1;
            var json = $@"{{
    ""Type"" : ""GivenValue"",
    ""targetFormDataPath"": ""foo"",
    ""targetCalculationResultPath"": ""bar"",
    ""NewValue"": ""\""baz\"""",
    ""ScopeType"": ""QuoteVersion"",
    ""ScopeEntityId"": ""{scopeEntityId.ToString()}"",
    ""ScopeVersionNumber"": {scopeVersionNumber}
}}";
            var sut = JsonConvert.DeserializeObject<IncomingPolicyDataPatchCommandModel>(json);

            // Act
            var command = sut.ToCommand();

            // Assert
            Assert.True(command.Scope.Type == PatchScopeType.QuoteVersion);
            Assert.Equal(scopeEntityId, command.Scope.EntityId);
            Assert.Equal(scopeVersionNumber, command.Scope.VersionNumber);
        }

        [Fact]
        public void ToCommand_Throws_WhenScopeTypeIsQuoteVersionButScopeEntityIdIsMissing()
        {
            // Arrange
            var json = $@"{{
    ""Type"" : ""GivenValue"",
    ""targetFormDataPath"": ""foo"",
    ""targetCalculationResultPath"": ""bar"",
    ""NewValue"": ""\""baz\"""",
    ""ScopeType"": ""QuoteVersion"",
    ""ScopeVersionNumber"": 1
}}";
            var sut = JsonConvert.DeserializeObject<IncomingPolicyDataPatchCommandModel>(json);

            // Act
            Action act = () => sut.ToCommand();

            // Assert
            act.Should().Throw<BadRequestException>();
        }

        [Fact]
        public void ToCommand_Throws_WhenScopeTypeIsQuoteVersionButScopeVersionNumberIsMissing()
        {
            // Arrange
            var scopeEntityVersion = Guid.NewGuid();
            var json = $@"{{
    ""Type"" : ""GivenValue"",
    ""targetFormDataPath"": ""foo"",
    ""targetCalculationResultPath"": ""bar"",
    ""NewValue"": ""\""baz\"""",
    ""ScopeType"": ""QuoteVersion"",
    ""ScopeEntityId"": ""{scopeEntityVersion.ToString()}""
}}";
            var sut = JsonConvert.DeserializeObject<IncomingPolicyDataPatchCommandModel>(json);

            // Act
            Action act = () => sut.ToCommand();

            // Assert
            act.Should().Throw<BadRequestException>();
        }

        [Fact]
        public void ToCommand_ReturnsCombinedRules_WhenRulesAreSpecified()
        {
            // Arrange
            var json = @"{
    ""Type"" : ""GivenValue"",
    ""targetFormDataPath"": ""foo"",
    ""targetCalculationResultPath"": ""bar"",
    ""NewValue"": ""\""baz\"""",
    ""Rules"": [
        ""PropertyDoesNotExist"",
        ""PropertyIsMissingOrNullOrEmpty""
    ]
}";
            var sut = JsonConvert.DeserializeObject<IncomingPolicyDataPatchCommandModel>(json);

            // Act
            var command = sut.ToCommand();

            // Assert
            Assert.True(command.Rules.HasFlag(PatchRules.PropertyDoesNotExist));
            Assert.True(command.Rules.HasFlag(PatchRules.PropertyIsMissingOrNullOrEmpty));
        }

        [Fact]
        public void ToCommand_Throws_WhenRuleAreContradictory()
        {
            // Arrange
            var json = @"{
    ""Type"" : ""GivenValue"",
    ""targetFormDataPath"": ""foo"",
    ""targetCalculationResultPath"": ""bar"",
    ""NewValue"": ""\""baz\"""",
    ""Rules"": [
        ""PropertyDoesNotExist"",
        ""PropertyExists""
    ]
}";
            var sut = JsonConvert.DeserializeObject<IncomingPolicyDataPatchCommandModel>(json);

            // Act
            Action act = () => sut.ToCommand();

            // Assert
            act.Should().Throw<BadRequestException>();
        }
    }
}

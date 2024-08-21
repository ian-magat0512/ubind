// <copyright file="QuoteWorkflowTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

#pragma warning disable SA1201

namespace UBind.Domain.Tests.Aggregates.Quote.Workflow
{
    using System;
    using FluentAssertions;
    using Newtonsoft.Json;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Exceptions;
    using Xunit;

    public class QuoteWorkflowTests
    {
        private QuoteWorkflow sut;

        public QuoteWorkflowTests()
        {
            // Arrange
            this.sut = JsonConvert.DeserializeObject<QuoteWorkflow>(QuoteWorkflowJson);
        }

        [Fact]
        public void IsActionPermittedByState_ReturnsTrue_WhenStateMatchesSingleRequiredStatesInTransitionForAction()
        {
            // Act
            var isPermitted = this.sut.IsActionPermittedByState(QuoteAction.Actualise, "Nascent");

            // Assert
            Assert.True(isPermitted);
        }

        [Fact]
        public void IsActionPermittedByState_ReturnsTrue_WhenStateMatchesOneOfMultipleRequiredStatesInTransitionForAction()
        {
            // Act
            var approved = this.sut.IsActionPermittedByState(QuoteAction.ReviewReferral, "Approved");

            // Assert
            Assert.True(approved);
        }

        [Fact]
        public void IsActionPermittedByState_ReturnsFalse_WhenCurrentStateDoesNotMatchAnyOfRequiredStatesInTransitionForAction()
        {
            // Act
            var isPermitted = this.sut.IsActionPermittedByState(QuoteAction.ReviewReferral, "Nascent");

            // Assert
            Assert.False(isPermitted);
        }

        [Fact]
        public void IsActionPermittedByState_ThrowsErrorException_WhenWorkflowDoesNotIncludeTransitionForAction()
        {
            // Act
            Action act = () => this.sut.IsActionPermittedByState(QuoteAction.EndorsementReferral, "Review");

            // Assert
            act.Should().Throw<ErrorException>().And.Error.Code.Should().Be("quote.workflow.operation.not.defined");
        }

        [Fact]
        public void IsActionPermittedByState_ReturnsTrue_WhenActionIsFormUpdateAndStateIsApproved()
        {
            // Act
            var isPermitted = this.sut.IsActionPermittedByState(QuoteAction.FormUpdate, "Approved");

            // Assert
            Assert.True(isPermitted);
        }

        // TODO: Rename (and posible re-write all unit tests below here.
        [Fact]
        public void StateMachineLogicTest_ActionIsPermitted_AndReturnsStatusToIncomplete()
        {
            // Act
            var isPermitted = this.sut.IsActionPermittedByState(QuoteAction.Return, "Approved");

            // Assert
            Assert.True(isPermitted);
        }

        [Fact]
        public void StateMachineLogicTest_ActionIsPermitted_WhenActionIsQuoteVersion_AndCurrentStateIsApplicable()
        {
            // Act
            var isPermitted = this.sut.IsActionPermittedByState(QuoteAction.QuoteVersion, "Incomplete");

            // Assert
            Assert.True(isPermitted);
        }

        [Fact]
        public void StateMachineLogicTest_ActionIsNotPermitted_WhenActionIsQuoteVersion_AndCurrentStateIsNotApplicable()
        {
            // Act
            var isPermitted = this.sut.IsActionPermittedByState(QuoteAction.QuoteVersion, "Complete");

            // Assert
            Assert.False(isPermitted);
        }

        private static readonly string QuoteWorkflowJson = @"{
    ""type"": ""workflow"",
    ""isSettlementRequired"": true,
    ""bindOptions"": 2,
	""transitions"": [
		{
			""action"": ""Quote"",
			""requiredStates"": [""Nascent""], 
			""resultingState"": ""Incomplete""
		},
        {
			""action"": ""ReviewReferral"",
			""requiredStates"": [""Incomplete"", ""Approved""], 
			""resultingState"": ""Review""
		},
        {
			""action"": ""Return"",
			""requiredStates"": [""Review"", ""Approved"", ""Referred"" ], 
			""resultingState"": ""Incomplete""
		},
        {
            ""action"": ""QuoteVersion"",
            ""requiredStates"": [],
            ""resultingState"": """"
        }
	]
}";
    }
}

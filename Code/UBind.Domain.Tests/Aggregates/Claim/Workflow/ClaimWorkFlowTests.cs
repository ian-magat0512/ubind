// <copyright file="ClaimWorkFlowTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Aggregates.Claim.Workflow
{
    using FluentAssertions;
    using Newtonsoft.Json;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Aggregates.Claim.Workflow;
    using UBind.Domain.Extensions;
    using Xunit;

    public class ClaimWorkFlowTests
    {
        private static readonly string ClaimWorkflowJson = @"{
            ""type"": ""workflow"",
            ""isSettlementRequired"": true,
            ""bindOptions"": 2,
	        ""transitions"": [
		        {
			        ""action"": ""Actualise"",
			        ""requiredStates"": [""Nascent""], 
			        ""resultingState"": ""Incomplete""
		        },
                {
			        ""action"": ""AutoApproval"",
			        ""requiredStates"": [""Incomplete""], 
			        ""resultingState"": ""Approved""
		        },
                {
			        ""action"": ""Notify"",
			        ""requiredStates"": [""Incomplete""],
			        ""resultingState"": ""Notified""
		        },
                {
			        ""action"": ""Acknowledge"",
			        ""requiredStates"": [""Notified""],
			        ""resultingState"": ""Acknowledged""
		        }
	        ]
        }";

        private ClaimWorkflow sut;

        public ClaimWorkFlowTests()
        {
            // Arrange
            this.sut = JsonConvert.DeserializeObject<ClaimWorkflow>(ClaimWorkflowJson);
        }

        [Fact]
        public void Test_AllWorkflowActions_IsPermitted_IsCorrectResultState()
        {
            var transitions = this.sut.Transitions;
            foreach (var transition in transitions)
            {
                foreach (var state in transition.RequiredStates)
                {
                    this.IsPermittedByState_ReturnTrue(transition.Action.ToEnumOrThrow<ClaimActions>(), state);
                    this.IsCorrectResultState(transition.Action.ToEnumOrThrow<ClaimActions>(), state, transition.ResultingState);
                }
            }
        }

        [Fact]
        public void IsActionPermittedByState_ReturnsTrue_WhenClaim_NascentState()
        {
            this.IsPermittedByState_ReturnTrue(ClaimActions.Actualise, ClaimState.Nascent);
        }

        [Fact]
        public void IsActionPermittedByState_ReturnsFalse_WhenClaim_OtherState()
        {
            this.IsPermittedByState_ReturnFalse(ClaimActions.Actualise, ClaimState.Declined);
        }

        [Fact]
        public void IsCorrectResultState_ReturnsTrue_WhenClaim_NascentState_ExpectsIncomplete()
        {
            this.IsCorrectResultState(ClaimActions.Actualise, ClaimState.Nascent, ClaimState.Incomplete);
        }

        [Fact]
        public void IsInCorrectResultState_ReturnsTrue_WhenClaim_NascentState_OtherResultState()
        {
            this.IsInCorrectResultState(ClaimActions.Actualise, ClaimState.Nascent, ClaimState.Notified);
        }

        [Fact]
        public void IsActionPermittedByState_ReturnsTrue_WhenAutoApproval_IncompleteState()
        {
            this.IsPermittedByState_ReturnTrue(ClaimActions.AutoApproval, ClaimState.Incomplete);
        }

        [Fact]
        public void IsActionPermittedByState_ReturnsFalse_WhenAutoApproval_OtherState()
        {
            this.IsPermittedByState_ReturnFalse(ClaimActions.AutoApproval, ClaimState.Complete);
        }

        [Fact]
        public void IsCorrectResultState_ReturnsTrue_WhenAutoApproval_IncompleteState_ExpectsApproved()
        {
            this.IsCorrectResultState(ClaimActions.AutoApproval, ClaimState.Complete, ClaimState.Approved);
        }

        [Fact]
        public void IsInCorrectResultState_ReturnsTrue_WhenAutoApproval_IncompleteState_OtherResultState()
        {
            this.IsInCorrectResultState(ClaimActions.AutoApproval, ClaimState.Complete, ClaimState.Incomplete);
        }

        [Fact]
        public void IsActionPermittedByState_ReturnsTrue_WhenNotify_IncompleteState()
        {
            this.IsPermittedByState_ReturnTrue(ClaimActions.Notify, ClaimState.Incomplete);
        }

        [Fact]
        public void IsActionPermittedByState_ReturnsFalse_WhenNotify_OtherState()
        {
            this.IsPermittedByState_ReturnFalse(ClaimActions.Notify, ClaimState.Complete);
        }

        [Fact]
        public void IsCorrectResultState_ReturnsTrue_WhenNotify_IncompleteState_ExpectNotified()
        {
            this.IsCorrectResultState(ClaimActions.Notify, ClaimState.Incomplete, ClaimState.Notified);
        }

        [Fact]
        public void IsInCorrectResultState_ReturnsFalse_WhenNotify_IncompleteState_OtherResultState()
        {
            this.IsInCorrectResultState(ClaimActions.Notify, ClaimState.Incomplete, ClaimState.Review);
        }

        private void IsPermittedByState_ReturnTrue(ClaimActions operation, string currentState)
        {
            // Act
            var permitted = this.sut.IsActionPermittedByState(operation, currentState.ToString());

            // Assert
            permitted.Should().BeTrue();
        }

        private void IsPermittedByState_ReturnFalse(ClaimActions operation, string currentState)
        {
            // Act
            var permitted = this.sut.IsActionPermittedByState(operation, currentState);

            // Assert
            permitted.Should().BeFalse();
        }

        private void IsCorrectResultState(ClaimActions operation, string currentState, string expectedState)
        {
            // Act
            var resultState = this.sut.GetResultingState(operation, currentState);

            // Assert
            resultState.Should().Be(expectedState);
        }

        private void IsInCorrectResultState(ClaimActions operation, string currentState, string expectedState)
        {
            // Act
            var resultState = this.sut.GetResultingState(operation, currentState);

            // Assert
            resultState.Should().NotBe(expectedState);
        }
    }
}

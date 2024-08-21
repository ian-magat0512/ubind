// <copyright file="ClaimWorkflowOperationsControllerTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

// CS1591 Missing XML comment for publicly visible type or member
// Suppress CS1591. The unit test method should be named correctly instead of adding a comment in it.
#pragma warning disable CS1591

namespace UBind.Web.Tests.Controllers.RolesAndPermissions
{
    using System;
    using FluentAssertions;
    using UBind.Domain.Permissions;
    using UBind.Web.Controllers.Quoter;
    using UBind.Web.ResourceModels.Claim;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="ClaimControllerTest" />.
    /// </summary>
    public class ClaimWorkflowOperationsControllerTest
    {
        /// <summary>
        /// Defines the claimController.
        /// </summary>
        private ClaimWorkflowOperationsController claimWFOController;

        public ClaimWorkflowOperationsControllerTest()
        {
            this.claimWFOController = new ClaimWorkflowOperationsController(null, null, null, null, null);
        }

        [Fact]
        public void ApproveClaim_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageClaims.ToString(),
            };

            // Assert
            AuthorizationTest.IsAuthorized(
                this.claimWFOController,
                "AutoApproveClaim",
                new Type[] { typeof(string), typeof(ClaimFormDataUpdateModel) },
                userPermissions,
                null).Should().BeFalse();
        }

        [Fact]
        public void NotifyClaim_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageClaims.ToString(),
            };

            // Assert
            AuthorizationTest.IsAuthorized(
                this.claimWFOController,
                "NotifyClaim",
                new Type[] { typeof(string), typeof(ClaimFormDataUpdateModel) },
                userPermissions,
                null).Should().BeFalse();
        }

        [Fact]
        public void AcknowledgeClaim_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.AcknowledgeClaimNotifications.ToString(),
            };

            // Assert
            AuthorizationTest.IsAuthorized(
                this.claimWFOController,
                "AcknowledgeClaim",
                new Type[] { typeof(string), typeof(ClaimFormDataUpdateModel) },
                userPermissions,
                null).Should().BeTrue();
        }

        [Fact]
        public void ReturnClaim_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageClaims.ToString(),
            };

            // Assert
            AuthorizationTest.IsAuthorized(
                this.claimWFOController,
                "ReturnClaim",
                new Type[] { typeof(string), typeof(ClaimFormDataUpdateModel) },
                userPermissions,
                null).Should().BeTrue();
        }

        [Fact]
        public void ReviewReferralClaim_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageClaims.ToString(),
            };

            // Assert
            AuthorizationTest.IsAuthorized(
                this.claimWFOController,
                "ReviewReferralClaim",
                new Type[] { typeof(string), typeof(ClaimFormDataUpdateModel) },
                userPermissions,
                null).Should().BeTrue();
        }

        [Fact]
        public void ReviewApprovalClaim_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ReviewClaims.ToString(),
            };

            // Assert
            AuthorizationTest.IsAuthorized(
                this.claimWFOController,
                "ReviewApprovalClaim",
                new Type[] { typeof(string), typeof(ClaimFormDataUpdateModel) },
                userPermissions,
                null).Should().BeTrue();
        }

        [Fact]
        public void AssessmentReferralClaim_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageClaims.ToString(),
            };

            // Assert
            AuthorizationTest.IsAuthorized(
                this.claimWFOController,
                "AssessmentReferralClaim",
                new Type[] { typeof(string), typeof(ClaimFormDataUpdateModel) },
                userPermissions,
                null).Should().BeTrue();
        }

        [Fact]
        public void AssessmentApprovalClaim_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.AssessClaims.ToString(),
            };

            // Assert
            AuthorizationTest.IsAuthorized(
                this.claimWFOController,
                "AssessmentApprovalClaim",
                new Type[] { typeof(string), typeof(ClaimFormDataUpdateModel) },
                userPermissions,
                null).Should().BeTrue();
        }

        [Fact]
        public void DeclineClaim_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageClaims.ToString(),
            };

            // Assert
            AuthorizationTest.IsAuthorized(
                this.claimWFOController,
                "DeclineClaim",
                new Type[] { typeof(string), typeof(ClaimFormDataUpdateModel) },
                userPermissions,
                null).Should().BeTrue();
        }

        [Fact]
        public void WithdrawClaim_Success()
        {
            // Assert
            AuthorizationTest.IsAnonymous(
                 this.claimWFOController,
                 "WithdrawClaim",
                 new Type[] { typeof(string), typeof(ClaimFormDataUpdateModel) })
                 .Should().BeTrue();
        }

        [Fact]
        public void SettleClaim_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageClaims.ToString(),
            };

            // Assert
            AuthorizationTest.IsAuthorized(
                this.claimWFOController,
                "SettleClaim",
                new Type[] { typeof(string), typeof(ClaimFormDataUpdateModel) },
                userPermissions,
                null).Should().BeTrue();
        }
    }
}

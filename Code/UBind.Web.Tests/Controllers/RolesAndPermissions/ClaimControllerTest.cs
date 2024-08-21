// <copyright file="ClaimControllerTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

// CS1591 Missing XML comment for publicly visible type or member
// Suppress CS1591. The unit test method must be named correctly instead of adding a comment in it.
#pragma warning disable CS1591

namespace UBind.Web.Tests.Controllers.RolesAndPermissions
{
    using System;
    using FluentAssertions;
    using UBind.Domain.Permissions;
    using UBind.Web.Controllers;
    using UBind.Web.ResourceModels;
    using UBind.Web.ResourceModels.Claim;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="ClaimControllerTest" />.
    /// </summary>
    public class ClaimControllerTest
    {
        /// <summary>
        /// Defines the claimController.
        /// </summary>
        private readonly ClaimController claimController;

        public ClaimControllerTest()
        {
            this.claimController = new ClaimController(
                null, null, null, null, null, null, null, null, null, null, null, null, null, null);
        }

        /// <summary>
        /// The Get_Claims_Success.
        /// </summary>
        [Fact]
        public void Get_Claims_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewClaims.ToString(),
            };

            // Assert
            AuthorizationTest
                .IsAuthorized(
                    this.claimController,
                    "GetClaims",
                    new Type[] { typeof(QueryOptionsModel) },
                    userPermissions,
                    null)
                .Should()
                .BeTrue();
        }

        /// <summary>
        /// Test associate claim with policy should succeed.
        /// </summary>
        [Fact]
        public void AssociateClaimWithPolicy_ShouldSucceed()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageClaims.ToString(),
            };

            // Assert
            AuthorizationTest
                .IsAuthorized(
                    this.claimController,
                    "AssociateClaimWithPolicy",
                    new Type[] { typeof(Guid), typeof(Guid) },
                    userPermissions,
                    null)
                .Should()
                .BeTrue();
        }

        /// <summary>
        /// Test associate claim with policy should throw unauthorized when user does not have permission.
        /// </summary>
        [Fact]
        public void AssociateClaimWithPolicy_ShouldThrowUnauthorized_WhenUserDoesNotHavePermission()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewMessages.ToString(),
            };

            // Assert
            AuthorizationTest
                .IsAuthorized(
                    this.claimController,
                    "AssociateClaimWithPolicy",
                    new Type[] { typeof(Guid), typeof(Guid) },
                    userPermissions,
                    null)
                .Should()
                .BeFalse();
        }

        /// <summary>
        /// The Get_Claims_Forbidden.
        /// </summary>
        [Fact]
        public void Get_Claims_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewMessages.ToString(),
            };

            // Assert
            AuthorizationTest
                .IsAuthorized(
                    this.claimController,
                    "GetClaims",
                    new Type[] { typeof(QueryOptionsModel) },
                    userPermissions,
                    null)
                .Should()
                .BeFalse();
        }

        /// <summary>
        /// The Get_ClaimDetails_Success.
        /// </summary>
        [Fact]
        public void Get_ClaimDetails_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewClaims.ToString(),
            };

            // Assert
            AuthorizationTest
                .IsAuthorized(
                    this.claimController,
                    "GetClaimDetails",
                    new Type[] { typeof(Guid), typeof(string) },
                    userPermissions,
                    null)
                .Should()
                .BeTrue();
        }

        /// <summary>
        /// The Get_ClaimDetails_Forbidden.
        /// </summary>
        [Fact]
        public void Get_ClaimDetails_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewPolicies.ToString(),
            };

            // Assert
            AuthorizationTest
                .IsAuthorized(
                    this.claimController,
                    "GetClaimDetails",
                    new Type[] { typeof(Guid), typeof(string) },
                    userPermissions,
                    null)
                .Should()
                .BeFalse();
        }

        /// <summary>
        /// The AssignClaimNumber_Success.
        /// </summary>
        [Fact]
        public void Patch_ClaimNumber_Assign_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageClaims.ToString(),
            };

            // Assert
            AuthorizationTest
                .IsAuthorized(
                    this.claimController,
                    "AssignClaimNumber",
                    new Type[] { typeof(Guid), typeof(string), typeof(ClaimNumberAssignmentModel) },
                    userPermissions,
                    null)
                .Should()
                .BeTrue();
        }

        /// <summary>
        /// The AssignClaimNumber_Forbidden.
        /// </summary>
        [Fact]
        public void Patch_ClaimNumber_Assign_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageCustomers.ToString(),
            };

            // Assert
            AuthorizationTest
                .IsAuthorized(
                    this.claimController,
                    "AssignClaimNumber",
                    new Type[] { typeof(Guid), typeof(string), typeof(ClaimNumberAssignmentModel) },
                    userPermissions,
                    null)
                .Should()
                .BeFalse();
        }

        /// <summary>
        /// The AssignClaimNumber_Success.
        /// </summary>
        [Fact]
        public void Patch_ClaimNumber_Unassign_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageClaims.ToString(),
            };

            // Assert
            AuthorizationTest
                .IsAuthorized(
                    this.claimController,
                    "UnassignClaimNumber",
                    new Type[] { typeof(Guid), typeof(string), typeof(ClaimNumberAssignmentModel) },
                    userPermissions,
                    null)
                .Should()
                .BeTrue();
        }

        /// <summary>
        /// The AssignClaimNumber_Forbidden.
        /// </summary>
        [Fact]
        public void Patch_ClaimNumber_Unassign_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageCustomers.ToString(),
            };

            // Assert
            AuthorizationTest
                .IsAuthorized(
                    this.claimController,
                    "UnassignClaimNumber",
                    new Type[] { typeof(Guid), typeof(string), typeof(ClaimNumberAssignmentModel) },
                    userPermissions,
                    null)
                .Should()
                .BeFalse();
        }
    }
}

// <copyright file="PersonControllerTest.cs" company="uBind">
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
    using UBind.Domain;
    using UBind.Domain.Permissions;
    using UBind.Web.Controllers;
    using UBind.Web.ResourceModels;
    using UBind.Web.ResourceModels.Person;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="PersonControllerTest" />.
    /// </summary>
    public class PersonControllerTest
    {
        /// <summary>
        /// Defines the PersonController.
        /// </summary>
        private PersonController personController;

        public PersonControllerTest()
        {
            this.personController = new PersonController(null, null, null, null);
        }

        /// <summary>
        /// The Get_People_Success.
        /// </summary>
        [Fact]
        public void Get_People_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewCustomers.ToString(),
            };

            // Assert
            Assert.True(this.IsAuthorized(
                "GetPeople",
                new Type[] { typeof(QueryOptionsModel) },
                userPermissions));
        }

        /// <summary>
        /// The Get_People_Forbidden.
        /// </summary>
        [Fact]
        public void Get_People_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewClaims.ToString(),
            };

            // Assert
            Assert.False(this.IsAuthorized(
                "GetPeople",
                new Type[] { typeof(QueryOptionsModel) },
                userPermissions));
        }

        /// <summary>
        /// The Get_PersonDetails_Success.
        /// </summary>
        [Fact]
        public void Get_PersonDetails_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewCustomers.ToString(),
            };

            // Assert
            Assert.True(this.IsAuthorized(
                "GetPersonDetails",
                new Type[] { typeof(Guid) },
                userPermissions));
        }

        /// <summary>
        /// The Get_PersonDetails_Success.
        /// </summary>
        [Fact]
        public void Get_PersonDetails_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewClaims.ToString(),
            };

            // Assert
            Assert.False(this.IsAuthorized(
                "GetPersonDetails",
                new Type[] { typeof(Guid) },
                userPermissions));
        }

        /// <summary>
        /// The Post_CreatePerson_Success.
        /// </summary>
        [Fact]
        public void Post_CreatePerson_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageCustomers.ToString(),
            };

            // Assert
            Assert.True(this.IsAuthorized(
                "CreatePerson",
                new Type[] { typeof(PersonUpsertModel) },
                userPermissions));
        }

        /// <summary>
        /// The Post_CreatePerson_Forbidden.
        /// </summary>
        [Fact]
        public void Post_CreatePerson_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewClaims.ToString(),
            };

            // Assert
            Assert.False(this.IsAuthorized(
                "CreatePerson",
                new Type[] { typeof(PersonUpsertModel) },
                userPermissions));
        }

        /// <summary>
        /// The Put_UpdatePerson_Success.
        /// </summary>
        [Fact]
        public void Put_UpdatePerson_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageCustomers.ToString(),
            };

            // Assert
            Assert.True(this.IsAuthorized(
                "UpdatePerson",
                new Type[] { typeof(Guid), typeof(PersonUpsertModel) },
                userPermissions));
        }

        /// <summary>
        /// The Put_UpdatePerson_Forbidden.
        /// </summary>
        [Fact]
        public void Put_UpdatePerson_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewClaims.ToString(),
            };

            // Assert
            Assert.False(this.IsAuthorized(
                "UpdatePerson",
                new Type[] { typeof(Guid), typeof(PersonUpsertModel) },
                userPermissions));
        }

        /// <summary>
        /// The Delete_Person_Success.
        /// </summary>
        [Fact]
        public void Delete_Person_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageCustomers.ToString(),
            };

            // Assert
            Assert.True(this.IsAuthorized(
                "DeletePerson",
                new Type[] { typeof(Guid) },
                userPermissions));
        }

        /// <summary>
        /// The Delete_Person_Forbidden.
        /// </summary>
        [Fact]
        public void Delete_Person_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ViewClaims.ToString(),
            };

            // Assert
            Assert.False(this.IsAuthorized(
                "DeletePerson",
                new Type[] { typeof(Guid) },
                userPermissions));
        }

        [Fact]
        public void CreateUserAccountForCustomer_Customer_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageMessages.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.personController,
                "CreateUserAccountForPerson",
                new Type[] { typeof(Guid), typeof(DeploymentEnvironment) },
                userPermissions,
                null));
        }

        [Fact]
        public void CreateUserAccountForCustomer_Customer_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageCustomers.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.personController,
                "CreateUserAccountForPerson",
                new Type[] { typeof(Guid), typeof(DeploymentEnvironment) },
                userPermissions,
                null));
        }

        [Fact]
        public void Deactivate_Customer_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageQuotes.ToString(),
                Permission.ManagePortals.ToString(),
                Permission.ManageCustomers.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.personController,
                "Deactivate",
                new Type[] { typeof(Guid) },
                userPermissions,
                null));
        }

        [Fact]
        public void Deactivate_Customer_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageQuotes.ToString(),
                Permission.ManagePortals.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.personController,
                "Deactivate",
                new Type[] { typeof(Guid) },
                userPermissions,
                null));
        }

        [Fact]
        public void Reactivate_Customer_Success()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageCustomers.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.personController,
                "Reactivate",
                new Type[] { typeof(Guid) },
                userPermissions,
                null));
        }

        [Fact]
        public void Reactivate_Customer_Forbidden()
        {
            string[] userPermissions = new string[]
            {
                Permission.ManageQuotes.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.personController,
                "Reactivate",
                new Type[] { typeof(Guid) },
                userPermissions,
                null));
        }

        private bool IsAuthorized(string functionName, Type[] paramterTypes, string[] userPermissions)
        {
            return AuthorizationTest.IsAuthorized(
                this.personController,
                functionName,
                paramterTypes,
                userPermissions,
                null);
        }
    }
}

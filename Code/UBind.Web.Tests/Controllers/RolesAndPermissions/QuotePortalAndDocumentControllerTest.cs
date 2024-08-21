// <copyright file="QuotePortalAndDocumentControllerTest.cs" company="uBind">
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
    using System.Collections.Generic;
    using UBind.Domain.Permissions;
    using UBind.Web.Controllers;
    using UBind.Web.Controllers.Portal;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="QuotePortalAndDocumentControllerTest" />.
    /// </summary>
    public class QuotePortalAndDocumentControllerTest
    {
        /// <summary>
        /// Defines the quotePortalController.
        /// </summary>
        private QuoteController quotePortalController;

        /// <summary>
        /// Defines the quoteDocumentController.
        /// </summary>
        private QuoteDocumentController quoteDocumentController;

        /// <summary>
        /// Defines the quoteVersionController.
        /// </summary>
        private QuoteVersionController quoteVersionController;

        public QuotePortalAndDocumentControllerTest()
        {
            this.quotePortalController = new QuoteController(null, null, null, null, null, null, null, null, null, null, null, null);
            this.quoteDocumentController = new QuoteDocumentController(null);
            this.quoteVersionController = new QuoteVersionController(null, null, null, null, null, null, null, null, null);
        }

        /// <summary>
        /// The Get_QuoteDetails_Success.
        /// </summary>
        [Fact]
        public void Get_QuoteDetails_Success()
        {
            List<string> userPermissions = new List<string>
            {
                Permission.ViewQuotes.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.quotePortalController,
                "GetQuoteDetails",
                new Type[] { typeof(Guid), typeof(string) },
                userPermissions.ToArray(),
                null));
        }

        /// <summary>
        /// The Get_QuoteDetails_Forbidden.
        /// </summary>
        [Fact]
        public void Get_QuoteDetails_Forbidden()
        {
            List<string> userPermissions = new List<string>
            {
                Permission.ViewMessages.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.quotePortalController,
                "GetQuoteDetails",
                new Type[] { typeof(Guid), typeof(string) },
                userPermissions.ToArray(),
                null));
        }

        /// <summary>
        /// The Get_QuoteFormdata_Success.
        /// </summary>
        [Fact]
        public void Get_QuoteFormdata_Success()
        {
            List<string> userPermissions = new List<string>
            {
                Permission.ViewQuotes.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.quotePortalController,
                "GetFormData",
                new Type[] { typeof(Guid), typeof(string) },
                userPermissions.ToArray(),
                null));
        }

        /// <summary>
        /// The Get_QuoteFormdata_Forbidden.
        /// </summary>
        [Fact]
        public void Get_QuoteFormdata_Forbidden()
        {
            List<string> userPermissions = new List<string>
            {
                Permission.ViewPolicies.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.quotePortalController,
                "GetFormData",
                new Type[] { typeof(Guid), typeof(string) },
                userPermissions.ToArray(),
                null));
        }

        /// <summary>
        /// The Get_QuoteVersionsList_Success.
        /// </summary>
        [Fact]
        public void Get_QuoteVersionsList_Success()
        {
            List<string> userPermissions = new List<string>
            {
                Permission.ViewQuoteVersions.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.quotePortalController,
                "GetQuoteVersionsListAsync",
                new Type[] { typeof(Guid) },
                userPermissions.ToArray(),
                null));
        }

        /// <summary>
        /// The Get_QuoteVersionsList_Forbidden.
        /// </summary>
        [Fact]
        public void Get_QuoteVersionsList_Forbidden()
        {
            List<string> userPermissions = new List<string>
            {
                Permission.ViewRoles.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.quotePortalController,
                "GetQuoteVersionsListAsync",
                new Type[] { typeof(Guid) },
                userPermissions.ToArray(),
                null));
        }

        /// <summary>
        /// The Get_QuoteVersionDetail_Success.
        /// </summary>
        [Fact]
        public void Get_QuoteVersionDetail_Success()
        {
            List<string> userPermissions = new List<string>
            {
                Permission.ViewQuoteVersions.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.quoteVersionController,
                "GetQuoteVersionDetail",
                new Type[] { typeof(Guid), typeof(string) },
                userPermissions.ToArray(),
                null));
        }

        /// <summary>
        /// The Get_QuoteVersionDetail_Forbidden.
        /// </summary>
        [Fact]
        public void Get_QuoteVersionDetail_Forbidden()
        {
            List<string> userPermissions = new List<string>
            {
                Permission.ViewQuoteVersions.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.quoteVersionController,
                "GetQuoteVersionDetail",
                new Type[] { typeof(Guid), typeof(string) },
                userPermissions.ToArray(),
                null));
        }

        /// <summary>
        /// The Get_QuoteDocument_Success.
        /// </summary>
        [Fact]
        public void Get_QuoteDocument_Success()
        {
            List<string> userPermissions = new List<string>
            {
                Permission.ViewQuotes.ToString(),
            };

            // Assert
            Assert.True(AuthorizationTest.IsAuthorized(
                this.quoteDocumentController,
                "GetQuoteDocument",
                new Type[] { typeof(Guid), typeof(Guid) },
                userPermissions.ToArray(),
                null));
        }

        /// <summary>
        /// The Get_QuoteDocument_Forbidden.
        /// </summary>
        [Fact]
        public void Get_QuoteDocument_Forbidden()
        {
            List<string> userPermissions = new List<string>
            {
                Permission.ManageCustomers.ToString(),
            };

            // Assert
            Assert.False(AuthorizationTest.IsAuthorized(
                this.quoteDocumentController,
                "GetQuoteDocument",
                new Type[] { typeof(Guid), typeof(Guid) },
                userPermissions.ToArray(),
                null));
        }
    }
}

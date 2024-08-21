// <copyright file="PortalModelValidationUnitTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Tests.ModelValidation
{
    using System;
    using UBind.Domain;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Web.ResourceModels.ModelHelpers;
    using UBind.Web.ResourceModels.Portal;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="PortalModelValidationUnitTest" />.
    /// </summary>
    public class PortalModelValidationUnitTest
    {
        /// <summary>
        /// The PortalModel_Validation_Success_IfValidInput.
        /// </summary>
        /// <param name="name">The name<see cref="string"/>.</param>
        /// <param name="alias">The alias<see cref="string"/>.</param>
        /// <param name="title">The title<see cref="string"/>.</param>
        /// <param name="stylesheetUrl">The stylesheet url<see cref="string"/>.</param>
        [InlineData(
            "jeo",
            "alias-2",
            "this some title",
            "http://google.com:123/css.css?")]
        [InlineData(
            "jeo",
            "alias-1",
            "this some tiztle",
            "http://google.com:123/css.css")]
        [InlineData(
            "jeo talavera",
            "voxroowoq",
            "anothertitle",
            "http://google.com/qweww.css?qwe=qwe")]
        [InlineData(
            "jeo talavera",
            "roroew-we",
            "PortalOrg2-1",
            "http://localhost/qwosxow/qwoexqwe/qweww.css?qwe=qwe")]
        [Theory]
        public void PortalModel_Validation_Success_IfValidInput(
           string name,
           string alias,
           string title,
           string stylesheetUrl)
        {
            // Arrange
            var tenant = new Tenant(Guid.NewGuid());
            var portal = new PortalReadModel
            {
                TenantId = Guid.NewGuid(),
                Id = Guid.NewGuid(),
                Name = name,
                Alias = alias,
                Title = title,
                StyleSheetUrl = stylesheetUrl,
                Disabled = false,
                Deleted = false,
            };
            var model = new PortalModel(portal);

            // Act
            var results = ResourceModelTestHelper.Validate(model);

            // Assert
            Assert.Equal(0, results.Count);
        }

        /// <summary>
        /// The PortalModel_Validation_Fail_IfInvalidInput.
        /// </summary>
        /// <param name="name">The name<see cref="string"/>.</param>
        /// <param name="alias">The alias<see cref="string"/>.</param>
        /// <param name="title">The title<see cref="string"/>.</param>
        /// <param name="stylesheetUrl">The stylesheet url<see cref="string"/>.</param>
        [InlineData(
           "",
           "",
           "",
           "localhost://123")]
        [InlineData(
           "  ",
           "-voxroowoq-",
           "  ",
           "google.css")]
        [InlineData(
           "             ",
           "-roroew-we-",
           "()",
           "google.com.ph/style.css")]
        [InlineData(
           "@",
           "boomTown",
           ")!@(#)!(@#!@!@#",
           "google.com.ph?test=true")]
        [InlineData(
           "@",
           "boomTown",
           ")!@(#)!(@#!@!@#",
           "testing.com.ph@qo.css")]
        [Theory]
        public void PortalModel_Validation_Fail_IfInvalidInput(
          string name,
          string alias,
          string title,
          string stylesheetUrl)
        {
            // Arrange
            var tenant = new Tenant(Guid.NewGuid());
            var portal = new PortalReadModel
            {
                TenantId = Guid.NewGuid(),
                Id = Guid.NewGuid(),
                Name = name,
                Alias = alias,
                Title = title,
                StyleSheetUrl = stylesheetUrl,
                Disabled = false,
                Deleted = false,
            };
            var model = new PortalModel(portal);

            // Act
            var results = ResourceModelTestHelper.Validate(model);

            // Assert
            Assert.Equal(4, results.Count);
        }
    }
}

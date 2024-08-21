// <copyright file="AccountUpdateViewModelValidationUnitTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

// CS1591 Missing XML comment for publicly visible type or member
// Suppress CS1591. The unit test method must be named correctly instead of adding a comment in it.
#pragma warning disable CS1591

namespace UBind.Web.Tests.ModelValidation
{
    using UBind.Web.ResourceModels;
    using UBind.Web.ResourceModels.ModelHelpers;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="AccountUpdateViewModelValidationUnitTest" />.
    /// </summary>
    public class AccountUpdateViewModelValidationUnitTest
    {
        [InlineData(
            "jeo",
            "john walter talavera",
            "nprefix",
            "fname",
            "middle",
            "lname",
            "nsuffix",
            "uBind",
            "developer",
            "jeo.taalv@gmail.com",
            "jeo.taalv@gmail.com",
            "0412345678",
            "0412345678",
            "0412345678")]
        [InlineData(
            "jeo talavera",
            "oowowewkqo www III",
            "nprefix",
            "fname",
            "middle",
            "lname",
            "nsuffix",
            "uBind",
            "developer",
            "jeo.taalv@gmail.com",
            "",
            "",
            "",
            "")]
        [InlineData(
            "jeo talavera",
            "oowowewkqo www III",
            "nprefix",
            "fname",
            "middle",
            "lname",
            "nsuffix",
            "uBind",
            "developer",
            "jeo.taalv@gmail.com",
            "",
            "+61 5 0000 0000",
            "00 0000 0000",
            "+61 0 0000 0000")]
        [InlineData(
            "jeo-talavera smith",
            "calabia o. presinto",
            "nprefix",
            "fname",
            "middle",
            "lname",
            "nsuffix",
            "uBind",
            "developer",
            "jeo.taalv@gmail.com",
            "",
            "+61 5 0000 0000",
            "00 0000 0000",
            "+61 0 0000 0000")]
        [InlineData(
            "smithson' walambo",
            "thingsomename -'. , ",
            "nprefix",
            "fname",
            "middle",
            "lname",
            "nsuffix",
            "uBind",
            "developer",
            "jeo.taalv@gmail.com",
            "",
            "+61 5 0000 0000",
            "00 0000 0000",
            "+61 0 0000 0000")]
        [Theory]
        public void AccountUpdateViewModel_Validation_Success_IfValidInput(
            string preferredName,
            string fullName,
            string namePrefix,
            string firstName,
            string middelNames,
            string lastName,
            string nameSuffix,
            string company,
            string title,
            string email,
            string alternativeEmail,
            string mobilePhone,
            string workPhone,
            string homePhone)
        {
            // Arrange
            var model = new AccountUpdateViewModel()
            {
                PreferredName = preferredName,
                FullName = fullName,
                NamePrefix = namePrefix,
                FirstName = firstName,
                MiddleNames = middelNames,
                LastName = lastName,
                NameSuffix = nameSuffix,
                Company = company,
                Title = title,
                Email = email,
                AlternativeEmail = alternativeEmail,
                MobilePhone = mobilePhone,
                WorkPhone = workPhone,
                HomePhone = homePhone,
            };

            // Act
            var results = ResourceModelTestHelper.Validate(model);

            // Assert
            Assert.Equal(0, results.Count);
        }

        [InlineData(
            "@oowowewk*o www III",
            "@oowowewkqo www III",
            "nprefix*",
            "fname*",
            "middle*",
            "lname*",
            "nsuffix*",
            "uBind*",
            "developer*",
            "john.talavera@@u in.com",
            "jeo.taalv@gmail.com@",
            "+61 5 0000 00a0",
            "041s2345678",
            "041230045678")]
        [InlineData(
            "@oowowewkqo www III",
            "@oowowewkqo www III",
            "nprefix*",
            "fname*",
            "middle*",
            "lname*",
            "nsuffix*",
            "uBind*",
            "developer*",
            "j@eo.taalv@gmail.com",
            "jeo..taalv@gmail.com@",
            "wdq",
            "dwqw",
            "wwwwwwwwwww")]
        [InlineData(
            "20231",
            "@oowowewkqo www III",
            "nprefix*",
            "fname*",
            "middle*",
            "lname*",
            "nsuffix*",
            "uBind*",
            "developer*",
            "2222",
            "2222222",
            "+61 5 0000 00a0",
            "*00 5 0000 00a0",
            "+41 0 0000 0000")]
        [InlineData(
            "^&!)(*@*&#^$",
            "~~~",
            "nprefix*",
            "fname*",
            "middle*",
            "lname*",
            "nsuffix*",
            "uBind*",
            "developer*",
            "2222",
            "2222222",
            "+41 5 0000 00a0",
            "+21 5 0000 00a0",
            "+41 0 0000 0000")]
        [Theory]
        public void AccountUpdateViewModel_Validation_Fail_IfInvalidInput(
            string preferredName,
            string fullName,
            string namePrefix,
            string firstName,
            string middelNames,
            string lastName,
            string nameSuffix,
            string company,
            string title,
            string email,
            string alternativeEmail,
            string mobilePhone,
            string workPhone,
            string homePhone)
        {
            // Arrange
            var model = new AccountUpdateViewModel()
            {
                PreferredName = preferredName,
                FullName = fullName,
                NamePrefix = namePrefix,
                FirstName = firstName,
                MiddleNames = middelNames,
                LastName = lastName,
                NameSuffix = nameSuffix,
                Company = company,
                Title = title,
                Email = email,
                AlternativeEmail = alternativeEmail,
                MobilePhoneNumber = mobilePhone,
                WorkPhoneNumber = workPhone,
                HomePhoneNumber = homePhone,
            };

            // Act
            var results = ResourceModelTestHelper.Validate(model);

            // Assert
            Assert.Equal(0, results.Count);
        }
    }
}

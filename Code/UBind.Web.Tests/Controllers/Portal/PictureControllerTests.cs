// <copyright file="PictureControllerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

// CS1591 Missing XML comment for publicly visible type or member
// Suppress CS1591. The unit test method must be named correctly instead of adding a comment in it.
#pragma warning disable CS1591

namespace UBind.Web.Tests.Controllers.Portal
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Primitives;
    using Moq;
    using UBind.Application.Infrastructure;
    using UBind.Application.User;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Web.Controllers;
    using Xunit;

    public class PictureControllerTests
    {
        private Mock<ICachingResolver> mockcachingResolver;
        private Mock<IUserProfilePictureRepository> mockUserProfilePictureRepo;
        private Mock<IUserService> mockUserService;
        private PictureController sut;

        public PictureControllerTests()
        {
            this.mockcachingResolver = new Mock<ICachingResolver>();
            this.mockUserProfilePictureRepo = new Mock<IUserProfilePictureRepository>();
            this.mockUserService = new Mock<IUserService>();
            this.sut = new PictureController(
                this.mockUserProfilePictureRepo.Object,
                this.mockcachingResolver.Object);
        }

        private IFormFileCollection MockFormFileCollection
        {
            get
            {
                var fileMock = new Mock<IFormFile>();
                fileMock.Setup(_ => _.Length).Returns(0);
                var coll = new FormFileCollection();
                coll.Add(fileMock.Object);

                return coll;
            }
        }

        [Fact]
        public void GetPicture_Returns_CorrectPictureDataValue_When_UserIdIsFound()
        {
            // Arrange
            var pictureData = new byte[] { 1, 2, 3 };
            var guid = Guid.Parse("3CB23246-66A1-478F-B9C7-F4A8180D0F64");
            var picData = Convert.ToBase64String(pictureData);
            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        new Claim[]
                   {
                        new Claim(ClaimTypes.Name, "example name"),
                        new Claim(ClaimNames.TenantId, Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.NameIdentifier, "3CB23246-66A1-478F-B9C7-F4A8180D0F64"),
                   }, "mock")),
                },
            };

            context.HttpContext.Request.Form = new FormCollection(It.IsAny<Dictionary<string, StringValues>>(), this.MockFormFileCollection);
            this.sut.ControllerContext = context;

            this.mockUserProfilePictureRepo.Setup(_ => _.GetById(It.IsAny<Guid>())).Returns(new UserProfilePicture(pictureData));

            // Act
            var result = this.sut.GetPicture(guid, It.IsAny<string>()) as FileContentResult;

            // Assert
            Assert.Equal(picData, Convert.ToBase64String(result.FileContents));
        }
    }
}

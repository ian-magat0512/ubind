// <copyright file="AdditionalPropertyControllerFixture.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

// CS1591 Missing XML comment for publicly visible type or member
// Supporess CS1591. Variable and method named must be named correctly instead of adding comment.
#pragma warning disable CS1591

namespace UBind.Web.Tests.Controllers.Portal
{
    using Hangfire;
    using Hangfire.Storage;
    using Microsoft.Extensions.Hosting;
    using Moq;
    using NodaTime;
    using UBind.Application.Authorisation;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Services.AdditionalPropertyDefinition;
    using UBind.Web.Controllers.Portal;
    using UBind.Web.Helpers;

    public class AdditionalPropertyControllerFixture
    {
        public AdditionalPropertyControllerFixture()
        {
            this.MediatorMock = new Mock<ICqrsMediator>(MockBehavior.Strict);
            this.AdditionalPropertyValidatorMock = new Mock<IAdditionalPropertyDefinitionValidator>(
                MockBehavior.Strict);
            this.BackgroundJobClient = new Mock<IBackgroundJobClient>();
            this.StorageConnection = new Mock<IStorageConnection>(MockBehavior.Strict);
            this.ClockMock = new Mock<IClock>();
            this.PerformerResolverMock = new Mock<IHttpContextPropertiesResolver>();
            this.AdditionalPropertyContextValidatorMock = new Mock<AdditionalPropertyDefinitionContextValidator>(
                MockBehavior.Strict);
            this.AdditionalPropertyModelResolver = new Mock<IAdditionalPropertyModelResolverHelper>();
            this.AdditionalPropertyAuthorisationServiceMock = new Mock<IAdditionalPropertyAuthorisationService>();
            this.CachingResolverMock = new Mock<ICachingResolver>();
            this.IHostingEnvironment = new Mock<IHostEnvironment>();
            this.AdditionalPropertyDefinitionJsonValidator = new Mock<IAdditionalPropertyDefinitionJsonValidator>();

            this.Controller = new AdditionalPropertyDefinitionController(
                this.MediatorMock.Object,
                this.ClockMock.Object,
                this.PerformerResolverMock.Object,
                this.AdditionalPropertyValidatorMock.Object,
                this.AdditionalPropertyModelResolver.Object,
                this.CachingResolverMock.Object,
                this.AdditionalPropertyAuthorisationServiceMock.Object);
        }

        public AdditionalPropertyDefinitionController Controller { get; private set; }

        public Mock<ICqrsMediator> MediatorMock { get; private set; }

        public Mock<IAdditionalPropertyDefinitionValidator> AdditionalPropertyValidatorMock { get; }

        public Mock<AdditionalPropertyDefinitionContextValidator> AdditionalPropertyContextValidatorMock { get; private set; }

        public Mock<IAdditionalPropertyModelResolverHelper> AdditionalPropertyModelResolver { get; }

        public Mock<IBackgroundJobClient> BackgroundJobClient { get; }

        public Mock<IStorageConnection> StorageConnection { get; }

        public Mock<IClock> ClockMock { get; }

        public Mock<IHttpContextPropertiesResolver> PerformerResolverMock { get; }

        public Mock<IAuthorisationService> AuthorisationServiceMock { get; }

        public Mock<IAdditionalPropertyAuthorisationService> AdditionalPropertyAuthorisationServiceMock { get; }

        public Mock<ICachingResolver> CachingResolverMock { get; }

        public Mock<IHostEnvironment> IHostingEnvironment { get; set; }

        public Mock<IAdditionalPropertyDefinitionJsonValidator> AdditionalPropertyDefinitionJsonValidator { get; set; }
    }
}

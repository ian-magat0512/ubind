// <copyright file="FakeAdditionalPropertyValueService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Fakes
{
    using System.Collections.Generic;
    using Moq;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Enums;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.Services.AdditionalPropertyDefinition;
    using UBind.Domain.Services.AdditionalPropertyValue;

    /// <summary>
    /// Fake Additional Property Value Service class.
    /// </summary>
    internal class FakeAdditionalPropertyValueService : AdditionalPropertyValueService
    {
        public FakeAdditionalPropertyValueService(IAdditionalPropertyDefinitionRepository? additionalPropertyDefinitionRepository = null)
        : base(
                new Mock<ICqrsMediator>().Object,
                new Mock<ICustomerReadModelRepository>().Object,
                new Mock<IQuoteReadModelRepository>().Object,
                new Mock<IPolicyTransactionReadModelRepository>().Object,
                new Mock<IQuoteVersionReadModelRepository>().Object,
                new Mock<IClaimVersionReadModelRepository>().Object,
                additionalPropertyDefinitionRepository == null
                    ? new Mock<IAdditionalPropertyDefinitionRepository>().Object
                    : additionalPropertyDefinitionRepository,
                new PropertyTypeEvaluatorService(
                new Dictionary<AdditionalPropertyDefinitionType, IAdditionalPropertyValueProcessor>()),
                new Mock<IHttpContextPropertiesResolver>().Object,
                new Mock<IQuoteAggregateRepository>().Object,
                new Mock<IClaimAggregateRepository>().Object,
                new Mock<ICustomerAggregateRepository>().Object,
                new Mock<IUserAggregateRepository>().Object,
                new Mock<IOrganisationAggregateRepository>().Object,
                new Mock<IClock>().Object,
                new Mock<IAdditionalPropertyDefinitionJsonValidator>().Object,
                new Mock<IAdditionalPropertyTransformHelper>().Object)
        {
        }
    }
}

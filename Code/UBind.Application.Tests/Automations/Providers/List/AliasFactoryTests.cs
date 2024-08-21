// <copyright file="AliasFactoryTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.List
{
    using System;
    using FluentAssertions;
    using UBind.Application.Automation.Providers.List;
    using UBind.Domain.Automation;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using Xunit;

    public class AliasFactoryTests
    {
        [Theory]
        [InlineData(typeof(Event), "event")]
        [InlineData(typeof(ClaimReadModelWithRelatedEntities), "claim")]
        [InlineData(typeof(ClaimVersionReadModelWithRelatedEntities), "claimVersion")]
        [InlineData(typeof(CustomerReadModelWithRelatedEntities), "customer")]
        [InlineData(typeof(DocumentReadModelWithRelatedEntities), "document")]
        [InlineData(typeof(EmailReadModelWithRelatedEntities), "email")]
        [InlineData(typeof(OrganisationReadModelWithRelatedEntities), "organisation")]
        [InlineData(typeof(PolicyReadModelWithRelatedEntities), "policy")]
        [InlineData(typeof(PolicyTransactionReadModelWithRelatedEntities), "policyTransaction")]
        [InlineData(typeof(QuoteReadModelWithRelatedEntities), "quote")]
        [InlineData(typeof(QuoteVersionReadModelWithRelatedEntities), "quoteVersion")]
        [InlineData(typeof(UserReadModelWithRelatedEntities), "user")]
        [InlineData(typeof(PortalWithRelatedEntities), "portal")]
        [InlineData(typeof(TenantWithRelatedEntities), "tenant")]
        [InlineData(typeof(ProductWithRelatedEntities), "product")]
        [InlineData(typeof(string), "item")]
        [InlineData(typeof(long), "item")]
        [InlineData(typeof(bool), "item")]
        public void Generate_ReturnsCorrectType_BasedOnElementType(Type elementType, string expectedAlias)
        {
            // Act
            var alias = AliasFactory.Generate(elementType);

            // Assert
            alias.Should().Be(expectedAlias);
        }
    }
}

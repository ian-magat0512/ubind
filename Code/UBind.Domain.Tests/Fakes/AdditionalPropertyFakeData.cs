// <copyright file="AdditionalPropertyFakeData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Fakes
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain;
    using UBind.Domain.Aggregates.AdditionalPropertyDefinition;
    using UBind.Domain.Enums;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Web.ResourceModels;

    public class AdditionalPropertyFakeData
    {
        public const string FakePropertyName = "Additional Ledger Id";
        public const string FakeTenantContext = "Tenant";
        public const string FakeEntity = "Quote";
        public const string FakeAlias = "additional-ledger-id";
        public const string FakeValue = "ACT_XXX";
        public const string FakeType = "text";
        public const string FakeProductId = "fake_product";
        public const string FakeProductContext = "Product";
        public const string FakeOrganisationContext = "Organisation";
        public const string FakeOrganisationAlias = "Fake-org-alias";
        public const string FakeOrganisationName = "Fake org name";
        public const string FakeDefaultValue = "Fake default value";

        public static Tenant CreateFakeTenant()
        {
            var tenant = TenantFactory.Create();
            return tenant;
        }

        public static AdditionalPropertyDefinitionReadModel CreateFakeAdditionalPropertyDefinitionReadModel(Guid id)
        {
            var testClock = new TestClock(false);

            var propertyDefinition = new AdditionalPropertyDefinitionReadModel(
                Guid.NewGuid(),
                id,
                testClock.GetCurrentInstant(),
                FakeAlias,
                FakePropertyName,
                AdditionalPropertyEntityType.Quote,
                AdditionalPropertyDefinitionContextType.Tenant,
                Guid.NewGuid(),
                true,
                true,
                false,
                AdditionalPropertyFakeData.FakeDefaultValue,
                AdditionalPropertyDefinitionType.Text,
                null);

            return propertyDefinition;
        }

        public static AdditionalPropertyDefinitionModel CreateFakeAdditionalPropertyModelOnCreate()
        {
            var modelOnCreate = new AdditionalPropertyDefinitionModel
            {
                Alias = FakeAlias,
                EntityType = AdditionalPropertyEntityType.Quote,
                Name = FakePropertyName,
                IsRequired = true,
                Type = AdditionalPropertyDefinitionType.Text,
                IsUnique = true,
                DefaultValue = FakeValue,
            };
            return modelOnCreate;
        }

        public static AdditionalPropertyDefinitionCreateOrUpdateModel CreateFakeAdditionalPropertyCreateModel()
        {
            var modelOnCreate = new AdditionalPropertyDefinitionCreateOrUpdateModel
            {
                Alias = FakeAlias,
                EntityType = AdditionalPropertyEntityType.Quote,
                Name = FakePropertyName,
                IsRequired = true,
                Type = AdditionalPropertyDefinitionType.Text,
                IsUnique = true,
                DefaultValue = FakeValue,
            };
            return modelOnCreate;
        }

        public static AdditionalPropertyDefinitionModel CreateFakeAdditionalPropertyModelOnUpdate()
        {
            var modelOnUpdate = new AdditionalPropertyDefinitionModel
            {
                Alias = FakeAlias,
                Name = FakePropertyName,
                IsRequired = true,
                IsUnique = true,
                DefaultValue = FakeValue,
            };
            return modelOnUpdate;
        }

        public static AdditionalPropertyDefinitionCreateOrUpdateModel CreateFakeAdditionalPropertyUpdateModel()
        {
            var modelOnUpdate = new AdditionalPropertyDefinitionCreateOrUpdateModel
            {
                Alias = FakeAlias,
                Name = FakePropertyName,
                IsRequired = true,
                IsUnique = true,
                DefaultValue = FakeValue,
            };
            return modelOnUpdate;
        }

        public static Product CreateFakeProduct()
        {
            var product = new Product(Guid.NewGuid(), Guid.NewGuid(), "Fake Product Name", "fake-product-alias", new TestClock(false).Timestamp);
            return product;
        }

        public static OrganisationReadModel CreateFakeOrganisation(Guid id)
        {
            var testClock = new TestClock(false);
            var organisation = new OrganisationReadModel(
                Guid.NewGuid(),
                id,
                FakePropertyName,
                FakeAlias,
                null, true,
                false,
                testClock.Timestamp);
            return organisation;
        }

        public static AdditionalPropertyDefinition CreateAdditionalPropertyDefinition()
        {
            var additionalPropertyDefinition = AdditionalPropertyDefinition
                .CreateForText(
                Guid.NewGuid(),
                FakeAlias,
                FakePropertyName,
                AdditionalPropertyEntityType.Quote,
                AdditionalPropertyDefinitionContextType.Tenant,
                true,
                true,
                Guid.NewGuid(),
                null,
                string.Empty,
                Guid.NewGuid(),
                new TestClock().Timestamp);
            return additionalPropertyDefinition;
        }

        public static AdditionalPropertyDefinition CreateAdditionalPropertyDefinitionWithId(Guid id)
        {
            var additionalPropertyDefinition = AdditionalPropertyDefinition
                .CreateForText(
                Guid.NewGuid(),
                FakeAlias,
                FakePropertyName,
                AdditionalPropertyEntityType.Quote,
                AdditionalPropertyDefinitionContextType.Tenant,
                true,
                true,
                Guid.NewGuid(),
                null,
                string.Empty,
                id,
                new TestClock().Timestamp);
            return additionalPropertyDefinition;
        }

        public static IList<AdditionalPropertyDefinitionReadModel> GenerateFakeList(
            AdditionalPropertyDefinitionContextType contextType)
        {
            var contextId = Guid.NewGuid();
            var fakeList = new List<AdditionalPropertyDefinitionReadModel>
            {
                new AdditionalPropertyDefinitionReadModel(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    new TestClock().Timestamp,
                    "alias-a",
                    "Name A",
                    AdditionalPropertyEntityType.Quote,
                    contextType,
                    contextId,
                    true,
                    true,
                    false,
                    "Default value a",
                    AdditionalPropertyDefinitionType.Text,
                    null),
                new AdditionalPropertyDefinitionReadModel(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    new TestClock().Timestamp,
                    "alias-b",
                    "Name B",
                    AdditionalPropertyEntityType.Quote,
                    contextType,
                    contextId,
                    true,
                    true,
                    false,
                    "Default value b",
                    AdditionalPropertyDefinitionType.Text,
                    null),
                new AdditionalPropertyDefinitionReadModel(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    new TestClock().Timestamp,
                    "alias-c",
                    "Name V",
                    AdditionalPropertyEntityType.Quote,
                    contextType,
                    contextId,
                    true,
                    true,
                    false,
                    "Default value c",
                    AdditionalPropertyDefinitionType.Text,
                    null),
            };
            return fakeList;
        }
    }
}

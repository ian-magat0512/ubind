// <copyright file="ObjectToJsonTextProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Text
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using MorseCode.ITask;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.Automation.Providers.Text;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.SerialisedEntitySchemaObject;
    using Xunit;

    public class ObjectToJsonTextProviderTests
    {
        [Fact]
        public async Task ObjectToJsonTextProvider_ShouldConvertSerializedEntityToText()
        {
            // Arrange
            var productEntity = new Product(Guid.NewGuid());
            productEntity.Alias = "my-product-alias";
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var contextEntityProviderMock = new Mock<IEntityProvider>();
            contextEntityProviderMock.Setup(s => s.Resolve(It.IsAny<ProviderContext>())).Returns(
                Task.FromResult(ProviderResult<Data<IEntity>>.Success(new Data<IEntity>(productEntity))).AsITask());
            var entityObjectProvider = new EntityObjectProvider(contextEntityProviderMock.Object);
            var objectToJsonTextProvider = new ObjectToJsonTextProvider(entityObjectProvider);

            // Act
            var objectToJsonText = await objectToJsonTextProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var result = JObject.Parse(objectToJsonText.GetValueOrThrowIfFailed().DataValue);
            result["alias"].ToString().Should().Be("my-product-alias");
        }
    }
}

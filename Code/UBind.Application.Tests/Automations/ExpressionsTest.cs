// <copyright file="ExpressionsTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using FluentAssertions;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Helper;
    using UBind.Application.Automation.PathLookup;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.List;
    using UBind.Domain.Exceptions;
    using UBind.Persistence.Extensions;
    using Xunit;

    public class ExpressionsTest
    {
        [Fact]
        public void ListOfObjects_CanBeFiltered_ByExpression()
        {
            // Make a list of pets
            var petsList = new List<object>()
            {
                new Pet() { Id = "1", Name = "Lucky", Breed = "Rottweiler" },
                new Pet() { Id = "2", Name = "Blacky", Breed = "Lucky" },
                new Pet() { Id = "3", Name = "Don", Inner = new Pet() { Id = "3.1", Name = "Lulu" } },
                "Kingsbury",
            };

            var valueToken = JToken.FromObject(petsList);

            // list is added into Variables
            // list retrieved via path
            // list will be used for iteration
            var query = new List<object>(valueToken);
            var dataQuery = new GenericDataList<object>(valueToken);

            Func<object, bool> preds = (object x) =>
            {
                var valueToEquate = "Lucky";
                var equalsToConstant = Expression.Constant(valueToEquate);
                IData wrappedData = null;
                if (DataObjectHelper.IsStructuredObjectOrArray(x))
                {
                    var path = "Name";
                    var pathPointer = new PocoJsonPointer(path, "SchemaSomething", new JObject());
                    var data = PocoPathLookupResolver.Resolve(x, path, "SchemaSomthing", new JObject()).GetValueOrThrowIfFailed();
                    wrappedData = ObjectWrapper.Wrap(data.GetValueFromGeneric());
                }
                else
                {
                    wrappedData = ObjectWrapper.Wrap(x as JToken);
                }

                var expressionConstant = Expression.Constant(wrappedData.GetValueFromGeneric());
                var expressionEquals = Expression.Equal(expressionConstant, equalsToConstant);
                var predicate = Expression.Lambda(expressionEquals).Compile();
                return predicate.Invoke() == true;
            };

            var possibleResult = dataQuery.Where(x => preds(x)).ToList();
            possibleResult.Should().HaveCount(1);
        }

        [Fact]
        public void CallPocoJsonPointer_ToRetrieve_PathValue()
        {
            // Make a list of pets
            var petsList = new List<object>()
            {
                new Pet() { Id = "1", Name = "Lucky", Breed = "Rottweiler" },
                new Pet() { Id = "2", Name = "Blacky", Breed = "Lucky" },
                new Pet() { Id = "3", Name = "Don", Inner = new Pet() { Id = "3.1", Name = "Lulu" } },
                "Kingsbury",
            };

            var valueToken = JToken.FromObject(petsList);

            // list is added into Variables
            // list retrieved via path
            // list will be used for iteration
            var query = new List<object>(valueToken);
            var dataQuery = new GenericDataList<object>(valueToken);

            var path = "/0/Name";
            var pocoPet = petsList.First();
            var pocoObjectExpr = Expression.Constant(valueToken);
            var pathExpr = Expression.Constant(path);
            var providerNameExpr = Expression.Constant("TestProvider");
            var debugContextExpr = Expression.Constant(new JObject());
            var methodInfo = typeof(PocoPathLookupResolver).GetMethod("Resolve");
            var staticCallExpr = Expression.Call(methodInfo, new Expression[] { pocoObjectExpr, pathExpr, providerNameExpr, debugContextExpr });
            var lambda = Expression.Lambda(staticCallExpr);

            var providerData = lambda.Compile().DynamicInvoke() as IProviderResult<IData>;
            var result = providerData?.GetValueOrThrowIfFailed();
            result.Should().NotBeNull();
            result.ToString().Should().Be("Lucky");
        }

        [Fact]
        public void ToTicksAtMidnight_CanConvertLocalDateExpression_ToTicksExpression()
        {
            // Arrange
            var localDate = new LocalDate(2022, 12, 1);
            var localDateExpression = Expression.Constant(localDate);
            var ticksValue = localDate.AtMidnight().InUtc().ToInstant().ToUnixTimeTicks();

            // Act
            var ticksExpression = localDateExpression.ToTicksAtMidnight();

            // Assert
            var ticksValueExpression = Expression.Constant(ticksValue);
            var equalityExpression = Expression.Equal(ticksExpression, ticksValueExpression);
            var lambdaExpression = Expression.Lambda(equalityExpression);

            lambdaExpression.Compile().DynamicInvoke().Should().Be(true);
        }

        [Fact]
        public void ToTicksAtMidnight_ShouldThrowException_WhenExpressionIsNotOfExpectedType()
        {
            // Arrange
            var localDate = new LocalDate(2022, 12, 1);
            var dateStringExpression = Expression.Constant(localDate.ToString());

            // Act
            var act = () => dateStringExpression.ToTicksAtMidnight();

            // Assert
            act.Should().Throw<ErrorException>();
        }

        internal class Pet
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public string Breed { get; set; }

            public Pet Inner { get; set; }
        }
    }
}

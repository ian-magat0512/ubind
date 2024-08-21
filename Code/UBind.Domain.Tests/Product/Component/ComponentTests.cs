// <copyright file="ComponentTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Product.Component
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using FluentAssertions;
    using UBind.Domain.Product.Component;
    using UBind.Domain.Product.Component.Form;
    using UBind.Domain.Validation;
    using Xunit;
    using DataType = UBind.Domain.Product.Component.Form.DataType;

    public class ComponentTests
    {
        [Fact]
        public void Component_DetectsInvalidProperties_WhenPropertiesAreInvalid()
        {
            // Arrange
            Component component = new Component()
            {
                Form = new Form
                {
                    QuestionSets = new List<QuestionSet>
                    {
                        new QuestionSet
                        {
                            Fields = new List<Field>
                            {
                                new DropDownSelectField
                                {
                                    DataType = DataType.Text,
                                },
                            },
                        },
                    },
                },
            };

            // Act
            IReadOnlyList<ValidationResult> validationResults = component.Validate();

            // Assert
            validationResults.Should().NotBeEmpty();
            var flatResults = validationResults.FlattenCompositeResults();
            flatResults.Should().HaveCount(4);
        }
    }
}

// <copyright file="AddColumnIfNotExistsOperationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.Migrations.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations.Builders;
    using System.Data.Entity.Migrations.Model;
    using FluentAssertions;
    using UBind.Persistence.Migrations.Extensions;
    using Xunit;

    public class AddColumnIfNotExistsOperationTests
    {
        [Fact]
        public void Can_get_and_set_table_and_column_info()
        {
            // Arrange
            Func<ColumnBuilder, ColumnModel> action = c => c.Decimal(name: "T");

            // Act
            var addColumnOperation = new AddColumnIfNotExistsOperation("T", "C", action, null);

            // Assert
            addColumnOperation.Table.Should().Be("T");
            addColumnOperation.Name.Should().Be("C");
        }

        [Fact]
        public void Inverse_should_produce_drop_column_operation()
        {
            // Arrange
            Func<ColumnBuilder, ColumnModel> action = c => c.Decimal(name: "C", annotations: new Dictionary<string, AnnotationValues> { { "A1", new AnnotationValues(null, "V1") } });
            var addColumnOperation = new AddColumnIfNotExistsOperation("T", "C", action, null);

            // Act
            var dropColumnOperation = (DropColumnOperation)addColumnOperation.Inverse;

            // Assert
            dropColumnOperation.Name.Should().Be("C");
            dropColumnOperation.Table.Should().Be("T");
            ((AnnotationValues)dropColumnOperation.RemovedAnnotations["A1"]).NewValue.Should().Be("V1");
            ((AnnotationValues)dropColumnOperation.RemovedAnnotations["A1"]).OldValue.Should().BeNull();
        }

        [Fact]
        public void Ctor_should_validate_preconditions_tableName()
        {
            // Arrange
            Func<ColumnBuilder, ColumnModel> action = c => c.Decimal(name: "T");

            // Act
            Action act = () => new AddColumnIfNotExistsOperation(null, "T", action, null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Ctor_should_validate_preconditions_columnName()
        {
            // Arrange
            Func<ColumnBuilder, ColumnModel> action = c => c.Decimal();

            // Act
            Action act = () => new AddColumnIfNotExistsOperation("T", null, action, null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Ctor_should_validate_preconditions_columnAction()
        {
            // Arrange
            // (nothing to arrange)

            // Act
            Action act = () => new AddColumnIfNotExistsOperation("T", "C", null, null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}

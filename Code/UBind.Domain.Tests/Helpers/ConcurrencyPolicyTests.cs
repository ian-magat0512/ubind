// <copyright file="ConcurrencyPolicyTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Helpers
{
    using System.Threading.Tasks;
    using FluentAssertions;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using Xunit;

    public class ConcurrencyPolicyTests
    {
        [Fact]
        public async Task ExecuteWithRetriesAsync_CorrectlyUpdatesVariableInEnclosingScope_WhenFunctionIsParameterlessLocalFunctionAndRetryActionSetsVariable()
        {
            // Arrange
            var entity = new DummyEntity(1);
            var attemptCount = 0;

            async Task DoSomething()
            {
                ++attemptCount;
                if (attemptCount < 3)
                {
                    throw new ConcurrencyException("Fake");
                }

                await Task.CompletedTask;
            }

            // Act
            await ConcurrencyPolicy.ExecuteWithRetriesAsync(DoSomething, () => { entity = new DummyEntity(entity.Version + 1); });

            // Assert
            entity.Version.Should().Be(3);
        }

        [Fact]
        public async Task ExecuteWithRetriesAsync_CorrectlyPassesUpdatedParameterToFunction_WhenFunctionIsSeparateMethodAndRetryActionSetsVariableUsedAsFunctionParameter()
        {
            // Arrange
            var entity = new DummyEntity(1);

            // Act
            await ConcurrencyPolicy.ExecuteWithRetriesAsync(() => this.DoSomethingWithEntity(entity), () => { entity = new DummyEntity(entity.Version + 1); });

            // Assert
            entity.Version.Should().Be(2);
        }

        private async Task DoSomethingWithEntity(DummyEntity entity)
        {
            if (entity.Version > 1)
            {
                await Task.CompletedTask;
                return;
            }

            throw new ConcurrencyException("Fake");
        }

        private class DummyEntity
        {
            public DummyEntity(int version)
            {
                this.Version = version;
            }

            public int Version { get; }
        }
    }
}

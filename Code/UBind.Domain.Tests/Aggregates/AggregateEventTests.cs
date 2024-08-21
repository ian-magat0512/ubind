// <copyright file="AggregateEventTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Aggregates
{
    using FluentAssertions;
    using Newtonsoft.Json;
    using Xunit;

    public class AggregateEventTests
    {
        [Fact(Skip = "Failing test illustrating problem with renaming types used in events.")]
        public void Deserialization_Succeeds_EvenAfterTypeNameChanges()
        {
            // Arrange
            var value = 7;
            var serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };

            var originalEvent = new OldEvent(new OldFoo(value));
            var serializedEvent = JsonConvert.SerializeObject(originalEvent, serializerSettings);

            // Act
            // This will fail since we are trying to deserialize to a new type, even
            // though the types are structurally compatible.
            var deserializedEvent = JsonConvert.DeserializeObject<NewEvent>(serializedEvent, serializerSettings);

            // Assert
            value.Should().Be(deserializedEvent.Foo.X);
        }
    }

    public class OldEvent
    {
        public OldEvent(OldFoo foo)
        {
            this.Foo = foo;
        }

        [JsonConstructor]
        private OldEvent()
        {
        }

        [JsonProperty]
        public OldFoo Foo { get; private set; }
    }

    public class NewEvent
    {
        public NewEvent(NewFoo foo)
        {
            this.Foo = foo;
        }

        [JsonConstructor]
        private NewEvent()
        {
        }

        [JsonProperty]
        public NewFoo Foo { get; private set; }
    }

    public class OldFoo
    {
        public OldFoo(int x)
        {
            this.X = x;
        }

        [JsonConstructor]
        private OldFoo()
        {
        }

        [JsonProperty]
        public int X { get; private set; }
    }

    public class NewFoo
    {
        public NewFoo(int x)
        {
            this.X = x;
        }

        [JsonConstructor]
        private NewFoo()
        {
        }

        [JsonProperty]
        public int X { get; private set; }
    }
}

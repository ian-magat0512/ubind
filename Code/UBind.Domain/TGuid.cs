// <copyright file="TGuid.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;

    /// <summary>
    /// Represents a typed Guid. This is needed so that we can have stricter type checking when using Guids
    /// as parameters on methods. We've had problems when people pass Guids in the wrong order, and so we
    /// need to handle that by having a different Guid type for each entity.
    /// </summary>
    public readonly struct TGuid<T> : IEquatable<TGuid<T>>
    {
        private readonly Guid value;

        public TGuid(Guid value)
        {
            this.value = value;
        }

        public Guid Value => this.value;

        public bool IsEmpty => this.value == Guid.Empty;

        public bool IsDefault => this.value == default;

        /// <summary>
        /// Automatically convert a typed Guid to a Guid.
        /// </summary>
        public static implicit operator Guid(TGuid<T> typedGuid) => typedGuid.value;

        /// <summary>
        /// Explicitly convert a Guid to a typed Guid.
        /// e.g. var typedGuid = (TGuid&gt;Quote&lt;)guid;
        /// The explicit cast operator allows you to keep type safety when converting a Guid to a specific TGuid&gt;T&lt;.
        /// It ensures that you consciously decide to perform the conversion and acknowledge the possibility of a mismatch.
        /// </summary>
        public static explicit operator TGuid<T>(Guid guid) => new TGuid<T>(guid);

        public static TGuid<T> NewGuid()
        {
            return new TGuid<T>(Guid.NewGuid());
        }

        public override string ToString() => this.value.ToString();

        public override int GetHashCode() => this.value.GetHashCode();

        public override bool Equals(object obj) =>
            obj is TGuid<T> other && this.value.Equals(other.value);

        public bool Equals(TGuid<T> other) => this.value.Equals(other.value);
    }
}

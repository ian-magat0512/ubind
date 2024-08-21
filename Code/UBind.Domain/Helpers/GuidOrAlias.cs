// <copyright file="GuidOrAlias.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Helpers
{
    using System;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Identifies a string parameter if its a guid or an alias.
    /// Usecase: this is used to determine if a tenantId parameter is a GUID ID or an Alias.
    /// </summary>
    public struct GuidOrAlias : IEquatable<GuidOrAlias>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GuidOrAlias"/> struct.
        /// </summary>
        /// <param name="id">An id that is nullable.</param>
        public GuidOrAlias(Guid? id)
        {
            this.Alias = null;
            this.Guid = id;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GuidOrAlias"/> struct.
        /// </summary>
        /// <param name="idOrAlias">A string that is either an id or an alias.</param>
        public GuidOrAlias(string idOrAlias)
        {
            this.Alias = null;
            this.Guid = null;
            _ = System.Guid.TryParse(idOrAlias, out Guid tempGuid);

            if (tempGuid != default)
            {
                this.Guid = tempGuid;
            }
            else
            {
                this.Alias = idOrAlias;
            }
        }

        /// <summary>
        /// Gets or sets the alias.
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the Guid.
        /// </summary>
        public Guid? Guid { get; set; }

        /// <summary>
        /// Check if Alias or Guid has value.
        /// </summary>
        /// <returns>true if either the alias or guid has value.</returns>
        public bool HasValue()
        {
            return !string.IsNullOrEmpty(this.Alias) || !this.Guid.IsNullOrEmpty();
        }

        public override bool Equals(object obj)
        {
            if (obj is GuidOrAlias other)
            {
                return this.Equals(other);
            }
            return false;
        }

        public bool Equals(GuidOrAlias other)
        {
            if (this.Guid.HasValue && other.Guid.HasValue)
            {
                return this.Guid.Value == other.Guid.Value;
            }
            else if (!string.IsNullOrEmpty(this.Alias) && !string.IsNullOrEmpty(other.Alias))
            {
                return this.Alias == other.Alias;
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                if (this.Guid.HasValue)
                {
                    hash = hash * 23 + this.Guid.Value.GetHashCode();
                }
                if (!string.IsNullOrEmpty(this.Alias))
                {
                    hash = hash * 23 + this.Alias.GetHashCode();
                }
                return hash;
            }
        }
    }
}

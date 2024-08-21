// <copyright file="JwtKey.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Models
{
    using NodaTime;

    /// <summary>
    /// Represents a JWT key, used for signing JWTs.
    /// These will be automatically rotated by the <see cref="JwtKeyRotator"/>.
    /// </summary>
    public class JwtKey : Entity<Guid>
    {
        public JwtKey(Guid id, string keyBase64, Instant timestamp)
            : base(id, timestamp)
        {
            this.KeyBase64 = keyBase64;
        }

        /// <summary>
        /// Parameterless constructor for Entity Framework.
        /// </summary>
        private JwtKey()
            : base(default, default)
        {
        }

        public string KeyBase64 { get; set; }

        public bool IsRotated { get; set; }

        public bool IsExpired { get; set; }
    }
}

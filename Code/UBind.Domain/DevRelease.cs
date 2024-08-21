// <copyright file="DevRelease.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using NodaTime;

    /// <summary>
    /// Product Development Release.
    /// </summary>
    public class DevRelease : ReleaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DevRelease"/> class.
        /// </summary>
        /// <param name="tenantId">The ID of tenant.</param>
        /// <param name="productId">The ID of product.</param>
        /// <param name="createdTimestamp"> The current time. .</param>
        public DevRelease(Guid tenantId, Guid productId, Instant createdTimestamp)
            : base(tenantId, productId, createdTimestamp)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DevRelease"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for EF.
        /// .</remarks>
        [Obsolete]
        protected DevRelease()
            : base()
        {
        }
    }
}

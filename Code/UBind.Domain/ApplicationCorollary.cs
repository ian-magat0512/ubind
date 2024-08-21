// <copyright file="ApplicationCorollary.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using NodaTime;

    /// <summary>
    /// Base class for things arising from applications.
    /// </summary>
    public class ApplicationCorollary : MutableEntity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationCorollary"/> class.
        /// </summary>
        /// <remarks>Parameterless constructor for EF.</remarks>
        public ApplicationCorollary()
            : base(default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationCorollary"/> class.
        /// </summary>
        /// <param name="submittedFormUpdateId">The ID of the form update that was used.</param>
        /// <param name="submittedCalculationResultId">The ID of the calculation result that was used.</param>
        /// <param name="createdTimestamp">The time the corollary was created.</param>
        public ApplicationCorollary(Guid submittedFormUpdateId, Guid submittedCalculationResultId, Instant createdTimestamp)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            this.SubmittedFormUpdateId = submittedFormUpdateId;
            this.SubmittedCalculationResultId = submittedCalculationResultId;
        }

        /// <summary>
        /// Gets the ID of the form update that was used for this corollary.
        /// </summary>
        public Guid SubmittedFormUpdateId { get; private set; }

        /// <summary>
        /// Gets the ID of the calculation result that was used for this corollary.
        /// </summary>
        public Guid SubmittedCalculationResultId { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this corollary included a calculation result.
        /// </summary>
        public bool HasCalculationResult => this.SubmittedCalculationResultId != default(Guid);
    }
}

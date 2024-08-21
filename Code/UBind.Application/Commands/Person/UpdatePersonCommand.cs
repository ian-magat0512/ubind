// <copyright file="UpdatePersonCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Person
{
    using System;
    using MediatR;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command for updating a person record.
    /// </summary>
    public class UpdatePersonCommand : ICommand<Unit>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatePersonCommand"/> class.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="personId">The ID of the person.</param>
        /// <param name="personDetails">The person details.</param>
        public UpdatePersonCommand(Guid tenantId, Guid personId, IPersonalDetails personDetails)
        {
            this.TenantId = tenantId;
            this.PersonId = personId;
            this.PersonDetails = personDetails;
        }

        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the ID of the person.
        /// </summary>
        public Guid PersonId { get; private set; }

        /// <summary>
        /// Gets the person details.
        /// </summary>
        public IPersonalDetails PersonDetails { get; private set; }
    }
}

// <copyright file="OrganisationDrop.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.SystemEmail
{
    using System;
    using global::DotLiquid;

    /// <summary>
    /// A drop model for organisation.
    /// </summary>
    public class OrganisationDrop : Drop
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrganisationDrop"/> class.
        /// </summary>
        /// <param name="id">The ID of the organisation.</param>
        /// <param name="alias">The alias of the organisation.</param>
        /// <param name="name">The name of the organisation to be displayed.</param>
        public OrganisationDrop(Guid id, string alias, string name)
        {
            this.Id = id;
            this.Name = name;
            this.Alias = alias;
        }

        /// <summary>
        /// Gets the Id of the organisation.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the Alias of the organisation.
        /// </summary>
        public string Alias { get; }

        /// <summary>
        /// Gets the name of the organisation.
        /// </summary>
        public string Name { get; }
    }
}

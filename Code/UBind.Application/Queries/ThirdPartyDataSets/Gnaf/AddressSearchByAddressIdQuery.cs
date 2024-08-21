// <copyright file="AddressSearchByAddressIdQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.ThirdPartyDataSets.Gnaf
{
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ThirdPartyDataSets.Gnaf;

    /// <summary>
    /// Represents the Query for obtaining the Gnaf address by address id.
    /// </summary>
    public class AddressSearchByAddressIdQuery : IQuery<MaterializedAddressView>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddressSearchByAddressIdQuery"/> class.
        /// </summary>
        /// <param name="id">The Gnaf address id.</param>
        public AddressSearchByAddressIdQuery(string id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Gets the address search id.
        /// </summary>
        public string Id { get; }
    }
}

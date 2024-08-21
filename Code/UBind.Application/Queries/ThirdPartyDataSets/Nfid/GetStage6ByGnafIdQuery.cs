// <copyright file="GetStage6ByGnafIdQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.ThirdPartyDataSets.Nfid
{
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ThirdPartyDataSets.Nfid;

    /// <summary>
    /// Command to obtain NFID details provided by GNAF address id.
    /// </summary>
    public class GetStage6ByGnafIdQuery : IQuery<Detail>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetStage6ByGnafIdQuery"/> class.
        /// </summary>
        /// <param name="gnafId">The GNAF address id.</param>
        public GetStage6ByGnafIdQuery(string gnafId)
        {
            this.GnafAddressId = gnafId;
        }

        /// <summary>
        /// Gets the GNAF address id.
        /// </summary>
        public string GnafAddressId { get; }
    }
}

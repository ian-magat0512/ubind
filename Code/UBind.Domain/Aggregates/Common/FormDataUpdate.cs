// <copyright file="FormDataUpdate.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Common
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;

    /// <summary>
    /// Stores a snapshot of the form data for an application.
    /// </summary>
    /// <typeparam name="TData">The type of the data being updated.</typeparam>
    public class FormDataUpdate<TData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormDataUpdate{TData}"/> class.
        /// </summary>
        /// <param name="id">An ID uniquely identifying this data update.</param>
        /// <param name="data">The updated data.</param>
        /// <param name="createdTimestamp">A timestamp for the update.</param>
        public FormDataUpdate(Guid id, TData data, Instant createdTimestamp)
        {
            this.Id = id;
            this.Data = data;
            this.CreatedTimestamp = createdTimestamp;
        }

        [JsonConstructor]
        private FormDataUpdate()
        {
        }

        /// <summary>
        /// Gets the unique ID for this form data event.
        /// </summary>
        [JsonProperty]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the form data as JSON.
        /// </summary>
        [JsonProperty]
        public TData Data { get; private set; }

        /// <summary>
        /// Gets the time the update was created.
        /// </summary>
        [JsonProperty]
        public Instant CreatedTimestamp { get; private set; }
    }
}

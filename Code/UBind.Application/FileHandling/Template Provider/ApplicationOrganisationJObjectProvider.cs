// <copyright file="ApplicationOrganisationJObjectProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling.Template_Provider
{
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using UBind.Domain;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Generates json about the organisation which the quote or customer belongs to.
    /// </summary>
    public class ApplicationOrganisationJObjectProvider : IJObjectProvider
    {
        private readonly IOrganisationReadModelSummary organisationSummary;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationOrganisationJObjectProvider"/> class.
        /// </summary>
        /// <param name="organisationSummary">The organisation read model summary.</param>
        public ApplicationOrganisationJObjectProvider(
            IOrganisationReadModelSummary organisationSummary)
        {
            this.organisationSummary = organisationSummary;
        }

        /// <inheritdoc/>
        public JObject JsonObject { get; private set; } = new JObject();

        /// <inheritdoc/>
        public Task CreateJsonObject(ApplicationEvent application)
        {
            if (this.organisationSummary != null)
            {
                dynamic jsonObject = new JObject();
                jsonObject.id = this.organisationSummary.Id;
                jsonObject.name = this.organisationSummary.Name;
                jsonObject.alias = this.organisationSummary.Alias;

                this.JsonObject = jsonObject;
            }

            return Task.CompletedTask;
        }
    }
}

// <copyright file="CreateNewBusinessQuoteCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Quote
{
    using System;
    using System.Collections.ObjectModel;
    using Newtonsoft.Json.Linq;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    public class CreateNewBusinessQuoteCommand : ICommand<NewQuoteReadModel>
    {
        public CreateNewBusinessQuoteCommand(
            Guid tenantId,
            Guid organisationId,
            Guid? portalId,
            Guid productId,
            DeploymentEnvironment environment,
            bool isTestData,
            Guid? customerId,
            Guid? ownerUserId,
            JObject? formData,
            string? initialQuoteState = null,
            ReadOnlyDictionary<string, object>? additionalProperties = null,
            string? productRelease = null)
        {
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
            this.PortalId = portalId;
            this.ProductId = productId;
            this.Environment = environment;
            this.OwnerUserId = ownerUserId;
            this.FormData = formData;
            this.IsTestData = isTestData;
            this.CustomerId = customerId;
            this.InitialQuoteState = initialQuoteState;
            this.AdditionalProperties = additionalProperties;
            this.ProductRelease = productRelease;
        }

        public Guid TenantId { get; }

        public Guid OrganisationId { get; }

        public Guid? PortalId { get; }

        public Guid ProductId { get; }

        public DeploymentEnvironment Environment { get; }

        public Guid? OwnerUserId { get; }

        /// <summary>
        /// Gets the initial form data to seed the new quote with.
        /// </summary>
        public JObject? FormData { get; }

        public Guid? CustomerId { get; }

        public bool IsTestData { get; }

        public string? InitialQuoteState { get; }

        public ReadOnlyDictionary<string, object>? AdditionalProperties { get; }

        public string? ProductRelease { get; }
    }
}

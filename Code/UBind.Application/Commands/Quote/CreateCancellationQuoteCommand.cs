// <copyright file="CreateCancellationQuoteCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Quote
{
    using System;
    using System.Collections.ObjectModel;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    [RetryOnDbException(5)]
    public class CreateCancellationQuoteCommand : ICommand<NewQuoteReadModel>
    {
        public CreateCancellationQuoteCommand(
            Guid tenantId,
            Guid policyId,
            bool discardExisting,
            FormData? formData = null,
            string? initialQuoteState = null,
            ReadOnlyDictionary<string, object>? additionalProperties = null,
            string? productRelease = null)
        {
            this.TenantId = tenantId;
            this.PolicyId = policyId;
            this.DiscardExisting = discardExisting;
            this.FormData = formData;
            this.InitialQuoteState = initialQuoteState;
            this.AdditionalProperties = additionalProperties;
            this.ProductRelease = productRelease;
        }

        public Guid TenantId { get; }

        public Guid PolicyId { get; }

        public bool DiscardExisting { get; }

        public FormData? FormData { get; set; }

        public string? InitialQuoteState { get; }

        public ReadOnlyDictionary<string, object>? AdditionalProperties { get; }

        public string? ProductRelease { get; }
    }
}

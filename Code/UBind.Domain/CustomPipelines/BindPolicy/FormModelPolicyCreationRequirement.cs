// <copyright file="FormModelPolicyCreationRequirement.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.CustomPipelines.BindPolicy
{
    using System;
    using NodaTime;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Json;
    using UBind.Domain.SerialisedEntitySchemaObject;

    /// <summary>
    /// This is the required parameters used for policy creation using a form model instead of a quote
    /// (an alternative for creating policy).
    /// </summary>
    public class FormModelPolicyCreationRequirement
    {
        public FormModelPolicyCreationRequirement(
            Guid organisationId,
            Customer? customer,
            string? externalPolicyNumber,
            FormData formData,
            CachingJObjectWrapper calculationResult,
            DateTimeZone timeZone,
            bool isTestData)
        {
            this.OrganisationId = organisationId;
            this.Customer = customer;
            this.ExternalPolicyNumber = externalPolicyNumber;
            this.FormData = formData;
            this.CalculationResult = calculationResult;
            this.IsTestData = isTestData;
            this.TimeZone = timeZone;
        }

        public Guid OrganisationId { get; }

        public Customer? Customer { get; }

        public string? ExternalPolicyNumber { get; }

        public FormData FormData { get; }

        public CachingJObjectWrapper CalculationResult { get; }

        public bool IsTestData { get; }

        public DateTimeZone TimeZone { get; }
    }
}

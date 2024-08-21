// <copyright file="AggregateJsonContext.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Json;

using System.Text.Json;
using System.Text.Json.Serialization;
using UBind.Domain;
using UBind.Domain.Aggregates.Accounting.Payment;
using UBind.Domain.Aggregates.Accounting.Refund;
using UBind.Domain.Aggregates.AdditionalPropertyDefinition;
using UBind.Domain.Aggregates.AdditionalPropertyValue;
using UBind.Domain.Aggregates.Claim;
using UBind.Domain.Aggregates.Customer;
using UBind.Domain.Aggregates.Person;
using UBind.Domain.Aggregates.Portal;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Aggregates.Quote.Entities;
using UBind.Domain.Aggregates.Report;
using UBind.Domain.Entities;
using UBind.Domain.Json;
using UBind.Domain.ReadWriteModel;
using OrganisationAggregate = Domain.Aggregates.Organisation.Organisation;

/// <summary>
/// The JSON context for serializing and deserializing domain aggregates and any related types.
/// This class is used by the JSON serializer to generate serialization code at runtime.
/// This more efficient than using reflection-based serialization.
/// See https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-source-generation
/// </summary>
[JsonSerializable(typeof(QuoteAggregate))]
[JsonSerializable(typeof(List<Quote>))]
[JsonSerializable(typeof(List<PolicyTransaction>))]
[JsonSerializable(typeof(NewBusinessQuote))]
[JsonSerializable(typeof(AdjustmentQuote))]
[JsonSerializable(typeof(RenewalQuote))]
[JsonSerializable(typeof(CancellationQuote))]
[JsonSerializable(typeof(PolicyTransaction))]
[JsonSerializable(typeof(NewBusinessPolicyTransaction))]
[JsonSerializable(typeof(AdjustmentPolicyTransaction))]
[JsonSerializable(typeof(RenewalPolicyTransaction))]
[JsonSerializable(typeof(CancellationPolicyTransaction))]
[JsonSerializable(typeof(PersonAggregate))]
[JsonSerializable(typeof(CustomerAggregate))]
[JsonSerializable(typeof(ClaimAggregate))]
[JsonSerializable(typeof(OrganisationAggregate))]
[JsonSerializable(typeof(ReportAggregate))]
[JsonSerializable(typeof(TextAdditionalPropertyValue))]
[JsonSerializable(typeof(AdditionalPropertyDefinition))]
[JsonSerializable(typeof(PaymentAggregate))]
[JsonSerializable(typeof(RefundAggregate))]
[JsonSerializable(typeof(PortalAggregate))]
[JsonSerializable(typeof(StructuredDataAdditionalPropertyValue))]
[JsonSerializable(typeof(QuoteDataUpdate<FormData>))]
[JsonSerializable(typeof(FormData))]
[JsonSerializable(typeof(CachingJObjectWrapper))]
[JsonSerializable(typeof(QuoteDataUpdate<CalculationResult>))]
[JsonSerializable(typeof(CalculationResult))]
[JsonSerializable(typeof(FixedAndScalablePrice))]
[JsonSerializable(typeof(QuoteStateChange))]
[JsonSerializable(typeof(Invoice))]
[JsonSerializable(typeof(Domain.Aggregates.Quote.PaymentAttemptResult))]
[JsonSerializable(typeof(Policy))]
[JsonSerializable(typeof(List<QuoteVersion>))]
[JsonSerializable(typeof(QuoteVersion))]
[JsonSerializable(typeof(QuoteDataUpdate<PersonalDetails>))]
[JsonSerializable(typeof(PersonalDetails))]
[JsonSerializable(typeof(List<QuoteFileAttachment>))]
[JsonSerializable(typeof(QuoteFileAttachment))]
[JsonSerializable(typeof(FundingProposal))]
[JsonSerializable(typeof(IDictionary<string, QuoteDocument>))]
[JsonSerializable(typeof(AdditionalPropertyValue))]
[JsonSourceGenerationOptions(
    WriteIndented = false,
    IncludeFields = true,
    GenerationMode = JsonSourceGenerationMode.Metadata | JsonSourceGenerationMode.Serialization,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNameCaseInsensitive = false,
    IgnoreReadOnlyFields = false,
    IgnoreReadOnlyProperties = false)]
public partial class AggregateJsonContext : JsonSerializerContext
{
    public static JsonSerializerOptions SourceGenerationOptions
    {
        get
        {
            return new JsonSerializerOptions(Default.Options)
            {
                TypeInfoResolver = Default,
            };
        }
    }
}

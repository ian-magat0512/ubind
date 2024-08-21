// <copyright file="PolicyTransactionConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.JsonConverters;

using System.Text.Json;
using System.Text.Json.Serialization;
using UBind.Domain;
using UBind.Domain.Aggregates.Quote.Entities;

/// <summary>
/// This custom converter is to handle the deserialization of the PolicyTransaction object.
/// Since we have multiple types of PolicyTransaction objects, we need to determine the type of the object during deserialization.
/// System.Text.Json does not support polymorphic deserialization out of the box.
/// Also, we need to handle the old data that was not migrated to the new structure.
/// During serialization we need to skip writing the old policy transaction data.
/// </summary>
public class PolicyTransactionConverter : JsonConverter<PolicyTransaction>
{
    public override PolicyTransaction Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (var doc = JsonDocument.ParseValue(ref reader))
        {
            JsonElement element = doc.RootElement;
            var type = element.GetProperty(nameof(PolicyTransaction.Type).ToLower()).GetInt32();
            TransactionType transactionType = Enum.Parse<TransactionType>(type.ToString());

            PolicyTransaction transaction = transactionType switch
            {
                TransactionType.NewBusiness => JsonSerializer.Deserialize<NewBusinessPolicyTransaction>(element.GetRawText(), options) ?? throw new JsonException("Cannot deserialize NewBusinessPolicyTransaction."),
                TransactionType.Adjustment => JsonSerializer.Deserialize<AdjustmentPolicyTransaction>(element.GetRawText(), options) ?? throw new JsonException("Cannot deserialize AdjustmentPolicyTransaction."),
                TransactionType.Renewal => JsonSerializer.Deserialize<RenewalPolicyTransaction>(element.GetRawText(), options) ?? throw new JsonException("Cannot deserialize RenewalPolicyTransaction."),
                TransactionType.Cancellation => JsonSerializer.Deserialize<CancellationPolicyTransaction>(element.GetRawText(), options) ?? throw new JsonException("Cannot deserialize CancellationPolicyTransaction."),
                _ => throw new JsonException($"Unknown policy transaction type: {type}")
            };

            return transaction;
        }
    }

    public override void Write(Utf8JsonWriter writer, PolicyTransaction value, JsonSerializerOptions options)
    {
        // Skip writing the value if it is a PolicyTransactionOld instance.
        // This was for old data that was not migrated to the new structure.
        // We don't want to write this data back to the database.
        // We will remove this check once all the data is migrated.
        // This is a temporary solution for old records during migration.
        if (value is PolicyTransactionOld)
        {
            return;
        }

        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}

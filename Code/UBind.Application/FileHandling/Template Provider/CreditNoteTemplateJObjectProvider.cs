﻿// <copyright file="CreditNoteTemplateJObjectProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling.Template_Provider
{
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using NodaTime.Text;
    using UBind.Domain;

    /// <summary>
    /// Credit Note template JSON object provider.
    /// </summary>
    public class CreditNoteTemplateJObjectProvider : IJObjectProvider
    {
        /// <inheritdoc/>
        public JObject JsonObject { get; private set; } = new JObject();

        /// <inheritdoc/>
        public Task CreateJsonObject(ApplicationEvent applicationEvent)
        {
            var quote = applicationEvent.Aggregate.GetQuoteOrThrow(applicationEvent.QuoteId);
            if (!quote.CreditNoteIssued)
            {
                return Task.CompletedTask;
            }

            dynamic jsonObject = new JObject();
            var datePattern = ZonedDateTimePattern.CreateWithInvariantCulture("dd MMM yyyy", DateTimeZoneProviders.Tzdb);
            var dateTimePattern = ZonedDateTimePattern.CreateWithInvariantCulture("dd MMM yyyy hh:mmtt x", DateTimeZoneProviders.Tzdb);
            var zonedDateTime = quote.CreditNote.CreatedTimestamp.InZone(Timezones.AET);

            jsonObject.CreditNoteNumber = quote.CreditNote.CreditNoteNumber;
            jsonObject.CreditNoteDate = datePattern.Format(zonedDateTime);
            jsonObject.CreditNoteCreationDate = jsonObject.CreditNoteDate;
            jsonObject.CreditNoteCreatedTimestamp = dateTimePattern.Format(zonedDateTime);

            IJsonObjectParser parser = new GenericJObjectParser(string.Empty, jsonObject);
            if (parser.JsonObject != null)
            {
                this.JsonObject = parser.JsonObject;
            }

            return Task.CompletedTask;
        }
    }
}

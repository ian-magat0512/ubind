// <copyright file="DefaultClaimDataMapper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Claim
{
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Domain.Aggregates.Common;
    using UBind.Domain.Extensions;
    using UBind.Domain.Services;

    /// <summary>
    /// The default claim data mapper.
    /// </summary>
    public class DefaultClaimDataMapper : IClaimDataMapper
    {
        private readonly IPolicyTransactionTimeOfDayScheme timeOfDayScheme;

        private IDatumLocator amountDataLocator =
            new DatumLocator(DatumLocationObject.FormData, "claimAmount");

        private IDatumLocator descriptionLocator =
            new DatumLocator(DatumLocationObject.FormData, "description");

        private IDatumLocator incidentDateLocator =
            new DatumLocator(DatumLocationObject.FormData, "incidentDate");

        public DefaultClaimDataMapper(IPolicyTransactionTimeOfDayScheme timeOfDayScheme)
        {
            this.timeOfDayScheme = timeOfDayScheme;
        }

        /// <inheritdoc/>
        public IClaimData ExtractClaimData(string formDataJson, string calculationJson, DateTimeZone timeZone)
        {
            var formDataObject = JObject.Parse(formDataJson);
            var calculationResultObject = JObject.Parse(calculationJson);
            var amount = this.amountDataLocator.Invoke<decimal?>(formDataObject, calculationResultObject);
            var description = this.descriptionLocator.Invoke<string>(formDataObject, calculationResultObject);
            var incidentDateString = this.incidentDateLocator.Invoke<string>(formDataObject, calculationResultObject);
            var parseResult = incidentDateString.TryParseAsLocalDate();
            var incidentDate = parseResult.IsSuccess ? parseResult.Value : (LocalDate?)null;
            Instant? incidentTimestamp = incidentDate.HasValue
                ? incidentDate.Value.At(this.timeOfDayScheme.GetInceptionTime()).InZoneLeniently(timeZone).ToInstant()
                : (Instant?)null;
            return new ClaimData(amount, description, incidentTimestamp, timeZone);
        }

        /// <summary>
        /// Updates the claim data with the relevant values from the formdata json.
        /// </summary>
        /// <param name="claimData">The current claim data.</param>
        /// <param name="formDataJson">The form data json.</param>
        /// <returns>The updated claim data structure.</returns>
        public IClaimData UpdateClaimData(ClaimData claimData, string formDataJson)
        {
            var formDataObject = JObject.Parse(formDataJson);
            var calculationResultObject = new JObject();
            if (this.amountDataLocator.Object == DatumLocationObject.FormData)
            {
                claimData.Amount = this.amountDataLocator.Invoke<decimal?>(formDataObject, calculationResultObject);
            }

            if (this.descriptionLocator.Object == DatumLocationObject.FormData)
            {
                claimData.Description = this.descriptionLocator.Invoke<string>(formDataObject, calculationResultObject);
            }

            if (this.incidentDateLocator.Object == DatumLocationObject.FormData)
            {
                var incidentDateString = this.incidentDateLocator.Invoke<string>(formDataObject, calculationResultObject);
                var parseResult = incidentDateString.TryParseAsLocalDate();
                var incidentDate = parseResult.IsSuccess ? parseResult.Value : (LocalDate?)null;
                Instant? incidentTimestamp = incidentDate.HasValue
                    ? incidentDate.Value.At(this.timeOfDayScheme.GetInceptionTime()).InZoneLeniently(claimData.TimeZone).ToInstant()
                    : (Instant?)null;
                claimData.IncidentTimestamp = incidentTimestamp;
            }

            return claimData;
        }

        /// <inheritdoc/>
        public JObject SyncFormdata(JObject formModel, IClaimData claimData)
        {
            var newFormModel = formModel.DeepClone() as JObject;
            if (claimData.Amount.HasValue)
            {
                var value = new JValue(claimData.Amount.Value.ToString());
                this.amountDataLocator.UpdateFormModel(newFormModel, value);
            }

            if (claimData.Description != null)
            {
                var value = new JValue(claimData.Description);
                this.descriptionLocator.UpdateFormModel(newFormModel, value);
            }

            if (claimData.IncidentTimestamp.HasValue)
            {
                LocalDate incidentDate = claimData.IncidentTimestamp.Value.InZone(claimData.TimeZone).Date;
                var value = new JValue(incidentDate.ToMMDDYYYWithSlashes());
                this.incidentDateLocator.UpdateFormModel(newFormModel, value);
            }

            return newFormModel;
        }
    }
}

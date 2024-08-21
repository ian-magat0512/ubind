// <copyright file="CustomerDetailsJObjectProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling.Template_Provider
{
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Person;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Helpers;

    /// <summary>
    /// CustomerDetail template JSON object provider.
    /// </summary>
    public class CustomerDetailsJObjectProvider : IJObjectProvider
    {
        private IPersonService personService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerDetailsJObjectProvider"/> class.
        /// </summary>
        /// <param name="personService">the person service.</param>
        public CustomerDetailsJObjectProvider(IPersonService personService)
        {
            this.personService = personService;
        }

        /// <inheritdoc/>
        public JObject JsonObject { get; private set; } = new JObject();

        /// <inheritdoc/>
        public Task CreateJsonObject(ApplicationEvent applicationEvent)
        {
            var quote = applicationEvent.Aggregate.GetQuoteOrThrow(applicationEvent.QuoteId);
            var quoteCustomer = quote?.LatestCustomerDetails?.Data;
            dynamic jsonObject = new JObject();

            if (applicationEvent.Aggregate.HasCustomer)
            {
                jsonObject.CustomerId = applicationEvent.Aggregate.CustomerId.Value.ToString("D");
                var personAggregate = this.personService.GetByCustomerId(applicationEvent.Aggregate.TenantId, applicationEvent.Aggregate.CustomerId.Value);
                IPersonalDetails personDetails = personAggregate ?? quoteCustomer;
                jsonObject.CustomerEnvironment = applicationEvent.Aggregate.Environment;
                jsonObject.CustomerFullName = PersonPropertyHelper.GetFullNameFromParts(
                        personDetails.PreferredName,
                        personDetails.NamePrefix,
                        personDetails.FirstName,
                        personDetails.LastName,
                        personDetails.NameSuffix,
                        personDetails.MiddleNames);
                jsonObject.CustomerNamePrefix = personDetails.NamePrefix ?? string.Empty;
                jsonObject.CustomerFirstName = personDetails.FirstName ?? string.Empty;
                jsonObject.CustomerMiddleNames = personDetails.MiddleNames ?? string.Empty;
                jsonObject.CustomerLastName = personDetails.LastName ?? string.Empty;
                jsonObject.CustomerNameSuffix = personDetails.NameSuffix ?? string.Empty;
                jsonObject.CustomerPreferredName = personDetails.PreferredName ?? string.Empty;
                jsonObject.CustomerEmail = personDetails.Email ?? string.Empty;
                jsonObject.CustomerAlternativeEmail = personDetails.AlternativeEmail ?? string.Empty;
                jsonObject.CustomerMobilePhone = personDetails.MobilePhone ?? string.Empty;
                jsonObject.CustomerHomePhone = personDetails.HomePhone ?? string.Empty;
                jsonObject.CustomerWorkPhone = personDetails.WorkPhone ?? string.Empty;
                jsonObject.Title = personDetails.Title ?? string.Empty;
                jsonObject.Company = personDetails.Company ?? string.Empty;
            }
            else if (quoteCustomer != null)
            {
                jsonObject.CustomerTenantId = applicationEvent.Aggregate.TenantId;
                jsonObject.CustomerEnvironment = applicationEvent.Aggregate.Environment;
                jsonObject.Fullname = quoteCustomer.FullName;
                jsonObject.CustomerFullName = PersonPropertyHelper.GetFullNameFromParts(
                        quoteCustomer.PreferredName,
                        quoteCustomer.NamePrefix,
                        quoteCustomer.FirstName,
                        quoteCustomer.LastName,
                        quoteCustomer.NameSuffix,
                        quoteCustomer.MiddleNames);
                jsonObject.CustomerNamePrefix = quoteCustomer.NamePrefix ?? string.Empty;
                jsonObject.CustomerFirstName = quoteCustomer.FirstName ?? string.Empty;
                jsonObject.CustomerMiddleNames = quoteCustomer.MiddleNames ?? string.Empty;
                jsonObject.CustomerLastName = quoteCustomer.LastName ?? string.Empty;
                jsonObject.CustomerNameSuffix = quoteCustomer.NameSuffix ?? string.Empty;
                jsonObject.CustomerPreferredName = quoteCustomer.PreferredName ?? string.Empty;
                jsonObject.CustomerEmail = quoteCustomer.Email ?? string.Empty;
                jsonObject.CustomerAlternativeEmail = quoteCustomer.AlternativeEmail ?? string.Empty;
                jsonObject.CustomerMobilePhone = quoteCustomer.MobilePhone ?? string.Empty;
                jsonObject.CustomerHomePhone = quoteCustomer.HomePhone ?? string.Empty;
                jsonObject.CustomerWorkPhone = quoteCustomer.WorkPhone ?? string.Empty;
                jsonObject.Title = quoteCustomer.Title ?? string.Empty;
                jsonObject.Company = quoteCustomer.Company ?? string.Empty;
            }

            IJsonObjectParser parser
                = new GenericJObjectParser(string.Empty, jsonObject);

            if (parser.JsonObject != null)
            {
                this.JsonObject = parser.JsonObject;
            }

            return Task.CompletedTask;
        }
    }
}

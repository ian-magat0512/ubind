// <copyright file="ApplicationEventViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export.ViewModels
{
    using System;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;
    using NodaTime;
    using UBind.Application.FileHandling.Template_Provider;
    using UBind.Application.Person;
    using UBind.Application.Queries.Customer;
    using UBind.Application.Queries.Portal;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Configuration;
    using UBind.Domain.Dto;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Customer;

    /// <summary>
    /// View model for Razor Templates to use.
    /// </summary>
    public class ApplicationEventViewModel
    {
        private readonly ApplicationEvent applicationEvent;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationEventViewModel"/> class.
        /// </summary>
        /// <param name="applicationEvent">The application event to present.</param>
        /// <param name="configuration">The email configuration.</param>
        /// <param name="configurationService">The release query service.</param>
        /// <param name="displayableField">the displayable fields object.</param>
        /// <param name="tenant">The tenant Service.</param>
        /// <param name="product">The product Service.</param>
        /// <param name="organisationSummary">The organisation read model summary.</param>
        private ApplicationEventViewModel(
            ApplicationEvent applicationEvent,
            IEmailInvitationConfiguration configuration,
            IConfigurationService configurationService,
            DisplayableFieldDto displayableField,
            Tenant tenant,
            Product product,
            IOrganisationReadModelSummary organisationSummary,
            IClock clock,
            string portalUrl,
            OrganisationTemplateJObjectProvider organisation,
            ProductTemplateJObjectProvider productProvider)
        {
            this.applicationEvent = applicationEvent;
            this.ApplicationUrl = UrlFormatting.GetApplicationUrl(configuration.InvitationLinkHost);
            this.AssetsUrl = UrlFormatting.GetAssetsUrl(
                configuration.InvitationLinkHost,
                tenant.Details.Alias,
                product.Details.Alias,
                applicationEvent.Aggregate.Environment,
                WebFormAppType.Quote,
                applicationEvent.ProductReleaseId);
            this.TimeZoneAlias = "AET";
            var quote = applicationEvent.Aggregate.GetQuoteOrThrow(applicationEvent.QuoteId);
            var jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            });

            var organisationObj = JObject.FromObject(new OrganisationViewModel(organisation.JsonObject), jsonSerializer);
            this.Organisation = new JsonViewModel(organisationObj);
            var applicationOrganisation = new ApplicationOrganisationJObjectProvider(organisationSummary);
            applicationOrganisation.CreateJsonObject(applicationEvent);
            this.ApplicationOrganisation = new JsonViewModel(applicationOrganisation.JsonObject);
            this.PortalUrl = portalUrl;
            this.Tenant = new JsonViewModel(JObject.FromObject(new TenantViewModel(tenant), jsonSerializer));
            this.Product = new JsonViewModel(JObject.FromObject(new ProductViewModel(productProvider.JsonObject), jsonSerializer));
            var calculationJson = quote.LatestCalculationResult?.Data?.Json;
            var calculationJObject = calculationJson != null
                ? JObject.Parse(calculationJson)
                : new JObject();

            if (quote.LatestCalculationResult != null)
            {
                var calculationResultData = quote.LatestCalculationResult.Data;
                calculationJObject["payment"]["payableComponents"] = PriceBreakdownViewModelHelper.GenerateViewModelAsJObject(calculationResultData.PayablePrice);
                calculationJObject["payment"]["refundComponents"] = PriceBreakdownViewModelHelper.GenerateViewModelAsJObject(calculationResultData.RefundBreakdown);
            }

            this.Calculation = new JsonViewModel(calculationJObject, displayableField);
        }

        /// <summary>
        /// Gets the type of the event.
        /// </summary>
        public ApplicationEventType EventType => this.applicationEvent.EventType;

        /// <summary>
        /// Gets the system application's base url.
        /// </summary>
        public string ApplicationUrl { get; private set; }

        /// <summary>
        /// Gets the system application's portal url.
        /// </summary>
        public string PortalUrl { get; private set; }

        /// <summary>
        /// Gets the system application's portal url.
        /// </summary>
        public string CustomerPortalUrl { get; private set; }

        /// <summary>
        /// Gets the system application's assets url.
        /// </summary>
        public string AssetsUrl { get; private set; }

        /// <summary>
        /// Gets the timezone alias.
        /// </summary>
        public string TimeZoneAlias { get; private set; }

        /// <summary>
        /// Gets the aggregate Id.
        /// </summary>
        public string QuoteId
        {
            get
            {
                var quote = this.applicationEvent.Aggregate.GetQuoteOrThrow(this.applicationEvent.QuoteId);
                var quoteId = quote != null ?
                    quote.Id : this.applicationEvent.Aggregate.Id;

                return quoteId.ToString("D");
            }
        }

        /// <summary>
        /// Gets the environment the application belongs to.
        /// </summary>
        public DeploymentEnvironment Environment => this.applicationEvent.Aggregate.Environment;

        /// <summary>
        ///  Gets a view model presenting organisation.
        /// </summary>
        public JsonViewModel Organisation { get; private set; } = new JsonViewModel(new JObject());

        /// <summary>
        ///  Gets a view model presenting the ubind organisation.
        /// </summary>
        public JsonViewModel ApplicationOrganisation { get; private set; } = new JsonViewModel(new JObject());

        /// <summary>
        ///  Gets a view model presenting form data.
        /// </summary>
        public JsonViewModel Form { get; private set; } = new JsonViewModel(new JObject());

        /// <summary>
        /// Gets a view model presenting the quote, if it exists.
        /// </summary>
        public JsonViewModel Quote { get; private set; } = new JsonViewModel(new JObject());

        /// <summary>
        /// Gets the view model presenting calculation.
        /// </summary>
        public JsonViewModel Calculation { get; private set; } = new JsonViewModel(new JObject());

        /// <summary>
        /// Gets a view model presenting the policy, if it exists.
        /// </summary>
        public JsonViewModel Policy { get; private set; } = new JsonViewModel(new JObject());

        /// <summary>
        /// Gets a view model presenting the invoice, if it exists.
        /// </summary>
        public JsonViewModel Invoice { get; private set; } = new JsonViewModel(new JObject());

        /// <summary>
        /// Gets a view model presenting the credit note, if it exists.
        /// </summary>
        public JsonViewModel CreditNote { get; private set; } = new JsonViewModel(new JObject());

        /// <summary>
        /// Gets a view model presenting the tenant for the quote.
        /// </summary>
        public JsonViewModel Tenant { get; private set; } = new JsonViewModel(new JObject());

        /// <summary>
        /// Gets a view model presenting the product for the quote.
        /// </summary>
        public JsonViewModel Product { get; private set; } = new JsonViewModel(new JObject());

        /// <summary>
        /// Gets a view model representing details of a successful the payment, if it exists.
        /// </summary>
        public PaymentDetailsViewModel Payment { get; private set; }

        /// <summary>
        /// Gets a view model representing details of an accepted funding proposal, if it exists.
        /// </summary>
        public FundingDetailsViewModel Funding { get; private set; }

        /// <summary>
        /// Gets a view model presenting the customer, if it exists.
        /// </summary>
        public JsonViewModel Customer { get; private set; } = new JsonViewModel(new JObject());

        public static async Task<ApplicationEventViewModel> Create(
            IFormDataPrettifier formDataPrettifier,
            ApplicationEvent applicationEvent,
            IEmailInvitationConfiguration configuration,
            IConfigurationService configurationService,
            IProductConfiguration productConfiguration,
            DisplayableFieldDto displayableField,
            IPersonService personService,
            Tenant tenant,
            Product product,
            IOrganisationReadModelSummary organisationSummary,
            IClock clock,
            ICqrsMediator mediator)
        {
            Guid? portalId = null;
            if (applicationEvent.Aggregate.HasCustomer)
            {
                var customer = await mediator.Send(
                    new GetCustomerByIdQuery(tenant.Id, applicationEvent.Aggregate.CustomerId.Value));
                portalId = customer.PortalId;
            }

            portalId = portalId ?? await mediator.Send(new GetDefaultPortalIdQuery(
                tenant.Id, organisationSummary.Id, PortalUserType.Customer));
            string portalUrl = await mediator.Send(
                new GetPortalUrlQuery(tenant.Id, organisationSummary.Id, portalId, applicationEvent.Aggregate.Environment));
            var organisationProvider = new OrganisationTemplateJObjectProvider(configurationService);
            await organisationProvider.CreateJsonObject(applicationEvent);
            var productProvider = new ProductTemplateJObjectProvider(configurationService);
            await productProvider.CreateJsonObject(applicationEvent);
            var viewModel = new ApplicationEventViewModel(
                applicationEvent,
                configuration,
                configurationService,
                displayableField,
                tenant,
                product,
                organisationSummary,
                clock,
                portalUrl,
                organisationProvider,
                productProvider);
            await viewModel.Init(
                productConfiguration,
                formDataPrettifier,
                displayableField,
                tenant,
                mediator,
                personService,
                product,
                clock);
            return viewModel;
        }

        private async Task Init(
            IProductConfiguration productConfiguration,
            IFormDataPrettifier formDataPrettifier,
            DisplayableFieldDto displayableField,
            Tenant tenant,
            ICqrsMediator mediator,
            IPersonService personService,
            Product product,
            IClock clock)
        {
            var quote = this.applicationEvent.Aggregate.GetQuoteOrThrow(this.applicationEvent.QuoteId);
            var jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            });
            JObject formModelJson = quote.LatestFormData?.Data?.FormModel ?? new JObject();
            IFormDataSchema formDataSchema = productConfiguration.FormDataSchema;

            if (this.applicationEvent.Aggregate != null)
            {
                var quoteObj = JObject.FromObject(new QuoteViewModel(this.applicationEvent.Aggregate, product, quote.Id), jsonSerializer);
                this.Quote = new JsonViewModel(quoteObj);

                if (quote.TransactionCompleted)
                {
                    var obj = JObject.FromObject(
                        new PolicyViewModel(this.applicationEvent.Aggregate.Policy, clock), jsonSerializer);
                    this.Policy = new JsonViewModel(obj);
                }

                if (quote.InvoiceIssued)
                {
                    var obj = JObject.FromObject(
                        new InvoiceViewModel(quote.Invoice), jsonSerializer);
                    this.Invoice = new JsonViewModel(obj);
                }

                if (quote.CreditNoteIssued)
                {
                    var obj = JObject.FromObject(
                        new CreditNoteViewModel(quote.CreditNote), jsonSerializer);
                    this.CreditNote = new JsonViewModel(obj);
                }

                if (this.applicationEvent.Aggregate.HasCustomer)
                {
                    CustomerReadModelDetail customer = await mediator.Send(
                        new GetCustomerByIdQuery(quote.Aggregate.TenantId, quote.CustomerId.Value));

                    var portalId = customer.PortalId ?? await mediator.Send(new GetDefaultPortalIdQuery(
                        tenant.Id, customer.OrganisationId, PortalUserType.Customer));
                    this.CustomerPortalUrl = await mediator.Send(
                        new GetPortalUrlQuery(customer.TenantId, null, portalId, quote.Aggregate.Environment));
                    var personAggregate = personService.GetByCustomerId(this.applicationEvent.Aggregate.TenantId, this.applicationEvent.Aggregate.CustomerId.Value);
                    IPersonalDetails personDetails = personAggregate ?? quote.LatestCustomerDetails?.Data;

                    var customerViewModel = new CustomerViewModel(
                        this.applicationEvent.Aggregate.CustomerId.Value,
                        personDetails,
                        this.applicationEvent.Aggregate.Environment)
                    {
                        FullName = PersonPropertyHelper.GetFullNameFromParts(
                            customer.PreferredName,
                            customer.NamePrefix,
                            customer.FirstName,
                            customer.LastName,
                            customer.NameSuffix,
                            customer.MiddleNames),
                    };

                    var obj = JObject.FromObject(customerViewModel, jsonSerializer);
                    this.Customer = new JsonViewModel(obj);

                    if (formModelJson["fullName"] == null)
                    {
                        formModelJson.Add(new JProperty("fullName", personDetails.FullName));
                    }
                }
                else if (this.applicationEvent.Aggregate.HasCustomer && quote.LatestCustomerDetails?.Data != null)
                {
                    var obj = JObject.FromObject(
                        new CustomerViewModel(
                            this.applicationEvent.Aggregate.CustomerId.Value,
                            quote.LatestCustomerDetails.Data,
                            this.applicationEvent.Aggregate.Environment), jsonSerializer);
                    this.Customer = new JsonViewModel(obj);

                    if (formModelJson["fullName"] == null)
                    {
                        formModelJson.Add(new JProperty("fullName", quote.LatestCustomerDetails.Data.FullName));
                    }
                }
            }

            this.Form = new JsonViewModel(
                formDataPrettifier,
                formModelJson,
                displayableField,
                formDataSchema,
                this.applicationEvent.Aggregate.Id,
                "Quote",
                new ProductContext(tenant.Id, product.Id, this.applicationEvent.Aggregate.Environment));

            if (quote.IsPaidFor)
            {
                this.Payment = new PaymentDetailsViewModel(quote.LatestPaymentAttemptResult);
            }

            if (quote.IsFunded)
            {
                this.Funding = new FundingDetailsViewModel(quote.AcceptedProposal);
            }
        }
    }
}

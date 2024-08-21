// <copyright file="RazorTextProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using System.Diagnostics.Contracts;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using RazorEngine.Templating;
    using UBind.Application.Export.ViewModels;
    using UBind.Application.Person;
    using UBind.Application.Queries.AssetFile;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Configuration;
    using UBind.Domain.Dto;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.Services;

    /// <summary>
    /// Text provider that returns fixed text.
    /// </summary>
    public class RazorTextProvider : ITextProvider
    {
        private readonly ITextProvider templateName;
        private readonly IRazorEngineService razorEngineService;
        private readonly IEmailInvitationConfiguration emailConfiguration;
        private readonly IConfigurationService configurationService;
        private readonly ITenantService tenantService;
        private readonly IClock clock;
        private readonly Application.IProductService productService;
        private readonly IPersonService personService;
        private readonly IProductConfiguration productConfiguration;
        private readonly IFormDataPrettifier formDataPrettifier;
        private readonly IOrganisationService organisationService;
        private readonly ICqrsMediator mediator;
        private readonly ICachingResolver cachingResolver;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RazorTextProvider"/> class.
        /// </summary>
        /// <param name="templateName">The name of the template to use.</param>
        /// <param name="razorEngineService">Razor engine service.</param>
        /// <param name="emailConfiguration">Email configuration used in invitations.</param>
        /// <param name="configurationService">configuration service.</param>
        /// <param name="tenantService">the tenant service.</param>
        /// <param name="productService">the product service.</param>
        /// <param name="productConfiguration">The product configuration service.</param>
        /// <param name="clock">Clock for obtaining current time.</param>
        /// <param name="formDataPrettifier">The form data prettifier.</param>
        /// <param name="organisationService">The organisation service.</param>
        /// <param name="mediator">The mediator.</param>
        public RazorTextProvider(
            ITextProvider templateName,
            IRazorEngineService razorEngineService,
            IEmailInvitationConfiguration emailConfiguration,
            IConfigurationService configurationService,
            IPersonService personService,
            ITenantService tenantService,
            Application.IProductService productService,
            IProductConfiguration productConfiguration,
            IClock clock,
            IFormDataPrettifier formDataPrettifier,
            IOrganisationService organisationService,
            ICqrsMediator mediator,
            ICachingResolver cachingResolver,
            ILogger logger)
        {
            Contract.Assert(templateName != null);

            this.clock = clock;
            this.templateName = templateName;
            this.razorEngineService = razorEngineService;
            this.emailConfiguration = emailConfiguration;
            this.configurationService = configurationService;
            this.tenantService = tenantService;
            this.productService = productService;
            this.productConfiguration = productConfiguration;
            this.formDataPrettifier = formDataPrettifier;
            this.organisationService = organisationService;
            this.mediator = mediator;
            this.personService = personService;
            this.cachingResolver = cachingResolver;
            this.logger = logger;
        }

        /// <inheritdoc />
        public async Task<string> Invoke(ApplicationEvent applicationEvent)
        {
            var tenant = await this.cachingResolver.GetTenantOrThrow(applicationEvent.Aggregate.TenantId);
            var product = await this.cachingResolver.GetProductOrThrow(
                applicationEvent.Aggregate.TenantId,
                applicationEvent.Aggregate.ProductId);
            var organisationId = applicationEvent.Aggregate.OrganisationId;
            if (organisationId == default)
            {
                organisationId = tenant.Details.DefaultOrganisationId;
            }

            var organisation = await this.organisationService.GetOrganisationSummaryForTenantIdAndOrganisationId(
                tenant.Id, organisationId);
            var templateName = await this.templateName.Invoke(applicationEvent);
            var template = await this.mediator.Send(new GetProductFileContentsByFileNameQuery(
                new ReleaseContext(
                    applicationEvent.Aggregate.TenantId,
                    applicationEvent.Aggregate.ProductId,
                    applicationEvent.Aggregate.Environment,
                    applicationEvent.ProductReleaseId),
                WebFormAppType.Quote,
                FileVisibility.Private,
                templateName));
            string templateString = template != null ? System.Text.Encoding.UTF8.GetString(template) : string.Empty;
            var displayableFields = await this.DisplayableFields(applicationEvent);
            var viewModel = await ApplicationEventViewModel.Create(
                this.formDataPrettifier,
                applicationEvent,
                this.emailConfiguration,
                this.configurationService,
                this.productConfiguration,
                displayableFields,
                this.personService,
                tenant,
                product,
                organisation,
                this.clock,
                this.mediator);

            var executionId = Guid.NewGuid();
            this.logger.LogWarning($"Integrations: About to execute the razor template \"{templateName}\" "
                + $"for tenant {tenant.Details.Alias}, organisation {organisation.Alias}, product {product.Details.Alias}. "
                + "Poorly written Razor templates can crash the app with a stack overflow, and our Razor execution "
                + "does not currently run in a separate sandboxed process, so it could crash uBind. "
                + $"Execution ID: {executionId}");
            var result = this.razorEngineService.IsTemplateCached(templateString, typeof(ApplicationEventViewModel))
                ? this.razorEngineService.Run(templateString, null, viewModel)
                : this.razorEngineService.RunCompile(templateString, templateString, null, viewModel);
            this.logger.LogWarning($"Integrations: Razor template execution {executionId} completed successfully.");
            return result;
        }

        private async Task<DisplayableFieldDto> DisplayableFields(ApplicationEvent applicationEvent)
        {
            var task = await Task.Run(async () =>
            {
                var productContext = new ReleaseContext(
                    applicationEvent.Aggregate.TenantId,
                    applicationEvent.Aggregate.ProductId,
                    applicationEvent.Aggregate.Environment,
                    applicationEvent.ProductReleaseId);
                return await this.configurationService.GetDisplayableFieldsAsync(productContext);
            });

            return task;
        }
    }
}

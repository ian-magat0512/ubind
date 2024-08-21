﻿// <copyright file="MsWordTemplateFileProviderModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling
{
    using System.Collections.Generic;
    using UBind.Application.Export;
    using UBind.Application.FileHandling.Template_Provider;
    using UBind.Domain.Configuration;

    /// <summary>
    /// Model for fixed MS word file provider.
    /// </summary>
    public class MsWordTemplateFileProviderModel
        : IExporterModel<IAttachmentProvider>
    {
        /// <summary>
        /// Gets or sets the name of the template to use.
        /// </summary>
        public IExporterModel<ITextProvider> TemplateName { get; set; }

        /// <summary>
        /// Gets or sets the name of the output file (with extension).
        /// </summary>
        public IExporterModel<ITextProvider> OutputFileName { get; set; }

        /// <summary>
        /// Gets or sets the condition that should be triggered in response to a relevant event.
        /// </summary>
        public IExporterModel<EventExporterCondition> Condition { get; set; }

        /// <summary>
        /// Build a fixed file provider.
        /// </summary>
        /// <param name="dependencyProvider">Container for dependencies required when building exporters.
        /// </param>
        /// <param name="productConfiguration">Contains the per-product configuration.</param>
        /// <returns>The new fixed file provider.</returns>
        public IAttachmentProvider Build(
            IExporterDependencyProvider dependencyProvider,
            IProductConfiguration productConfiguration)
        {
            IEnumerable<IJObjectProvider> templateProviders
                = new List<IJObjectProvider>
            {
                new OrganisationTemplateJObjectProvider(dependencyProvider.ConfigurationService),
                new ProductTemplateJObjectProvider(dependencyProvider.ConfigurationService),
                new FormModelTemplateJObjectProvider(
                    dependencyProvider.FormDataPrettifier,
                    productConfiguration.FormDataSchema,
                    dependencyProvider.FileAttachmentRepository),
                new CalculationStateTemplateJObjectProvider(),
                new CalculationTriggerTemplateJObjectProvider(),
                new CalculationPaymentsTemplateJObjectProvider(),
                new CalculationRiskTemplateJObjectProvider(),
                new PolicyTemplateJObjectProvider(dependencyProvider.Clock),
                new ApplicationTemplateJObjectProvider(),
                new InvoiceTemplateJObjectProvider(),
                new CustomerDetailsJObjectProvider(dependencyProvider.PersonService),
                new ApplicationSubmissionsJObjectProvider(),
                new PaymentAttemptResultsJObjectProvider(),
                new FundingProposalJObjectProvider(),
                new QuoteTemplateJObjectProvider(dependencyProvider.CachingResolver),
                new ApplicationJObjectProvider(
                    dependencyProvider.EmailConfiguration,
                    dependencyProvider.CachingResolver,
                    dependencyProvider.Mediator),
                new TenantTemplateJOBjectProvider(dependencyProvider.TenantService),
                new CreditNoteTemplateJObjectProvider(),
            };

            return new MsWordTemplateFileProvider(
                this.TemplateName?.Build(dependencyProvider, productConfiguration),
                this.OutputFileName?.Build(dependencyProvider, productConfiguration),
                this.Condition?.Build(dependencyProvider, productConfiguration),
                templateProviders,
                dependencyProvider.FileContentsLoader,
                dependencyProvider.MsWordEngineService);
        }
    }
}

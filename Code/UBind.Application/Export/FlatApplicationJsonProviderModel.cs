// <copyright file="FlatApplicationJsonProviderModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using System.Collections.Generic;
    using UBind.Application.FileHandling.Template_Provider;
    using UBind.Domain.Configuration;

    /// <summary>
    /// For converting exporting all applicaton data as flattened json.
    /// </summary>
    public class FlatApplicationJsonProviderModel : IExporterModel<ITextProvider>
    {
        /// <summary>
        /// Build a flat application json text provider.
        /// </summary>
        /// <param name="dependencyProvider">Container for dependencies required for exporter building.</param>
        /// <param name="productConfiguration">Contains per-product configuration.</param>
        /// <returns>A new instance of <see cref="FlatApplicationJsonProvider"></see>.</returns>
        public ITextProvider Build(IExporterDependencyProvider dependencyProvider, IProductConfiguration productConfiguration)
        {
            var templateProviders = new List<IJObjectProvider>
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
                new InvoiceTemplateJObjectProvider(),
                new ApplicationTemplateJObjectProvider(),
                new CustomerDetailsJObjectProvider(dependencyProvider.PersonService),
                new ApplicationSubmissionsJObjectProvider(),
                new PaymentAttemptResultsJObjectProvider(),
                new FundingProposalJObjectProvider(),
                new QuoteTemplateJObjectProvider(dependencyProvider.CachingResolver),
                new CreditNoteTemplateJObjectProvider(),
            };

            return new FlatApplicationJsonProvider(templateProviders);
        }
    }
}

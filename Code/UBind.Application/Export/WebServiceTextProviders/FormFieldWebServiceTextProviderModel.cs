// <copyright file="FormFieldWebServiceTextProviderModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export.WebServiceTextProviders
{
    using UBind.Domain.Configuration;

    /// <summary>
    /// Model for providing form field text providers for web service integration.
    /// </summary>
    public class FormFieldWebServiceTextProviderModel : IExporterModel<IWebServiceTextProvider>
    {
        /// <summary>
        /// Gets or sets the name of the form field to get text from.
        /// </summary>
        public IExporterModel<IWebServiceTextProvider> FieldName { get; set; }

        /// <inheritdoc/>
        public IWebServiceTextProvider Build(IExporterDependencyProvider dependencyProvider, IProductConfiguration productConfiguration)
        {
            return new FormFieldWebServiceTextProvider(this.FieldName?.Build(dependencyProvider, productConfiguration));
        }
    }
}

// <copyright file="GetProductComponentTriggerConfigurationQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.ProductConfiguration
{
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.Product.Component;

    /// <summary>
    /// Query for getting the triggers from the product configuration. This is needed because when we are
    /// displaying the price in the portal against a quote, depending upon the triggers that are currently
    /// in place, we might have to hide the price.
    /// </summary>
    public class GetProductComponentTriggerConfigurationQuery : IQuery<List<Trigger>>
    {
        public GetProductComponentTriggerConfigurationQuery(
            ReleaseContext releaseContext,
            WebFormAppType webFormAppType = WebFormAppType.Quote)
        {
            this.ReleaseContext = releaseContext;
            this.WebFormAppType = webFormAppType;
        }

        public ReleaseContext ReleaseContext { get; }

        public WebFormAppType WebFormAppType { get; }
    }
}

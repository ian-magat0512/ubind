// <copyright file="SerialisedEntityFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Entity
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UBind.Application.Queries.Portal;
    using UBind.Application.Services.Imports;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Sms;
    using UBind.Domain.SerialisedEntitySchemaObject;
    using UBind.Domain.Services;
    using Portal = UBind.Domain.SerialisedEntitySchemaObject.Portal;
    using Product = UBind.Domain.SerialisedEntitySchemaObject.Product;
    using Tenant = UBind.Domain.SerialisedEntitySchemaObject.Tenant;

    /// <summary>
    /// This class is needed because we need to resolve which serialiser to use for specific entity.
    /// </summary>
    public class SerialisedEntityFactory : ISerialisedEntityFactory
    {
        private readonly Dictionary<Type, Func<IEntityWithRelatedEntities, IEnumerable<string>, Task<IEntity>>> mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerialisedEntityFactory"/> class.
        /// </summary>
        /// <param name="urlConfiguration">The configuration to retrieve the api URL.</param>
        public SerialisedEntityFactory(
            IInternalUrlConfiguration urlConfiguration,
            IProductConfigurationProvider productConfigurationProvider,
            IFormDataPrettifier formDataPrettifier,
            ICachingResolver cachingResolver,
            ICqrsMediator mediator,
            IPolicyTransactionTimeOfDayScheme timeOfDayScheme)
        {
            var baseApiUrl = urlConfiguration.BaseApi;
            this.mapper = new Dictionary<Type, Func<IEntityWithRelatedEntities, IEnumerable<string>, Task<IEntity>>>()
            {
                {
                    typeof(QuoteReadModelWithRelatedEntities), (model, includedProperties) => Task.FromResult(new Quote(
                        (IQuoteReadModelWithRelatedEntities)model,
                        formDataPrettifier,
                        productConfigurationProvider,
                        includedProperties,
                        baseApiUrl,
                        cachingResolver,
                        timeOfDayScheme) as IEntity)
                },
                {
                    typeof(QuoteVersionReadModelWithRelatedEntities), (model, includedProperties) => Task.FromResult(new QuoteVersion(
                        (IQuoteVersionReadModelWithRelatedEntities)model,
                        formDataPrettifier,
                        productConfigurationProvider,
                        includedProperties,
                        baseApiUrl,
                        cachingResolver) as IEntity)
                },
                {
                    typeof(ClaimReadModelWithRelatedEntities), (model, includedProperties) => Task.FromResult(new Claim(
                        (IClaimReadModelWithRelatedEntities)model,
                        formDataPrettifier,
                        productConfigurationProvider,
                        includedProperties,
                        baseApiUrl,
                        cachingResolver) as IEntity)
                },
                {
                    typeof(ClaimVersionReadModelWithRelatedEntities), (model, includedProperties) => Task.FromResult(new ClaimVersion(
                        (IClaimVersionReadModelWithRelatedEntities)model,
                        formDataPrettifier,
                        productConfigurationProvider,
                        includedProperties,
                        baseApiUrl,
                        cachingResolver) as IEntity)
                },
                {
                    typeof(CustomerReadModelWithRelatedEntities), async (model, includedProperties) =>
                    {
                        ICustomerReadModelWithRelatedEntities customerWithRelatedEntities = (ICustomerReadModelWithRelatedEntities)model;
                        if (customerWithRelatedEntities.Portal != null)
                        {
                            customerWithRelatedEntities.PortalLocations
                                = await mediator.Send(new GetPortalLocationsQuery(customerWithRelatedEntities.Portal));
                        }

                        return new Customer(
                            (ICustomerReadModelWithRelatedEntities)model,
                            formDataPrettifier,
                            productConfigurationProvider,
                            includedProperties,
                            baseApiUrl);
                    }
                },
                {
                    typeof(DocumentReadModelWithRelatedEntities), (model, includedProperties) => Task.FromResult(new Document(
                        (IDocumentReadModelWithRelatedEntities)model,
                        baseApiUrl) as IEntity)
                },
                {
                    typeof(EmailReadModelWithRelatedEntities), (model, includedProperties) => Task.FromResult(new EmailMessage(
                        (IEmailReadModelWithRelatedEntities)model,
                        baseApiUrl) as IEntity)
                },
                {
                    typeof(OrganisationReadModelWithRelatedEntities), (model, includedProperties) => Task.FromResult(new Organisation(
                        (IOrganisationReadModelWithRelatedEntities)model,
                        includedProperties) as IEntity)
                },
                {
                    typeof(ProductWithRelatedEntities), (model, includedProperties) => Task.FromResult(new Product(
                        (IProductWithRelatedEntities)model,
                        includedProperties,
                        baseApiUrl,
                        cachingResolver) as IEntity)
                },
                {
                    typeof(TenantWithRelatedEntities), (model, includedProperties) => Task.FromResult(new Tenant(
                        (ITenantWithRelatedEntities)model,
                        includedProperties) as IEntity)
                },
                {
                    typeof(UserReadModelWithRelatedEntities), async (model, includedProperties) =>
                    {
                        IUserReadModelWithRelatedEntities userWithRelatedEntities = (IUserReadModelWithRelatedEntities)model;
                        if (userWithRelatedEntities.Portal != null)
                        {
                            userWithRelatedEntities.PortalLocations
                                = await mediator.Send(new GetPortalLocationsQuery(userWithRelatedEntities.Portal));
                        }

                        return new User(
                            (IUserReadModelWithRelatedEntities)model,
                            includedProperties);
                    }
                },
                {
                    typeof(PolicyReadModelWithRelatedEntities), (model, includedProperties) => Task.FromResult(new Policy(
                        (IPolicyReadModelWithRelatedEntities)model,
                        formDataPrettifier,
                        productConfigurationProvider,
                        includedProperties,
                        baseApiUrl,
                        cachingResolver,
                        timeOfDayScheme) as IEntity)
                },
                {
                    typeof(PolicyTransactionReadModelWithRelatedEntities), (model, includedProperties) => Task.FromResult(new PolicyTransaction(
                        (IPolicyTransactionReadModelWithRelatedEntities)model,
                        formDataPrettifier,
                        productConfigurationProvider,
                        includedProperties,
                        baseApiUrl,
                        cachingResolver,
                        timeOfDayScheme) as IEntity)
                },
                {
                    typeof(PortalWithRelatedEntities), async (model, includedProperties) =>
                    {
                        IPortalWithRelatedEntities portalWithRelatedEntities = (IPortalWithRelatedEntities)model;
                        var locations = await mediator.Send(new GetPortalLocationsQuery(portalWithRelatedEntities.Portal));
                        return new Portal(
                            portalWithRelatedEntities,
                            includedProperties,
                            locations);
                    }
                },
                {
                    typeof(PersonReadModelWithRelatedEntities), (model, includedProperties) => Task.FromResult(new Person(
                        (IPersonReadModelWithRelatedEntities)model,
                        includedProperties) as IEntity)
                },
                {
                    typeof(SmsReadModelWithRelatedEntities), (model, includedProperties) => Task.FromResult(new SmsMessage(
                        (ISmsReadModelWithRelatedEntities)model,
                        includedProperties) as IEntity)
                },
                {
                    typeof(SystemEventWithRelatedEntities), (model, includedProperties) => Task.FromResult(new Event(
                        (ISystemEventWithRelatedEntities)model,
                        includedProperties) as IEntity)
                },
            };
        }

        /// <inheritdoc/>
        public async Task<IEntity> Create(IEntityWithRelatedEntities model, IEnumerable<string> includedProperties)
        {
            if (this.mapper.TryGetValue(model.GetType(), out var factoryFunc))
            {
                return await factoryFunc.Invoke(model, includedProperties);
            }

            throw new InvalidOperationException("An attempt was made to create a seriliased entity for an instance of "
                + $"type {model.GetType().Name}, however no factory function exists.");
        }
    }
}

// <copyright file="EntityProviderFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Entity;

using UBind.Domain.Extensions;
using UBind.Domain;
using UBind.Domain.ReadModel;
using Microsoft.Extensions.DependencyInjection;
using UBind.Domain.Repositories;
using UBind.Domain.Product;
using UBind.Domain.ReadModel.Claim;
using UBind.Domain.Exceptions;
using Newtonsoft.Json.Linq;
using UBind.Domain.ReadModel.Portal;

/// <summary>
/// Resolves the entity provider for the given entity type.
/// </summary>
public static class EntityProviderFactory
{
    public static async Task<BaseEntityProvider> Create(
        string entityType,
        string entityId,
        IServiceProvider dependencyProvider,
        Func<Task<JObject>> errorDataCallback)
    {
        var serialisedEntityFactory = dependencyProvider.GetRequiredService<ISerialisedEntityFactory>();
        var cachingResolver = dependencyProvider.GetRequiredService<ICachingResolver>();
        var entityTypeEnum = entityType.ToEnumOrThrow<EntityType>();
        var entityIdProvider = new StaticProvider<Data<string>>(new Data<string>(entityId));
        switch (entityTypeEnum)
        {
            case EntityType.Quote:
                var quoteReadModelRepository = dependencyProvider.GetRequiredService<IQuoteReadModelRepository>();
                return new QuoteEntityProvider(entityIdProvider, quoteReadModelRepository, serialisedEntityFactory);
            case EntityType.Policy:
                var policyReadModelRepository = dependencyProvider.GetRequiredService<IPolicyReadModelRepository>();
                return new PolicyEntityProvider(entityIdProvider, policyReadModelRepository, serialisedEntityFactory);
            case EntityType.PolicyTransaction:
                var policyTransactionReadModelRepository = dependencyProvider.GetRequiredService<IPolicyTransactionReadModelRepository>();
                return new PolicyTransactionEntityProvider(entityIdProvider, policyTransactionReadModelRepository, serialisedEntityFactory);
            case EntityType.Tenant:
                var tenantRepository = dependencyProvider.GetRequiredService<ITenantRepository>();
                return new TenantEntityProvider(entityIdProvider, tenantRepository, serialisedEntityFactory, cachingResolver);
            case EntityType.Product:
                var productRepository = dependencyProvider.GetRequiredService<IProductRepository>();
                return new ProductEntityProvider(entityIdProvider, productRepository, serialisedEntityFactory, cachingResolver);
            case EntityType.Customer:
                var customerReadModelRepository = dependencyProvider.GetRequiredService<ICustomerReadModelRepository>();
                return new CustomerEntityProvider(entityIdProvider, customerReadModelRepository, serialisedEntityFactory);
            case EntityType.User:
                var userReadModelRepository = dependencyProvider.GetRequiredService<IUserReadModelRepository>();
                return new UserEntityProvider(entityIdProvider, userReadModelRepository, serialisedEntityFactory, cachingResolver);
            case EntityType.EmailMessage:
                var emailRepository = dependencyProvider.GetRequiredService<IEmailRepository>();
                return new EmailMessageEntityProvider(entityIdProvider, emailRepository, serialisedEntityFactory);
            case EntityType.Document:
                var documentRepository = dependencyProvider.GetRequiredService<IQuoteDocumentReadModelRepository>();
                return new DocumentEntityProvider(entityIdProvider, documentRepository, serialisedEntityFactory);
            case EntityType.Claim:
                var claimReadModelRepository = dependencyProvider.GetRequiredService<IClaimReadModelRepository>();
                return new ClaimEntityProvider(entityIdProvider, claimReadModelRepository, serialisedEntityFactory);
            case EntityType.Organisation:
                var organisationReadModelRepository = dependencyProvider.GetRequiredService<IOrganisationReadModelRepository>();
                return new OrganisationEntityProvider(entityIdProvider, organisationReadModelRepository, serialisedEntityFactory);
            case EntityType.ClaimVersion:
                var claimVersionReadModelRepository = dependencyProvider.GetRequiredService<IClaimVersionReadModelRepository>();
                return new ClaimVersionEntityProvider(entityIdProvider, claimVersionReadModelRepository, serialisedEntityFactory);
            case EntityType.QuoteVersion:
                var quoteVersionReadModelRepository = dependencyProvider.GetRequiredService<IQuoteVersionReadModelRepository>();
                return new QuoteVersionEntityProvider(entityIdProvider, quoteVersionReadModelRepository, serialisedEntityFactory);
            case EntityType.Portal:
                var portalRepository = dependencyProvider.GetRequiredService<IPortalReadModelRepository>();
                return new PortalEntityProvider(entityIdProvider, portalRepository, serialisedEntityFactory);
            case EntityType.Role:
                var roleRepository = dependencyProvider.GetRequiredService<IRoleRepository>();
                return new RoleEntityProvider(entityIdProvider, roleRepository, serialisedEntityFactory, cachingResolver);
            case EntityType.Report:
                var reportRepository = dependencyProvider.GetRequiredService<IReportReadModelRepository>();
                return new ReportEntityProvider(entityIdProvider, reportRepository, serialisedEntityFactory, cachingResolver);
            case EntityType.Person:
                var personRepository = dependencyProvider.GetRequiredService<IPersonReadModelRepository>();
                return new PersonEntityProvider(entityIdProvider, personRepository, serialisedEntityFactory, cachingResolver);
            case EntityType.SmsMessage:
                var smsRepository = dependencyProvider.GetRequiredService<ISmsRepository>();
                return new SmsMessageEntityProvider(entityIdProvider, smsRepository, serialisedEntityFactory);
            case EntityType.Message:
                var smsRepository2 = dependencyProvider.GetRequiredService<ISmsRepository>();
                var emailRepository2 = dependencyProvider.GetRequiredService<IEmailRepository>();
                return new MessageEntityProvider(entityIdProvider, smsRepository2, emailRepository2, serialisedEntityFactory);
            case EntityType.Event:
                var systemEventRepository = dependencyProvider.GetRequiredService<ISystemEventRepository>();
                return new SystemEventEntityProvider(entityIdProvider, systemEventRepository, serialisedEntityFactory);
            default:
                throw new ErrorException(Errors.Automation.Provider.Entity.TypeNotSupported(
                    entityType, await errorDataCallback()));
        }
    }
}

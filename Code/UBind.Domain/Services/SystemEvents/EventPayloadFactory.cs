// <copyright file="EventPayloadFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Services.SystemEvents;

using UBind.Domain;
using UBind.Domain.Aggregates.Customer;
using UBind.Domain.Aggregates.Organisation;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Aggregates.User;
using UBind.Domain.Entities;
using UBind.Domain.Events.Payload;
using UBind.Domain.Helpers;
using UBind.Domain.ReadModel;
using UBind.Domain.ReadModel.Customer;
using UBind.Domain.ReadModel.User;
using UBind.Domain.Repositories;
using UBind.Domain.Services;

/// <summary>
/// This class is used to create event payloads
/// that is used as event data in automations.
/// </summary>
public class EventPayloadFactory : IEventPayloadFactory
{
    private readonly ICachingResolver cachingResolver;
    private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
    private readonly IUserSessionService userSessionService;
    private readonly ICustomerReadModelRepository customerReadModelRepository;
    private readonly IRoleRepository roleRepository;

    public EventPayloadFactory(
        ICachingResolver cachingResolver,
        IHttpContextPropertiesResolver httpContextPropertiesResolver,
        IUserSessionService userSessionService,
        ICustomerReadModelRepository customerReadModelRepository,
        IRoleRepository roleRepository)
    {
        this.cachingResolver = cachingResolver;
        this.httpContextPropertiesResolver = httpContextPropertiesResolver;
        this.userSessionService = userSessionService;
        this.customerReadModelRepository = customerReadModelRepository;
        this.roleRepository = roleRepository;
    }

    /// <inheritdoc />
    public async Task<OrganisationEventPayload> CreateOrganisationEventPayload(Organisation organisation)
    {
        var payload = new OrganisationEventPayload();
        await this.SetPerformingUser(payload);
        await this.SetTenant(payload, organisation.TenantId);
        payload.Organisation = new Events.Models.Organisation
        {
            Id = organisation.Id,
            Alias = organisation.Alias,
        };
        return payload;
    }

    /// <inheritdoc />
    public async Task<OrganisationEventPayload> CreateOrganisationEventPayload(Guid tenantId, Guid organisationId)
    {
        var payload = new OrganisationEventPayload();
        await this.SetPerformingUser(payload);
        await this.SetTenant(payload, tenantId);
        var organisation = await this.cachingResolver.GetOrganisationOrThrow(tenantId, organisationId);
        payload.Organisation = EventPayloadHelper.GetOrganisation(organisation);
        return payload;
    }

    /// <inheritdoc />
    public async Task<CustomerEventPayload> CreateCustomerEventPayload(CustomerAggregate aggregate)
    {
        return await this.CreateCustomerEventPayload(
            aggregate.TenantId,
            aggregate.OrganisationId,
            aggregate.Id,
            aggregate.DisplayName);
    }

    /// <inheritdoc />
    public async Task<CustomerEventPayload> CreateCustomerEventPayload(CustomerReadModelDetail customer)
    {
        return await this.CreateCustomerEventPayload(
            customer.TenantId,
            customer.OrganisationId,
            customer.Id,
            customer.DisplayName = !string.IsNullOrEmpty(customer.DisplayName)
                ? customer.DisplayName
                : customer.FullName);
    }

    /// <inheritdoc />
    public async Task<QuoteOperationEventPayload> CreateQuoteEventPayload(QuoteAggregate aggregate, Guid? quoteId)
    {
        var quote = quoteId.HasValue ? aggregate.GetQuoteOrThrow(quoteId.Value) : null;
        string customerDisplayName = quote?.LatestCustomerDetails?.Data == null
                ? string.Empty
                : !string.IsNullOrEmpty(quote.LatestCustomerDetails.Data.DisplayName)
                    ? quote.LatestCustomerDetails.Data.DisplayName
                    : quote.LatestCustomerDetails.Data.FullName;

        var payload = await this.CreateQuoteEventPayload(
            aggregate.TenantId,
            aggregate.OrganisationId,
            aggregate.ProductId,
            aggregate.CustomerId,
            customerDisplayName);
        if (quote != null)
        {
            payload.SetQuote(quote);
        }

        return payload;
    }

    /// <inheritdoc />
    public async Task<QuoteOperationEventPayload> CreateQuoteEventPayload(NewQuoteReadModel quote)
    {
        var payload = await this.CreateQuoteEventPayload(
            quote.TenantId,
            quote.OrganisationId,
            quote.ProductId,
            quote.CustomerId);
        payload.Quote = EventPayloadHelper.GetQuote(quote);
        return payload;
    }

    /// <inheritdoc />
    public async Task<TenantEventPayload> CreateTenantEventPayload(Guid tenantId)
    {
        var payload = new TenantEventPayload();
        await this.SetPerformingUser(payload);
        await this.SetTenant(payload, tenantId);
        return payload;
    }

    /// <inheritdoc />
    public async Task<UserEventPayload> CreateUserEventPayload(UserAggregate aggregate, Guid? roleId = null)
    {
        var payload = await this.CreateUserEventPayload(
            aggregate.TenantId,
            aggregate.OrganisationId,
            aggregate.Id,
            aggregate.DisplayName ?? string.Empty,
            aggregate.LoginEmail ?? string.Empty);
        if (roleId.HasValue)
        {
            var role = this.roleRepository.GetRoleById(aggregate.TenantId, roleId.Value);
            role = EntityHelper.ThrowIfNotFound<Role>(role, roleId.Value);
            payload.SetRole(role);
        }

        return payload;
    }

    /// <inheritdoc />
    public async Task<UserEventPayload> CreateUserEventPayload(UserReadModel user)
    {
        return await this.CreateUserEventPayload(
            user.TenantId,
            user.OrganisationId,
            user.Id,
            user.DisplayName,
            user.LoginEmail);
    }

    public async Task<UserEventPayload> CreateUserEventPayload(Guid tenantId, string accountEmailAddress)
    {
        var payload = new UserEventPayload();
        await this.SetTenant(payload, tenantId);
        payload.SetEmailAddress(accountEmailAddress);
        return payload;
    }

    private async Task SetPerformingUser(IEventPayload payload)
    {
        var performingUser = this.httpContextPropertiesResolver.PerformingUser;
        if (performingUser == null)
        {
            return;
        }

        var userSession = await this.userSessionService.Get(performingUser);
        if (userSession == null)
        {
            return;
        }

        payload.PerformingUser = EventPayloadHelper.GetPerformingUser(userSession);
    }

    private async Task SetTenant(IEventPayload payload, Guid tenantId)
    {
        var tenant = await this.cachingResolver.GetTenantOrThrow(tenantId);
        payload.Tenant = EventPayloadHelper.GetTenant(tenant);
    }

    private async Task<QuoteOperationEventPayload> CreateQuoteEventPayload(Guid tenantId, Guid organisationId, Guid productId, Guid? customerId, string? customerDisplayName = null)
    {
        var payload = new QuoteOperationEventPayload();
        await this.SetPerformingUser(payload);
        await this.SetTenant(payload, tenantId);
        var organisation = await this.cachingResolver.GetOrganisationOrThrow(tenantId, organisationId);
        payload.Organisation = EventPayloadHelper.GetOrganisation(organisation);
        var product = await this.cachingResolver.GetProductOrThrow(tenantId, productId);
        payload.Product = EventPayloadHelper.GetProduct(product);
        if (customerId.HasValue)
        {
            if (string.IsNullOrEmpty(customerDisplayName))
            {
                var customer = this.customerReadModelRepository.GetCustomerById(tenantId, customerId.Value);
                payload.Customer = EventPayloadHelper.GetCustomer(customer);
                return payload;
            }

            payload.Customer = EventPayloadHelper.GetCustomer(customerId.Value, customerDisplayName);
        }

        return payload;
    }

    private async Task<UserEventPayload> CreateUserEventPayload(Guid tenantId, Guid organisationId, Guid userId, string displayName, string accountEmailAddress)
    {
        var payload = new UserEventPayload();
        await this.SetPerformingUser(payload);
        await this.SetTenant(payload, tenantId);
        var organisation = await this.cachingResolver.GetOrganisationOrThrow(tenantId, organisationId);
        payload.Organisation = EventPayloadHelper.GetOrganisation(organisation);
        payload.SetUser(userId, displayName, accountEmailAddress);
        return payload;
    }

    private async Task<CustomerEventPayload> CreateCustomerEventPayload(Guid tenantId, Guid organisationId, Guid customerId, string displayName)
    {
        var payload = new CustomerEventPayload();
        await this.SetPerformingUser(payload);
        await this.SetTenant(payload, tenantId);
        var organisation = await this.cachingResolver.GetOrganisationOrThrow(tenantId, organisationId);
        payload.Organisation = EventPayloadHelper.GetOrganisation(organisation);
        payload.Customer = EventPayloadHelper.GetCustomer(customerId, displayName);
        return payload;
    }
}

// <copyright file="IEventPayloadFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Services.SystemEvents;

using UBind.Domain.Aggregates.Customer;
using UBind.Domain.Aggregates.Organisation;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Aggregates.User;
using UBind.Domain.Events.Payload;
using UBind.Domain.ReadModel;
using UBind.Domain.ReadModel.Customer;
using UBind.Domain.ReadModel.User;

/// <summary>
/// Interface for the Event Payload Factory.
/// </summary>
public interface IEventPayloadFactory
{
    /// <summary>
    /// Creates an event payload for a tenant event.
    /// </summary>
    Task<TenantEventPayload> CreateTenantEventPayload(Guid tenantId);

    /// <summary>
    /// Creates an event payload for an organisation event from an organisation aggregate.
    /// </summary>
    Task<OrganisationEventPayload> CreateOrganisationEventPayload(Organisation organisation);

    /// <summary>
    /// Creates an event payload for an organisation event.
    /// This is use when the system event is not triggered by an aggregateEvent
    /// </summary>
    Task<OrganisationEventPayload> CreateOrganisationEventPayload(Guid tenantId, Guid organisationId);

    /// <summary>
    /// Creates a customer event payload from the customer aggregate.
    /// </summary>
    Task<CustomerEventPayload> CreateCustomerEventPayload(CustomerAggregate aggregate);

    /// <summary>
    /// Creates a customer event payload from the customerreadmodel.
    /// This is use when the system event is not triggered by an aggregateEvent.
    /// </summary>
    Task<CustomerEventPayload> CreateCustomerEventPayload(CustomerReadModelDetail customer);

    /// <summary>
    /// Creates a user event payload from the user aggregate.
    /// </summary>
    /// <param name="aggregate">The useraggregate.</param>
    /// <param name="roleId">The id of the role added or retracted from a user in role related event.</param>
    Task<UserEventPayload> CreateUserEventPayload(UserAggregate aggregate, Guid? roleId = null);

    /// <summary>
    /// Creates an event payload for a user event with only tenant and emailaddress in the payload.
    /// </summary>
    Task<UserEventPayload> CreateUserEventPayload(Guid tenantId, string accountEmailAddress);

    /// <summary>
    /// Creates a user event payload from the userreadmodel.
    /// This should be used when the system event is not triggered by an aggregateEvent.
    /// </summary>
    Task<UserEventPayload> CreateUserEventPayload(UserReadModel user);

    /// <summary>
    /// Creates a quote event payload from the quote aggregate.
    /// </summary>
    Task<QuoteOperationEventPayload> CreateQuoteEventPayload(QuoteAggregate aggregate, Guid? quoteId);

    /// <summary>
    /// Creates a quote event payload from the quote readmodel.
    /// This should be used when the system event is not triggered by an aggregateEvent.
    /// </summary>
    Task<QuoteOperationEventPayload> CreateQuoteEventPayload(NewQuoteReadModel quote);
}

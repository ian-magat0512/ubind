// <copyright file="EventPayloadHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Events.Payload;

using UBind.Domain.Events.Models;
using UBind.Domain.Extensions;
using UBind.Domain.Helpers;
using UBind.Domain.ReadModel;
using UBind.Domain.ReadModel.Customer;
using UBind.Domain.Redis;

/// <summary>
/// Helper class for the Event Payload.
/// This returns a small object reprentation of the entities provided.
/// </summary>
public static class EventPayloadHelper
{
    public static Organisation? GetOrganisation(OrganisationReadModel organisation)
    {
        if (organisation == null)
        {
            return null;
        }

        return new Organisation
        {
            Id = organisation.Id,
            Alias = organisation.Alias,
        };
    }

    public static Tenant? GetTenant(Domain.Tenant tenant)
    {
        if (tenant == null)
        {
            return null;
        }

        return new Tenant(tenant.Id, tenant.Details.Alias);
    }

    public static Customer? GetCustomer(CustomerReadModelDetail customer)
    {
        if (customer == null)
        {
            return null;
        }

        return new Customer
        {
            Id = customer.Id,
            DisplayName = !string.IsNullOrEmpty(customer.DisplayName)
                ? PersonInformationHelper.GetMaskedNameWithHashing(customer.DisplayName)
                : PersonInformationHelper.GetMaskedNameWithHashing(customer.FullName),
        };
    }

    public static Customer? GetCustomer(Guid customerId, string displayName)
    {
        return new Customer
        {
            Id = customerId,
            DisplayName = PersonInformationHelper.GetMaskedNameWithHashing(displayName),
        };
    }

    public static Product? GetProduct(Domain.Product.Product product)
    {
        if (product == null)
        {
            return null;
        }

        return new Product
        {
            Id = product.Id,
            Alias = product.Details.Alias,
        };
    }

    public static User? GetPerformingUser(UserSessionModel user)
    {
        if (user == null)
        {
            return null;
        }

        return new User
        {
            Id = user.Id,
            DisplayName = PersonInformationHelper.GetMaskedNameWithHashing(user.DisplayName),
            AccountEmailAddress = PersonInformationHelper.GetMaskedEmailWithHashing(user.AccountEmailAddress),
        };
    }

    public static Quote? GetQuote(NewQuoteReadModel quote)
    {
        if (quote == null)
        {
            return null;
        }

        return new Quote
        {
            Id = quote.Id,
            QuoteReference = !string.IsNullOrEmpty(quote.QuoteNumber) ? quote.QuoteNumber : null,
            Type = quote.Type.ToString().ToCamelCase(),
        };
    }

    public static Quote? GetQuote(Domain.Aggregates.Quote.Quote quote)
    {
        if (quote == null)
        {
            return null;
        }

        return new Quote
        {
            Id = quote.Id,
            QuoteReference = !string.IsNullOrEmpty(quote.QuoteNumber) ? quote.QuoteNumber : null,
            Type = quote.Type.ToString().ToCamelCase(),
        };
    }
}

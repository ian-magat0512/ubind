// <copyright file="IZaiConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Payment.Zai
{
    using System.Collections.Generic;
    using UBind.Application.Payment.Zai.ZaiEntities;

    /// <summary>
    /// Account configuration for the Zai payment gateway for an environment.
    /// </summary>
    public interface IZaiConfiguration
    {
        /// <summary>
        /// Gets the client ID for obtaining the access token.
        /// </summary>
        string ClientId { get; }

        /// <summary>
        /// Gets the client secret to be used for obtaining access token.
        /// </summary>
        string ClientSecret { get; }

        /// <summary>
        /// Gets the scope to specify to be used for obtaining access.
        /// </summary>
        string Scope { get; }

        /// <summary>
        /// Gets the list of seller accounts that can be used to tag the item against.
        /// </summary>
        List<OrganisationSellerAccount> OrganisationSellerAccounts { get; }

        /// <summary>
        /// Gets the URL for making payment requests.
        /// </summary>
        string PaymentUrl { get; }

        /// <summary>
        /// Gets the URL for obtaining access tokens.
        /// </summary>
        string AuthorizationUrl { get; }

        /// <summary>
        /// Gets the URL used for creating a user with Zai.
        /// </summary>
        string UserCreationUrl { get; }

        /// <summary>
        /// Gets the URL used for retrieving a user account.
        /// </summary>
        string UserRetrievalUrl { get; }

        /// <summary>
        /// Gets the URL to be used for retrieving items associated with a user.
        /// </summary>
        string UserItemRetrievalUrl { get; }

        /// <summary>
        /// Gets the URL used for creating an item with Zai.
        /// </summary>
        string ItemCreationUrl { get; }

        /// <summary>
        /// Gets the URL used for updating an item with Zai.
        /// </summary>
        string ItemUpdateUrl { get; }

        /// <summary>
        /// Gets the URL used for posting card accounts with Zai.
        /// </summary>
        string CardCaptureUrl { get; }

        /// <summary>
        /// Gets the URL used for retrieving card accounts with Zai.
        /// </summary>
        string CardRetrievalUrl { get; }

        /// <summary>
        /// Gets the url to be used to create a fee to be associated with an item.
        /// </summary>
        string FeeCreationUrl { get; }
    }
}

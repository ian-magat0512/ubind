// <copyright file="Customer.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

#pragma warning disable SA1118 // Parameter should not span multiple lines

namespace UBind.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Allows enumeration of all application errors as outlined here:
    /// https://enterprisecraftsmanship.com/posts/advanced-error-handling-techniques/.
    /// </summary>
    public static partial class Errors
    {
        public static class Customer
        {
            public static Error NotFound(Guid customerId) =>
                new Error(
                    "customer.not.found",
                    $"Customer not found",
                    $"When trying to find customer '{customerId}', nothing came up. Please ensure that you are passing the correct ID or contact customer support if you are experiencing this error in the portal.",
                    HttpStatusCode.NotFound,
                    null,
                    new JObject()
                    {
                        { "customerId", customerId },
                    });

            public static Error BelongsToTheSameOrganisation(Guid customerId, Guid organisationId) =>
                new Error(
                    "customer.belongs.to.the.same.organisation",
                    $"The customer '{customerId}' already belongs to the same organisation",
                    $"When trying to transfer the customer to another organisation, the application won't allow it because the customer with an Id '{customerId}' already belongs to the organisation with an Id '{organisationId}'."
                    + "Please ensure that you are passing the correct organisation ID or if you need further assistance, please don't hesitate to contact customer support.",
                    HttpStatusCode.Forbidden,
                    null,
                    new JObject()
                    {
                        { "customerId", customerId },
                        { "organisationId", organisationId },
                    });

            public static Error PersonNotFound(Guid customerId) =>
                new Error(
                    "customer.person.not.found",
                    $"Person not found",
                    $"When trying to find a person for customer '{customerId}', nothing came up. Please ensure that you are passing the correct ID or contact customer support if you are experiencing this error in the portal.",
                    HttpStatusCode.NotFound,
                    null,
                    new JObject()
                    {
                        { "customerId", customerId },
                    });

            public static Error CreationFailedMissingValue(string valueDescription) =>
                new Error(
                    "customer.create.missing.value",
                    "Unable to create customer",
                    $"When trying to create a customer object in the system, the value for {valueDescription} was missing. "
                    + "The customer object was not able to be created. Please ensure complete customer details are provided.",
                    HttpStatusCode.BadRequest);

            public static Error EmailAddressInUseByAnotherUser(string emailAddress) =>
                new Error(
                    "customer.email.address.in.use.by.user",
                    "Someone's using that email address",
                    $"The customers primary email address \"{emailAddress}\" is already associated with another user account. "
                    + "If the customer already has an account, you may want to merge this customer with the existing account. "
                    + "Otherwise, please change this customer's primary email address to something unique.",
                    HttpStatusCode.Conflict);

            public static Error PersonIsAlreadyAPrimaryRecord(Guid personId, string personDisplayName, Guid customerId) =>
               new Error(
                   "customer.person.is.already.a.primary.record",
                   "This person is already the primary person",
                   $"The person \"{personDisplayName} is already the primary person record for this customer.",
                   HttpStatusCode.Conflict,
                   null,
                   new JObject()
                    {
                        { "PersonId", personId.ToString() },
                        { "CustomerId", customerId.ToString() },
                    });

            public static Error LoginEmailAddressShouldNotBeEmpty(string fullName, List<string> additionalDetails) =>
              new Error(
                  "customer.login.email.address.should.not.be.empty",
                  "Login email address should not be empty",
                  $"When trying to update the login email address of the customer \"{fullName}\", the new email address was empty. "
                       + "You cannot remove the login email address of a customer. "
                       + "If you are creating a quote while logged in as a customer, "
                       + "then it's likely there is an issue with the form which needs to be fixed by a product developer. "
                       + "We apologise for the inconvenience. "
                       + "If you need further assistance, please don't hesitate to contact customer support.",
                  HttpStatusCode.NotAcceptable,
                  additionalDetails);

            public static class Merge
            {
                public static Error DeletingCustomerStillHasPerson(Guid customerId) =>
                    new Error(
                          "customer.delete.on.merge.still.has.person",
                          "The customer you are trying to delete still has person",
                          $"When trying to delete customer with ID {customerId}, there is still a person that has not been processed. "
                               + "It's likely there is an issue with the merging which needs to be fixed by a developer. "
                               + "We apologise for the inconvenience. "
                               + "If you need further assistance, please don't hesitate to contact customer support.",
                          HttpStatusCode.InternalServerError,
                          null);
            }
        }
    }
}

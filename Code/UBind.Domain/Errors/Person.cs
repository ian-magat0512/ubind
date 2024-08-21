// <copyright file="Person.cs" company="uBind">
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
    using Humanizer;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Aggregates.Person.Fields;

    /// <summary>
    /// Allows enumeration of all application errors as outlined here:
    /// https://enterprisecraftsmanship.com/posts/advanced-error-handling-techniques/.
    /// </summary>
    public static partial class Errors
    {
        public static class Person
        {
            public static Error NotFound(Guid personId) =>
                new Error(
                    "person.not.found",
                    $"Person not found",
                    $"When trying to find the person '{personId}', nothing came up. Please ensure that you are passing "
                    + "the correct ID or contact customer support if you are experiencing this error in the portal.",
                    HttpStatusCode.NotFound,
                    new List<string> { $"Person Id: {personId}", },
                    new JObject() { { "personId", personId }, });

            public static Error CustomerNotFound(Guid personId) =>
                new Error(
                    "customer.for.person.not.found",
                    $"Customer for person not found",
                    $"When trying to find the customer for person '{personId}', nothing came up. Please ensure that "
                    + "you are passing the correct ID or contact customer support if you are experiencing this error "
                    + "in the portal.",
                    HttpStatusCode.NotFound,
                    new List<string> { $"Person Id: {personId}", },
                    new JObject() { { "personId", personId }, });

            public static Error UserNotFound(Guid personId) =>
                new Error(
                    "user.for.person.not.found",
                    $"User for person not found",
                    $"When trying to find the user for person '{personId}', nothing came up. Please ensure that "
                    + "you are passing the correct ID or contact customer support if you are experiencing this error "
                    + "in the portal.",
                    HttpStatusCode.NotFound,
                    new List<string> { $"Person Id: {personId}", },
                    new JObject() { { "personId", personId }, });

            public static Error DeletePrimaryPersonRecord(Guid personId, string personDisplayName) =>
               new Error(
                   "person.delete.primary.record",
                   "Cannot delete primary person",
                   $"The person \"{personDisplayName}\" cannot be deleted because it is the primary person for this "
                   + "customer. To delete this person, please make another person the primary person first.",
                   HttpStatusCode.Conflict,
                   new List<string> { $"Person Id: {personId}", },
                   new JObject() { { "personId", personId.ToString() }, });

            public static Error PersonRecordAlreadyDeleted(Guid personId) =>
                new Error(
                    "person.record.already.deleted",
                    "Cannot delete a non-existing person record",
                    $"The person that you are trying to delete has already been deleted.",
                    HttpStatusCode.Conflict,
                    new List<string> { $"Person Id: {personId}", },
                    new JObject() { { "personId", personId.ToString() }, });

            public static Error CannotSendActivationBeforeUserAccountCreated(string displayName) =>
                new Error(
                    "person.cannot.send.activation.before.user.account.created",
                    "Cannot send an activation invitation for a person with no user account",
                    $"You've attempted to send an activation invitation to the person \"{displayName}\", "
                    + "however there's no user account associated with that person. Please ensure a user account "
                    + "is created first, before sending the activation invitation. If you have created an account "
                    + "for this person, and therefore believe this is an error, please get in touch with customer "
                    + "support.",
                    HttpStatusCode.PreconditionFailed,
                    new List<string> { $"Display Name: {displayName}", },
                    new JObject { { "displayName", displayName }, });

            public static Error CannotCreateAUserAccountForAPersonWithExistingUser(Guid userId) =>
                new Error(
                    "person.cannot.create.user.account.when.one.already.exists",
                    "Cannot create more than one user account for a person",
                    $"You've attempted to create a user account for a person that already has "
                    + "an existing account. A person can only have one user account.",
                    HttpStatusCode.Conflict,
                    new List<string> { $"Current User Account Id: {userId}", });

            public static Error CannotAssociateAUserAccountForAPersonWithExistingUser(Guid personId) =>
                new Error(
                    "person.cannot.be.associated.to.user.when.one.already.exists",
                    "Cannot associate a user account to a person with an existing user account",
                    $"You've attempted to associate a user account to a person (id: {personId}) that "
                    + "already has an existing account. A person can only have one user account.",
                    HttpStatusCode.Conflict,
                    new List<string> { $"Person Id: {personId}" });

            public static Error PhoneNumberInvalidAustralianPhoneNumber(string phoneNumber) =>
                new Error(
                    "person.phone.number.invalid",
                    "Invalid phone number",
                    $"The person phone number \"{phoneNumber}\" is not a valid australian phone number. " +
                    "Please use valid Australian phone number of the following format e.g. 0x xxxx xxxx, or +61 x xxxx xxxx.",
                    HttpStatusCode.BadRequest);

            public static Error InvalidField<T>(T item, string value)
                where T : LabelledOrderedField
            {
                var contextValue = item.GetType().Name
                    .Humanize()
                    .ToLower()
                    .Replace("id", "ID")
                    .Replace("field", string.Empty)
                    .Trim();

                var label = item.Label.ToLower() != "other" ? item.Label.Humanize() : item.CustomLabel;
                if (item.GetType() == typeof(PhoneNumberField))
                {
                    return InvalidField("phone number", value, "\"+61XXXXXXX\" or \"0XXXXXXXXX\"", label);
                }
                else if (item.GetType() == typeof(EmailAddressField))
                {
                    return InvalidField(contextValue, value, "\"xxx@xx.xx\"", label);
                }
                else if (item.GetType() == typeof(WebsiteAddressField))
                {
                    return InvalidField(contextValue, value, "\"https://google.com\" or \"www.bing.com\"", label);
                }
                else
                {
                    return InvalidField(contextValue, value, "\"ubind.aptiture or skype:ubind2005\"", label);
                }
            }

            public static Error InvalidField(string fieldContext, string value, string exampleFormat, string label) =>
                new Error(
                    $"person.{fieldContext.Trim().ToLower().Replace(" ", ".")}.invalid",
                    $"Invalid {fieldContext}",
                    $"The value \"{value}\" provided for the {fieldContext} contact detail with the label \"{label}\" is invalid. " +
                    $"All {fieldContext} contact details must be provided in the format {exampleFormat}.",
                    HttpStatusCode.BadRequest);

            public static Error EmailAddressInvalid(string emailAddress) =>
                new Error(
                    "person.email.address.invalid",
                    "The person email address is invalid",
                    $"The person email address '{emailAddress}' is invalid. " +
                    "The person email address values must use the format 'xxx@xx.xx'",
                    HttpStatusCode.BadRequest);

            public static Error StreetAddressIncomplete(string address, string suburb, string state, string postcode) =>
                new Error(
                    "person.street.address.incomplete",
                    "The person street address provided is incomplete",
                    $"The person street address you provided is invalid. " +
                    "Please input a proper contact streed address complete with address, suburb, state and postcode.",
                    HttpStatusCode.BadRequest,
                    new List<string> { $"Address: {address}", $"Suburb: {suburb}", $"State: {state}", $"Postcode: {postcode}" });

            public static Error StreetAddressStateInvalid(string label, string state, List<string> validStates) =>
               new Error(
                   "person.street.address.state.invalid",
                   "The person street address state is invalid",
                   $"The state '{state}' is invalid. " +
                   $"Please input a proper state value (ex: \"{string.Join(", ", validStates)}\") for the label '{label}'.",
                   HttpStatusCode.BadRequest,
                   new List<string> { $"Label: {label}", $"State: {state}" });

            public static Error StreetAddressStateAndPostcodeMismatch(string label, string state, string postCode)
            {
                return new Error(
                    "person.street.address.postcode.and.state.mismatch",
                    "The postcode does not match the specified state",
                    $"The postcode \"{postCode}\" does not match the specified state \"{state}\" in the information provided for the address with the label \"{label}\". " +
                    $"To prevent this issue, please ensure that all postcode values match their corresponding state value.",
                    HttpStatusCode.BadRequest,
                    new List<string> { $"Label: {label}", $"State: {state}", $"Postcode: {postCode}" });
            }

            public static Error CustomLabelInvalid(string label, string context, string contextValue) =>
               new Error(
                   "person.label.invalid",
                   "The label value is invalid",
                   $"The label value '{label}' from {context} with value '{contextValue}' is invalid. " +
                    "The label must must only contain letters, numbers and spaces.",
                   HttpStatusCode.BadRequest,
                   new List<string> { $"Label: {label}" });

            public static Error MoreThanOneItemHasDefault(string context) =>
               new Error(
                   "person.multiple.default.contact.details",
                   $"Multiple contact details of same type were specified as default",
                   $"More than one {context} contact detail was specified as default. " +
                   $"If more than one contact detail of a certain type is provided, only one of them can be specified as default.",
                   HttpStatusCode.BadRequest,
                   null);

            public static Error CompanyNameInvalid(string value) =>
               new Error(
                   "person.company.name.invalid",
                   $"The company name is invalid",
                   $"The company name with value '{value}' is invalid. " +
                   $"Please try company name value that only contain letters, numbers and selected special characters e.g. '.?!&+%:$-@\\/(),.",
                   HttpStatusCode.BadRequest);

            public static Error NameInvalid(string nameType, string value)
            {
                var name = nameType.Humanize().ToLower();
                return new Error(
                    $"person.{name}.invalid",
                    $"The {name} is invalid",
                    $"The person {name} is invalid. " +
                    $"Person {name} must start with a letter, and may only contain letters, spaces, hyphens, apostrophes, commas and period characters.",
                    HttpStatusCode.BadRequest,
                    new List<string> { $"Name Type: {nameType.Humanize()}", $"Value: {value}" });
            }
        }
    }
}

// <copyright file="User.cs" company="uBind">
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
    using UBind.Domain.Permissions;

    /// <summary>
    /// Allows enumeration of all application errors as outlined here: https://enterprisecraftsmanship.com/posts/advanced-error-handling-techniques/.
    /// </summary>
    public static partial class Errors
    {
        /// <summary>
        /// User errors.
        /// </summary>
        public static class User
        {
            public static Error NotFound(Guid userId, IEnumerable<string> additionalDetails = null) =>
                new Error(
                    "user.not.found",
                    $"User not found",
                    $"When trying to find user '{userId}', nothing came up. Please ensure that you are passing the correct ID or contact customer support if you are experiencing this error in the portal.",
                    HttpStatusCode.NotFound,
                    additionalDetails,
                    new JObject()
                    {
                        { "userId", userId },
                    });

            public static Error AgentOrEmailNotFound(string agentEmail) =>
                new Error(
                    "agent.or.email.not.found",
                    $"Agent not found or email address is not associated with a user of type Agent",
                    $"When trying to find agent '{agentEmail}', either no agent was found with this email address, or the email address does not belong to a user of type agent. " +
                    $"Please ensure that you are passing the correct Agent Email or if you need further assistance, please don't hesitate to contact customer support.",
                    HttpStatusCode.NotFound,
                    null,
                    new JObject()
                    {
            { "agentEmail", agentEmail},
                    });

            public static Error CannotTransferToAnotherOrganisation(Guid userId, Guid organisationId) =>
                new Error(
                    "user.cannot.transfer.to.another.organisation",
                    $"Cannot transfer the user '{userId}' to another organisation",
                    $"Organisation with an Id '{organisationId}' must have at least one admin user. Before moving the user with an Id of '{userId}', first assign another admin to manage it.",
                    HttpStatusCode.Forbidden,
                    null,
                    new JObject()
                    {
                        { "userId", userId },
                        { "organisationId", organisationId },
                    });

            public static Error BelongsToTheSameOrganisation(Guid userId, Guid organisationId) =>
                new Error(
                    "user.belongs.to.the.same.organisation",
                    $"The user '{userId}' already belongs to the same organisation",
                    $"When trying to transfer the user to another organisation, the application won't allow it because the user with an Id '{userId}' already belongs to the organisation with an Id '{organisationId}'."
                    + "Please ensure that you are passing the correct organisation ID or if you need further assistance, please don't hesitate to contact customer support.",
                    HttpStatusCode.Forbidden,
                    null,
                    new JObject()
                    {
                        { "userId", userId },
                        { "organisationId", organisationId },
                    });

            public static Error CannotDisableOwnAccount() =>
                new Error(
                    "user.cannot.disable.own.account",
                    "Cannot disable own account",
                    $"You may not disable your own account. If you would like to have your account disabled, please contact an administrator, or contact customer support.",
                    HttpStatusCode.Forbidden);

            public static Error SessionExpiredDueToInactivity() =>
                new Error(
                    "user.session.expired.inactive",
                    "Please login again",
                    $"Your session has expired due to inactivity. Please login again to resume your session.",
                    HttpStatusCode.Unauthorized);

            public static Error SessionExpiredDueToMaximumPeriod() =>
                new Error(
                    "user.session.expired.maximum.period",
                    "Please login again",
                    $"You have been logged in for the maximum period in accordance with security settings. Please log in again to resume your session.",
                    HttpStatusCode.Unauthorized);

            public static Error SessionNotFound() =>
                new Error(
                    "user.session.not.found",
                    "Please login",
                    $"You are required to login to access this resource.",
                    HttpStatusCode.Unauthorized);

            public static Error CannotCreateUserForNonExistentCustomer() =>
                new Error(
                    "cannot.create.user.for.non.existent.customer",
                    "Cannot create user without customer",
                    "Cannot create a user for this quote when the customer has not yet been created.",
                    HttpStatusCode.PreconditionFailed);

            public static Error CannotCreateIncompleteDetails(string missingField) =>
                new Error(
                    "cannot.create.user.incomplete.details",
                    "You're missing something",
                    $"When trying to create a user, the \"{missingField}\" was missing.",
                    HttpStatusCode.BadRequest);

            public static Error CustomerEmailAddressInUseByAnotherUser(string emailAddress) =>
                new Error(
                    "customer.user.account.email.address.already.in.use",
                    "This customer account email address is already in use",
                    $"The customer account email address \"{emailAddress}\" is already used by another customer from the same organisation. "
                    + "To prevent this issue please ensure that the account email address for a new customer user account will be unique within the customer's organisation. "
                    + "Alternatively, if the reason for this issue is that the customer in question has a duplicate customer record, you may want to consider merging the two records instead.",
                    HttpStatusCode.Conflict);

            public static Error UserEmailAddressInUseByAnotherUser(string emailAddress) =>
                new Error(
                    "user.account.email.address.already.in.use",
                    "This user account email address is already in use",
                    $"The user account email address \"{emailAddress}\" is already used by another user from the same organisation. "
                    + "To prevent this issue please ensure that the account email address for a new user account will be unique within the new user's organisation. ",
                    HttpStatusCode.Conflict);

            public static Error PortalBelongsToDifferentOrganisation(Guid portalId, string portalName, Guid organisationId, string organisationName) =>
               new Error(
                   "user.portal.belongs.to.different.organisation",
                   "The portal belongs to a different organisation",
                   $"The \"{portalName}\" portal does not belong to the \"{organisationName}\" organisation. " +
                   "The portal must belong to the same organisation as the user.",
                   HttpStatusCode.BadRequest,
                   new List<string> { $"Portal ID: {portalId}", $"Organisation ID: {organisationId}" });

            public static Error PortalUserTypeMismatch(string portalName, string userType, string expectedUserType) =>
                new Error(
                    "user.portal.user.type.mismatch",
                    "The user type does not match the portal user type",
                    $"The \"{portalName}\" portal can only be assigned to \"{expectedUserType}\" users.",
                    HttpStatusCode.BadRequest,
                    new List<string> { $"Portal: {portalName}", $"User type: {userType}", $"Expected user type: {expectedUserType}" });

            public static Error LinkedIdentityProviderAlreadyExists(string loginEmail, Guid authenticationMethodId) => new Error(
                "user.linked.identity.provider.already.exists",
                "The linked identity provider already exists",
                $"The user \"{loginEmail} already has a linked identity provider with an ID of \"{authenticationMethodId}\". "
                    + "Please remove that linked identity and try again.",
                HttpStatusCode.Conflict,
                new List<string> {
                    $"Authentication method ID: {authenticationMethodId}",
                    $"Account Email: {loginEmail}",
                });

            public static Error CannotManageUserRolesLocallyDueToExclusiveManagement(
                string authenticationMethodName, string userDisplayName) => new Error(
                    "user.cannot.manage.roles.locally.due.to.exclusive.management",
                    "You can't unassign or assign roles to that user",
                    $"The user \"{userDisplayName}\" has a linked identity to the \"{authenticationMethodName}\" authentication method. "
                    + "Users of that authentication method have their roles managed exclusively by the remote identity provider.",
                    HttpStatusCode.MethodNotAllowed,
                    new List<string>
                    {
                        $"Authentication method name: {authenticationMethodName}",
                        $"User name: {userDisplayName}",
                    });

            public static class Authorisation
            {
                public static Error CannotModifyUserWithElevatedPermission(
                    string userDisplayName,
                    Permission elevatedPermission) =>
                    new Error(
                        "user.authorisation.cannot.modify.elevated.user",
                        "You're not allowed to modify that user",
                        $"You've tried to modify the user \"{userDisplayName}\" however you're not allowed to "
                        + " make changes to an elevated user account with the "
                        + elevatedPermission.Humanize() + " permission unless you also have that permission, "
                        + "which you do not.",
                        HttpStatusCode.Forbidden);
            }

            public static class Login
            {
                public static Error IncorrectCredentials() =>
                    new Error(
                        "user.login.incorrect.credentials",
                        "Incorrect credentials",
                        $"Your attempt to login was unsuccessful. Please check that you have entered the correct email address and password, and try again.",
                        HttpStatusCode.Unauthorized);

                public static Error AccountDisabled() =>
                    new Error(
                        "user.login.account.disabled",
                        "Account disabled",
                        $"Your account has been disabled by an administrator. Please contact an administrator to request for your account to be enabled.",
                        HttpStatusCode.Unauthorized);

                public static Error AccountLocked() =>
                    new Error(
                        "user.login.account.locked",
                        "Account locked",
                        $"Your account has been temporarily locked due to too many login attempts. Please try again in 30 minutes.",
                        HttpStatusCode.Unauthorized);
            }

            public static class PasswordExpiry
            {
                public static Error UserPasswordExpired(Guid userId, string emailAddress) =>
                    new Error(
                        "user.password.expired",
                        "Password Expired",
                        $"The password you're trying to use to login your account has expired."
                        + "Please reset your password.",
                        HttpStatusCode.Gone,
                        null,
                        new JObject()
                        {
                            { "userId", userId.ToString() },
                            { "emailAddress", emailAddress },
                        });
            }

            public static class ChangePassword
            {
                public static Error ReuseDetected() =>
                    new Error(
                        "user.changepassword.reuse.detected",
                        "Cannot reuse old password",
                        "For security reasons you are not able to reuse an old password. Please enter a password that you have not used before.",
                        HttpStatusCode.Conflict);

                public static Error TooSimple(IEnumerable<string> additionalDetails) =>
                    new Error(
                        "user.changepassword.too.simple",
                        "That password is totally hackable",
                        "You can't change your password to that. It doesn't meet the minimimum password complexity requirements. "
                        + "So that your account doesn't get hacked, please come up with something a bit more complex. "
                        + "Please try again with a different password.",
                        HttpStatusCode.NotAcceptable,
                        additionalDetails);
            }

            public static class InitialPassword
            {
                public static Error TooSimple(IEnumerable<string> additionalDetails) =>
                    new Error(
                        "user.initial.password.did.not.meet.minimum.requirements",
                        "Your password didn't meet the minimum requirements",
                        "You can't set your password to that. It doesn't meet the minimimum password complexity requirements. "
                        + "So that your account doesn't get hacked, please come up with something a bit more complex. "
                        + "Please try again with a different password.",
                        HttpStatusCode.NotAcceptable,
                        additionalDetails);
            }

            public static class Activation
            {
                public static Error UserNotFound(Guid userId) =>
                    new Error(
                        "user.activation.user.not.found",
                        "We couldn't find your user account",
                        $"When trying to activate your account, a user account with id {userId} was not found. Please check you are operating "
                        + "in the correct environment. If you're still having trouble please get in touch with support.",
                        HttpStatusCode.NotFound,
                        null,
                        new JObject()
                        {
                            { "UserId", userId.ToString() },
                        });

                public static Error AlreadyActive(Guid userId, Guid invitationId) =>
                    new Error(
                        "user.activation.already.activated",
                        "Account already activated",
                        $"You're trying to activate your account, but it's actually already been activated. "
                        + "If it wasn't you that activated your account, please report this to support immediately.",
                        HttpStatusCode.Gone,
                        null,
                        new JObject()
                        {
                            { "UserId", userId.ToString() },
                            { "InvitationId", invitationId.ToString() },
                        });

                public static Error UserBlocked(Guid userId) =>
                    new Error(
                        "user.activation.user.blocked",
                        "Your account has been deactivated",
                        $"When trying to activate your account, we found that your account has been deactivated. "
                        + "We apologise for the inconvenience. If you would like your account to be re-activated, please get in touch with support.",
                        HttpStatusCode.PreconditionFailed,
                        null,
                        new JObject()
                        {
                            { "UserId", userId.ToString() },
                        });

                public static Error TokenAlreadyUsed(Guid userId, Guid invitationId) =>
                    new Error(
                        "user.activation.token.already.used",
                        "Account already activated",
                        $"The token you're trying to use to activate your account has already been used. "
                        + "If it wasn't you that activated your account, please report this to support immediately.",
                        HttpStatusCode.Gone,
                        null,
                        new JObject()
                        {
                            { "UserId", userId.ToString() },
                            { "InvitationId", invitationId.ToString() },
                        });

                public static Error TokenNotFound(Guid userId, Guid invitationId) =>
                    new Error(
                        "user.activation.token.not.found",
                        "Activation token not found",
                        $"The token you're trying to use to activate your account appears to be invalid. "
                        + "Please verify you are trying to activate your account in the correct environment. "
                        + "If you're still having issues, please contact customer support.",
                        HttpStatusCode.NotFound,
                        null,
                        new JObject()
                        {
                            { "UserId", userId.ToString() },
                            { "InvitationId", invitationId.ToString() },
                        });

                public static Error TokenExpired(Guid userId, Guid invitationId) =>
                    new Error(
                        "user.activation.token.expired",
                        "You took too long",
                        $"The token you're trying to use to activate your account has expired. "
                        + "Please request a new one.",
                        HttpStatusCode.Gone,
                        null,
                        new JObject()
                        {
                            { "UserId", userId.ToString() },
                            { "InvitationId", invitationId.ToString() },
                        });

                public static Error PersonIdProvided(Guid personId, Guid invitationId) =>
                    new Error(
                        "user.activation.person.id.provided",
                        "Person id provided",
                        $"You have attempted to activate a user account but a Person ID was provided, instead of a User ID. " +
                        $"This is a product configuration issue. We apologise for the inconvenience. " +
                        $"Please report this to customer support so we can have this fixed.",
                        HttpStatusCode.BadRequest,
                        null,
                        new JObject()
                        {
                            { "PersonId", personId.ToString() },
                            { "InvitationId", invitationId.ToString() },
                        });

                public static Error CustomerIdProvided(Guid customerId, Guid invitationId) =>
                    new Error(
                        "user.activation.customer.id.provided",
                        "Customer id provided",
                        $"You have attempted to activate a user account but a Customer ID was provided, instead of a User ID. " +
                        $"This is a product configuration issue. " +
                        $"We apologise for the inconvenience. Please report this to customer support so we can have this fixed.",
                        HttpStatusCode.BadRequest,
                        null,
                        new JObject()
                        {
                            { "CustomerId", customerId.ToString() },
                            { "InvitationId", invitationId.ToString() },
                        });
            }

            public static class RequestResetPassword
            {
                public static Error UserNotFound(string emailAddress, string tenantAlias) =>
                    new Error(
                        "user.reset.password.user.not.found",
                        "We couldn't find your user account",
                        $"When trying to reset the password for a user with the email address \"{emailAddress}\" for tenant \"{tenantAlias}\", "
                        + "no user with that email address was found. No password reset invitiation will be sent.",
                        HttpStatusCode.NotFound,
                        null,
                        new JObject()
                        {
                            { "emailAddress", emailAddress },
                            { "tenantAlias", tenantAlias },
                        });

                public static Error UserDisabled(string emailAddress, string tenantAlias) =>
                    new Error(
                        "user.reset.password.user.disabled",
                        "Your account has been disabled",
                        $"When trying to reset the password for a user with the email address \"{emailAddress}\" for tenant \"{tenantAlias}\", "
                        + "only a disabled user with that email address was found. No password reset invitiation will be sent.",
                        HttpStatusCode.PreconditionFailed,
                        null,
                        new JObject()
                        {
                            { "emailAddress", emailAddress },
                            { "tenantAlias", tenantAlias },
                        });

                public static Error TooManyAttempts(
                    string emailAddress,
                    string tenantAlias,
                    int maxAttempts,
                    int periodMinutes) =>
                    new Error(
                    "user.reset.password.too.many.attempts",
                    "There have been too many reset password attempts",
                    $"When trying to reset the password for a user with the email address \"{emailAddress}\" for tenant \"{tenantAlias}\", "
                    + "there have been too many requests to reset your password. Please try again in 30 minutes.",
                    HttpStatusCode.PreconditionFailed,
                    null,
                    new JObject()
                    {
                        { "emailAddress", emailAddress },
                        { "tenantAlias", tenantAlias },
                        { "maxAttempts", maxAttempts },
                        { "periodMinutes", periodMinutes },
                    });
            }

            public static class ResetPassword
            {
                public static Error UserNotFound(Guid userId) =>
                    new Error(
                        "user.resetpassword.user.not.found",
                        "We couldn't find your user account",
                        $"When trying to reset your password, a user account with id {userId} was not found. Please check you are operating "
                        + "in the correct environment. If you're still having trouble please get in touch with support.",
                        HttpStatusCode.NotFound,
                        null,
                        new JObject()
                        {
                            { "userId", userId.ToString() },
                        });

                public static Error UserNotActivated(Guid userId) =>
                    new Error(
                        "user.resetpassword.user.not.activated",
                        "Your account hasn't been activated yet",
                        $"When trying to reset your password, we found that your account has not yet been activate. Please activate your account first. "
                        + "If you need to request another activation link to be sent to your email address, please get in touch with support.",
                        HttpStatusCode.PreconditionFailed,
                        null,
                        new JObject()
                        {
                            { "userId", userId.ToString() },
                        });

                public static Error UserBlocked(Guid userId) =>
                    new Error(
                        "user.resetpassword.user.blocked",
                        "Your account has been deactivated",
                        $"When trying to reset your password, we found that your account has been deactivated. "
                        + "We apologise for the inconvenience. If you would like your account to be re-activated, please get in touch with support.",
                        HttpStatusCode.PreconditionFailed,
                        null,
                        new JObject()
                        {
                            { "userId", userId.ToString() },
                        });

                public static Error TokenAlreadyUsed(Guid userId, Guid invitationId) =>
                    new Error(
                        "user.resetpassword.token.already.used",
                        "Password already reset",
                        $"The token you're trying to use to reset your password has already been used. "
                        + "You'll need to request another one. If you haven't used your reset password token yet then please report this to customer support.",
                        HttpStatusCode.Gone,
                        null,
                        new JObject()
                        {
                            { "userId", userId.ToString() },
                            { "invitationId", invitationId.ToString() },
                        });

                public static Error TokenSuperseded(Guid userId, Guid invitationId) =>
                    new Error(
                        "user.resetpassword.token.superseded",
                        "That's an old token",
                        $"The token you're trying to use to reset your password has been superseded by a newer one. "
                        + "Please find the most recent token and use that to reset your password.",
                        HttpStatusCode.Gone,
                        null,
                        new JObject()
                        {
                            { "userId", userId.ToString() },
                            { "invitationId", invitationId.ToString() },
                        });

                public static Error TokenNotFound(Guid userId, Guid invitationId) =>
                    new Error(
                        "user.resetpassword.token.not.found",
                        "Password reset token not found",
                        $"The token you're trying to use to reset your password appears to be invalid. "
                        + "Please verify you are trying to reset your password in the correct environment, "
                        + "with the same account you used to request it.",
                        HttpStatusCode.NotFound,
                        null,
                        new JObject()
                        {
                            { "userId", userId.ToString() },
                            { "invitationId", invitationId.ToString() },
                        });

                public static Error TokenExpired(Guid userId, Guid invitationId) =>
                    new Error(
                        "user.resetpassword.token.expired",
                        "You took too long",
                        $"The token you're trying to use to reset your password has expired. "
                        + "Please request a new one.",
                        HttpStatusCode.Gone,
                        null,
                        new JObject()
                        {
                            { "userId", userId.ToString() },
                            { "invitationId", invitationId.ToString() },
                        });
            }

            public static class Organisation
            {
                public static Error UnauthorizedForNonDefault(Guid? performingUserId) => new Error(
                    "organisation.unauthorised.for.non.default",
                    "Unauthorised access for non-default organisation.",
                    "You have tried to access a resource without the necessary permissions. If you think you should have access to this resource, please ask your administrator to grant you access.",
                    HttpStatusCode.Forbidden,
                    null,
                    new JObject()
                    {
                        { "performingUserId", performingUserId },
                    });
            }
        }
    }
}

// <copyright file="Quote.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

#pragma warning disable SA1600
#pragma warning disable SA1118 // Parameter should not span multiple lines

namespace UBind.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using Humanizer;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Helpers;
    using UBind.Domain.Product;

    /// <summary>
    /// Allows enumeration of all application errors as outlined here: https://enterprisecraftsmanship.com/posts/advanced-error-handling-techniques/.
    /// </summary>
    public static partial class Errors
    {
        /// <summary>
        /// Quote errors.
        /// </summary>
        public static class Quote
        {
            public static Error NotFound(Guid? quoteId) =>
                new Error(
                    "quote.not.found",
                    $"Quote not found",
                    $"When trying to find quote '{quoteId}', nothing came up. Please ensure that you are passing the correct ID or contact customer support if you are experiencing this error in the portal.",
                    HttpStatusCode.NotFound,
                    null,
                    new JObject()
                    {
                        { "quoteId", quoteId },
                    });

            public static Error NotFound(string quoteId) =>
                new Error(
                    "quote.not.found",
                    $"The quote could not be found",
                    $"We failed to retrieve a quote using quote ID '{quoteId}'. " +
                    $"Please ensure that you have provided the correct ID (GUID) for the quote you are trying to retrieve. " +
                    $"If you believe this error could be caused by a bug, please contact customer support.",
                    HttpStatusCode.NotFound,
                    null,
                    new JObject()
                    {
                        { "quoteId", quoteId },
                    });

            public static Error NotFound(Guid policyId, QuoteType quoteType) =>
                new Error(
                    "quote.of.type.not.found.for.policy",
                    $"The quote could not be found",
                    $"We couldn't find an active quote of type {quoteType.Humanize()} on the policy '{policyId}'. "
                    + $"It could be that the previous {quoteType.Humanize()} quote has since been discarded "
                    + $"in which case you may want to create a new {quoteType.Humanize()} quote against this policy. "
                    + $"If you believe this error could be caused by a bug, please contact customer support.",
                    HttpStatusCode.NotFound,
                    null,
                    new JObject()
                    {
                        { "policyId", policyId },
                        { "quoteType", quoteType.Humanize() },
                    });

            public static Error ProductMismatch(string productAlias, Guid quoteId) =>
                new Error(
                    "quote.product.mismatch",
                    $"Quote product mismatch",
                    $"When trying to find quote '{quoteId}' from '{productAlias}' product, nothing came up. " +
                    $"Please ensure that you are passing the correct ID or contact customer support " +
                    $"if you are experiencing this error in the portal.",
                    HttpStatusCode.NotFound,
                    null,
                    new JObject()
                    {
                        { "quoteId", quoteId },
                        { "productAlias", productAlias },
                    });

            public static Error QuestionChangedAfterApproved(Guid quoteId, List<string> additionalDetails) =>
                new Error(
                    "question.answer.changed.after.quote.approved",
                    "An approved answer was changed",
                    "When trying to update your quote, we found that your quote was approved with one or more answers to key questions. "
                    + "Since your quote was approved, changing these answers will require your new answers to also be approved. "
                    + "If you would like to keep these changes, you will need to go back and edit your quote so that it can be re-approved. "
                    + "We apologise for the inconvenience. If you need assistance please don't hesitate to get in touch with customer support.",
                    HttpStatusCode.Conflict,
                    additionalDetails,
                    new JObject()
                    {
                        { "quoteId", quoteId.ToString() },
                    });

            public static Error NewReferralTriggeredAfterQuoteApproved() =>
                new Error(
                    "new.referral.triggered.after.quote.approved",
                    "Referral rules updated",
                    "Since your quote was approved, the referral rules and restrictions have been changed. "
                    + "You will not be able to proceed with the approved quote. Please go back and edit your quote to seek approval based upon the new rules. "
                    + "We apologise for any inconvenience.",
                    HttpStatusCode.Conflict);

            public static Error CannotPerformOperationOnSubmittedQuote(string operationName) =>
                new Error(
                    "cannot.perform.operation.on.submitted.quote",
                    "Quote already submitted",
                    $"This quote has already been submitted so you cannot perform the operation '{operationName}' on it.",
                    HttpStatusCode.Conflict);

            public static Error CannotPerformOperationOnIssuedPolicy(string operationName, bool isMutual)
            {
                var code = "cannot.perform.operation.on.issued.policy";
                var title = "Policy already issued";
                var message = $"A policy has already been issued for this quote, so you cannot perform the operation '{operationName}' on it.";
                TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                return new Error(
                    code,
                    title,
                    message,
                    HttpStatusCode.Conflict);
            }

            public static Error CannotAssignQuoteNumberBeforeCustomerCreated() =>
                new Error(
                    "cannot.assign.quote.number.before.customer.created",
                    "Customer record needed",
                    "When trying to assign a quote number to this quote, we found that a customer record "
                    + "has not yet been created or associated with this quote. The workflow configuration for this product "
                    + "needs to be adjusted to create the customer record before assigning a quote number.",
                    HttpStatusCode.PreconditionFailed);

            public static Error CannotImportNascentQuote(string quoteNumber) =>
                new Error(
                    "cannot.import.nascent.quote",
                    "Cannot import a nascent quote.",
                    $"When trying to import the given quote {quoteNumber}, the specified state is set to nascent. Nascent quotes cannot be imported into the system",
                    HttpStatusCode.PreconditionFailed);

            public static Error QuoteNumberAlreadyAssigned(string existingQuoteNumber, string? newQuoteNumber = null) =>
                new Error(
                    "quote.number.already.assigned",
                    "Quote number already assigned",
                    "You can't " + (newQuoteNumber != null ? $"set the quote number on this quote to {newQuoteNumber}" : "assign a quote number to this quote") + $" because it already has the quote number {existingQuoteNumber}. "
                    + "Once a quote number has been assigned, it can't be changed.",
                    HttpStatusCode.PreconditionFailed,
                    null,
                    new JObject()
                    {
                        { "existingQuoteNumber", existingQuoteNumber },
                        { "newQuoteNumber", newQuoteNumber },
                    });

            public static Error AlreadySubmitted(Guid quoteId) =>
                new Error(
                    "quote.already.submitted",
                    "Quote already submitted",
                    $"Your attempt to submit this quote failed because it has already been submitted. A quote can't be submitted more than once.",
                    HttpStatusCode.PreconditionFailed,
                    null,
                    new JObject()
                    {
                        { "quoteId", quoteId.ToString() },
                    });

            public static Error SubmissionRequiresFormData(Guid quoteId) =>
                new Error(
                    "quote.submission.requires.form.data",
                    "There's no form data",
                    "Your attempt to submit this quote failed because it wasn't accompanied with form data. Please ensure that the form is filled out before attempting to submit a quote.",
                    HttpStatusCode.PreconditionFailed,
                    null,
                    new JObject()
                    {
                        { "quoteId", quoteId.ToString() },
                    });

            public static Error CannotIssuePolicyForIncompleteQuoteWithActiveTriggers() =>
                new Error(
                    "quote.issue.policy.incomplete.quote.has.active.triggers",
                    "The incomplete quote has active triggers",
                    "The issue policy operation is not allowed for incomplete quotes with active triggers. Please make sure the incomplete quote does not have any triggers.",
                    HttpStatusCode.BadRequest);

            public static Error CannotIssuePolicyForIncompleteQuoteWithNonBindingCalculation(string quoteStatus) =>
                new Error(
                    "quote.issue.policy.incomplete.quote.has.none.binding.calculation",
                    "The incomplete quote has none binding calculation",
                    $"Policy operation is not applicable in current quote state {quoteStatus} because the calculation result state is not \"bindingQuote\". " +
                    "Please ensure that the calculation result state is \"bindingQuote\"",
                    HttpStatusCode.BadRequest);

            public static Error CannotIssuePolicyForInvalidQuoteState(string quoteStatus) =>
                new Error(
                    "quote.issue.policy.invalid.quote.state",
                    "Unable to issue policy for an invalid quote state",
                    $"Policy operation is not applicable in current quote state {quoteStatus}. " +
                    "Please ensure that the quote is approved or incomplete with no active triggers and has a calculation result of \"bindingQuote\"",
                    HttpStatusCode.BadRequest);

            public static Error CannotIssueMultiplePolicies() =>
                new Error(
                    "quote.issue.policy.cannot.issue.multiple.policies",
                    "Unable to issue multiple policies",
                    $"The quote has a policy attached to issue another policy. " +
                    "Please ensure that the quote does not have a policy before trying to issue a policy.",
                    HttpStatusCode.BadRequest);

            public static Error CannotIssuePolicyWithoutFormData() =>
                new Error(
                    "quote.issue.policy.has.no.form.data",
                    "Unable to issue policy to a quote without form data",
                    $"The quote does not have from data to create a policy. " +
                    "Please ensure that the quote has form data before trying to issue a policy.",
                    HttpStatusCode.BadRequest);

            public static Error CannotIssuePolicyWithoutCalculationResult() =>
                new Error(
                    "quote.issue.policy.has.no.calculation.result",
                    "Unable to issue policy to a quote without a calculation result",
                    $"The quote has no caculation result. " +
                    "Please ensure that the quote has a calculation result before trying to issue a policy.",
                    HttpStatusCode.BadRequest);

            public static Error OperationNotPermittedForState(
                string operationName,
                string fromState,
                string? toState,
                List<string> allowedStates) =>
                new Error(
                    "quote.workflow.operation.not.permitted.for.state",
                    $"That operation is not permitted for the current quote state",
                    "The quote workflow is configured to allow certain operations when in certain states. "
                    + $"When trying to perform the operation \"{operationName}\" the state was \"{fromState}\", "
                    + $"however it is only allowed from the following states: {string.Join(", ", allowedStates)}. "
                    + (!string.IsNullOrWhiteSpace(toState) ? $"We were therefore unable to transition to the state \"{toState}\". " : string.Empty)
                    + "This is a product configuration issue, which can be resolved by either ensuring a prior "
                    + "transition has happened before performing this one, or configuring the workflow to allow "
                    + $"the \"{operationName}\" operation to be performed when the quote is in the \"{fromState}\" "
                    + "state.",
                    HttpStatusCode.PreconditionFailed,
                    null,
                    new JObject
                    {
                        { "operationName", operationName },
                        { "fromState", fromState },
                        { "toState", toState },
                        { "allowedStates", JArray.FromObject(allowedStates ?? new List<string>()) },
                    });

            public static Error OperationNotDefinedForWorkflow(
                string operationName,
                string currentState,
                List<string> definedTransitionActions) =>
                new Error(
                    "quote.workflow.operation.not.defined",
                    $"That operation has not been defined in the current workflow",
                    "The quote workflow is configured to allow certain operations when in certain states. "
                    + $"When trying to perform the operation \"{operationName}\", there was no transition with that "
                    + $"action/operation found. "
                    + "This is a product configuration issue, which can be resolved by either ensuring that the "
                    + "workflow configured for this product has all of the operations defined that might be used. ",
                    HttpStatusCode.PreconditionFailed,
                    new List<string>
                    {
                        "Defined transition actions: " + string.Join(", ", definedTransitionActions),
                        "Current workflow state: " + currentState,
                    },
                    new JObject
                    {
                        { "operationName", operationName },
                        { "currentState", currentState },
                        { "definedTransitionActions", JArray.FromObject(definedTransitionActions) },
                    });

            public static Error ResultingStateNotDefinedForWorkflow(
                string resultingState) =>
                new Error(
                    "quote.workflow.resulting.state.not.defined",
                    $"That resulting state has not been defined in the current workflow",
                    "The quote workflow is configured to allow certain operations when in certain states. "
                    + $"When trying to update the quote state, there was no resulting state \"{resultingState}\" defined."
                    + "To address this, it is necessary to ensure that the resulting state is supported by the product.",
                    HttpStatusCode.PreconditionFailed,
                    null,
                    new JObject
                    {
                        { "resultinState", resultingState },
                    });

            public static Error CannotBeApprovedWhenNotBindable(string calculationResultState, string expectedCalculationResultState) =>
                new Error(
                    "quote.cannot.be.approved.when.not.bindable",
                    "This quote can't be approved just yet",
                    $"You're trying to approve a quote which is not bindable, so it's not possible. "
                    + "Please ensure the quote has a valid calculation and no outstanding referral or decline triggers. "
                    + "Additionally, the workbook typically calculates the validity of question sets ratingPrimary, "
                    + "ratingSecondary, details and disclosure and requires them all to be valid before setting the "
                    + "calculation result state to \"bindingQuote\".",
                    HttpStatusCode.PreconditionFailed,
                    new string[]
                    {
                        $"The calculation result state was \"{calculationResultState}\", however it needs to be \"{expectedCalculationResultState}\" to be approved for binding.\"",
                    },
                    new JObject()
                    {
                        { "calculationResultState", calculationResultState },
                        { "expectedCalculationResultState", expectedCalculationResultState },
                    });

            public static Error CannotApproveQuoteWithoutCalculation(Guid quoteId) =>
                new Error(
                    "quote.cannot.be.approved.without.a.calculation",
                    "This quote can't be approved just yet",
                    $"You're trying to approve a quote which has not had a calculation done yet. "
                    + "Without a calculation having been performed, we cannot know whether there were any triggers "
                    + "that might stop this quote from being approved. "
                    + "This is likely a product misconfiguration. To resolve the issue, the product developer should configure "
                    + "a calculation to happen before triggering any approval operations.",
                    HttpStatusCode.PreconditionFailed,
                    null,
                    new JObject()
                    {
                        { "quoteId", quoteId.ToString() },
                    });

            public static Error CannotAutoApproveQuoteWithoutCalculation(Guid quoteId) =>
                new Error(
                    "quote.cannot.be.auto.approved.without.a.calculation",
                    "This quote can't be auto approved.",
                    $"You're trying to auto approve a quote which has not had a calculation done yet. "
                    + "Without a calculation having been performed, we cannot know whether there were any triggers "
                    + "that might stop this quote from being approved. "
                    + "This is likely a product misconfiguration. To resolve the issue, the product developer should configure "
                    + "a calculation to happen before triggering any approval operations.",
                    HttpStatusCode.PreconditionFailed,
                    null,
                    new JObject()
                    {
                         { "quoteId", quoteId.ToString() },
                    });

            public static Error CannotReferQuoteWithoutCalculation(Guid quoteId) =>
                new Error(
                    "quote.cannot.be.referred.without.a.calculation",
                    "This quote can't be referred just yet",
                    $"You're trying to refer a quote which has not had a calculation done yet. "
                    + "Without a calculation having been performed, we cannot know whether there were any triggers "
                    + "that might need to be approved. "
                    + "This is likely a product misconfiguration. To resolve the issue, the product developer should configure "
                    + "a calculation to happen before triggering any referral operations.",
                    HttpStatusCode.PreconditionFailed,
                    null,
                    new JObject()
                    {
                        { "quoteId", quoteId.ToString() },
                    });

            public static Error QuoteCalculationResultNotFound(Guid quoteId, Guid calculationId) =>
                    new Error(
                        "quote.calculation.cannot.be.found",
                        "The quote does not have a calculation result that matches the calculation Id in the request.",
                        "While trying to process your quote, we encountered a technical issue that prevents us from completing the transaction. "
                        + "We apologise for the inconvenience. "
                        + "Please do not retry this operation. Instead our team will need to complete this for you manually. Please get in touch with customer support.",
                        HttpStatusCode.NotFound,
                        new List<string>()
                        {
                                            $"Quote Id: {quoteId}",
                                            $"Calculation Id: {calculationId}",
                        });

            public static Error QuoteIdDoesNotExist(Guid quoteId) =>
                 new Error(
                     "quote.does.not.Exist",
                     "Unable to load the quote.",
                     $"The record for the quote with ID {quoteId} that you are trying to load does not exist.",
                     HttpStatusCode.NotFound,
                     null,
                     new JObject()
                     {
                        { "quoteId", quoteId.ToString() },
                     });

            public static Error CannotBeApprovedWithTriggers() =>
                new Error(
                    "quote.cannot.be.approved.with.triggers",
                    "You can't do that to the quote right now",
                    $"You're trying to approve a quote which is has outstanding referral triggers, so it's not possible. Please ensure the quote has no outstanding referral or decline triggers.",
                    HttpStatusCode.PreconditionFailed);

            public static Error CannotBeReferredForEndorsementWithoutTriggers() =>
                new Error(
                    "quote.cannot.be.referred.for.endorsement.without.triggers",
                    "You can't do that to the quote right now",
                    $"You're trying to refer a quote for endorsement but the quote doesn't have any referral triggers, so it's not possible. Please ensure the quote has at least one referral trigger.",
                    HttpStatusCode.PreconditionFailed);

            public static Error CannotReferWhenDeclined() =>
                new Error(
                    "quote.cannot.refer.when.declined",
                    "Can't refer quote when declined",
                    "The quote is currently in a declined state, so it's not possible to refer it.",
                    HttpStatusCode.Conflict);

            public static Error CannotUpdateWhenDiscarded() =>
                new Error(
                    "quote.cannot.update.when.discarded",
                    "This quote has been discarded",
                    "You can't continue to update this quote, because it's been discarded. Please open the replacement quote or start a new quote.",
                    HttpStatusCode.Gone);

            public static Error CannotUpdateWhenExpired(Guid quoteId) =>
                new Error(
                    "quote.cannot.update.when.expired",
                    "This quote has expired",
                    $"You can no longer update quote Id '{quoteId}', because it has expired. Please copy to a new quote or start a new quote.",
                    HttpStatusCode.Gone,
                    null,
                    new JObject()
                    {
                        { "quoteId", quoteId.ToString() },
                    });

            public static Error CannotEditWhenDiscarded(Guid quoteId) =>
                new Error(
                    "quote.cannot.edit.when.discarded",
                    "This quote has been discarded",
                    "You can no longer edit this quote because it's been discarded. Please open the replacement quote or start a new quote." +
                    " If you believe this is a mistake, or you would like assistance, please contact customer support.",
                    HttpStatusCode.Gone,
                    null,
                    new JObject()
                    {
                        { "quoteId", quoteId.ToString() },
                    });

            public static Error CannotLoadExpiredQuote(Guid quoteId) =>
                new Error(
                    "quote.cannot.load.expired.quote",
                    "This quote has expired",
                    "You can no longer edit this quote because it has expired. Please start a new quote." +
                    " If you believe this is a mistake, or you would like assistance, please contact customer support.",
                    HttpStatusCode.Gone,
                    null,
                    new JObject()
                    {
                        { "quoteId", quoteId.ToString() },
                    });

            public static Error CannotAssociateWithTheSameCustomerId(Guid customerId) =>
                new Error(
                    "quote.cannot.associate.same.customer",
                    "Cannot associate quote with the same customer.",
                    $"The customer associated with this quote is the same as the new requested customer with an ID '{customerId}'."
                    + " Please retry this operation with a different customer ID.",
                    HttpStatusCode.Conflict,
                    null,
                    new JObject()
                    {
                        { "CustomerId", customerId.ToString() },
                    });

            public static Error MismatchedCustomerId(Guid oldCustomerId, Guid newCustomerId) =>
                new Error(
                    "quote.has.mismatched.customer",
                    "Unable to update the customer.",
                    $"The customer associated with this quote has changed, so you can't update customer with ID "
                    + $"{oldCustomerId} when working with this quote. The new customer Id is {newCustomerId}. "
                    + "Please retry this operation with the new customer ID.",
                    HttpStatusCode.Conflict);

            public static Error NoCustomerAssociated(Guid quoteId, Guid customerId) =>
                new Error(
                    "quote.has.no.customer.associated",
                    "Customer update unsuccessful.",
                    $"There is no customer associated with the quote (ID: {quoteId}). As a result, the customer with ID " +
                    $"{customerId} cannot be updated. " +
                    "Please try the operation again with a quote that has a valid customer association.",
                    HttpStatusCode.Conflict);

            public static Error CustomerDetailsNotFound(Guid quoteId) =>
                new Error(
                    "quote.customer.details.not.found",
                    "Unable to find customer details.",
                    $"The customer details for quote with Id of {quoteId} was not found. Please update the customer details of this quote and retry this operation.",
                    HttpStatusCode.NotFound);

            public static Error WorkflowConfigurationNotFound(ReleaseContext releaseContext) =>
                new Error(
                    "quote.workflow.configuration.not.found",
                    "Quote workflow configuration not found",
                    "The workflow configuration was not found for this quote. "
                    + "Please ensure you have defined a product.json file with the object \"quoteWorkflow\". "
                    + "This is a product misconfiguration.",
                    HttpStatusCode.NotFound,
                    null,
                    JObject.FromObject(releaseContext));

            public static Error CannotCreateQuoteOfTypeWithoutPolicy(QuoteType quoteType) =>
                new Error(
                    "cannot.create.quote.of.type.without.policy",
                    $"You must specify a policy to create a {quoteType.Humanize()} quote against",
                    $"You have attempted to create a {quoteType.Humanize()} quote without specifying a"
                    + $"Policy ID, however you can only create a {quoteType.Humanize()} quote against "
                    + "an existing policy.",
                    HttpStatusCode.PreconditionFailed);

            public static Error CannotUnexpireQuoteThatsNotExpired(Guid quoteId) =>
                new Error(
                    "quote.cannot.revert.expiry.of.not.expired.quote",
                    "You cannot unexpire a quote that is not expired",
                    $"While updating the expiry date of the quote with ID {quoteId}, it was noted that this quote cannot be reverted "
                    + "to previous state as quote is not expired",
                    HttpStatusCode.PreconditionFailed);

            public static Error ReturnQuoteInvalidQuoteState(Guid quoteId, string quoteState) =>
                new Error(
                    "quote.return.invalid.quote.state",
                    "Quote cannot be returned because it is in an invalid state",
                    $"You are trying to return a quote that is in an invalid state. " +
                    "Please ensure that the quote you are trying to return is in \"review\", \"endorsement\" or \"approved\" state before trying again. ",
                    HttpStatusCode.BadRequest,
                    new List<string> { "Quote ID: " + quoteId, "Quote State: " + quoteState });

            public static class CreditNote
            {
                public static Error AlreadyIssued(Guid quoteId) =>
                    new Error(
                        "quote.credit.note.already.issued",
                        "Credit note already issued",
                        $"A credit note has already been issued for quote with ID {quoteId}, so we cannot issue another one.",
                        HttpStatusCode.PreconditionFailed);

                public static Error RequiresFormData() =>
                    new Error(
                        "quote.credit.note.requires.form.data",
                        "There's no form data",
                        "Your attempt to create a credit note for this quote failed because it wasn't accompanied with form data. Please ensure that the form is filled out before attempting to generate a credit note for a quote.",
                        HttpStatusCode.PreconditionFailed);
            }

            public static class AssociationInvitation
            {
                public static Error NotFound(Guid associationInvitationId) =>
                    new Error(
                        "quote.association.invitation.not.found",
                        "Association invitation not found",
                        $"An association invitation with id '{associationInvitationId}' was not found. Please check that you've defined an association invitation with the exact id.",
                        HttpStatusCode.NotFound);

                public static Error CustomerUserNotFound(Guid associationInvitationId) =>
                    new Error(
                        "quote.association.user.not.found",
                        "Customer user record not found",
                        $"You're trying to associate the customer to a quote with a given invitation id of {associationInvitationId}, however the customer user id is a non-existing record.");

                public static Error QuoteNotFound(Guid quoteId, Guid associationInvitationId) =>
                    new Error(
                        "quote.association.quote.not.found",
                        "Quote not found",
                        $"Quote with id '{quoteId}' was not found for association invitation of '{associationInvitationId}'. Please check that you've defined a quote with the exact id.");

                public static Error Expired(Guid associationInvitationId) =>
                    new Error(
                        "quote.association.invitation.expired",
                        "Quote association invitation has expired",
                        $"Your quote association invitation with id '{associationInvitationId}' expired has expired. You can ask either your client administrator or the system administrator for a new invitation to associate your quote with your user account.",
                        HttpStatusCode.Gone);
            }

            public static class AssociationWithCustomer
            {
                public static Error MismatchedTenant(
                    Guid quoteId, Guid quoteTenantId, Guid customerId, Guid customerTenantId)
                        => new Error(
                            "quote.association.with.customer.has.mismatched.tenant",
                            "Cannot associate quote with customer due to mismatched tenant",
                            $"When trying to associate the quote '{quoteId}' with the customer '{customerId}', we found that they do not belong to the same tenancy. "
                            + $"The quote belongs to tenant '{quoteTenantId}', meanwhile, the customer belongs to {customerTenantId}",
                            HttpStatusCode.NotFound,
                            null,
                            new JObject()
                            {
                                { "quoteId", quoteId },
                                { "quoteTenantId", quoteTenantId },
                                { "customerId", customerId },
                                { "customerTenantId", customerTenantId },
                            });

                public static Error MismatchedOrganisation(
                    Guid quoteId, Guid quoteOrganisationId, Guid customerId, Guid customerOrganisationId)
                        => new Error(
                            "quote.association.with.customer.has.mismatched.organisation",
                            "Cannot associate quote with customer due to mismatched organisation",
                            $"When trying to associate the quote '{quoteId}' with the customer '{customerId}', we found that they do not belong to the same organisation. "
                            + $"The quote belongs to organisation '{quoteOrganisationId}', meanwhile, the customer belongs to {customerOrganisationId}",
                            HttpStatusCode.NotFound,
                            null,
                            new JObject()
                            {
                                { "quoteId", quoteId },
                                { "quoteOrganisationId", quoteOrganisationId },
                                { "customerId", customerId },
                                { "customerOrganisationId", customerOrganisationId },
                            });

                public static Error MismatchedEnvironment(
                    Guid quoteId, DeploymentEnvironment quoteEnvironment, Guid customerId, DeploymentEnvironment customerEnvironment)
                        => new Error(
                            "quote.association.with.customer.has.mismatched.environment",
                            "Cannot associate quote with customer due to mismatched environment",
                            $"When trying to associate the quote '{quoteId}' with the customer '{customerId}', we found that they do not belong to the same environment. "
                            + $"The quote belongs to environment '{quoteEnvironment}', meanwhile, the customer belongs to {customerEnvironment}",
                            HttpStatusCode.NotFound,
                            null,
                            new JObject()
                            {
                                { "quoteId", quoteId },
                                { "quoteEnvironment", quoteEnvironment.Humanize() },
                                { "customerId", customerId },
                                { "customerEnvironment", customerEnvironment.Humanize() },
                            });
            }

            public static class Bind
            {
                public static Error BindCannotBeProcessedDueToConcurrency(Guid quoteId, bool isSettled) =>
                    new Error(
                        "quote.cannot.be.bound.due.to.system.errors",
                        "We were unable to complete the binding of your quote",
                        "While trying to complete your quote, we encountered a technical issue that prevents us from completing the transaction. " +
                        "We apologise for the inconvenience. " +
                        (isSettled
                        ? "We managed to successfully process your payment (or funding contract) however we couldn't complete the bind process. " +
                          "Please do not retry this operation. Instead our team will need to complete this for you manually. Please get in touch with customer support."
                        : "Please feel free to try again, or contact customer support for assistance"),
                        HttpStatusCode.Conflict,
                        new List<string>()
                        {
                            $"Quote Id: {quoteId}",
                            $"Is Settled: {isSettled}",
                        });
            }

            public static class Creation
            {
                public static Error OrganisationDisabled(string organisationId, string organisationAlias, string organisationName, JObject errorData) =>
                    new Error(
                        "quote.creation.organisation.disabled",
                        $"Cannot create a quote for disabled organisation",
                        $"When trying to create a quote for organisation \"{organisationName}\", the attempt failed because the specified organisation was disabled. "
                        + "To resolve this issue, please enable the specified organisation before you proceed. "
                        + "If you need further assistance please contact technical support.",
                        HttpStatusCode.BadRequest,
                        new List<string>
                        {
                            "Organisation ID: " + organisationId,
                            "Organisation Alias: " + organisationAlias,
                        },
                        errorData);

                public static Error CustomerMismatchWithOrganisation(
                    string specifiedOrganisationId,
                    string organisationName,
                    string customerId,
                    string customerDisplayName,
                    string customerOrganisationId,
                    string customerOrganisationName,
                    JObject? errorData) =>
                    new Error(
                        "quote.creation.customer.mismatch.with.organisation",
                        $"The specified organisation was different from the one associated with the specified customer",
                        $"When trying to create a new quote for a specific customer (\"{PersonInformationHelper.GetMaskedNameWithHashing(customerDisplayName)}\") and a specific organisation (\"{organisationName}\"), "
                        + $"the attempt failed because the specified organisation is different from the organisation associated with the specified customer (\"{customerOrganisationName}\"). "
                        + $"To resolve this issue, either omit the \"organisation\" parameter or ensure that the specified customer belongs to the specified organisation. "
                        + "If you need further assistance please contact technical support.",
                        HttpStatusCode.Conflict,
                        new List<string>
                            {
                                "Customer ID: " + customerId,
                                "Customer Organisation ID: " + customerOrganisationId,
                                "Specified Organisation ID: " + specifiedOrganisationId,
                            },
                        errorData);

                public static Error NewBusinessQuoteTypeDisabled(string productId, string productAlias, string productName, JObject errorData) =>
                    new Error(
                        "quote.creation.new.business.quote.type.disabled",
                        $"New business quotes are disabled for this product",
                        $"When trying to create a new business quote for the \"{productName}\" product, "
                        + $"the attempt failed because the product settings for \"{productName}\" prevent the creation of new business quotes. "
                        + $"To resolve this issue please enable new business quotes in the product settings for the \"{productName}\" product. "
                        + "If you need further assistance please contact technical support.",
                        HttpStatusCode.BadRequest,
                        new List<string>
                        {
                            "Product ID: " + productId,
                            "Product Alias: " + productAlias,
                        },
                        errorData);

                public static Error AdjustmentQuoteTypeDisabled(string productId, string productAlias, string productName, JObject errorData) =>
                    new Error(
                        "quote.creation.adjustment.quote.type.disabled",
                        $"Adjustment quotes are disabled for this product",
                        $"When trying to create an adjustment quote for the \"{productName}\" product, "
                        + $"the attempt failed because the product settings for \"{productName}\" prevent the creation of adjustment quotes. "
                        + $"To resolve this issue please enable adjustment quotes in the product settings for the \"{productName}\" product. "
                        + "If you need further assistance please contact technical support.",
                        HttpStatusCode.BadRequest,
                        new List<string>
                        {
                            "Product ID: " + productId,
                            "Product Alias: " + productAlias,
                        },
                        errorData);

                public static Error RenewalQuoteTypeDisabled(string productId, string productAlias, string productName, JObject errorData) =>
                    new Error(
                        "quote.creation.renewal.quote.type.disabled",
                        $"Renewal quotes are disabled for this product",
                        $"When trying to create a renewal quote for the \"{productName}\" product, "
                        + $"the attempt failed because the product settings for \"{productName}\" prevent the creation of renewal quotes. "
                        + $"To resolve this issue please enable renewal quotes in the product settings for the \"{productName}\" product. "
                        + "If you need further assistance please contact technical support.",
                        HttpStatusCode.BadRequest,
                        new List<string>
                        {
                            "Product ID: " + productId,
                            "Product Alias: " + productAlias,
                        },
                        errorData);

                public static Error CancellationQuoteTypeDisabled(string productId, string productAlias, string productName, JObject errorData) =>
                    new Error(
                        "quote.creation.cancellation.quote.type.disabled",
                        $"Cancellation quotes are disabled for this product",
                        $"When trying to create a cancellation quote for the \"{productName}\" product, "
                        + $"the attempt failed because the product settings for \"{productName}\" prevent the creation of cancellation quotes. "
                        + $"To resolve this issue please enable cancellation quotes in the product settings for the \"{productName}\" product. "
                        + "If you need further assistance please contact technical support.",
                        HttpStatusCode.BadRequest,
                        new List<string>
                        {
                            "Product ID: " + productId,
                            "Product Alias: " + productAlias,
                        },
                        errorData);

                public static Error ProductQuotesDisabledForOrganisation(
                    string organisationId, string organisationAlias, string organisationName, string productId, string productAlias, string productName, JObject errorData) =>
                    new Error(
                        "quote.creation.product.quotes.disabled.for.organisation",
                        $"Quotes for this product are disabled for the specified organisation",
                        $"When trying to create a new \"{productName}\" quote for the \"{organisationName}\" organisation, "
                        + $"the attempt failed because the organisation settings for \"{organisationName}\" did not allow the creation of new quotes for the \"{productName}\" product. "
                        + $"To resolve this issue, please ensure that \"{productName}\" quotes are enabled in the organisation settings for \"{organisationName}\". "
                        + "If you need further assistance please contact technical support.",
                        HttpStatusCode.BadRequest,
                        new List<string>
                            {
                                "Organisation ID: " + organisationId,
                                "Organisation Alias: " + organisationAlias,
                                "Product ID: " + productId,
                                "Product Alias: " + productAlias,
                            },
                        errorData);

                public static Error InitialQuoteStateInvalid(string productName, string initialQuoteState, JObject? errorData = null) =>
                    new Error(
                        "quote.creation.initial.quote.state.invalid",
                        $"The specified initial quote state is not permitted for this product",
                        $"When trying to create a quote with an initial state of \"{initialQuoteState.Camelize()}\", "
                        + $"the attempt failed because that initial quote state is not permitted by the workflow state machine for the \"{productName}\" product. "
                        + $"To resolve this issue please specify an initial quote state that is permitted by the workflow state machine for the specified product. "
                        + "If you need further assistance please contact technical support.",
                        HttpStatusCode.BadRequest,
                        Enumerable.Empty<string>(),
                        errorData);

                public static Error PolicyStatusInvalid(string policyId, string policyNumber, string policyState, string policyStatus, string? quoteType, JObject? errorData) =>
                    new Error(
                        "quote.creation.policy.status.invalid",
                        $"The specified policy had an invalid status",
                        $"When trying to create a new {quoteType?.Camelize()} quote for the policy \"{policyNumber}\", "
                        + $"the attempt failed because the specified policy has an invalid status (\"{policyStatus.Camelize()}\"). "
                        + $"To resolve this issue please ensure that the specified policy is active"
                        + $"{(!policyState.Equals("expired", StringComparison.OrdinalIgnoreCase) ? ". " : ", or that the product settings allow " + quoteType.Camelize() + " of expired policies. ")}"
                        + $"If you require further assistance please contact technical support.",
                        HttpStatusCode.BadRequest,
                        new List<string>
                            {
                                "Policy ID: " + policyId,
                            },
                        errorData);

                public static Error ProductMismatchWithPolicy(
                    string policyId, string policyProductId, string policyProductName, string policyNumber, string? quoteType, string specifiedProductId, string specifiedProductName, JObject? errorData) =>
                     new Error(
                        "quote.creation.product.mismatch.with.policy",
                        $"The specified product was different from the one associated with the specified policy",
                        $"When trying to create a new {quoteType?.Camelize()} quote for a specific policy (\"{policyNumber}\") and a specific product (\"{specifiedProductName}\"), "
                        + $"the attempt failed because the specified product is different from the product associated with the specified policy (\"{policyProductName}\")."
                        + "To resolve this issue, either omit the \"product\" parameter or specify the product associated with the specified policy. "
                        + "If you need further assistance please contact technical support. ",
                        HttpStatusCode.Conflict,
                        new List<string>
                        {
                            "Policy ID: " + policyId,
                            "Policy Product ID: " + policyProductId,
                            "Specified Product ID: " + specifiedProductId,
                        },
                        errorData);

                public static Error OrganisationMismatchWithPolicy(
                    string policyId, string policyOrganisationId, string policyOrganisationName, string policyNumber, string? quoteType, string specifiedOrganisationId, string specifiedOrganisationName, JObject? errorData) =>
                    new Error(
                        "quote.creation.organisation.mismatch.with.policy",
                        $"The specified organisation was different from the one associated with the specified policy",
                        $"When trying to create a new {quoteType?.Camelize()} quote for a specific policy (\"{policyNumber}\") and a specific organisation (\"{specifiedOrganisationName}\"), "
                        + $"the attempt failed because the specified organisation is different from the organisation associated with the specified policy (\"{policyOrganisationName}\"). "
                        + "To resolve this issue, either omit the \"organisation\" parameter or specify the organisation associated with the specified policy. "
                        + "If you need further assistance please contact technical support.",
                        HttpStatusCode.BadRequest,
                        new List<string>
                        {
                            "Policy ID: " + policyId,
                            "Policy Organisation ID: " + policyOrganisationId,
                            "Specified Organisation ID: " + specifiedOrganisationId,
                        },
                        errorData);

                public static Error CustomerMismatchWithPolicy(
                    string policyId, string? policyCustomerId, string policyCustomerDisplayName, string policyNumber, string? quoteType, string specifiedCustomerId, string specifiedCustomerDisplayName, JObject? errorData) =>
                    new Error(
                        "quote.creation.customer.mismatch.with.policy",
                        $"The specified customer was different from the one associated with the specified policy",
                        $"When trying to create a new {quoteType?.Camelize()} quote for a specific customer (\"{PersonInformationHelper.GetMaskedNameWithHashing(specifiedCustomerDisplayName)}\") and a specific policy (\"{policyNumber}\"), "
                        + $"the attempt failed because the specified customer is different from the one associated with the specified policy (\"{PersonInformationHelper.GetMaskedNameWithHashing(policyCustomerDisplayName)}\"). "
                        + "To resolve this issue, please ensure that the specified customer is the same as the one associated with the specified policy. "
                        + "If you require further assistance please contact technical support.",
                        HttpStatusCode.Conflict,
                        new List<string>
                        {
                            "Policy ID: " + policyId,
                            "Policy Customer ID: " + policyCustomerId,
                            "Specified Customer ID: " + specifiedCustomerId,
                        },
                        errorData);

                public static Error EnvironmentMismatchWithPolicy(
                    string policyId, string policyEnvironment, string policyNumber, string? quoteType, string specifiedEnvironment, JObject? errorData) =>
                    new Error(
                        "quote.creation.environment.mismatch.with.policy",
                        $"The specified environment was different from the one associated with the specified policy",
                        $"When trying to create a new {quoteType?.Camelize()} quote for a specific policy (\"{policyNumber}\") and a specific environment (\"{specifiedEnvironment.Camelize()}\"), "
                        + $"the attempt failed because the specified environment is different from the environment associated with the specified policy (\"{policyEnvironment.Camelize()}\"). "
                        + "To resolve this issue, either omit the \"environment\" parameter or specify the environment associated with the specified policy. "
                        + "If you need further assistance please contact technical support.",
                        HttpStatusCode.Conflict,
                        new List<string>
                        {
                            "Policy ID: " + policyId,
                            "Policy Environment: " + policyEnvironment.Camelize(),
                            "Specified Environment: " + specifiedEnvironment.Camelize(),
                        },
                        errorData);

                public static Error ProductDisabled(string productId, string productAlias, string productName, JObject errorData) =>
                    new Error(
                        "quote.creation.product.disabled",
                        $"Cannot create a quote for disabled product",
                        $"When trying to create a quote for product \"{productName}\", the attempt failed because the specified product was disabled. "
                        + "To resolve this issue, please enable the specified product before you proceed. If you need further assistance please contact technical support.",
                        HttpStatusCode.BadRequest,
                        new List<string>
                        {
                            "Product ID: " + productId,
                            "Product Alias: " + productAlias,
                        },
                        errorData);

                public static Error PolicyHasPendingTransaction(string policyId, string policyNumber, string pendingPolicyTransactionType, JObject? errorData) =>
                    new Error(
                        "quote.creation.policy.has.pending.transaction",
                        $"Cannot create a quote for a policy with a pending transaction",
                        $"When trying to create a new quote for the policy \"{policyNumber}\", the attempt failed because the policy in question has a pending {pendingPolicyTransactionType.Camelize()} transaction. "
                        + "To resolve this issue, please ensure that the specified policy has no pending transaction. If you need further assistance please contact technical support.",
                        HttpStatusCode.BadRequest,
                        new List<string>
                        {
                            "Policy ID: " + policyId,
                        },
                        errorData);

                public static Error EnvironmentMismatchWithCustomer(
                    string customerId,
                    string customerDisplayName,
                    string customerEnvironment,
                    string specifiedEnvironment,
                    JObject? errorData) =>
                    new Error(
                        "quote.creation.environment.mismatch.with.customer",
                        $"The specified environment was different from the one associated with the specified customer",
                        $"When trying to create a new quote for a specific customer (\"{PersonInformationHelper.GetMaskedNameWithHashing(customerDisplayName)}\") and a specific environment (\"{specifiedEnvironment.Camelize()}\"), "
                        + $"the attempt failed because the specified environment is different from the environment associated with the specified customer (\"{customerEnvironment.Camelize()}\"). "
                        + $"To resolve this issue, either omit the \"environment\" parameter or specify the environment associated with the specified customer. "
                        + "If you need further assistance please contact technical support.",
                        HttpStatusCode.Conflict,
                        new List<string>
                        {
                            "Customer ID: " + customerId,
                            "Customer Environment: " + customerEnvironment.Camelize(),
                            "Specified Environment: " + specifiedEnvironment.Camelize(),
                        },
                        errorData);
            }
        }
    }
}

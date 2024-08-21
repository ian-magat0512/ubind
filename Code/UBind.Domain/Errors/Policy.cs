// <copyright file="Policy.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

#pragma warning disable SA1600
#pragma warning disable SA1118 // Parameter should not span multiple lines

namespace UBind.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using Humanizer;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;

    /// <summary>
    /// Allows enumeration of all application errors as outlined here: https://enterprisecraftsmanship.com/posts/advanced-error-handling-techniques/.
    /// </summary>
    public static partial class Errors
    {
        /// <summary>
        /// Quote errors.
        /// </summary>
        public static class Policy
        {
            public static Error NotFound(Guid policyId) =>
                new Error(
                    "policy.id.not.found",
                    $"Policy ID not found",
                    $"When trying to identify a policy using a policy ID (GUID), the attempt failed because a policy with the specified ID could not be found."
                    + $" To resolve this issue, ensure that you have provided the correct policy ID. If you require further assistance please contact technical support.",
                    HttpStatusCode.NotFound,
                    new List<string>
                    {
                        { "Policy ID : " + policyId.ToString() },
                    });

            public static Error NotFoundUnderTenant(Guid policyId) =>
                new Error(
                    "policy.id.not.found",
                    $"Policy ID not found",
                    $"When trying to resolve a policy entity using a specified policy ID, the attempt failed because the specified ID could not be matched against one of the policies in this tenancy."
                    + $" To resolve this problem, please ensure that the specified policy ID matches a policy in this tenancy. If you require further assistance please contact technical support.",
                    HttpStatusCode.NotFound,
                    new List<string>
                    {
                        { "Policy ID : " + policyId.ToString() },
                    });

            public static Error CannotAssociateWithTheSameCustomerId(Guid customerId) =>
                new Error(
                    "policy.cannot.associate.same.customer",
                    "Cannot associate policy with the same customer.",
                    $"The customer associated with this policy is the same as the new requested customer with an ID '{customerId}'."
                    + " Please retry this operation with a different customer ID.",
                    HttpStatusCode.Conflict,
                    null,
                    new JObject()
                    {
                        { "CustomerId", customerId.ToString() },
                    });

            public static Error NoPermissionToAccessPolicy(Guid policyId, bool isMutual)
            {
                var code = "no.permission.access.policy";
                var title = "No permission to access Policy";
                var message = $"You tried to access the policy with ID \"{policyId}\", but it is not your policy or you do not have permission to access this policy. If you believe you should have access to this policy, please get in touch with your administrator, or contact customer support.";
                TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                return new Error(
                    code,
                    title,
                    message,
                    HttpStatusCode.Forbidden);
            }

            public static Error AggregateNotFound(Guid policyId) =>
                new Error(
                    "aggregate.not.found",
                    $"The aggregate was not found",
                    $"When trying to find the aggregate for the policy '{policyId}', nothing came up. This is a platform " +
                    $"issue. To resolve this, please contact the technical support.",
                    HttpStatusCode.NotFound,
                    null,
                    new JObject()
                    {
                        { "policyId", policyId },
                    });

            public static Error PolicyNumberAlreadyAssigned(string newPolicyNumber) =>
                new Error(
                    "policy.number.already.assigned",
                    "Policy number already assigned",
                    $"You can't set the policy number of this policy to {newPolicyNumber} "
                    + "because that number is already assigned to a different policy. ",
                    HttpStatusCode.PreconditionFailed,
                    null,
                    new JObject()
                    {
                        { "newPolicyNumber", newPolicyNumber },
                    });

            public static Error PolicyNotFoundForAggregate(Guid aggregateId) =>
                new Error(
                    "policy.not.found",
                    $"The policy was not found",
                    $"When trying to find the policy for the aggregate '{aggregateId}', nothing came up. This is a platform " +
                    $"issue. To resolve this, please contact the technical support.",
                    HttpStatusCode.NotFound,
                    null,
                    new JObject()
                    {
                                    { "aggregateId", aggregateId },
                    });

            public static class AssociationWithCustomer
            {
                public static Error MismatchedTenant(
                    Guid policyId, Guid policyTenantId, Guid customerId, Guid customerTenantId)
                        => new Error(
                            "policy.association.with.customer.has.mismatched.tenant",
                            "Cannot associate policy with customer due to mismatched tenant",
                            $"When trying to associate the policy '{policyId}' with the customer '{customerId}', we found that they do not belong to the same tenancy. "
                            + $"The policy belongs to tenant '{policyTenantId}', meanwhile, the customer belongs to {customerTenantId}",
                            HttpStatusCode.NotFound,
                            null,
                            new JObject()
                            {
                                { "policyId", policyId },
                                { "policyTenantId", policyTenantId },
                                { "customerId", customerId },
                                { "customerTenantId", customerTenantId },
                            });

                public static Error MismatchedOrganisation(
                    Guid policyId, Guid policyOrganisationId, Guid customerId, Guid customerOrganisationId)
                        => new Error(
                            "policy.association.with.customer.has.mismatched.organisation",
                            "Cannot associate policy with customer due to mismatched organisation",
                            $"When trying to associate the policy '{policyId}' with the customer '{customerId}', we found that they do not belong to the same organisation. "
                            + $"The policy belongs to organisation '{policyOrganisationId}', meanwhile, the customer belongs to {customerOrganisationId}",
                            HttpStatusCode.NotFound,
                            null,
                            new JObject()
                            {
                                { "policyId", policyId },
                                { "policyOrganisationId", policyOrganisationId },
                                { "customerId", customerId },
                                { "customerOrganisationId", customerOrganisationId },
                            });

                public static Error MismatchedEnvironment(
                    Guid policyId, DeploymentEnvironment policyEnvironment, Guid customerId, DeploymentEnvironment customerEnvironment)
                        => new Error(
                            "policy.association.with.customer.has.mismatched.environment",
                            "Cannot associate policy with customer due to mismatched environment",
                            $"When trying to associate the policy '{policyId}' with the customer '{customerId}', we found that they do not belong to the same environment. "
                            + $"The policy belongs to environment '{policyEnvironment}', meanwhile, the customer belongs to {customerEnvironment}",
                            HttpStatusCode.NotFound,
                            null,
                            new JObject()
                            {
                                { "policyId", policyId },
                                { "policyEnvironment", policyEnvironment.Humanize() },
                                { "customerId", customerId },
                                { "customerEnvironment", customerEnvironment.Humanize() },
                            });
            }

            public static class AssociationWithClaim
            {
                public static Error HasAssociation(Guid policyId) => new Error(
                    "policy.deletion.policy.associated.with.claim",
                    "Policy could not be deleted because it was associated with a claim",
                    $"When trying to delete a policy, the attempt failed because the specified policy was associated with one or more claims and the \"associatedClaimAction\" parameter was either omitted or set to \"error\"."
                    + $" To resolve this issue, either ensure that the policy you are trying to delete does not have any associated claims, or set a value for the \"associatedClaimAction\" parameter value to either \"disassociate\" or \"delete\"."
                    + $" If you require further assistance please contact technical support.",
                    HttpStatusCode.BadRequest,
                    new List<string>
                    {
                        { "Policy ID : " + policyId.ToString() },
                    });
            }

            public static class Issuance
            {
                public static Error AlreadyIssued(string policyNumber, bool isMutual)
                {
                    var code = "policy.already.issued";
                    var title = "Policy Already Issued";
                    var message = $"You've tried to issue a policy {policyNumber} which has already been issued. " +
                        $"You cannot issue multiple policies for a single application.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message.Replace("  ", " "),
                        HttpStatusCode.Conflict);
                }

                public static Error RequiresFormData(bool isMutual)
                {
                    var code = "policy.issuance.requires.form.data";
                    var title = "There's no form data";
                    var message = "Your attempt to issue the policy failed because it wasn't accompanied with form data. Please ensure that the form is filled out before attempting to issue a policy.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.PreconditionFailed);
                }

                public static Error RequiresCustomerData(bool isMutual)
                {
                    var code = "policy.issuance.requires.customer.data";
                    var title = "Customer record needed";
                    var message = "When trying to issue a policy, we found that a customer record "
                        + "has not yet been created or associated with this quote.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.PreconditionFailed);
                }

                public static Error RequiresCalculation(Guid quoteId) =>
                    new Error(
                        "policy.issuance.requires.calculation",
                        "Calculation needed",
                        $"You're trying to issue a policy for a quote which has not had a calculation done yet. " +
                        $"Please ensure that calculation is done before attempting to issue a policy.",
                        HttpStatusCode.PreconditionFailed,
                        null,
                        new JObject()
                        {
                            { "quoteId", quoteId.ToString() },
                        });

                public static Error RequiresApprovedOrIncompleteQuoteState(Guid quoteId) =>
                    new Error(
                        "policy.issuance.quote.state.invalid",
                        "Quote does not have a valid state",
                        $"You're trying to issue a policy for a quote that does not have a valid state. " +
                        $"Please ensure that the specified quote is in state \"approved\" or \"incomplete\".",
                        HttpStatusCode.PreconditionFailed,
                        null,
                        new JObject()
                        {
                            { "quoteId", quoteId.ToString() },
                        });

                public static Error CustomerMismatch(Guid quoteId, Guid? quoteCustomerId, Guid customerId) =>
                    new Error(
                        "policy.issuance.customer.mismatch.with.quote",
                        "The specified customer is different from the specified quote customer",
                        $"The specified customer is different from the customer associated with the quote. " +
                        $"Please ensure that the customer provided matches the quote customer.",
                        HttpStatusCode.PreconditionFailed,
                        new List<string>
                        {
                            { "Quote ID : " + quoteId.ToString() },
                            { "Quote Customer ID : " + quoteCustomerId.ToString() },
                            { "Customer ID : " + customerId.ToString() },
                        },
                        new JObject()
                        {
                            { "quoteId", quoteId.ToString() },
                            { "quoteCustomerid", quoteCustomerId.ToString() },
                            { "customerid", customerId.ToString() },
                        });

                public static Error OrganisationMismatch(Guid quoteId, Guid quoteOrganisationId, Guid organisationId) =>
                    new Error(
                        "policy.issuance.organisation.mismatch.with.quote",
                        "The specified organisation is different from the specified quote organisation",
                        $"The specified organisation is different from the organisation associated with the quote. " +
                        $"Please ensure that the organisation provided matches the quote organisation.",
                        HttpStatusCode.PreconditionFailed,
                        new List<string>
                        {
                            { "Quote ID : " + quoteId.ToString() },
                            { "Quote Organisation ID : " + quoteOrganisationId.ToString() },
                            { "Organisation ID : " + organisationId.ToString() },
                        },
                        new JObject()
                        {
                            { "quoteId", quoteId.ToString() },
                            { "quoteOrganisationId", quoteOrganisationId.ToString() },
                            { "organisationId", organisationId.ToString() },
                        });

                public static Error ProductMismatch(Guid quoteId, Guid quoteProductId, Guid productId) =>
                    new Error(
                        "policy.issuance.product.mismatch.with.quote",
                        "The specified product is different from the specified quote product",
                        $"The specified product is different from the product associated with the quote. " +
                        $"Please ensure that the product provided matches the quote product.",
                        HttpStatusCode.PreconditionFailed,
                        new List<string>
                        {
                            { "Quote ID : " + quoteId.ToString() },
                            { "Quote Product ID : " + quoteProductId.ToString() },
                            { "Product ID : " + productId.ToString() },
                        },
                        new JObject()
                        {
                            { "quoteId", quoteId.ToString() },
                            { "quoteProductId", quoteProductId.ToString() },
                            { "productId", productId.ToString() },
                        });

                public static Error EnvironmentMismatch(Guid quoteId, DeploymentEnvironment quoteEnvironment, DeploymentEnvironment environment) =>
                    new Error(
                        "policy.issuance.environment.mismatch.with.quote",
                        "The specified environment is different from the specified quote environment",
                        $"The specified environment is different from the environment associated with the quote. " +
                        $"Please ensure that the environment provided matches the quote environment.",
                        HttpStatusCode.PreconditionFailed,
                        new List<string>
                        {
                            { "Quote ID : " + quoteId.ToString() },
                            { "Quote Environment : " + quoteEnvironment.ToString() },
                            { "Environment : " + environment.ToString() },
                        },
                        new JObject()
                        {
                            { "quoteId", quoteId.ToString() },
                            { "quoteEnvironment", quoteEnvironment.Humanize() },
                            { "environment", environment.Humanize() },
                        });

                public static Error TestDataMismatch(Guid quoteId, bool quoteTestData, bool testData) =>
                    new Error(
                        "policy.issuance.test.data.mismatch.with.quote",
                        "The specified test data value is different from the specified quote test data value",
                        $"The specified test data value is different from the test data value associated with the quote. " +
                        $"Please ensure that the test data value provided matches the quote environment.",
                        HttpStatusCode.PreconditionFailed,
                        new List<string>
                        {
                            { "Quote ID : " + quoteId.ToString() },
                            { "Quote Test Data : " + quoteTestData.ToString() },
                            { "Test Data : " + testData.ToString() },
                        },
                        new JObject()
                        {
                            { "quoteId", quoteId.ToString() },
                            { "quoteTestData", quoteTestData },
                            { "testData", testData },
                        });

                public static Error RequiresBindableCalculationResult(string calcResultState) =>
                    new Error(
                        "policy.issuance.requires.bindable.calculation.result",
                        "Calculation needs to be in bindable state",
                        $"You're trying to issue a policy for a quote with calculation state '{calcResultState}'. " +
                        $"Please ensure that calculation state is 'bindingQuote' before attempting to issue a policy.",
                        HttpStatusCode.PreconditionFailed,
                        null,
                        new JObject()
                        {
                            { "Calculation State", calcResultState },
                        });

                public static Error IncompleteQuoteRequiresBindableCalculationResult(string calcResultState, bool hasNewFormData) =>
                    new Error(
                        "policy.issuance.quote.calculation.state.invalid",
                        "Calculation needs to be in bindable state",
                        $"You're trying to issue a policy for an 'incomplete' quote with calculation state '{calcResultState}'. " +
                        $"Please ensure that calculation state is 'bindingQuote'{(hasNewFormData ? string.Empty : ", and a new set of input data was not provided to perform a new calculation")} before attempting to issue a policy.",
                        HttpStatusCode.PreconditionFailed,
                        null,
                        new JObject()
                        {
                            { "Calculation State", calcResultState },
                        });

                public static Error InputDataRequiresBindingCalculation(string calcResultState) =>
                    new Error(
                        "policy.issuance.quote.calculation.state.invalid",
                        "Calculation needs to be in bindable state",
                        $"You're trying to issue a policy for an inputData that results to a calculation with calculation state '{calcResultState}'. " +
                        $"Please ensure that the input data can result to a calculation with state 'bindingQuote'.",
                        HttpStatusCode.PreconditionFailed,
                        null,
                        new JObject()
                        {
                            { "Calculation State", calcResultState },
                        });

                public static Error RequiresApprovedState(string quoteState) =>
                    new Error(
                        "policy.issuance.requires.approved.or.incomplete.state",
                        "Quote needs to be in approved or incomplete state",
                        $"You're trying to issue a policy for a quote in '{quoteState.ToLower()}' state. " +
                        $"Please ensure that the quote is in 'approved' or 'incomplete' state before attempting to issue a policy.",
                        HttpStatusCode.PreconditionFailed,
                        null,
                        new JObject()
                        {
                            { "Quote State", quoteState },
                        });

                public static Error IncompleteQuoteHasActiveTriggers() =>
                    new Error(
                        "policy.issuance.quote.calculation.triggers.active",
                        "Incomplete quote has active triggers",
                        $"You're trying to issue a policy for a quote in 'Incomplete' state that has active triggers. " +
                        $"Please ensure that the quote has no active triggers before attempting to issue a policy.",
                        HttpStatusCode.PreconditionFailed,
                        null,
                        new JObject()
                        {
                            { "Quote State", "Incomplete" },
                        });

                public static Error InputDataHasActiveTriggers() =>
                    new Error(
                        "policy.issuance.quote.calculation.triggers.active",
                        "Input data has active triggers",
                        $"The calculation performed using the provided input data resulted in one or more active triggers. " +
                        $"Please ensure that the input data does not have any active triggers.",
                        HttpStatusCode.PreconditionFailed,
                        null,
                        new JObject()
                        {
                        });

                public static Error PaymentRequired() =>
                    new Error(
                        "policy.issuance.payment.required",
                        "Payment is required prior to issuance of policy",
                        $"The operation requires payment to be settled prior to issuance of policy",
                        HttpStatusCode.PaymentRequired);

                public static Error InvalidStateDetected(string quoteState) =>
                    new Error(
                        "policy.issuance.invalid.state",
                        "Invalid state detected",
                        $"The quote state is '{quoteState.ToLower()}', " +
                        $"it must be in 'approved' state before any transactions can be executed.",
                        HttpStatusCode.BadRequest,
                        null,
                        new JObject()
                        {
                            { "Quote State", quoteState },
                        });

                public static Error InceptionDateShouldPrecedeExpiryDate(LocalDate? inceptionDate, LocalDate? expiryDate) =>
                    new Error(
                        "inception.date.should.precede.expiry.date",
                        "Inception date should precede expiry date",
                        $"You're trying to issue a policy for a quote which has an inception date after the expiry. " +
                        $"Please ensure that inception date precedes the expiry date before attempting to issue a policy.",
                        HttpStatusCode.PreconditionFailed,
                        null,
                        new JObject()
                        {
                            { "inceptionDate", inceptionDate.ToString() },
                            { "expiryDate", expiryDate.ToString() },
                        });

                public static Error RequiresNewBusinessQuote(Guid quoteId, string quoteType) =>
                    new Error(
                        "policy.issuance.requires.new.business.quote",
                        "New business quote required",
                        $"Your attempt to issue the policy failed " +
                        $"because a new policy can only be issued in relation to a new business quote. " +
                        $"Please ensure that you're using a new business quote to issue a policy.",
                        HttpStatusCode.PreconditionFailed,
                        null,
                        new JObject()
                        {
                            { "quoteId", quoteId.ToString() },
                            { "quoteType", quoteType.ToString() },
                        });

                public static Error PolicyAlreadyIssued() =>
                    new Error(
                        "policy.already.issued",
                        "Policy already issued",
                        "A policy has already been issued for this quote. Only one policy can be issued from a quote.",
                        HttpStatusCode.Conflict);

                public static Error PolicyIssuancePolicyNumberNotUnique(DeploymentEnvironment environment, string productName, string policyNumber) =>
                    new Error(
                        "policy.issuance.policy.number.not.unique",
                        "The specified policy number is already in use",
                        $"When trying to issue a new policy for the {productName} product with the custom policy number {policyNumber}, " +
                        "the policy could not be issued because the specified policy number is already in used by another policy in the same " +
                        $"product environment ({environment}). To resolve this issue please ensure that the specified policy number is unique " +
                        $"with the {environment} environment. If you require further assistance please contact technical support.",
                        HttpStatusCode.BadRequest);

                public static Error PolicyIssuanceQuoteAlreadyHasAPolicy(string quoteReference) =>
                    new Error(
                        "policy.issuance.quote.already.has.policy",
                        "The specified quote already has a policy",
                        $"When trying to issue a policy in relation to the quote with reference {quoteReference}, " +
                        "a policy could not be issued because a policy has already been issued in relation to the specified quote. " +
                        "To resolve this issue please ensure that the quote you are trying to issue a policy in relation to does not already " +
                        "have a policy associated with it. If you require further assistance please contact technical support.",
                        HttpStatusCode.Conflict);
            }

            public static class Adjustment
            {
                public static Error ExpiredAdjustmentQuoteExists(Guid quoteId, Instant createdTimestamp, string policyNumber, bool isMutual)
                {
                    var code = "policy.expired.adjustment.quote.already.exists";
                    var title = "Existing policy adjustment quote has expired";
                    var message = $"An adjustment quote for the policy {policyNumber} has already expired. " +
                        "To start an adjustment, you must discard your expired adjustment quote first.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.Conflict,
                        null,
                        new JObject()
                        {
                            { "quoteId", quoteId.ToString() },
                            { "createdDateTime", createdTimestamp.ToExtendedIso8601String() },
                            { "policyNumber", policyNumber },
                        });
                }

                public static Error ManualApprovalRequired() =>
                new Error(
                    "adjustment.quote.manual.approval.required",
                    "Manual approval required",
                    $"The refund settings for this product require that cancellation quotes must go through a manual approval process when a refund may be applicable. Please ensure that a review or endorsement is triggered for adjustment quotes that result in a refund. This is a product configuration" +
                    $" issue which a product developer must fix.",
                    HttpStatusCode.PreconditionFailed);

                public static Error AdjustmentQuoteExists(Guid quoteId, Instant createdTimestamp, string policyNumber, bool isMutual)
                {
                    var code = "policy.adjustment.quote.already.exists";
                    var title = "Policy adjustment in progress";
                    var message = $"An adjustment quote for the policy {policyNumber} has already been started, but never completed. "
                        + "You may resume the existing adjustment quote, or if you would like to start again, you must cancel the existing adjustment quote first.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.Conflict,
                        null,
                        new JObject()
                        {
                            { "quoteId", quoteId.ToString() },
                            { "createdDateTime", createdTimestamp.ToExtendedIso8601String() },
                            { "policyNumber", policyNumber },
                        });
                }

                public static Error ExpiredRenewalQuoteExists(Guid quoteId, Instant createdTimestamp, string policyNumber, bool isMutual)
                {
                    var code = "policy.expired.renewal.quote.exists.when.adjusting";
                    var title = "Existing policy renewal quote has expired";
                    var message = $"A renewal quote for the policy {policyNumber} has already expired. " +
                        "To start an adjustment, you must discard your expired renewal quote first.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.Conflict,
                        null,
                        new JObject()
                        {
                                { "quoteId", quoteId.ToString() },
                                { "createdDateTime", createdTimestamp.ToExtendedIso8601String() },
                                { "policyNumber", policyNumber },
                        });
                }

                public static Error RenewalQuoteExists(Guid quoteId, Instant createdTimestamp, string policyNumber, bool isMutual)
                {
                    var code = "policy.renewal.quote.exists.when.adjusting";
                    var title = "Policy renewal in progress";
                    var message = $"A renewal quote for the policy {policyNumber} has already been started, but never completed. "
                        + "To start an adjustment, you must discard your renewal quote first.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.Conflict,
                        null,
                        new JObject()
                        {
                            { "quoteId", quoteId.ToString() },
                            { "createdDateTime", createdTimestamp.ToExtendedIso8601String() },
                            { "policyNumber", policyNumber },
                        });
                }

                public static Error ExpiredCancellationQuoteExists(Guid quoteId, Instant createdTimestamp, string policyNumber, bool isMutual)
                {
                    var code = "policy.expired.cancellation.quote.exists.when.adjusting";
                    var title = "Existing policy cancellation quote has expired";
                    var message = $"A cancellation quote for the policy {policyNumber} has already expired. " +
                         "To start an adjustment, you must discard your expired cancelation quote first.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.Conflict,
                        null,
                        new JObject()
                        {
                            { "quoteId", quoteId.ToString() },
                            { "createdDateTime", createdTimestamp.ToExtendedIso8601String() },
                            { "policyNumber", policyNumber },
                        });
                }

                public static Error CancellationQuoteExists(Guid quoteId, Instant createdTimestamp, string policyNumber, bool isMutual)
                {
                    var code = "policy.cancellation.quote.exists.when.adjusting";
                    var title = "Policy renewal in progress";
                    var message = $"A cancellation quote for the policy {policyNumber} has already been started, but never completed. "
                        + "To start an adjustment, you must discard your cancellation quote first.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.Conflict,
                        null,
                        new JObject()
                        {
                            { "quoteId", quoteId.ToString() },
                            { "createdDateTime", createdTimestamp.ToExtendedIso8601String() },
                            { "policyNumber", policyNumber },
                        });
                }

                public static Error PolicyHasExpired(string policyNumber, Instant expiryTimestamp, Instant adjustmentTimestamp, bool isMutual)
                {
                    var code = "policy.adjustment.policy.has.expired";
                    var title = "You can't adjust an expired policy";
                    var message = $"The policy {policyNumber} expired at {expiryTimestamp} and so "
                        + $"cannot be adjusted.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.Conflict,
                        null,
                        new JObject()
                        {
                            { "expiryDateTime", expiryTimestamp.ToExtendedIso8601String() },
                            { "adjustmentDateTime", adjustmentTimestamp.ToExtendedIso8601String() },
                            { "policyNumber", policyNumber },
                        });
                }

                public static Error PolicyHasBeenCancelled(
                    string policyNumber,
                    Instant cancellationEffectiveTimestamp,
                    Instant adjustmentTimestamp,
                    bool isMutual)
                {
                    var code = "policy.adjustment.policy.has.been.cancelled";
                    var title = "Can't adjust a cancelled quote";
                    var message = $"The policy {policyNumber} was cancelled, effective {cancellationEffectiveTimestamp}. "
                        + "You cannot adjust a policy which has been cancelled.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.Conflict,
                        null,
                        new JObject()
                        {
                            { "cancellationEffectiveDateTime", cancellationEffectiveTimestamp.ToExtendedIso8601String() },
                            { "adjustmentDateTime", adjustmentTimestamp.ToExtendedIso8601String() },
                            { "policyNumber", policyNumber },
                        });
                }

                public static Error DatesMustMatch(string policyNumber, LocalDate originalInceptionDate, LocalDate originalExpiryDate, LocalDate adjustmentInceptionDate, LocalDate adjustmentExpiryDate, bool isMutual)
                {
                    var code = "policy.adjustment.dates.must.match";
                    var title = "Policy dates must match";
                    var message = $"The policy {policyNumber} would be cancelled at the time of the adjustment. Please ensure your adjustment is effective before cancellation.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.Conflict,
                        null,
                        new JObject()
                        {
                            { "policyNumber", policyNumber },
                            { "originalInceptionDate", originalInceptionDate.ToIso8601() },
                            { "originalExpiryDate", originalExpiryDate.ToIso8601() },
                            { "adjustmentInceptionDate", adjustmentInceptionDate.ToIso8601() },
                            { "adjustmentExpiryDate", adjustmentExpiryDate.ToIso8601() },
                        });
                }

                public static Error PolicyAdjustmentExpiryDateMustNotBeBeforePolicyPeriodStarted(
                    string policyNumber,
                    LocalDateTime policyPeriodStartDateTime,
                    LocalDateTime adjustmentExpiryDateTime,
                    bool isMutual)
                {
                    var code = "policy.adjustment.expiry.date.must.not.be.before.policy.period.started";
                    var title = "Invalid policy adjustment expiry date";
                    var message = $"When trying to adjust the policy {policyNumber}, the expiry date of the "
                        + "adjustment was set to a date before the current policy period started. "
                        + "This is not allowed. If you'd like to cancel the policy, pleae create a cancellation quote.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.Conflict,
                        null,
                        new JObject()
                        {
                            { "policyNumber", policyNumber },
                            { "policyPeriodStartDateTime", policyPeriodStartDateTime.ToIso8601() },
                            { "adjustmentExpiryDateTime", adjustmentExpiryDateTime.ToIso8601() },
                        });
                }

                public static Error PolicyAdjustmentEffectiveDateMustNotBeBeforePolicyPeriodStarted(
                    string policyNumber,
                    LocalDateTime policyPeriodStartDateTime,
                    LocalDateTime adjustmentEffectoveDateTime,
                    bool isMutual)
                {
                    var code = "policy.adjustment.effective.date.must.not.be.before.policy.period.started";
                    var title = "Invalid policy adjustment effective date";
                    var message = $"When trying to adjust the policy {policyNumber}, the effective date of the "
                        + "adjustment was set to a date before the policy period started. You cannot make changes "
                        + "to a previous completed policy period.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.Conflict,
                        null,
                        new JObject()
                        {
                            { "policyNumber", policyNumber },
                            { "policyPeriodStartDateTime", policyPeriodStartDateTime.ToIso8601() },
                            { "adjustmentEffectoveDateTime", adjustmentEffectoveDateTime.ToIso8601() },
                        });
                }

                public static Error PolicyAdjustmentEffectiveDateMustNotBeAfterPolicyPeriodEnded(
                    string policyNumber,
                    LocalDateTime policyPeriodEndDateTime,
                    LocalDateTime adjustmentEffectoveDateTime,
                    bool isMutual)
                {
                    var code = "policy.adjustment.effective.date.must.not.be.after.policy.period.ended";
                    var title = "Invalid policy adjustment effective date";
                    var message = $"When trying to adjust the policy {policyNumber}, the effective date of the "
                        + "adjustment was set to a date after the policy period ended.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.Conflict,
                        null,
                        new JObject()
                        {
                            { "policyNumber", policyNumber },
                            { "policyPeriodEndDate", policyPeriodEndDateTime.ToIso8601() },
                            { "adjustmentEffectoveDate", adjustmentEffectoveDateTime.ToIso8601() },
                        });
                }

                public static Error PolicyCancellationEffectiveDateMustNotBeBeforePolicyPeriodStarted(
                    string policyNumber,
                    LocalDateTime policyPeriodStartDateTime,
                    LocalDateTime cancellationEffectiveDateTime,
                    bool isMutual)
                {
                    var code = "policy.cancellation.effective.date.must.not.be.before.policy.period.started";
                    var title = "Invalid policy cancellation effective date";
                    var message = $"When trying to cancel the policy {policyNumber}, the effective date of the "
                        + "cancellation was set to a date before the policy period started. You cannot make changes "
                        + "to a previous completed policy period.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.Conflict,
                        null,
                        new JObject()
                        {
                            { "policyNumber", policyNumber },
                            { "policyPeriodStartDateTime", policyPeriodStartDateTime.ToIso8601() },
                            { "cancellationEffectiveDateTime", cancellationEffectiveDateTime.ToIso8601() },
                        });
                }

                public static Error PolicyCancellationEffectiveDateMustNotBeAfterPolicyPeriodEnded(
                    string policyNumber,
                    LocalDateTime policyPeriodEndDateTime,
                    LocalDateTime cancellationEffectiveDateTime,
                    bool isMutual)
                {
                    var code = "policy.cancellation.effective.date.must.not.be.after.policy.period.ended";
                    var title = "Invalid policy cancellation effective date";
                    var message = $"When trying to cancel the policy {policyNumber}, the effective date of the "
                        + "cancellation was set to a date after the policy period ended.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.Conflict,
                        null,
                        new JObject()
                        {
                            { "policyNumber", policyNumber },
                            { "policyPeriodEndDateTime", policyPeriodEndDateTime.ToIso8601() },
                            { "cancellationEffectiveDateTime", cancellationEffectiveDateTime.ToIso8601() },
                        });
                }

                public static Error NoPolicyExists(bool isMutual)
                {
                    var title = "There's no policy to adjust";
                    var message = "You requested to start an adjustment quote but there's no policy to adjust.";
                    title = TenantHelper.CheckAndChangeTextToMutual(title, isMutual);
                    message = TenantHelper.CheckAndChangeTextToMutual(message, isMutual);
                    return new Error(
                        "policy.adjustment.no.policy.exists",
                        title,
                        message,
                        HttpStatusCode.PreconditionFailed);
                }
            }

            public static class Renewal
            {
                public static Error ExpiredRenewalQuoteExists(Guid quoteId, Instant createdTimestamp, string policyNumber, bool isMutual)
                {
                    var code = "policy.expired.renewal.quote.already.exists";
                    var title = "Existing policy renewal quote has expired";
                    var message = $"A renewal quote for the policy {policyNumber} has already expired. " +
                           "To start a renewal, you must discard your expired renewal quote first.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                           code,
                           title,
                           message,
                           HttpStatusCode.Conflict,
                           null,
                           new JObject()
                           {
                            { "quoteId", quoteId.ToString() },
                            { "createdDateTime", createdTimestamp.ToExtendedIso8601String() },
                            { "policyNumber", policyNumber },
                           });
                }

                public static Error SendingRenewalInviteNotWithIn60DaysAfterExpiryNotAllowed(string policyNumber, bool isMutual)
                {
                    var code = "policy.cannot.send.renewal.invitation";
                    var title = "Cannot send renewal invitation";
                    var message = $"You can't send a renewal invitation for {policyNumber} just yet. You can only send " +
                        $"renewal invites for active policy that will expire within 60 days.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);

                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.PreconditionFailed);
                }

                public static Error SendingExpiredPolicyRenewalInviteNotAllowed(string policyNumber, bool isMutual, bool allowRenewalOfExpiredPolicies, int numberOfDaysToExpire, int allowableDaysToRenewAfterExpiry)
                {
                    var code = "expired.policy.not.allowed.for.sending.renewal";
                    var title = "Cannot send renewal invitation for expired policy";
                    var message = $"You can't send a renewal invitation for {policyNumber}. The policy is already expired and the allow renewal of expired policies option is disabled or the expiry date of the policy does not fall within the allowed renewal period.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);

                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.PreconditionFailed,
                        null,
                        new JObject()
                        {
                            { "policyNumber", policyNumber },
                            { "allowRenewalOfExpiredPolicies", allowRenewalOfExpiredPolicies.ToString() },
                            { "numberOfDaysToExpire", numberOfDaysToExpire.ToString() },
                            { "allowableDaysToRenewAfterExpiry", allowableDaysToRenewAfterExpiry.ToString() },
                        });
                }

                public static Error ExpiredPolicyRenewalNotAllowed(string policyNumber)
                {
                    var code = "expired.policy.renewal.not.allowed";
                    var title = "Expired policy renewal is not allowed";
                    var message = $"The policy with Policy Number {policyNumber} has expired and the allow renewal of expired policies option is not enabled or the " +
                        "date of the Policy did not fall within the allowed renewal period."
                          + " If you believe this is a mistake, or you would like assistance, please contact customer support.";
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.Conflict,
                        null,
                        new JObject()
                        {
                            { "policyNumber", policyNumber },
                        });
                }

                public static Error InvalidEffectiveDate(LocalDate previousExpiryDate, LocalDate renewalEffectiveDate)
                {
                    var code = "invalid.policy.start.date";
                    var title = "Invalid policy start date";
                    var message = $"This renewal quote cannot be bound because the period start date is invalid. For renewal quotes, " +
                        $"the effective date must be set to the expiry date of the previous term.";
                    var additionalDetails = new List<string>();
                    additionalDetails.Add($"Previous expiry date: {previousExpiryDate.ToIso8601()}");
                    additionalDetails.Add($"Renewal effective date: {renewalEffectiveDate.ToIso8601()}");
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.Conflict,
                        additionalDetails,
                        new JObject()
                        {
                            { "previousExpiryDate", previousExpiryDate.ToIso8601() },
                            { "renewalEffectiveDate", renewalEffectiveDate.ToIso8601() },
                        });
                }

                public static Error RenewalQuoteExists(Guid quoteId, Instant createdTimestamp, string policyNumber, bool isMutual)
                {
                    var code = "policy.renewal.quote.already.exists";
                    var title = "Policy renewal in progress";
                    var message = $"A renewal quote for the policy {policyNumber} has already been started, but never completed. "
                        + "You may resume the existing renewal quote, or if you would like to start again, you must cancel the existing renewal quote first.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.Conflict,
                        null,
                        new JObject()
                        {
                            { "quoteId", quoteId.ToString() },
                            { "createdDateTime", createdTimestamp.ToExtendedIso8601String() },
                            { "policyNumber", policyNumber },
                        });
                }

                public static Error ExpiredAdjustmentQuoteExists(Guid quoteId, Instant createdTimestamp, string policyNumber, bool isMutual)
                {
                    var code = "policy.expired.adjustment.quote.exists.when.renewing";
                    var title = "Existing policy adjustment quote has expired";
                    var message = $"An adjustment quote for the policy {policyNumber} has already expired. " +
                        "To start a renewal, you must discard your expired adjustment quote first.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.Conflict,
                        null,
                        new JObject()
                        {
                            { "quoteId", quoteId.ToString() },
                            { "createdDateTime", createdTimestamp.ToExtendedIso8601String() },
                            { "policyNumber", policyNumber },
                        });
                }

                public static Error AdjustmentQuoteExists(Guid quoteId, Instant createdTimestamp, string policyNumber, bool isMutual)
                {
                    var code = "policy.adjustment.quote.exists.when.renewing";
                    var title = "Policy adjustment in progress";
                    var message = $"An adjustment quote for the policy {policyNumber} has already been started, but never completed. "
                        + "To start a renewal, you must discard your adjustment quote first.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.Conflict,
                        null,
                        new JObject()
                        {
                            { "quoteId", quoteId.ToString() },
                            { "createdDateTime", createdTimestamp.ToExtendedIso8601String() },
                            { "policyNumber", policyNumber },
                        });
                }

                public static Error ExpiredCancellationQuoteExists(Guid quoteId, Instant createdTimestamp, string policyNumber, bool isMutual)
                {
                    var code = "policy.expired.cancellation.quote.exists.when.renewing";
                    var title = "Existing policy cancellation quote has expired";
                    var message = $"An cancellation quote for the policy {policyNumber} has already expired. "
                        + "To start a renewal, you must discard your expired cancellation quote first.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.Conflict,
                        null,
                        new JObject()
                        {
                            { "quoteId", quoteId.ToString() },
                            { "createdDateTime", createdTimestamp.ToExtendedIso8601String() },
                            { "policyNumber", policyNumber },
                        });
                }

                public static Error CancellationQuoteExists(Guid quoteId, Instant createdTimestamp, string policyNumber, bool isMutual)
                {
                    var code = "policy.cancellation.quote.exists.when.renewing";
                    var title = "Policy cancellation in progress";
                    var message = $"An cancellation quote for the policy {policyNumber} has already been started, but never completed. "
                        + "To start a renewal, you must discard your cancellation quote first.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.Conflict,
                        null,
                        new JObject()
                        {
                            { "quoteId", quoteId.ToString() },
                            { "createdDateTime", createdTimestamp.ToExtendedIso8601String() },
                            { "policyNumber", policyNumber },
                        });
                }

                public static Error RenewalEffectiveDateTimeMustMatchExpiry(
                    string policyNumber,
                    LocalDateTime expiryDateTime,
                    Instant expiryTimestamp,
                    LocalDateTime effectiveDateTime,
                    Instant effectiveTimestamp,
                    bool isMutual)
                {
                    var code = "policy.renewal.date.time.must.match.expiry";
                    var title = "Policy renewal date must match expiry date";
                    var message = $"When renewing policy {policyNumber}, the previous policy expiry date and time need to match the renewal effective date and time.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.Conflict,
                        null,
                        new JObject()
                        {
                            { "policyNumber", policyNumber },
                            { "expiryDateTime", expiryDateTime.ToIso8601() },
                            { "expiryTimestamp", expiryTimestamp.ToExtendedIso8601String() },
                            { "effectiveDateTime", effectiveDateTime.ToIso8601() },
                            { "effectiveTimestamp", effectiveTimestamp.ToExtendedIso8601String() },
                        });
                }

                public static Error RenewalEndDateTimeMustNotBeBeforePreviousPolicyPeriodEndDate(
                    string policyNumber,
                    LocalDateTime previousExpiryDateTime,
                    Instant previousExpiryTimestamp,
                    LocalDateTime newExpiryDateTime,
                    Instant newExpiryTimestamp,
                    bool isMutual)
                {
                    var code = "policy.renewal.end.date.must.not.be.before.previous.policy.period.end.date";
                    var title = "Policy renewal end date invalid";
                    var message = $"When renewing policy {policyNumber}, the selected end date for the new policy "
                        + "period is before the previous policy period's expiry date. "
                        + $"A policy can't end before it starts. Please ensure the end date for the new policy period "
                        + "Is after the previous policy period's start date.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.Conflict,
                        null,
                        new JObject()
                        {
                            { "policyNumber", policyNumber },
                            { "previousExpiryDateTime", previousExpiryDateTime.ToIso8601() },
                            { "previousExpiryTimestamp", previousExpiryTimestamp.ToExtendedIso8601String() },
                            { "newExpiryDateTime", newExpiryDateTime.ToIso8601() },
                            { "newExpiryTimestamp", newExpiryTimestamp.ToExtendedIso8601String() },
                        });
                }

                public static Error NoPolicyExists(bool isMutual)
                {
                    var code = "policy.renewal.no.policy.exists";
                    var title = "There's no policy to renew";
                    var message = "You requested to start a renewal quote but there's no policy to renew.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.PreconditionFailed);
                }

                public static Error CannotSendRenewalInvitationYet(string policyNumber, bool isMutual)
                {
                    var code = "policy.cannot.send.renewal.invitation";
                    var title = "Cannot send renewal invitation";
                    var message = $"You can't send a renewal invitation for {policyNumber} just yet. You can only send renewal invites for active policy that will expire within 60 days.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);

                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.PreconditionFailed);
                }

                public static Error CannotSendRenewalInvitationWithoutCustomer(string policyNumber) =>
                    new Error(
                        "policy.cannot.send.renewal.invitation.without.customer",
                        "Cannot send renewal invitation without customer",
                        "You'll need to create a proper customer record for this policy before you can send "
                        + "a renewal invitation to them. We apologise for the inconvenience. Please get in touch "
                        + "with customer support so we can assist you with this.",
                        HttpStatusCode.PreconditionFailed,
                        null,
                        new JObject
                        {
                            { "policyNumber", policyNumber },
                        });

                public static Error UserNotFound(Guid userId, bool isMutual)
                {
                    var code = "We couldn't find your user account";
                    code = TenantHelper.CheckAndChangeTextToMutual(code, isMutual);
                    return new Error(
                        code,
                        "We couldn't find your user account",
                        $"When trying to reset your password, a user account with id {userId} was not found. Please check you are operating "
                        + "in the correct environment. If you're still having trouble please get in touch with support.",
                        HttpStatusCode.NotFound,
                        null,
                        new JObject()
                        {
                            { "UserId", userId.ToString() },
                        });
                }
            }

            public static class Cancellation
            {
                public static Error ExpiredRenewalQuoteExists(Guid quoteId, Instant createdTimestamp, string policyNumber, bool isMutual)
                {
                    var code = "policy.expired.renewal.quote.already.exists.when.cancelling";
                    var title = "Existing policy renewal quote has expired";
                    var message = $"A renewal quote for the policy {policyNumber} has already expired. " +
                        "To start a cancellation, you must discard your expired renewal quote first.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.Conflict,
                        null,
                        new JObject()
                        {
                            { "quoteId", quoteId.ToString() },
                            { "createdDateTime", createdTimestamp.ToExtendedIso8601String() },
                            { "policyNumber", policyNumber },
                        });
                }

                public static Error ManualApprovalRequired() =>
                    new Error(
                        "cancellation.quote.manual.approval.required",
                        "Manual approval required",
                        $"The refund settings for this product require that cancellation quotes must go through a manual approval process when a refund may be applicable. Please ensure that a review or endorsement is always triggered for cancellation quotes that result in a refund. This is a product" +
                        $" configuration issue which a product developer must fix.",
                        HttpStatusCode.PreconditionFailed);

                public static Error RenewalQuoteExists(Guid quoteId, Instant createdTimestamp, string policyNumber, bool isMutual)
                {
                    var code = "policy.renewal.quote.already.exists.when.cancelling";
                    var title = "Policy renewal in progress";
                    var message = $"A renewal quote for the policy {policyNumber} has already been started, but never completed. "
                        + "To start a cancellation, you must discard your renewal quote first.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.Conflict,
                        null,
                        new JObject()
                        {
                          { "quoteId", quoteId.ToString() },
                          { "createdDateTime", createdTimestamp.ToExtendedIso8601String() },
                          { "policyNumber", policyNumber },
                        });
                }

                public static Error ExpiredAdjustmentQuoteExists(Guid quoteId, Instant createdTimestamp, string policyNumber, bool isMutual)
                {
                    var code = "policy.expired.adjustment.quote.exists.when.cancelling";
                    var title = "Existing policy adjustment quote has expired";
                    var message = $"An adjustment quote for the policy {policyNumber} has already expired. " +
                        "To start a cancellation, you must discard your expired adjustment quote first.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.Conflict,
                        null,
                        new JObject()
                        {
                            { "quoteId", quoteId.ToString() },
                            { "createdDateTime", createdTimestamp.ToExtendedIso8601String() },
                            { "policyNumber", policyNumber },
                        });
                }

                public static Error AdjustmentQuoteExists(Guid quoteId, Instant createdTimestamp, string policyNumber, bool isMutual)
                {
                    var code = "policy.adjustment.quote.exists.when.cancelling";
                    var title = "Policy adjustment in progress";
                    var message = $"An adjustment quote for the policy {policyNumber} has already been started, but never completed. "
                        + "To start a cancellation, you must discard your adjustment quote first.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.Conflict,
                        null,
                        new JObject()
                        {
                            { "quoteId", quoteId.ToString() },
                            { "createdDateTime", createdTimestamp.ToExtendedIso8601String() },
                            { "policyNumber", policyNumber },
                        });
                }

                public static Error ExpiredCancellationQuoteExists(Guid quoteId, Instant createdTimestamp, string policyNumber, bool isMutual)
                {
                    var code = "policy.expired.cancellation.quote.already.exists";
                    var title = "Existing policy cancellation quote has expired";
                    var message = $"A cancellation quote for the policy {policyNumber} has already expired. " +
                        "To start a cancellation, you must discard your expired cancellation quote first.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);

                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.Conflict,
                        null,
                        new JObject()
                        {
                        { "quoteId", quoteId.ToString() },
                        { "createdDateTime", createdTimestamp.ToExtendedIso8601String() },
                        { "policyNumber", policyNumber },
                        });
                }

                public static Error CancellationQuoteExists(Guid quoteId, Instant createdTimestamp, string policyNumber, bool isMutual)
                {
                    var code = "policy.cancellation.quote.already.exists";
                    var title = "Policy cancellation in progress";
                    var message = $"A cancellation quote for the policy {policyNumber} has already been started, but never completed. "
                        + "To start a cancellation, you must discard your existing cancellation quote first.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.Conflict,
                        null,
                        new JObject()
                        {
                            { "quoteId", quoteId.ToString() },
                            { "createdDateTime", createdTimestamp.ToExtendedIso8601String() },
                            { "policyNumber", policyNumber },
                        });
                }

                public static Error AlreadyCancelled(bool isMutual)
                {
                    var code = "policy.cancellation.already.cancelled";
                    var title = "It's already cancelled";
                    var message = $"The policy you're trying to cancel is already cancelled.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.PreconditionFailed);
                }

                public static Error NotPendingOrActive(PolicyStatus policyStatus, bool isMutual)
                {
                    var code = "policy.cancellation.not.pending.or.active";
                    var title = "You can't cancel this policy";
                    var message = $"The policy you're trying to cancel has the status {policyStatus}, "
                        + "however you can only cancel policies with status Pending or Active.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.PreconditionFailed,
                        null,
                        new JObject()
                        {
                            { "policyStatus", policyStatus.ToString() },
                        });
                }

                public static Error NoPolicyExists(bool isMutual)
                {
                    var code = "policy.cancellation.no.policy.exists";
                    var title = "There's no policy to cancel";
                    var message = "Your request to cancel the policy was received, but no policy exists yet.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.NotFound);
                }

                public static Error PendingAdjustment(bool isMutual)
                {
                    var code = "policy.cancellation.pending.adjustment";
                    var title = "There's a pending adjustment";
                    var message = "Since there's a pending adjustment of this policy, it can't be cancelled at this time. "
                        + "If you still need to cancel, please get in touch with customer support.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.Conflict);
                }

                public static Error PendingRenewal(bool isMutual)
                {
                    var code = "policy.cancellation.pending.renewal";
                    var title = "There's a pending renewal";
                    var message = "Since there's a pending renewal of this policy, it can't be cancelled at this time. "
                        + "If you still need to cancel, please get in touch with customer support.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.Conflict);
                }

                public static Error PendingCancellation(bool isMutual)
                {
                    var code = "policy.cancellation.pending.cancellation";
                    var title = "There's already a pending cancellation";
                    var message = "Since there's already a pending cancellation of this policy, it can't be cancelled at this time. "
                        + "If you need to alter the pending cancellation, please get in touch with customer support.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        code,
                        title,
                        message,
                        HttpStatusCode.Conflict);
                }
            }

            public static class Transaction
            {
                public static Error MustHaveEffectiveDateOrTimestamp(string quoteNumber, Guid transactionId, Guid policyId)
                    => new Error(
                        "policy.transaction.must.have.effective.date.or.timestamp",
                        "The policy transaction must have an effecive date or timestamp",
                        $"When trying to create a policy transaction from quote \"{quoteNumber}\", "
                        + "there was no effective date or effective timestamp provided. "
                        + "We apologise for the inconvenience. Please get in touch with customer support so we "
                        + "resolve this issue.",
                        HttpStatusCode.BadRequest,
                        null,
                        new JObject
                        {
                                { "policyId", policyId },
                                { "transactionId", transactionId },
                                { "quoteNumber", quoteNumber },
                        });

                public static Error NotFound(Guid transactionId) =>
                    new Error(
                        "policy.transaction.not.found",
                        $"The policy transaction was not found",
                        $"When trying to find the policy transaction '{transactionId}' for a policy, nothing came up. This is a platform " +
                        $"issue. To resolve this, please contact the technical support.",
                        HttpStatusCode.NotFound,
                        null,
                        new JObject()
                        {
                                        { "policyTransactionId", transactionId },
                        });

                public static Error NotFoundForPolicy(Guid policyId) =>
                    new Error(
                        "policy.transaction.not.found",
                        $"The policy transaction was not found",
                        $"When trying to find the policy transaction for a policy '{policyId}', nothing came up. This is a platform " +
                        $"issue. To resolve this, please contact the technical support.",
                        HttpStatusCode.NotFound,
                        null,
                        new JObject()
                        {
                            { "policyId", policyId },
                        });
            }

            public static class DatePatching
            {
                public static Error NoPolicyUpsertEventFound(string policyId, string dateName, JObject errorData)
                    => new Error(
                        "policy.date.patching.upsert.event.not.found",
                        "Quote aggregate did not contain a policy upsert event",
                        $"When trying to patch the \"${dateName}\" date property in the aggregate of the "
                        + $"policy \"{policyId}\", the attempt failed because the aggregate did not contain "
                        + "any policy upsert event. To resolve this issue, please contact technical support.",
                        HttpStatusCode.NotFound,
                        null,
                        errorData);

                public static Error RequiredUpsertEventForInceptionDatePatchingNotFound(string policyId, JObject errorData)
                    => new Error(
                        "policy.inception.date.patching.required.upsert.event.not.found",
                        "Quote aggregate did not contain the required policy upsert event",
                        $"When trying to patch the \"inceptionDate\" date property in the aggregate of the "
                        + $"policy \"{policyId}\", the attempt failed because the aggregate did not contain the "
                        + "required policy upsert event (\"PolicyIssuedEvent\" or \"PolicyIssuedWithoutQuoteEvent\"). " +
                        "To resolve this issue, please contact technical support.",
                        HttpStatusCode.NotFound,
                        null,
                        errorData);

                public static Error InvalidDateName(string dateName, JObject errorData)
                    => new Error(
                        "policy.date.patching.date.name.invalid",
                        "Date name is invalid",
                        $"When trying to patch the \"{dateName}\" date property in the aggregate of the "
                        + $"policy, the attempt failed because the provided date name is invalid. To resolve "
                        + "this issue, please ensure that the provided date name is valid (\"inceptionDate\", "
                        + "\"effectiveDate\" or \"expiryDate\"). If you need further assistance, please contact technical "
                        + "support",
                        HttpStatusCode.PreconditionFailed,
                        null,
                        errorData);

                public static Error PropertyNotFound(string policyId, string dateName, JObject errorData)
                    => new Error(
                        "policy.date.patching.property.not.found",
                        "Date name not found",
                        $"When trying to patch the \"{dateName}\" date property in the aggregate of the "
                        + $"policy \"{policyId}\", the attempt failed because the date property was not found in both form data and "
                        + "calculation result. To resolve this issue, please ensure that the correct data location is set "
                        + "in the product configuration. For further assistance, please contact technical support.",
                        HttpStatusCode.NotFound,
                        null,
                        errorData);

                public static Error CannotPatchInceptionDateOfCancelledPolicy(string policyId, JObject errorData)
                    => new Error(
                        "policy.date.patching.cancelled.policy.inception.date.not.patchable",
                        "Can't patch inception date of a cancelled policy",
                        "When trying to patch the \"inceptionDate\" date property in the aggregate of the "
                        + $"cancelled policy \"{policyId}\", the attempt was rejected because it is forbidden to modify "
                        + "a cancelled policy's inception date. If you need further assistance, please contact technical support.",
                        HttpStatusCode.NotFound,
                        null,
                        errorData);

                public static Error CannotPatchExpiryDateOfCancelledPolicy(string policyId, JObject errorData)
                    => new Error(
                        "policy.date.patching.cancelled.policy.expiry.date.not.patchable",
                        "Can't patch expiry date of a cancelled policy",
                        "When trying to patch the \"expiryDate\" date property in the aggregate of the "
                        + $"cancelled policy \"{policyId}\", the attempt was rejected because it is forbidden to modify "
                        + "a cancelled policy's expiry date. If you need further assistance, please contact technical support.",
                        HttpStatusCode.NotFound,
                        null,
                        errorData);

                public static Error InvalidDate(string policyId, string dateName, JObject errorData) => new Error(
                    "policy.date.patching.invalid.date",
                    "Invalid date",
                    $"When trying to patch the \"{dateName}\" date property in the aggregate of the "
                    + $"policy \"{policyId}\", the attempt failed because the provided date is invalid. To resolve "
                    + "this issue, please ensure that the provided date is valid. If you need further assistance, "
                    + "please contact technical support.",
                    HttpStatusCode.PreconditionFailed,
                    null,
                    errorData);

                public static Error InvalidTime(string policyId, string dateName, JObject errorData) => new Error(
                    "policy.date.patching.invalid.time",
                    "Invalid time",
                    $"When trying to patch the \"{dateName}\" date property in the aggregate of the "
                    + $"policy \"{policyId}\", the attempt failed because the provided time is invalid. To resolve "
                    + "this issue, please ensure that the provided time is valid. If you need further assistance, "
                    + "please contact technical support.",
                    HttpStatusCode.PreconditionFailed,
                    null,
                    errorData);
            }
        }
    }
}

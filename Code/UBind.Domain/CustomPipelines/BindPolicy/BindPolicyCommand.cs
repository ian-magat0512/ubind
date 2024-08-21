// <copyright file="BindPolicyCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.CustomPipelines.BindPolicy
{
    using System;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Payment;
    using UBind.Domain.Dto;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Payment;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Policy;

    public class BindPolicyCommand : ICommand<ValueTuple<NewQuoteReadModel, PolicyReadModel>>
    {
        private BindPolicyCommand(
            ReleaseContext releaseContext,
            Guid? quoteId,
            BindRequirementDto? requirements = null,
            FormModelPolicyCreationRequirement? formModelPolicyCreationRequirement = null,
            Guid? fundingProposalId = null,
            IPaymentMethodDetails? paymentMethodDetails = null,
            Guid? savedPaymentMethodId = null,
            string? paymentToken = null,
            string? policyNumber = null,
            DeploymentEnvironment? environment = null,
            bool allowBindingForIncompleteQuotes = false)
        {
            this.ReleaseContext = releaseContext;
            this.QuoteId = quoteId;
            this.BindRequirements = requirements;
            this.FormModelPolicyCreationRequirement = formModelPolicyCreationRequirement;
            this.FundingProposalId = fundingProposalId;
            this.PaymentMethodDetails = paymentMethodDetails;
            this.SavedPaymentId = savedPaymentMethodId.GetValueOrDefault();
            this.PaymentToken = paymentToken;
            this.HaveTriedPersistingCommandBefore = false;
            this.PolicyNumber = policyNumber;
            this.Environment = environment;
            this.AllowBindingForIncompleteQuotes = allowBindingForIncompleteQuotes;
        }

        public Guid? QuoteId { get; }

        public BindRequirementDto? BindRequirements { get; }

        public FormModelPolicyCreationRequirement? FormModelPolicyCreationRequirement { get; }

        public Guid? FundingProposalId { get; }

        public IPaymentMethodDetails? PaymentMethodDetails { get; }

        public Guid SavedPaymentId { get; }

        public string? PaymentToken { get; }

        public QuoteAggregate? QuoteAggregate { get; set; }

        public FundingProposal? AcceptedFundingProposal { get; set; }

        public PaymentGatewayResult? PaymentResult { get; set; }

        public ReleaseContext ReleaseContext { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the command is already being persisted.
        /// This is used to ensure that the aggregate is reloaded when a concurrency exception is raised
        /// during the first try.
        /// </summary>
        public bool HaveTriedPersistingCommandBefore { get; set; }

        public string? PolicyNumber { get; set; }

        public DeploymentEnvironment? Environment { get; }

        /// <summary>
        /// Allows binding for incomplete quotes.
        /// </summary>
        public bool AllowBindingForIncompleteQuotes { get; }

        /// <summary>
        /// Creates a binding policy command for the quote.
        /// </summary>
        public static BindPolicyCommand CreateForBindingWithQuote(
            ReleaseContext releaseContext,
            Guid? quoteId = null,
            BindRequirementDto? requirements = null,
            Guid? fundingProposalId = null,
            IPaymentMethodDetails? paymentMethodDetails = null,
            Guid savedPaymentMethodId = default,
            string? externalPolicyNumber = null,
            string? paymentToken = null,
            DeploymentEnvironment? environment = null,
            bool allowBindingForIncompleteQuotes = false)
        {
            return new BindPolicyCommand(
                releaseContext,
                quoteId,
                requirements,
                null,
                fundingProposalId,
                paymentMethodDetails,
                savedPaymentMethodId,
                paymentToken,
                externalPolicyNumber,
                environment,
                allowBindingForIncompleteQuotes);
        }

        /// <summary>
        /// Creates a binding policy command for the form data.
        /// </summary>
        public static BindPolicyCommand CreateForBindingWithoutQuote(
            ReleaseContext releaseContext,
            FormModelPolicyCreationRequirement? formModelPolicyCreationRequirement = null,
            string? externalPolicyNumber = null,
            Guid? fundingProposalId = null,
            IPaymentMethodDetails? paymentMethodDetails = null,
            Guid savedPaymentMethodId = default,
            string? paymentToken = null)
        {
            return new BindPolicyCommand(
                releaseContext,
                null,
                null,
                formModelPolicyCreationRequirement,
                fundingProposalId,
                paymentMethodDetails,
                savedPaymentMethodId,
                paymentToken,
                externalPolicyNumber);
        }
    }
}

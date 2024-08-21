// <copyright file="PolicyTransactionViewModelTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Report
{
    using System;
    using System.Globalization;
    using FluentAssertions;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Report;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadWriteModel;
    using Xunit;

    public class PolicyTransactionViewModelTests
    {
        [Fact]
        public void Properties_AreTransformedCorrectly()
        {
            // Arrange
            var now = SystemClock.Instance.Now();
            var policyTransactionTestClass = new FakePolicyTransactionReadModel(now);

            // Act
            var policyTransactionTemplateModel = new PolicyTransactionViewModel(policyTransactionTestClass);

            // Assert
            this.IsExpectedDateFormat(policyTransactionTemplateModel.CreationDate);
            this.IsExpectedDateFormat(policyTransactionTemplateModel.InceptionDate);
            this.IsExpectedDateFormat(policyTransactionTemplateModel.AdjustmentDate);
            this.IsExpectedTimeFormat(policyTransactionTemplateModel.CreationTime);
            this.IsExpectedTimeFormat(policyTransactionTemplateModel.InceptionTime);
            this.IsExpectedTimeFormat(policyTransactionTemplateModel.AdjustmentTime);
            this.AssertIsAuCurrency(policyTransactionTemplateModel.BasePremium);
            this.AssertIsAuCurrency(policyTransactionTemplateModel.Esl);
            this.AssertIsAuCurrency(policyTransactionTemplateModel.PremiumGst);
            this.AssertIsAuCurrency(policyTransactionTemplateModel.StampDutyAct);
            this.AssertIsAuCurrency(policyTransactionTemplateModel.StampDutyNsw);
            this.AssertIsAuCurrency(policyTransactionTemplateModel.StampDutyNt);
            this.AssertIsAuCurrency(policyTransactionTemplateModel.StampDutyQld);
            this.AssertIsAuCurrency(policyTransactionTemplateModel.StampDutySa);
            this.AssertIsAuCurrency(policyTransactionTemplateModel.StampDutyTas);
            this.AssertIsAuCurrency(policyTransactionTemplateModel.StampDutyVic);
            this.AssertIsAuCurrency(policyTransactionTemplateModel.StampDutyWa);
            this.AssertIsAuCurrency(policyTransactionTemplateModel.StampDutyTotal);
            this.AssertIsAuCurrency(policyTransactionTemplateModel.TotalPremium);
            this.AssertIsAuCurrency(policyTransactionTemplateModel.Commission);
            this.AssertIsAuCurrency(policyTransactionTemplateModel.CommissionGst);
            this.AssertIsAuCurrency(policyTransactionTemplateModel.BrokerFee);
            this.AssertIsAuCurrency(policyTransactionTemplateModel.BrokerFeeGst);
            this.AssertIsAuCurrency(policyTransactionTemplateModel.UnderwriterFee);
            this.AssertIsAuCurrency(policyTransactionTemplateModel.UnderwriterFeeGst);
            this.AssertIsAuCurrency(policyTransactionTemplateModel.TotalGst);
            this.AssertIsAuCurrency(policyTransactionTemplateModel.TotalPayable);
        }

        private void AssertIsAuCurrency(string value)
        {
            value.Should().MatchRegex("^-?\\$\\d{1,3}(,\\d\\d\\d)*\\.\\d\\d$");
        }

        private bool IsExpectedDateFormat(string value)
        {
            DateTime dateValue;
            var format = "dd/MM/yyyy";
            var culture = CultureInfo.CreateSpecificCulture("en-AU");
            return DateTime.TryParseExact(value, format, culture, DateTimeStyles.None, out dateValue);
        }

        private bool IsExpectedTimeFormat(string value)
        {
            DateTime dateValue;
            var format = "hh:mm tt";
            var culture = CultureInfo.CreateSpecificCulture("en-AU");
            return DateTime.TryParseExact(value, format, culture, DateTimeStyles.None, out dateValue);
        }

        private class FakePolicyTransactionReadModel : IPolicyTransactionReadModelSummary
        {
            public FakePolicyTransactionReadModel(Instant now)
            {
                this.CreatedTimestamp = now;
                this.AdjustmentEffectiveTimestamp = now;
                this.InceptionTimestamp = now;
                this.ExpiryTimestamp = now;
                this.EffectiveTimestamp = now;
                this.CancellationEffectiveTimestamp = now;
                this.CancellationEffectiveTicksSinceEpoch = now.ToUnixTimeTicks();
            }

            public string TransactionType => string.Empty;

            public string ProductName => string.Empty;

            public LocalDate CreatedDate => this.CreatedTimestamp.ToLocalDateInAet();

            public Instant CreatedTimestamp { get; }

            public LocalDateTime? AdjustmentEffectiveDateTime => this.AdjustmentEffectiveTimestamp?.ToLocalDateTimeInAet();

            public LocalDateTime? CancellationEffectiveDateTime => this.CancellationEffectiveTimestamp?.ToLocalDateTimeInAet();

            public Instant? AdjustmentEffectiveTimestamp { get; }

            public Instant? CancellationEffectiveTimestamp { get; }

            public string PolicyNumber => string.Empty;

            public string QuoteReference => string.Empty;

            public string InvoiceNumber => string.Empty;

            public string CreditNoteNumber => string.Empty;

            public string CustomerFullName => string.Empty;

            public string CustomerEmail => string.Empty;

            public string RatingState => string.Empty;

            public PriceBreakdown PriceBreakdown => PriceBreakdown.Zero(PriceBreakdown.DefaultCurrencyCode);

            public bool IsTestData => false;

            public bool IsAdjusted => false;

            public long CancellationEffectiveTicksSinceEpoch { get; }

            public LocalDateTime? InceptionDateTime => this.InceptionTimestamp?.ToLocalDateTimeInAet();

            public Instant? InceptionTimestamp { get; }

            public LocalDateTime? ExpiryDateTime => this.ExpiryTimestamp?.ToLocalDateTimeInAet();

            public Instant? ExpiryTimestamp { get; }

            public LocalDateTime EffectiveDateTime => this.EffectiveTimestamp.ToLocalDateTimeInAet();

            public Instant EffectiveTimestamp { get; }

            public Instant EffectiveEndTimestamp { get; }

            public string FormData => "{\"formModel\": {\"data\": \"value\"}}";

            public string CalculationResult => "{\"Json\": \"{\\\"data\\\": \\\"value\\\"}\"}";

            public string PaymentGateway => string.Empty;

            public string PaymentResponseJson => string.Empty;

            public CalculationResultReadModel LatestCalculationResult => throw new NotImplementedException();

            public string SerializedLatestCalculationResult
            {
                get => JsonConvert.SerializeObject(new CalculationResultReadModel(null));
                set => throw new NotImplementedException();
            }

            public string OrganisationName { get; }

            public string OrganisationAlias { get; }

            public string AgentName { get; }
        }
    }
}

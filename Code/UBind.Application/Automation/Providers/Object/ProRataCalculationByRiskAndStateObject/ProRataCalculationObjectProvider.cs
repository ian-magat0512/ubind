// <copyright file="ProRataCalculationObjectProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Object;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Humanizer;
using MorseCode.ITask;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NodaTime;
using UBind.Application.Automation.Enums;
using UBind.Application.Automation.Extensions;
using UBind.Domain;
using UBind.Domain.Exceptions;
using UBind.Domain.ReadModel;
using UBind.Domain.ReadModel.Policy;
using UBind.Domain.ReadWriteModel;
using CalculationResult = UBind.Domain.ReadWriteModel.CalculationResult;

/// <summary>
/// This class is needed because we need to have a functionality
/// to retrieve pro-rata premium breakdown by risk and by state.
/// Note: This is a temporary provider only created for Stand > Business product.
/// </summary>
public class ProRataCalculationObjectProvider : IObjectProvider
{
    private readonly IPolicyTransactionReadModelRepository policyTransactionReadModelRepository;

    public ProRataCalculationObjectProvider(
        IProvider<Data<string>> policyTransactionId,
        IPolicyTransactionReadModelRepository policyTransactionReadModelRepository)
    {
        this.PolicyTransactionId = policyTransactionId;
        this.policyTransactionReadModelRepository = policyTransactionReadModelRepository;
    }

    public string SchemaReferenceKey => "proRataCalculationObjectProvider";

    private IProvider<Data<string>> PolicyTransactionId { get; }

    /// <inheritdoc/>
    public async ITask<IProviderResult<Data<object>>> Resolve(IProviderContext providerContext)
    {
        var id = (await this.PolicyTransactionId.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
        var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
        var additionalDetails = new List<string>
        {
            $"{ErrorDataKey.EntityType.Titleize()}: PolicyTransaction",
            $"Policy Transaction Id: {id}",
        };

        if (!Guid.TryParse(id, out Guid policyTransactionId))
        {
            throw new ErrorException(
                Errors.Automation.Provider.Entity.NotFound(
                    EntityType.PolicyTransaction.Humanize(), "policyTransactionId", id, errorData, additionalDetails));
        }

        var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;
        var environment = providerContext.AutomationData.System.Environment;
        var policyTransactionDetails =
            this.policyTransactionReadModelRepository.GetPolicyTransactionWithRelatedEntities(
                tenantId, environment, policyTransactionId, new List<string>() { "policy" });

        if (policyTransactionDetails == null)
        {
            throw new ErrorException(
                Errors.Automation.Provider.Entity.NotFound(
                    EntityType.PolicyTransaction.Humanize(), "policyTransactionId", id, errorData, additionalDetails));
        }

        var latestCalculation = policyTransactionDetails.PolicyTransaction.PolicyData.SerializedCalculationResult;
        var calculation = JsonConvert.DeserializeObject<CalculationResult>(
            latestCalculation, CustomSerializerSetting.JsonSerializerSettings)!;

        // according to Gene, state percent breakdown is in risk1 only.
        var risk1StatePrecentBreakdown = calculation.JObject.SelectToken("risk1.statePercentBreakdown");
        if (risk1StatePrecentBreakdown == null)
        {
            throw new ErrorException(Errors.Calculation.RequiredDataMissing(
                "risk1.statePercentBreakdown",
                "We were using the prorataCalculationProvider in automations",
                errorData));
        }
        var percentByState = JsonConvert.DeserializeObject<PercentBreakdownByState>(risk1StatePrecentBreakdown.ToString())!;
        var calculations = new List<CalculationResult>();
        var transactions = policyTransactionDetails.PolicyTransactions
        .Where(c => c.CreatedTicksSinceEpoch <= policyTransactionDetails.PolicyTransaction.CreatedTicksSinceEpoch)
        .OrderByDescending(c => c.CreatedTicksSinceEpoch).ToList();

        var inceptionDate = policyTransactionDetails.Policy.InceptionDateTime.Date;
        var expiryDateTime = policyTransactionDetails.Policy.ExpiryDateTime;
        if (expiryDateTime == null)
        {
            throw new ErrorException(Errors.Calculation.RequiredDataMissing(
                "expiryDateTime",
                "We were using the prorataCalculationProvider in automations",
                errorData));
        }
        var expiryDate = expiryDateTime.Value.Date;
        var transactionRisks = this.GetTransactionRisks(transactions);

        var result = new Dictionary<string, object>();
        for (int i = 1; i <= transactionRisks.Last().RisksPremium.Count; i++)
        {
            var premium = this.CalculatePayableByRisk(transactionRisks, i, inceptionDate, expiryDate);
            var premiumByState = new PremiumBreakdownByState(percentByState, premium);
            result.Add($"risk{i}", premiumByState);
        }

        return ProviderResult<Data<object>>.Success(
            new Data<object>(new ReadOnlyDictionary<string, object>(result)));
    }

    public List<PolicyTransactionRiskPremium> GetTransactionRisks(List<PolicyTransaction> transactions)
    {
        var riskPremiums = new List<PolicyTransactionRiskPremium>();
        foreach (var transaction in transactions)
        {
            var latestCalculation = transaction.PolicyData.SerializedCalculationResult;
            var calculation = JsonConvert.DeserializeObject<CalculationResult>(
                latestCalculation, CustomSerializerSetting.JsonSerializerSettings)!;

            var startDate = calculation.ApplicablePrice.StartDate;
            var endDate = calculation.ApplicablePrice.EndDate;
            if (transaction is CancellationTransaction cancel)
            {
                startDate = cancel.CancellationEffectiveDateTime.Date;
            }

            var risks = new Dictionary<int, decimal>();
            int i = 1;
            var token = calculation.JObject[$"risk{i}"];
            while (token != null && token.Type != JTokenType.Null)
            {
                // in the workbook it says prorata, but its not prorata, its base premium.
                var basePremium = (decimal)token.SelectToken($"payment.premium")!;
                if (transaction is CancellationTransaction)
                {
                    basePremium = 0m;
                }

                risks.Add(i, basePremium);
                i++;
                token = calculation.JObject[$"risk{i}"];
            }

            var riskBreakdown = new PolicyTransactionRiskPremium()
            {
                StartDate = startDate,
                EndDate = endDate,
                RisksPremium = risks,
            };

            riskPremiums.Add(riskBreakdown);
        }

        return riskPremiums;
    }

    public decimal CalculatePayableByRisk(
        List<PolicyTransactionRiskPremium> calculations,
        int riskNo,
        LocalDate inceptionDate,
        LocalDate expiryDate)
    {
        // if there no other transactions, it means its a new business transaction
        // just return the risk premium
        if (calculations.Count == 1)
        {
            var startDate = calculations.First().StartDate;
            var endDate = calculations.First().EndDate;
            var premium = calculations.First().RisksPremium[riskNo];
            return this.CalculateProRatePremium(premium, inceptionDate, expiryDate, startDate, endDate);
        }

        var previousTransactions = new List<PolicyTransactionRiskPremium>();
        previousTransactions.AddRange(calculations);

        var effectiveDate = calculations[0].EndDate;
        var initialPremium = calculations.Last().RisksPremium[riskNo];
        var payable = -1 * initialPremium;

        // e.g. for adjustment transaction.
        // NewBusiness risk1 premium = 3000, Jan 1 - Jan 30
        // Adjustment 1 = risk1 premium = 1500, Jan 5 - Jan 30
        // Adjustment 2 = risk1 premium = 300, Jan 20 - Jan 30
        // payable = (proRataOfAdj2 - PayableOnAdj1 + proRataOfAdj1 + proRataOfNewBusiness) - initial premium
        // payable = [(300 * (10/ 30)) + (payable on Adjustment1) + (1500 * (15/30)) + (3000 * (4 / 30))] - 3000

        // e.g. for new business and renewal
        // payable = premium

        // e.g. for cancellation
        // NewBusiness risk1 premium = 3000, Jan 1 - Jan 30
        // Cancellation = Jan 5 - Jan 30
        // payable = proRataOfCancellation - initial premium
        // payable = [(3000 * (4 / 30))] - 3000;
        foreach (var calculation in calculations)
        {
            var startDate = calculation.StartDate;
            var endDate = calculation.EndDate;

            if (calculation.RisksPremium.ContainsKey(riskNo))
            {
                var premium = calculation.RisksPremium[riskNo];
                var proRataPremium = this.CalculateProRatePremium(premium, inceptionDate, expiryDate, startDate, effectiveDate);
                previousTransactions.Remove(calculation);
                var previousPayable = 0m;
                if (previousTransactions.Count > 1)
                {
                    previousPayable = this.CalculatePayableByRisk(previousTransactions, riskNo, inceptionDate, expiryDate);
                }

                payable = payable + proRataPremium - previousPayable;
            }

            // use the start date of previous transaction as effective date.
            effectiveDate = calculation.StartDate;
        }

        return payable;
    }

    private decimal CalculateProRatePremium(
        decimal basePremium,
        LocalDate inceptionDate,
        LocalDate expiryDate,
        LocalDate startDate,
        LocalDate effectiveDate)
    {
        var zeroPremium = PriceBreakdown.Zero(PriceBreakdown.DefaultCurrencyCode);
        var priceBreakdown = PriceBreakdown.OnlyBasePremium(PriceBreakdown.DefaultCurrencyCode, basePremium);
        var fixedAndScalablePrice = new FixedAndScalablePrice(zeroPremium, priceBreakdown);

        var intervalPrice = new IntervalPrice(fixedAndScalablePrice, inceptionDate, expiryDate);
        var applicablePrice = intervalPrice.CalculateProRataPrice(startDate, effectiveDate);
        return applicablePrice.FixedAndScalablePrice.ScalableComponents.BasePremium;
    }
}

// <copyright file="GenerateCalculationResultHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.CustomPipelines.QuoteCalculation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using Humanizer;
    using MediatR;
    using MoreLinq;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Commands.QuoteCalculation;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever;
    using UBind.Domain.Clients.DVA.Perils.Entities;
    using UBind.Domain.Clients.DVA.Perils.Interfaces;
    using UBind.Domain.Configuration;
    using UBind.Domain.Extensions;
    using UBind.Domain.Json;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// This class is needed to validate the quote before processing the quote calculation.
    /// </summary>
    /// <typeparam name="TRequest">The command request.</typeparam>
    /// <typeparam name="TResponse">The command response.</typeparam>
    public class GenerateCalculationResultHandler<TRequest, TResponse>
        : IPipelineBehavior<QuoteCalculationCommand, CalculationResponseModel>
        where TRequest : QuoteCalculationCommand
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IEnvironmentSetting<ProductSetting> environmentSetting;
        private readonly ICalculationService calculationService;
        private readonly IProductConfigurationProvider productConfigurationProvider;
        private readonly IPerilsRepository perilsRepository;
        private readonly IClaimReadModelRepository claimReadModelRepository;
        private readonly IAutomationExtensionPointService automationExtensionPointService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateCalculationResultHandler{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="calculationService">Calculation service.</param>
        /// <param name="productConfigurationProvider">The  product configuration.</param>
        /// <param name="perilsRepository">The DVA perils repository.</param>
        /// <param name="claimReadModelRepository">The claim repository.</param>
        public GenerateCalculationResultHandler(
            ICalculationService calculationService,
            IProductConfigurationProvider productConfigurationProvider,
            IPerilsRepository perilsRepository,
            IClaimReadModelRepository claimReadModelRepository,
            IEnvironmentSetting<ProductSetting> environmentSetting,
            ICachingResolver cachingResolver,
            IAutomationExtensionPointService automationExtensionPointService)
        {
            this.cachingResolver = cachingResolver;
            this.environmentSetting = environmentSetting;
            this.calculationService = calculationService;
            this.productConfigurationProvider = productConfigurationProvider;
            this.perilsRepository = perilsRepository;
            this.claimReadModelRepository = claimReadModelRepository;
            this.automationExtensionPointService = automationExtensionPointService;
        }

        public async Task<CalculationResponseModel> Handle(
            QuoteCalculationCommand request,
            RequestHandlerDelegate<CalculationResponseModel> next,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            request.ProductConfiguration = await this.productConfigurationProvider.GetProductConfiguration(
                request.ReleaseContext, WebFormAppType.Quote);
            var additionalRatingFactors =
                await this.GenerateAdditionalRatingFactors(request);

            // try to do automation and retrieve new formModelResult
            var formModelResult = await this.automationExtensionPointService.TriggerBeforeQuoteCalculation(
                request.Quote,
                request.ReleaseContext,
                request.ProductConfiguration,
                request.CalculationDataModel.FormModel,
                request.OrganisationId,
                cancellationToken);

            // retrieve form model from the automation and use it instead.
            if (formModelResult != null)
            {
                request.CalculationDataModel.FormModel = formModelResult;
                var items = this.RetrieveAdditionalRatingFactorsFromFormModel(request.CalculationDataModel.FormModel);
                items.ForEach(item => additionalRatingFactors.AdditionalRatingFactorsMap.Add(item.Key, item.Value));
            }

            request.ReleaseCalculationOutput = this.calculationService.GetQuoteCalculation(
                request.ReleaseContext, new SpreadsheetCalculationDataModel { FormModel = request.CalculationDataModel.FormModel }, additionalRatingFactors);
            request.CalculationResultData = new CachingJObjectWrapper(request.ReleaseCalculationOutput.CalculationJson);

            // try to do automation and retrieve new formModelResult
            var result = await this.automationExtensionPointService.TriggerAfterQuoteCalculation(
                request.Quote,
                request.ReleaseContext,
                request.ProductConfiguration,
                request.CalculationDataModel.FormModel,
                request.CalculationResultData.JObject,
                request.OrganisationId,
                cancellationToken);

            if (result?.CalculationResult != null)
            {
                request.ReleaseCalculationOutput.CalculationJson = result.CalculationResult.ToString();
                request.CalculationResultData = new CachingJObjectWrapper(request.ReleaseCalculationOutput.CalculationJson);
            }

            if (result?.FormModel != null)
            {
                request.CalculationDataModel.FormModel = result.FormModel;
            }

            request.PriceBreakdown = PriceBreakdown.CreateFromCalculationResultData(request.CalculationResultData);

            return await next();
        }

        private async Task<IAdditionalRatingFactors> GenerateAdditionalRatingFactors(
            QuoteCalculationCommand command)
        {
            var configuration = command.ProductConfiguration;
            var formDataModel = command.CalculationDataModel;
            Dictionary<string, dynamic> additionalRatingFactorsMap = new Dictionary<string, dynamic>();

            // Injected regardless of quote type:
            additionalRatingFactorsMap.Add(RatingFactorConstants.Quote.QuoteType, command.QuoteType.Humanize().ToCamelCase());
            if (command.Quote != null)
            {
                additionalRatingFactorsMap.Add(
                    RatingFactorConstants.Quote.QuoteState,
                    command.Quote.QuoteStatus.ToCamelCase());
            }

            additionalRatingFactorsMap.Add(
                RatingFactorConstants.Quote.Environment, command.ProductContext.Environment.Humanize().ToCamelCase());

            // last premium and claims amount.
            if (command.Quote != null && (command.Quote is RenewalQuote || command.Quote is AdjustmentQuote))
            {
                var lastPremium = this.GetLastPremiumFromNonRunOffQuote(command.Quote.Aggregate, configuration);
                additionalRatingFactorsMap.Add(RatingFactorConstants.Quote.LastPremium, lastPremium);
                var totalClaimsAmount = this.claimReadModelRepository
                    .GetTotalClaimsAmountByPolicyNumberInPastFiveYears(
                        command.Quote.Aggregate.TenantId,
                        command.Quote.Aggregate.ProductId,
                        command.Quote.Aggregate.Policy.PolicyNumber);
                additionalRatingFactorsMap.Add(RatingFactorConstants.Claim.TotalClaimsAmount, totalClaimsAmount);
            }

            // DVA perils
            var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(command.ProductContext.TenantId);
            if (tenantAlias == "dva")
            {
                JToken gnafId = formDataModel.FormModel[RatingFactorConstants.FormData.GnafId];
                JToken startDate = formDataModel.FormModel[RatingFactorConstants.FormData.PolicyStartDate];

                Peril peril = gnafId != null && startDate != null
                    ? this.perilsRepository.GetDetailsByPropertyIdForPolicyStartDate(gnafId.ToString(), startDate.ToString())
                    : null;
                SetAdditionalRatingFactorsForDvaPerils(peril);
            }

            var productAlias = await this.cachingResolver.GetProductAliasOrThrowAsync(
                command.ProductContext.TenantId, command.ProductContext.ProductId);
            return new AdditionalRatingFactors(additionalRatingFactorsMap);

            void SetAdditionalRatingFactorsForDvaPerils(Peril peril)
            {
                if (additionalRatingFactorsMap == null)
                {
                    additionalRatingFactorsMap = new Dictionary<string, dynamic>();
                }

                additionalRatingFactorsMap[RatingFactorConstants.DvaPerils.IcaZone] = peril?.IcaZone ?? string.Empty;
                additionalRatingFactorsMap[RatingFactorConstants.DvaPerils.GnafId] = peril?.GnafPid ?? string.Empty;
                additionalRatingFactorsMap[RatingFactorConstants.DvaPerils.StormRate] = peril?.StormRate ?? 0.00;
                additionalRatingFactorsMap[RatingFactorConstants.DvaPerils.BushfireRate] = peril?.FireRate ?? 0.00;
                additionalRatingFactorsMap[RatingFactorConstants.DvaPerils.EarthquakeRate] = peril?.QuakeRate ?? 0.00;
                additionalRatingFactorsMap[RatingFactorConstants.DvaPerils.FloodRate] = peril?.FloodRate ?? 0.00;
                additionalRatingFactorsMap[RatingFactorConstants.DvaPerils.CycloneRate] = peril?.CycloneRate ?? 0.00;
            }
        }

        /// <summary>
        /// Retrieve additional rating factors from the form model.
        /// </summary>
        /// <param name="formModel">The form model.</param>
        /// <returns>The dictionary pair of the form model property name and its value in List object.</returns>
        private Dictionary<string, dynamic> RetrieveAdditionalRatingFactorsFromFormModel(JObject formModel)
        {
            var dictionary = new Dictionary<string, dynamic>();

            foreach (var prop in formModel)
            {
                if (prop.Value is JArray)
                {
                    if (prop.Value == null)
                    {
                        continue;
                    }

                    var array = prop.Value as JArray;
                    dictionary.Add(prop.Key, array.ToObject<List<object>>());
                }
            }

            return dictionary;
        }

        private decimal GetLastPremiumFromNonRunOffQuote(
            QuoteAggregate aggregate,
            IProductConfiguration productConfiguration)
        {
            return aggregate.Policy.Transactions
                .OrderByDescending(t => t.EffectiveDateTime)
                .OrderByDescending(t => t.EventSequenceNumber)
                .Where(t => t.GetQuoteData(productConfiguration).Retrieve<bool>(StandardQuoteDataField.IsRunOffPolicy) == false)
                .Select(t => t.CalculationResult.AnnualizedPrice.TotalPremium)
                .FirstOrDefault();
        }
    }
}

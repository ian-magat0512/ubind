// <copyright file="ServiceCollectionExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.CustomPipelines
{
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using UBind.Application.Commands.Quote;
    using UBind.Application.Commands.QuoteCalculation;
    using UBind.Application.CustomPipelines.BindPolicy;
    using UBind.Application.CustomPipelines.QuoteCalculation;
    using UBind.Domain.CustomPipelines.BindPolicy;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// This class is needed to make sure the pipeline is registered in correct order.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Method for registering mediatr and custom pipelines.
        /// </summary>
        /// <param name="services">The service collection.</param>
        public static void RegisterCustomPipelines(this IServiceCollection services)
        {
            RegisterQuoteCalculationPipelines(services);
            RegisterBindPolicyPipelines(services);
        }

        private static void RegisterQuoteCalculationPipelines(IServiceCollection services)
        {
            services.AddTransient(
                typeof(IPipelineBehavior<QuoteCalculationCommand, CalculationResponseModel>),
                typeof(ValidateQuoteCalculationCommandHandler<QuoteCalculationCommand, CalculationResponseModel>));
            services.AddTransient(
                typeof(IPipelineBehavior<QuoteCalculationCommand, CalculationResponseModel>),
                typeof(GenerateCalculationResultHandler<QuoteCalculationCommand, CalculationResponseModel>));
            services.AddTransient(
                typeof(IPipelineBehavior<QuoteCalculationCommand, CalculationResponseModel>),
                typeof(SaveFormDataHandler<QuoteCalculationCommand, CalculationResponseModel>));
            services.AddTransient(
                typeof(IPipelineBehavior<QuoteCalculationCommand, CalculationResponseModel>),
                typeof(CreatePremiumFundingProposalHandler<QuoteCalculationCommand, CalculationResponseModel>));
        }

        private static void RegisterBindPolicyPipelines(IServiceCollection services)
        {
            services.AddTransient(
                typeof(IPipelineBehavior<BindPolicyCommand, ValueTuple<NewQuoteReadModel, PolicyReadModel>>),
                typeof(ValidateBindPolicyCommandHandler<BindPolicyCommand, ValueTuple<NewQuoteReadModel, PolicyReadModel>>));
            services.AddTransient(
                typeof(IPipelineBehavior<BindPolicyCommand, ValueTuple<NewQuoteReadModel, PolicyReadModel>>),
                typeof(AcceptFundingProposalCommandHandler));
            services.AddTransient(
                typeof(IPipelineBehavior<BindPolicyCommand, ValueTuple<NewQuoteReadModel, PolicyReadModel>>),
                typeof(CardPaymentCommandHandler));
            services.AddTransient(
                typeof(IPipelineBehavior<BindPolicyCommand, ValueTuple<NewQuoteReadModel, PolicyReadModel>>),
                typeof(SaveBindCommandHandler<BindPolicyCommand, ValueTuple<NewQuoteReadModel, PolicyReadModel>>));
        }
    }
}

// <copyright file="RenewPolicyActionConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Actions;

using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NodaTime;
using UBind.Application.Automation.Error;
using UBind.Application.Automation.Providers;
using UBind.Application.Automation.Providers.Entity;
using UBind.Application.Automation.Providers.Object;
using UBind.Application.Releases;
using UBind.Domain;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Aggregates.Quote.Workflow;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Product;
using UBind.Domain.ReferenceNumbers;
using UBind.Domain.Services;

/// <summary>
/// Represents the configuration model for the "Renew Policy" action within the automation workflow.
/// This configuration model provides the necessary parameters and settings required to instantiate
/// and configure instances of the <see cref="RenewPolicyAction"/> class.
/// </summary>
public class RenewPolicyActionConfigModel : BaseActionConfigurationModel, IBuilder<Action>
{
    [JsonConstructor]
    public RenewPolicyActionConfigModel(
        string name,
        string alias,
        string description,
        bool asynchronous,
        IBuilder<IProvider<Data<bool>>>? runCondition,
        IEnumerable<ErrorConditionConfigModel>? beforeRunErrorConditions,
        IEnumerable<ErrorConditionConfigModel>? afterRunErrorConditions,
        IEnumerable<IBuilder<Action>>? onErrorActions,
        QuoteEntityProviderConfigModel? quote,
        PolicyEntityProviderConfigModel? policy,
        IBuilder<IObjectProvider>? inputData)
        : base(
          name,
          alias,
          description,
          asynchronous,
          runCondition,
          beforeRunErrorConditions,
          afterRunErrorConditions,
          onErrorActions)
    {
        this.Quote = quote;
        this.Policy = policy;
        this.InputData = inputData;
    }

    public QuoteEntityProviderConfigModel? Quote { get; private set; }

    public PolicyEntityProviderConfigModel? Policy { get; private set; }

    public IBuilder<IObjectProvider>? InputData { get; private set; }

    public override Action Build(IServiceProvider dependencyProvider)
    {
        var beforeRunConditions = this.BeforeRunErrorConditions?.Select(br => br.Build(dependencyProvider));
        var afterRunConditions = this.AfterRunErrorConditions?.Select(ar => ar.Build(dependencyProvider));
        var errorActions = this.OnErrorActions?.Select(oa => oa.Build(dependencyProvider));
        return new RenewPolicyAction(
            this.Name,
            this.Alias,
            this.Description,
            this.Asynchronous,
            this.RunCondition?.Build(dependencyProvider),
            beforeRunConditions,
            afterRunConditions,
            errorActions,
            (QuoteEntityProvider?)this.Quote?.Build(dependencyProvider),
            (PolicyEntityProvider?)this.Policy?.Build(dependencyProvider),
            this.InputData?.Build(dependencyProvider),
            dependencyProvider.GetRequiredService<ICqrsMediator>(),
            dependencyProvider.GetRequiredService<IClock>(),
            dependencyProvider.GetRequiredService<IProductConfigurationProvider>(),
            dependencyProvider.GetRequiredService<IQuoteWorkflowProvider>(),
            dependencyProvider.GetRequiredService<IHttpContextPropertiesResolver>(),
            dependencyProvider.GetRequiredService<ICachingResolver>(),
            dependencyProvider.GetRequiredService<IQuoteAggregateResolverService>(),
            dependencyProvider.GetRequiredService<IPolicyService>(),
            dependencyProvider.GetRequiredService<IReleaseQueryService>(),
            dependencyProvider.GetRequiredService<IProductFeatureSettingService>(),
            dependencyProvider.GetRequiredService<IQuoteAggregateRepository>(),
            dependencyProvider.GetRequiredService<IInvoiceNumberRepository>(),
            dependencyProvider.GetRequiredService<IPolicyTransactionTimeOfDayScheme>());
    }
}

// <copyright file="CreateOrganisationConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Services.AdditionalPropertyValue;

    public class CreateOrganisationConfigModel : BaseActionConfigurationModel, IBuilder<Action>
    {
        [JsonConstructor]
        public CreateOrganisationConfigModel(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IBuilder<IProvider<Data<bool>>> runCondition,
            IEnumerable<ErrorConditionConfigModel> beforeRunErrorConditions,
            IEnumerable<ErrorConditionConfigModel> afterRunErrorConditions,
            IEnumerable<IBuilder<Action>> onErrorActions,
            IBuilder<IProvider<Data<string>>> organisationName,
            IBuilder<IProvider<Data<string>>> organisationAlias,
            IBuilder<IObjectProvider> additionalProperties)
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
            this.OrganisationName = organisationName;
            this.OrganisationAlias = organisationAlias;
            this.AdditionalProperties = additionalProperties;
        }

        [JsonProperty("organisationName")]
        public IBuilder<IProvider<Data<string>>> OrganisationName { get; set; }

        [JsonProperty("organisationAlias")]
        public IBuilder<IProvider<Data<string>>> OrganisationAlias { get; set; }

        [JsonProperty("managingOrganisation")]
        public OrganisationEntityProviderConfigModel? ManagingOrganisation { get; set; }

        [JsonProperty("additionalProperties")]
        public IBuilder<IObjectProvider> AdditionalProperties { get; private set; }

        public override Action Build(IServiceProvider dependencyProvider)
        {
            return new CreateOrganisationAction(
                this.Name,
                this.Alias,
                this.Description,
                this.Asynchronous,
                this.RunCondition?.Build(dependencyProvider),
                this.BeforeRunErrorConditions?.Select(bc => bc.Build(dependencyProvider)),
                this.AfterRunErrorConditions?.Select(ac => ac.Build(dependencyProvider)),
                this.OnErrorActions?.Select(ea => ea.Build(dependencyProvider)),
                this.OrganisationName.Build(dependencyProvider),
                this.OrganisationAlias.Build(dependencyProvider),
                this.ManagingOrganisation?.Build(dependencyProvider),
                this.AdditionalProperties?.Build(dependencyProvider),
                dependencyProvider.GetRequiredService<IClock>(),
                dependencyProvider.GetRequiredService<ICachingResolver>(),
                dependencyProvider.GetRequiredService<IAdditionalPropertyTransformHelper>(),
                dependencyProvider.GetRequiredService<ICqrsMediator>());
        }
    }
}

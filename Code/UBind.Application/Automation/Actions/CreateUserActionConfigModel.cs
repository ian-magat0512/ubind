// <copyright file="CreateUserActionConfigModel.cs" company="uBind">
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
    using UBind.Application.Automation.ContactDetail;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services.AdditionalPropertyValue;

    public class CreateUserActionConfigModel : BaseActionConfigurationModel, IBuilder<Action>
    {
        [JsonConstructor]
        public CreateUserActionConfigModel(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IBuilder<IProvider<Data<bool>>>? runCondition,
            IEnumerable<ErrorConditionConfigModel>? beforeRunErrorConditions,
            IEnumerable<ErrorConditionConfigModel>? afterRunErrorConditions,
            IEnumerable<IBuilder<Action>>? onErrorActions,
            IBuilder<IProvider<Data<string>>> accountEmail,
            OrganisationEntityProviderConfigModel organisation,
            PortalEntityProviderConfigModel? portal,
            PersonConstructorConfigModel person,
            IEnumerable<IBuilder<IProvider<Data<string>>>>? initialRoles,
            IBuilder<IObjectProvider>? additionalProperties)
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
            this.AccountEmail = accountEmail;
            this.Organisation = organisation;
            this.Portal = portal;
            this.PersonConstructorConfigModel = person;
            this.AdditionalProperties = additionalProperties;
            this.InitialRoles = initialRoles;
        }

        [JsonProperty("accountEmail")]
        public IBuilder<IProvider<Data<string>>> AccountEmail { get; set; }

        [JsonProperty("organisation")]
        public OrganisationEntityProviderConfigModel Organisation { get; set; }

        [JsonProperty("portal")]
        public PortalEntityProviderConfigModel? Portal { get; set; }

        [JsonProperty("person")]
        public PersonConstructorConfigModel PersonConstructorConfigModel { get; set; }

        [JsonProperty("additionalProperties")]
        public IBuilder<IObjectProvider>? AdditionalProperties { get; set; }

        [JsonProperty("initialRoles")]
        public IEnumerable<IBuilder<IProvider<Data<string>>>>? InitialRoles { get; set; }

        public override Action Build(IServiceProvider dependencyProvider)
        {
            return new CreateUserAction(
                this.Name,
                this.Alias,
                this.Description,
                this.Asynchronous,
                this.RunCondition?.Build(dependencyProvider),
                this.BeforeRunErrorConditions?.Select(bc => bc.Build(dependencyProvider)),
                this.AfterRunErrorConditions?.Select(ac => ac.Build(dependencyProvider)),
                this.OnErrorActions?.Select(ea => ea.Build(dependencyProvider)),
                this.AccountEmail.Build(dependencyProvider),
                this.Organisation.Build(dependencyProvider),
                this.Portal?.Build(dependencyProvider),
                this.AdditionalProperties?.Build(dependencyProvider),
                this.InitialRoles?.Select(x => x.Build(dependencyProvider)),
                dependencyProvider.GetRequiredService<IAdditionalPropertyTransformHelper>(),
                this.PersonConstructorConfigModel.Build(dependencyProvider),
                dependencyProvider.GetRequiredService<ICqrsMediator>(),
                dependencyProvider.GetRequiredService<IRoleRepository>(),
                dependencyProvider.GetRequiredService<IClock>());
        }
    }
}

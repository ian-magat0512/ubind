// <copyright file="CreateOrganisationAction.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Actions
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using NodaTime;
    using StackExchange.Profiling;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.Commands.Organisation;
    using UBind.Domain;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using Void = UBind.Domain.Helpers.Void;

    public class CreateOrganisationAction : Action
    {
        private readonly IClock clock;
        private readonly Regex organisationNameRegex = new Regex("(^[a-zA-Z](?:-?[a-zA-Z0-9. ,']+)*[^-_])$");
        private readonly Regex organisationAliasRegex = new Regex("(^[a-z](?:-?[a-z0-9]+)*$)");
        private readonly IAdditionalPropertyTransformHelper additionalPropertyTransformHelper;
        private ICachingResolver cachingResolver;
        private ICqrsMediator mediator;

        public CreateOrganisationAction(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IProvider<Data<bool>>? runCondition,
            IEnumerable<ErrorCondition>? beforeErrorConditions,
            IEnumerable<ErrorCondition>? afterErrorConditions,
            IEnumerable<Action>? onErrorActions,
            IProvider<Data<string>> organisationName,
            IProvider<Data<string>> organisationAlias,
            OrganisationEntityProvider? managingOrganisation,
            IObjectProvider? additionalProperties,
            IClock clock,
            ICachingResolver cachingResolver,
            IAdditionalPropertyTransformHelper additionalPropertyTransformHelper,
            ICqrsMediator mediator)
            : base(
                  name,
                  alias,
                  description,
                  asynchronous,
                  runCondition,
                  beforeErrorConditions,
                  afterErrorConditions,
                  onErrorActions)
        {
            this.cachingResolver = cachingResolver;
            this.mediator = mediator;
            this.OrganisationNameProvider = organisationName;
            this.OrganisationAliasProvider = organisationAlias;
            this.ManagingOrganisationProvider = managingOrganisation;
            this.additionalPropertyTransformHelper = additionalPropertyTransformHelper;
            this.AdditionalPropertiesProvider = additionalProperties;
            this.clock = clock;
        }

        public IProvider<Data<string>> OrganisationNameProvider { get; }

        public IProvider<Data<string>> OrganisationAliasProvider { get; }

        public OrganisationEntityProvider? ManagingOrganisationProvider { get; }

        public IObjectProvider? AdditionalPropertiesProvider { get; }

        public override ActionData CreateActionData() => new CreateOrganisationActionData(this.Name, this.Alias, this.clock);

        public override bool IsReadOnly() => false;

        public async override Task<Result<Void, Domain.Error>> Execute(
            IProviderContext providerContext,
            ActionData actionData,
            bool isInternal = false)
        {
            using (MiniProfiler.Current.Step(nameof(CreateOrganisationAction) + "." + nameof(this.Execute)))
            {
                actionData.UpdateState(ActionState.Running);

                var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;
                var organisationId = providerContext.AutomationData.ContextManager.Organisation.Id;
                var tenantAlias = this.cachingResolver.GetTenantAliasOrThrow(tenantId);
                string organisationName = (await this.OrganisationNameProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
                string organisationAlias = (await this.OrganisationAliasProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
                var managingOrganisation = (await this.ManagingOrganisationProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
                ReadOnlyDictionary<string, object>? additionalProperties = (ReadOnlyDictionary<string, object>?)
                    (await this.AdditionalPropertiesProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
                await this.Validate(providerContext, organisationName, organisationAlias);

                List<AdditionalPropertyValueUpsertModel>? additionalPropertyUpserts = null;
                if (additionalProperties != null)
                {
                    additionalPropertyUpserts = await this.additionalPropertyTransformHelper.TransformObjectDictionaryToValueUpsertModels(
                        tenantId,
                        organisationId,
                        Domain.Enums.AdditionalPropertyEntityType.Organisation,
                        additionalProperties);
                }

                try
                {
                    // create the organisation here.
                    var newOrganisation = await this.mediator.Send(new CreateOrganisationCommand(
                       tenantId,
                       organisationAlias,
                       organisationName,
                       managingOrganisation?.Id,
                       additionalPropertyUpserts));
                    var organisationActionData = (CreateOrganisationActionData)actionData;
                    organisationActionData.OrganisationId = newOrganisation.Id;
                    organisationActionData.OrganisationName = organisationName;
                    organisationActionData.OrganisationAlias = organisationAlias;
                    if (additionalProperties != null)
                    {
                        organisationActionData.AdditionalProperties = additionalProperties;
                    }
                }
                catch (ErrorException ex)
                {
                    var errorData = await providerContext.GetDebugContext();
                    if (ex.Error.Code == Errors.Organisation.NameUnderTenantAlreadyExists(null!, null!).Code)
                    {
                        var error = Domain.Errors.Automation.CreateOrganisationAction.OrganisationNameExists(tenantAlias, organisationName, errorData);
                        throw new ErrorException(error);
                    }

                    if (ex.Error.Code == Errors.Organisation.AliasUnderTenantAlreadyExists(null!, null!).Code)
                    {
                        var error = Domain.Errors.Automation.CreateOrganisationAction.OrganisationAliasNonUnique(tenantAlias, organisationAlias, errorData);
                        throw new ErrorException(error);
                    }

                    throw ex;
                }

                return await Task.FromResult(Result.Success<Void, Domain.Error>(default));
            }
        }

        private async Task Validate(IProviderContext providerContext, string organisationName, string organisationAlias)
        {
            if (!this.organisationNameRegex.IsMatch(organisationName))
            {
                var errorData = await providerContext.GetDebugContext();
                var error = Domain.Errors.Automation.CreateOrganisationAction.OrganisationNameInvalid(organisationName, errorData);
                throw new ErrorException(error);
            }

            if (!this.organisationAliasRegex.IsMatch(organisationAlias))
            {
                var errorData = await providerContext.GetDebugContext();
                var error = Domain.Errors.Automation.CreateOrganisationAction.OrganisationAliasInvalid(organisationAlias, errorData);
                throw new ErrorException(error);
            }
        }
    }
}

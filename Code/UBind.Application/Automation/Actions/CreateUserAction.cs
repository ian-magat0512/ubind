// <copyright file="CreateUserAction.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using Humanizer;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using StackExchange.Profiling;
    using UBind.Application.Automation.ContactDetail;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.Commands.User;
    using UBind.Application.User;
    using UBind.Domain;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Person.Fields;
    using UBind.Domain.Entities;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using Organisation = UBind.Domain.SerialisedEntitySchemaObject.Organisation;
    using Void = UBind.Domain.Helpers.Void;

    public class CreateUserAction : Action
    {
        private readonly IClock clock;
        private readonly IAdditionalPropertyTransformHelper additionalPropertyTransformHelper;
        private IProviderContext? providerContext;
        private List<string>? additionalDetails;

        public CreateUserAction(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IProvider<Data<bool>>? runCondition,
            IEnumerable<ErrorCondition>? beforeErrorConditions,
            IEnumerable<ErrorCondition>? afterErrorConditions,
            IEnumerable<Action>? onErrorActions,
            IProvider<Data<string>> accountEmail,
            IEntityProvider organisationProvider,
            IEntityProvider? portalProvider,
            IObjectProvider? additionalProperties,
            IEnumerable<IProvider<Data<string>>>? initialRoles,
            IAdditionalPropertyTransformHelper additionalPropertyTransformHelper,
            PersonConstructor personConstructor,
            ICqrsMediator mediator,
            IRoleRepository roleRepository,
            IClock clock)
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
            this.AccountEmailProvider = accountEmail;
            this.InitialRoleProviders = initialRoles;
            this.OrganisationProvider = organisationProvider;
            this.PortalProvider = portalProvider;
            this.PersonConstructor = personConstructor;
            this.RoleRepository = roleRepository;
            this.clock = clock;
            this.Mediator = mediator;
            this.additionalPropertyTransformHelper = additionalPropertyTransformHelper;
            this.AdditionalPropertiesProvider = additionalProperties;
        }

        public IProvider<Data<string>> AccountEmailProvider { get; }

        public IEnumerable<IProvider<Data<string>>>? InitialRoleProviders { get; }

        public IEntityProvider? PortalProvider { get; }

        public IEntityProvider OrganisationProvider { get; }

        public PersonConstructor PersonConstructor { get; }

        public ICqrsMediator Mediator { get; }

        public IRoleRepository RoleRepository { get; }

        public IObjectProvider? AdditionalPropertiesProvider { get; }

        public override ActionData CreateActionData() => new CreateUserActionData(this.Name, this.Alias, this.clock);

        public override bool IsReadOnly() => false;

        public async override Task<Result<Void, Domain.Error>> Execute(
            IProviderContext providerContext,
            ActionData actionData,
            bool isInternal = false)
        {
            using (MiniProfiler.Current.Step(nameof(SetVariableAction) + "." + nameof(this.Execute)))
            {
                actionData.UpdateState(ActionState.Running);
                this.additionalDetails = new List<string>
                {
                    "Action Type: " + actionData.Type.Humanize(),
                    "Action Alias: " + actionData.Alias,
                };

                var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;
                var organisationId = providerContext.AutomationData.ContextManager.Organisation.Id;
                var environment = providerContext.AutomationData.System.Environment;
                this.providerContext = providerContext;
                PersonConstructorData? person = this.PersonConstructor != null
                    ? await this.PersonConstructor.Resolve(providerContext)
                    : null;
                string accountEmail = (await this.AccountEmailProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
                Organisation organisation = await this.GetOrganisation(tenantId, providerContext);
                UBind.Domain.SerialisedEntitySchemaObject.Portal? portal = await this.GetPortal(organisation, providerContext);

                ReadOnlyDictionary<string, object>? additionalProperties
                    = (ReadOnlyDictionary<string, object>?)(await this.AdditionalPropertiesProvider.ResolveValueIfNotNull(providerContext))?.DataValue;

                List<Guid> initialRoles = await this.GetRoles(tenantId, providerContext);

                try
                {
                    List<AdditionalPropertyValueUpsertModel>? additionalPropertyUpserts = additionalProperties != null
                        ? await this.additionalPropertyTransformHelper.TransformObjectDictionaryToValueUpsertModels(
                             tenantId,
                             organisationId,
                             Domain.Enums.AdditionalPropertyEntityType.User,
                             additionalProperties)
                        : null;

                    await this.ValidateAccountEmail(accountEmail);
                    IEnumerable<PhoneNumberField>? phoneNumbers = person?.GetPhoneNumbers();
                    IEnumerable<EmailAddressField>? emailAddresses = person?.GetEmailAddresses();
                    IEnumerable<StreetAddressField>? streetAddresses = person?.GetStreetAddresses();
                    IEnumerable<WebsiteAddressField>? websiteAddresses = person?.GetWebsiteAddresses();
                    IEnumerable<MessengerIdField>? messengerIds = person?.GetMessengerIds();
                    IEnumerable<SocialMediaIdField>? socialMediaIds = person?.GetSocialmediaIds();
                    UserSignupModel userSignupModel = new UserSignupModel
                    {
                        TenantId = tenantId,
                        OrganisationId = organisation.Id,
                        PortalId = portal?.Id,
                        Environment = environment,
                        Email = accountEmail,
                        UserType = UserType.Client,
                        FirstName = person?.FirstName,
                        LastName = person?.LastName,
                        MiddleNames = person?.MiddleNames,
                        NameSuffix = person?.NameSuffix,
                        NamePrefix = person?.NamePrefix,
                        PreferredName = person?.PreferredName,
                        Company = person?.Company,
                        Title = person?.Title,
                        PhoneNumbers = phoneNumbers?.ToList(),
                        EmailAddresses = emailAddresses?.ToList(),
                        StreetAddresses = streetAddresses?.ToList(),
                        WebsiteAddresses = websiteAddresses?.ToList(),
                        MessengerIds = messengerIds?.ToList(),
                        SocialMediaIds = socialMediaIds?.ToList(),
                        Properties = additionalPropertyUpserts,
                        InitialRoles = initialRoles,
                    };

                    var command = new CreateUserCommand(userSignupModel);
                    var newUser = await this.Mediator.Send(command);
                    ((CreateUserActionData)actionData).SetFields(newUser.Id, organisation?.Id, accountEmail, additionalProperties, portal?.Id);
                }
                catch (ErrorException ex)
                {
                    JObject errorData = await providerContext.GetDebugContext();
                    ex.EnrichAndRethrow(errorData, this.additionalDetails);

                    throw;
                }

                return await Task.FromResult(Result.Success<Void, Domain.Error>(default));
            }
        }

        private async Task<Organisation> GetOrganisation(Guid tenantId, IProviderContext providerContext)
        {
            var resolveOrganisation = await this.OrganisationProvider.Resolve(providerContext);
            var organisation = (Organisation)resolveOrganisation.GetValueOrThrowIfFailed().DataValue;
            if (Guid.Parse(organisation.TenantId) != tenantId)
            {
                var errorData = await providerContext.GetDebugContext();
                var error = Domain.Errors.Automation.CreateUserAction.OrganisationIsNotValidInThisTenancy(organisation.Alias, errorData);
                throw new ErrorException(error);
            }

            return organisation;
        }

        private async Task<Domain.SerialisedEntitySchemaObject.Portal?> GetPortal(Organisation organisation, IProviderContext providerContext)
        {
            UBind.Domain.SerialisedEntitySchemaObject.Portal? portal = null;
            try
            {
                portal = (UBind.Domain.SerialisedEntitySchemaObject.Portal?)
                    (await this.PortalProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
            }
            catch (ErrorException ex)
            {
                var errorData = await providerContext.GetDebugContext();
                ex.EnrichAndRethrow(errorData, this.additionalDetails);

                throw;
            }

            return portal;
        }

        private async Task<List<Guid>> GetRoles(Guid tenantId, IProviderContext providerContext)
        {
            List<Guid> initialRoles = new List<Guid>();
            if (this.InitialRoleProviders != null)
            {
                foreach (var roleNameProvider in this.InitialRoleProviders)
                {
                    string? roleName = (await roleNameProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
                    if (!string.IsNullOrEmpty(roleName))
                    {
                        Role role = this.RoleRepository.GetRoleByNameOrThrow(tenantId, roleName);
                        if (role.Type != Domain.Permissions.RoleType.Client)
                        {
                            var errorData = await providerContext.GetDebugContext();
                            var error = Domain.Errors.Automation.CreateUserAction.AssigningInvalidRole(roleName, errorData);
                            throw new ErrorException(error);
                        }

                        initialRoles.Add(role.Id);
                        continue;
                    }
                }
            }

            return initialRoles;
        }

        private async Task ValidateAccountEmail(string accountEmail)
        {
            try
            {
                var email = new MailAddress(accountEmail);
            }
            catch (FormatException)
            {
                var errorData = await this.providerContext!.GetDebugContext();
                Error error = Domain.Errors.Automation.CreateUserAction.AccountEmailAddressInvalid(accountEmail, errorData);
                throw new ErrorException(error);
            }
        }
    }
}

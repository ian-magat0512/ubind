// <copyright file="SendSmsAction.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Actions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using StackExchange.Profiling;
    using UBind.Application.Automation.Email;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Helper;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Sms;
    using UBind.Application.Commands.Sms;
    using UBind.Application.Sms;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ValueTypes;
    using Void = UBind.Domain.Helpers.Void;

    public class SendSmsAction : Action
    {
        private readonly ISmsClient smsClient;
        private readonly ICachingResolver cachingResolver;
        private readonly ILogger<SendSmsAction> logger;
        private readonly ICqrsMediator mediator;
        private readonly IClock clock;

        public SendSmsAction(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IProvider<Data<bool>> runCondition,
            IEnumerable<ErrorCondition> beforeRunConditions,
            IEnumerable<ErrorCondition> afterRunConditions,
            IEnumerable<IRunnableAction> errorActions,
            SmsConfiguration sms,
            IEnumerable<IProvider<Data<string>>> tags,
            IEnumerable<RelationshipConfiguration> relationships,
            ISmsClient smsClient,
            ICachingResolver cachingResolver,
            ILogger<SendSmsAction> logger,
            ICqrsMediator mediator,
            IClock clock)
            : base(
                  name,
                  alias,
                  description,
                  asynchronous,
                  runCondition,
                  beforeRunConditions,
                  afterRunConditions,
                  errorActions)
        {
            this.Sms = sms;
            this.Tags = tags;
            this.Relationships = relationships;
            this.smsClient = smsClient;
            this.cachingResolver = cachingResolver;
            this.logger = logger;
            this.mediator = mediator;
            this.clock = clock;
        }

        public SmsConfiguration Sms { get; }

        public IEnumerable<IProvider<Data<string>>> Tags { get; private set; }

        public IEnumerable<RelationshipConfiguration> Relationships { get; private set; }

        public override ActionData CreateActionData() => new SendSmsActionData(this.Name, this.Alias, this.clock);

        public override bool IsReadOnly() => false;

        public override async Task<Result<Void, Domain.Error>> Execute(
            IProviderContext providerContext,
            ActionData actionData,
            bool isInternal)
        {
            using (MiniProfiler.Current.Step(nameof(SendSmsAction) + "." + nameof(this.Execute)))
            {
                actionData.UpdateState(ActionState.Running);
                var smsData = await this.Sms.ResolveSmsProperties(providerContext);

                var sms = new UBind.Application.Sms.Sms(
                    smsData.To.Select(to => new PhoneNumber(to)).ToList(),
                    new PhoneNumber(smsData.From),
                    smsData.Message);

                var sendSmsActionProperties = (SendSmsActionData)actionData;
                sendSmsActionProperties.Sms = smsData;

                var response = await this.smsClient.SendSms(sms);
                if (response.ResponseType != SmsResponseType.Success)
                {
                    JObject errorData = response.ErrorData ?? await providerContext.GetDebugContext();
                    errorData.Add("smsTo", string.Join(", ", string.Join(", ", smsData.To)));
                    errorData.Add("smsFrom", smsData.From);
                    errorData.Add("smsContent", smsData.Message);

                    var errorDetails = Errors.Automation.Action.SmsActionSendError(this.Alias, response.ErrorMessage, errorData);
                    return Result.Failure<Void, Domain.Error>(errorDetails);
                }

                var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;
                var resolveTags = this.Tags != null ? await this.Tags.SelectAsync(async oa => (await oa.Resolve(providerContext)).GetValueOrThrowIfFailed()) : null;
                var tags = resolveTags?.Where(t => !string.IsNullOrEmpty(t)).Select(c => c.DataValue).ToList();
                var relationships = await RelationshipHelper.ResolveMessageRelationships(providerContext, this.Relationships, tenantId);
                var productId = providerContext.AutomationData.ContextManager.Product.Id;
                var organisationId = providerContext.AutomationData.ContextManager.Organisation.Id;
                var environment = providerContext.AutomationData.System.Environment;

                var saveCommand = new SaveSmsCommand(
                    tenantId,
                    sms.To,
                    sms.From,
                    sms.Message,
                    productId,
                    organisationId,
                    environment,
                    tags,
                    relationships);

                await this.mediator.Send(saveCommand);

                if (tags != null && tags.Any())
                {
                    (actionData as SendSmsActionData).Tags = tags;
                }

                if (relationships != null && relationships.Any())
                {
                    (actionData as SendSmsActionData).Relationships
                        = relationships.Select(r => new UBind.Domain.SerialisedEntitySchemaObject.Relationship(
                            r.RelationshipType,
                            r.SourceEntity,
                            r.TargetEntity)).ToList();
                }

                this.logger.LogInformation("\"SendSmsAction\" " + actionData.Alias + " - Success");
                return Result.Success<Void, Domain.Error>(default);
            }
        }
    }
}

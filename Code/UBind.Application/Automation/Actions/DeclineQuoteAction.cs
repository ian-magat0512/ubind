// <copyright file="DeclineQuoteAction.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Actions
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using StackExchange.Profiling;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Application.Commands.Quote;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.SerialisedEntitySchemaObject;
    using Void = UBind.Domain.Helpers.Void;

    /// <summary>
    /// An automation action that declines any quote except completed.
    /// </summary>
    public class DeclineQuoteAction : Action
    {
        private readonly IEntityProvider quoteEntityProvider;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;
        private readonly ICqrsMediator mediator;

        public DeclineQuoteAction(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IProvider<Data<bool>> runCondition,
            IEnumerable<ErrorCondition> beforeErrorConditions,
            IEnumerable<ErrorCondition> afterErrorConditions,
            IEnumerable<Action> onErrorActions,
            IEntityProvider quoteEntityProvider,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock,
            ICqrsMediator mediator)
            : base(name, alias, description, asynchronous, runCondition, beforeErrorConditions, afterErrorConditions, onErrorActions)
        {
            this.quoteEntityProvider = quoteEntityProvider;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
            this.mediator = mediator;
        }

        public override ActionData CreateActionData() => new DeclineQuoteActionData(this.Name, this.Alias, this.clock);

        public override bool IsReadOnly() => false;

        public override async Task<Result<Void, Domain.Error>> Execute(
            IProviderContext providerContext,
            ActionData actionData,
            bool isInternal = false)
        {
            using (MiniProfiler.Current.Step(nameof(DeclineQuoteAction) + "." + nameof(this.Execute)))
            {
                actionData.UpdateState(ActionState.Running);

                var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;
                try
                {
                    var resolveQuoteEntity = (await this.quoteEntityProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();
                    Quote quote = (Quote)resolveQuoteEntity.DataValue;
                    (actionData as DeclineQuoteActionData).QuoteId = quote.Id;
                    await this.mediator.Send(new DeclineQuoteCommand(tenantId, quote.Id, null));
                }
                catch (ErrorException ex)
                {
                    JObject errorData = await providerContext.GetDebugContext();
                    ex.EnrichAndRethrow(errorData);

                    throw;
                }

                return await Task.FromResult(Result.Success<Void, Domain.Error>(default));
            }
        }
    }
}

// <copyright file="ApproveQuoteAction.cs" company="uBind">
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
    using NodaTime;
    using StackExchange.Profiling;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Application.Commands.Quote;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.SerialisedEntitySchemaObject;
    using UBind.Domain.ValueTypes;
    using Void = UBind.Domain.Helpers.Void;

    /// <summary>
    /// An automation action that approves a specific quote with a restriction that it can only approve 'incomplete', 'review' or 'endorsement' quote states.
    /// </summary>
    public class ApproveQuoteAction : Action
    {
        private ICqrsMediator mediator;

        public ApproveQuoteAction(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IProvider<Data<bool>> runCondition,
            IEnumerable<ErrorCondition> beforeErrorConditions,
            IEnumerable<ErrorCondition> afterErrorConditions,
            IEnumerable<Action> onErrorActions,
            IEntityProvider quoteEntityProvider,
            ICqrsMediator mediator,
            IClock clock)
            : base(name, alias, description, asynchronous, runCondition, beforeErrorConditions, afterErrorConditions, onErrorActions)
        {
            this.mediator = mediator;
            this.Clock = clock;
            this.QuoteEntityProvider = quoteEntityProvider;
        }

        public IEntityProvider QuoteEntityProvider { get; }

        public IClock Clock { get; }

        public override ActionData CreateActionData() => new ApproveQuoteActionData(this.Name, this.Alias, this.Clock);

        public override bool IsReadOnly() => false;

        public override async Task<Result<Void, Domain.Error>> Execute(
            IProviderContext providerContext,
            ActionData actionData,
            bool isInternal = false)
        {
            using (MiniProfiler.Current.Step(nameof(ApproveQuoteAction) + "." + nameof(this.Execute)))
            {
                actionData.UpdateState(ActionState.Running);

                var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;
                if (!this.QuoteEntityProvider.IncludedProperties.Any())
                {
                    this.QuoteEntityProvider.IncludedProperties = new List<string> { "calculation" };
                }

                var resolveQuote = await this.QuoteEntityProvider.Resolve(providerContext);
                Quote quote = (Quote)resolveQuote.GetValueOrThrowIfFailed().DataValue;
                var quoteState = quote.State.ToLower();
                if (quoteState == StandardQuoteStates.Review.ToLower())
                {
                    await this.mediator.Send(new ApproveReviewedQuoteCommand(tenantId, quote.Id, null));
                }
                else if (quoteState == StandardQuoteStates.Endorsement.ToLower())
                {
                    await this.mediator.Send(new ApproveEndorsedQuoteCommand(tenantId, quote.Id, null));
                }
                else if (quoteState == StandardQuoteStates.Incomplete.ToLower())
                {
                    try
                    {
                        var calculationState = quote.Calculation.State;
                        if (calculationState == CalculationResult.BindableState)
                        {
                            await this.mediator.Send(new AutoApproveQuoteCommand(tenantId, quote.Id, null));
                        }
                        else
                        {
                            var errorData = await providerContext.GetDebugContext();
                            var error = Domain.Errors.Automation.ApproveQuoteAction.IncompleteWithoutBindingQuote(quote.Id, quoteState, calculationState, errorData);
                            throw new ErrorException(error);
                        }
                    }
                    catch (ErrorException e)
                    {
                        if (e.Error.Code == "quote.cannot.be.approved.without.a.calculation")
                        {
                            var errorData = await providerContext.GetDebugContext();
                            var error = Domain.Errors.Automation.ApproveQuoteAction.InvalidQuoteStateForApproval(quote.Id, quoteState, errorData);
                            throw new ErrorException(error);
                        }

                        throw e;
                    }
                }
                else
                {
                    var errorData = providerContext.GetDebugContext();
                    var error = Domain.Errors.Automation.ApproveQuoteAction.InvalidQuoteStateForApproval(quote.Id, quoteState, await errorData);
                    throw new ErrorException(error);
                }

                (actionData as ApproveQuoteActionData).QuoteId = quote.Id;
                return await Task.FromResult(Result.Success<Void, Domain.Error>(default));
            }
        }
    }
}

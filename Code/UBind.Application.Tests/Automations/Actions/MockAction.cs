// <copyright file="MockAction.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Actions;

using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UBind.Application.Automation.Actions;
using UBind.Application.Automation.Error;
using UBind.Application.Automation.Providers;
using UBind.Application.Automation;
using UBind.Domain;
using Action = Automation.Actions.Action;
using NodaTime;

public class MockAction : Action
{
    private readonly IClock clock;

    public MockAction(
        string name,
        string alias,
        string description,
        bool asynchronous,
        IProvider<Data<bool>> runCondition,
        IEnumerable<ErrorCondition> beforeRunConditions,
        IEnumerable<ErrorCondition> afterRunConditions,
        IEnumerable<IRunnableAction> errorActions,
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
        this.clock = clock;
    }

    public override async Task<Result<Domain.Helpers.Void, Domain.Error>> Execute(
        IProviderContext providerContext,
        ActionData actionData,
        bool isInternal = false)
    {
        // Simulate running for around 500ms
        await Task.Delay(1000);

        // For this example, we'll just return a successful result.
        return Result.Success<Domain.Helpers.Void, Error>(default);
    }

    public override ActionData CreateActionData()
    {
        var id = Guid.NewGuid();
        return new MockActionData(
            $"Mock Action Data {id}",
            $"mockActionData{id}",
            Application.Automation.Enums.ActionType.SetVariableAction,
            this.clock);
    }

    public override bool IsReadOnly()
    {
        return false;
    }
}

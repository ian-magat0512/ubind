// <copyright file="ConditionalTrigger.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Triggers;

using UBind.Application.Automation.Providers;

// <summary>
// Represents an abstract class for conditional triggers, inheriting from the base Trigger class.
// </summary>
public abstract class ConditionalTrigger : Trigger
{
    // <summary>
    // Initializes a new instance of the <see cref="ConditionalTrigger"/> class.
    // </summary>
    // <param name="name">The name of the trigger.</param>
    // <param name="alias">The alias of the trigger.</param>
    // <param name="description">The description of the trigger.</param>
    // <param name="runCondition">The condition that determines if the trigger should run.</param>
    protected ConditionalTrigger(
        string name, string alias, string description, IProvider<Data<bool>>? runCondition)
        : base(name, alias, description)
    {
        this.RunCondition = runCondition;
    }

    /// <summary>
    /// Gets the run condition of the trigger.
    /// </summary>
    public IProvider<Data<bool>>? RunCondition { get; }
}

// <copyright file="ActionType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Enums
{
    using System.ComponentModel;

    /// <summary>
    /// Refers to the different types of automation action.
    /// </summary>
    public enum ActionType
    {
        /// <summary>
        /// An action that fires an http request.
        /// </summary>
        [Description("httpRequestAction")]
        HttpRequestAction,

        /// <summary>
        /// An action that sends an email.
        /// </summary>
        [Description("sendEmailAction")]
        SendEmailAction,

        /// <summary>
        /// An action that sends an sms.
        /// </summary>
        [Description("sendSmsAction")]
        SendSmsAction,

        /// <summary>
        /// An action that raises an event.
        /// </summary>
        [Description("raiseEventAction")]
        RaiseEventAction,

        /// <summary>
        /// An action that raises an error
        /// </summary>
        [Description("raiseErrorAction")]
        RaiseErrorAction,

        /// <summary>
        /// An action that results in new data objects to be created.
        /// </summary>
        [Description("transformDataAction")]
        TransformDataAction,

        /// <summary>
        /// An action that is essentially a collection of more specific actions,
        /// </summary>
        [Description("groupAction")]
        GroupAction,

        /// <summary>
        /// An action that iterates over a list of items, running a list of actions for each item.
        /// </summary>
        [Description("iterateAction")]
        IterateAction,

        /// <summary>
        /// An action that runs another automation, referenced by alias, with an optional custom data object.
        /// </summary>
        [Description("runAutomationAction")]
        RunAutomationAction,

        /// <summary>
        /// An action that attach file(s) to an entity or list of entities.
        /// </summary>
        [Description("attachFilesToEntitiesAction")]
        AttachFilesToEntitiesAction,

        /// <summary>
        /// An action that replicates the functionality in the existing 'policy' operation available to the webFormApp.
        /// </summary>
        [Description("issuePolicyAction")]
        IssuePolicyAction,

        /// <summary>
        /// An action that sets the value of a variable.
        /// </summary>
        [Description("setVariableAction")]
        SetVariableAction,

        /// <summary>
        /// An action that sets the value of a variable.
        /// </summary>
        [Description("incrementAdditionalPropertyValueAction")]
        IncrementAdditionalPropertyValueAction,

        /// <summary>
        /// An action that attach file(s) to an entity or list of entities.
        /// </summary>
        [Description("setAdditionalPropertyValueAction")]
        SetAdditionalPropertyValueAction,

        /// <summary>
        /// An action that replicates the functionality in the existing 'policy' operation available to the webFormApp.
        /// </summary>
        [Description("performQuoteCalculationAction")]
        PerformQuoteCalculationAction,

        /// <summary>
        /// An action that moves a quote from the incomplete, review, or endorsement states to the approved state.
        /// Requires that the quote is Bindable, which means it will need a calculation to have been done on it
        /// with the status "bindingQuote".
        /// </summary>
        [Description("approveQuoteAction")]
        ApproveQuoteAction,

        /// <summary>
        /// An action that creates a new organisation.
        /// </summary>
        [Description("createOrganisationAction")]
        CreateOrganisationAction,

        /// <summary>
        /// An action that moves a quote from the approved, review, or endorsement states to the incomplete state.
        /// Requires that the quote is Bindable, which means it will need a calculation to have been done on it
        /// with the status "bindingQuote".
        /// </summary>
        [Description("returnQuoteAction")]
        ReturnQuoteAction,

        /// <summary>
        /// An action that uploads a file.
        /// </summary>
        [Description("uploadFileAction")]
        UploadFileAction,

        /// <summary>
        /// An action that declines a quote that is not yet completed.
        /// </summary>
        [Description("declineQuoteAction")]
        DeclineQuoteAction,

        /// <summary>
        /// An action that creates a new user.
        /// </summary>
        [Description("createUserAction")]
        CreateUserAction,

        /// <summary>
        /// An action that creates a new quote.
        /// </summary>
        [Description("createQuoteAction")]
        CreateQuoteAction,

        /// <summary>
        /// An action that renews an existing policy.
        /// </summary>
        [Description("renewPolicyAction")]
        RenewPolicyAction,
    }
}

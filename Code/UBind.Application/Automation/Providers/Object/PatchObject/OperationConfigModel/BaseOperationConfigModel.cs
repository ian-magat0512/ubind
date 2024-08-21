// <copyright file="BaseOperationConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Object.PatchObject
{
    using System;

    /// <summary>
    /// Base class of all operation config models.
    /// </summary>
    public class BaseOperationConfigModel
    {
        public virtual BaseOperation Build(IServiceProvider dependencyProvider)
        {
            throw new NotImplementedException();
        }

        protected PrePatchDirective GetPrePatchAction(string? actionName)
        {
            if (string.IsNullOrWhiteSpace(actionName))
            {
                return PrePatchDirective.RaiseError;
            }

            return (PrePatchDirective)Enum.Parse(typeof(PrePatchDirective), actionName, true);
        }
    }
}

// <copyright file="RemoveOperationConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Object.PatchObject
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This class is needed for creating an instance of <see cref="RemoveOperation"/> from the JSON configuration.
    /// </summary>
    public class RemoveOperationConfigModel : BaseOperationConfigModel, IBuilder<BaseOperation>
    {
        [JsonProperty("path")]

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public IBuilder<IProvider<Data<string>>> PathProviderBuilder { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [JsonProperty("whenPropertyNotFound")]
        public string? WhenPropertyNotFound { get; set; }

        public override BaseOperation Build(IServiceProvider dependencyProvider)
        {
            return new RemoveOperation(
                this.PathProviderBuilder.Build(dependencyProvider),
                this.GetPrePatchAction(this.WhenPropertyNotFound));
        }
    }
}

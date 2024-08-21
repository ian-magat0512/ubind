// <copyright file="ReplaceOperationConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Object.PatchObject
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This class is needed for creating an instance of <see cref="ReplaceOperation"/> from the JSON configuration.
    /// </summary>
    public class ReplaceOperationConfigModel : BaseOperationConfigModel, IBuilder<BaseOperation>
    {
        [JsonProperty("path")]
        public IBuilder<IProvider<Data<string>>>? PathProviderBuilder { get; set; }

        [JsonProperty("value")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public IBuilder<IProvider<IData>> ValueProviderBuilder { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [JsonProperty("whenPropertyNotFound")]
        public string? WhenPropertyNotFound { get; set; }

        [JsonProperty("whenParentPropertyNotFound")]
        public string? WhenParentPropertyNotFound { get; set; }

        public override BaseOperation Build(IServiceProvider dependencyProvider)
        {
            return new ReplaceOperation(
                this.PathProviderBuilder?.Build(dependencyProvider),
                this.ValueProviderBuilder.Build(dependencyProvider),
                this.GetPrePatchAction(this.WhenPropertyNotFound),
                this.GetPrePatchAction(this.WhenParentPropertyNotFound));
        }
    }
}

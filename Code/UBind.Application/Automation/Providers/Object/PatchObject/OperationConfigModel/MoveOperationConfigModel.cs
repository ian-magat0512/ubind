// <copyright file="MoveOperationConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Object.PatchObject
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This class is needed for creating an instance of <see cref="MoveOperation"/> from the JSON configuration.
    /// </summary>
    public class MoveOperationConfigModel : BaseOperationConfigModel, IBuilder<BaseOperation>
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [JsonProperty("from")]
        public IBuilder<IProvider<Data<string>>> FromProviderBuilder { get; set; }

        [JsonProperty("to")]
        public IBuilder<IProvider<Data<string>>> ToProviderBuilder { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [JsonProperty("whenSourcePropertyNotFound")]
        public string? WhenSourcePropertyNotFound { get; set; }

        [JsonProperty("whenDestinationParentPropertyNotFound")]
        public string? WhenDestinationParentPropertyNotFound { get; set; }

        [JsonProperty("whenDestinationPropertyAlreadyExists")]
        public string? WhenDestinationPropertyAlreadyExists { get; set; }

        public override BaseOperation Build(IServiceProvider dependencyProvider)
        {
            return new MoveOperation(
                this.FromProviderBuilder.Build(dependencyProvider),
                this.ToProviderBuilder.Build(dependencyProvider),
                this.GetPrePatchAction(this.WhenSourcePropertyNotFound),
                this.GetPrePatchAction(this.WhenDestinationParentPropertyNotFound),
                this.GetPrePatchAction(this.WhenDestinationPropertyAlreadyExists));
        }
    }
}

// <copyright file="UploadFileActionConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.File;

    public class UploadFileActionConfigModel : BaseActionConfigurationModel, IBuilder<Action>
    {
        [JsonConstructor]
        public UploadFileActionConfigModel(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IBuilder<IProvider<Data<bool>>> runCondition,
            IEnumerable<ErrorConditionConfigModel> beforeRunErrorConditions,
            IEnumerable<ErrorConditionConfigModel> afterRunErrorConditions,
            IEnumerable<IBuilder<Action>> onErrorActions,
            IBuilder<IProvider<Data<string>>> protocol,
            IBuilder<IProvider<Data<string>>> hostName,
            IBuilder<IProvider<Data<long>>> port,
            IBuilder<IProvider<Data<string>>> username,
            IBuilder<IProvider<Data<string>>> password,
            IBuilder<IProvider<Data<string>>> privateKey,
            IBuilder<IProvider<Data<string>>> privateKeyPassword,
            IBuilder<IProvider<Data<FileInfo>>> file,
            IBuilder<IProvider<Data<string>>> remotePath,
            IBuilder<IProvider<Data<bool>>> createMissingFolders,
            IBuilder<IProvider<Data<bool>>> overwriteExistingFile)
             : base(
              name,
              alias,
              description,
              asynchronous,
              runCondition,
              beforeRunErrorConditions,
              afterRunErrorConditions,
              onErrorActions)
        {
            this.ProtocolProvider = protocol;
            this.HostNameProvider = hostName;
            this.PortProvider = port;
            this.UsernameProvider = username;
            this.PasswordProvider = password;
            this.PrivateKeyProvider = privateKey;
            this.PrivateKeyPasswordProvider = privateKeyPassword;
            this.FileProvider = file;
            this.RemotePathProvider = remotePath;
            this.CreateMissingFolderProvider = createMissingFolders;
            this.OverrideExistingFileProvider = overwriteExistingFile;
        }

        [JsonProperty("protocol")]
        public IBuilder<IProvider<Data<string>>> ProtocolProvider { get; set; }

        [JsonProperty("hostName")]
        public IBuilder<IProvider<Data<string>>> HostNameProvider { get; set; }

        [JsonProperty("port")]
        public IBuilder<IProvider<Data<long>>> PortProvider { get; private set; }

        [JsonProperty("username")]
        public IBuilder<IProvider<Data<string>>> UsernameProvider { get; private set; }

        [JsonProperty("password")]
        public IBuilder<IProvider<Data<string>>> PasswordProvider { get; private set; }

        [JsonProperty("privateKey")]
        public IBuilder<IProvider<Data<string>>> PrivateKeyProvider { get; private set; }

        [JsonProperty("privateKeyPassword")]
        public IBuilder<IProvider<Data<string>>> PrivateKeyPasswordProvider { get; private set; }

        [JsonProperty("file")]
        public IBuilder<IProvider<Data<FileInfo>>> FileProvider { get; private set; }

        [JsonProperty("removePath")]
        public IBuilder<IProvider<Data<string>>> RemotePathProvider { get; private set; }

        [JsonProperty("createMissingFolder")]
        public IBuilder<IProvider<Data<bool>>> CreateMissingFolderProvider { get; private set; }

        [JsonProperty("overrideExistingFile")]
        public IBuilder<IProvider<Data<bool>>> OverrideExistingFileProvider { get; private set; }

        public override Action Build(IServiceProvider dependencyProvider)
        {
            return new UploadFileAction(
                this.Name,
                this.Alias,
                this.Description,
                this.Asynchronous,
                this.RunCondition?.Build(dependencyProvider),
                this.BeforeRunErrorConditions?.Select(bc => bc.Build(dependencyProvider)),
                this.AfterRunErrorConditions?.Select(ac => ac.Build(dependencyProvider)),
                this.OnErrorActions?.Select(ea => ea.Build(dependencyProvider)),
                this.ProtocolProvider.Build(dependencyProvider),
                this.HostNameProvider.Build(dependencyProvider),
                this.PortProvider?.Build(dependencyProvider),
                this.FileProvider.Build(dependencyProvider),
                this.UsernameProvider.Build(dependencyProvider),
                this.PasswordProvider?.Build(dependencyProvider),
                this.PrivateKeyProvider?.Build(dependencyProvider),
                this.PrivateKeyPasswordProvider?.Build(dependencyProvider),
                this.RemotePathProvider?.Build(dependencyProvider),
                this.CreateMissingFolderProvider?.Build(dependencyProvider),
                this.OverrideExistingFileProvider?.Build(dependencyProvider),
                dependencyProvider.GetRequiredService<IClock>());
        }
    }
}

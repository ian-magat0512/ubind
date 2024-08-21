// <copyright file="UploadFileAction.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Actions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using Quartz.Util;
    using Renci.SshNet;
    using Renci.SshNet.Common;
    using StackExchange.Profiling;
    using UBind.Application.Automation.Actions.UploadFile;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using Void = UBind.Domain.Helpers.Void;

    public class UploadFileAction : Action
    {
        private readonly IClock clock;
        private Guid tenantId;
        private IProviderContext? providerContext;
        private string? hostname;
        private string? username;
        private string? protocolStr;
        private Providers.File.FileInfo? file;
        private string? password;
        private bool? canOverrideExistingFile;
        private bool? createMissingFolders;
        private string? remotePath;
        private long? port;
        private string? privateKey;
        private string? privateKeyPassword;
        private ulong bytesAlreadyUploaded;

        /// <summary>
        /// This is used to indicate where an error happened.
        /// this is useful because some error codes from exceptions are all the same, but it came from different places.
        /// useful for deciding which error message to use.
        /// </summary>
        private string? internalErrorCode;

        public UploadFileAction(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IProvider<Data<bool>>? runCondition,
            IEnumerable<ErrorCondition>? beforeRunConditions,
            IEnumerable<ErrorCondition>? afterRunConditions,
            IEnumerable<IRunnableAction>? errorActions,
            IProvider<Data<string>> protocolProvider,
            IProvider<Data<string>> hostnameProvider,
            IProvider<Data<long>>? portProvider,
            IProvider<Data<Providers.File.FileInfo>> fileProvider,
            IProvider<Data<string>> usernameProvider,
            IProvider<Data<string>>? passwordProvider,
            IProvider<Data<string>>? privateKeyProvider,
            IProvider<Data<string>>? privateKeyPasswordProvider,
            IProvider<Data<string>>? remotePathProvider,
            IProvider<Data<bool>>? createMissingFoldersProvider,
            IProvider<Data<bool>>? overwriteExistingFileProvider,
            IClock clock)
            : base(name, alias, description, asynchronous, runCondition, beforeRunConditions, afterRunConditions, errorActions)
        {
            this.ProtocolProvider = protocolProvider;
            this.HostnameProvider = hostnameProvider;
            this.PortProvider = portProvider;
            this.FileProvider = fileProvider;
            this.UsernameProvider = usernameProvider;
            this.PasswordProvider = passwordProvider;
            this.PrivateKeyProvider = privateKeyProvider;
            this.PrivateKeyPasswordProvider = privateKeyPasswordProvider;
            this.RemotePathProvider = remotePathProvider;
            this.CreateMissingFoldersProvider = createMissingFoldersProvider;
            this.OverwriteExistingFileProvider = overwriteExistingFileProvider;
            this.clock = clock;
        }

        public IProvider<Data<string>> ProtocolProvider { get; set; }

        public IProvider<Data<string>> HostnameProvider { get; set; }

        public IProvider<Data<long>>? PortProvider { get; set; }

        public IProvider<Data<Providers.File.FileInfo>> FileProvider { get; set; }

        public IProvider<Data<string>> UsernameProvider { get; set; }

        public IProvider<Data<string>>? PasswordProvider { get; set; }

        public IProvider<Data<string>>? PrivateKeyProvider { get; set; }

        public IProvider<Data<string>>? PrivateKeyPasswordProvider { get; set; }

        public IProvider<Data<string>>? RemotePathProvider { get; }

        public IProvider<Data<bool>>? CreateMissingFoldersProvider { get; }

        public IProvider<Data<bool>>? OverwriteExistingFileProvider { get; }

        public override ActionData CreateActionData() => new UploadFileActionData(this.Name, this.Alias, this.clock);

        public override bool IsReadOnly() => false;

        public async override Task<Result<Void, Domain.Error>> Execute(
            IProviderContext providerContext,
            ActionData actionData,
            bool isInternal = false)
        {
            using (MiniProfiler.Current.Step(nameof(UploadFileAction) + "." + nameof(this.Execute)))
            {
                actionData.UpdateState(ActionState.Running);

                this.tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;
                this.providerContext = providerContext;
                this.hostname = (await this.HostnameProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
                this.username = (await this.UsernameProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
                this.protocolStr = (await this.ProtocolProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue.ToUpper();
                this.file = (await this.FileProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
                this.password = (await this.PasswordProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
                this.canOverrideExistingFile = (await this.OverwriteExistingFileProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
                this.createMissingFolders = (await this.CreateMissingFoldersProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
                this.remotePath = (await this.RemotePathProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
                this.port = (await this.PortProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
                this.privateKey = (await this.PrivateKeyProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
                this.privateKeyPassword = (await this.PrivateKeyPasswordProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
                Enum.TryParse(this.protocolStr, true, out Protocol protocol);
                switch (protocol)
                {
                    case Protocol.SFTP:
                        return await this.ProcessSftp(actionData);
                    default:
                        // throw error if protocol is not supported.
                        var errorData = await this.GetDebugContext();
                        var error = Domain.Errors.Automation.UploadFileAction.InvalidProtocol(this.protocolStr, errorData);
                        throw new ErrorException(error);
                }
            }
        }

        /// <summary>
        /// Create Keyboard Authentication Method instance that use to listen for the server keyboard prompt(s) to authenticate
        /// </summary>
        /// <returns>instance of type KeyboardInteractiveAuthenticationMethod</returns>
        private async Task<KeyboardInteractiveAuthenticationMethod> GetSftpKeyboardAuthenticationMethodInstance()
        {
            KeyboardInteractiveAuthenticationMethod instance = new KeyboardInteractiveAuthenticationMethod(this.username);
            instance.AuthenticationPrompt += new EventHandler<AuthenticationPromptEventArgs>(this.OnAuthenticationPrompt);
            return await Task.FromResult(instance);
        }

        /// <summary>
        /// Create a Password Authentication Method instance that is used to authenticate using a username and password.
        /// </summary>
        /// <returns>instance of type PasswordAuthenticationMethod</returns>
        private async Task<PasswordAuthenticationMethod> GetSftpPasswordAuthenticationMethodInstance()
        {
            PasswordAuthenticationMethod instance = new PasswordAuthenticationMethod(this.username, this.password);
            return await Task.FromResult(instance);
        }

        /// <summary>
        /// Create a Private Key Authentication Method that is used to authenticate using a private key file.
        /// </summary>
        /// <returns>instance of type GetSftpPrivateKeyAuthenticationMethod</returns>
        private async Task<PrivateKeyAuthenticationMethod> GetSftpPrivateKeyAuthenticationMethodInstance()
        {
            PrivateKeyFile? privateKeyFile = null;
            if (this.privateKey != null)
            {
                MemoryStream privateKeyStream = new MemoryStream();
                using (StreamWriter writer = new StreamWriter(privateKeyStream))
                {
                    writer.Write(this.privateKey);
                    writer.Flush();
                    privateKeyStream.Position = 0;

                    privateKeyFile = this.privateKeyPassword.IsNullOrWhiteSpace()
                        ? new PrivateKeyFile(privateKeyStream)
                        : new PrivateKeyFile(privateKeyStream, this.privateKeyPassword);
                }
            }

            PrivateKeyAuthenticationMethod instance = new PrivateKeyAuthenticationMethod(this.username, privateKeyFile);
            return await Task.FromResult(instance);
        }

        /// <summary>
        /// The event callback that will be fired/executed by the created instance from method GetSftpKeyboardAuthenticationMethodInstance(string username)
        /// </summary>
        /// <param name="sender">The source object that fires the event</param>
        /// <param name="eventArguments">The arguments passed by the source object</param>
        private void OnAuthenticationPrompt(object? sender, AuthenticationPromptEventArgs eventArguments)
        {
            if (!eventArguments.Prompts.Any())
            {
                return;
            }

            foreach (AuthenticationPrompt prompt in eventArguments.Prompts)
            {
                this.PromptResponder(prompt);
            }
        }

        private void PromptResponder(AuthenticationPrompt prompt)
        {
            if (prompt.Request.Contains("Password"))
            {
                prompt.Response = this.password ?? string.Empty;
            }
            else if (prompt.Request.Contains("Passphrase"))
            {
                prompt.Response = this.privateKeyPassword ?? string.Empty;
            }
        }

        /// <summary>
        /// Creates an instance of SftpClient with the necessary authentication methods listed below in sequence.
        /// The order of authentication should be followed
        /// PrivateKey
        /// Username and Password
        /// Keyboard Prompts (Passphrase or Password)
        /// </summary>
        /// <returns>Instance of type SftpClient</returns>
        /// <exception cref="ErrorException"></exception>
        private async Task<SftpClient> GetSftpClientInstance()
        {
            List<AuthenticationMethod> authenticationMethods = new List<AuthenticationMethod>();
            List<string> addtionalDetails = new List<string>();
            JObject data = new JObject()
            {
                { "method" , "GetSftpClientInstance" },
                { "hostname" , string.IsNullOrEmpty(this.hostname) ? "<null>" : "redacted" },
                { "port" , string.IsNullOrEmpty(this.port.ToString()) ? "<null>" : "redacted" },
                { "username" ,  string.IsNullOrEmpty(this.username) ? "<null>" : "redacted" },
                { "password" ,  string.IsNullOrEmpty(this.password) ? "<null>" : "redacted" },
                { "privatekey" ,  string.IsNullOrEmpty(this.privateKey) ? "<null>" : "redacted" },
                { "privateKeyPassword" ,  string.IsNullOrEmpty(this.privateKeyPassword) ? "<empty>" : "redacted" },
            };

            if (string.IsNullOrEmpty(this.hostname))
            {
                addtionalDetails.Add("hostname value not set");
                throw new ErrorException(Errors.Automation.AdditionalPropertyValueNotSet("hostname", "hostname", addtionalDetails, data));
            }

            int port = (int)(this.port ?? 22);
            if (string.IsNullOrEmpty(this.port.ToString()))
            {
                addtionalDetails.Add("port value not set -- automatic set to default port 22");
            }

            if (!string.IsNullOrEmpty(this.privateKey))
            {
                PrivateKeyAuthenticationMethod privateKeyAuthenticationMethodInstance = await this.GetSftpPrivateKeyAuthenticationMethodInstance();
                authenticationMethods.Add(privateKeyAuthenticationMethodInstance);
            }

            if (!string.IsNullOrEmpty(this.username))
            {
                if (string.IsNullOrEmpty(this.password))
                {
                    KeyboardInteractiveAuthenticationMethod keyboardAuthenticationMethodInstance = await this.GetSftpKeyboardAuthenticationMethodInstance();
                    authenticationMethods.Add(keyboardAuthenticationMethodInstance);
                }
                else
                {
                    PasswordAuthenticationMethod passwordAuthenticationInstance = await this.GetSftpPasswordAuthenticationMethodInstance();
                    authenticationMethods.Add(passwordAuthenticationInstance);
                }
            }

            ConnectionInfo connectionInfo = new ConnectionInfo(this.hostname, port, this.username, authenticationMethods.ToArray());
            return new SftpClient(connectionInfo);
        }

        private async Task<Result<Void, Error>> ProcessSftp(ActionData actionData)
        {
            this.protocolStr ??= string.Empty;
            this.hostname ??= string.Empty;
            this.username ??= string.Empty;

            if (this.file == null)
            {
                throw new ErrorException(Errors.File.NotFound(string.Empty, string.Empty));
            }
            try
            {
                // Create an SFTP client instance
                using (var sftp = await this.GetSftpClientInstance())
                {
                    // Connect to the SFTP server
                    sftp.Connect();

                    if (!string.IsNullOrWhiteSpace(this.remotePath))
                    {
                        // Change to the remote directory
                        if (this.createMissingFolders.HasValue && this.createMissingFolders.Value)
                        {
                            bool folderExists = this.FolderExists(sftp, this.remotePath);
                            if (!folderExists)
                            {
                                this.internalErrorCode = "createdirectory";
                                sftp.CreateDirectory(this.remotePath);
                            }
                        }

                        this.internalErrorCode = "changedirectory";
                        sftp.ChangeDirectory(this.remotePath);
                    }

                    Stream stream = new MemoryStream(this.file.Content);
                    Action<ulong> progressAction = (bytesUploaded) =>
                    {
                        this.bytesAlreadyUploaded = bytesUploaded;
                    };

                    this.internalErrorCode = "uploadfile";
                    sftp.UploadFile(
                        stream,
                        this.file.FileName.ToString(),
                        this.canOverrideExistingFile.HasValue
                            ? this.canOverrideExistingFile.Value
                            : false,
                        progressAction);

                    ((UploadFileActionData)actionData).SetValues(
                        this.protocolStr,
                        this.hostname,
                        this.port,
                        this.file.FileName.ToString(),
                        (long)this.bytesAlreadyUploaded,
                        this.remotePath,
                        this.createMissingFolders ?? false,
                        this.canOverrideExistingFile ?? false);

                    // Disconnect from the SFTP server
                    sftp.Disconnect();
                }
            }
            catch (SshAuthenticationException e)
            {
                // AC 2. It should explain that the password was incorrect.
                if (e.Message == "Permission denied (password).")
                {
                    var errorData = await this.GetDebugContext();
                    var error = Domain.Errors.Automation.UploadFileAction.PasswordIncorrect(this.protocolStr, this.hostname, errorData);
                    throw new ErrorException(error);
                }

                // public or private key is incorrect.
                if (e.Message == "Permission denied (publickey).")
                {
                    var errorData = await this.GetDebugContext();
                    var error = Domain.Errors.Automation.UploadFileAction.PrivateKeyIncorrect(this.protocolStr, this.hostname, errorData);
                    throw new ErrorException(error);
                }

                throw;
            }
            catch (SftpPermissionDeniedException ex)
            {
                // somehow the permission is denied to upload even if its correct credentials.
                // AC 8. It should explain that the authenticated user does not have access to one of the folders in the specified remote path
                if (ex.Message == "Permission denied" && this.internalErrorCode == "changedirectory")
                {
                    var errorData = await this.GetDebugContext();
                    var error = Domain.Errors.Automation.UploadFileAction.PermissionDenied(this.protocolStr, this.hostname, this.remotePath, errorData);
                    throw new ErrorException(error);
                }

                // AC 10. It should explain that the authenticated user does not have write access to the target folder
                // AC 16. It should explain that the authenticated user does not have permission to overwrite the existing file in the target location
                if (ex.Message == "Permission denied" && this.internalErrorCode == "uploadfile")
                {
                    var errorData = await this.GetDebugContext();
                    var error = Domain.Errors.Automation.UploadFileAction.WriteAccessDenied(this.protocolStr, this.hostname, this.remotePath, errorData);
                    throw new ErrorException(error);
                }

                // AC 12. It should explain that the authenticated user does not have permission to create folders in the target location
                if (ex.Message == "Permission denied" && this.internalErrorCode == "createdirectory")
                {
                    var errorData = await this.GetDebugContext();
                    var error = Domain.Errors.Automation.UploadFileAction.CreateFolderDenied(this.protocolStr, this.hostname, this.remotePath, errorData);
                    throw new ErrorException(error);
                }

                throw;
            }
            catch (SocketException e)
            {
                // AC 7 - It should explain that the remote host was unreachable
                if (e.SocketErrorCode == SocketError.HostNotFound)
                {
                    var errorData = await this.GetDebugContext();
                    var error = Domain.Errors.Automation.UploadFileAction.HostnameInvalid(this.protocolStr, this.hostname, errorData);
                    throw new ErrorException(error);
                }

                // A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond
                // Maybe not connected via VPN.
                if (e.SocketErrorCode == SocketError.TimedOut)
                {
                    var errorData = await this.GetDebugContext();
                    var error = Domain.Errors.Automation.UploadFileAction.TimeoutException(this.protocolStr, this.hostname, errorData);
                    throw new ErrorException(error);
                }

                // Incomplete credentials (authentication methods credentials)
                if (e.SocketErrorCode == SocketError.ConnectionRefused)
                {
                    var errorData = await this.GetDebugContext();
                    var error = Domain.Errors.Automation.UploadFileAction.ConnectionRefusedException(this.protocolStr, this.hostname, errorData);
                    throw new ErrorException(error);
                }

                throw;
            }
            catch (SftpPathNotFoundException e)
            {
                // AC 13. should explain that the location identified by the remotePath does not exist, and the createMissingFolders parameter is false or omitted
                // AC 14. It should explain that the location identified by the remotePath does not exist, and the createMissingFolders parameter is false or omitted
                if (e.Message == "No such file")
                {
                    var errorData = await this.GetDebugContext();
                    var error = Domain.Errors.Automation.UploadFileAction.PathDoesntExist(this.protocolStr, this.hostname, this.remotePath, errorData);
                    throw new ErrorException(error);
                }

                throw;
            }
            catch (SshException e)
            {
                // AC 17. t should explain that there is an existing file with the same name in the target location, and the replaceExistingFile parameter is false or omitted
                // AC 18. t should explain that there is an existing file with the same name in the target location, and the replaceExistingFile parameter is false or omitted
                if (e.Message == "Failure")
                {
                    var errorData = await this.GetDebugContext();
                    var error = Domain.Errors.Automation.UploadFileAction.OverrideExistingFileDenied(this.protocolStr, this.file.FileName.ToString(), this.hostname, this.remotePath, errorData);
                    throw new ErrorException(error);
                }

                // Message: Connection failed to establish within 30000 milliseconds. somehow a connection failure.
                if (e.Message == "Connection failed to establish within 30000 milliseconds. somehow a connection failure.")
                {
                    var errorData = await this.GetDebugContext();
                    var error = Domain.Errors.Automation.UploadFileAction.TimeoutException(this.protocolStr, this.hostname, errorData);
                    throw new ErrorException(error);
                }

                throw;
            }

            return await Task.FromResult(Result.Success<Void, Domain.Error>(default));
        }

        private bool FolderExists(SftpClient sftp, string remotePath)
        {
            try
            {
                remotePath = remotePath.Replace("//", "/").Replace("./", sftp.WorkingDirectory + "/");
                var list = sftp.ListDirectory(remotePath);
                if (list.Any())
                {
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

        private async Task<JObject> GetDebugContext()
        {
            if (this.providerContext == null)
            {
                throw new ErrorException(Errors.General.Unexpected("providerContext was not initialized"));
            }

            var data = await this.providerContext.GetDebugContext();
            data.Add("hostname", this.hostname);
            data.Add("username", this.username);
            data.Add("protocol", this.protocolStr);
            if (this.remotePath != null)
            {
                data.Add("remotePath", this.remotePath);
            }

            if (this.port > 0)
            {
                data.Add("port", this.port);
            }

            data.Add("createMissingFolders", this.createMissingFolders ?? false);
            data.Add("canOverrideFile", this.canOverrideExistingFile ?? false);
            if (this.bytesAlreadyUploaded > 0)
            {
                data.Add("uploadProgressInBytes", this.bytesAlreadyUploaded);
            }

            return data;
        }
    }
}

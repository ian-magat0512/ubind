// <copyright file="UploadFileActionData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Actions
{
    using Humanizer;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation.Enums;

    public class UploadFileActionData : ActionData
    {
        public UploadFileActionData(
            string name,
            string alias,
            IClock clock)
            : base(name, alias, ActionType.UploadFileAction, clock)
        {
        }

        [JsonConstructor]
        public UploadFileActionData()
            : base(ActionType.UploadFileAction)
        {
        }

        [JsonProperty("protocol")]
        public string? Protocol { get; set; }

        [JsonProperty("hostname")]
        public string? Hostname { get; set; }

        [JsonProperty("overwriteExistingFile", NullValueHandling = NullValueHandling.Ignore)]
        public bool OverwriteExistingFile { get; set; }

        [JsonProperty("createMissingFolders", NullValueHandling = NullValueHandling.Ignore)]
        public bool CreateMissingFolders { get; set; }

        [JsonProperty("remotePath", NullValueHandling = NullValueHandling.Ignore)]
        public string? RemotePath { get; set; }

        [JsonProperty("port", NullValueHandling = NullValueHandling.Ignore)]
        public long? Port { get; set; }

        [JsonProperty("fileName", NullValueHandling = NullValueHandling.Ignore)]
        public string? FileName { get; set; }

        [JsonProperty("fileSizeBytes", NullValueHandling = NullValueHandling.Ignore)]
        public long? FileSizeBytes { get; set; }

        [JsonProperty("fileSize", NullValueHandling = NullValueHandling.Ignore)]
        public string? FileSize { get; set; }

        internal void SetValues(
            string protocol,
            string hostname,
            long? port,
            string fileName,
            long bytesAlreadyUploaded,
            string? remotePath,
            bool createMissingFolders,
            bool overwriteExistingFile)
        {
            this.Protocol = protocol?.ToUpper();
            this.Hostname = hostname;
            this.Port = port == 0 ? null : port;
            this.FileName = fileName;
            this.FileSizeBytes = bytesAlreadyUploaded;
            this.FileSize = bytesAlreadyUploaded.Bytes().Humanize("0.0");
            this.RemotePath = remotePath;
            this.CreateMissingFolders = createMissingFolders;
            this.OverwriteExistingFile = overwriteExistingFile;
        }
    }
}

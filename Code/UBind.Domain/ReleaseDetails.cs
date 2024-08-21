// <copyright file="ReleaseDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Text;
    using MoreLinq;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Dto;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// All the details that a release needs to be usable.
    /// </summary>
    public class ReleaseDetails : Entity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseDetails"/> class.
        /// </summary>
        /// <param name="createdTimestamp">The time the release was created.</param>
        /// <param name="appType">The type of of web form app the details are for.</param>
        public ReleaseDetails(
            WebFormAppType appType,
            Instant createdTimestamp)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            this.AppType = appType;
        }

        public ReleaseDetails(
            WebFormAppType appType,
            string configurationJson,
            string workflowJson,
            string? exportsJson,
            string? automationsJson,
            string? paymentFormJson,
            string? paymentJson,
            string? fundingJson,
            string? productJson,
            IEnumerable<Asset> files,
            IEnumerable<Asset> assets,
            byte[] spreadsheetBytes,
            Instant createdTimestamp)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            this.ConfigurationJson = configurationJson;
            this.WorkflowJson = workflowJson;
            this.ExportsJson = exportsJson;
            this.AutomationsJson = automationsJson;
            this.PaymentFormJson = paymentFormJson;
            this.PaymentJson = paymentJson;
            this.FundingJson = fundingJson;
            this.ProductJson = productJson;
            this.Files = new List<Asset>(files);
            this.Assets = new List<Asset>(assets);
            this.FlexCelWorkbook = spreadsheetBytes;
            this.AppType = appType;
        }

        public ReleaseDetails(ReleaseDetails other, Instant createdTimestamp)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            this.ConfigurationJson = other.ConfigurationJson;
            this.FormConfigurationJsonLastModifiedTicksSinceEpoch = other.FormConfigurationJsonLastModifiedTicksSinceEpoch;
            this.WorkflowJson = other.WorkflowJson;
            this.WorkflowJsonLastModifiedTicksSinceEpoch = other.WorkflowJsonLastModifiedTicksSinceEpoch;
            this.ExportsJson = other.ExportsJson;
            this.IntegrationsJsonLastModifiedTicksSinceEpoch = other.IntegrationsJsonLastModifiedTicksSinceEpoch;
            this.AutomationsJson = other.AutomationsJson;
            this.AutomationsJsonLastModifiedTicksSinceEpoch = other.AutomationsJsonLastModifiedTicksSinceEpoch;
            this.PaymentFormJson = other.PaymentFormJson;
            this.PaymentFormJsonLastModifiedTicksSinceEpoch = other.PaymentFormJsonLastModifiedTicksSinceEpoch;
            this.PaymentJson = other.PaymentJson;
            this.PaymentJsonLastModifiedTicksSinceEpoch = other.PaymentJsonLastModifiedTicksSinceEpoch;
            this.FundingJson = other.FundingJson;
            this.FundingJsonLastModifiedTicksSinceEpoch = other.FundingJsonLastModifiedTicksSinceEpoch;
            this.ProductJson = other.ProductJson;
            this.ProductJsonLastModifiedTicksSinceEpoch = other.ProductJsonLastModifiedTicksSinceEpoch;
            this.Files = other.Files.Select(f => new Asset(f)).ToList();
            this.Assets = other.Assets.Select(f => new Asset(f)).ToList();
            this.FlexCelWorkbook = other.FlexCelWorkbook;
            this.SpreadsheetLastModifiedTicksSinceEpoch = other.SpreadsheetLastModifiedTicksSinceEpoch;
            this.AppType = other.AppType;
            this.LastSynchronisedTicksSinceEpoch = other.LastSynchronisedTicksSinceEpoch;
        }

        // Parameterless constructor for EF.
        protected ReleaseDetails()
            : base(default(Guid), default(Instant))
        {
            this.Files = new List<Asset>();
            this.Assets = new List<Asset>();
        }

        /// <summary>
        /// Gets or sets the JSON for the form configuration.
        /// </summary>
        public string? ConfigurationJson { get; set; }

        [NotMapped]
        public Instant? FormConfigurationJsonLastModifiedTimestamp
        {
            get => this.FormConfigurationJsonLastModifiedTicksSinceEpoch.HasValue
                ? Instant.FromUnixTimeTicks(this.FormConfigurationJsonLastModifiedTicksSinceEpoch.Value)
                : (Instant?)null;
            set => this.FormConfigurationJsonLastModifiedTicksSinceEpoch = value?.ToUnixTimeTicks();
        }

        public long? FormConfigurationJsonLastModifiedTicksSinceEpoch { get; private set; }

        /// <summary>
        /// Gets or sets the JSON which defines the workflow steps and navigation.
        /// </summary>
        public string? WorkflowJson { get; set; }

        [NotMapped]
        public Instant? WorkflowJsonLastModifiedTimestamp
        {
            get => this.WorkflowJsonLastModifiedTicksSinceEpoch.HasValue
                ? Instant.FromUnixTimeTicks(this.WorkflowJsonLastModifiedTicksSinceEpoch.Value)
                : (Instant?)null;
            set => this.WorkflowJsonLastModifiedTicksSinceEpoch = value?.ToUnixTimeTicks();
        }

        public long? WorkflowJsonLastModifiedTicksSinceEpoch { get; private set; }

        /// <summary>
        /// Gets or sets the JSON which defines the old "integrations" configuration.
        /// </summary>
        public string? ExportsJson { get; set; }

        [NotMapped]
        public Instant? IntegrationsJsonLastModifiedTimestamp
        {
            get => this.IntegrationsJsonLastModifiedTicksSinceEpoch.HasValue
                ? Instant.FromUnixTimeTicks(this.IntegrationsJsonLastModifiedTicksSinceEpoch.Value)
                : (Instant?)null;
            set => this.IntegrationsJsonLastModifiedTicksSinceEpoch = value?.ToUnixTimeTicks();
        }

        public long? IntegrationsJsonLastModifiedTicksSinceEpoch { get; private set; }

        /// <summary>
        /// Gets or sets the JSON which defines the automations configuration.
        /// </summary>
        public string? AutomationsJson { get; set; }

        [NotMapped]
        public Instant? AutomationsJsonLastModifiedTimestamp
        {
            get => this.AutomationsJsonLastModifiedTicksSinceEpoch.HasValue
                ? Instant.FromUnixTimeTicks(this.AutomationsJsonLastModifiedTicksSinceEpoch.Value)
                : (Instant?)null;
            set => this.AutomationsJsonLastModifiedTicksSinceEpoch = value?.ToUnixTimeTicks();
        }

        public long? AutomationsJsonLastModifiedTicksSinceEpoch { get; private set; }

        /// <summary>
        /// Gets or sets the payment configuration json.
        /// </summary>
        public string? PaymentJson { get; set; }

        [NotMapped]
        public Instant? PaymentJsonLastModifiedTimestamp
        {
            get => this.PaymentJsonLastModifiedTicksSinceEpoch.HasValue
                ? Instant.FromUnixTimeTicks(this.PaymentJsonLastModifiedTicksSinceEpoch.Value)
                : (Instant?)null;
            set => this.PaymentJsonLastModifiedTicksSinceEpoch = value?.ToUnixTimeTicks();
        }

        public long? PaymentJsonLastModifiedTicksSinceEpoch { get; private set; }

        /// <summary>
        /// Gets the payment form configuration json.
        /// </summary>
        public string PaymentFormJson { get; set; }

        [NotMapped]
        public Instant? PaymentFormJsonLastModifiedTimestamp
        {
            get => this.PaymentFormJsonLastModifiedTicksSinceEpoch.HasValue
                ? Instant.FromUnixTimeTicks(this.PaymentFormJsonLastModifiedTicksSinceEpoch.Value)
                : (Instant?)null;
            set => this.PaymentFormJsonLastModifiedTicksSinceEpoch = value?.ToUnixTimeTicks();
        }

        public long? PaymentFormJsonLastModifiedTicksSinceEpoch { get; private set; }

        /// <summary>
        /// Gets or sets the funding configuration json.
        /// </summary>
        public string? FundingJson { get; set; }

        [NotMapped]
        public Instant? FundingJsonLastModifiedTimestamp
        {
            get => this.FundingJsonLastModifiedTicksSinceEpoch.HasValue
                ? Instant.FromUnixTimeTicks(this.FundingJsonLastModifiedTicksSinceEpoch.Value)
                : (Instant?)null;
            set => this.FundingJsonLastModifiedTicksSinceEpoch = value?.ToUnixTimeTicks();
        }

        public long? FundingJsonLastModifiedTicksSinceEpoch { get; private set; }

        /// <summary>
        /// Gets or sets the product configuration json.
        /// </summary>
        public string? ProductJson { get; set; }

        [NotMapped]
        public Instant? ProductJsonLastModifiedTimestamp
        {
            get => this.ProductJsonLastModifiedTicksSinceEpoch.HasValue
                ? Instant.FromUnixTimeTicks(this.ProductJsonLastModifiedTicksSinceEpoch.Value)
                : (Instant?)null;
            set => this.ProductJsonLastModifiedTicksSinceEpoch = value?.ToUnixTimeTicks();
        }

        public long? ProductJsonLastModifiedTicksSinceEpoch { get; private set; }

        /// <summary>
        /// Gets the private files associated with the release.
        /// </summary>
        public ICollection<Asset> Files { get; private set; } = new List<Asset>();

        /// <summary>
        /// Gets the public files associated with the release.
        /// </summary>
        public ICollection<Asset> Assets { get; private set; } = new List<Asset>();

        /// <summary>
        /// Gets or sets the Excel workbook.
        /// </summary>
        public byte[]? FlexCelWorkbook { get; set; }

        [NotMapped]
        public Instant? SpreadsheetLastModifiedTimestamp
        {
            get => this.SpreadsheetLastModifiedTicksSinceEpoch.HasValue
                ? Instant.FromUnixTimeTicks(this.SpreadsheetLastModifiedTicksSinceEpoch.Value)
                : (Instant?)null;
            set => this.SpreadsheetLastModifiedTicksSinceEpoch = value?.ToUnixTimeTicks();
        }

        public long? SpreadsheetLastModifiedTicksSinceEpoch { get; private set; }

        /// <summary>
        /// Gets the type details (for quote or claims).
        /// </summary>
        public WebFormAppType AppType { get; private set; }

        [NotMapped]
        public Instant? LastSynchronisedTimestamp
        {
            get => this.LastSynchronisedTicksSinceEpoch.HasValue
                ? Instant.FromUnixTimeTicks(this.LastSynchronisedTicksSinceEpoch.Value)
                : (Instant?)null;
            set => this.LastSynchronisedTicksSinceEpoch = value?.ToUnixTimeTicks();
        }

        public long? LastSynchronisedTicksSinceEpoch { get; private set; }

        /// <summary>
        /// retrieve the release source file.
        /// </summary>
        /// <param name="filePath">the file path.</param>
        /// <param name="fileNameProvider">default release file names.</param>
        /// <returns>returns the source file.</returns>
        public IFileContentReadModel GetSourceFile(string filePath, IReleaseSourceFileNameProvider fileNameProvider)
        {
            var fileContentDictionary = this.CreateFileNameContentDictionary(fileNameProvider);

            var file = this.Files?.FirstOrDefault(x => string.Equals("file/" + x.Name, filePath, StringComparison.OrdinalIgnoreCase));
            if (file != null)
            {
                fileContentDictionary.Add("file/" + file.Name, file.FileContent.HashCode);
            }

            foreach (var keyValuePair in fileContentDictionary)
            {
                if (keyValuePair.Value != null && string.Equals(keyValuePair.Key, filePath, StringComparison.OrdinalIgnoreCase))
                {
                    return new FileContentReadModel()
                    {
                        ContentType = "application/json",
                        Name = keyValuePair.Key,
                        FileContent = Encoding.ASCII.GetBytes(keyValuePair.Value),
                    };
                }
            }

            var asset = this.Files?.FirstOrDefault(x => string.Equals("file/" + x.Name, filePath, StringComparison.OrdinalIgnoreCase));
            if (asset == null)
            {
                asset = this.Assets?.FirstOrDefault(x => string.Equals("asset/" + x.Name, filePath, StringComparison.OrdinalIgnoreCase));
            }

            if (asset != null)
            {
                return new FileContentReadModel()
                {
                    ContentType = "application/json",
                    Name = asset.Name,
                    FileContent = asset.FileContent?.Content,
                };
            }

            return null;
        }

        /// <summary>
        /// retrieve a list of resource source files.
        /// </summary>
        /// <param name="releaseId">the release id.</param>
        /// <param name="fileNameProvider">default release file names.</param>
        /// <param name="baseApiUrl">the base api url.</param>
        /// <returns>list of release source files.</returns>
        public IEnumerable<ConfigurationFileDto> GetSourceFiles(Guid releaseId, IReleaseSourceFileNameProvider fileNameProvider, string baseApiUrl)
        {
            var configFiles = new List<ConfigurationFileDto>();

            var fileContentDictionary = this.CreateFileNameContentDictionary(fileNameProvider);

            foreach (var keyValuePair in fileContentDictionary)
            {
                if (keyValuePair.Value != null)
                {
                    this.AddConfigFileModel(configFiles, keyValuePair.Key);
                }
            }

            if (this.Files != null)
            {
                foreach (var file in this.Files)
                {
                    this.AddConfigFileModel(configFiles, "file/" + file.Name, file.Name);
                }
            }

            if (this.Assets != null)
            {
                foreach (var file in this.Assets)
                {
                    this.AddConfigFileModel(configFiles, "asset/" + file.Name, file.Name);
                }
            }

            configFiles.ForEach(file =>
            {
                file.ResourceUrl = baseApiUrl + $"/api/v1/production/releases/{releaseId}/files/{file.Id}";
            });

            return configFiles.AsEnumerable();
        }

        /// <summary>
        /// put file names and content in a dictionary for easy management.
        /// </summary>
        /// <param name="fileNameProvider">the file name provider.</param>
        /// <returns>return a dictionary.</returns>
        private Dictionary<string, string> CreateFileNameContentDictionary(IReleaseSourceFileNameProvider fileNameProvider)
        {
            var config = JsonConvert.DeserializeObject<dynamic>(this.ConfigurationJson);
            string formModelJson = config["formModelJson"]?.ToString();

            Dictionary<string, string> fileContentDictionary = new Dictionary<string, string>();
            fileContentDictionary.Add(fileNameProvider.WorkflowFileName, this.WorkflowJson);
            fileContentDictionary.Add(fileNameProvider.FundingFileName, this.FundingJson);
            fileContentDictionary.Add(fileNameProvider.IntegrationsFileName, this.ExportsJson);
            fileContentDictionary.Add(fileNameProvider.FormModelFileName, formModelJson);
            fileContentDictionary.Add(fileNameProvider.PaymentFileName, this.PaymentJson);
            fileContentDictionary.Add(fileNameProvider.ProductConfigFileName, this.ProductJson);

            return fileContentDictionary;
        }

        /// <summary>
        /// create a config file model.
        /// </summary>
        /// <param name="configFiles">list of config files.</param>
        /// <param name="fileId">id of the file.</param>
        /// <param name="fileName">file name.</param>
        private void AddConfigFileModel(List<ConfigurationFileDto> configFiles, string fileId, string fileName = null)
        {
            if (fileName == null)
            {
                fileName = fileId;
            }

            var workflowJson = new ConfigurationFileDto()
            {
                Id = fileId,
                Path = fileName,
            };

            configFiles.Add(workflowJson);
        }
    }
}

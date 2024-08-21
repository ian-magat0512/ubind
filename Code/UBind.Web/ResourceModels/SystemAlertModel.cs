// <copyright file="SystemAlertModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;
    using UBind.Domain;

    /// <summary>
    /// Resource model for systemAlerts.
    /// </summary>
    public class SystemAlertModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SystemAlertModel"/> class.
        /// </summary>
        /// <param name="systemAlert">The systemAlert.</param>
        public SystemAlertModel(SystemAlert systemAlert)
        {
            if (systemAlert != null)
            {
                this.Id = systemAlert.Id;
                this.Disabled = systemAlert.Disabled;
                this.SystemAlertType = new SystemAlertTypeModel(systemAlert.Type);
                this.SortOrder = 0;
                this.WarningThreshold = systemAlert.WarningThreshold;
                this.CriticalThreshold = systemAlert.CriticalThreshold;
                this.AlertMessage = this.GetAlertMessage();
                this.TenantId = systemAlert.TenantId;
                this.ProductId = systemAlert.ProductId;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemAlertModel"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for JSON deserializer.
        /// .</remarks>
        [JsonConstructor]
        private SystemAlertModel()
        {
        }

        /// <summary>
        /// Gets the systemAlert ID.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the systemAlert sort order.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        public int SortOrder { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the tenant is disabled.
        /// </summary>
        [JsonProperty]
        public bool Disabled { get; private set; }

        /// <summary>
        /// Gets the SystemAlertType.
        /// </summary>
        [JsonProperty]
        public SystemAlertTypeModel SystemAlertType { get; private set; }

        /// <summary>
        /// Gets or sets the systemAlertType Warning Threshold.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        [Range(0, 10000, ErrorMessage = "value should be ranging from 0 to 10000")]
        public int? WarningThreshold { get; set; }

        /// <summary>
        /// Gets or sets the systemAlertType CriticalThreshold.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        [Range(0, 10000, ErrorMessage = "value should be ranging from 0 to 10000")]
        public int? CriticalThreshold { get; set; }

        /// <summary>
        /// Gets the tenant id.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the Product Id.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        public Guid? ProductId { get; private set; }

        /// <summary>
        /// Gets the systemAlertType alertMessage.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        public string AlertMessage { get; private set; }

        private string GetAlertMessage()
        {
            if (this.Disabled)
            {
                return "Disabled";
            }
            else if (this.WarningThreshold.HasValue && this.CriticalThreshold.HasValue)
            {
                return $"Warning threshold: {this.WarningThreshold.Value}, Critical threshold: {this.CriticalThreshold.Value}";
            }
            else if (this.WarningThreshold > 0)
            {
                return $"Warning threshold: {this.WarningThreshold}";
            }
            else if (this.CriticalThreshold > 0)
            {
                return $"Critical threshold: {this.CriticalThreshold}";
            }
            else
            {
                return "Not entered";
            }
        }
    }
}

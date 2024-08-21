// <copyright file="TriggerType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Enums
{
    using System.ComponentModel;

    /// <summary>
    /// Defines the different types of trigger that can be part of a automation data context request.
    /// </summary>
    public enum TriggerType
    {
        /// <summary>
        /// Depicts a data with associated HTTP trigger.
        /// </summary>
        [Description("httpTrigger")]
        HttpTrigger,

        /// <summary>
        ///  Depicts context data associated with local email trigger.
        /// </summary>
        [Description("localEmailTrigger")]
        LocalEmailTrigger,

        /// <summary>
        /// Depicts data associated with a remote email trigger.
        /// </summary>
        [Description("remoteEmailTrigger")]
        RemoteEmailTrigger,

        /// <summary>
        /// Depicts data associated with an event trigger.
        /// </summary>
        [Description("eventTrigger")]
        EventTrigger,

        /// <summary>
        /// Depicts data associated with an periodic trigger.
        /// </summary>
        [Description("periodicTrigger")]
        PeriodicTrigger,

        /// <summary>
        /// Depicts data associated with a portal page trigger.
        /// </summary>
        [Description("portalPageTrigger")]
        PortalPageTrigger,

        /// <summary>
        /// Depicts data associated with an extension trigger.
        /// </summary>
        [Description("extensionPointTrigger")]
        ExtensionPointTrigger,
    }
}

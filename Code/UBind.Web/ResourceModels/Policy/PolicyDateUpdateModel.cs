// <copyright file="PolicyDateUpdateModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Policy
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Resource model for updating of a policy date.
    /// </summary>
    public class PolicyDateUpdateModel
    {
        [Required]
        public int Year { get; set; }

        [Required]
        public int Month { get; set; }

        [Required]
        public int Day { get; set; }

        public int? Hour { get; set; }

        public int? Minute { get; set; }

        public int? Second { get; set; }
    }
}

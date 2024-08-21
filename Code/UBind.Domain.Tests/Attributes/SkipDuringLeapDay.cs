// <copyright file="SkipDuringLeapDay.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Attributes
{
    using NodaTime;
    using UBind.Domain.Extensions;
    using Xunit;

    /// <summary>
    /// This custom Fact attribute is used to skip tests that are not valid during leap day.
    /// Used when generating dates on different years based on current date. And then validating against the current date.
    /// Use with caution. Make sure to create scenarios that would handle leap days.
    /// </summary>
    public class SkipDuringLeapDay : FactAttribute
    {
        public SkipDuringLeapDay()
        {
            var today = SystemClock.Instance.Now().InUtc();
            if (today.Month == 2 && today.Day == 29)
            {
                this.Skip = "Skip during leap day";
            }

            // TODO: This is used as a temporary fix. Validate if tests assets are correct.
        }
    }
}

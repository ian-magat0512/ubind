// <copyright file="TestPersonalDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Fakes
{
    using Moq;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Helpers;

    /// <summary>
    /// Defines the <see cref="TestPersonalDetails" />.
    /// </summary>
    public class TestPersonalDetails
    {
        /// <summary>
        /// Gets the Sample.
        /// </summary>
        public static IPersonalDetails Sample
        {
            get
            {
                var mock = new Mock<IPersonalDetails>();
                mock.SetupAllProperties();
                mock.SetupGet(pd => pd.FullName).Returns("Jo Smith");
                mock.SetupGet(pd => pd.Email).Returns("jsmith@example.com");
                mock.SetupGet(pd => pd.DisplayName).Returns(PersonPropertyHelper.GetDisplayName(mock.Object));
                return mock.Object;
            }
        }
    }
}

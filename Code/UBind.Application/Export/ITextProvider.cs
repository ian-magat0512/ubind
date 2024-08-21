// <copyright file="ITextProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using System.Threading.Tasks;

    /// <summary>
    /// For providing text in response to an application event.
    /// </summary>
    public interface ITextProvider
    {
        /// <summary>
        /// Gets the text in response to an application event.
        /// </summary>
        /// <param name="applicationEvent">The application event.</param>
        /// <returns>The generated text.</returns>
        Task<string> Invoke(Domain.ApplicationEvent applicationEvent);
    }
}

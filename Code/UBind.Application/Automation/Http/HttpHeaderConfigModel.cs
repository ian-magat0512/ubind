// <copyright file="HttpHeaderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Http
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Configuration model for a http header.
    /// </summary>
    public class HttpHeaderConfigModel : IBuilder<IProvider<KeyValuePair<string, IEnumerable<string>>>>
    {
        /// <summary>
        /// Gets or sets the name of the header, defined by a text provider.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> Name { get; set; }

        /// <summary>
        /// Gets or sets the values of the header, defined by a text provider.
        /// </summary>
        public IReadOnlyList<IBuilder<IProvider<Data<string>>>> Values { get; set; }

        /// <inheritdoc/>
        public IProvider<KeyValuePair<string, IEnumerable<string>>> Build(IServiceProvider dependencyProvider)
        {
            return new HttpHeaderProvider(
                this.Name.Build(dependencyProvider),
                this.Values.Select(v => v.Build(dependencyProvider)).ToList());
        }
    }
}

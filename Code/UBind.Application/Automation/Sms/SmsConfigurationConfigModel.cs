// <copyright file="SmsConfigurationConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Sms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Application.Automation.Providers;

    public class SmsConfigurationConfigModel : IBuilder<SmsConfiguration>
    {
        public IEnumerable<IBuilder<IProvider<Data<string>>>> To { get; set; } = Enumerable.Empty<IBuilder<IProvider<Data<string>>>>();

        public IBuilder<IProvider<Data<string>>> From { get; set; }

        public IBuilder<IProvider<Data<string>>> Content { get; set; }

        public SmsConfiguration Build(IServiceProvider dependencyProvider)
        {
            var to = this.To.Select(t => t.Build(dependencyProvider));
            var from = this.From?.Build(dependencyProvider);
            var message = this.Content.Build(dependencyProvider);
            return new SmsConfiguration(to, from, message);
        }
    }
}

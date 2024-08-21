// <copyright file="IpAddressInRangeConditionConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Conditions
{
    /// <summary>
    /// Model for building of an instance <see cref="IpAddressInRangeCondition"/>.
    /// </summary>
    public class IpAddressInRangeConditionConfigModel : IBuilder<IProvider<Data<bool>>>
    {
        public IBuilder<IProvider<Data<string>>> IpAddress { get; set; }

        public IBuilder<IProvider<Data<string>>> IsInRange { get; set; }

        public IProvider<Data<bool>> Build(IServiceProvider dependencyProvider)
        {
            return new IpAddressInRangeCondition(
                this.IpAddress.Build(dependencyProvider),
                this.IsInRange.Build(dependencyProvider));
        }
    }
}

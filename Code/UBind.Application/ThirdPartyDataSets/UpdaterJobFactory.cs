// <copyright file="UpdaterJobFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.ThirdPartyDataSets
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <inheritdoc/>
    public class UpdaterJobFactory : IUpdaterJobFactory
    {
        private readonly IEnumerable<IUpdaterJob> updaterJobStateMachines;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdaterJobFactory"/> class.
        /// </summary>
        /// <param name="updaterJobStateMachines">The updater job state machines in the services collection.</param>
        public UpdaterJobFactory(IEnumerable<IUpdaterJob> updaterJobStateMachines)
        {
            this.updaterJobStateMachines = updaterJobStateMachines;
        }

        /// <inheritdoc/>
        public IUpdaterJob GetUpdaterJob(Type updaterJobType)
        {
            return this.updaterJobStateMachines.First(o => o.GetType().IsAssignableFrom(updaterJobType));
        }
    }
}

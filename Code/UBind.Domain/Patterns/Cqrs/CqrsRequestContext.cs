// <copyright file="CqrsRequestContext.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Patterns.Cqrs
{
    using System;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Maintains contextual information about the executing command.
    /// </summary>
    public class CqrsRequestContext : ICqrsRequestContext
    {
        private RequestIntent? requestIntent;
        private IUBindDbContext dbContext;

        public CqrsRequestContext()
        {
        }

        /// <inheritdoc/>
        public bool RequestScopeCreated { get; set; }

        /// <inheritdoc/>
        public RequestIntent? RequestIntent
        {
            get
            {
                return this.requestIntent;
            }

            set
            {
                if (this.dbContext != null)
                {
                    throw new InvalidOperationException("An attempt was made to set the RequestIntent after "
                        + "the dbContext has been created. This is not allowed, since the RequestIntent determines "
                        + "which database instance the database connection is made to, either the read-write master, "
                        + "or a read-only replica.");
                }

                if (this.requestIntent == Cqrs.RequestIntent.ReadWrite && value == Cqrs.RequestIntent.ReadOnly)
                {
                    throw new InvalidOperationException("An attempt was made to set the RequestIntent to ReadOnly "
                        + "after it has already been set to ReadWrite. This indicates a logic error. It's not safe to "
                        + "downgrade the request intent if prior code has set it to ReadWrite, because the read "
                        + "request would be directed to the read-only replica, which would not yet have the recently "
                        + "written data (it takes a little time to synchronise). This would create unexpected results "
                        + "for users. An example could be that you create a new aggregate entity, and then "
                        + "immediately try to fetch its read model - it would not be found."
                        + "Please check the code and ensure the Request Intent is set for the entire set of commands "
                        + "and queries associated with a request, job or operation scope.");
                }

                this.requestIntent = value;
            }
        }

        /// <inheritdoc/>
        public IUBindDbContext DbContext
        {
            get
            {
                return this.dbContext;
            }

            set
            {
                if (this.dbContext != null)
                {
                    throw new InvalidOperationException("An attempt was made to set the DbContext on "
                        + "ICqrsRequestContext when it has already been set. It should only be set once.");
                }

                this.dbContext = value;
            }
        }
    }
}

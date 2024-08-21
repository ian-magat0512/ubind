// <copyright file="CreditNoteNumberRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Repository for credit note numbers.
    /// </summary>
    public class CreditNoteNumberRepository : NumberPoolRepositoryBase<CreditNoteNumber>, ICreditNoteNumberRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreditNoteNumberRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="connectionConfiguration">SQL connection configuration.</param>
        /// <param name="clock">Clock for obtaining the current time.</param>
        public CreditNoteNumberRepository(IUBindDbContext dbContext, IConnectionConfiguration connectionConfiguration, IClock clock)
            : base(dbContext, connectionConfiguration, clock)
        {
        }

        /// <inheritdoc/>
        public override string Prefix
        {
            get { return "CN-"; }
        }
    }
}

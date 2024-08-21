// <copyright file="SavedPaymentMethodRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MoreLinq;
    using UBind.Domain.Entities;
    using UBind.Domain.Repositories;

    public class SavedPaymentMethodRepository : ISavedPaymentMethodRepository
    {
        private readonly IUBindDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="SavedPaymentMethodRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The UBind database context.</param>
        public SavedPaymentMethodRepository(IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public IEnumerable<SavedPaymentMethod> GetSavedPaymentMethodsForPaymentByCustomer(Guid tenantId, Guid customerId)
        {
            return this.dbContext.SavedPaymentMethods
                .Where(spm => spm.TenantId == tenantId && spm.CustomerId == customerId)
                .OrderByDescending(spm => spm.CreatedTicksSinceEpoch);
        }

        /// <inheritdoc/>
        public SavedPaymentMethod GetByIdForPayment(Guid tenantId, Guid savedPaymentId)
        {
            return this.dbContext.SavedPaymentMethods
                .FirstOrDefault(spm => spm.TenantId == tenantId && spm.Id == savedPaymentId);
        }

        /// <inheritdoc/>
        public SavedPaymentMethod GetByIdForDisplay(Guid tenantId, Guid savedPaymentId)
        {
            var savedPaymentMethod = this.dbContext.SavedPaymentMethods
                .FirstOrDefault(spm => spm.TenantId == tenantId && spm.Id == savedPaymentId);

            savedPaymentMethod.ClearSensitiveInformation();
            return savedPaymentMethod;
        }

        /// <inheritdoc/>
        public SavedPaymentMethod Insert(SavedPaymentMethod paymentMethod)
        {
            return this.dbContext.SavedPaymentMethods.Add(paymentMethod);
        }

        /// <inheritdoc/>
        public void Delete(Guid tenantId, Guid customerId, Guid savedPaymentMethodId)
        {
            var savedPaymentMethod = this.dbContext.SavedPaymentMethods
                .FirstOrDefault(spm => spm.TenantId == tenantId && spm.Id == savedPaymentMethodId && spm.CustomerId == customerId);
            this.dbContext.SavedPaymentMethods.Remove(savedPaymentMethod);
            this.dbContext.SaveChanges();
        }

        /// <inheritdoc/>
        public void SaveChanges()
        {
            this.dbContext.SaveChanges();
        }
    }
}

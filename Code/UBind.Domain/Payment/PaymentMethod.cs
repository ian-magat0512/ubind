// <copyright file="PaymentMethod.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Payment
{
    using System;

    /// <summary>
    /// Defines the method to be used for payment transactions, i.e. zaiCardPayment.
    /// </summary>
    public class PaymentMethod
    {
        public PaymentMethod(
            Guid id,
            string name,
            string alias,
            PaymentMethodType type)
        {
            this.Id = id;
            this.Name = name;
            this.Alias = alias;
            this.Type = type;
        }

        /// <summary>
        /// Gets or sets the ID of the payment method.
        /// </summary>
        public Guid Id { get; protected set; }

        /// <summary>
        /// Gets or sets the name of the payment method.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Gets or sets the alias of the payment method.
        /// </summary>
        public string Alias { get; protected set; }

        public PaymentMethodType Type { get; protected set; }

        public static PaymentMethod GetZaiPaymentMethod()
            => new PaymentMethod(Guid.Parse("51EAFEF8-D199-4D34-aC50-2B49B59FD218"), "Zai Card Payment", "zaiCardPayment", PaymentMethodType.CardPayment);
    }
}

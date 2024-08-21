// <copyright file="Data.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation
{
    using System;

    /// <summary>
    /// Represents the values to be used by automations.
    /// </summary>
    /// <typeparam name="TData">The type of the data value.</typeparam>
    public class Data<TData> : IData, IDataWrapper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Data{TData}"/> class.
        /// </summary>
        /// <param name="valueData">The data to be wrapped.</param>
        public Data(TData valueData)
        {
            this.DataValue = valueData;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public TData DataValue { get; }

        /// <inheritdoc/>
        object IDataWrapper.Data => this.DataValue;

        /// <summary>
        /// Override conversion operator to ensure contravariance enablement.
        /// </summary>
        /// <param name="data">The data to be converted.</param>
        public static implicit operator TData(Data<TData> data)
        {
            if (data == null)
            {
                return default(TData);
            }

            return data.DataValue;
        }

        /// <summary>
        /// Override conversion operator to ensure covariance enablement.
        /// </summary>
        /// <param name="data">The data to be converted.</param>
        public static implicit operator Data<TData>(TData data) => new Data<TData>(data);

        /// <inheritdoc/>
        public dynamic GetValueFromGeneric()
        {
            return this.DataValue;
        }

        /// <inheritdoc/>
        public Type GetInnerType()
        {
            return typeof(TData);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.DataValue.ToString();
        }
    }
}

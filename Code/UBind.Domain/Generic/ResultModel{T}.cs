// <copyright file="ResultModel{T}.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Generic
{
    using System;

    /// <summary>
    /// standardized result model with custom model. use if you want to send a model and a success value.
    /// </summary>
    /// <typeparam name="T">the type.</typeparam>
    [Serializable]
    public class ResultModel<T> : ResultModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResultModel{T}"/> class.
        /// </summary>
        public ResultModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultModel{T}"/> class.
        /// </summary>
        /// <param name="model">the model.</param>
        public ResultModel(T model)
        {
            this.Model = model;
            this.Successful = true;
            this.Message = "Successful";
        }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        public T Model { get; set; }
    }
}

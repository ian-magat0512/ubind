// <copyright file="ServiceSoapClient.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Funding.EfundExpress
{
    using System;
    using System.ServiceModel;

    /// <summary>
    /// Extends the ServiceSoapClient to properly close, abort the service.
    /// </summary>
    public partial class ServiceSoapClient : IDisposable
    {
        private bool isDisposed;

        /// <summary>
        /// Finalizes an instance of the <see cref="ServiceSoapClient"/> class.
        /// dsadas
        /// </summary>
        ~ServiceSoapClient()
        {
            this.Dispose();
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources. </summary>
        public void Dispose()
        {
            ServiceClientUtilities.Dispose((ICommunicationObject)this, ref this.isDisposed);
        }
    }
}

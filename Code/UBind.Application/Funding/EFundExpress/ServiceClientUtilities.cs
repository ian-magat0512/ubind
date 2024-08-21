// <copyright file="ServiceClientUtilities.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Funding.EfundExpress
{
    using System;
    using System.ServiceModel;

    /// <summary>
    /// Provides utility methods for WCF service clients.
    /// </summary>
    public static class ServiceClientUtilities
    {
        /// <summary>Disposes the service client. </summary>
        /// <param name="service">The service client. </param>
        /// <param name="isDisposed">The reference to a value indicating whether the service is disposed. </param>
        public static void Dispose(ICommunicationObject service, ref bool isDisposed)
        {
            if (isDisposed)
            {
                return;
            }

            try
            {
                if (service.State == CommunicationState.Faulted)
                {
                    service.Abort();
                }
                else
                {
                    try
                    {
                        service.Close();
                    }
                    catch (Exception closeException)
                    {
                        try
                        {
                            service.Abort();
                        }
                        catch (Exception abortException)
                        {
                            throw new AggregateException(closeException, abortException);
                        }

                        throw;
                    }
                }
            }
            finally
            {
                isDisposed = true;
            }
        }
    }
}

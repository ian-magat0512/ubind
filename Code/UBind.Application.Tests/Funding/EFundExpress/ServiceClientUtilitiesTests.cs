// <copyright file="ServiceClientUtilitiesTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Funding.EFundExpress
{
    using System;
    using System.ServiceModel;
    using Moq;
    using UBind.Application.Funding.EfundExpress;
    using Xunit;
    using Assert = Xunit.Assert;

    public class ServiceClientUtilitiesTests
    {
        [Fact]
        public void Dispose_CallsAbort_WhenCommunicationStateIsFaulted()
        {
            // Arrange
            var mockCommObject = new Mock<ICommunicationObject>();
            mockCommObject.Setup(x => x.State).Returns(CommunicationState.Faulted);

            bool isDispose = false;

            // Act
            ServiceClientUtilities.Dispose(mockCommObject.Object, ref isDispose);

            // Assert
            mockCommObject.Verify(x => x.Abort(), Times.Once);
        }

        [Fact]
        public void Dispose_CallsAbortAndThrowsException_WhenThereIsAnExceptionRaisedWhenTryingToCloseService()
        {
            // Assert
            var mockCommObject = new Mock<ICommunicationObject>();
            mockCommObject.Setup(x => x.State).Returns(CommunicationState.Opened);
            mockCommObject.Setup(x => x.Close()).Throws(new TimeoutException("Fake Timeout Exception"));
            var mockUtility = new MockServiceClientUtilities();

            // Act
            // Assert
            Assert.Throws<TimeoutException>(() => mockUtility.Dispose(mockCommObject.Object));
            mockCommObject.Verify(x => x.Abort(), Times.Once);
        }

        [Fact]
        public void Dispose_ThrowsAggregateException_WhenThereIsAnExceptionRaisedWhenTryingToAbortService()
        {
            // Assert
            var mockCommObject = new Mock<ICommunicationObject>();
            mockCommObject.Setup(x => x.State).Returns(CommunicationState.Opened);

            mockCommObject.Setup(x => x.Close()).Throws(new TimeoutException("Fake Timeout Exception"));
            mockCommObject.Setup(x => x.Abort()).Throws(new NullReferenceException("Fake Null Ref Exception"));

            var mockUtility = new MockServiceClientUtilities();

            // Act
            // Assert
            Assert.Throws<AggregateException>(() => mockUtility.Dispose(mockCommObject.Object));
        }

        private class MockServiceClientUtilities
        {
            public void Dispose(ICommunicationObject service)
            {
                bool isDisposed = false;
                ServiceClientUtilities.Dispose(service, ref isDisposed);
            }
        }
    }
}

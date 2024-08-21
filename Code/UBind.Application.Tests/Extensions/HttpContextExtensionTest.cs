// <copyright file="HttpContextExtensionTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Aplication.Tests.Extensions
{
    using System;
    using System.Net;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using UBind.Application.ExtensionMethods;
    using UBind.Domain.Exceptions;
    using Xunit;

    /// <summary>
    /// tests http context extensions.
    /// Defines the <see cref="HttpContextExtensionTest" />.
    /// </summary>
    public class HttpContextExtensionTest
    {
        /// <summary>
        /// check GetClientIPAddress expect it to return remote ip address if withut customer header.
        /// </summary>
        [Fact]
        public void GetClientIPAddress_ReturnRemoteIpAddress_IfWithoutCustomHeader()
        {
            // Arrange
            string code = "dk20wqkwdw";
            string remoteIpAddressString = "127.0.0.1";
            HttpContext context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = IPAddress.Parse(remoteIpAddressString);

            // Act
            var ipAddress = context.GetClientIPAddress(code);

            // Assert
            Assert.Equal(ipAddress.ToString(), remoteIpAddressString);
        }

        /// <summary>
        /// check GetClientIPAddress expect it to return remote ip address if with X-Forwarded-For header.
        /// </summary>
        [Fact]
        public void GetClientIPAddress_ReturnRemoteIpAddress_IfWithXForwardedForHeader()
        {
            // Arrange
            string code = "dk20wqkwdw";
            string remoteIpAddressString = "127.0.0.1";
            HttpContext context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = IPAddress.Parse(remoteIpAddressString);
            context.Request.Headers["X-Forwarded-For"] = "200.1.42.123";

            // Act
            var ipAddress = context.GetClientIPAddress(code);

            // Assert
            Assert.Equal(ipAddress.ToString(), remoteIpAddressString);
        }

        /// <summary>
        /// check GetClientIPAddress expect it to return remote ip address if with custom header but no value.
        /// </summary>
        [Fact]
        public void GetClientIPAddress_ReturnRemoteIpAddress_IfWithCustomHeaderButNoValue()
        {
            // Arrange
            string code = "dk20wqkwdw";
            string remoteIpAddressString = "127.0.0.1";
            HttpContext context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = IPAddress.Parse(remoteIpAddressString);
            context.Request.Headers["UBind-Client-IP-" + code] = string.Empty;

            // Act
            var ipAddress = context.GetClientIPAddress(code);

            // Assert
            Assert.Equal(ipAddress.ToString(), remoteIpAddressString);
        }

        /// <summary>
        /// check GetClientIPAddress expect it to return remote ip address if with custom header and X-Forwarded-For but no value.
        /// </summary>
        [Fact]
        public void GetClientIPAddress_ReturnRemoteIpAddress_IfWithCustomHeaderAndXForwardedForButNoValue()
        {
            // Arrange
            string code = "dk20wqkwdw";
            string remoteIpAddressString = "127.0.0.1";
            HttpContext context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = IPAddress.Parse(remoteIpAddressString);
            context.Request.Headers["UBind-Client-IP-" + code] = string.Empty;
            context.Request.Headers["X-Forwarded-For"] = string.Empty;

            // Act
            var ipAddress = context.GetClientIPAddress(code);

            // Assert
            Assert.Equal(ipAddress.ToString(), remoteIpAddressString);
        }

        /// <summary>
        /// check GetClientIPAddress expect it to return InvalidOperationException if code is not valid.
        /// </summary>
        /// <param name="code">the secret code.</param>
        [Theory]
        [InlineData("")]
        [InlineData("e2e1e1")]
        [InlineData("e21eqwe1f 1r 111")]
        [InlineData("OW01IENQUW9")]
        public void GetClientIPAddress_ReturnInvalidOperationException_IfCodeIsNotValid(string code = null)
        {
            // Arrange
            string remoteIpAddressString = "127.0.0.1";
            HttpContext context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = IPAddress.Parse(remoteIpAddressString);
            context.Request.Headers["UBind-Client-IP-" + code] = string.Empty;
            context.Request.Headers["X-Forwarded-For"] = string.Empty;

            // Act
            Action act = () => context.GetClientIPAddress(code);

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        /// <summary>
        /// check GetClientIPAddress expect it to return BadRequestException if ip address is not valid.
        /// </summary>
        /// <param name="ipAddress">the ip address.</param>
        [Theory]
        [InlineData("e2e1e1")]
        [InlineData("e21eqwe1f 1r 111")]
        [InlineData("OW01IENQUW9")]
        [InlineData("192.1.1.257")]
        public void GetClientIPAddress_ReturnBadRequestException_IfIPAddressIsNotValid(string ipAddress = null)
        {
            // Arrange
            string code = "dk20wqkwdw";
            string remoteIpAddressString = "127.0.0.1";
            HttpContext context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = IPAddress.Parse(remoteIpAddressString);
            context.Request.Headers["UBind-Client-IP-" + code] = ipAddress;
            context.Request.Headers["X-Forwarded-For"] = string.Empty;

            // Act
            Action act = () => context.GetClientIPAddress(code);

            // Assert
            act.Should().Throw<ErrorException>().And.Error.Code.Should().Be("bad.request");
        }

        /// <summary>
        /// check GetClientIPAddress expect it to return the client ip address from customer header if with custom header has value and has 10digit alpha code.
        /// </summary>
        /// <param name="code">the secret code.</param>
        [Theory]
        [InlineData("1234w67890")]
        [InlineData("dk20wqkwdw")]
        [InlineData("w91kf7asdw")]
        [InlineData("zxcklalskd")]
        public void GetClientIPAddress_ReturnClientIPFromCustomHeader_IfWithCustomHeaderHasValueAndHAs10DigitAlphanumericCode(string code = null)
        {
            // Arrange
            string remoteIpAddressString = "127.0.0.1";
            string headerIP = "123.123.123.123";
            HttpContext context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = IPAddress.Parse(remoteIpAddressString);
            context.Request.Headers["UBind-Client-IP-" + code] = headerIP;
            context.Request.Headers["X-Forwarded-For"] = string.Empty;

            // Act
            var ipAddress = context.GetClientIPAddress(code);

            // Assert
            Assert.Equal(ipAddress.ToString(), headerIP);
        }

        /// <summary>
        /// check GetClientIPAddress expect it to return the client ip address from customer header if with custom header has value.
        /// </summary>
        [Fact]
        public void GetClientIPAddress_ReturnClientIPFromCustomHeader_IfWithCustomHeaderHasValue()
        {
            // Arrange
            string code = "dk20wqkwdw";
            string remoteIpAddressString = "127.0.0.1";
            string headerIP = "123.123.123.123";
            HttpContext context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = IPAddress.Parse(remoteIpAddressString);
            context.Request.Headers["UBind-Client-IP-" + code] = headerIP;
            context.Request.Headers["X-Forwarded-For"] = string.Empty;

            // Act
            var ipAddress = context.GetClientIPAddress(code);

            // Assert
            Assert.Equal(ipAddress.ToString(), headerIP);
        }

        /// <summary>
        /// check GetClientIPAddress expect it to return the client ip address from customer header if with custom header has value and X-Forwarded-For has value.
        /// </summary>
        [Fact]
        public void GetClientIPAddress_ReturnClientIPFromCustomHeader_IfWithCustomHeaderHasValueAndXForwardedForHasValue()
        {
            // Arrange
            string code = "dk20wqkwdw";
            string remoteIpAddressString = "127.0.0.1";
            string headerIP = "123.123.123.123";
            string otherIP = "0.0.0.0";
            HttpContext context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = IPAddress.Parse(remoteIpAddressString);
            context.Request.Headers["UBind-Client-IP-" + code] = headerIP;
            context.Request.Headers["X-Forwarded-For"] = otherIP;

            // Act
            var ipAddress = context.GetClientIPAddress(code);

            // Assert
            Assert.Equal(ipAddress.ToString(), headerIP);
        }

        /// <summary>
        /// check GetClientIPAddress expect it to return the client ip address from customer header if with custom header has same value as remote IP.
        /// </summary>
        [Fact]
        public void GetClientIPAddress_ReturnClientIPFromCustomHeader_IfCustomHeaderHasSameValueAsRemoteIP()
        {
            // Arrange
            string code = "dk20wqkwdw";
            string remoteIpAddressString = "127.0.0.1";
            string otherIP = "0.0.0.0";
            HttpContext context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = IPAddress.Parse(remoteIpAddressString);
            context.Request.Headers["UBind-Client-IP-" + code] = remoteIpAddressString;
            context.Request.Headers["X-Forwarded-For"] = otherIP;

            // Act
            var ipAddress = context.GetClientIPAddress(code);

            // Assert
            Assert.Equal(ipAddress.ToString(), remoteIpAddressString);
        }
    }
}
